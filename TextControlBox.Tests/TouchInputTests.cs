#pragma warning disable MSTEST0037
using Microsoft.Graphics.Canvas;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using TextControlBoxNS.Core;
using TextControlBoxNS.Helper;
using Windows.Foundation;

namespace TextControlBox.Tests;

[TestClass]
public class TouchInputTests
{
    [UITestMethod]
    public void TouchTap_PlacesCursorAndClearsSelection()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("hello world test");
        core.textRenderer.EnsureTextFormat();
        core.textRenderer.CalculateLinesToRender();
        core.textRenderer.UpdateCurrentLineTextLayout(core.canvasText);

        var layout = core.textRenderer.CurrentLineTextLayout;
        Assert.IsNotNull(layout);

        // Character 6 ('w' of "world")
        var caret = layout.GetCaretPosition(6, false);
        float hitY = core.textRenderer.TopInset + core.textRenderer.SingleLineHeight * 0.5f;
        Point tapPoint = new Point(caret.X + 2, hitY);

        // Put an initial selection
        core.selectionManager.SetSelection(0, 0, 0, 5);
        Assert.IsTrue(core.selectionManager.HasSelection);

        // Single tap at character 6
        core.pointerActionsManager.HandleTouchPressed(core, pointerId: 1, tapPoint);
        core.pointerActionsManager.HandleTouchReleased(pointerId: 1, tapPoint);

