using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using System;
using System.Diagnostics;
using System.Linq;
using TextControlBoxNS.Core;
using TextControlBoxNS.Test;

namespace TextControlBox.Tests;

[TestClass]
public class UndoRedoTests
{
    private CoreTextControlBox MakeTextbox(int addNewLines = 100)
    {
        CoreTextControlBox textbox = new CoreTextControlBox();

        if (addNewLines == 0)
        {
            textbox.LoadLines([]);
        }
        textbox.LoadLines(Enumerable.Range(0, addNewLines).Select(x => "Line " + x + " is cool right?"));
        return textbox;
    }

    private (bool undo, bool redo) CheckUndoRedo(CoreTextControlBox textbox, Action action, int count = 1)
    {
        string textBeforeAction = textbox.GetText(); //store the original text

        action.Invoke();

        string textBeforeUndo = textbox.GetText(); //capture text after action

        for (int i = 0; i < count; i++)
            textbox.Undo(); // Undo action

        string textAfterUndo = textbox.GetText();

        //ensure undo reverts to original
        bool undoRes = textAfterUndo.Equals(textBeforeAction);

        for (int i = 0; i < count; i++)
            textbox.Redo();

        //ensure redo restores changes
        bool redoRes = textbox.GetText().Equals(textBeforeUndo);

        Debug.WriteLine($" (Undo: {undoRes} Redo:{redoRes})");
        //Debug.Assert(undoRes && redoRes);

        return (undoRes, redoRes);
    }

    [UITestMethod]
    public void SingleLineDeleteLine0()
    {
        //first line
        Debug.Write("Single Line (Delete Line 0)");
        var coreTextbox = MakeTextbox();
        var res1 = CheckUndoRedo(coreTextbox, () =>
        {
            coreTextbox.DeleteLine(0);
        });

        //any line
        Debug.Write("Single Line (Delete Line 3)");
        var coreTextbox2 = MakeTextbox();
        var res2 = CheckUndoRedo(coreTextbox2, () =>
        {
            coreTextbox2.DeleteLine(3);
        });

        //last line
        Debug.Write("Single Line (Delete Last Line)");
        var coreTextbox3 = MakeTextbox();
        var res3 = CheckUndoRedo(coreTextbox3, () =>
        {
            coreTextbox3.DeleteLine(coreTextbox3.textManager.LinesCount - 1);
        });

        Debug.Assert(res1.undo && res1.redo && res2.undo && res2.redo && res3.undo && res3.redo);
    }
    [UITestMethod]
    public void SingleLineReplaceLine0()
    {
        //first line
        Debug.Write("Single Line (Replace line 0 with 'Hello World'))");
        var coreTextbox1 = MakeTextbox();

        coreTextbox1.SelectLine(0);
        var res1 = CheckUndoRedo(coreTextbox1, () =>
        {
            coreTextbox1.textActionManager.AddCharacter("Hello World");
        });

        //middle line
        Debug.WriteLine("Single Line (Replace line 3 with 'Hello World'))");
        var coreTextbox2 = MakeTextbox();

        coreTextbox2.SelectLine(3);
        var res2 = CheckUndoRedo(coreTextbox2, () =>
        {
            coreTextbox2.textActionManager.AddCharacter("Hello World");
        });


        //last line
        Debug.WriteLine("Single Line (Replace LAST line with 'Hello World'))");
        var coreTextbox3 = MakeTextbox();

        coreTextbox3.SelectLine(coreTextbox3.textManager.LinesCount - 1);
        var res3 = CheckUndoRedo(coreTextbox3, () =>
        {
            coreTextbox3.textActionManager.AddCharacter("Hello World");
        });

        Debug.Assert(res1.undo && res1.redo && res2.undo && res2.redo && res3.undo && res3.redo);
    }

    [UITestMethod]
    public void SingleLineReplaceLineN()
    {
        var coreTextbox1 = MakeTextbox();
        var replaceText = coreTextbox1.stringManager.CleanUpString("Hello World\nHello World");
        //first line
        Debug.WriteLine("Single Line (Replace line 0 with 'Hello WorldHello World'))");
        var textbox = MakeTextbox();

        coreTextbox1.SelectLine(0);
        var res1 = CheckUndoRedo(coreTextbox1, () =>
        {
            coreTextbox1.textActionManager.AddCharacter(replaceText);
        });

        //middle line
        Debug.WriteLine("Single Line (Replace line 3 with 'Hello WorldHello World'))");
        var coreTextbox2 = MakeTextbox();

        coreTextbox2.SelectLine(3);
        var res2 = CheckUndoRedo(coreTextbox2, () =>
        {
            coreTextbox2.textActionManager.AddCharacter(replaceText);
        });

        //last line
        Debug.WriteLine("Single Line (Replace LAST line with 'Hello WorldHello World'))");
        var coreTextbox3 = MakeTextbox();

        coreTextbox3.SelectLine(coreTextbox3.textManager.LinesCount - 1);
        var res3 = CheckUndoRedo(coreTextbox3, () =>
        {
            coreTextbox3.textActionManager.AddCharacter(replaceText);
        });

        Debug.Assert(res1.undo && res1.redo && res2.undo && res2.redo && res3.undo && res3.redo);
    }

