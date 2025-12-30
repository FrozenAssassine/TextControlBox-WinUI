using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using System;
using TextControlBoxNS;
using TextControlBoxNS.Core;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Helper;

namespace TextControlBox.Tests;

[TestClass]
public class InternalLogicTests
{
    private static (bool undo, bool redo) CheckUndoRedo(CoreTextControlBox core, Action action, int count = 1)
        => TestHelper.CheckUndoRedo(core, action, count);

    [UITestMethod]
    public void TabsSpacesHelper_DetectTabsSpaces_String_GcdBasedIndent_IsDetected()
    {
        // Indent levels: 2, 4, 6 => diff-GCD = 2
        var text = "Line1\n  Line2\n    Line3\n      Line4\n";

        var (useSpaces, spaces) = TabsSpacesHelper.DetectTabsSpaces(text);

        Assert.IsTrue(useSpaces);
        Assert.AreEqual(2, spaces);
    }

    [UITestMethod]
    public void TabsSpacesHelper_DetectTabsSpaces_String_SingleIndentDepth_ReturnsDepth()
    {
        var text = "Line1\n    Line2\n    Line3\n";

        var (useSpaces, spaces) = TabsSpacesHelper.DetectTabsSpaces(text);

        Assert.IsTrue(useSpaces);
        Assert.AreEqual(4, spaces);
    }

    [UITestMethod]
    public void TabsSpacesHelper_DetectTabsSpaces_String_PrefersTabs_WhenMoreTabIndentedLines()
    {
        var text = "\tLine1\n\tLine2\n    Line3\n";

        var (useSpaces, spaces) = TabsSpacesHelper.DetectTabsSpaces(text);

        Assert.IsFalse(useSpaces);
        Assert.AreEqual(4, spaces);
    }

    [UITestMethod]
    public void AutoPairing_AutoPair_AddsClosingPair_AndKeepsCursorAdvancementLength()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SyntaxHighlighting = TextControlBoxNS.TextControlBox.GetSyntaxHighlightingFromID(SyntaxHighlightID.CSharp);
        core.DoAutoPairing = true;

        var (text, length) = AutoPairing.AutoPair(core, "(");

