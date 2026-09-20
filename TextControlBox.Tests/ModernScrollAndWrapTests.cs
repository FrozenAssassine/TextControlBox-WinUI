#pragma warning disable MSTEST0037
using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using TextControlBoxNS;
using TextControlBoxNS.Core;
using TextControlBoxNS.Core.Renderer;

namespace TextControlBox.Tests;

/// <summary>
/// Comprehensive unit and UI tests for the modern scroll and wrap pipeline:
/// WordWrap (visual-row model), long-line horizontal virtualization, centered horizontal reveal,
/// wrapped-line virtualization, and zoom anchoring.
/// </summary>
[TestClass]
public class ModernScrollAndWrapTests
{
    [UITestMethod]
    public void WordWrap_DefaultIsFalse()
    {
        var core = TestHelper.MakeCoreTextbox();
        Assert.IsFalse(core.WordWrap);

        var tb = new TextControlBoxNS.TextControlBox();
        Assert.IsFalse(tb.WordWrap);
    }

    [UITestMethod]
    public void WordWrap_PropertyToggle_PropagatesToCore()
    {
        var tb = new TextControlBoxNS.TextControlBox();
        tb.WordWrap = true;
        Assert.IsTrue(tb.WordWrap);

        tb.WordWrap = false;
        Assert.IsFalse(tb.WordWrap);
    }