    [UITestMethod]
    public void SelectallDeleteDelteReplaceReplaceAll()
    {
        //delete selection
        Debug.WriteLine("Select All (Delete)");
        var coreTextbox1 = MakeTextbox();

        coreTextbox1.SelectAll();
        var res1 = CheckUndoRedo(coreTextbox1, () =>
        {
            coreTextbox1.textActionManager.DeleteSelection();
        });

        //replace single line
        Debug.WriteLine("Select All (Replace single line)");
        var coreTextbox2 = MakeTextbox();

        coreTextbox2.SelectAll();
        var res2 = CheckUndoRedo(coreTextbox2, () =>
        {
            coreTextbox2.textActionManager.AddCharacter("Hello World");
        });

        //replace less lines than selected
        Debug.WriteLine("Select All (Replace two lines line)");
        var coreTextbox3 = MakeTextbox();

        coreTextbox3.SelectAll();
        var res3 = CheckUndoRedo(coreTextbox3, () =>
        {
            coreTextbox3.textActionManager.AddCharacter(coreTextbox3.stringManager.CleanUpString("Hello World\nHello World"));
        });

        //replace more lines than selected
        Debug.WriteLine("Select All (Replace 100 line)");
        var coreTextbox4 = MakeTextbox();

        coreTextbox4.SelectAll();
        var res4 = CheckUndoRedo(coreTextbox4, () =>
        {
            coreTextbox4.textActionManager.AddCharacter(coreTextbox4.stringManager.CleanUpString(string.Join("\n", TestHelper.MakeLines(100))));
        });

        Debug.Assert(res1.undo && res1.redo && res2.undo && res2.redo && res3.undo && res3.redo && res4.undo && res4.redo);
    }

    [UITestMethod]
    public void SelectLine1_5Delete()
    {
        //delete selection
        Debug.WriteLine("Select Lines 1-4 (Delete)");
        var coreTextbox1 = MakeTextbox();

        coreTextbox1.SelectLines(1, 3);
        var res1 = CheckUndoRedo(coreTextbox1, () =>
        {
            coreTextbox1.textActionManager.DeleteSelection();
        });

        //replace single line
        Debug.WriteLine("Select Lines 1-4 (Replace single line)");
        var coreTextbox2 = MakeTextbox();

        coreTextbox2.SelectLines(1, 3);
        var res2 = CheckUndoRedo(coreTextbox2, () =>
        {
            coreTextbox2.textActionManager.AddCharacter("Hello World");
        });

        //replace less lines than selected
        Debug.WriteLine("Select Lines 1-4 (Replace two lines line)");
        var coreTextbox3 = MakeTextbox();

        coreTextbox3.SelectLines(1, 3);
        var res3 = CheckUndoRedo(coreTextbox3, () =>
        {
            coreTextbox3.textActionManager.AddCharacter(coreTextbox3.stringManager.CleanUpString("Hello World\nHello World"));
        });

        //replace more lines than selected
        Debug.WriteLine("Select Lines 1-4 (Replace 100 line)");
        var coreTextbox4 = MakeTextbox();

        coreTextbox4.SelectLines(1, 3);
        var res4 = CheckUndoRedo(coreTextbox4, () =>
        {
            coreTextbox4.textActionManager.AddCharacter(coreTextbox4.stringManager.CleanUpString(string.Join("\n", TestHelper.MakeLines(100))));
        });

        Debug.Assert(res1.undo && res1.redo && res2.undo && res2.redo && res3.undo && res3.redo && res4.undo && res4.redo);
    }

    [UITestMethod]
    public void AddNewLineSingleLineTripleSelectedEverythingSelected()
    {
        Debug.WriteLine("Add new line (in single line selected)");
        var coreTextbox1 = MakeTextbox();

        coreTextbox1.SetSelection(1, 3);
        var res1 = CheckUndoRedo(coreTextbox1, () =>
        {
            coreTextbox1.textActionManager.AddNewLine();
        });

        Debug.WriteLine("Add new line (line 1 triple click selected)");
        var coreTextbox2 = MakeTextbox();

        coreTextbox2.SelectLine(1);
        var res2 = CheckUndoRedo(coreTextbox2,() =>
        {
            coreTextbox2.textActionManager.AddNewLine();
        });

        Debug.WriteLine("Add new line (everything selected)");
        var coreTextbox3 = MakeTextbox();

        coreTextbox3.SelectAll();
        var res3 = CheckUndoRedo(coreTextbox3 , () =>
        {
            coreTextbox3.textActionManager.AddNewLine();
        });

        Debug.WriteLine("Add new line (two lines selected)");
        var coreTextbox4 = MakeTextbox();

        coreTextbox4.SelectLines(1, 2);
        var res4 = CheckUndoRedo(coreTextbox4, () =>
        {
            coreTextbox4.textActionManager.AddNewLine();
        });

        Debug.WriteLine("Add new line (in two lines selected)");
        var coreTextbox5 = MakeTextbox();

        coreTextbox5.SetSelection(8, 16);

        var res5 = CheckUndoRedo(coreTextbox5, () =>
        {
            coreTextbox5.textActionManager.AddNewLine();
        });
        Debug.Assert(res1.undo && res1.redo && res2.undo && res2.redo && res3.undo && res3.redo && res4.undo && res4.redo && res5.undo && res5.redo);
    }

    [UITestMethod]
    public void DeleteAllLines()
    {
        Debug.WriteLine("Delete all lines (one by one)");
        var textbox = MakeTextbox();

        int lines = textbox.NumberOfLines;
        var res1 = CheckUndoRedo(textbox, () =>
        {
            for (int i = 0; i < lines; i++)
            {
                textbox.DeleteLine(0);
            }
        }, lines);
        Debug.Assert(res1.undo && res1.redo);
    }
}