        // Cursor should now be placed at 'w' (char 6) and selection cleared
        Assert.AreEqual(0, core.cursorManager.LineNumber);
        Assert.AreEqual(6, core.cursorManager.CharacterPosition);
        Assert.IsFalse(core.selectionManager.HasSelection);
    }

    [UITestMethod]
    public void TouchDoubleTap_SelectsWord()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("hello world test");
        core.textRenderer.EnsureTextFormat();
        core.textRenderer.CalculateLinesToRender();
        core.textRenderer.UpdateCurrentLineTextLayout(core.canvasText);

        var layout = core.textRenderer.CurrentLineTextLayout;
        Assert.IsNotNull(layout);

        // Inside "world" (char 8)
        var caret = layout.GetCaretPosition(8, false);
        float hitY = core.textRenderer.TopInset + core.textRenderer.SingleLineHeight * 0.5f;
        Point tapPoint = new Point(caret.X + 2, hitY);

        // Tap 1
        core.pointerActionsManager.HandleTouchPressed(core, pointerId: 1, tapPoint);
        core.pointerActionsManager.HandleTouchReleased(pointerId: 1, tapPoint);

        // Tap 2 (within multi-tap window)
        core.pointerActionsManager.HandleTouchPressed(core, pointerId: 1, tapPoint);
        core.pointerActionsManager.HandleTouchReleased(pointerId: 1, tapPoint);

        // "world" is [6, 11)
        Assert.IsTrue(core.selectionManager.HasSelection);
        var ordered = core.selectionManager.OrderTextSelectionSeparated();
        Assert.AreEqual(0, ordered.startLine);
        Assert.AreEqual(6, ordered.startChar);
        Assert.AreEqual(0, ordered.endLine);
        Assert.AreEqual(11, ordered.endChar);
    }

    [UITestMethod]
    public void TouchDrag_ScrollsDocumentWithoutMovingCursor()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 200);
        core.textRenderer.EnsureTextFormat();
        core.textRenderer.CalculateLinesToRender();

        // Cursor is initially at 0, 0
        core.cursorManager.SetCursorPosition(0, 0);
        double initialScroll = core.scrollManager.OffsetSource.VerticalOffset;
        Assert.AreEqual(0, initialScroll);

        Point startPoint = new Point(100, 200);
        Point movedPoint = new Point(100, 120); // Finger moved UP by 80px -> scroll down

        core.pointerActionsManager.HandleTouchPressed(core, pointerId: 1, startPoint);
        core.pointerActionsManager.HandleTouchMoved(pointerId: 1, movedPoint);

        // Document should have scrolled down (VerticalOffset increased by 80px)
        double scrolledOffset = core.scrollManager.OffsetSource.VerticalOffset;
        Assert.IsTrue(scrolledOffset > 0, "VerticalOffset should have increased after dragging up");

        core.pointerActionsManager.HandleTouchReleased(pointerId: 1, movedPoint);

        // Cursor must NOT have moved during scrolling
        Assert.AreEqual(0, core.cursorManager.LineNumber);
        Assert.AreEqual(0, core.cursorManager.CharacterPosition);
    }

    [UITestMethod]
    public void TouchLongPress_SelectsWord()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("hello world test");
        core.textRenderer.EnsureTextFormat();
        core.textRenderer.CalculateLinesToRender();
        core.textRenderer.UpdateCurrentLineTextLayout(core.canvasText);

        var layout = core.textRenderer.CurrentLineTextLayout;
        Assert.IsNotNull(layout);

        var caret = layout.GetCaretPosition(8, false);
        float hitY = core.textRenderer.TopInset + core.textRenderer.SingleLineHeight * 0.5f;
        Point touchPoint = new Point(caret.X + 2, hitY);

        core.pointerActionsManager.HandleTouchPressed(core, pointerId: 1, touchPoint);
        // Trigger long press
        core.pointerActionsManager.TriggerTouchLongPress();

        Assert.IsTrue(core.selectionManager.HasSelection);
        var ordered = core.selectionManager.OrderTextSelectionSeparated();
        Assert.AreEqual(6, ordered.startChar);
        Assert.AreEqual(11, ordered.endChar);

        core.pointerActionsManager.HandleTouchReleased(pointerId: 1, touchPoint);
        Assert.IsTrue(core.selectionManager.HasSelection);
    }

    [UITestMethod]
    public void TouchPinch_ZoomsDocument()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 10);
        int initialZoom = core.zoomManager._ZoomFactor;

        Point finger1 = new Point(100, 100);
        Point finger2 = new Point(100, 200); // 100px apart

        core.pointerActionsManager.HandleTouchPressed(core, pointerId: 1, finger1);
        core.pointerActionsManager.HandleTouchPressed(core, pointerId: 2, finger2);

        Assert.AreEqual(TouchInteractionState.Pinching, core.pointerActionsManager.TouchState);

        // Spread fingers further apart: finger 2 moves to (100, 300) -> 200px apart (2x zoom)
        Point finger2Spread = new Point(100, 300);
        core.pointerActionsManager.HandleTouchMoved(pointerId: 2, finger2Spread);

        Assert.IsTrue(core.zoomManager._ZoomFactor > initialZoom, "Zoom factor should have increased after pinch spread");

        core.pointerActionsManager.HandleTouchReleased(pointerId: 2, finger2Spread);
        core.pointerActionsManager.HandleTouchReleased(pointerId: 1, finger1);

        Assert.AreEqual(TouchInteractionState.None, core.pointerActionsManager.TouchState);
    }

    [UITestMethod]
    public void TouchCancel_ResetsStateSafely()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 10);
        Point point = new Point(100, 100);

        core.pointerActionsManager.HandleTouchPressed(core, pointerId: 1, point);
        core.pointerActionsManager.HandleTouchMoved(pointerId: 1, new Point(100, 150));

        Assert.AreEqual(TouchInteractionState.Scrolling, core.pointerActionsManager.TouchState);

        core.pointerActionsManager.HandleTouchCanceled();

        Assert.AreEqual(TouchInteractionState.None, core.pointerActionsManager.TouchState);
    }

    [UITestMethod]
    public void TouchpadPinchZoom_PrecisionDeltas_AccumulateSmoothlyAndZoom()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 10);
        int initialZoom = core.zoomManager._ZoomFactor;
        Assert.AreEqual(100, initialZoom);

        // Precision touchpad sends small deltas (e.g. 10 units each)
        // First small delta (10) triggers a 1% step or accumulates
        core.pointerActionsManager.ApplyZoomDelta(core.zoomManager, 10);
        Assert.IsTrue(core.zoomManager._ZoomFactor >= 100);

        // Second small delta (+15) ensures accumulated delta (>= 20) yields zoom increase
        core.pointerActionsManager.ApplyZoomDelta(core.zoomManager, 15);
        Assert.IsTrue(core.zoomManager._ZoomFactor > 100, "Zoom factor should have increased with accumulated pinch delta");

        int zoomedIn = core.zoomManager._ZoomFactor;

        // Negative deltas (pinch out/in reverse) zoom back out
        core.pointerActionsManager.ApplyZoomDelta(core.zoomManager, -25);
        Assert.IsTrue(core.zoomManager._ZoomFactor < zoomedIn, "Zoom factor should have decreased with negative pinch delta");
    }

    [UITestMethod]
    public void TouchpadPinchZoom_MouseWheelStep_MatchesExpectedRatio()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 10);
        int initialZoom = core.zoomManager._ZoomFactor;

        // Standard mouse wheel notch is 120 (120 / 20 = 6% zoom)
        core.pointerActionsManager.ApplyZoomDelta(core.zoomManager, 120);
        Assert.AreEqual(initialZoom + 6, core.zoomManager._ZoomFactor);

        core.pointerActionsManager.ApplyZoomDelta(core.zoomManager, -120);
        Assert.AreEqual(initialZoom, core.zoomManager._ZoomFactor);
    }

    [UITestMethod]
    public void HorizontalScroll_AfterZoom_IsNotBlocked()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 10);
        core.SetText("A very very very very long text line that extends far beyond the default window viewport width to allow horizontal scrolling");
        core.textRenderer.EnsureTextFormat();
        core.textRenderer.CalculateLinesToRender();

        // 1. Initial horizontal scroll
        core.scrollManager.HorizontalScroll = 50;
        Assert.AreEqual(50, core.scrollManager.HorizontalScroll);

        // 2. Perform a zoom gesture
        core.pointerActionsManager.ApplyZoomDelta(core.zoomManager, 120);
        Assert.IsTrue(core.zoomManager._ZoomFactor > 100);

        // 3. User finishes zooming and scrolls horizontally
        core.zoomManager.ResetZoomAnchors();
        core.scrollManager.HorizontalScroll = 120;
        core.textRenderer.CalculateLinesToRender();

        // Must NOT snap back to 0 or zoom anchor
        Assert.AreEqual(120, core.scrollManager.HorizontalScroll);
        Assert.AreEqual(120, core.scrollManager.horizontalScrollBar.Value);
    }

    [UITestMethod]
    public void LineNumberFormat_ScalesWithZoom()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 10);
        core.ShowLineNumbers = true;
        core.textRenderer.EnsureTextFormat();
        core.lineNumberRenderer.CreateLineNumberTextFormat();

        float initialFontSize = core.lineNumberRenderer.LineNumberTextFormat.FontSize;
        Assert.IsTrue(initialFontSize > 0);

        // Zoom in to 200%
        core.ZoomFactor = 200;
        core.textRenderer.EnsureTextFormat();

        Assert.IsNotNull(core.lineNumberRenderer.LineNumberTextFormat);
        float zoomedFontSize = core.lineNumberRenderer.LineNumberTextFormat.FontSize;
        Assert.AreEqual(core.zoomManager.ZoomedFontSize, zoomedFontSize);
        Assert.AreEqual(initialFontSize * 2f, zoomedFontSize, 0.01f);
        Assert.AreEqual(core.textRenderer.SingleLineHeight, core.lineNumberRenderer.LineNumberTextFormat.LineSpacing, 0.01f);

        // Zoom out to 50%
        core.ZoomFactor = 50;
        core.textRenderer.EnsureTextFormat();

        float zoomedOutFontSize = core.lineNumberRenderer.LineNumberTextFormat.FontSize;
        Assert.AreEqual(core.zoomManager.ZoomedFontSize, zoomedOutFontSize);
        Assert.AreEqual(initialFontSize * 0.5f, zoomedOutFontSize, 0.01f);
        Assert.AreEqual(core.textRenderer.SingleLineHeight, core.lineNumberRenderer.LineNumberTextFormat.LineSpacing, 0.01f);
    }

    [UITestMethod]
    public void WhitespaceGlyphs_ScaleWithZoom()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 10);
        core.ShowWhitespaceCharacters = true;
        core.textRenderer.EnsureTextFormat();

        var format100 = core.textRenderer.TextFormat;
        float fontSize100 = format100.FontSize; // <-- Vor dem Re-Create zwischenspeichern

        var (space100, tab100) = core.textLayoutManager.CreateGlyphs(CanvasDevice.GetSharedDevice(), format100);
        space100.Dispose();
        tab100.Dispose();

        // Zoom to 200%
        core.ZoomFactor = 200;
        core.textRenderer.EnsureTextFormat(); // Hier wird die Instanz hinter format100 disposed

        var format200 = core.textRenderer.TextFormat;
        Assert.AreEqual(core.zoomManager.ZoomedFontSize, format200.FontSize);
        var (space200, tab200) = core.textLayoutManager.CreateGlyphs(CanvasDevice.GetSharedDevice(), format200);

        Assert.IsTrue(format200.FontSize > fontSize100); // <-- Sicherer Vergleich

        space200.Dispose();
        tab200.Dispose();
    }
}


