using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using System;
using TextControlBoxNS;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Helper;
using TextControlBoxNS.Models;
using TextControlBoxNS.Models.Enums;

namespace TextControlBox.Tests;

[TestClass]
public class ProductionStabilityTests
{
    [UITestMethod]
    public void Search_NullAndEmptyInputs_ReturnInvalidInput_WithoutCrashing()
    {
        var core = TestHelper.MakeCoreTextbox(5);

        var nullResult = core.searchManager.BeginSearch(null, false, false);
        Assert.AreEqual(SearchResult.InvalidInput, nullResult);
        Assert.IsFalse(core.searchManager.IsSearchOpen);

        var emptyResult = core.searchManager.BeginSearch("", false, false);
        Assert.AreEqual(SearchResult.InvalidInput, emptyResult);
        Assert.IsFalse(core.searchManager.IsSearchOpen);
    }

    [UITestMethod]
    public void Search_RegexSpecialCharacters_MatchesLiterally()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("Pattern: [abc] * (def)+ {1,2} $ ^ . ? \\");
        core.SetCursorPosition(0, 0);

        var res1 = core.searchManager.BeginSearch("[abc]", wholeWord: false, matchCase: true);
        Assert.AreEqual(SearchResult.Found, res1);

        var found1 = core.searchManager.FindNext(core.cursorManager.currentCursorPosition);
        Assert.AreEqual(SearchResult.Found, found1.Result);
        Assert.IsNotNull(found1.Selection);

        core.SetCursorPosition(0, 0);
        var res2 = core.searchManager.BeginSearch("(def)+", wholeWord: false, matchCase: true);
        Assert.AreEqual(SearchResult.Found, res2);

        core.SetCursorPosition(0, 0);
        var res3 = core.searchManager.BeginSearch("{1,2}", wholeWord: false, matchCase: true);
        Assert.AreEqual(SearchResult.Found, res3);

