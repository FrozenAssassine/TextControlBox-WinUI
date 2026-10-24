using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using System.Linq;
using TextControlBoxNS;
using TextControlBoxNS.Core;

namespace TextControlBox.Tests.RegressionTests;

[TestClass]
public class FatalBugRegressionTests
{
    private static CoreTextControlBox MakeCore(string text)
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText(text);
        core.ClearUndoRedoHistory();
        return core;
    }

    private static string Clean(CoreTextControlBox core, string text) => core.stringManager.CleanUpString(text);

    //Removing or inserting an EMPTY line was not recorded in the undo stack ("" before == "" after),
    //which shifted every older undo item by one line and overwrote the wrong lines on undo.

    [UITestMethod]
    public void Undo_DeleteEmptyLine_IsRecordedAndKeepsOlderItemsAligned()
    {
        var core = MakeCore("A\n\nC\nD\nE");

        core.SetLineText(2, "Cx");
        core.DeleteLine(1);
        Assert.AreEqual(Clean(core, "A\nCx\nD\nE"), core.GetText());

        core.Undo();
        Assert.AreEqual(Clean(core, "A\n\nCx\nD\nE"), core.GetText());

        core.Undo();
        Assert.AreEqual(Clean(core, "A\n\nC\nD\nE"), core.GetText());

        core.Redo();
        core.Redo();
        Assert.AreEqual(Clean(core, "A\nCx\nD\nE"), core.GetText());
    }

    [UITestMethod]
    public void Undo_DeleteSelectedEmptyLine_IsRecorded()
    {
        var core = MakeCore("A\n\nC\nD");

        core.SetLineText(2, "Cx");

        //whole line selected including the newline: (1, 0) to (2, 0)
        core.SetSelection(1, 0, 2, 0);
        core.textActionManager.DeleteSelection();
        Assert.AreEqual(Clean(core, "A\nCx\nD"), core.GetText());

        core.Undo();
        Assert.AreEqual(Clean(core, "A\n\nCx\nD"), core.GetText());

        core.Undo();
        Assert.AreEqual(Clean(core, "A\n\nC\nD"), core.GetText());
    }

    [UITestMethod]
    public void Undo_PasteEmptyLine_IsRecorded()
    {
        var core = MakeCore("A\nB\nC");

        core.SetLineText(2, "Cx");
        core.SetCursorPosition(1, 0, scrollIntoView: false);
        core.textActionManager.PasteLine(core.textManager.NewLineCharacter);
        Assert.AreEqual(Clean(core, "A\n\nB\nCx"), core.GetText());

        core.Undo();
        Assert.AreEqual(Clean(core, "A\nB\nCx"), core.GetText());

        core.Undo();
        Assert.AreEqual(Clean(core, "A\nB\nC"), core.GetText());
    }

    [UITestMethod]
    public void Undo_AddEmptyLineAtEnd_IsRecorded()
    {
        var core = MakeCore("A\nB");

        core.AddLine(2, "");
        Assert.AreEqual(3, core.NumberOfLines);
        Assert.IsTrue(core.CanUndo);

        core.Undo();
        Assert.AreEqual(Clean(core, "A\nB"), core.GetText());
    }

    //Deleting the last line through a whole line selection (linenumber click / SelectLine) left the
    //cursor behind the last line. The next key press threw an IndexOutOfRangeException.

    [UITestMethod]
    public void DeleteSelectedLastLine_KeepsCursorInsideDocument()
    {
        var core = MakeCore("A\nB\nC");

        core.SelectLine(2);
        core.Delete();

        Assert.AreEqual(Clean(core, "A\nB"), core.GetText());
        Assert.IsTrue(core.CursorPosition.LineNumber < core.NumberOfLines);

        core.cursorManager.MoveLeft();
        core.textActionManager.AddCharacter("x");
        core.textActionManager.AddNewLine();
        Assert.AreEqual(3, core.NumberOfLines);

        while (core.CanUndo)
            core.Undo();
        Assert.AreEqual(Clean(core, "A\nB\nC"), core.GetText());
    }

    //The matching search lines were not refreshed by LoadText/LoadLines/SetText, FindNext and
    //FindPrevious then accessed lines that do not exist anymore.

    [UITestMethod]
    public void Search_AfterLoadingShorterText_DoesNotThrow()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.LoadLines(Enumerable.Range(0, 50).Select(i => i == 40 ? "foo here" : "line " + i));
        Assert.AreEqual(SearchResult.Found, core.BeginSearch("foo", false, false));

        core.LoadText("short document");
        Assert.AreNotEqual(SearchResult.Found, core.FindNext());
        Assert.AreNotEqual(SearchResult.Found, core.FindPrevious());

        core.LoadLines(["nothing", "also nothing"]);
        Assert.AreNotEqual(SearchResult.Found, core.FindNext());
    }

    [UITestMethod]
    public void Search_AfterSetText_FindsMatchesInNewText()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.LoadLines(Enumerable.Range(0, 50).Select(i => i == 40 ? "foo here" : "line " + i));
        core.BeginSearch("foo", false, false);

        core.SetText("a foo\nb");
        core.SetCursorPosition(0, 0, scrollIntoView: false);

        Assert.AreEqual(SearchResult.Found, core.FindNext());
        Assert.AreEqual(0, core.CursorPosition.LineNumber);
    }

    //ReplaceAll passed the replacement to Regex.Replace, where "$0", "$&" or "$$" are substitutions.

    [UITestMethod]
    public void ReplaceAll_UsesReplacementLiterally()
    {
        var core = MakeCore("the price is\nno match");

        Assert.AreEqual(SearchResult.Found, core.ReplaceAll("price", "$0.99 ($$) $&", true, false));
        Assert.AreEqual(Clean(core, "the $0.99 ($$) $& is\nno match"), core.GetText());

        core.Undo();
        Assert.AreEqual(Clean(core, "the price is\nno match"), core.GetText());
    }

    [UITestMethod]
    public void ReplaceAll_MultilineReplacement_SplitsLinesAndSurvivesUndoRedo()
    {
        var core = MakeCore("a,b\nc\nd,e");
        string before = core.GetText();

        Assert.AreEqual(SearchResult.Found, core.ReplaceAll(",", "\n", true, false));
        Assert.AreEqual(5, core.NumberOfLines);
        string after = Clean(core, "a\nb\nc\nd\ne");
        Assert.AreEqual(after, core.GetText());

        core.Undo();
        Assert.AreEqual(before, core.GetText());
        core.Redo();
        Assert.AreEqual(after, core.GetText());
        core.Undo();
        Assert.AreEqual(before, core.GetText());
        Assert.AreEqual(3, core.NumberOfLines);
    }

    //SelectedText (and with it copy/cut) mixed up the start and end column of multiline selections,
    //whenever the selection started in a higher column than it ended.

    [UITestMethod]
    public void SelectedText_MultiLine_StartColumnGreaterThanEndColumn()
    {
        var core = MakeCore("0123456789\nbbbb\ncccccc");
        string expected = Clean(core, "6789\nbbbb\ncc");

        core.SetSelection(0, 6, 2, 2);
        Assert.AreEqual(expected, core.SelectedText);

        core.SetSelection(2, 2, 0, 6);
        Assert.AreEqual(expected, core.SelectedText);

        core.SetSelection(0, 9, 1, 0);
        Assert.AreEqual(Clean(core, "9\n"), core.SelectedText);

        core.SetSelection(0, 2, 1, 4);
        Assert.AreEqual(Clean(core, "23456789\nbbbb"), core.SelectedText);
    }

    //With LF or CR the newline is a single character, which was counted as "one line of text".

    [UITestMethod]
    public void SingleCharNewLine_EnterWithSelection_UndoRestoresText()
    {
        foreach (var lineEnding in new[] { LineEnding.LF, LineEnding.CR, LineEnding.CRLF })
        {
            var core = MakeCore("hello world\nsecond\nthird");
            core.LineEnding = lineEnding;
            core.ClearUndoRedoHistory();
            string before = core.GetText();

            core.SetSelection(0, 5, 0, 6);
            core.textActionManager.AddNewLine();
            Assert.AreEqual(4, core.NumberOfLines, lineEnding.ToString());
            Assert.AreEqual("hello", core.GetLineText(0));
            Assert.AreEqual("world", core.GetLineText(1));

            core.Undo();
            Assert.AreEqual(before, core.GetText(), lineEnding.ToString());
            core.Redo();
            Assert.AreEqual(4, core.NumberOfLines, lineEnding.ToString());
            core.Undo();
            Assert.AreEqual(before, core.GetText(), lineEnding.ToString());
        }
    }

    [UITestMethod]
    public void SingleCharNewLine_InsertedWithoutSelection_SplitsTheLine()
    {
        foreach (var lineEnding in new[] { LineEnding.LF, LineEnding.CR, LineEnding.CRLF })
        {
            var core = MakeCore("hello\nsecond");
            core.LineEnding = lineEnding;
            core.ClearUndoRedoHistory();
            string before = core.GetText();

            core.SetCursorPosition(0, 2, scrollIntoView: false);
            core.textActionManager.AddCharacter(core.textManager.NewLineCharacter);

            Assert.AreEqual(3, core.NumberOfLines, lineEnding.ToString());
            Assert.AreEqual("he", core.GetLineText(0));
            Assert.AreEqual("llo", core.GetLineText(1));

            core.Undo();
            Assert.AreEqual(before, core.GetText(), lineEnding.ToString());
        }
    }

    [UITestMethod]
    public void SingleCharNewLine_ReplaceNext_UndoRestoresText()
    {
        var core = MakeCore("a,b\nc");
        core.LineEnding = LineEnding.LF;
        core.ClearUndoRedoHistory();
        string before = core.GetText();

        core.SetCursorPosition(0, 0, scrollIntoView: false);
        core.BeginSearch(",", false, true);
        core.ReplaceNext("\n");
        Assert.AreEqual(3, core.NumberOfLines);

        core.Undo();
        Assert.AreEqual(before, core.GetText());
    }

    //Undo/Redo handled grouped items recursively and overflowed the stack on large groups.

    [UITestMethod]
    public void ActionGroup_Large_UndoRedoDoesNotOverflowTheStack()
    {
        var core = TestHelper.MakeCoreTextbox(100);
        core.ClearUndoRedoHistory();
        string before = core.GetText();

        core.ExecuteActionGroup(() =>
        {
            for (int i = 0; i < 50000; i++)
                core.SetLineText(i % 100, "v" + i);
        });
        string after = core.GetText();

        core.Undo();
        Assert.AreEqual(before, core.GetText());
        Assert.IsFalse(core.CanUndo);

        core.Redo();
        Assert.AreEqual(after, core.GetText());
        Assert.IsFalse(core.CanRedo);

        core.Undo();
        Assert.AreEqual(before, core.GetText());
    }
}
