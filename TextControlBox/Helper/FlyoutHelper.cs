using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using System;
using Windows.Foundation;
using TextControlBoxNS.Core;

namespace TextControlBoxNS.Helper
{
    internal class FlyoutHelper
    {
        public MenuFlyout menuFlyout;
        public Flyout selectionFlyout;

        private Button _cutButton;
        private Button _copyButton;
        private Button _pasteButton;
        private Button _deleteButton;
        private Button _selectAllButton;

        public void Init(CoreTextControlBox sender)
        {
            CreateFlyout(sender);
            CreateSelectionFlyout(sender);
        }

        public void CreateFlyout(CoreTextControlBox sender)
        {
            menuFlyout = new MenuFlyout();

            var cutItem = CreateItem(() => { sender.Cut(); }, "Cut", Symbol.Cut, "Ctrl + X");
            var copyItem = CreateItem(() => { sender.Copy(); }, "Copy", Symbol.Copy, "Ctrl + C");
            var pasteItem = CreateItem(() => { sender.Paste(); }, "Paste", Symbol.Paste, "Ctrl + V");
            var deleteItem = CreateItem(() => { sender.Delete(); }, "Delete", Symbol.Delete, "Del");
            var selectAllItem = CreateItem(() => { sender.SelectAll(); }, "Select All", Symbol.SelectAll, "Ctrl + A");
            var undoItem = CreateItem(() => { sender.Undo(); }, "Undo", Symbol.Undo, "Ctrl + Z");
            var redoItem = CreateItem(() => { sender.Redo(); }, "Redo", Symbol.Redo, "Ctrl + Y");

            menuFlyout.Items.Add(cutItem);
            menuFlyout.Items.Add(copyItem);
            menuFlyout.Items.Add(pasteItem);
            menuFlyout.Items.Add(deleteItem);
            menuFlyout.Items.Add(new MenuFlyoutSeparator());
            menuFlyout.Items.Add(selectAllItem);
            menuFlyout.Items.Add(new MenuFlyoutSeparator());
            menuFlyout.Items.Add(undoItem);
            menuFlyout.Items.Add(redoItem);

            menuFlyout.Opening += (_, _) =>
            {
                bool hasSelection = sender.selectionManager != null && sender.selectionManager.HasSelection;
                bool isReadOnly = sender.IsReadOnly;

                cutItem.IsEnabled = hasSelection && !isReadOnly;
                copyItem.IsEnabled = hasSelection;
                deleteItem.IsEnabled = hasSelection && !isReadOnly;
                pasteItem.IsEnabled = !isReadOnly;
                selectAllItem.IsEnabled = true;
                undoItem.IsEnabled = !isReadOnly && sender.undoRedo != null && sender.undoRedo.CanUndo;
                redoItem.IsEnabled = !isReadOnly && sender.undoRedo != null && sender.undoRedo.CanRedo;
            };

            menuFlyout.Closed += (_, _) => { sender.Focus(FocusState.Programmatic); };
        }

        public void CreateSelectionFlyout(CoreTextControlBox sender)
        {
            selectionFlyout = new Flyout
            {
                Placement = FlyoutPlacementMode.Top
            };

            var flyoutPresenterStyle = new Style(typeof(FlyoutPresenter));
            flyoutPresenterStyle.Setters.Add(new Setter(Control.PaddingProperty, new Thickness(2)));
            flyoutPresenterStyle.Setters.Add(new Setter(Control.CornerRadiusProperty, new CornerRadius(8)));
            flyoutPresenterStyle.Setters.Add(new Setter(Control.MinWidthProperty, 0.0));
            flyoutPresenterStyle.Setters.Add(new Setter(Control.MinHeightProperty, 0.0));
            selectionFlyout.FlyoutPresenterStyle = flyoutPresenterStyle;

            var panel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 2,
                Padding = new Thickness(4, 2, 4, 2)
            };