        core.SetCursorPosition(0, 0);
        var res4 = core.searchManager.BeginSearch("$ ^ . ? \\", wholeWord: false, matchCase: true);
        Assert.AreEqual(SearchResult.Found, res4);
    }

    [UITestMethod]
    public void Replace_NullAndEmptyWord_ReturnsInvalidInput_WithoutCrashing()
    {
        var core = TestHelper.MakeCoreTextbox(5);

        var resNull = core.replaceManager.ReplaceAll(null, "bar", false, false);
        Assert.AreEqual(SearchResult.InvalidInput, resNull);

        var resEmpty = core.replaceManager.ReplaceAll("", "bar", false, false);
        Assert.AreEqual(SearchResult.InvalidInput, resEmpty);
    }

    [UITestMethod]
    public void Replace_NullReplaceWord_TreatedAsEmptyString_WithoutCrashing()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("Hello world foo world");

        var res = core.replaceManager.ReplaceAll("world", null, matchCase: true, wholeWord: false);
        Assert.AreEqual(SearchResult.Found, res);
        Assert.AreEqual("Hello  foo ", core.GetText());
    }

    [UITestMethod]
    public void ReplaceNext_NullReplaceWord_ReplacesWithEmptyString()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("Alpha Beta Gamma");
        core.SetCursorPosition(0, 0);

        core.searchManager.BeginSearch("Beta", wholeWord: false, matchCase: true);
        var replaceRes = core.replaceManager.ReplaceNext(null);
        Assert.AreEqual(SearchResult.Found, replaceRes.Result);
        Assert.AreEqual("Alpha  Gamma", core.GetText());
    }

    [UITestMethod]
    public void AddCharacter_NullString_DoesNotCrash()
    {
        var core = TestHelper.MakeCoreTextbox(5);
        string textBefore = core.GetText();

        core.textActionManager.AddCharacter(null);
        Assert.AreEqual(textBefore, core.GetText());
    }

    [UITestMethod]
    public void AddCharacter_EmptyString_NoSelection_DoesNotAlterText()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("Sample text");
        core.SetCursorPosition(0, 6);

        core.textActionManager.AddCharacter("");
        Assert.AreEqual("Sample text", core.GetText());
        Assert.AreEqual(6, core.cursorManager.CharacterPosition);
    }

    [UITestMethod]
    public void AddNewLine_WithFullSelection_PositionsCursorAtLine1Column0()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("Line 1\nLine 2\nLine 3");
        core.SelectAll();

        core.textActionManager.AddNewLine();

        Assert.AreEqual(2, core.textManager.LinesCount);
        Assert.AreEqual("", core.textManager.GetLineText(0));
        Assert.AreEqual("", core.textManager.GetLineText(1));
        Assert.AreEqual(1, core.cursorManager.LineNumber);
        Assert.AreEqual(0, core.cursorManager.CharacterPosition);
    }

    [UITestMethod]
    public void RemoveLineAbove_BackspaceAtLineStart_UndoRedoRestoresExactText()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.LineEnding = LineEnding.LF;
        core.SetText("First Line\nSecond Line\nThird Line");
        core.SetCursorPosition(1, 0);

        var undoRedoResult = TestHelper.CheckUndoRedo(core, () =>
        {
            core.textActionManager.RemoveText();
        });

        Assert.IsTrue(undoRedoResult.undo, "Undo should restore exact 3 lines");
        Assert.IsTrue(undoRedoResult.redo, "Redo should restore merged line");
        Assert.AreEqual("First LineSecond Line\nThird Line", core.GetText());

        core.Undo();
        Assert.AreEqual("First Line\nSecond Line\nThird Line", core.GetText());
        Assert.AreEqual(3, core.textManager.LinesCount);
        Assert.AreEqual("Third Line", core.textManager.GetLineText(2));
    }

    [UITestMethod]
    public void DeleteTextAction_MergeWithLineBelow_UndoRedoRestoresExactText()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.LineEnding = LineEnding.LF;
        core.SetText("Line A\nLine B\nLine C");
        core.SetCursorPosition(0, 6); // at end of "Line A"

        var undoRedoResult = TestHelper.CheckUndoRedo(core, () =>
        {
            core.textActionManager.DeleteText();
        });

        Assert.IsTrue(undoRedoResult.undo);
        Assert.IsTrue(undoRedoResult.redo);
        Assert.AreEqual("Line ALine B\nLine C", core.GetText());

        core.Undo();
        Assert.AreEqual("Line A\nLine B\nLine C", core.GetText());
    }

    [UITestMethod]
    public void AutoIndention_Line0Indented_PreservesIndentationOnEnter()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("    int x = 42;");
        core.SetCursorPosition(0, 15); // end of line 0

        core.textActionManager.AddNewLine();

        Assert.AreEqual(2, core.textManager.LinesCount);
        Assert.AreEqual("    int x = 42;", core.textManager.GetLineText(0));
        Assert.IsTrue(core.textManager.GetLineText(1).StartsWith("    ", StringComparison.Ordinal),
            "Line 1 should inherit indentation from indented line 0");
    }

    [UITestMethod]
    public void Selection_GetSelectionFromPosition_MatchesCalculateSelectionStartLength_WithCRLF()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.LineEnding = LineEnding.CRLF;
        core.SetText("Hello\r\nWorld\r\nTest");

        // Select "World" which is after "Hello\r\n" (5 + 2 = index 7, length 5)
        core.SetSelection(7, 5);

        var (start, len) = core.selectionManager.CalculateSelectionStartLength();
        Assert.AreEqual(7, start, "Start index should match CRLF offset");
        Assert.AreEqual(5, len, "Length should match World");

        var ordered = core.selectionManager.OrderTextSelectionSeparated();
        Assert.AreEqual(1, ordered.startLine);
        Assert.AreEqual(0, ordered.startChar);
        Assert.AreEqual(1, ordered.endLine);
        Assert.AreEqual(5, ordered.endChar);
    }

    [UITestMethod]
    public void Selection_GetSelectionFromPosition_MatchesCalculateSelectionStartLength_WithLF()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.LineEnding = LineEnding.LF;
        core.SetText("Hello\nWorld\nTest");

        // Select "World" which is after "Hello\n" (5 + 1 = index 6, length 5)
        core.SetSelection(6, 5);

        var (start, len) = core.selectionManager.CalculateSelectionStartLength();
        Assert.AreEqual(6, start, "Start index should match LF offset");
        Assert.AreEqual(5, len, "Length should match World");

        var ordered = core.selectionManager.OrderTextSelectionSeparated();
        Assert.AreEqual(1, ordered.startLine);
        Assert.AreEqual(0, ordered.startChar);
        Assert.AreEqual(1, ordered.endLine);
        Assert.AreEqual(5, ordered.endChar);
    }

    [UITestMethod]
    public void Selection_MultiLineSpanning_CalculatesAccurateLength()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.LineEnding = LineEnding.CRLF;
        core.SetText("AAA\r\nBBB\r\nCCC");

        // Select from middle of AAA (index 2) through middle of BBB (index 2)
        // "A\r\nBB" = 1 + 2 + 2 = 5 characters
        core.SetSelection(2, 5);

        var (start, len) = core.selectionManager.CalculateSelectionStartLength();
        Assert.AreEqual(2, start);
        Assert.AreEqual(5, len);

        var ordered = core.selectionManager.OrderTextSelectionSeparated();
        Assert.AreEqual(0, ordered.startLine);
        Assert.AreEqual(2, ordered.startChar);
        Assert.AreEqual(1, ordered.endLine);
        Assert.AreEqual(2, ordered.endChar);
    }

    [UITestMethod]
    public void Zoom_ExtremeValues_ClampsCleanly()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 5);

        core.ZoomFactor = -999;
        Assert.IsGreaterThanOrEqualTo(core.ZoomFactor, 4, "ZoomFactor should clamp to MinZoom (4)");

        core.ZoomFactor = 99999;
        Assert.IsLessThanOrEqualTo(core.ZoomFactor, 400, "ZoomFactor should clamp to MaxZoom (400)");

        core.ZoomFactor = 100;
        Assert.AreEqual(100, core.ZoomFactor);
    }

    [UITestMethod]
    public void TextAction_DeleteAtDocumentEdges_DoesNotCrash()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("Single Line");

        // Backspace at (0, 0)
        core.SetCursorPosition(0, 0);
        core.textActionManager.RemoveText();
        Assert.AreEqual("Single Line", core.GetText());

        // Delete at end of document
        core.SetCursorPosition(0, 11);
        core.textActionManager.DeleteText();
        Assert.AreEqual("Single Line", core.GetText());
    }

    [UITestMethod]
    public void TabsSpacesHelper_NullStringAndEmpty_HandledSafely()
    {
        var (useSpaces, spaces) = TabsSpacesHelper.DetectTabsSpaces((string)null!);
        Assert.IsFalse(useSpaces);
        Assert.AreEqual(4, spaces);

        var (useSpacesEmpty, spacesEmpty) = TabsSpacesHelper.DetectTabsSpaces("");
        Assert.IsTrue(useSpacesEmpty);
        Assert.AreEqual(4, spacesEmpty);
    }

    [UITestMethod]
    public void MoveLine_Boundaries_ReturnsFalseWithoutThrowing()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.LineEnding = LineEnding.LF;
        core.SetText("Line 0\nLine 1");

        core.SetCursorPosition(0, 0);
        Assert.IsFalse(core.moveLineManager.Move(LineMoveDirection.Up), "Moving line 0 up should return false");

        core.SetCursorPosition(1, 0);
        Assert.IsFalse(core.moveLineManager.Move(LineMoveDirection.Down), "Moving last line down should return false");

        // Successful move down of line 0
        core.SetCursorPosition(0, 0);
        Assert.IsTrue(core.moveLineManager.Move(LineMoveDirection.Down));
        Assert.AreEqual("Line 1\nLine 0", core.GetText());
        Assert.AreEqual(1, core.cursorManager.LineNumber);

        // Undo move
        core.Undo();
        Assert.AreEqual("Line 0\nLine 1", core.GetText());
        Assert.AreEqual(0, core.cursorManager.LineNumber);
    }

    [UITestMethod]
    public void PublicApi_SurroundSelectionWith_NullOrEmpty_Safe()
    {
        var tb = TestHelper.MakeTextbox(3);
        tb.SetSelection(0, 3);

        // Multiline check throws ArgumentException as documented
        Assert.ThrowsExactly<ArgumentException>(() =>
        {
            tb.SurroundSelectionWith("(\n", ")");
        });

        // Surrounding with valid single-line tokens
        bool success = tb.SurroundSelectionWith("[", "]");
        Assert.IsTrue(success);
        Assert.IsTrue(tb.GetText().StartsWith("[Lin]e 0", StringComparison.Ordinal));
    }

    [UITestMethod]
    public void DuplicateCurrentLine_WorksAndCanUndo()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.LineEnding = LineEnding.LF;
        core.SetText("Hello\nWorld");
        core.SetCursorPosition(0, 2);

        var res = TestHelper.CheckUndoRedo(core, () =>
        {
            core.DuplicateCurrentLine();
        });

        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);
        Assert.AreEqual("Hello\nHello\nWorld", core.GetText());
    }

    [UITestMethod]
    public void ClickPlacement_WordWrapOff_HitTestingLeftAndRightOfCharacter_PlacesCursorAccurately()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.WordWrap = false;
        core.SetText("ABCDEFGHIJ");
        core.textRenderer.EnsureTextFormat();
        core.textRenderer.CalculateLinesToRender();
        core.textRenderer.UpdateCurrentLineTextLayout(core.canvasText);

        var layout = core.textRenderer.CurrentLineTextLayout;
        Assert.IsNotNull(layout);
        // Position of character index 3 ('D')
        var caret3Leading = layout.GetCaretPosition(3, false);
        var caret3Trailing = layout.GetCaretPosition(3, true);
        float charWidth = caret3Trailing.X - caret3Leading.X;
        float charMidX = caret3Leading.X + charWidth * 0.5f;

        float hitY = core.textRenderer.TopInset + core.textRenderer.SingleLineHeight * 0.5f;
        var cursorPos = new TextControlBoxNS.CursorPosition(0, 0);

        // Hit test on the left half of character 3 ('D') -> should place cursor before 'D' (index 3)
        float leftHalfX = caret3Leading.X + charWidth * 0.2f;
        TextControlBoxNS.Helper.CursorHelper.UpdateCursorPosFromPoint(
            core.canvasText,
            core.currentLineManager,
            core.textRenderer,
            core.scrollManager,
            new Windows.Foundation.Point(leftHalfX, hitY),
            cursorPos,
            isSelecting: false);

        Assert.AreEqual(0, cursorPos.LineNumber);
        Assert.AreEqual(3, cursorPos.CharacterPosition, "Clicking left half of 'D' must place cursor at index 3 (before 'D')");

        // Hit test on the right half of character 3 ('D') -> should place cursor after 'D' (index 4)
        float rightHalfX = caret3Leading.X + charWidth * 0.8f;
        TextControlBoxNS.Helper.CursorHelper.UpdateCursorPosFromPoint(
            core.canvasText,
            core.currentLineManager,
            core.textRenderer,
            core.scrollManager,
            new Windows.Foundation.Point(rightHalfX, hitY),
            cursorPos,
            isSelecting: false);

        Assert.AreEqual(0, cursorPos.LineNumber);
        Assert.AreEqual(4, cursorPos.CharacterPosition, "Clicking right half of 'D' must place cursor at index 4 (after 'D')");
    }

    [UITestMethod]
    public void ClickPlacement_Zoomed_WordWrapOff_HitTestingLeftOfCharacter_DoesNotShiftToNextCharacter()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.WordWrap = false;
        core.ZoomFactor = 200; // 200% zoom
        core.SetText("ABCDEFGHIJ");
        core.textRenderer.EnsureTextFormat();
        core.textRenderer.CalculateLinesToRender();
        core.textRenderer.UpdateCurrentLineTextLayout(core.canvasText);

        var layout = core.textRenderer.CurrentLineTextLayout;
        Assert.IsNotNull(layout);
        var caret5Leading = layout.GetCaretPosition(5, false);
        var caret5Trailing = layout.GetCaretPosition(5, true);
        float charWidth = caret5Trailing.X - caret5Leading.X;

        float hitY = core.textRenderer.TopInset + core.textRenderer.SingleLineHeight * 0.5f;
        var cursorPos = new TextControlBoxNS.CursorPosition(0, 0);

        // Click on the left quarter of character 5 ('F') at 200% zoom
        float clickX = caret5Leading.X + charWidth * 0.25f;
        TextControlBoxNS.Helper.CursorHelper.UpdateCursorPosFromPoint(
            core.canvasText,
            core.currentLineManager,
            core.textRenderer,
            core.scrollManager,
            new Windows.Foundation.Point(clickX, hitY),
            cursorPos,
            isSelecting: false);

        Assert.AreEqual(0, cursorPos.LineNumber);
        Assert.AreEqual(5, cursorPos.CharacterPosition, "At 200% zoom, clicking left quarter of 'F' must place cursor at index 5");
    }
}
