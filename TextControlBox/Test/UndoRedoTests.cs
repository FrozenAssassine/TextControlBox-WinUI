using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using TextControlBoxNS.Core;
using Windows.Media.Protection.PlayReady;

namespace TextControlBoxNS.Test;

internal class UndoRedoTests : TestCase
{
    public CoreTextControlBox coreTextbox;

    public UndoRedoTests(string name, CoreTextControlBox coreTextbox)
    {
        this.coreTextbox = coreTextbox;
        coreTextbox.LineEnding = LineEnding.LF;
        this.name = name;
    }

    public override string name { get; set; }

    private string originalText;
    public override Func<bool>[] GetAllTests()
    {
        originalText = string.Join(coreTextbox.textManager.NewLineCharacter, TestHelper.MakeLines(10));

        return [
            this.Test_1,
            this.Test_2,
            this.Test_3,
            this.Test_4,
            this.Test_5,
            this.Test_6,
            this.Test_7,
            ];
    }

    public (bool undo, bool redo) CheckUndoRedo(Action action, int count = 1)
    {
        string textBeforeAction = coreTextbox.GetText(); //store the original text

        action.Invoke();

        string textBeforeUndo = coreTextbox.GetText(); //capture text after action

        for (int i = 0; i < count; i++)
            coreTextbox.Undo(); // Undo action

        string textAfterUndo = coreTextbox.GetText();

        //ensure undo reverts to original
        bool undoRes = textAfterUndo.Equals(textBeforeAction);

        for (int i = 0; i < count; i++)
            coreTextbox.Redo();

        //ensure redo restores changes
        bool redoRes = coreTextbox.GetText().Equals(textBeforeUndo);

        Debug.WriteLine($" (Undo: {undoRes} Redo:{redoRes})");
        //Debug.Assert(undoRes && redoRes);

        return (undoRes, redoRes);
    }

    private void ResetText()
    {
        coreTextbox.SetText(originalText);
    }

    public bool Test_1()
    {
        //first line
        Debug.Write("Single Line (Delete Line 0)");
        ResetText();
        var res1 = CheckUndoRedo(() =>
        {
            coreTextbox.DeleteLine(0);
        });

        //any line
        Debug.Write("Single Line (Delete Line 3)");
        ResetText();
        var res2 = CheckUndoRedo(() =>
        {
            coreTextbox.DeleteLine(3);
        });

        //last line
        Debug.Write("Single Line (Delete Last Line)");
        ResetText();
        var res3 = CheckUndoRedo(() =>
        {
            coreTextbox.DeleteLine(coreTextbox.textManager.LinesCount - 1);
        });

        return res1.undo && res1.redo && res2.undo && res2.redo && res3.undo && res3.redo;
    }

    public bool Test_2()
    {
        //first line
        Debug.Write("Single Line (Replace line 0 with 'Hello World'))");
        ResetText();

        coreTextbox.SelectLine(0);
        var res1 = CheckUndoRedo(() =>
        {
            coreTextbox.textActionManager.AddCharacter("Hello World");
        });

        //middle line
        Debug.WriteLine("Single Line (Replace line 3 with 'Hello World'))");
        ResetText();

        coreTextbox.SelectLine(3);
        var res2 = CheckUndoRedo(() =>
        {
            coreTextbox.textActionManager.AddCharacter("Hello World");
        });


        //last line
        Debug.WriteLine("Single Line (Replace LAST line with 'Hello World'))");
        ResetText();

        coreTextbox.SelectLine(coreTextbox.textManager.LinesCount - 1);
        var res3 = CheckUndoRedo(() =>
        {
            coreTextbox.textActionManager.AddCharacter("Hello World");
        });

        return res1.undo && res1.redo && res2.undo && res2.redo && res3.undo && res3.redo;
    }

    public bool Test_3()
    {
        var replaceText = coreTextbox.stringManager.CleanUpString("Hello World\nHello World");
        //first line
        Debug.WriteLine("Single Line (Replace line 0 with 'Hello WorldHello World'))");
        ResetText();

        coreTextbox.SelectLine(0);
        var res1 = CheckUndoRedo(() =>
        {
            coreTextbox.textActionManager.AddCharacter(replaceText);
        });

        //middle line
        Debug.WriteLine("Single Line (Replace line 3 with 'Hello WorldHello World'))");
        ResetText();

        coreTextbox.SelectLine(3);
        var res2 = CheckUndoRedo(() =>
        {
            coreTextbox.textActionManager.AddCharacter(replaceText);
        });

        //last line
        Debug.WriteLine("Single Line (Replace LAST line with 'Hello WorldHello World'))");
        ResetText();

        coreTextbox.SelectLine(coreTextbox.textManager.LinesCount - 1);
        var res3 = CheckUndoRedo(() =>
        {
            coreTextbox.textActionManager.AddCharacter(replaceText);
        });

        return res1.undo && res1.redo && res2.undo && res2.redo && res3.undo && res3.redo;
    }

