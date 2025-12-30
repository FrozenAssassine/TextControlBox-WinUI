using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using System;
using TextControlBoxNS;
using Windows.UI.Text.Core;

namespace TextControlBox.Tests;

[TestClass]
public class AdditionalBehaviorTests
{
    [UITestMethod]
    public void Core_SetCursorPosition_AutoClampFalse_ThrowsOnInvalidValues()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 5);

        Assert.ThrowsExactly<IndexOutOfRangeException>(() => core.SetCursorPosition(-1, 0, scrollIntoView: false, autoClamp: false));
        Assert.ThrowsExactly<IndexOutOfRangeException>(() => core.SetCursorPosition(9999, 0, scrollIntoView: false, autoClamp: false));
        Assert.ThrowsExactly<IndexOutOfRangeException>(() => core.SetCursorPosition(0, -1, scrollIntoView: false, autoClamp: false));

        var len = core.GetLineText(0).Length;
        Assert.ThrowsExactly<IndexOutOfRangeException>(() => core.SetCursorPosition(0, len + 1, scrollIntoView: false, autoClamp: false));
    }

    [UITestMethod]
    public void Public_SetCursorPosition_IgnoresAutoClampFalse_BugRegression()
    {
        var tb = new TextControlBoxNS.TextControlBox();
        tb.LoadLines(["Hello"]);

        // This should throw if the public API correctly forwarded autoClamp=false.
        // It currently does NOT forward the parameter to coreTextBox.SetCursorPosition.
        // Keeping this test red makes the bug visible.
        Assert.ThrowsExactly<IndexOutOfRangeException>(() => tb.SetCursorPosition(-1, -1, scrollIntoView: false, autoClamp: false));
    }

    [UITestMethod]
    public void CalculateSelectionPosition_SelectAll_MatchesTextLength()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 10);
        core.LineEnding = LineEnding.CRLF;
        string text = core.GetText();

        core.SelectAll();
        var selPos = core.CalculateSelectionPosition();

        Assert.AreEqual(0, selPos.Index);
        Assert.AreEqual(text.Length, selPos.Length);
    }

    [UITestMethod]
    public void CalculateSelectionPosition_NoSelection_ReturnsZero()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 10);
        core.ClearSelection();

        var selPos = core.CalculateSelectionPosition();
        Assert.AreEqual(0, selPos.Index);
        Assert.AreEqual(0, selPos.Length);
    }

    [UITestMethod]
    public void Counts_CharacterCount_EqualsGetTextLength()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 20);
        core.LineEnding = LineEnding.CRLF;

        Assert.AreEqual(core.GetText().Length, core.CharacterCount());

        core.SetText("Line1\nLine2\n");
        Assert.AreEqual(core.GetText().Length, core.CharacterCount());
    }

    [UITestMethod]
    public void Counts_WordCount_BasicCases()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);

        core.SetText("  Hello   World  ");
        Assert.AreEqual(2, core.WordCount());

        core.SetText("\n\n");
        Assert.AreEqual(0, core.WordCount());

        core.SetText("Hello\nWorld\tTabbed");
        Assert.AreEqual(3, core.WordCount());
    }

    [UITestMethod]
    public void UndoRedoFlags_ClearUndoRedoHistory_ResetsCanUndoCanRedo()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 3);
        core.SetCursorPosition(0, 0, scrollIntoView: false);
        core.textActionManager.AddCharacter("X");

        Assert.IsTrue(core.CanUndo);

        core.ClearUndoRedoHistory();

        Assert.IsFalse(core.CanUndo);
        Assert.IsFalse(core.CanRedo);
    }

    [UITestMethod]
    public void FindNextWithoutBeginSearch_ReturnsSearchNotOpened()
    {
        var tb = TestHelper.MakeTextbox(0);
        tb.LoadText("Hello World");

        Assert.AreEqual(SearchResult.SearchNotOpened, tb.FindNext());
        Assert.AreEqual(SearchResult.SearchNotOpened, tb.FindPrevious());
    }

    [UITestMethod]
    public void EndSearch_ResetsSearchIsOpen()
    {
        var tb = TestHelper.MakeTextbox();
        tb.LoadText("Hello World\nHello World");

        Assert.AreEqual(SearchResult.Found, tb.BeginSearch("Hello", wholeWord: false, matchCase: false));
        Assert.IsTrue(tb.SearchIsOpen);

        tb.EndSearch();
        Assert.IsFalse(tb.SearchIsOpen);

        Assert.AreEqual(SearchResult.SearchNotOpened, tb.FindNext());
    }

    [UITestMethod]
    public void SurroundSelectionWith_NoSelection_DoesNotModifyText()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("Hello");
        core.ClearSelection();

        string before = core.GetText();
        core.SurroundSelectionWith("<b>", "</b>");

        Assert.AreEqual(before, core.GetText());
    }

    [UITestMethod]
    public void DuplicateLine_ReadOnly_ShouldNotDuplicate_WhenNotIgnored()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 5);
        core.IsReadOnly = true;

        int linesBefore = core.NumberOfLines;
        bool ok = core.DuplicateLine(1, ignoreIsReadOnly: false);

        Assert.IsFalse(ok);
        Assert.AreEqual(linesBefore, core.NumberOfLines);
    }

    [UITestMethod]
    public void TabSpaceManager_RewriteTabsSpaces_InvalidSpaces_Throws()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText("\tLine1");

        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => core.RewriteTabsSpaces(0, true));
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => core.RewriteTabsSpaces(-1, true));
    }
}