        Assert.AreEqual("()", text);
        Assert.AreEqual(1, length, "Should keep cursor advance at original input length.");
    }

    [UITestMethod]
    public void AutoPairing_AutoPair_Disabled_ReturnsInputUnchanged()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SyntaxHighlighting = TextControlBoxNS.TextControlBox.GetSyntaxHighlightingFromID(SyntaxHighlightID.CSharp);
        core.DoAutoPairing = false;

        var (text, length) = AutoPairing.AutoPair(core, "(");

        Assert.AreEqual("(", text);
        Assert.AreEqual(1, length);
    }

    [UITestMethod]
    public void AutoPairing_AutoPairSelection_SurroundsSelection_AndReturnsNull()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SyntaxHighlighting = TextControlBoxNS.TextControlBox.GetSyntaxHighlightingFromID(SyntaxHighlightID.CSharp);
        core.DoAutoPairing = true;

        core.SetText("Hello");
        core.SetSelection(1, 3); // "ell"

        var res = AutoPairing.AutoPairSelection(core, "\"");

        Assert.IsNull(res);
        Assert.AreEqual("H\"ell\"o", core.GetText());
    }

    [UITestMethod]
    public void SearchManager_FindNextAndPrevious_ReturnsReachedEndReachedBegin()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("abc\nabc\nabc");

        Assert.AreEqual(SearchResult.Found, core.BeginSearch("abc", wholeWord: false, matchCase: true));

        core.SetCursorPosition(2, 3, scrollIntoView: false);
        Assert.AreEqual(SearchResult.ReachedEnd, core.FindNext());

        core.SetCursorPosition(0, 0, scrollIntoView: false);
        Assert.AreEqual(SearchResult.ReachedBegin, core.FindPrevious());
    }

    // ------------------------------
    // AddCharacter (AddCharacterTextAction)
    // ------------------------------

    [UITestMethod]
    public void AddCharacter_NoSelection_SingleChar_InsertsAtCursor()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("Hello");

        core.SetCursorPosition(0, 2, scrollIntoView: false);

        var res = CheckUndoRedo(core, () => core.textActionManager.AddCharacter("X"));

        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);
        Assert.AreEqual("HeXllo", core.GetText());
        Assert.AreEqual(0, core.CursorPosition.LineNumber);
        Assert.AreEqual(3, core.CursorPosition.CharacterPosition);
    }

    [UITestMethod]
    public void AddCharacter_NoSelection_SingleChar_AppendsAtLineEnd()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("Hello");

        core.SetCursorPosition(0, 5, scrollIntoView: false);

        var res = CheckUndoRedo(core, () => core.textActionManager.AddCharacter("!"));

        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);
        Assert.AreEqual("Hello!", core.GetText());
        Assert.AreEqual(6, core.CursorPosition.CharacterPosition);
    }

    [UITestMethod]
    public void AddCharacter_NoSelection_MultiLineText_SplitsLines_AndMovesCursor()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("HelloWorld");

        core.SetCursorPosition(0, 5, scrollIntoView: false);

        var inserted = core.stringManager.CleanUpString("A\nB\nC");

        var res = CheckUndoRedo(core, () => core.textActionManager.AddCharacter(inserted));

        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);

        Assert.AreEqual(3, core.NumberOfLines);
        Assert.AreEqual("HelloA", core.GetLineText(0));
        Assert.AreEqual("B", core.GetLineText(1));
        Assert.AreEqual("CWorld", core.GetLineText(2));

        Assert.AreEqual(2, core.CursorPosition.LineNumber);
        Assert.AreEqual(1, core.CursorPosition.CharacterPosition);
    }

    [UITestMethod]
    public void AddCharacter_SingleLineSelection_ReplacesSelection_TextIsClearedSelection()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("HelloWorld");

        // select "World"
        core.SetSelection(5, 5);

        var res = CheckUndoRedo(core, () => core.textActionManager.AddCharacter("X"));

        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);

        Assert.AreEqual("HelloX", core.GetText());
        Assert.IsFalse(core.HasSelection);
        Assert.AreEqual(0, core.CursorPosition.LineNumber);
        Assert.AreEqual(6, core.CursorPosition.CharacterPosition);
    }

    [UITestMethod]
    public void AddCharacter_MultiLineSelection_ReplacesSelection_WithMultiLineText()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.LineEnding = LineEnding.CR;
        core.SetText("abc\ndef\nghi");

        // selection spanning "c\ndef\ng"
        core.SetSelection(0, 2, 2, 1);

        var inserted = core.stringManager.CleanUpString("X\nY");

        var res = CheckUndoRedo(core, () => core.textActionManager.AddCharacter(inserted));

        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);

        Assert.AreEqual(2, core.NumberOfLines);
        Assert.AreEqual("abX", core.GetLineText(0));
        Assert.AreEqual("Yhi", core.GetLineText(1));
        Assert.IsFalse(core.HasSelection);
        Assert.AreEqual(1, core.CursorPosition.LineNumber);
        Assert.AreEqual(1, core.CursorPosition.CharacterPosition);
    }

    [UITestMethod]
    public void AddCharacter_Selection_EmptyString_DeletesSelection()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("abc\ndef");

        core.SetSelection(0, 1, 1, 2); // spans "bc\nde"

        var res = CheckUndoRedo(core, () => core.textActionManager.AddCharacter(""));

        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);

        Assert.AreEqual("af", core.GetText());
        Assert.IsFalse(core.HasSelection);
        Assert.AreEqual(0, core.CursorPosition.LineNumber);
        Assert.AreEqual(1, core.CursorPosition.CharacterPosition);
    }

    // ------------------------------
    // AddNewLine (AddNewLineTextAction)
    // ------------------------------

    [UITestMethod]
    public void AddNewLine_NoSelection_SplitsLine_AndMovesCursorToNewLine()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("HelloWorld");

        core.SetCursorPosition(0, 5, scrollIntoView: false);

        var res = CheckUndoRedo(core, () => core.textActionManager.AddNewLine());

        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);

        Assert.AreEqual(2, core.NumberOfLines);
        Assert.AreEqual("Hello", core.GetLineText(0));
        Assert.AreEqual("World", core.GetLineText(1));

        Assert.AreEqual(1, core.CursorPosition.LineNumber);
        Assert.AreEqual(0, core.CursorPosition.CharacterPosition);
    }

    [UITestMethod]
    public void AddNewLine_SingleLineSelection_ReplacesWithNewLine()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("HelloWorld");

        // select "World"
        core.SetSelection(5, 5);

        var res = CheckUndoRedo(core, () => core.textActionManager.AddNewLine());

        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);

        Assert.AreEqual(2, core.NumberOfLines);
        Assert.AreEqual("Hello", core.GetLineText(0));
        Assert.AreEqual(string.Empty, core.GetLineText(1));
        Assert.IsFalse(core.HasSelection);

        Assert.AreEqual(1, core.CursorPosition.LineNumber);
        Assert.AreEqual(0, core.CursorPosition.CharacterPosition);
    }

    [UITestMethod]
    public void AddNewLine_FullTextSelection_ClearsDocument_AndLeavesTwoLines()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("a\nb\nc");

        core.SelectAll();

        // Special-case path HandleFullTextSelection
        core.textActionManager.AddNewLine();

        Assert.AreEqual(2, core.NumberOfLines);
        Assert.AreEqual(string.Empty, core.GetLineText(0));
        Assert.AreEqual(string.Empty, core.GetLineText(1));
        Assert.IsFalse(core.HasSelection);
    }

    // ------------------------------
    // DeleteText (DeleteTextAction)
    // ------------------------------

    [UITestMethod]
    public void DeleteText_NoSelection_DeletesNextChar()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("Hello");

        core.SetCursorPosition(0, 1, scrollIntoView: false);

        var res = CheckUndoRedo(core, () => core.textActionManager.DeleteText(controlIsPressed: false));

        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);

        Assert.AreEqual("Hllo", core.GetText());
        Assert.AreEqual(1, core.CursorPosition.CharacterPosition);
    }

    [UITestMethod]
    public void DeleteText_NoSelection_AtLineEnd_MergesNextLine()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("ab\ncd");

        core.SetCursorPosition(0, 2, scrollIntoView: false);

        var res = CheckUndoRedo(core, () => core.textActionManager.DeleteText());

        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);

        Assert.AreEqual(1, core.NumberOfLines);
        Assert.AreEqual("abcd", core.GetLineText(0));
        Assert.AreEqual(2, core.CursorPosition.CharacterPosition);
    }

    [UITestMethod]
    public void DeleteText_ShiftPressed_NoSelection_DeletesCurrentLine()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("a\nb\nc");

        core.SetCursorPosition(1, 0, scrollIntoView: false);

        var res = CheckUndoRedo(core, () => core.textActionManager.DeleteText(controlIsPressed: false, shiftIsPressed: true));

        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);

        Assert.AreEqual(2, core.NumberOfLines);
        Assert.AreEqual("a", core.GetLineText(0));
        Assert.AreEqual("c", core.GetLineText(1));
    }

    [UITestMethod]
    public void DeleteText_WithSelection_DeletesSelection()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("abc\ndef");

        core.SetSelection(0, 1, 1, 2); // "bc\nde"

        var res = CheckUndoRedo(core, () => core.textActionManager.DeleteText());

        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);

        Assert.AreEqual("af", core.GetText());
        Assert.IsFalse(core.HasSelection);
    }

    // ------------------------------
    // RemoveText (RemoveTextAction) - Backspace semantics
    // ------------------------------

    [UITestMethod]
    public void RemoveText_NoSelection_RemovesPreviousChar()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("Hello");

        core.SetCursorPosition(0, 3, scrollIntoView: false);

        var res = CheckUndoRedo(core, () => core.textActionManager.RemoveText(controlIsPressed: false));

        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);

        Assert.AreEqual("Helo", core.GetText());
        Assert.AreEqual(2, core.CursorPosition.CharacterPosition);
    }

    [UITestMethod]
    public void RemoveText_NoSelection_AtLineStart_MergesWithPreviousLine()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("ab\ncd");

        core.SetCursorPosition(1, 0, scrollIntoView: false);

        var res = CheckUndoRedo(core, () => core.textActionManager.RemoveText(controlIsPressed: false));

        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);

        Assert.AreEqual(1, core.NumberOfLines);
        Assert.AreEqual("abcd", core.GetLineText(0));
        Assert.AreEqual(0, core.CursorPosition.LineNumber);
        Assert.AreEqual(2, core.CursorPosition.CharacterPosition);
    }

    [UITestMethod]
    public void RemoveText_WithSelection_DeletesSelection()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("abc\ndef");

        core.SetSelection(0, 1, 1, 2); // "bc\nde"

        var res = CheckUndoRedo(core, () => core.textActionManager.RemoveText(controlIsPressed: false));

        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);

        Assert.AreEqual("af", core.GetText());
        Assert.IsFalse(core.HasSelection);
    }
}