    public bool Test_4()
    {
        //delete selection
        Debug.WriteLine("Select All (Delete)");
        ResetText();

        coreTextbox.SelectAll();
        var res1 = CheckUndoRedo(() =>
        {
            coreTextbox.textActionManager.DeleteSelection();
        });

        //replace single line
        Debug.WriteLine("Select All (Replace single line)");
        ResetText();

        coreTextbox.SelectAll();
        var res2 = CheckUndoRedo(() =>
        {
            coreTextbox.textActionManager.AddCharacter("Hello World");
        });

        //replace less lines than selected
        Debug.WriteLine("Select All (Replace two lines line)");
        ResetText();

        coreTextbox.SelectAll();
        var res3 = CheckUndoRedo(() =>
        {
            coreTextbox.textActionManager.AddCharacter(coreTextbox.stringManager.CleanUpString("Hello World\nHello World"));
        });

        //replace more lines than selected
        Debug.WriteLine("Select All (Replace 100 line)");
        ResetText();

        coreTextbox.SelectAll();
        var res4 = CheckUndoRedo(() =>
        {
            coreTextbox.textActionManager.AddCharacter(coreTextbox.stringManager.CleanUpString(string.Join("\n", TestHelper.MakeLines(100))));
        });

        return res1.undo && res1.redo && res2.undo && res2.redo && res3.undo && res3.redo && res4.undo && res4.redo;
    }

    public bool Test_5()
    {
        //delete selection
        Debug.WriteLine("Select Lines 1-4 (Delete)");
        ResetText();

        coreTextbox.SelectLines(1, 3);
        var res1 = CheckUndoRedo(() =>
        {
            coreTextbox.textActionManager.DeleteSelection();
        });

        //replace single line
        Debug.WriteLine("Select Lines 1-4 (Replace single line)");
        ResetText();

        coreTextbox.SelectLines(1,3);
        var res2 = CheckUndoRedo(() =>
        {
            coreTextbox.textActionManager.AddCharacter("Hello World");
        });

        //replace less lines than selected
        Debug.WriteLine("Select Lines 1-4 (Replace two lines line)");
        ResetText();

        coreTextbox.SelectLines(1, 3);
        var res3 = CheckUndoRedo(() =>
        {
            coreTextbox.textActionManager.AddCharacter(coreTextbox.stringManager.CleanUpString("Hello World\nHello World"));
        });

        //replace more lines than selected
        Debug.WriteLine("Select Lines 1-4 (Replace 100 line)");
        ResetText();

        coreTextbox.SelectLines(1, 3);
        var res4 = CheckUndoRedo(() =>
        {
            coreTextbox.textActionManager.AddCharacter(coreTextbox.stringManager.CleanUpString(string.Join("\n", TestHelper.MakeLines(100))));
        });

        return res1.undo && res1.redo && res2.undo && res2.redo && res3.undo && res3.redo && res4.undo && res4.redo;
    }

    public bool Test_6()
    {
        Debug.WriteLine("Add new line (in single line selected)");
        ResetText();

        coreTextbox.SetSelection(1, 3);
        var res1 = CheckUndoRedo(() =>
        {
            coreTextbox.textActionManager.AddNewLine();
        });

        Debug.WriteLine("Add new line (line 1 triple click selected)");
        ResetText();

        coreTextbox.SelectLine(1);
        var res2 = CheckUndoRedo(() =>
        {
            coreTextbox.textActionManager.AddNewLine();
        });

        Debug.WriteLine("Add new line (everything selected)");
        ResetText();

        coreTextbox.SelectAll();
        var res3 = CheckUndoRedo(() =>
        {
            coreTextbox.textActionManager.AddNewLine();
        });

        Debug.WriteLine("Add new line (two lines selected)");
        ResetText();

        coreTextbox.SelectLines(1,2);
        var res4 = CheckUndoRedo(() =>
        {
            coreTextbox.textActionManager.AddNewLine();
        });

        Debug.WriteLine("Add new line (in two lines selected)");
        ResetText();

        coreTextbox.SetSelection(8, 16);

        var res5 = CheckUndoRedo(() =>
        {
            coreTextbox.textActionManager.AddNewLine();
        });
        return res1.undo && res1.redo && res2.undo && res2.redo && res3.undo && res3.redo && res4.undo && res4.redo && res5.undo && res5.redo;
    }

    public bool Test_7()
    {
        Debug.WriteLine("Delete all lines (one by one)");
        ResetText();

        int lines = coreTextbox.textManager.LinesCount;
        var res1 = CheckUndoRedo(() =>
        {
            for (int i = 0; i < lines; i++)
            {
                coreTextbox.DeleteLine(0);
            }
        }, lines);
        return res1.undo && res1.redo;
    }
}
