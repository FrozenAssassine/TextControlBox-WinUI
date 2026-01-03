using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using System;
using System.Diagnostics;
using System.Linq;
using TextControlBoxNS.Core;

namespace TextControlBox.Tests;

[TestClass]
public class UndoRedoTests
{
    [UITestMethod]
    public void SingleLineDeleteLine0()
    {
        //first line
        var coreTextbox = TestHelper.MakeCoreTextbox();
        var res1 = TestHelper.CheckUndoRedo(coreTextbox, () =>
        {
            coreTextbox.DeleteLine(0);
        }); 
        Assert.IsTrue(res1.undo);
        Assert.IsTrue(res1.redo);

        //any line
        var coreTextbox2 = TestHelper.MakeCoreTextbox();
        var res2 = TestHelper.CheckUndoRedo(coreTextbox2, () =>
        {
            coreTextbox2.DeleteLine(3);
        });
        Assert.IsTrue(res2.undo);
        Assert.IsTrue(res2.redo);

        //last line
        var coreTextbox3 = TestHelper.MakeCoreTextbox();
        var res3 = TestHelper.CheckUndoRedo(coreTextbox3, () =>
        {
            coreTextbox3.DeleteLine(coreTextbox3.textManager.LinesCount - 1);
        });
        Assert.IsTrue(res3.undo);
        Assert.IsTrue(res3.redo);
    }

    [UITestMethod]
    public void SingleLineReplaceLine0()
    {
        //first line
        var coreTextbox1 = TestHelper.MakeCoreTextbox();
        coreTextbox1.SelectLine(0);
        var res1 = TestHelper.CheckUndoRedo(coreTextbox1, () =>
        {
            coreTextbox1.textActionManager.AddCharacter("Hello World");
        });
        Assert.IsTrue(res1.undo);
        Assert.IsTrue(res1.redo);

        //middle line
        var coreTextbox2 = TestHelper.MakeCoreTextbox();
        coreTextbox2.SelectLine(3);
        var res2 = TestHelper.CheckUndoRedo(coreTextbox2, () =>
        {
            coreTextbox2.textActionManager.AddCharacter("Hello World");
        });
        Assert.IsTrue(res2.undo);
        Assert.IsTrue(res2.redo);

        //last line
        var coreTextbox3 = TestHelper.MakeCoreTextbox();
        coreTextbox3.SelectLine(coreTextbox3.textManager.LinesCount - 1);
        var res3 = TestHelper.CheckUndoRedo(coreTextbox3, () =>
        {
            coreTextbox3.textActionManager.AddCharacter("Hello World");
        });
        Assert.IsTrue(res3.undo);
        Assert.IsTrue(res3.redo);
    }

    [UITestMethod]
    public void SingleLineReplaceLineN()
    {
        var coreTextbox1 = TestHelper.MakeCoreTextbox();
        var replaceText = coreTextbox1.stringManager.CleanUpString("Hello World\nHello World");
        //first line
        var textbox = TestHelper.MakeCoreTextbox();

        coreTextbox1.SelectLine(0);
        var res1 = TestHelper.CheckUndoRedo(coreTextbox1, () =>
        {
            coreTextbox1.textActionManager.AddCharacter(replaceText);
        });
        Assert.IsTrue(res1.undo);
        Assert.IsTrue(res1.redo);

        //middle line
        var coreTextbox2 = TestHelper.MakeCoreTextbox();

        coreTextbox2.SelectLine(3);
        var res2 = TestHelper.CheckUndoRedo(coreTextbox2, () =>
        {
            coreTextbox2.textActionManager.AddCharacter(replaceText);
        }); 
        Assert.IsTrue(res2.undo);
        Assert.IsTrue(res2.redo);

        //last line
        var coreTextbox3 = TestHelper.MakeCoreTextbox();

        coreTextbox3.SelectLine(coreTextbox3.textManager.LinesCount - 1);
        var res3 = TestHelper.CheckUndoRedo(coreTextbox3, () =>
        {
            coreTextbox3.textActionManager.AddCharacter(replaceText);
        });
        Assert.IsTrue(res3.undo);
        Assert.IsTrue(res3.redo);
    }

