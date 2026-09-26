using System;
using System.Diagnostics;
using System.Numerics;
using Microsoft.UI.Composition;
using Microsoft.UI.Composition.Interactions;
using Microsoft.UI.Xaml.Hosting;

namespace TextControlBoxNS.Core;

/// <summary>
/// Diagonal (two-axis) precision-touchpad panning.
///
/// <para>A composition <see cref="InteractionTracker"/> + <see cref="VisualInteractionSource"/> on the
/// selection canvas reads the raw 2-D precision-touchpad manipulation delta — the thing the single-axis
/// wheel stream physically cannot express — and drives the pixel <see cref="IScrollOffsetSource"/>, after
/// which the existing redraw repaints. The source is configured
/// <see cref="VisualInteractionSourceRedirectionMode.CapableTouchpadOnly"/> so it captures ONLY the
/// precision-touchpad pan; the mouse wheel, Shift+wheel, and Ctrl+wheel zoom keep flowing through the
/// existing <c>PointerActionsManager.PointerWheelAction</c> untouched, and mouse click/drag selection is
/// unaffected. The two <see cref="Microsoft.UI.Xaml.Controls.Primitives.ScrollBar"/> primitives stay in sync
/// because the tracker writes through the same offset source they back.</para>
/// </summary>
internal sealed partial class CoreTextControlBox : IInteractionTrackerOwner
{
    private InteractionTracker _scrollTracker;
    private VisualInteractionSource _scrollInteractionSource;
    private Microsoft.UI.Dispatching.DispatcherQueueTimer _scrollTrackerTimer;
    private bool _scrollTrackerReady;
    private bool _scrollTrackerInteracting;
    private bool _applyingTrackerScroll;
    private float _lastTrackerX;
    private float _lastTrackerY;
    private float _lastTrackerScale = 1.0f;