    [UITestMethod]
    public void WordWrap_Enabled_PinsHorizontalOffsetAtZero()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 10);
        core.WordWrap = true;

        // Try centered reveal
        core.ScrollIntoViewHorizontallyCentered();

        Assert.AreEqual(0.0, core.scrollManager.OffsetSource.HorizontalOffset, 0.001);
    }

    [UITestMethod]
    public void WordWrap_MoveCursorByVisualRows_NavigatesCleanly()
    {
        var core = TestHelper.MakeCoreTextbox();
        // Load text with lines
        core.SetText("First line is short.\nSecond line is also short.\nThird line is simple.");
        core.WordWrap = true;

        core.SetCursorPosition(0, 0);
        Assert.AreEqual(0, core.cursorManager.LineNumber);
        Assert.AreEqual(0, core.cursorManager.CharacterPosition);

        // Move down by 1 visual row
        bool moved = core.textRenderer.MoveCursorByVisualRows(core.canvasText, core.cursorManager.currentCursorPosition, 1);
        Assert.IsTrue(moved);
        Assert.AreEqual(1, core.cursorManager.LineNumber);

        // Move down again
        moved = core.textRenderer.MoveCursorByVisualRows(core.canvasText, core.cursorManager.currentCursorPosition, 1);
        Assert.IsTrue(moved);
        Assert.AreEqual(2, core.cursorManager.LineNumber);

        // Move up by 1 visual row
        moved = core.textRenderer.MoveCursorByVisualRows(core.canvasText, core.cursorManager.currentCursorPosition, -1);
        Assert.IsTrue(moved);
        Assert.AreEqual(1, core.cursorManager.LineNumber);
    }

    [UITestMethod]
    public void LongLine_HorizontalVirtualization_CanLoadAndNavigate()
    {
        var core = TestHelper.MakeCoreTextbox();
        // Create a pathological 60,000-char line (exceeds HorizontalVirtualizationThreshold = 50k)
        string longLine = new string('a', 60000);
        core.SetText(longLine);

        Assert.AreEqual(1, core.textManager.LinesCount);
        Assert.AreEqual(60000, core.textManager.GetLineLength(0));

        // Position cursor in the middle and near the end
        core.SetCursorPosition(0, 30000);
        Assert.AreEqual(30000, core.cursorManager.CharacterPosition);

        core.SetCursorPosition(0, 59990);
        Assert.AreEqual(59990, core.cursorManager.CharacterPosition);

        // Moving right near end should succeed and stop at line end
        core.cursorManager.MoveRight();
        Assert.AreEqual(59991, core.cursorManager.CharacterPosition);
    }

    [UITestMethod]
    public void ScrollIntoViewHorizontallyCentered_PublicApi_WorksWithoutException()
    {
        var tb = new TextControlBoxNS.TextControlBox();
        tb.LoadLines(["Short line", new string('x', 500), "Another line"]);

        tb.SetCursorPosition(250, 1);
        // Invoke public method
        tb.ScrollIntoViewHorizontallyCentered();
    }

    [TestMethod]
    public void WrappedLineVirtualization_RowEstimation_HandlesVeryLongLine()
    {
        var metrics = new WrapRowMetrics();
        // Simulate a line that estimates 1000 rows
        metrics.Rebuild(1, _ => 1000);

        Assert.AreEqual(1000, metrics.TotalVisualRows);
        Assert.AreEqual(0, metrics.GetLineStartRow(0, 1));
        Assert.AreEqual(1000, metrics.GetLineStartRow(1, 1));
        Assert.AreEqual(0, metrics.GetDocumentLineFromVisualRow(500, 1));
    }

    [UITestMethod]
    public void ZoomScrollAnchor_Integration_DoesNotThrowOnZoom()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 200);
        core.SetCursorPosition(50, 0);

        // Change zoom factor up and down
        core.ZoomFactor = 150;

        Assert.IsTrue(core.scrollManager.OffsetSource.VerticalOffset >= 0);

        core.ZoomFactor = 75;

        Assert.IsTrue(core.scrollManager.OffsetSource.VerticalOffset >= 0);
    }

    [UITestMethod]
    public void CoreTextControlBox_Unload_CleansUpDiagonalScroll()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 10);
        // Unload should tear down composition tracker and timer cleanly
        core.Unload();
    }

    [UITestMethod]
    public void WordWrap_LineNumbers_AlignWithWrappedRows()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        // Short line, long wrapping line, short line
        core.SetText("Line 1\nA very long line that wraps across multiple visual rows in word wrap mode because it contains many words and characters.\nLine 3");
        core.WordWrap = true;
        core.textRenderer.EnsureTextFormat();
        core.textRenderer.EnsureWrapMetrics(core.canvasText);
        core.lineNumberRenderer.CreateLineNumberTextFormat();
        core.textRenderer.NumberOfRenderedLines = core.textManager.LinesCount;
        core.lineNumberRenderer.CheckGenerateLineNumberText();

        string lineNumberText = core.lineNumberRenderer.LineNumberTextToRender;
        Assert.IsNotNull(lineNumberText);

        string[] rows = lineNumberText.Split(Environment.NewLine, StringSplitOptions.None);
        // First row must be "1"
        Assert.AreEqual("1", rows[0]);
        // Second row must be "2" (start of line 2)
        Assert.AreEqual("2", rows[1]);
        // Wrapped continuation rows of line 2 must be blank
        int line2RowCount = core.textRenderer.GetWrappedRowCount(1);
        Assert.IsTrue(line2RowCount > 1, "Line 2 should wrap to at least 2 rows");
        for (int r = 1; r < line2RowCount; r++)
        {
            Assert.AreEqual("", rows[1 + r], $"Continuation row {r} of wrapped line 2 should be empty string");
        }
        // Next row after line 2's wrapped rows must be "3"
        Assert.AreEqual("3", rows[1 + line2RowCount]);
    }

    [UITestMethod]
    public void CursorRenderer_WordWrapOff_CaretPositionAndHeight()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("Line 1\nLine 2\nLine 3");
        core.WordWrap = false;
        core.SetCursorPosition(0, 0);

        core.textRenderer.EnsureTextFormat();
        core.textRenderer.UpdateCurrentLineTextLayout(core.canvasText);

        var cursorPoint = core.GetCursorPosition();
        float singleLineHeight = core.textRenderer.SingleLineHeight;
        float topInset = core.textRenderer.TopInset;

        // In non-wrap mode, line 0 caret Y should be TopInset
        Assert.AreEqual(topInset, (float)cursorPoint.Y, 0.5f);

        // Caret on line 1 should be at TopInset + SingleLineHeight
        core.SetCursorPosition(1, 0);
        var cursorPointLine1 = core.GetCursorPosition();
        Assert.AreEqual(topInset + singleLineHeight, (float)cursorPointLine1.Y, 0.5f);
    }

    [UITestMethod]
    public void CursorRenderer_WordWrapOn_LineHighlighterMatchesCursorRow()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("Line 1 short\nA very long line that wraps across multiple visual rows in word wrap mode because it contains many words and characters.\nLine 3 short");
        core.WordWrap = true;
        core.textRenderer.EnsureTextFormat();
        core.textRenderer.UpdateRenderedLineRange(core.canvasText);
        core.textRenderer.UpdateCurrentLineTextLayout(core.canvasText);

        float singleLineHeight = core.textRenderer.SingleLineHeight;
        float topInset = core.textRenderer.TopInset;

        // Cursor on line 0 (row 0)
        core.SetCursorPosition(0, 0);
        var p0 = core.GetCursorPosition();
        Assert.AreEqual(topInset, (float)p0.Y, 0.5f);

        // Cursor on line 1, column 0 (first row of line 1)
        core.SetCursorPosition(1, 0);
        core.textRenderer.UpdateCurrentLineTextLayout(core.canvasText);
        var pLine1Row0 = core.GetCursorPosition();
        Assert.AreEqual(topInset + singleLineHeight, (float)pLine1Row0.Y, 0.5f);

        // Cursor on line 1 at end (wrapped row of line 1)
        int line1Len = core.textManager.GetLineLength(1);
        core.SetCursorPosition(1, line1Len);
        core.textRenderer.UpdateCurrentLineTextLayout(core.canvasText);
        var pLine1RowEnd = core.GetCursorPosition();
        int wrappedRowCount = core.textRenderer.GetWrappedRowCount(1);
        Assert.AreEqual(topInset + (wrappedRowCount) * singleLineHeight, (float)pLine1RowEnd.Y, 0.5f);
    }

    [UITestMethod]
    public void WordWrap_HorizontalScrollBarMaximum_IsAlwaysZero()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText(new string('w', 10000));
        core.WordWrap = true;

        core.scrollManager.EnsureHorizontalScrollBounds(core.canvasText, core.longestLineManager, false, true);
        Assert.AreEqual(0.0, core.scrollManager.horizontalScrollBar.Maximum);
    }

    [UITestMethod]
    public void VirtualizedWrappedLine_IndexMapping_RowStrideIsAccurate()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText(new string('x', 60000));
        core.WordWrap = true;
        core.textRenderer.EnsureTextFormat();

        // Simulate virtualization state: charsPerRow = 100, sliceStart = 1000
        core.textRenderer.IsVirtualizedWrappedLine = true;
        core.textRenderer.VirtualizedLineCharsPerRow = 100;
        core.textRenderer.VirtualizedLineSliceStart = 1000;
        core.textRenderer.RenderedText = new string('x', 2000);
        int newlineLen = core.textManager.NewLineCharacter.Length;

        // Position 1000 is start of slice -> row 0, col 0 -> layoutIndex 0
        int idx0 = core.textRenderer.GetRenderedLayoutIndexForVirtualizedWrappedLine(1000);
        Assert.AreEqual(0, idx0);

        // Position 1050 is row 0, col 50 -> layoutIndex 50
        int idx50 = core.textRenderer.GetRenderedLayoutIndexForVirtualizedWrappedLine(1050);
        Assert.AreEqual(50, idx50);

        // Position 1100 is row 1, col 0 -> layoutIndex 100 + newlineLen
        int idxRow1 = core.textRenderer.GetRenderedLayoutIndexForVirtualizedWrappedLine(1100);
        Assert.AreEqual(100 + newlineLen, idxRow1);
    }

    [UITestMethod]
    public void HitTesting_NonWrap_ClickOnLine0And1_MapsCorrectly()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("Line 0\nLine 1\nLine 2");
        core.WordWrap = false;
        core.textRenderer.EnsureTextFormat();

        float lineHeight = core.textRenderer.SingleLineHeight;
        float topInset = core.textRenderer.TopInset;

        // TopInset is consistent
        Assert.AreEqual(core.textRenderer.TopInset, topInset);

        // Click on Line 0 (y in [0, lineHeight))
        int line0 = TextControlBoxNS.Helper.CursorHelper.GetCursorLineFromPoint(core.textRenderer, new Windows.Foundation.Point(10, 5));
        Assert.AreEqual(0, line0);

        // Click on Line 1 (y in [lineHeight, 2 * lineHeight))
        int line1 = TextControlBoxNS.Helper.CursorHelper.GetCursorLineFromPoint(core.textRenderer, new Windows.Foundation.Point(10, lineHeight + 5));
        Assert.AreEqual(1, line1);
    }
    [UITestMethod]
    public void WordWrap_HitTesting_ClickPastEndOfVisualRow_ReturnsPositionAfterLastChar()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        // Line 0 wraps across multiple rows: row 0 ends with a colon
        string text = "Hello world this is a wrapped line that ends with colon: and this text continues on row one";
        core.SetText(text);
        core.WordWrap = true;
        core.textRenderer.EnsureTextFormat();
        core.textRenderer.CalculateLinesToRender();

        int colonIndex = text.IndexOf(':');
        Assert.IsTrue(colonIndex > 0);

        // Update current line text layout
        core.textRenderer.UpdateCurrentLineTextLayout(core.canvasText);
        Assert.IsNotNull(core.textRenderer.CurrentLineTextLayout);

        // Hit-test for cursor placement (isSelecting = false): cursor stays on visual row 0 at the colon
        var cursorPos = new TextControlBoxNS.CursorPosition(0, 0);
        float hitY = core.textRenderer.TopInset + core.textRenderer.SingleLineHeight * 0.5f;
        TextControlBoxNS.Helper.CursorHelper.UpdateCursorPosFromPoint(
            core.canvasText,
            core.currentLineManager,
            core.textRenderer,
            core.scrollManager,
            new Windows.Foundation.Point(9999, hitY),
            cursorPos,
            isSelecting: false);

        Assert.AreEqual(0, cursorPos.LineNumber);
        Assert.AreEqual(colonIndex, cursorPos.CharacterPosition, "For cursor placement, cursor must stay on row 0 (at colon) instead of jumping to row 1 at col 0");
        Assert.IsTrue(cursorPos.IsTrailing, "Cursor must have IsTrailing set to true when placed at the end of a wrapped line");

        // Set cursor in core and verify GetCursorPosition() is on row 0 at the trailing edge (behind the colon)
        core.cursorManager.SetCursorPositionCopyValues(cursorPos);
        var cursorPoint = core.GetCursorPosition();
        Assert.AreEqual(core.textRenderer.TopInset, (float)cursorPoint.Y, 0.5f, "Cursor Y must be on row 0 (TopInset)");
        Assert.IsTrue(cursorPoint.X > 0, "Cursor X must be at the end of row 0 (behind colon)");

        // Hit-test for selection (isSelecting = true): must yield colonIndex + 1 so selection includes the colon
        var selPos = new TextControlBoxNS.CursorPosition(0, 0);
        TextControlBoxNS.Helper.CursorHelper.UpdateCursorPosFromPoint(
            core.canvasText,
            core.currentLineManager,
            core.textRenderer,
            core.scrollManager,
            new Windows.Foundation.Point(9999, hitY),
            selPos,
            isSelecting: true);
        Assert.AreEqual(colonIndex + 1, selPos.CharacterPosition, "Selection must include the colon");

        // Typing a character at trailing position must insert after the colon
        core.textActionManager.AddCharacter("X");
        Assert.AreEqual("X", core.textManager.GetLineText(0).Substring(colonIndex + 1, 1), "Typing with IsTrailing must insert after the colon");

        // Check CaretPosition trailing on colon vs leading on next char
        var trailingColon = core.textRenderer.CurrentLineTextLayout.GetCaretPosition(colonIndex, true);
        var leadingNext = core.textRenderer.CurrentLineTextLayout.GetCaretPosition(colonIndex + 1, false);
        Assert.IsTrue(trailingColon.Y < leadingNext.Y, $"Trailing caret Y ({trailingColon.Y}) must be on row 0, above row 1 leading Y ({leadingNext.Y})");
        Assert.IsTrue(trailingColon.X > 0, "Trailing caret X must be at the end of row 0");
    }

    [UITestMethod]
    public void ClickPastEndOfLine5_WordWrapTrue_DoesNotJumpToLine6()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("Line 0\nLine 1\nLine 2\nLine 3\nLine 4\nLine 5\nLine 6");
        core.WordWrap = true;
        core.textRenderer.EnsureTextFormat();
        core.textRenderer.CalculateLinesToRender();

        float lineHeight = core.textRenderer.SingleLineHeight;
        float hitY = core.textRenderer.TopInset + 5.5f * lineHeight; // Line 5
        var cursorPos = new TextControlBoxNS.CursorPosition(0, 0);

        TextControlBoxNS.Helper.CursorHelper.UpdateCursorPosFromPoint(
            core.canvasText,
            core.currentLineManager,
            core.textRenderer,
            core.scrollManager,
            new Windows.Foundation.Point(9999, hitY),
            cursorPos,
            isSelecting: false);

        Assert.AreEqual(5, cursorPos.LineNumber);
        Assert.AreEqual(6, cursorPos.CharacterPosition);
    }

    [UITestMethod]
    public void SelectLine_TripleClickSimulation_SelectsCorrectLine()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("Zeile 1 kurz\n" + "START_" + new string('X', 500) + "_END\n" + "Zeile 3 kurz");
        core.WordWrap = true;
        core.textRenderer.EnsureTextFormat();

        // Select line 1 (the wrapped line)
        bool success = core.SelectLine(1);
        Assert.IsTrue(success);
        Assert.AreEqual(1, core.selectionManager.selectionStart.LineNumber);
        Assert.AreEqual(0, core.selectionManager.selectionStart.CharacterPosition);
        Assert.AreEqual(1, core.selectionManager.selectionEnd.LineNumber);
        Assert.IsTrue(core.selectionManager.HasSelection);
    }

    [UITestMethod]
    public void CalculateWrappedLinesToRender_IncludesLongLineWhenStartingBeforeIt()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        string longLine = "START_" + new string('X', 100_000) + "_END";
        core.SetText("Line 0 short\n" + longLine + "\nLine 2 short");
        core.WordWrap = true;
        core.textRenderer.EnsureTextFormat();

        var (startLine, linesToRender) = core.textRenderer.CalculateLinesToRender();
        Assert.AreEqual(0, startLine);
        // Includes line 0 and the virtualized visible slice of line 1
        Assert.AreEqual(2, linesToRender);
    }

    [UITestMethod]
    public void WordWrap_MeasureWrappedRowCount_SingleLineIsExactlyOneRow()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("Line 1 short\nLine 2 short\nLine 3 short");
        core.WordWrap = true;
        core.textRenderer.EnsureTextFormat();

        Assert.AreEqual(1, core.textRenderer.GetWrappedRowCount(0));
        Assert.AreEqual(1, core.textRenderer.GetWrappedRowCount(1));
        Assert.AreEqual(1, core.textRenderer.GetWrappedRowCount(2));
    }

    [UITestMethod]
    public void WordWrap_ArrowDown_NavigatesAcrossShortLines()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("First line.\nSecond line.\nThird line.");
        core.WordWrap = true;
        core.textRenderer.EnsureTextFormat();

        core.SetCursorPosition(0, 0);
        Assert.AreEqual(0, core.cursorManager.LineNumber);

        // Move down to line 1
        bool moved = core.textRenderer.MoveCursorByVisualRows(core.canvasText, core.cursorManager.currentCursorPosition, 1);
        Assert.IsTrue(moved);
        Assert.AreEqual(1, core.cursorManager.LineNumber);

        // Move down to line 2
        moved = core.textRenderer.MoveCursorByVisualRows(core.canvasText, core.cursorManager.currentCursorPosition, 1);
        Assert.IsTrue(moved);
        Assert.AreEqual(2, core.cursorManager.LineNumber);

        // Move down past the end should return false and clamp
        moved = core.textRenderer.MoveCursorByVisualRows(core.canvasText, core.cursorManager.currentCursorPosition, 1);
        Assert.IsFalse(moved);
        Assert.AreEqual(2, core.cursorManager.LineNumber);
    }

    [UITestMethod]
    public void WordWrap_ArrowDown_NavigatesAcrossVisualRowsWithinSingleWrappedLine()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        // Create a line that definitely wraps across multiple visual rows at default width (800)
        string longWrappedLine = string.Join(" ", Enumerable.Repeat("The quick brown fox jumps over the lazy dog", 25));
        core.SetText(longWrappedLine + "\nNext short line.");
        core.WordWrap = true;
        core.textRenderer.EnsureTextFormat();

        int rowCountLine0 = core.textRenderer.GetWrappedRowCount(0);
        Assert.IsTrue(rowCountLine0 >= 2, $"Expected at least 2 visual rows, got {rowCountLine0}");

        core.SetCursorPosition(0, 0);
        Assert.AreEqual(0, core.cursorManager.LineNumber);
        Assert.AreEqual(0, core.cursorManager.CharacterPosition);

        // Each Arrow Down within line 0 should advance visual row and character position while staying on line 0
        int prevCharPos = core.cursorManager.CharacterPosition;
        for (int r = 1; r < rowCountLine0; r++)
        {
            bool moved = core.textRenderer.MoveCursorByVisualRows(core.canvasText, core.cursorManager.currentCursorPosition, 1);
            Assert.IsTrue(moved, $"Arrow Down on visual row {r - 1} should succeed");
            Assert.AreEqual(0, core.cursorManager.LineNumber, $"Arrow Down should remain on line 0 at visual row {r}");
            Assert.IsTrue(core.cursorManager.CharacterPosition > prevCharPos, $"Row {r} of {rowCountLine0}: expected char pos > {prevCharPos}, got {core.cursorManager.CharacterPosition}");
            prevCharPos = core.cursorManager.CharacterPosition;
        }

        // The next Arrow Down from the last visual row of line 0 should advance to line 1
        bool movedToLine1 = core.textRenderer.MoveCursorByVisualRows(core.canvasText, core.cursorManager.currentCursorPosition, 1);
        Assert.IsTrue(movedToLine1);
        Assert.AreEqual(1, core.cursorManager.LineNumber);
    }

    [UITestMethod]
    public void WordWrap_ArrowUp_NavigatesAcrossVisualRowsAndLines()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        string longWrappedLine = string.Join(" ", Enumerable.Repeat("The quick brown fox jumps over the lazy dog", 25));
        core.SetText(longWrappedLine + "\nNext short line.");
        core.WordWrap = true;
        core.textRenderer.EnsureTextFormat();

        int rowCountLine0 = core.textRenderer.GetWrappedRowCount(0);
        Assert.IsTrue(rowCountLine0 >= 2);

        // Start on line 1
        core.SetCursorPosition(1, 0);
        Assert.AreEqual(1, core.cursorManager.LineNumber);

        // Arrow Up from line 1 should move to the LAST visual row of line 0
        bool moved = core.textRenderer.MoveCursorByVisualRows(core.canvasText, core.cursorManager.currentCursorPosition, -1);
        Assert.IsTrue(moved);
        Assert.AreEqual(0, core.cursorManager.LineNumber);

        // Then Arrow Up should move through earlier visual rows of line 0
        int prevCharPos = core.cursorManager.CharacterPosition;
        for (int r = rowCountLine0 - 2; r >= 0; r--)
        {
            moved = core.textRenderer.MoveCursorByVisualRows(core.canvasText, core.cursorManager.currentCursorPosition, -1);
            Assert.IsTrue(moved, $"Arrow Up to visual row {r} should succeed");
            Assert.AreEqual(0, core.cursorManager.LineNumber);
            Assert.IsTrue(core.cursorManager.CharacterPosition < prevCharPos, "Character position should decrease when moving up");
            prevCharPos = core.cursorManager.CharacterPosition;
        }

        // Arrow Up at the very top (line 0, row 0) should return false
        moved = core.textRenderer.MoveCursorByVisualRows(core.canvasText, core.cursorManager.currentCursorPosition, -1);
        Assert.IsFalse(moved);
        Assert.AreEqual(0, core.cursorManager.LineNumber);
    }

    [UITestMethod]
    public void WordWrap_ArrowLeftAndRight_NavigatesAcrossVisualRows()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("Line 0 is short\nLine 1 is short");
        core.WordWrap = true;

        core.SetCursorPosition(0, 0);
        int line0Len = core.textManager.GetLineLength(0);

        // Move Right until end of line 0
        for (int i = 0; i < line0Len; i++)
        {
            core.cursorManager.MoveRight();
            Assert.AreEqual(0, core.cursorManager.LineNumber);
            Assert.AreEqual(i + 1, core.cursorManager.CharacterPosition);
        }

        // Next MoveRight should wrap to line 1, position 0
        core.cursorManager.MoveRight();
        Assert.AreEqual(1, core.cursorManager.LineNumber);
        Assert.AreEqual(0, core.cursorManager.CharacterPosition);

        // MoveLeft from line 1, position 0 should wrap back to end of line 0
        core.cursorManager.MoveLeft();
        Assert.AreEqual(0, core.cursorManager.LineNumber);
        Assert.AreEqual(line0Len, core.cursorManager.CharacterPosition);
    }

    [UITestMethod]
    public void WordWrap_ArrowKeys_WithEmptyLines()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("Line 0\n\nLine 2");
        core.WordWrap = true;
        core.textRenderer.EnsureTextFormat();

        core.SetCursorPosition(0, 0);

        // Move down from Line 0 to empty Line 1
        bool moved = core.textRenderer.MoveCursorByVisualRows(core.canvasText, core.cursorManager.currentCursorPosition, 1);
        Assert.IsTrue(moved);
        Assert.AreEqual(1, core.cursorManager.LineNumber);
        Assert.AreEqual(0, core.cursorManager.CharacterPosition);

        // Move down from empty Line 1 to Line 2
        moved = core.textRenderer.MoveCursorByVisualRows(core.canvasText, core.cursorManager.currentCursorPosition, 1);
        Assert.IsTrue(moved);
        Assert.AreEqual(2, core.cursorManager.LineNumber);

        // Move up from Line 2 to empty Line 1
        moved = core.textRenderer.MoveCursorByVisualRows(core.canvasText, core.cursorManager.currentCursorPosition, -1);
        Assert.IsTrue(moved);
        Assert.AreEqual(1, core.cursorManager.LineNumber);
        Assert.AreEqual(0, core.cursorManager.CharacterPosition);

        // Move up from empty Line 1 to Line 0
        moved = core.textRenderer.MoveCursorByVisualRows(core.canvasText, core.cursorManager.currentCursorPosition, -1);
        Assert.IsTrue(moved);
        Assert.AreEqual(0, core.cursorManager.LineNumber);
    }

    [UITestMethod]
    public void WordWrap_VerticalNavigation_PreservesPreferredHorizontalPositionAcrossShorterLine()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("    public void ScrollPageDown()\n    {\n        cursorManager.LineNumber += 1;");
        core.WordWrap = true;
        core.textRenderer.EnsureTextFormat();

        // Line 0: "    public void ScrollPageDown()", index 16 is after "    public void "
        core.SetCursorPosition(0, 16);
        Assert.AreEqual(0, core.cursorManager.LineNumber);
        Assert.AreEqual(16, core.cursorManager.CharacterPosition);

        // Arrow down to Line 1 ("    {", length 5)
        bool moved = core.textRenderer.MoveCursorByVisualRows(core.canvasText, core.cursorManager.currentCursorPosition, 1);
        Assert.IsTrue(moved);
        Assert.AreEqual(1, core.cursorManager.LineNumber);
        Assert.AreEqual(5, core.cursorManager.CharacterPosition, "Should clamp to end of short line");

        // Arrow down again to Line 2 ("        cursorManager.LineNumber += 1;", length 38)
        moved = core.textRenderer.MoveCursorByVisualRows(core.canvasText, core.cursorManager.currentCursorPosition, 1);
        Assert.IsTrue(moved);
        Assert.AreEqual(2, core.cursorManager.LineNumber);
        Assert.AreEqual(16, core.cursorManager.CharacterPosition, "Should restore preferred column 16 on line 2");

        // Arrow up back to Line 1
        moved = core.textRenderer.MoveCursorByVisualRows(core.canvasText, core.cursorManager.currentCursorPosition, -1);
        Assert.IsTrue(moved);
        Assert.AreEqual(1, core.cursorManager.LineNumber);
        Assert.AreEqual(5, core.cursorManager.CharacterPosition, "Should clamp to end of line 1 again");

        // Arrow up back to Line 0
        moved = core.textRenderer.MoveCursorByVisualRows(core.canvasText, core.cursorManager.currentCursorPosition, -1);
        Assert.IsTrue(moved);
        Assert.AreEqual(0, core.cursorManager.LineNumber);
        Assert.AreEqual(16, core.cursorManager.CharacterPosition, "Should restore preferred column 16 on line 0");
    }

    [UITestMethod]
    public void NonWrap_VerticalNavigation_PreservesPreferredHorizontalPositionAcrossShorterLine()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("    public void ScrollPageDown()\n    {\n        cursorManager.LineNumber += 1;");
        core.WordWrap = false;

        core.SetCursorPosition(0, 16);
        Assert.AreEqual(0, core.cursorManager.LineNumber);
        Assert.AreEqual(16, core.cursorManager.CharacterPosition);

        // MoveDown to Line 1 ("    {", length 5)
        core.cursorManager.MoveDown();
        Assert.AreEqual(1, core.cursorManager.LineNumber);
        Assert.AreEqual(5, core.cursorManager.CharacterPosition);

        // MoveDown to Line 2
        core.cursorManager.MoveDown();
        Assert.AreEqual(2, core.cursorManager.LineNumber);
        Assert.AreEqual(16, core.cursorManager.CharacterPosition, "Should restore column 16 on line 2");

        // MoveUp to Line 1
        core.cursorManager.MoveUp();
        Assert.AreEqual(1, core.cursorManager.LineNumber);
        Assert.AreEqual(5, core.cursorManager.CharacterPosition);

        // MoveUp to Line 0
        core.cursorManager.MoveUp();
        Assert.AreEqual(0, core.cursorManager.LineNumber);
        Assert.AreEqual(16, core.cursorManager.CharacterPosition, "Should restore column 16 on line 0");
    }

    [UITestMethod]
    public void Selection_InMultiLineWithVirtualizedWrappedLine_MapsCleanly()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        string longLine = "START_" + new string('X', 100_000) + "_END";
        core.SetText("Line 0 short\n" + longLine + "\nLine 2 short");
        core.WordWrap = true;
        core.textRenderer.EnsureTextFormat();

        // Simulate multi-line rendering where StartLine is 0 and Line 1 is virtualized
        core.textRenderer.NumberOfStartLine = 0;
        core.textRenderer.NumberOfRenderedLines = 2;
        core.textRenderer.IsVirtualizedWrappedLine = true;
        core.textRenderer.VirtualizedLineIndex = 1;
        core.textRenderer.VirtualizedLineCharsPerRow = 100;
        core.textRenderer.VirtualizedLineSliceStart = 0;
        core.textRenderer.VirtualizedWrappedRowsToRender = 10;
        core.textRenderer.RenderedText = "Line 0 short\n" + new string('X', 1000);

        int line0Prefix = core.textManager.GetLineLength(0) + core.textManager.NewLineCharacter.Length;

        // Selection on Line 1 from char 10 to char 50
        int idxStart = core.textRenderer.GetRenderedLayoutIndexForVirtualizedWrappedLine(1, 10);
        int idxEnd = core.textRenderer.GetRenderedLayoutIndexForVirtualizedWrappedLine(1, 50);

        Assert.AreEqual(line0Prefix + 10, idxStart);
        Assert.AreEqual(line0Prefix + 50, idxEnd);
        Assert.IsTrue(idxEnd > idxStart);

        // Selection spanning from Line 0 (char 5) to Line 1 (char 25)
        int crossStart = core.textRenderer.GetRenderedLayoutIndexForVirtualizedWrappedLine(0, 5);
        int crossEnd = core.textRenderer.GetRenderedLayoutIndexForVirtualizedWrappedLine(1, 25);

        Assert.AreEqual(5, crossStart);
        Assert.AreEqual(line0Prefix + 25, crossEnd);
        Assert.IsTrue(crossEnd > crossStart);
    }

    [UITestMethod]
    public void Selection_SingleVirtualizedWrappedLine_ClampsToBounds()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        string longLine = new string('X', 100_000);
        core.SetText(longLine);
        core.WordWrap = true;
        core.textRenderer.EnsureTextFormat();

        core.textRenderer.NumberOfStartLine = 0;
        core.textRenderer.NumberOfRenderedLines = 1;
        core.textRenderer.IsVirtualizedWrappedLine = true;
        core.textRenderer.VirtualizedLineIndex = 0;
        core.textRenderer.VirtualizedLineCharsPerRow = 100;
        core.textRenderer.VirtualizedLineSliceStart = 5000;
        core.textRenderer.VirtualizedWrappedRowsToRender = 5; // 500 chars rendered
        core.textRenderer.RenderedText = new string('X', 500 + 4 * core.textManager.NewLineCharacter.Length);

        // Before slice start -> clamps to 0
        int idxBefore = core.textRenderer.GetRenderedLayoutIndexForVirtualizedWrappedLine(0, 1000);
        Assert.AreEqual(0, idxBefore);

        // At slice start (5000) -> 0
        int idxAtStart = core.textRenderer.GetRenderedLayoutIndexForVirtualizedWrappedLine(0, 5000);
        Assert.AreEqual(0, idxAtStart);

        // Char 5050 -> row 0, col 50 -> 50
        int idxRow0 = core.textRenderer.GetRenderedLayoutIndexForVirtualizedWrappedLine(0, 5050);
        Assert.AreEqual(50, idxRow0);

        // Char 5150 -> row 1, col 50 -> 100 + newlineLen + 50
        int newlineLen = core.textManager.NewLineCharacter.Length;
        int idxRow1 = core.textRenderer.GetRenderedLayoutIndexForVirtualizedWrappedLine(0, 5150);
        Assert.AreEqual(100 + newlineLen + 50, idxRow1);
    }

    [UITestMethod]
    public void Cursor_VirtualizedWrappedLine_Scrolled_CaretYMatchesVisualRow()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        string longLine = new string('X', 100_000);
        core.SetText(longLine);
        core.WordWrap = true;
        core.textRenderer.EnsureTextFormat();

        core.textRenderer.NumberOfStartLine = 0;
        core.textRenderer.NumberOfRenderedLines = 1;
        core.textRenderer.IsVirtualizedWrappedLine = true;
        core.textRenderer.VirtualizedLineIndex = 0;
        core.textRenderer.VirtualizedLineCharsPerRow = 100;
        core.textRenderer.WrappedStartRowOffset = 50; // scrolled down 50 rows
        core.textRenderer.StartVisualRow = 50;
        core.textRenderer.VirtualizedLineSliceStart = 5000;
        core.textRenderer.VirtualizedWrappedRowsToRender = 10;
        core.textRenderer.RenderedText = new string('X', 1000 + 9 * core.textManager.NewLineCharacter.Length);
        core.textRenderer.UpdateCurrentLineTextLayout(core.canvasText);

        // GetCurrentLineLayoutTopY for line 0 when virtualized and at start line must be 0, not -50 * SingleLineHeight
        float layoutTop = core.textRenderer.GetCurrentLineLayoutTopY(0);
        Assert.AreEqual(0f, layoutTop, 0.001f);

        // Cursor at char 5200 (row 2 in the visible slice, col 0)
        core.SetCursorPosition(0, 5200, scrollIntoView: false);
        var pt = core.GetCursorPosition();
        float singleLine = core.textRenderer.SingleLineHeight;

        float topInset = core.textRenderer.TopInset;

        // Visual row in slice is 2, so Y must be topInset + 2 * singleLine (around 40px), NOT negative or shifted up by 50 rows
        Assert.AreEqual(topInset + 2 * singleLine, (float)pt.Y, 1.0f);
    }

    [UITestMethod]
    public void UpdateScrollToShowCursor_WhenCaretVisible_DoesNotScroll()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        string longLine = new string('X', 100_000);
        core.SetText(longLine);
        core.WordWrap = true;
        core.textRenderer.EnsureTextFormat();

        core.textRenderer.NumberOfStartLine = 0;
        core.textRenderer.NumberOfRenderedLines = 1;
        core.textRenderer.IsVirtualizedWrappedLine = true;
        core.textRenderer.VirtualizedLineIndex = 0;
        core.textRenderer.VirtualizedLineCharsPerRow = 100;
        core.textRenderer.WrappedStartRowOffset = 50;
        core.textRenderer.StartVisualRow = 50;
        core.textRenderer.VirtualizedLineSliceStart = 5000;
        core.textRenderer.VirtualizedWrappedRowsToRender = 10;
        core.textRenderer.RenderedText = new string('X', 1000 + 9 * core.textManager.NewLineCharacter.Length);
        core.textRenderer.UpdateCurrentLineTextLayout(core.canvasText);

        float singleLine = core.textRenderer.SingleLineHeight;
        double initialOffset = 50 * singleLine;
        core.scrollManager.OffsetSource.VerticalOffset = initialOffset;

        // Caret is at char 5200 (visible in the slice at row 2)
        core.SetCursorPosition(0, 5200, scrollIntoView: false);

        // UpdateScrollToShowCursor should detect that the caret is visible on screen and not change VerticalOffset
        core.scrollManager.UpdateScrollToShowCursor(update: false);
        Assert.AreEqual(initialOffset, core.scrollManager.OffsetSource.VerticalOffset, 0.001);
    }

    [UITestMethod]
    public void WordWrap_ScrollbarMaximum_IncludesBottomBuffer()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        string longLine = new string('A', 100_000);
        core.SetText(longLine);
        core.WordWrap = true;
        core.textRenderer.EnsureTextFormat();
        core.textRenderer.EnsureWrapMetrics(core.canvasText);

        core.textRenderer.CalculateLinesToRender();

        float singleLine = core.textRenderer.SingleLineHeight;
        int sensitivity = core.scrollManager.DefaultVerticalScrollSensitivity;
        double gridHeight = core.canvasText != null && core.canvasText.ActualHeight > 0 ? core.canvasText.ActualHeight : core.scrollGrid.ActualHeight;

        int totalRows = core.textRenderer.GetWrappedRowCount(0);
        double expectedMax = Math.Max(0, ((totalRows + TextRenderer.WrappedBottomBufferRows) * singleLine - gridHeight) / sensitivity);
        Assert.AreEqual(expectedMax, core.scrollManager.verticalScrollBar.Maximum, 0.5);
    }

    [UITestMethod]
    public void WordWrap_MaxScroll_LastLineAlwaysFullyVisible_AcrossWindowHeights()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        // 50 lines of wrapped text
        var lines = System.Linq.Enumerable.Range(0, 50).Select(i => $"Line {i}: Some text to test word wrap buffering at the bottom of the editor").ToArray();
        core.LoadLines(lines);
        core.WordWrap = true;
        core.textRenderer.EnsureTextFormat();
        core.textRenderer.EnsureWrapMetrics(core.canvasText);

        float singleLine = core.textRenderer.SingleLineHeight;
        int sensitivity = core.scrollManager.DefaultVerticalScrollSensitivity;
        int totalVisualRows = 0;
        for (int i = 0; i < lines.Length; i++)
            totalVisualRows += core.textRenderer.GetWrappedRowCount(i);

        // Test various window heights: multiples of singleLine, sub-line remainders, small, medium, large
        double[] testHeights = [ 200, 205, 219, 300, 311, 400, 401, 415, 419, 420, 421, 500, 505.5, 612.3 ];
        foreach (double height in testHeights)
        {
            double maxScroll = Math.Max(0, ((totalVisualRows + TextRenderer.WrappedBottomBufferRows) * singleLine - height) / sensitivity);
            core.scrollManager.verticalScrollBar.Maximum = maxScroll;
            core.scrollManager.verticalScrollBar.Value = maxScroll;

            int startVisualRow = core.textRenderer.GetStartVisualRowFromScroll();
            int lastVisualRow = totalVisualRows - 1;

            // Distance from start visual row to last visual row
            int rowsFromStart = lastVisualRow - startVisualRow;
            // The top of the rendered layout starts with a SingleLineHeight offset
            float lastRowTop = singleLine + rowsFromStart * singleLine;
            float lastRowBottom = lastRowTop + singleLine;

            // The last row bottom must never exceed the viewport height (never cut off at the bottom)
            Assert.IsTrue(lastRowBottom <= height + 0.001,
                $"At window height {height}, last visual row bottom {lastRowBottom} exceeded viewport (cut off by {lastRowBottom - height}px)!");
        }
    }
}