    [UITestMethod]
    public void SelectallDeleteDelteReplaceReplaceAll()
    {
        //delete selection
        var coreTextbox1 = TestHelper.MakeCoreTextbox();
        coreTextbox1.SelectAll();
        var res1 = TestHelper.CheckUndoRedo(coreTextbox1, () =>
        {
            coreTextbox1.textActionManager.DeleteSelection();
        });
        Assert.IsTrue(res1.undo);
        Assert.IsTrue(res1.redo);

        //replace single line
        var coreTextbox2 = TestHelper.MakeCoreTextbox();
        coreTextbox2.SelectAll();
        var res2 = TestHelper.CheckUndoRedo(coreTextbox2, () =>
        {
            coreTextbox2.textActionManager.AddCharacter("Hello World");
        });
        Assert.IsTrue(res2.undo);
        Assert.IsTrue(res2.redo);

        //replace less lines than selected
        var coreTextbox3 = TestHelper.MakeCoreTextbox();
        coreTextbox3.SelectAll();
        var res3 = TestHelper.CheckUndoRedo(coreTextbox3, () =>
        {
            coreTextbox3.textActionManager.AddCharacter(coreTextbox3.stringManager.CleanUpString("Hello World\nHello World"));
        });
        Assert.IsTrue(res3.undo);
        Assert.IsTrue(res3.redo);

        //replace more lines than selected
        var coreTextbox4 = TestHelper.MakeCoreTextbox();

        coreTextbox4.SelectAll();
        var res4 = TestHelper.CheckUndoRedo(coreTextbox4, () =>
        {
            coreTextbox4.textActionManager.AddCharacter(coreTextbox4.stringManager.CleanUpString(string.Join("\n", TestHelper.MakeLines(100))));
        });
        Assert.IsTrue(res4.undo);
        Assert.IsTrue(res4.redo);
    }

    [UITestMethod]
    public void SelectLine1_5Delete()
    {
        //delete selection
        var coreTextbox1 = TestHelper.MakeCoreTextbox();
        coreTextbox1.SelectLines(1, 3);
        var res1 = TestHelper.CheckUndoRedo(coreTextbox1, () =>
        {
            coreTextbox1.textActionManager.DeleteSelection();
        });
        Assert.IsTrue(res1.undo);
        Assert.IsTrue(res1.redo);

        //replace single line
        var coreTextbox2 = TestHelper.MakeCoreTextbox();
        coreTextbox2.SelectLines(1, 3);
        var res2 = TestHelper.CheckUndoRedo(coreTextbox2, () =>
        {
            coreTextbox2.textActionManager.AddCharacter("Hello World");
        });
        Assert.IsTrue(res2.undo);
        Assert.IsTrue(res2.redo);

        //replace less lines than selected
        var coreTextbox3 = TestHelper.MakeCoreTextbox();
        coreTextbox3.SelectLines(1, 3);
        var res3 = TestHelper.CheckUndoRedo(coreTextbox3, () =>
        {
            coreTextbox3.textActionManager.AddCharacter(coreTextbox3.stringManager.CleanUpString("Hello World\nHello World"));
        });
        Assert.IsTrue(res3.undo);
        Assert.IsTrue(res3.redo);

        //replace more lines than selected
        var coreTextbox4 = TestHelper.MakeCoreTextbox();
        coreTextbox4.SelectLines(1, 3);
        var res4 = TestHelper.CheckUndoRedo(coreTextbox4, () =>
        {
            coreTextbox4.textActionManager.AddCharacter(
                coreTextbox4.stringManager.CleanUpString(string.Join("\n", TestHelper.MakeLines(100)))
            );
        });
        Assert.IsTrue(res4.undo);
        Assert.IsTrue(res4.redo);
    }