    /// <summary>Wires the tracker to the selection canvas' composition visual. Called from the control's
    /// <c>Loaded</c> event (the visual + size are available by then). Idempotent and best-effort — a
    /// composition failure just leaves the editor on wheel-only scrolling.</summary>
    private void SetupDiagonalScroll()
    {
        if (_scrollTrackerReady)
            return;

        try
        {
            Visual visual = ElementCompositionPreview.GetElementVisual(canvasSelection);
            Compositor compositor = visual.Compositor;

            _scrollTracker = InteractionTracker.CreateWithOwner(compositor, this);
            _scrollTracker.MinPosition = Vector3.Zero;
            _scrollTracker.MaxPosition = Vector3.Zero; // updated from content extent by the sync timer
            _scrollTracker.MinScale = 0.04f;
            _scrollTracker.MaxScale = 4.0f;
            _lastTrackerScale = (float)(zoomManager?._ZoomFactor ?? 100) / 100f;
            if (Math.Abs(_lastTrackerScale - 1.0f) > 0.005f)
            {
                _scrollTracker.TryUpdateScale(_lastTrackerScale, Vector3.Zero);
            }

            _scrollInteractionSource = VisualInteractionSource.Create(visual);
            // Capture precision-touchpad manipulation (diagonal pan and pinch-to-zoom).
            _scrollInteractionSource.ManipulationRedirectionMode = VisualInteractionSourceRedirectionMode.CapableTouchpadOnly;
            _scrollInteractionSource.PositionXSourceMode = InteractionSourceMode.EnabledWithoutInertia;
            _scrollInteractionSource.PositionYSourceMode = InteractionSourceMode.EnabledWithoutInertia;
            _scrollInteractionSource.ScaleSourceMode = InteractionSourceMode.EnabledWithoutInertia;
            // Don't chain past the editor to an ancestor scroller.
            _scrollInteractionSource.PositionXChainingMode = InteractionChainingMode.Never;
            _scrollInteractionSource.PositionYChainingMode = InteractionChainingMode.Never;
            _scrollInteractionSource.ScaleChainingMode = InteractionChainingMode.Never;
            _scrollTracker.InteractionSources.Add(_scrollInteractionSource);

            _scrollTrackerTimer = DispatcherQueue.CreateTimer();
            _scrollTrackerTimer.Interval = TimeSpan.FromMilliseconds(50);
            _scrollTrackerTimer.Tick += (_, _) => SyncScrollTracker();
            _scrollTrackerTimer.Start();

            // Immediately follow PROGRAMMATIC scrolls (caret-follow, page keys, go-to-line, find reveal,
            // match hand-off, wheel) instead of waiting up to one 50 ms timer tick — otherwise a touchpad
            // pan started right after one of those snapped back to the stale tracker position.
            if (scrollManager?.OffsetSource is { } offsetSource)
                offsetSource.ViewChanged += OnOffsetSourceViewChanged;

            _scrollTrackerReady = true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"TextControlBox diagonal scroll: SetupDiagonalScroll failed: {ex}");
        }
    }

    /// <summary>Keeps the tracker's scrollable range sized to the content extent, and (when the user isn't
    /// actively panning) re-syncs the tracker position to programmatic scrolls (caret-follow, go-to-line,
    /// page keys, wheel) so the next touchpad pan starts from the right place.</summary>
    private void SyncScrollTracker()
    {
        if (!_scrollTrackerReady || scrollManager?.OffsetSource is not { } src || _isTouchScrolling)
            return;

        try
        {
            // If the control was unloaded or the window/visual is closing, tear down immediately.
            if (!base.IsLoaded || XamlRoot == null)
            {
                TeardownDiagonalScroll();
                return;
            }

            var max = new Vector3(
                (float)Math.Max(0, src.HorizontalExtent - src.ViewportWidth),
                (float)Math.Max(0, src.VerticalExtent - src.ViewportHeight),
                0);
            if (_scrollTracker.MaxPosition != max)
                _scrollTracker.MaxPosition = max;

            if (!_scrollTrackerInteracting)
            {
                float srcX = (float)src.HorizontalOffset;
                float srcY = (float)src.VerticalOffset;
                if (Math.Abs(srcX - _lastTrackerX) > 0.5f || Math.Abs(srcY - _lastTrackerY) > 0.5f)
                {
                    _lastTrackerX = srcX;
                    _lastTrackerY = srcY;
                    _scrollTracker.TryUpdatePosition(new Vector3(srcX, srcY, 0));
                }

                float currentZoomScale = (float)(zoomManager?._ZoomFactor ?? 100) / 100f;
                if (Math.Abs(currentZoomScale - _lastTrackerScale) > 0.005f)
                {
                    _lastTrackerScale = currentZoomScale;
                    _scrollTracker.TryUpdateScale(currentZoomScale, Vector3.Zero);
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"TextControlBox diagonal scroll: SyncScrollTracker failed: {ex}");
            TeardownDiagonalScroll();
        }
    }

    /// <summary>Fires when the offset source's position changes. A PROGRAMMATIC scroll (any consumer that
    /// writes <see cref="IScrollOffsetSource"/>: caret-follow, page keys, go-to-line, find reveal, match
    /// hand-off, wheel, scrollbar drag) immediately moves the tracker to the new position so a touchpad pan
    /// started right after begins there instead of snapping back. Our OWN touchpad write (flagged in
    /// <see cref="ValuesChanged"/>) is ignored so it does not feed back into a redundant reposition.</summary>
    private void OnOffsetSourceViewChanged(object sender, EventArgs e)
    {
        if (_applyingTrackerScroll || _isTouchScrolling)
            return;
        SyncScrollTrackerToOffsetNow();
    }

    /// <summary>Immediately re-syncs the tracker to the current offset source. Safe to call from any
    /// programmatic scroll path; a no-op during an active pan/inertia and before the tracker is ready.</summary>
    internal void SyncScrollTrackerToOffsetNow()
    {
        if (_scrollTrackerReady)
            SyncScrollTracker();
    }

    /// <summary>Stops the sync timer and disposes the composition tracker + interaction source. Called from
    /// <c>Unload()</c> so an editor instance never leaks a forever-running 50 ms <c>DispatcherQueueTimer</c>
    /// or its composition objects. Idempotent and best-effort.</summary>
    private void TeardownDiagonalScroll()
    {
        if (!_scrollTrackerReady && _scrollTrackerTimer is null && _scrollTracker is null)
            return;

        try
        {
            if (scrollManager?.OffsetSource is { } offsetSource)
                offsetSource.ViewChanged -= OnOffsetSourceViewChanged;

            if (_scrollTrackerTimer is not null)
            {
                _scrollTrackerTimer.Stop();
                _scrollTrackerTimer = null;
            }

            if (_scrollTracker is not null && _scrollInteractionSource is not null)
            {
                try { _scrollTracker.InteractionSources.RemoveAll(); } catch { }
            }

            try { _scrollInteractionSource?.Dispose(); } catch { }
            _scrollInteractionSource = null;

            try { _scrollTracker?.Dispose(); } catch { }
            _scrollTracker = null;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"TextControlBox diagonal scroll: TeardownDiagonalScroll failed: {ex}");
        }
        finally
        {
            _scrollTrackerReady = false;
            _scrollTrackerInteracting = false;
        }
    }

    // ---- IInteractionTrackerOwner -----------------------------------------------------------------
    // ValuesChanged is raised on the UI thread (WinUI's compositor is UI-thread-affined), so it is safe
    // to touch the offset source + request a redraw directly.
    public void ValuesChanged(InteractionTracker sender, InteractionTrackerValuesChangedArgs args)
    {
        if (!_scrollTrackerReady || _isTouchScrolling)
            return;

        // 1. Precision-touchpad pinch-to-zoom:
        float currentScale = args.Scale;
        if (Math.Abs(currentScale - _lastTrackerScale) > 0.002f && zoomManager != null)
        {
            _lastTrackerScale = currentScale;
            _lastTrackerX = args.Position.X;
            _lastTrackerY = args.Position.Y;

            int newZoom = (int)Math.Clamp(Math.Round(currentScale * 100f), 4, 400);
            if (newZoom != zoomManager._ZoomFactor)
            {
                zoomManager._ZoomFactor = newZoom;
                zoomManager.UpdateZoom();
            }
            return;
        }

        // 2. Ctrl + 2-finger touchpad pan to zoom:
        if (TextControlBoxNS.Helper.Utils.IsKeyPressed(Windows.System.VirtualKey.Control) && zoomManager != null)
        {
            float deltaY = args.Position.Y - _lastTrackerY;
            _lastTrackerX = args.Position.X;
            _lastTrackerY = args.Position.Y;

            if (Math.Abs(deltaY) > 0.5f)
            {
                pointerActionsManager?.ApplyZoomDelta(zoomManager, -(int)(deltaY * 3));
            }
            return;
        }

        // 3. Normal 2-finger pan:
        _lastTrackerX = args.Position.X;
        _lastTrackerY = args.Position.Y;

        try
        {
            if (scrollManager?.OffsetSource is { } src)
            {
                // Flag our own touchpad-driven writes so the ViewChanged they raise is ignored by
                // OnOffsetSourceViewChanged (otherwise a pan would feed back into a redundant reposition).
                _applyingTrackerScroll = true;
                try
                {
                    src.HorizontalOffset = args.Position.X;
                    src.VerticalOffset = args.Position.Y;
                }
                finally
                {
                    _applyingTrackerScroll = false;
                }
                canvasUpdateManager.UpdateAll();
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"TextControlBox diagonal scroll: ValuesChanged failed: {ex}");
            TeardownDiagonalScroll();
        }
    }

    public void InteractingStateEntered(InteractionTracker sender, InteractionTrackerInteractingStateEnteredArgs args)
    {
        _scrollTrackerInteracting = true;
        if (zoomManager != null)
        {
            _lastTrackerScale = (float)zoomManager._ZoomFactor / 100f;
        }
    }

    public void IdleStateEntered(InteractionTracker sender, InteractionTrackerIdleStateEnteredArgs args)
    {
        _scrollTrackerInteracting = false;
        zoomManager?.ResetZoomAnchors();

        if (scrollManager?.OffsetSource is { } src && _scrollTracker != null)
        {
            _lastTrackerX = (float)src.HorizontalOffset;
            _lastTrackerY = (float)src.VerticalOffset;
            _scrollTracker.TryUpdatePosition(new Vector3(_lastTrackerX, _lastTrackerY, 0));
        }
        if (zoomManager != null && _scrollTracker != null)
        {
            _lastTrackerScale = (float)zoomManager._ZoomFactor / 100f;
            _scrollTracker.TryUpdateScale(_lastTrackerScale, Vector3.Zero);
        }
    }

    public void InertiaStateEntered(InteractionTracker sender, InteractionTrackerInertiaStateEnteredArgs args) { }

    public void CustomAnimationStateEntered(InteractionTracker sender, InteractionTrackerCustomAnimationStateEnteredArgs args) { }

    public void RequestIgnored(InteractionTracker sender, InteractionTrackerRequestIgnoredArgs args) { }
}
