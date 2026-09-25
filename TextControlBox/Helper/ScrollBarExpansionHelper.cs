#nullable enable

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Animation;
using System.Collections.Generic;
using System.Linq;

namespace TextControlBoxNS.Helper;

internal static class ScrollBarExpansionHelper
{
    private record RemovedStateInfo(VisualStateGroup Group, VisualState State);

    // Keep track of removed visual states per scrollbar so they can be restored when toggled off.
    private static readonly Dictionary<ScrollBar, List<RemovedStateInfo>> _removedStates = new();
    private static readonly Dictionary<ScrollBar, double> _scrollBarSizes = new();

    private static readonly HashSet<ScrollBar> _keepExpandedBars = new();

    public static void ShowIndicator(ScrollBar scrollBar)
    {
        if (scrollBar == null) return;
        VisualStateManager.GoToState(scrollBar, "MouseIndicator", true);
    }

    public static void HideIndicator(ScrollBar scrollBar)
    {
        if (scrollBar == null) return;
        VisualStateManager.GoToState(scrollBar, "NoIndicator", true);
    }

    public static void SetSize(ScrollBar scrollBar, double size)
    {
        if (scrollBar == null || size <= 0)
            return;

        _scrollBarSizes[scrollBar] = size;

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            ApplySize(scrollBar, size);
        }

        if (scrollBar.Orientation == Orientation.Vertical)
        {
            scrollBar.Width = size;
        }
        else
        {
            scrollBar.Height = size;
        }

        scrollBar.Resources["ScrollBarSize"] = size;

        if (!scrollBar.IsLoaded)
        {
            scrollBar.Loaded -= OnLoaded;
            scrollBar.Loaded += OnLoaded;
        }