    [UITestMethod]
    public void AddNewLineSingleLineTripleSelectedEverythingSelected()
    {
        var coreTextbox1 = TestHelper.MakeCoreTextbox();
        coreTextbox1.SetSelection(1, 3);
        var res1 = TestHelper.CheckUndoRedo(coreTextbox1, () =>
        {
            coreTextbox1.textActionManager.AddNewLine();
        });
        Assert.IsTrue(res1.undo);
        Assert.IsTrue(res1.redo);

        var coreTextbox2 = TestHelper.MakeCoreTextbox();
        coreTextbox2.SelectLine(1);
        var res2 = TestHelper.CheckUndoRedo(coreTextbox2,() =>
        {
            coreTextbox2.textActionManager.AddNewLine();
        });
        Assert.IsTrue(res2.undo);
        Assert.IsTrue(res2.redo);

        var coreTextbox3 = TestHelper.MakeCoreTextbox();
        coreTextbox3.SelectAll();
        var res3 = TestHelper.CheckUndoRedo(coreTextbox3 , () =>
        {
            coreTextbox3.textActionManager.AddNewLine();
        });
        Assert.IsTrue(res3.undo);
        Assert.IsTrue(res3.redo);

        var coreTextbox4 = TestHelper.MakeCoreTextbox();
        coreTextbox4.SelectLines(1, 2);
        var res4 = TestHelper.CheckUndoRedo(coreTextbox4, () =>
        {
            coreTextbox4.textActionManager.AddNewLine();
        }); 
        Assert.IsTrue(res4.undo);
        Assert.IsTrue(res4.redo);

        var coreTextbox5 = TestHelper.MakeCoreTextbox();
        coreTextbox5.SetSelection(8, 16);
        var res5 = TestHelper.CheckUndoRedo(coreTextbox5, () =>
        {
            coreTextbox5.textActionManager.AddNewLine();
        });
        Assert.IsTrue(res5.undo);
        Assert.IsTrue(res5.redo);
    }

    [UITestMethod]
    public void DeleteAllLines()
    {
        var textbox = TestHelper.MakeCoreTextbox();

        int lines = textbox.NumberOfLines;
        var res1 = TestHelper.CheckUndoRedo(textbox, () =>
        {
            for (int i = 0; i < lines; i++)
            {
                textbox.DeleteLine(0);
            }
        }, lines);

        Assert.IsTrue(res1.undo);
        Assert.IsTrue(res1.redo);
    }

