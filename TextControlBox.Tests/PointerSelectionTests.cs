#pragma warning disable MSTEST0037
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using TextControlBoxNS.Core;
using TextControlBoxNS.Helper;
using Windows.Foundation;

namespace TextControlBox.Tests;

[TestClass]
public class PointerSelectionTests
{
    [TestMethod]
    public void GetWordBoundaries_StandardWords()
    {
        string line = "hello world_123 test";
        // "hello" is [0, 5)
        var (s0, e0) = SelectionHelper.GetWordBoundaries(line, 0);
        Assert.AreEqual(0, s0);
        Assert.AreEqual(5, e0);

        var (s2, e2) = SelectionHelper.GetWordBoundaries(line, 2);
        Assert.AreEqual(0, s2);
        Assert.AreEqual(5, e2);

        // Right at the end of "hello"
        var (s5, e5) = SelectionHelper.GetWordBoundaries(line, 5);
        Assert.AreEqual(0, s5);
        Assert.AreEqual(5, e5);

        // "world_123" is [6, 15)
        var (s6, e6) = SelectionHelper.GetWordBoundaries(line, 6);
        Assert.AreEqual(6, s6);
        Assert.AreEqual(15, e6);

        var (s10, e10) = SelectionHelper.GetWordBoundaries(line, 10);
        Assert.AreEqual(6, s10);
        Assert.AreEqual(15, e10);

        // End of line on "test"
        var (sEnd, eEnd) = SelectionHelper.GetWordBoundaries(line, line.Length);
        Assert.AreEqual(16, sEnd);
        Assert.AreEqual(20, eEnd);
    }

    [TestMethod]
    public void GetWordBoundaries_WhitespaceAndSymbols()
    {
        string line = "a ==   b";
        // '==' is [2, 4)
        var (sSym, eSym) = SelectionHelper.GetWordBoundaries(line, 2);
        Assert.AreEqual(2, sSym);
        Assert.AreEqual(4, eSym);

        var (sSym2, eSym2) = SelectionHelper.GetWordBoundaries(line, 3);
        Assert.AreEqual(2, sSym2);
        Assert.AreEqual(4, eSym2);

        // Multiple spaces [4, 7)
        var (sSpc, eSpc) = SelectionHelper.GetWordBoundaries(line, 5);
        Assert.AreEqual(4, sSpc);
        Assert.AreEqual(7, eSpc);
    }

    [TestMethod]
    public void GetWordBoundaries_EmptyAndNull()
    {
        var (sEmpty, eEmpty) = SelectionHelper.GetWordBoundaries("", 0);
        Assert.AreEqual(0, sEmpty);
        Assert.AreEqual(0, eEmpty);

        var (sNull, eNull) = SelectionHelper.GetWordBoundaries(null, 0);
        Assert.AreEqual(0, sNull);
        Assert.AreEqual(0, eNull);
    }

    [UITestMethod]
    public void DoubleClick_SelectsWord()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("hello world test");
        core.textRenderer.EnsureTextFormat();
        core.textRenderer.CalculateLinesToRender();
        core.textRenderer.UpdateCurrentLineTextLayout(core.canvasText);

        var layout = core.textRenderer.CurrentLineTextLayout;
        Assert.IsNotNull(layout);

        // Point inside "world" (char 8)
        var caret = layout.GetCaretPosition(8, false);
        float hitY = core.textRenderer.TopInset + core.textRenderer.SingleLineHeight * 0.5f;
        Point clickPoint = new Point(caret.X + 2, hitY);

        // Click 1: single click
        core.pointerActionsManager.PointerPressedAction(core, clickPoint, leftButtonPressed: true, rightButtonPressed: false);
        core.pointerActionsManager.PointerReleasedAction(clickPoint);

        // Click 2: double click
        core.pointerActionsManager.PointerPressedAction(core, clickPoint, leftButtonPressed: true, rightButtonPressed: false);

        Assert.IsTrue(core.selectionManager.HasSelection);
        var ordered = core.selectionManager.OrderTextSelectionSeparated();
        Assert.AreEqual(0, ordered.startLine);
        Assert.AreEqual(6, ordered.startChar);
        Assert.AreEqual(0, ordered.endLine);
        Assert.AreEqual(11, ordered.endChar);