            _cutButton = CreateSelectionFlyoutButton(Symbol.Cut, "Cut", () => sender.Cut());
            _copyButton = CreateSelectionFlyoutButton(Symbol.Copy, "Copy", () => sender.Copy());
            _pasteButton = CreateSelectionFlyoutButton(Symbol.Paste, "Paste", () => sender.Paste());
            _deleteButton = CreateSelectionFlyoutButton(Symbol.Delete, "Delete", () => sender.Delete());
            _selectAllButton = CreateSelectionFlyoutButton(Symbol.SelectAll, "Select All", () => sender.SelectAll());

            panel.Children.Add(_cutButton);
            panel.Children.Add(_copyButton);
            panel.Children.Add(_pasteButton);
            panel.Children.Add(_deleteButton);

            var separator = new Border
            {
                Width = 1,
                Height = 16,
                Margin = new Thickness(3, 0, 3, 0),
                VerticalAlignment = VerticalAlignment.Center,
                Background = new SolidColorBrush(Microsoft.UI.ColorHelper.FromArgb(64, 128, 128, 128))
            };
            panel.Children.Add(separator);

            panel.Children.Add(_selectAllButton);

            selectionFlyout.Content = panel;

            selectionFlyout.Opening += (_, _) =>
            {
                UpdateSelectionFlyoutStates(sender);
            };

            selectionFlyout.Closed += (_, _) =>
            {
                sender.Focus(FocusState.Programmatic);
            };
        }

        private Button CreateSelectionFlyoutButton(Symbol symbol, string label, Action action)
        {
            var contentPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                Spacing = 5,
                VerticalAlignment = VerticalAlignment.Center
            };

            var icon = new SymbolIcon(symbol)
            {
                Width = 14,
                Height = 14,
                VerticalAlignment = VerticalAlignment.Center
            };

            var text = new TextBlock
            {
                Text = label,
                FontSize = 12,
                VerticalAlignment = VerticalAlignment.Center
            };

            contentPanel.Children.Add(icon);
            contentPanel.Children.Add(text);

            var button = new Button
            {
                Content = contentPanel,
                Padding = new Thickness(8, 5, 8, 5),
                CornerRadius = new CornerRadius(6),
                Background = new SolidColorBrush(Microsoft.UI.Colors.Transparent),
                BorderThickness = new Thickness(0),
                VerticalAlignment = VerticalAlignment.Center
            };

            ToolTipService.SetToolTip(button, label);

            button.Click += (_, _) =>
            {
                selectionFlyout.Hide();
                action();
            };

            return button;
        }

        public void UpdateSelectionFlyoutStates(CoreTextControlBox sender)
        {
            bool hasSelection = sender.selectionManager != null && sender.selectionManager.HasSelection;
            bool isReadOnly = sender.IsReadOnly;

            if (_cutButton != null) _cutButton.IsEnabled = hasSelection && !isReadOnly;
            if (_copyButton != null) _copyButton.IsEnabled = hasSelection;
            if (_deleteButton != null) _deleteButton.IsEnabled = hasSelection && !isReadOnly;
            if (_pasteButton != null) _pasteButton.IsEnabled = !isReadOnly;
            if (_selectAllButton != null) _selectAllButton.IsEnabled = true;
        }

        public void ShowSelectionFlyout(FrameworkElement target, Point pointerPosition, CoreTextControlBox sender)
        {
            if (selectionFlyout == null || sender.ContextFlyoutDisabled)
                return;

            UpdateSelectionFlyoutStates(sender);
            selectionFlyout.ShowAt(target, new FlyoutShowOptions
            {
                Position = pointerPosition,
                Placement = FlyoutPlacementMode.Top
            });
        }

        public void ShowContextFlyout(FrameworkElement target, Point pointerPosition, CoreTextControlBox sender)
        {
            if (menuFlyout == null || sender.ContextFlyoutDisabled)
                return;

            menuFlyout.ShowAt(target, new FlyoutShowOptions
            {
                Position = pointerPosition
            });
        }

        public MenuFlyoutItem CreateItem(Action action, string text, Symbol icon, string key)
        {
            var item = new MenuFlyoutItem
            {
                Text = text,
                KeyboardAcceleratorTextOverride = key,
                Icon = new SymbolIcon { Symbol = icon }
            };
            item.Click += delegate
            {
                action();
            };
            return item;
        }
    }
}