        ApplySize(scrollBar, size);
    }

    private static void ApplySize(ScrollBar scrollBar, double size)
    {
        if (scrollBar.Orientation == Orientation.Vertical)
        {
            scrollBar.Width = size;
        }
        else
        {
            scrollBar.Height = size;
        }

        scrollBar.Resources["ScrollBarSize"] = size;

        var root = FindTemplateRoot(scrollBar);
        if (root != null)
        {
            root.Resources["ScrollBarSize"] = size;

            // Update storyboards in ConsciousStates so animations use the new size
            var groups = VisualStateManager.GetVisualStateGroups(root);
            if (groups != null)
            {
                var consciousGroup = groups.FirstOrDefault(g => g.Name == "ConsciousStates");
                if (consciousGroup != null)
                {
                    var expandedState = consciousGroup.States.FirstOrDefault(s => s.Name == "Expanded");
                    if (expandedState?.Storyboard != null)
                    {
                        foreach (var timeline in expandedState.Storyboard.Children)
                        {
                            if (timeline is DoubleAnimationUsingKeyFrames anim)
                            {
                                var target = Storyboard.GetTargetName(anim);
                                var prop = Storyboard.GetTargetProperty(anim);
                                if ((target == "VerticalThumb" && prop == "Width") ||
                                    (target == "HorizontalThumb" && prop == "Height"))
                                {
                                    foreach (var kf in anim.KeyFrames)
                                    {
                                        kf.Value = size;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            bool isKeepExpanded = _keepExpandedBars.Contains(scrollBar);
            UpdateTemplateElementsSize(root, scrollBar.Orientation, size, isKeepExpanded);
        }
    }

    private static void UpdateTemplateElementsSize(FrameworkElement root, Orientation orientation, double size, bool isKeepExpanded)
    {
        foreach (var desc in FindAllDescendants(root))
        {
            if (desc is Thumb thumb)
            {
                if (isKeepExpanded)
                {
                    if (orientation == Orientation.Vertical && thumb.Name == "VerticalThumb")
                        thumb.Width = size;
                    else if (orientation == Orientation.Horizontal && thumb.Name == "HorizontalThumb")
                        thumb.Height = size;
                }
                else
                {
                    if (orientation == Orientation.Vertical && thumb.Name == "VerticalThumb")
                        thumb.ClearValue(FrameworkElement.WidthProperty);
                    else if (orientation == Orientation.Horizontal && thumb.Name == "HorizontalThumb")
                        thumb.ClearValue(FrameworkElement.HeightProperty);
                }
            }
            else if (desc is RepeatButton btn)
            {
                if (orientation == Orientation.Vertical)
                {
                    if (btn.Name == "VerticalSmallDecrease" || btn.Name == "VerticalSmallIncrease")
                    {
                        btn.Width = size;
                        btn.Height = size;
                    }
                }
                else
                {
                    if (btn.Name == "HorizontalSmallDecrease" || btn.Name == "HorizontalSmallIncrease")
                    {
                        btn.Width = size;
                        btn.Height = size;
                    }
                }
            }
        }
    }

    private static IEnumerable<DependencyObject> FindAllDescendants(DependencyObject element)
    {
        int count = VisualTreeHelper.GetChildrenCount(element);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(element, i);
            yield return child;
            foreach (var grandChild in FindAllDescendants(child))
            {
                yield return grandChild;
            }
        }
    }

    public static void SetKeepExpanded(ScrollBar scrollBar, bool keepExpanded)
    {
        if (scrollBar == null)
            return;

        void OnLoaded(object sender, RoutedEventArgs e)
        {
            Apply(scrollBar, keepExpanded);
        }

        void OnEffectiveViewportChanged(FrameworkElement sender, EffectiveViewportChangedEventArgs args)
        {
            Apply(scrollBar, keepExpanded);
        }

        scrollBar.Loaded -= OnLoaded;
        scrollBar.EffectiveViewportChanged -= OnEffectiveViewportChanged;

        if (keepExpanded)
        {
            scrollBar.Loaded += OnLoaded;
            scrollBar.EffectiveViewportChanged += OnEffectiveViewportChanged;
        }

        Apply(scrollBar, keepExpanded);
    }

    private static void Apply(ScrollBar scrollBar, bool keepExpanded)
    {
        if (keepExpanded)
        {
            _keepExpandedBars.Add(scrollBar);

            VisualStateManager.GoToState(scrollBar, "MouseIndicator", true);
            VisualStateManager.GoToState(scrollBar, "Expanded", true);

            var root = FindTemplateRoot(scrollBar);
            if (root != null)
            {
                var groups = VisualStateManager.GetVisualStateGroups(root);
                if (groups != null)
                {
                    if (!_removedStates.TryGetValue(scrollBar, out var list))
                    {
                        list = new List<RemovedStateInfo>();
                        _removedStates[scrollBar] = list;
                    }

                    RemoveState(groups, list, "ConsciousStates", "Collapsed");
                    RemoveState(groups, list, "ConsciousStates", "CollapsedWithoutAnimation");
                }
            }

            VisualStateManager.GoToState(scrollBar, "Expanded", true);

            if (_scrollBarSizes.TryGetValue(scrollBar, out var size))
            {
                ApplySize(scrollBar, size);
            }
        }
        else
        {
            _keepExpandedBars.Remove(scrollBar);

            if (_removedStates.TryGetValue(scrollBar, out var list))
            {
                foreach (var item in list)
                {
                    if (!item.Group.States.Contains(item.State))
                    {
                        item.Group.States.Add(item.State);
                    }
                }
                _removedStates.Remove(scrollBar);
            }

            VisualStateManager.GoToState(scrollBar, "Collapsed", true);

            if (_scrollBarSizes.TryGetValue(scrollBar, out var size))
            {
                ApplySize(scrollBar, size);
            }
        }
    }

    private static void RemoveState(IList<VisualStateGroup> groups, List<RemovedStateInfo> list, string groupName, string stateName)
    {
        var group = groups.FirstOrDefault(g => g.Name == groupName);
        if (group != null)
        {
            var state = group.States.FirstOrDefault(s => s.Name == stateName);
            if (state != null)
            {
                list.Add(new RemovedStateInfo(group, state));
                group.States.Remove(state);
            }
        }
    }

    private static FrameworkElement? FindTemplateRoot(DependencyObject element)
    {
        int childCount = VisualTreeHelper.GetChildrenCount(element);
        for (int i = 0; i < childCount; i++)
        {
            var child = VisualTreeHelper.GetChild(element, i);
            if (child is FrameworkElement fe)
            {
                if (fe.Name == "Root" || VisualStateManager.GetVisualStateGroups(fe)?.Count > 0)
                    return fe;

                var descendant = FindTemplateRoot(child);
                if (descendant != null)
                    return descendant;
            }
        }
        return null;
    }
}
