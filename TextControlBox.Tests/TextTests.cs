using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using System;
using System.Diagnostics;
using TextControlBoxNS;
using TextControlBoxNS.Core;

namespace TextControlBox.Tests;

[TestClass]
public class TextTests
{
    private (bool undo, bool redo) CheckUndoRedo(CoreTextControlBox coreTextbox, int count = 1)
    {
        string textBefore = coreTextbox.GetText();

        for (int i = 0; i < count; i++)
            coreTextbox.Undo();

        string textAfter = coreTextbox.GetText();

        bool undoRes = !textAfter.Equals(textBefore);

        for (int i = 0; i < count; i++)
            coreTextbox.Redo();

        bool redoRes = coreTextbox.GetText().Equals(textBefore);

        //Debug.Assert(undoRes && redoRes);

        return (undoRes, redoRes);
    }

    [UITestMethod]
    public void ClearSelection()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();

        Random r = new Random();
        coreTextbox.SetSelection(r.Next(0, 10), r.Next(10, 50));
        coreTextbox.ClearSelection();

        var sel = coreTextbox.selectionManager.currentTextSelection;
        bool res = sel.StartPosition.IsNull && sel.EndPosition.IsNull && !coreTextbox.selectionManager.HasSelection;

        Debug.Assert(res);
    }

    [UITestMethod]
    public void DeleteLine5()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();

        int linesBefore = coreTextbox.NumberOfLines;
        coreTextbox.textActionManager.DeleteLine(4);

        CheckUndoRedo(coreTextbox);

        Debug.Assert(coreTextbox.NumberOfLines == linesBefore - 1);
    }

    [UITestMethod]
    public void AddLine5()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();

        int linesBefore = coreTextbox.NumberOfLines;
        var sel = coreTextbox.textActionManager.AddLine(3, "Hello World this is the text of line 3");

        CheckUndoRedo(coreTextbox);

        Debug.Assert(coreTextbox.NumberOfLines == linesBefore + 1 && coreTextbox.GetLineText(3).Equals("Hello World this is the text of line 3"));

    }

    [UITestMethod]
    public void AddChar_SingleLineText_NoSelection()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();

        string textBefore = coreTextbox.GetLineText(3);
        string textToAdd = "Add single line character";

        coreTextbox.SetCursorPosition(3, 0);
        int lineBefore = coreTextbox.cursorManager.currentCursorPosition.LineNumber;
        coreTextbox.textActionManager.AddCharacter(coreTextbox.stringManager.CleanUpString(textToAdd));

        var cur = coreTextbox.cursorManager.currentCursorPosition;

        CheckUndoRedo(coreTextbox);

        bool res =
            cur.CharacterPosition == textToAdd.Length &&
            cur.LineNumber == lineBefore &&
            coreTextbox.GetLineText(3).Equals(textToAdd + textBefore);

        Debug.Assert(res);
    }
    [UITestMethod]
    public void AddChar_MultilineText_NoSelection()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();

        string textToAdd = "Add Line 1\nAdd Line 2\nAdd Line 3";
        coreTextbox.SetCursorPosition(2, 10);
        int lineBefore = coreTextbox.cursorManager.currentCursorPosition.LineNumber;
        coreTextbox.textActionManager.AddCharacter(coreTextbox.stringManager.CleanUpString(textToAdd));

        CheckUndoRedo(coreTextbox);

        var cur = coreTextbox.cursorManager.currentCursorPosition;
        bool res =
            cur.LineNumber == lineBefore + 2 &&
            coreTextbox.GetLineText(2).Substring(10, 10).Equals("Add Line 1") &&
            coreTextbox.GetLineText(3).Substring(0, 10).Equals("Add Line 2") &&
            coreTextbox.GetLineText(4).Substring(0, 10).Equals("Add Line 3") &&
            cur.CharacterPosition == 10;
        Debug.Assert(res);
    }
    [UITestMethod]
    public void AddChar_NoTextNoSelection()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();

        coreTextbox.SelectAll();
        coreTextbox.textActionManager.AddCharacter("");

        CheckUndoRedo(coreTextbox);

        var cur = coreTextbox.cursorManager.currentCursorPosition;
        bool res =
            cur.LineNumber == 0 &&
            coreTextbox.GetText().Length == 0 &&
            cur.CharacterPosition == 0;

        Debug.Assert(res);
    }
    [UITestMethod]
    public void AddCharSingleLine_SingleLineSelected()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();

        coreTextbox.SetSelection(5, 10);
        coreTextbox.textActionManager.AddCharacter("Hello World");

        CheckUndoRedo(coreTextbox);

        var cur = coreTextbox.cursorManager.currentCursorPosition;

        Assert.AreEqual(0, cur.LineNumber);
        Assert.AreEqual(16, cur.CharacterPosition);
    }

    [UITestMethod]
    public void AddCharMultiline_SingleLineSelected()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();

        coreTextbox.SetSelection(5, 10);
        coreTextbox.textActionManager.AddCharacter(coreTextbox.stringManager.CleanUpString("Line1\nLine2\nLine3"));

        CheckUndoRedo(coreTextbox);

        var cur = coreTextbox.cursorManager.currentCursorPosition;

        Assert.AreEqual(2, cur.LineNumber);
        Assert.AreEqual(11, cur.CharacterPosition);
    }
    [UITestMethod]
    public void AddCharMultiline_MultilineSelection_EverythinkSelected()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();

        coreTextbox.SelectAll();
        string text = coreTextbox.stringManager.CleanUpString("Line1\nLine2\nLine3");
        coreTextbox.textActionManager.AddCharacter(text);

        CheckUndoRedo(coreTextbox);

        var cur = coreTextbox.cursorManager.currentCursorPosition;

        Assert.AreEqual(2, cur.LineNumber);
        Assert.AreEqual(5, cur.CharacterPosition);
        Assert.AreEqual(coreTextbox.GetText(), text);
    }
    [UITestMethod]
    public void DeleteSelectionMultiLineSelectionWholeLines()
    {
        var (coreTextbox, originalText) = TestHelper.MakeCoreTextboxWithText();

        coreTextbox.SelectLines(1, 3);
        coreTextbox.textActionManager.DeleteSelection();

        CheckUndoRedo(coreTextbox);

        var lines = coreTextbox.textManager.totalLines;

        var cur = coreTextbox.cursorManager.currentCursorPosition;

        Assert.AreEqual(1, cur.LineNumber);        
        Assert.AreEqual(0, cur.CharacterPosition);
        Assert.AreEqual(0, coreTextbox.GetLineText(1).Length);
        Assert.AreEqual(coreTextbox.GetLineText(0), originalText.Split(coreTextbox.textManager.NewLineCharacter)[0]);
    }
    [UITestMethod]
    public void Test_12()
    {
        var (coreTextbox, originalText) = TestHelper.MakeCoreTextboxWithText();

        coreTextbox.SetSelection(4, 10);
        coreTextbox.textActionManager.DeleteSelection();

        CheckUndoRedo(coreTextbox);

        string line1 = TestHelper.GetFirstLine(originalText, coreTextbox.textManager.NewLineCharacter).ToString();
        var expected = line1.Substring(0, 4) + line1.Substring(14);
        var cur = coreTextbox.cursorManager.currentCursorPosition;
        var t = coreTextbox.GetLineText(0);
        bool res =
            cur.LineNumber == 0 &&
            cur.CharacterPosition == 4 &&
            t.Trim().Equals(expected.Trim());

        Debug.Assert(res);
    }
    [UITestMethod]
    public void DeleteSelection_SingleLineSelection_WholeLine()
    {
        var (coreTextbox, originalText) = TestHelper.MakeCoreTextboxWithText();

        coreTextbox.SelectLine(0);
        coreTextbox.textActionManager.DeleteSelection();
        CheckUndoRedo(coreTextbox);

        var cur = coreTextbox.cursorManager.currentCursorPosition;

        var str1 = coreTextbox.GetLineText(0);
        var split = originalText.Split(coreTextbox.textManager.NewLineCharacter)[1];

        Assert.AreEqual(0, cur.LineNumber);
        Assert.AreEqual(0, cur.CharacterPosition);
        Assert.AreEqual(str1, split);
    }
    [UITestMethod]
    public void DeleteSelectionLinesSelectedCompletelyNotWholeText()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        int linesBefore = coreTextbox.textManager.LinesCount;

        coreTextbox.SetSelection(0, 0, 2, coreTextbox.GetLineText(2).Length);
        coreTextbox.textActionManager.DeleteSelection();

        CheckUndoRedo(coreTextbox);

        var lineText = coreTextbox.GetLineText(0);
        bool res = coreTextbox.textManager.LinesCount == linesBefore - 2 &&
                   lineText == "";

        Debug.Assert(res);
    }
    [UITestMethod]
    public void DeleteSelectionStartLineCompletelySelected()
    {
        var (coreTextbox, originalText) = TestHelper.MakeCoreTextboxWithText();
        int linesBefore = coreTextbox.textManager.LinesCount;

        coreTextbox.SetSelection(0, 0, 1, 5);
        coreTextbox.textActionManager.DeleteSelection();

        CheckUndoRedo(coreTextbox);

        string expected = TestHelper.GetLineText(originalText, coreTextbox, 1).Substring(5);
        bool res = coreTextbox.GetLineText(0) == expected &&
                   coreTextbox.textManager.LinesCount == linesBefore - 1;

        Debug.Assert(res);
    }
    [UITestMethod]
    public void DeleteSelectionEndLineCompletelySelected()
    {
        var (coreTextbox, originalText) = TestHelper.MakeCoreTextboxWithText();
        int linesBefore = coreTextbox.textManager.LinesCount;

        coreTextbox.SetSelection(1, 3, 2, coreTextbox.GetLineText(2).Length);
        coreTextbox.textActionManager.DeleteSelection();

        CheckUndoRedo(coreTextbox);

        var text = coreTextbox.GetLineText(1);
        string expected = TestHelper.GetLineText(originalText, coreTextbox, 1).Substring(0, 3);

        bool res = text.Equals(expected) &&
            coreTextbox.textManager.LinesCount == linesBefore - 1;
        Debug.Assert(res);
    }
    [UITestMethod]
    public void DeleteSelectionStartEndNotCompletelySelected()
    {
        var (coreTextbox, originalText) = TestHelper.MakeCoreTextboxWithText();
        int linesBefore = coreTextbox.textManager.LinesCount;

        coreTextbox.SetSelection(1, 2, 2, 4);
        coreTextbox.textActionManager.DeleteSelection();

        CheckUndoRedo(coreTextbox);

        string expected = TestHelper.GetLineText(originalText, coreTextbox, 1).Substring(0, 2) + TestHelper.GetLineText(originalText, coreTextbox, 2).Substring(4);
        bool res = coreTextbox.GetLineText(1) == expected &&
                   coreTextbox.textManager.LinesCount == linesBefore - 1;

        Debug.Assert(res);
    }
    [UITestMethod]
    public void DuplicateLine()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        int linesBefore = coreTextbox.textManager.LinesCount;

        coreTextbox.SetCursorPosition(4, 10);
        coreTextbox.DuplicateLine(3);

        CheckUndoRedo(coreTextbox);

        bool res = coreTextbox.GetLineText(3).Equals(coreTextbox.GetLineText(4)) &&
                   coreTextbox.textManager.LinesCount == linesBefore + 1 && coreTextbox.CursorPosition.LineNumber == 5 && coreTextbox.CursorPosition.CharacterPosition == 10;

        Debug.Assert(res);
    }
    [UITestMethod]
    public void LoadLinesEmptyArray()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();

        coreTextbox.LoadLines(Array.Empty<string>());
        coreTextbox.LoadLines(null);

        Debug.Assert(coreTextbox.textManager.totalLines.Count == 1);
    }
    [UITestMethod]
    public void LoadTextNullEmptyString()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();

        coreTextbox.LoadText(null);
        coreTextbox.LoadText("");

        Debug.Assert(coreTextbox.textManager.totalLines.Count == 1);
    }
    [UITestMethod]
    public void SetTextNullEmptyString()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();

        coreTextbox.SetText(null);
        coreTextbox.SetText("");

        Debug.Assert(coreTextbox.textManager.totalLines.Count == 1);
    }
    [UITestMethod]
    public void MoveSelectionWithTab()
    {
        var (coreTextbox, originalText) = TestHelper.MakeCoreTextboxWithText();

        coreTextbox.SetCursorPosition(0, 0);
        coreTextbox.SetSelection(0, 40); //whole text

        for (int i = 0; i < 3; i++)
            coreTextbox.tabSpaceManager.MoveTab();

        coreTextbox.Undo();
        coreTextbox.Undo();
        coreTextbox.Undo();

        Debug.Assert(coreTextbox.Text == originalText);
    }

    [UITestMethod]
    public void AddLinesAtIndex3()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");
        string textBefore = coreTextbox.GetText();

        coreTextbox.AddLines(3, new string[] { "Hello", "Baum", "Nudel", "Kuchen", "Wurst" });

        var res = CheckUndoRedo(coreTextbox);
        coreTextbox.Undo();

        Debug.Assert(res.undo && res.redo && textBefore.Equals(coreTextbox.GetText()));
    }
    [UITestMethod]
    public void AddLinesAtBeginning()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();

        coreTextbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");
        string textBefore = coreTextbox.GetText();

        coreTextbox.AddLines(0, new string[] { "Hello", "Baum", "Nudel", "Kuchen", "Wurst" });

        var res = CheckUndoRedo(coreTextbox);
        coreTextbox.Undo();

        Debug.Assert(res.undo && res.redo && textBefore.Equals(coreTextbox.GetText()));
    }
    [UITestMethod]
    public void AddLinesAtEnd()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();

        coreTextbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");
        string textBefore = coreTextbox.GetText();

        coreTextbox.AddLines(4, new string[] { "Hello", "Baum", "Nudel", "Kuchen", "Wurst" });

        var res = CheckUndoRedo(coreTextbox);
        coreTextbox.Undo();

        Debug.Assert(res.undo && res.redo && textBefore.Equals(coreTextbox.GetText()));
    }

    [UITestMethod]
    public void UndoGrouping()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();

        coreTextbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");
        string textBefore = coreTextbox.GetText();

        coreTextbox.BeginActionGroup();

        coreTextbox.DeleteLine(3);
        coreTextbox.AddLine(3, "New");
        coreTextbox.SetLineText(1, "Edit");
        coreTextbox.AddLines(4, new string[] { "Hello", "Baum", "Nudel", "Kuchen", "Wurst" });

        coreTextbox.EndActionGroup();

        var res = CheckUndoRedo(coreTextbox);
        coreTextbox.Undo();

        Debug.Assert(res.undo && res.redo && textBefore.Equals(coreTextbox.GetText()));
    }
    [UITestMethod]
    public void UndoGroupingExtended()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");
        string textBefore = coreTextbox.GetText();

        coreTextbox.BeginActionGroup();

        coreTextbox.DeleteLine(3);
        coreTextbox.DeleteLine(0);
        coreTextbox.AddLine(3, "New");
        coreTextbox.SetLineText(1, "Edit");

        for (int i = 0; i < 10; i++)
        {
            coreTextbox.AddLines(4, new string[] { "Hello", "Baum", "Nudel", "Kuchen", "Wurst" });
        }

        for (int i = 5; i < 20; i++)
        {
            coreTextbox.DeleteLine(i);
        }

        coreTextbox.EndActionGroup();

        var res = CheckUndoRedo(coreTextbox);
        coreTextbox.Undo();

        Debug.Assert(res.undo && res.redo && textBefore.Equals(coreTextbox.GetText()));
    }
}