        core.pointerActionsManager.PointerReleasedAction(clickPoint);
        // Word remains selected after release
        Assert.IsTrue(core.selectionManager.HasSelection);
        ordered = core.selectionManager.OrderTextSelectionSeparated();
        Assert.AreEqual(6, ordered.startChar);
        Assert.AreEqual(11, ordered.endChar);
    }

    [UITestMethod]
    public void DoubleClickAndDrag_ExpandsWordByWord()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("hello world test");
        core.textRenderer.EnsureTextFormat();
        core.textRenderer.CalculateLinesToRender();
        core.textRenderer.UpdateCurrentLineTextLayout(core.canvasText);

        var layout = core.textRenderer.CurrentLineTextLayout;
        Assert.IsNotNull(layout);

        // Read caret positions upfront before actions recreate the layout
        var caretWorld = layout.GetCaretPosition(8, false);
        var caretTest = layout.GetCaretPosition(14, false);
        var caretHello = layout.GetCaretPosition(2, false);

        float hitY = core.textRenderer.TopInset + core.textRenderer.SingleLineHeight * 0.5f;
        Point clickPointWorld = new Point(caretWorld.X + 2, hitY);

        // Click 1
        core.pointerActionsManager.PointerPressedAction(core, clickPointWorld, leftButtonPressed: true, rightButtonPressed: false);
        core.pointerActionsManager.PointerReleasedAction(clickPointWorld);

        // Click 2 (double click)
        core.pointerActionsManager.PointerPressedAction(core, clickPointWorld, leftButtonPressed: true, rightButtonPressed: false);

        // Drag forward to "test" (char 14)
        Point dragPointTest = new Point(caretTest.X + 2, hitY);
        core.pointerActionsManager.PointerMovedAction(dragPointTest);

        // Should now encompass "world test" ([6, 16))
        Assert.IsTrue(core.selectionManager.HasSelection);
        var ordered = core.selectionManager.OrderTextSelectionSeparated();
        Assert.AreEqual(0, ordered.startLine);
        Assert.AreEqual(6, ordered.startChar);
        Assert.AreEqual(0, ordered.endLine);
        Assert.AreEqual(16, ordered.endChar);

        // Drag backward to "hello" (char 2)
        Point dragPointHello = new Point(caretHello.X + 2, hitY);
        core.pointerActionsManager.PointerMovedAction(dragPointHello);

        // Should now encompass "hello world" ([0, 11))
        ordered = core.selectionManager.OrderTextSelectionSeparated();
        Assert.AreEqual(0, ordered.startLine);
        Assert.AreEqual(0, ordered.startChar);
        Assert.AreEqual(0, ordered.endLine);
        Assert.AreEqual(11, ordered.endChar);

        core.pointerActionsManager.PointerReleasedAction(dragPointHello);
        Assert.IsTrue(core.selectionManager.HasSelection);
    }

    [UITestMethod]
    public void TripleClick_SelectsLine_AndDragsLineByLine()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("Line Zero\nLine One\nLine Two\nLine Three");
        core.textRenderer.EnsureTextFormat();
        core.textRenderer.CalculateLinesToRender();

        float lineHeight = core.textRenderer.SingleLineHeight;
        float hitX = 50;

        // Line 1 Y position
        float line1Y = core.textRenderer.TopInset + 1 * lineHeight + lineHeight * 0.5f;
        Point clickPointLine1 = new Point(hitX, line1Y);

        // Click 1
        core.pointerActionsManager.PointerPressedAction(core, clickPointLine1, leftButtonPressed: true, rightButtonPressed: false);
        core.pointerActionsManager.PointerReleasedAction(clickPointLine1);

        // Click 2
        core.pointerActionsManager.PointerPressedAction(core, clickPointLine1, leftButtonPressed: true, rightButtonPressed: false);
        core.pointerActionsManager.PointerReleasedAction(clickPointLine1);

        // Click 3: triple click on Line 1
        core.pointerActionsManager.PointerPressedAction(core, clickPointLine1, leftButtonPressed: true, rightButtonPressed: false);

        Assert.IsTrue(core.selectionManager.HasSelection);
        var ordered = core.selectionManager.OrderTextSelectionSeparated();
        Assert.AreEqual(1, ordered.startLine);
        Assert.AreEqual(0, ordered.startChar);
        Assert.AreEqual(2, ordered.endLine);
        Assert.AreEqual(0, ordered.endChar);

        // Drag down to Line 2
        float line2Y = core.textRenderer.TopInset + 2 * lineHeight + lineHeight * 0.5f;
        Point dragPointLine2 = new Point(hitX, line2Y);
        core.pointerActionsManager.PointerMovedAction(dragPointLine2);

        ordered = core.selectionManager.OrderTextSelectionSeparated();
        Assert.AreEqual(1, ordered.startLine);
        Assert.AreEqual(0, ordered.startChar);
        Assert.AreEqual(3, ordered.endLine);
        Assert.AreEqual(0, ordered.endChar);

        // Drag up to Line 0
        float line0Y = core.textRenderer.TopInset + 0 * lineHeight + lineHeight * 0.5f;
        Point dragPointLine0 = new Point(hitX, line0Y);
        core.pointerActionsManager.PointerMovedAction(dragPointLine0);

        ordered = core.selectionManager.OrderTextSelectionSeparated();
        Assert.AreEqual(0, ordered.startLine);
        Assert.AreEqual(0, ordered.startChar);
        Assert.AreEqual(2, ordered.endLine);
        Assert.AreEqual(0, ordered.endChar);

        core.pointerActionsManager.PointerReleasedAction(dragPointLine0);
        Assert.IsTrue(core.selectionManager.HasSelection);
    }
}