    [UITestMethod]
    public void AddCharacter_SingleChar_NoSelection()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.SetCursorPosition(0, 0);
        var res = TestHelper.CheckUndoRedo(coreTextbox, () =>
        {
            coreTextbox.textActionManager.AddCharacter("a");
        });
        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);
        Assert.AreEqual("a" + TestHelper.MakeLines(1).First(), coreTextbox.GetLineText(0));
    }

    [UITestMethod]
    public void AddCharacter_MultiChar_NoSelection()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.SetCursorPosition(0, 0);
        var res = TestHelper.CheckUndoRedo(coreTextbox, () =>
        {
            coreTextbox.textActionManager.AddCharacter("abc");
        });
        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);
        Assert.AreEqual("abc" + TestHelper.MakeLines(1).First(), coreTextbox.GetLineText(0));
    }

    [UITestMethod]
    public void AddCharacter_MultiLine_NoSelection()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.SetCursorPosition(0, 0);
        string textToAdd = coreTextbox.stringManager.CleanUpString("Line1\nLine2");
        var res = TestHelper.CheckUndoRedo(coreTextbox, () =>
        {
            coreTextbox.textActionManager.AddCharacter(textToAdd);
        });
        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);
        Assert.AreEqual("Line1", coreTextbox.GetLineText(0));
        Assert.AreEqual("Line2" + TestHelper.MakeLines(1).First(), coreTextbox.GetLineText(1));
    }

    [UITestMethod]
    public void AddCharacter_WithSelection_Replace()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.SetSelection(0, 0, 0, 5); // Select "Line "
        var res = TestHelper.CheckUndoRedo(coreTextbox, () =>
        {
            coreTextbox.textActionManager.AddCharacter("Replaced");
        });
        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);
        // Original: "Line 0 is cool right?"
        // Replaced: "Replaced0 is cool right?"
        Assert.StartsWith("Replaced0", coreTextbox.GetLineText(0));
    }

    [UITestMethod]
    public void AddNewLine_EndOfLine()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        int lineLength = coreTextbox.GetLineText(0).Length;
        coreTextbox.SetCursorPosition(0, lineLength);
        
        var res = TestHelper.CheckUndoRedo(coreTextbox, () =>
        {
            coreTextbox.textActionManager.AddNewLine();
        });
        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);
        Assert.AreEqual("", coreTextbox.GetLineText(1)); // New empty line
    }

    [UITestMethod]
    public void AddNewLine_MiddleOfLine()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.SetCursorPosition(0, 5); // "Line |0 is..."
        
        var res = TestHelper.CheckUndoRedo(coreTextbox, () =>
        {
            coreTextbox.textActionManager.AddNewLine();
        });
        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);
        Assert.AreEqual("Line ", coreTextbox.GetLineText(0));
        Assert.StartsWith("0 is cool", coreTextbox.GetLineText(1));
    }

    [UITestMethod]
    public void AddNewLine_StartOfLine()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.SetCursorPosition(0, 0);
        
        var res = TestHelper.CheckUndoRedo(coreTextbox, () =>
        {
            coreTextbox.textActionManager.AddNewLine();
        });
        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);
        Assert.AreEqual("", coreTextbox.GetLineText(0));
        Assert.StartsWith("Line 0", coreTextbox.GetLineText(1));
    }

    [UITestMethod]
    public void DeleteText_CharForward()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.SetCursorPosition(0, 0); // "Line..."
        
        var res = TestHelper.CheckUndoRedo(coreTextbox, () =>
        {
            coreTextbox.textActionManager.DeleteText(false);
        });
        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);
        Assert.StartsWith("ine 0", coreTextbox.GetLineText(0));
    }

    [UITestMethod]
    public void DeleteText_MergeLines()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        int lineLength = coreTextbox.GetLineText(0).Length;
        coreTextbox.SetCursorPosition(0, lineLength);
        
        var res = TestHelper.CheckUndoRedo(coreTextbox, () =>
        {
            coreTextbox.textActionManager.DeleteText(false);
        });
        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);
        // Line 0 and Line 1 should be merged
        string expected = "Line 0 is cool right?" + "Line 1 is cool right?";
        Assert.AreEqual(expected, coreTextbox.GetLineText(0));
    }

    [UITestMethod]
    public void DeleteText_WithSelection()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.SetSelection(0, 0, 0, 5); // "Line "
        
        var res = TestHelper.CheckUndoRedo(coreTextbox, () =>
        {
            coreTextbox.textActionManager.DeleteText(false);
        });
        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);
        Assert.StartsWith("0 is cool", coreTextbox.GetLineText(0));
    }

    [UITestMethod]
    public void RemoveText_CharBackward()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.SetCursorPosition(0, 1); // "L|ine..."
        
        var res = TestHelper.CheckUndoRedo(coreTextbox, () =>
        {
            coreTextbox.textActionManager.RemoveText(false);
        });
        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);
        Assert.StartsWith("ine 0", coreTextbox.GetLineText(0));
    }

    [UITestMethod]
    public void RemoveText_MergeLines()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.SetCursorPosition(1, 0); // Start of line 1
        
        var res = TestHelper.CheckUndoRedo(coreTextbox, () =>
        {
            coreTextbox.textActionManager.RemoveText(false);
        });
        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);
        // Line 0 and Line 1 should be merged
        string expected = "Line 0 is cool right?" + "Line 1 is cool right?";
        Assert.AreEqual(expected, coreTextbox.GetLineText(0));
    }

    [UITestMethod]
    public void RemoveText_WithSelection()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.SetSelection(0, 0, 0, 5); // "Line "
        
        var res = TestHelper.CheckUndoRedo(coreTextbox, () =>
        {
            coreTextbox.textActionManager.RemoveText(false);
        });
        Assert.IsTrue(res.undo);
        Assert.IsTrue(res.redo);
        Assert.StartsWith("0 is cool", coreTextbox.GetLineText(0));
    }

    [UITestMethod]
    public void RemoveText_StartOfDocument()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.SetCursorPosition(0, 0);
        string textBefore = coreTextbox.GetText();
        
        // Should do nothing
        coreTextbox.textActionManager.RemoveText(false);
        
        Assert.AreEqual(textBefore, coreTextbox.GetText());
        // Undo/Redo might not be recorded if nothing changed, or it might be recorded as a no-op.
        // CheckUndoRedo expects a change usually, but let's see if we can verify state.
        // If no action was recorded, Undo shouldn't change anything either.
        coreTextbox.Undo();
        Assert.AreEqual(textBefore, coreTextbox.GetText());
    }

    [UITestMethod]
    public void DeleteText_EndOfDocument()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.SetCursorPosition(coreTextbox.textManager.LinesCount - 1, coreTextbox.GetLineText(coreTextbox.textManager.LinesCount - 1).Length);
        string textBefore = coreTextbox.GetText();
        
        // Should do nothing
        coreTextbox.textActionManager.DeleteText(false);
        
        Assert.AreEqual(textBefore, coreTextbox.GetText());
        coreTextbox.Undo();
        Assert.AreEqual(textBefore, coreTextbox.GetText());
    }
}
