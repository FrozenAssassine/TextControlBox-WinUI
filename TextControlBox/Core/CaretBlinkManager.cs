using Microsoft.UI.Xaml;
using System;

namespace TextControlBoxNS.Core;

/// <summary>
/// Drives the always-on blinking of the text caret. While the editor is focused the
/// caret toggles between visible and hidden on a fixed interval; cursor activity
/// (typing, moving the caret) resets the phase so the caret stays solid for a full
/// interval, matching standard text-editor behavior.
/// </summary>
internal class CaretBlinkManager
{
    // Windows default caret blink interval (the GetCaretBlinkTime default is 530ms).
    private const int BlinkIntervalMs = 530;

    private CanvasUpdateManager canvasUpdateManager;
    // A XAML DispatcherTimer on purpose: a DispatcherQueueTimer keeps the target of its Tick handler (and
    // with it the whole textbox) alive forever, because the GC can not see that reference cycle.
    private DispatcherTimer timer;

    /// <summary>True while the caret should be painted for the current blink phase.</summary>
    public bool IsCaretVisible { get; private set; } = true;

    public void Init(CanvasUpdateManager canvasUpdateManager)
    {
        this.canvasUpdateManager = canvasUpdateManager;

        timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(BlinkIntervalMs) };
        timer.Tick += (s, e) =>
        {
            IsCaretVisible = !IsCaretVisible;
            canvasUpdateManager.RedrawCursorForBlink();
        };
    }

    /// <summary>Begin (or restart) blinking with the caret shown for a full interval.</summary>
    public void Start()
    {
        IsCaretVisible = true;
        if (timer != null && !timer.IsEnabled)
            timer.Start();
    }

    /// <summary>Stop blinking and leave the caret hidden (it only paints while focused).</summary>
    public void Stop()
    {
        IsCaretVisible = false;
        timer?.Stop();
    }

    /// <summary>
    /// Keep the caret solid for a full interval after cursor activity (typing, moving),
    /// so it doesn't blink mid-keystroke. No-op until the blink timer is running.
    /// </summary>
    public void NotifyCaretActivity()
    {
        IsCaretVisible = true;
        if (timer != null && timer.IsEnabled)
        {
            timer.Stop();
            timer.Start();
        }
    }
}
