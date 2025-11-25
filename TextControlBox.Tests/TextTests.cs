using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TextControlBoxNS;
using TextControlBoxNS.Core;
using TextControlBoxNS.Test;

namespace TextControlBox.Tests;

[TestClass]
public class TextTests
{
    private CoreTextControlBox CreateCore()
    {
        var core = new CoreTextControlBox();
        core.InitialiseOnStart();

        core.LoadLines(Enumerable.Range(0, 100).Select(x => "Line " + x + " is cool right?"));
        
        return core;
    }

    private (CoreTextControlBox coreTextBox, string text) CreateCoreWithText()
    {
        var core = CreateCore();
        return (core, core.GetText());
    }

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

        Debug.Write($" (Undo: {undoRes} Redo:{redoRes})");
        //Debug.Assert(undoRes && redoRes);

        return (undoRes, redoRes);
    }

    [UITestMethod]
    public void Test_1()
    {
        var coreTextbox = CreateCore();
        Debug.Write("Clear selection");

        Random r = new Random();
        coreTextbox.SetSelection(r.Next(0, 10), r.Next(10, 50));
        coreTextbox.ClearSelection();

        var sel = coreTextbox.selectionManager.currentTextSelection;
        bool res = sel.StartPosition.IsNull && sel.EndPosition.IsNull && !coreTextbox.selectionManager.HasSelection;

        Debug.Assert(res);
    }

    [UITestMethod]
    public void Test_2()
    {
        var coreTextbox = CreateCore();
        Debug.Write("Delete Line 5");

        int linesBefore = coreTextbox.NumberOfLines;
        coreTextbox.textActionManager.DeleteLine(4);

        CheckUndoRedo(coreTextbox);

        Debug.Assert(coreTextbox.NumberOfLines == linesBefore - 1);
    }

    [UITestMethod]
    public void Test_3()
    {
        var coreTextbox = CreateCore();
        Debug.Write("Add Line 5");

        int linesBefore = coreTextbox.NumberOfLines;
        var sel = coreTextbox.textActionManager.AddLine(3, "Hello World this is the text of line 3");

        CheckUndoRedo(coreTextbox);

        Debug.Assert(coreTextbox.NumberOfLines == linesBefore + 1 && coreTextbox.GetLineText(3).Equals("Hello World this is the text of line 3"));

    }

    [UITestMethod]
    public void Test_4()
    {
        var coreTextbox = CreateCore();
        Debug.Write("Add Character (single line text, no selection)");

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
    public void Test_5()
    {
        var coreTextbox = CreateCore();
        Debug.Write("Add Character (multi line text, no selection)");

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
    public void Test_6()
    {
        var coreTextbox = CreateCore();
        Debug.Write("Add Character (no text, no selection)");

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
    public void Test_7()
    {
        var coreTextbox = CreateCore();
        Debug.Write("Add Character (add single line, single line selected)");

        coreTextbox.SetSelection(5, 10);
        coreTextbox.textActionManager.AddCharacter("Hello World");

        CheckUndoRedo(coreTextbox);

        var cur = coreTextbox.cursorManager.currentCursorPosition;
        bool res =
            cur.LineNumber == 0 &&
            cur.CharacterPosition == 16;

        Debug.Assert(res);
    }

    [UITestMethod]
    public void Test_8()
    {
        var coreTextbox = CreateCore();
        Debug.Write("Add Character (add multiline line, single line selected)");

        coreTextbox.SetSelection(5, 10);
        coreTextbox.textActionManager.AddCharacter(coreTextbox.stringManager.CleanUpString("Line1\nLine2\nLine3"));

        CheckUndoRedo(coreTextbox);

        var cur = coreTextbox.cursorManager.currentCursorPosition;
        bool res =
            cur.LineNumber == 2 &&
            cur.CharacterPosition == 11;

        Debug.Assert(res);
    }
    [UITestMethod]
    public void Test_10()
    {
        var coreTextbox = CreateCore();
        Debug.Write("Add Character (add multi line, multi line selection, everything selected)");

        coreTextbox.SelectAll();
        string text = coreTextbox.stringManager.CleanUpString("Line1\nLine2\nLine3");
        coreTextbox.textActionManager.AddCharacter(text);

        CheckUndoRedo(coreTextbox);

        var cur = coreTextbox.cursorManager.currentCursorPosition;
        bool res =
            cur.LineNumber == 2 &&
            cur.CharacterPosition == 5 &&
            coreTextbox.GetText().Equals(text);

        Debug.Assert(res);
    }
    [UITestMethod]
    public void Test_11()
    {
        var (coreTextbox, originalText) = CreateCoreWithText();
        Debug.Write("Delete Selection (multi line selection, whole lines)");

        coreTextbox.SelectLines(1, 3);
        coreTextbox.textActionManager.DeleteSelection();

        CheckUndoRedo(coreTextbox);

        var lines = coreTextbox.textManager.totalLines;

        var cur = coreTextbox.cursorManager.currentCursorPosition;

        bool res =
            cur.LineNumber == 1 &&
            cur.CharacterPosition == 0 &&
            coreTextbox.GetLineText(1).Length == 0 && coreTextbox.GetLineText(0) == originalText.Split(coreTextbox.textManager.NewLineCharacter)[0];

        Debug.Assert(res);
    }
    [UITestMethod]
    public void Test_12()
    {
        var (coreTextbox, originalText) = CreateCoreWithText();

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
    public void Test_13()
    {
        var (coreTextbox, originalText) = CreateCoreWithText();
        Debug.Write("Delete Selection (single line selection, whole line)");

        coreTextbox.SelectLine(0);
        coreTextbox.textActionManager.DeleteSelection();
        CheckUndoRedo(coreTextbox);

        var cur = coreTextbox.cursorManager.currentCursorPosition;
        bool res =
            cur.LineNumber == 0 &&
            cur.CharacterPosition == 0 &&
            coreTextbox.GetLineText(0) == originalText.Split(coreTextbox.textManager.NewLineCharacter)[1];

        Debug.Assert(res);
    }
    [UITestMethod]
    public void Test_14()
    {
        var coreTextbox = CreateCore();
        Debug.Write("Delete Selection (lines selected completely (not whole text selected!))");
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
    public void Test_15()
    {
        var (coreTextbox, originalText) = CreateCoreWithText();
        Debug.Write("Delete Selection (only start line fully selected)");
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
    public void Test_16()
    {
        var (coreTextbox, originalText) = CreateCoreWithText();
        Debug.Write("Delete Selection (only end line fully selected)");
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
    public void Test_17()
    {
        var (coreTextbox, originalText) = CreateCoreWithText();
        Debug.Write("Delete Selection (neither start nor end line fully selected)");

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
    public void Test_18()
    {
        var coreTextbox = CreateCore();
        Debug.Write("Duplicate Line");
        int linesBefore = coreTextbox.textManager.LinesCount;

        coreTextbox.SetCursorPosition(4, 10);
        coreTextbox.DuplicateLine(3);

        CheckUndoRedo(coreTextbox);

        bool res = coreTextbox.GetLineText(3).Equals(coreTextbox.GetLineText(4)) &&
                   coreTextbox.textManager.LinesCount == linesBefore + 1 && coreTextbox.CursorPosition.LineNumber == 5 && coreTextbox.CursorPosition.CharacterPosition == 10;

        Debug.Assert(res);
    }
    [UITestMethod]
    public void Test_21()
    {
        var coreTextbox = CreateCore();
        Debug.WriteLine("Load Lines empty array");

        coreTextbox.LoadLines(Array.Empty<string>());
        coreTextbox.LoadLines(null);

        Debug.Assert(coreTextbox.textManager.totalLines.Count == 1);
    }
    [UITestMethod]
    public void Test_22()
    {
        var coreTextbox = CreateCore();
        Debug.WriteLine("Load Text null + empty string");

        coreTextbox.LoadText(null);
        coreTextbox.LoadText("");

        Debug.Assert(coreTextbox.textManager.totalLines.Count == 1);
    }
    [UITestMethod]
    public void Test_23()
    {
        var coreTextbox = CreateCore();
        Debug.WriteLine("Load Text null + empty string");

        coreTextbox.SetText(null);
        coreTextbox.SetText("");

        Debug.Assert(coreTextbox.textManager.totalLines.Count == 1);
    }
    [UITestMethod]
    public void Test_24()
    {
        var (coreTextbox, originalText) = CreateCoreWithText();
        Debug.WriteLine("Move selection with Tab");

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
    public void Test_30()
    {
        var coreTextbox = CreateCore();
        Debug.WriteLine("Surround Selection");


        coreTextbox.SetCursorPosition(0, 0);
        coreTextbox.SetSelection(0, 87); //whole text

        coreTextbox.SurroundSelectionWith("<div>", "</div>");
    }
    [UITestMethod]
    public void Test_31()
    {
        Debug.WriteLine("Add Lines at index 3");
        var coreTextbox = CreateCore();
        coreTextbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");
        string textBefore = coreTextbox.GetText();

        coreTextbox.AddLines(3, new string[] { "Hello", "Baum", "Nudel", "Kuchen", "Wurst" });

        var res = CheckUndoRedo(coreTextbox);
        coreTextbox.Undo();

        Debug.Assert(res.undo && res.redo && textBefore.Equals(coreTextbox.GetText()));
    }
    [UITestMethod]
    public void Test_32()
    {
        Debug.WriteLine("Add Lines at beginning");
        var coreTextbox = CreateCore();

        coreTextbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");
        string textBefore = coreTextbox.GetText();

        coreTextbox.AddLines(0, new string[] { "Hello", "Baum", "Nudel", "Kuchen", "Wurst" });

        var res = CheckUndoRedo(coreTextbox);
        coreTextbox.Undo();

        Debug.Assert(res.undo && res.redo && textBefore.Equals(coreTextbox.GetText()));
    }
    [UITestMethod]
    public void Test_33()
    {
        Debug.WriteLine("Add Lines at end");
        var coreTextbox = CreateCore();

        coreTextbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");
        string textBefore = coreTextbox.GetText();

        coreTextbox.AddLines(4, new string[] { "Hello", "Baum", "Nudel", "Kuchen", "Wurst" });

        var res = CheckUndoRedo(coreTextbox);
        coreTextbox.Undo();

        Debug.Assert(res.undo && res.redo && textBefore.Equals(coreTextbox.GetText()));
    }

    [UITestMethod]
    public void Test_34()
    {
        var coreTextbox = CreateCore();
        Debug.WriteLine("Undo Grouping");

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
    public void Test_35()
    {
        Debug.WriteLine("Undo Grouping Extended");

        var coreTextbox = CreateCore();
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
