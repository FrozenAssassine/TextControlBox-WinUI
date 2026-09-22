using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using System;
using System.Diagnostics;
using System.Linq;
using TextControlBoxNS;
using TextControlBoxNS.Core;
using TextControlBoxNS.Models;
using TextControlBoxNS.Models.Enums;

namespace TextControlBox.Tests;

[TestClass]
public class UndoRedoTests
{
    [UITestMethod]
    public void SingleLineDeleteLine0()
    {
        // First line: line 0
        var coreTextbox = TestHelper.MakeCoreTextbox();
        int initialCount = coreTextbox.textManager.LinesCount;
        string expectedLine1AsLine0 = coreTextbox.GetLineText(1);
        string originalLine0 = coreTextbox.GetLineText(0);

        coreTextbox.DeleteLine(0);

        Assert.AreEqual(initialCount - 1, coreTextbox.textManager.LinesCount);
        Assert.AreEqual(expectedLine1AsLine0, coreTextbox.GetLineText(0));

        // Undo restores initial lines and original line 0
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual(initialCount, coreTextbox.textManager.LinesCount);
        Assert.AreEqual(originalLine0, coreTextbox.GetLineText(0));

        // Redo re-deletes line 0
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual(initialCount - 1, coreTextbox.textManager.LinesCount);
        Assert.AreEqual(expectedLine1AsLine0, coreTextbox.GetLineText(0));

        // Middle line: line 3
        var coreTextbox2 = TestHelper.MakeCoreTextbox();
        string originalLine3 = coreTextbox2.GetLineText(3);
        string expectedLine4AsLine3 = coreTextbox2.GetLineText(4);

        coreTextbox2.DeleteLine(3);

        Assert.AreEqual(initialCount - 1, coreTextbox2.textManager.LinesCount);
        Assert.AreEqual(expectedLine4AsLine3, coreTextbox2.GetLineText(3));

        coreTextbox2.Undo();
        Assert.AreEqual(initialCount, coreTextbox2.textManager.LinesCount);
        Assert.AreEqual(originalLine3, coreTextbox2.GetLineText(3));

        coreTextbox2.Redo();
        Assert.AreEqual(initialCount - 1, coreTextbox2.textManager.LinesCount);
        Assert.AreEqual(expectedLine4AsLine3, coreTextbox2.GetLineText(3));

        // Last line: line 99
        var coreTextbox3 = TestHelper.MakeCoreTextbox();
        int lastIndex = coreTextbox3.textManager.LinesCount - 1;
        string originalLastLine = coreTextbox3.GetLineText(lastIndex);
        string expectedNewLast = coreTextbox3.GetLineText(lastIndex - 1);

        coreTextbox3.DeleteLine(lastIndex);

        Assert.AreEqual(initialCount - 1, coreTextbox3.textManager.LinesCount);
        Assert.AreEqual(expectedNewLast, coreTextbox3.GetLineText(coreTextbox3.textManager.LinesCount - 1));

        coreTextbox3.Undo();
        Assert.AreEqual(initialCount, coreTextbox3.textManager.LinesCount);
        Assert.AreEqual(originalLastLine, coreTextbox3.GetLineText(lastIndex));

        coreTextbox3.Redo();
        Assert.AreEqual(initialCount - 1, coreTextbox3.textManager.LinesCount);
        Assert.AreEqual(expectedNewLast, coreTextbox3.GetLineText(coreTextbox3.textManager.LinesCount - 1));
    }

    [UITestMethod]
    public void SingleLineReplaceLine0()
    {
        // First line: line 0
        var coreTextbox1 = TestHelper.MakeCoreTextbox();
        string origLine0 = coreTextbox1.GetLineText(0);
        int initialCount = coreTextbox1.textManager.LinesCount;

        coreTextbox1.SelectLine(0);
        coreTextbox1.textActionManager.AddCharacter("Hello World");

        Assert.AreEqual(initialCount, coreTextbox1.textManager.LinesCount);
        Assert.AreEqual("Hello World", coreTextbox1.GetLineText(0));

        coreTextbox1.Undo();
        Assert.AreEqual(initialCount, coreTextbox1.textManager.LinesCount);
        Assert.AreEqual(origLine0, coreTextbox1.GetLineText(0));

        coreTextbox1.Redo();
        Assert.AreEqual(initialCount, coreTextbox1.textManager.LinesCount);
        Assert.AreEqual("Hello World", coreTextbox1.GetLineText(0));

        // Middle line: line 3
        var coreTextbox2 = TestHelper.MakeCoreTextbox();
        string origLine3 = coreTextbox2.GetLineText(3);

        coreTextbox2.SelectLine(3);
        coreTextbox2.textActionManager.AddCharacter("Hello World");

        Assert.AreEqual(initialCount, coreTextbox2.textManager.LinesCount);
        Assert.AreEqual("Hello World", coreTextbox2.GetLineText(3));

        coreTextbox2.Undo();
        Assert.AreEqual(initialCount, coreTextbox2.textManager.LinesCount);
        Assert.AreEqual(origLine3, coreTextbox2.GetLineText(3));

        coreTextbox2.Redo();
        Assert.AreEqual(initialCount, coreTextbox2.textManager.LinesCount);
        Assert.AreEqual("Hello World", coreTextbox2.GetLineText(3));

        // Last line
        var coreTextbox3 = TestHelper.MakeCoreTextbox();
        int lastLine = coreTextbox3.textManager.LinesCount - 1;
        string origLastLine = coreTextbox3.GetLineText(lastLine);

        coreTextbox3.SelectLine(lastLine);
        coreTextbox3.textActionManager.AddCharacter("Hello World");

        Assert.AreEqual(initialCount, coreTextbox3.textManager.LinesCount);
        Assert.AreEqual("Hello World", coreTextbox3.GetLineText(lastLine));

        coreTextbox3.Undo();
        Assert.AreEqual(initialCount, coreTextbox3.textManager.LinesCount);
        Assert.AreEqual(origLastLine, coreTextbox3.GetLineText(lastLine));

        coreTextbox3.Redo();
        Assert.AreEqual(initialCount, coreTextbox3.textManager.LinesCount);
        Assert.AreEqual("Hello World", coreTextbox3.GetLineText(lastLine));
    }

    [UITestMethod]
    public void SingleLineReplaceLineN()
    {
        var coreTextbox1 = TestHelper.MakeCoreTextbox();
        var replaceText = coreTextbox1.stringManager.CleanUpString("Hello World\nHello World");
        string origLine0 = coreTextbox1.GetLineText(0);
        string origLine1 = coreTextbox1.GetLineText(1);
        int initialCount = coreTextbox1.textManager.LinesCount;

        // Replace line 0 with 2 lines
        coreTextbox1.SelectLine(0);
        coreTextbox1.textActionManager.AddCharacter(replaceText);

        Assert.AreEqual(initialCount + 1, coreTextbox1.textManager.LinesCount);
        Assert.AreEqual("Hello World", coreTextbox1.GetLineText(0));
        Assert.AreEqual("Hello World", coreTextbox1.GetLineText(1));
        Assert.AreEqual(origLine1, coreTextbox1.GetLineText(2));

        coreTextbox1.Undo();
        Assert.AreEqual(initialCount, coreTextbox1.textManager.LinesCount);
        Assert.AreEqual(origLine0, coreTextbox1.GetLineText(0));
        Assert.AreEqual(origLine1, coreTextbox1.GetLineText(1));

        coreTextbox1.Redo();
        Assert.AreEqual(initialCount + 1, coreTextbox1.textManager.LinesCount);
        Assert.AreEqual("Hello World", coreTextbox1.GetLineText(0));
        Assert.AreEqual("Hello World", coreTextbox1.GetLineText(1));

        // Middle line: line 3 replaced with 2 lines
        var coreTextbox2 = TestHelper.MakeCoreTextbox();
        string origLine3 = coreTextbox2.GetLineText(3);
        string origLine4 = coreTextbox2.GetLineText(4);

        coreTextbox2.SelectLine(3);
        coreTextbox2.textActionManager.AddCharacter(replaceText);

        Assert.AreEqual(initialCount + 1, coreTextbox2.textManager.LinesCount);
        Assert.AreEqual("Hello World", coreTextbox2.GetLineText(3));
        Assert.AreEqual("Hello World", coreTextbox2.GetLineText(4));
        Assert.AreEqual(origLine4, coreTextbox2.GetLineText(5));

        coreTextbox2.Undo();
        Assert.AreEqual(initialCount, coreTextbox2.textManager.LinesCount);
        Assert.AreEqual(origLine3, coreTextbox2.GetLineText(3));
        Assert.AreEqual(origLine4, coreTextbox2.GetLineText(4));

        coreTextbox2.Redo();
        Assert.AreEqual(initialCount + 1, coreTextbox2.textManager.LinesCount);
        Assert.AreEqual("Hello World", coreTextbox2.GetLineText(3));

        // Last line replaced with 2 lines
        var coreTextbox3 = TestHelper.MakeCoreTextbox();
        int lastLine = coreTextbox3.textManager.LinesCount - 1;
        string origLastLine = coreTextbox3.GetLineText(lastLine);

        coreTextbox3.SelectLine(lastLine);
        coreTextbox3.textActionManager.AddCharacter(replaceText);

        Assert.AreEqual(initialCount + 1, coreTextbox3.textManager.LinesCount);
        Assert.AreEqual("Hello World", coreTextbox3.GetLineText(lastLine));
        Assert.AreEqual("Hello World", coreTextbox3.GetLineText(lastLine + 1));

        coreTextbox3.Undo();
        Assert.AreEqual(initialCount, coreTextbox3.textManager.LinesCount);
        Assert.AreEqual(origLastLine, coreTextbox3.GetLineText(lastLine));

        coreTextbox3.Redo();
        Assert.AreEqual(initialCount + 1, coreTextbox3.textManager.LinesCount);
        Assert.AreEqual("Hello World", coreTextbox3.GetLineText(lastLine));
    }

    [UITestMethod]
    public void SelectallDeleteDelteReplaceReplaceAll()
    {
        // Delete entire document selection
        var coreTextbox1 = TestHelper.MakeCoreTextbox();
        string originalText = coreTextbox1.GetText();
        coreTextbox1.SelectAll();

        coreTextbox1.textActionManager.DeleteSelection();

        Assert.AreEqual(1, coreTextbox1.textManager.LinesCount);
        Assert.AreEqual("", coreTextbox1.GetText());

        coreTextbox1.Undo();
        Assert.AreEqual(100, coreTextbox1.textManager.LinesCount);
        Assert.AreEqual(originalText, coreTextbox1.GetText());
        Assert.IsTrue(coreTextbox1.selectionManager.HasSelection);

        coreTextbox1.Redo();
        Assert.AreEqual(1, coreTextbox1.textManager.LinesCount);
        Assert.AreEqual("", coreTextbox1.GetText());

        // Replace entire document with single line
        var coreTextbox2 = TestHelper.MakeCoreTextbox();
        coreTextbox2.SelectAll();

        coreTextbox2.textActionManager.AddCharacter("Hello World");

        Assert.AreEqual(1, coreTextbox2.textManager.LinesCount);
        Assert.AreEqual("Hello World", coreTextbox2.GetText());

        coreTextbox2.Undo();
        Assert.AreEqual(100, coreTextbox2.textManager.LinesCount);
        Assert.AreEqual(originalText, coreTextbox2.GetText());

        coreTextbox2.Redo();
        Assert.AreEqual(1, coreTextbox2.textManager.LinesCount);
        Assert.AreEqual("Hello World", coreTextbox2.GetText());

        // Replace entire document with fewer lines (2 lines)
        var coreTextbox3 = TestHelper.MakeCoreTextbox();
        coreTextbox3.SelectAll();
        string twoLines = coreTextbox3.stringManager.CleanUpString("Hello World\nHello World");

        coreTextbox3.textActionManager.AddCharacter(twoLines);

        Assert.AreEqual(2, coreTextbox3.textManager.LinesCount);
        Assert.AreEqual(twoLines, coreTextbox3.GetText());

        coreTextbox3.Undo();
        Assert.AreEqual(100, coreTextbox3.textManager.LinesCount);
        Assert.AreEqual(originalText, coreTextbox3.GetText());

        coreTextbox3.Redo();
        Assert.AreEqual(2, coreTextbox3.textManager.LinesCount);
        Assert.AreEqual(twoLines, coreTextbox3.GetText());

        // Replace entire document with 100 new lines
        var coreTextbox4 = TestHelper.MakeCoreTextbox();
        coreTextbox4.SelectAll();
        string hundredLines = coreTextbox4.stringManager.CleanUpString(string.Join("\n", TestHelper.MakeLines(100)));

        coreTextbox4.textActionManager.AddCharacter(hundredLines);

        Assert.AreEqual(100, coreTextbox4.textManager.LinesCount);
        Assert.AreEqual(hundredLines, coreTextbox4.GetText());

        coreTextbox4.Undo();
        Assert.AreEqual(100, coreTextbox4.textManager.LinesCount);
        Assert.AreEqual(originalText, coreTextbox4.GetText());

        coreTextbox4.Redo();
        Assert.AreEqual(100, coreTextbox4.textManager.LinesCount);
        Assert.AreEqual(hundredLines, coreTextbox4.GetText());
    }

    [UITestMethod]
    public void SelectLine1_5Delete()
    {
        // SelectLines(1, 3) selects lines 1, 2, 3 content (from line 1 col 0 to line 3 end)
        var coreTextbox1 = TestHelper.MakeCoreTextbox();
        string origLine0 = coreTextbox1.GetLineText(0);
        string origLine4 = coreTextbox1.GetLineText(4);
        string origFullText = coreTextbox1.GetText();

        coreTextbox1.SelectLines(1, 3);
        coreTextbox1.textActionManager.DeleteSelection();

        // Lines 2 and 3 are removed, line 1 is cleared to "" -> 98 total lines
        Assert.AreEqual(98, coreTextbox1.textManager.LinesCount);
        Assert.AreEqual(origLine0, coreTextbox1.GetLineText(0));
        Assert.AreEqual("", coreTextbox1.GetLineText(1));
        Assert.AreEqual(origLine4, coreTextbox1.GetLineText(2));

        coreTextbox1.Undo();
        Assert.AreEqual(100, coreTextbox1.textManager.LinesCount);
        Assert.AreEqual(origFullText, coreTextbox1.GetText());
        Assert.IsTrue(coreTextbox1.selectionManager.HasSelection);

        coreTextbox1.Redo();
        Assert.AreEqual(98, coreTextbox1.textManager.LinesCount);
        Assert.AreEqual("", coreTextbox1.GetLineText(1));

        // Replace with single line: Line 1 becomes "Hello World", lines 2 and 3 removed -> 98 lines
        var coreTextbox2 = TestHelper.MakeCoreTextbox();
        coreTextbox2.SelectLines(1, 3);
        coreTextbox2.textActionManager.AddCharacter("Hello World");

        Assert.AreEqual(98, coreTextbox2.textManager.LinesCount);
        Assert.AreEqual("Hello World", coreTextbox2.GetLineText(1));
        Assert.AreEqual(origLine4, coreTextbox2.GetLineText(2));

        coreTextbox2.Undo();
        Assert.AreEqual(100, coreTextbox2.textManager.LinesCount);
        Assert.AreEqual(origFullText, coreTextbox2.GetText());

        coreTextbox2.Redo();
        Assert.AreEqual(98, coreTextbox2.textManager.LinesCount);
        Assert.AreEqual("Hello World", coreTextbox2.GetLineText(1));

        // Replace with 2 lines -> 99 lines
        var coreTextbox3 = TestHelper.MakeCoreTextbox();
        coreTextbox3.SelectLines(1, 3);
        coreTextbox3.textActionManager.AddCharacter(coreTextbox3.stringManager.CleanUpString("Hello World\nHello World"));

        Assert.AreEqual(99, coreTextbox3.textManager.LinesCount);
        Assert.AreEqual("Hello World", coreTextbox3.GetLineText(1));
        Assert.AreEqual("Hello World", coreTextbox3.GetLineText(2));
        Assert.AreEqual(origLine4, coreTextbox3.GetLineText(3));

        coreTextbox3.Undo();
        Assert.AreEqual(100, coreTextbox3.textManager.LinesCount);
        Assert.AreEqual(origFullText, coreTextbox3.GetText());

        coreTextbox3.Redo();
        Assert.AreEqual(99, coreTextbox3.textManager.LinesCount);
        Assert.AreEqual("Hello World", coreTextbox3.GetLineText(1));
        Assert.AreEqual("Hello World", coreTextbox3.GetLineText(2));
    }

    [UITestMethod]
    public void AddNewLineSingleLineTripleSelectedEverythingSelected()
    {
        // 1. Partial line selection: SetSelection(0, 1, 0, 3) -> "in" in "Line 0..."
        var coreTextbox1 = TestHelper.MakeCoreTextbox();
        string origLine0 = coreTextbox1.GetLineText(0);
        coreTextbox1.SetSelection(0, 1, 0, 3);

        coreTextbox1.textActionManager.AddNewLine();

        Assert.AreEqual(101, coreTextbox1.textManager.LinesCount);
        Assert.AreEqual("L", coreTextbox1.GetLineText(0));
        Assert.AreEqual(origLine0.Substring(3), coreTextbox1.GetLineText(1));

        coreTextbox1.Undo();
        Assert.AreEqual(100, coreTextbox1.textManager.LinesCount);
        Assert.AreEqual(origLine0, coreTextbox1.GetLineText(0));

        coreTextbox1.Redo();
        Assert.AreEqual(101, coreTextbox1.textManager.LinesCount);
        Assert.AreEqual("L", coreTextbox1.GetLineText(0));

        // 2. Whole line triple-click selection (Form 1): SelectLine(1)
        var coreTextbox2 = TestHelper.MakeCoreTextbox();
        string origLine1 = coreTextbox2.GetLineText(1);
        coreTextbox2.SelectLine(1);

        coreTextbox2.textActionManager.AddNewLine();

        Assert.AreEqual(101, coreTextbox2.textManager.LinesCount);
        Assert.AreEqual("", coreTextbox2.GetLineText(1));

        coreTextbox2.Undo();
        Assert.AreEqual(100, coreTextbox2.textManager.LinesCount);
        Assert.AreEqual(origLine1, coreTextbox2.GetLineText(1));

        coreTextbox2.Redo();
        Assert.AreEqual(101, coreTextbox2.textManager.LinesCount);
        Assert.AreEqual("", coreTextbox2.GetLineText(1));

        // 3. Whole text selection: SelectAll() -> AddNewLine() leaves 2 empty lines
        var coreTextbox3 = TestHelper.MakeCoreTextbox();
        string fullText = coreTextbox3.GetText();
        coreTextbox3.SelectAll();

        coreTextbox3.textActionManager.AddNewLine();

        Assert.AreEqual(2, coreTextbox3.textManager.LinesCount);
        Assert.AreEqual("", coreTextbox3.GetLineText(0));
        Assert.AreEqual("", coreTextbox3.GetLineText(1));

        coreTextbox3.Undo();
        Assert.AreEqual(100, coreTextbox3.textManager.LinesCount);
        Assert.AreEqual(fullText, coreTextbox3.GetText());

        coreTextbox3.Redo();
        Assert.AreEqual(2, coreTextbox3.textManager.LinesCount);
        Assert.AreEqual("", coreTextbox3.GetLineText(0));

        // 4. Multi-line selection: SelectLines(1, 2) (content of lines 1 and 2)
        var coreTextbox4 = TestHelper.MakeCoreTextbox();
        string origLine2 = coreTextbox4.GetLineText(2);
        coreTextbox4.SelectLines(1, 2);

        coreTextbox4.textActionManager.AddNewLine();

        Assert.AreEqual(100, coreTextbox4.textManager.LinesCount);
        Assert.AreEqual("", coreTextbox4.GetLineText(1));
        Assert.AreEqual("", coreTextbox4.GetLineText(2));

        coreTextbox4.Undo();
        Assert.AreEqual(100, coreTextbox4.textManager.LinesCount);
        Assert.AreEqual(origLine1, coreTextbox4.GetLineText(1));
        Assert.AreEqual(origLine2, coreTextbox4.GetLineText(2));

        coreTextbox4.Redo();
        Assert.AreEqual(100, coreTextbox4.textManager.LinesCount);
        Assert.AreEqual("", coreTextbox4.GetLineText(1));
    }

    [UITestMethod]
    public void DeleteAllLines()
    {
        var textbox = TestHelper.MakeCoreTextbox();
        string originalText = textbox.GetText();
        int lines = textbox.NumberOfLines;

        for (int i = 0; i < lines; i++)
        {
            textbox.DeleteLine(0);
        }

        // Deleting all lines should leave exactly 1 line with empty content
        Assert.AreEqual(1, textbox.NumberOfLines);
        Assert.AreEqual("", textbox.GetLineText(0));

        // Undo all 100 line deletions
        for (int i = 0; i < lines; i++)
        {
            Assert.IsTrue(textbox.undoRedo.CanUndo);
            textbox.Undo();
        }

        Assert.AreEqual(lines, textbox.NumberOfLines);
        Assert.AreEqual(originalText, textbox.GetText());

        // Redo all 100 deletions
        for (int i = 0; i < lines; i++)
        {
            Assert.IsTrue(textbox.undoRedo.CanRedo);
            textbox.Redo();
        }

        Assert.AreEqual(1, textbox.NumberOfLines);
        Assert.AreEqual("", textbox.GetLineText(0));
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

    [UITestMethod]
    public void UndoBatching_ConsecutiveWordCharacters_SingleUndo()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.SetCursorPosition(0, 0);
        string textBefore = coreTextbox.GetText();

        foreach (char c in "Hello")
        {
            coreTextbox.textActionManager.AddCharacter(c.ToString());
        }

        Assert.StartsWith("Hello", coreTextbox.GetLineText(0));

        // A single Undo should revert the whole word
        coreTextbox.Undo();
        Assert.AreEqual(textBefore, coreTextbox.GetText());

        // A single Redo should restore the whole word
        coreTextbox.Redo();
        Assert.StartsWith("Hello", coreTextbox.GetLineText(0));
    }

    [UITestMethod]
    public void UndoBatching_WordAndSpace_SeparateUndo()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.SetCursorPosition(0, 0);
        string textBefore = coreTextbox.GetText();

        foreach (char c in "Hello World")
        {
            coreTextbox.textActionManager.AddCharacter(c.ToString());
        }

        Assert.AreEqual("Hello World" + textBefore, coreTextbox.GetText());

        // Undo #1 undos "World"
        coreTextbox.Undo();
        Assert.AreEqual("Hello " + textBefore, coreTextbox.GetText());

        // Undo #2 undos the space " "
        coreTextbox.Undo();
        Assert.AreEqual("Hello" + textBefore, coreTextbox.GetText());

        // Undo #3 undos "Hello"
        coreTextbox.Undo();
        Assert.AreEqual(textBefore, coreTextbox.GetText());

        // Redo restores each step in order
        coreTextbox.Redo();
        Assert.AreEqual("Hello" + textBefore, coreTextbox.GetText());

        coreTextbox.Redo();
        Assert.AreEqual("Hello " + textBefore, coreTextbox.GetText());

        coreTextbox.Redo();
        Assert.AreEqual("Hello World" + textBefore, coreTextbox.GetText());
    }

    [UITestMethod]
    public void UndoBatching_WordAndSymbol_SeparateUndo()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.SetCursorPosition(0, 0);
        string textBefore = coreTextbox.GetText();

        foreach (char c in "foo.bar")
        {
            coreTextbox.textActionManager.AddCharacter(c.ToString());
        }

        Assert.AreEqual("foo.bar" + textBefore, coreTextbox.GetText());

        // Undo #1 undos "bar"
        coreTextbox.Undo();
        Assert.AreEqual("foo." + textBefore, coreTextbox.GetText());

        // Undo #2 undos "."
        coreTextbox.Undo();
        Assert.AreEqual("foo" + textBefore, coreTextbox.GetText());

        // Undo #3 undos "foo"
        coreTextbox.Undo();
        Assert.AreEqual(textBefore, coreTextbox.GetText());
    }

    [UITestMethod]
    public void UndoBatching_CursorMovement_BreaksBatch()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.SetCursorPosition(0, 0);
        string textBefore = coreTextbox.GetText();

        foreach (char c in "Hello")
        {
            coreTextbox.textActionManager.AddCharacter(c.ToString());
        }

        // Reposition cursor to start
        coreTextbox.SetCursorPosition(0, 0);

        // Type 'X'
        coreTextbox.textActionManager.AddCharacter("X");
        Assert.StartsWith("XHello", coreTextbox.GetLineText(0));

        // Undo #1 should revert 'X'
        coreTextbox.Undo();
        Assert.StartsWith("Hello", coreTextbox.GetLineText(0));

        // Undo #2 should revert "Hello"
        coreTextbox.Undo();
        Assert.AreEqual(textBefore, coreTextbox.GetText());
    }

    [UITestMethod]
    public void UndoBatching_Backspace_BreaksBatch()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.SetCursorPosition(0, 0);
        string textBefore = coreTextbox.GetText();

        foreach (char c in "Hello")
        {
            coreTextbox.textActionManager.AddCharacter(c.ToString());
        }

        // Backspace removes the 'o'
        coreTextbox.textActionManager.RemoveText(false);
        Assert.StartsWith("Hell", coreTextbox.GetLineText(0));

        // Undo #1 should revert the Backspace (restores 'o')
        coreTextbox.Undo();
        Assert.StartsWith("Hello", coreTextbox.GetLineText(0));

        // Undo #2 should revert "Hello"
        coreTextbox.Undo();
        Assert.AreEqual(textBefore, coreTextbox.GetText());
    }

    [UITestMethod]
    public void UndoBatching_Timeout_BreaksBatch()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.undoRedo.BatchTimeout = TimeSpan.FromMilliseconds(50);
        coreTextbox.SetCursorPosition(0, 0);
        string textBefore = coreTextbox.GetText();

        foreach (char c in "He")
        {
            coreTextbox.textActionManager.AddCharacter(c.ToString());
        }

        // Wait longer than timeout
        System.Threading.Thread.Sleep(70);

        foreach (char c in "llo")
        {
            coreTextbox.textActionManager.AddCharacter(c.ToString());
        }

        Assert.AreEqual("Hello" + textBefore, coreTextbox.GetText());

        // Undo #1 should revert "llo"
        coreTextbox.Undo();
        Assert.AreEqual("He" + textBefore, coreTextbox.GetText());

        // Undo #2 should revert "He"
        coreTextbox.Undo();
        Assert.AreEqual(textBefore, coreTextbox.GetText());
    }

    [UITestMethod]
    public void UndoBatching_Disabled_BehavesCharByChar()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.undoRedo.UndoBatching = false;
        coreTextbox.SetCursorPosition(0, 0);
        string textBefore = coreTextbox.GetText();

        foreach (char c in "Hi")
        {
            coreTextbox.textActionManager.AddCharacter(c.ToString());
        }

        Assert.AreEqual("Hi" + textBefore, coreTextbox.GetText());

        // When batching is disabled, Undo reverts char by char
        coreTextbox.Undo();
        Assert.AreEqual("H" + textBefore, coreTextbox.GetText());

        coreTextbox.Undo();
        Assert.AreEqual(textBefore, coreTextbox.GetText());
    }

    [UITestMethod]
    public void UndoBatching_RedoStackClearedOnNewEdit()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.SetCursorPosition(0, 0);

        foreach (char c in "Hello")
        {
            coreTextbox.textActionManager.AddCharacter(c.ToString());
        }

        // Undo "Hello"
        coreTextbox.Undo();
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);

        // Type a new word "World" -> RedoStack must be cleared!
        foreach (char c in "World")
        {
            coreTextbox.textActionManager.AddCharacter(c.ToString());
        }

        Assert.IsFalse(coreTextbox.undoRedo.CanRedo);

        // Undo should now revert "World"
        coreTextbox.Undo();
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
    }

    [UITestMethod]
    public void AddCharacter_ReplaceSelection_Form2WholeLine_UndoRestoresAllLines()
    {
        // Regression test: replacing whole-line selection (0,0) to (1,0) previously destroyed Line 1 on Undo
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("Line 0\nLine 1\nLine 2");
        Assert.AreEqual(3, coreTextbox.textManager.LinesCount);

        // Select Form 2: from (0, 0) to (1, 0)
        coreTextbox.SetSelection(0, 0, 1, 0);
        Assert.IsTrue(coreTextbox.selectionManager.WholeLineSelected());

        coreTextbox.textActionManager.AddCharacter("Replaced");

        // After replacement, Line 0 is "ReplacedLine 1", Line 1 is "Line 2"
        Assert.AreEqual(2, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("ReplacedLine 1", coreTextbox.GetLineText(0));
        Assert.AreEqual("Line 2", coreTextbox.GetLineText(1));

        // Undo must restore ALL 3 lines, specifically Line 1 must NOT be lost!
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual(3, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Line 0", coreTextbox.GetLineText(0));
        Assert.AreEqual("Line 1", coreTextbox.GetLineText(1));
        Assert.AreEqual("Line 2", coreTextbox.GetLineText(2));

        // Redo must restore the replaced state
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual(2, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("ReplacedLine 1", coreTextbox.GetLineText(0));
        Assert.AreEqual("Line 2", coreTextbox.GetLineText(1));
    }

    [UITestMethod]
    public void UndoRedo_ActionGroup_RestoresCursorAndSelectionToGroupStartAndEnd()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        string textBefore = coreTextbox.stringManager.CleanUpString("Line 0\nLine 1\nLine 2");
        coreTextbox.SetText(textBefore);
        coreTextbox.SetCursorPosition(0, 0);

        coreTextbox.BeginActionGroup();
        coreTextbox.textActionManager.AddLine(1, "Inserted Line");
        coreTextbox.SetCursorPosition(1, 5);
        coreTextbox.textActionManager.AddCharacter("XXX");
        coreTextbox.EndActionGroup();

        // After action group: cursor is at (1, 8)
        Assert.AreEqual(1, coreTextbox.cursorManager.LineNumber);
        Assert.AreEqual(8, coreTextbox.cursorManager.CharacterPosition);

        // Undo should restore cursor to the START of the group (0, 0)
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual(0, coreTextbox.cursorManager.LineNumber);
        Assert.AreEqual(0, coreTextbox.cursorManager.CharacterPosition);
        Assert.AreEqual(textBefore, coreTextbox.GetText());

        // Redo should restore cursor to the END of the group (1, 8)
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual(1, coreTextbox.cursorManager.LineNumber);
        Assert.AreEqual(8, coreTextbox.cursorManager.CharacterPosition);
    }

    [UITestMethod]
    public void Safe_SetText_MixedLineEndings_UndoRestoresExactOriginalLines()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.LineEnding = LineEnding.CRLF;
        coreTextbox.SetText("Orig 0\r\nOrig 1");
        string textBefore = coreTextbox.GetText();
        Assert.AreEqual(2, coreTextbox.textManager.LinesCount);

        // SetText using Unix LF (\n)
        coreTextbox.SetText("New 0\nNew 1\nNew 2");

        Assert.AreEqual(3, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("New 0", coreTextbox.GetLineText(0));
        Assert.AreEqual("New 1", coreTextbox.GetLineText(1));
        Assert.AreEqual("New 2", coreTextbox.GetLineText(2));

        // Undo must restore exactly 2 lines without leaving extra lines
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual(2, coreTextbox.textManager.LinesCount);
        Assert.AreEqual(textBefore, coreTextbox.GetText());

        // Redo restores 3 lines
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual(3, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("New 0", coreTextbox.GetLineText(0));
    }

    [UITestMethod]
    public void UndoRedo_NoOpEmptyLine_DoesNotPushUndoItem()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("");
        coreTextbox.undoRedo.ClearAll();

        Assert.IsFalse(coreTextbox.undoRedo.CanUndo);

        // Backspace and delete on empty doc
        coreTextbox.textActionManager.RemoveText();
        Assert.IsFalse(coreTextbox.undoRedo.CanUndo);

        coreTextbox.textActionManager.DeleteText();
        Assert.IsFalse(coreTextbox.undoRedo.CanUndo);

        // Add empty string
        coreTextbox.textActionManager.AddCharacter("");
        Assert.IsFalse(coreTextbox.undoRedo.CanUndo);
    }

    [UITestMethod]
    public void UndoRedo_NullAll_DoesNotThrowNullReferenceException()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.undoRedo.NullAll();

        Assert.IsFalse(coreTextbox.undoRedo.CanUndo);
        Assert.IsFalse(coreTextbox.undoRedo.CanRedo);

        // Undo / Redo should safely return without throwing
        coreTextbox.Undo();
        coreTextbox.Redo();
    }

    [UITestMethod]
    public void DeleteLine_SingleLineDocument_ClearsAndRestoresOnUndo()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("Only Line In Document");
        Assert.AreEqual(1, coreTextbox.textManager.LinesCount);

        coreTextbox.DeleteLine(0);

        Assert.AreEqual(1, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("", coreTextbox.GetLineText(0));

        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual(1, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Only Line In Document", coreTextbox.GetLineText(0));

        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual(1, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("", coreTextbox.GetLineText(0));
    }

    [UITestMethod]
    public void DeleteSelection_SingleLineDocument_SelectLine_UndoRestoresSingleLine()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("Only Line In Document");
        Assert.AreEqual(1, coreTextbox.textManager.LinesCount);

        coreTextbox.SelectLine(0);
        coreTextbox.textActionManager.DeleteSelection();

        Assert.AreEqual(1, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("", coreTextbox.GetLineText(0));

        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual(1, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Only Line In Document", coreTextbox.GetLineText(0));

        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual(1, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("", coreTextbox.GetLineText(0));
    }

    [UITestMethod]
    public void AddLine_AtDocumentEnd_UndoRemovesAndRedoRestores()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("Line 0\nLine 1\nLine 2");
        Assert.AreEqual(3, coreTextbox.textManager.LinesCount);

        // Add line at index 3 (end of document)
        bool added = coreTextbox.textActionManager.AddLine(3, "Line 3");
        Assert.IsTrue(added);

        Assert.AreEqual(4, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Line 3", coreTextbox.GetLineText(3));

        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual(3, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Line 2", coreTextbox.GetLineText(2));

        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual(4, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Line 3", coreTextbox.GetLineText(3));
    }

    [UITestMethod]
    public void AddLines_MultipleLines_UndoRemovesAndRedoRestores()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("Line 0\nLine 1");
        Assert.AreEqual(2, coreTextbox.textManager.LinesCount);

        string[] newLines = ["Inserted A", "Inserted B"];
        bool added = coreTextbox.textActionManager.AddLines(1, newLines);
        Assert.IsTrue(added);

        Assert.AreEqual(4, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Line 0", coreTextbox.GetLineText(0));
        Assert.AreEqual("Inserted A", coreTextbox.GetLineText(1));
        Assert.AreEqual("Inserted B", coreTextbox.GetLineText(2));
        Assert.AreEqual("Line 1", coreTextbox.GetLineText(3));

        // Undo removes both inserted lines and restores 2 lines
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual(2, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Line 0", coreTextbox.GetLineText(0));
        Assert.AreEqual("Line 1", coreTextbox.GetLineText(1));

        // Redo re-inserts both lines
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual(4, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Inserted A", coreTextbox.GetLineText(1));
        Assert.AreEqual("Inserted B", coreTextbox.GetLineText(2));
        Assert.AreEqual("Line 1", coreTextbox.GetLineText(3));
    }

    [UITestMethod]
    public void MoveLine_UpDown_UndoAndRedo_PreservesTextAndCursor()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.LineEnding = LineEnding.LF;
        coreTextbox.SetText("Line A\nLine B\nLine C");
        coreTextbox.SetCursorPosition(1, 2);

        // Move Line B down
        bool moved = coreTextbox.moveLineManager.Move(LineMoveDirection.Down);
        Assert.IsTrue(moved);

        Assert.AreEqual("Line A\nLine C\nLine B", coreTextbox.GetText());
        Assert.AreEqual(2, coreTextbox.cursorManager.LineNumber);
        Assert.AreEqual(2, coreTextbox.cursorManager.CharacterPosition);

        // Undo move down
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual("Line A\nLine B\nLine C", coreTextbox.GetText());
        Assert.AreEqual(1, coreTextbox.cursorManager.LineNumber);
        Assert.AreEqual(2, coreTextbox.cursorManager.CharacterPosition);

        // Redo move down
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual("Line A\nLine C\nLine B", coreTextbox.GetText());
        Assert.AreEqual(2, coreTextbox.cursorManager.LineNumber);

        // Move Line B up (back to index 1)
        moved = coreTextbox.moveLineManager.Move(LineMoveDirection.Up);
        Assert.IsTrue(moved);
        Assert.AreEqual("Line A\nLine B\nLine C", coreTextbox.GetText());
        Assert.AreEqual(1, coreTextbox.cursorManager.LineNumber);
    }

    [UITestMethod]
    public void ReplaceNext_MultiLineReplacement_UndoRedoAccurate()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("Prefix Target Suffix");
        coreTextbox.SetCursorPosition(0, 0);

        coreTextbox.searchManager.BeginSearch("Target", wholeWord: false, matchCase: true);
        string multiLine = coreTextbox.stringManager.CleanUpString("LineA\nLineB");
        var replaceRes = coreTextbox.replaceManager.ReplaceNext(multiLine);

        Assert.AreEqual(SearchResult.Found, replaceRes.Result);
        Assert.AreEqual(2, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Prefix LineA", coreTextbox.GetLineText(0));
        Assert.AreEqual("LineB Suffix", coreTextbox.GetLineText(1));

        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual(1, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Prefix Target Suffix", coreTextbox.GetText());

        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual(2, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Prefix LineA", coreTextbox.GetLineText(0));
        Assert.AreEqual("LineB Suffix", coreTextbox.GetLineText(1));
    }

    [UITestMethod]
    public void Undo_AnyNewEditClearsRedoStack()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("Sample");

        // Action 1: Add a character
        coreTextbox.SetCursorPosition(0, 6);
        coreTextbox.textActionManager.AddCharacter("!");
        Assert.AreEqual("Sample!", coreTextbox.GetText());

        // Undo -> Redo is available
        coreTextbox.Undo();
        Assert.AreEqual("Sample", coreTextbox.GetText());
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);

        // Action 2: Backspace -> Redo must be cleared!
        coreTextbox.textActionManager.RemoveText();
        Assert.AreEqual("Sampl", coreTextbox.GetText());
        Assert.IsFalse(coreTextbox.undoRedo.CanRedo);
    }

    [UITestMethod]
    public void Undo_RestoresSelection_WhenReplacingOrDeletingSelection()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("Hello Amazing World");

        // Select "Amazing"
        coreTextbox.SetSelection(0, 6, 0, 13);
        Assert.IsTrue(coreTextbox.selectionManager.HasSelection);

        // Delete selection
        coreTextbox.textActionManager.DeleteSelection();
        Assert.AreEqual("Hello  World", coreTextbox.GetText());
        Assert.IsFalse(coreTextbox.selectionManager.HasSelection);

        // Undo should restore the exact selection
        coreTextbox.Undo();
        Assert.AreEqual("Hello Amazing World", coreTextbox.GetText());
        Assert.IsTrue(coreTextbox.selectionManager.HasSelection);

        var sel = coreTextbox.selectionManager.OrderTextSelectionSeparated();
        Assert.AreEqual(0, sel.startLine);
        Assert.AreEqual(6, sel.startChar);
        Assert.AreEqual(0, sel.endLine);
        Assert.AreEqual(13, sel.endChar);
    }

    [UITestMethod]
    public void UndoBatching_TypingAcrossNewLines_CreatesSeparateUndoSteps()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("");
        coreTextbox.undoRedo.ClearAll();

        // Type "Hello"
        foreach (char c in "Hello")
            coreTextbox.textActionManager.AddCharacter(c.ToString());

        // Enter
        coreTextbox.textActionManager.AddNewLine();

        // Type "World"
        foreach (char c in "World")
            coreTextbox.textActionManager.AddCharacter(c.ToString());

        Assert.AreEqual(2, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Hello", coreTextbox.GetLineText(0));
        Assert.AreEqual("World", coreTextbox.GetLineText(1));

        // Undo 1: Undoes "World"
        coreTextbox.Undo();
        Assert.AreEqual(2, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Hello", coreTextbox.GetLineText(0));
        Assert.AreEqual("", coreTextbox.GetLineText(1));

        // Undo 2: Undoes AddNewLine
        coreTextbox.Undo();
        Assert.AreEqual(1, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Hello", coreTextbox.GetLineText(0));

        // Undo 3: Undoes "Hello"
        coreTextbox.Undo();
        Assert.AreEqual(1, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("", coreTextbox.GetLineText(0));
    }

    [UITestMethod]
    public void DeleteSelection_LastLineContentSelected_UndoDoesNotDuplicateLine()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("Line 0\nLine 1\nLine 2");
        Assert.AreEqual(3, coreTextbox.textManager.LinesCount);

        // Select the entire last line content: (2, 0) to (2, 6)
        coreTextbox.SetSelection(2, 0, 2, coreTextbox.GetLineText(2).Length);

        coreTextbox.textActionManager.DeleteSelection();

        // Line 2 should be emptied, total lines should still be 3
        Assert.AreEqual(3, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("", coreTextbox.GetLineText(2));

        // Undo must restore the exact 3 lines without duplicating Line 2!
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual(3, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Line 2", coreTextbox.GetLineText(2));

        // Redo must restore the empty line state
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual(3, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("", coreTextbox.GetLineText(2));
    }

    [UITestMethod]
    public void AddNewLine_WholeTextSelected_UndoRestoresSelection()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("Alpha\nBeta\nGamma");
        coreTextbox.SelectAll();
        Assert.IsTrue(coreTextbox.selectionManager.WholeTextSelected());

        coreTextbox.textActionManager.AddNewLine();

        // After Enter on whole text: 2 empty lines
        Assert.AreEqual(2, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("", coreTextbox.GetLineText(0));
        Assert.AreEqual("", coreTextbox.GetLineText(1));

        // Undo must restore text AND selection
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual(3, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Alpha", coreTextbox.GetLineText(0));
        Assert.AreEqual("Beta", coreTextbox.GetLineText(1));
        Assert.AreEqual("Gamma", coreTextbox.GetLineText(2));
        Assert.IsTrue(coreTextbox.selectionManager.HasSelection);

        var sel = coreTextbox.selectionManager.OrderTextSelectionSeparated();
        Assert.AreEqual(0, sel.startLine);
        Assert.AreEqual(0, sel.startChar);
        Assert.AreEqual(2, sel.endLine);
        Assert.AreEqual(5, sel.endChar);
    }

    [UITestMethod]
    public void RemoveText_CtrlBackspace_DeletesWordAndUndoRestores()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("first second third");
        coreTextbox.SetCursorPosition(0, 18); // after "third"

        // Ctrl+Backspace deletes "third"
        coreTextbox.textActionManager.RemoveText(controlIsPressed: true);
        Assert.AreEqual("first second ", coreTextbox.GetText());
        Assert.AreEqual(13, coreTextbox.cursorManager.CharacterPosition);

        // Undo restores "third" and cursor at end
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual("first second third", coreTextbox.GetText());
        Assert.AreEqual(18, coreTextbox.cursorManager.CharacterPosition);

        // Redo re-deletes "third"
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual("first second ", coreTextbox.GetText());
        Assert.AreEqual(13, coreTextbox.cursorManager.CharacterPosition);
    }

    [UITestMethod]
    public void DeleteText_CtrlDelete_DeletesWordAndUndoRestores()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("first second third");
        coreTextbox.SetCursorPosition(0, 6); // at "s" in "second"

        // Ctrl+Delete deletes "second"
        coreTextbox.textActionManager.DeleteText(controlIsPressed: true);
        Assert.AreEqual("first  third", coreTextbox.GetText());
        Assert.AreEqual(6, coreTextbox.cursorManager.CharacterPosition);

        // Undo restores "second" and cursor at 6
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual("first second third", coreTextbox.GetText());
        Assert.AreEqual(6, coreTextbox.cursorManager.CharacterPosition);

        // Redo restores deletion
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual("first  third", coreTextbox.GetText());
        Assert.AreEqual(6, coreTextbox.cursorManager.CharacterPosition);
    }

    [UITestMethod]
    public void DeleteSelection_BackwardsSelection_UndoRestoresSelection()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("Hello Amazing World");

        // Backwards selection: start at (0, 13), end at (0, 6) -> selects "Amazing"
        coreTextbox.selectionManager.SetSelection(0, 13, 0, 6);
        Assert.IsTrue(coreTextbox.selectionManager.HasSelection);

        coreTextbox.textActionManager.DeleteSelection();
        Assert.AreEqual("Hello  World", coreTextbox.GetText());
        Assert.IsFalse(coreTextbox.selectionManager.HasSelection);

        // Undo should restore text and selection
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual("Hello Amazing World", coreTextbox.GetText());
        Assert.IsTrue(coreTextbox.selectionManager.HasSelection);

        var sel = coreTextbox.selectionManager.OrderTextSelectionSeparated();
        Assert.AreEqual(0, sel.startLine);
        Assert.AreEqual(6, sel.startChar);
        Assert.AreEqual(0, sel.endLine);
        Assert.AreEqual(13, sel.endChar);
    }

    [UITestMethod]
    public void DeleteSelection_BackwardsMultiLineSelection_UndoRestoresSelection()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("Line 0 start\nLine 1 middle\nLine 2 end");
        Assert.AreEqual(3, coreTextbox.textManager.LinesCount);

        // Select backwards from (2, 6) ("Line 2 ") up to (0, 7) ("Line 0 ")
        coreTextbox.selectionManager.SetSelection(2, 6, 0, 7);
        Assert.IsTrue(coreTextbox.selectionManager.HasSelection);

        coreTextbox.textActionManager.DeleteSelection();

        // Lines 0 and 2 are merged: "Line 0 " + " end" -> "Line 0  end"
        Assert.AreEqual(1, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Line 0  end", coreTextbox.GetLineText(0));
        Assert.IsFalse(coreTextbox.selectionManager.HasSelection);

        // Undo restores all 3 lines AND the ordered selection
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual(3, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Line 0 start", coreTextbox.GetLineText(0));
        Assert.AreEqual("Line 1 middle", coreTextbox.GetLineText(1));
        Assert.AreEqual("Line 2 end", coreTextbox.GetLineText(2));
        Assert.IsTrue(coreTextbox.selectionManager.HasSelection);

        var sel = coreTextbox.selectionManager.OrderTextSelectionSeparated();
        Assert.AreEqual(0, sel.startLine);
        Assert.AreEqual(7, sel.startChar);
        Assert.AreEqual(2, sel.endLine);
        Assert.AreEqual(6, sel.endChar);

        // Redo re-deletes
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual(1, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Line 0  end", coreTextbox.GetLineText(0));
    }

    [UITestMethod]
    public void DeleteSelection_MultiLineToEndOfDocument_UndoRestoresExactLines()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("Line 0\nLine 1\nLine 2\nLine 3");
        Assert.AreEqual(4, coreTextbox.textManager.LinesCount);

        // Select from (1, 3) to the very end of last line (3, 6)
        coreTextbox.SetSelection(1, 3, 3, 6);

        coreTextbox.textActionManager.DeleteSelection();

        // Line 1 becomes "Lin", lines 2 and 3 removed
        Assert.AreEqual(2, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Line 0", coreTextbox.GetLineText(0));
        Assert.AreEqual("Lin", coreTextbox.GetLineText(1));

        // Undo restores all 4 lines
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual(4, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Line 0", coreTextbox.GetLineText(0));
        Assert.AreEqual("Line 1", coreTextbox.GetLineText(1));
        Assert.AreEqual("Line 2", coreTextbox.GetLineText(2));
        Assert.AreEqual("Line 3", coreTextbox.GetLineText(3));

        // Redo restores deleted state
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual(2, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Lin", coreTextbox.GetLineText(1));
    }

    [UITestMethod]
    public void MoveLine_Boundaries_ReturnsFalseAndPushesNoUndo()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("Line 0\nLine 1");
        coreTextbox.undoRedo.ClearAll();

        // Move Line 0 Up -> impossible
        coreTextbox.SetCursorPosition(0, 0);
        bool movedUp = coreTextbox.moveLineManager.Move(LineMoveDirection.Up);
        Assert.IsFalse(movedUp);
        Assert.IsFalse(coreTextbox.undoRedo.CanUndo);

        // Move Line 1 Down -> impossible
        coreTextbox.SetCursorPosition(1, 0);
        bool movedDown = coreTextbox.moveLineManager.Move(LineMoveDirection.Down);
        Assert.IsFalse(movedDown);
        Assert.IsFalse(coreTextbox.undoRedo.CanUndo);

        // Move with active selection -> disallowed
        coreTextbox.SetSelection(0, 0, 0, 3);
        bool movedWithSel = coreTextbox.moveLineManager.Move(LineMoveDirection.Down);
        Assert.IsFalse(movedWithSel);
        Assert.IsFalse(coreTextbox.undoRedo.CanUndo);
    }

    [UITestMethod]
    public void DuplicateLine_LastLineAndInvalidLines()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("Line 0\nLine 1\nLine 2");
        Assert.AreEqual(3, coreTextbox.textManager.LinesCount);

        // Duplicate last line (index 2)
        coreTextbox.DuplicateLine(2);

        Assert.AreEqual(4, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Line 2", coreTextbox.GetLineText(2));
        Assert.AreEqual("Line 2", coreTextbox.GetLineText(3));

        // Undo restores 3 lines
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual(3, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Line 2", coreTextbox.GetLineText(2));

        // Redo restores 4 lines
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual(4, coreTextbox.textManager.LinesCount);

        // Out-of-bounds DuplicateLine calls must safely no-op without throwing
        coreTextbox.undoRedo.ClearAll();
        coreTextbox.DuplicateLine(-1);
        coreTextbox.DuplicateLine(100);
        Assert.IsFalse(coreTextbox.undoRedo.CanUndo);
        Assert.AreEqual(4, coreTextbox.textManager.LinesCount);
    }

    [UITestMethod]
    public void Tab_MultiLineSelection_IndentsAndUndoRestoresSelection()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.tabSpaceManager.UseSpacesInsteadTabs = true;
        coreTextbox.tabSpaceManager.NumberOfSpaces = 4;
        coreTextbox.SetText("Line 0\nLine 1\nLine 2");

        // Select across Line 0 and Line 1
        coreTextbox.SetSelection(0, 0, 1, 6);

        // Press Tab (indents both lines)
        coreTextbox.tabSpaceManager.MoveTab();

        Assert.AreEqual("    Line 0", coreTextbox.GetLineText(0));
        Assert.AreEqual("    Line 1", coreTextbox.GetLineText(1));
        Assert.AreEqual("Line 2", coreTextbox.GetLineText(2));

        // Undo must restore unindented lines AND original selection
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual("Line 0", coreTextbox.GetLineText(0));
        Assert.AreEqual("Line 1", coreTextbox.GetLineText(1));
        Assert.AreEqual("Line 2", coreTextbox.GetLineText(2));
        Assert.IsTrue(coreTextbox.selectionManager.HasSelection);

        var sel = coreTextbox.selectionManager.OrderTextSelectionSeparated();
        Assert.AreEqual(0, sel.startLine);
        Assert.AreEqual(0, sel.startChar);
        Assert.AreEqual(1, sel.endLine);
        Assert.AreEqual(6, sel.endChar);

        // Redo restores indented lines
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual("    Line 0", coreTextbox.GetLineText(0));
        Assert.AreEqual("    Line 1", coreTextbox.GetLineText(1));
    }

    [UITestMethod]
    public void ShiftTab_NoIndentation_DoesNotPushUndoItem()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.tabSpaceManager.UseSpacesInsteadTabs = true;
        coreTextbox.tabSpaceManager.NumberOfSpaces = 4;
        coreTextbox.SetText("NoIndent 0\nNoIndent 1");
        coreTextbox.undoRedo.ClearAll();

        coreTextbox.SetCursorPosition(0, 2);
        coreTextbox.ClearSelection();

        // Shift+Tab on line without indentation -> no change
        coreTextbox.tabSpaceManager.MoveTabBack();

        Assert.AreEqual("NoIndent 0", coreTextbox.GetLineText(0));
        Assert.IsFalse(coreTextbox.undoRedo.CanUndo);
    }

    [UITestMethod]
    public void UndoRedo_Disabled_DoesNotRecordActions()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("Initial Text");
        coreTextbox.undoRedo.ClearAll();

        coreTextbox.undoRedo.UndoRedoEnabled = false;

        coreTextbox.SetCursorPosition(0, 12);
        coreTextbox.textActionManager.AddCharacter(" Extra");
        Assert.AreEqual("Initial Text Extra", coreTextbox.GetText());

        // CanUndo must be false when disabled
        Assert.IsFalse(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();
        Assert.AreEqual("Initial Text Extra", coreTextbox.GetText());

        // Re-enable -> stacks should remain clean
        coreTextbox.undoRedo.UndoRedoEnabled = true;
        Assert.IsFalse(coreTextbox.undoRedo.CanUndo);
    }

    [UITestMethod]
    public void ReplaceAll_MultipleMatchesAcrossDocument_SingleUndoRestoresAll()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        string initialText = "apple and banana\nbanana and orange\ncherry and banana";
        coreTextbox.SetText(initialText);
        coreTextbox.undoRedo.ClearAll();

        var result = coreTextbox.replaceManager.ReplaceAll("banana", "MANGO", matchCase: true, wholeWord: true);

        Assert.AreEqual(SearchResult.Found, result);
        Assert.AreEqual(3, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("apple and MANGO", coreTextbox.GetLineText(0));
        Assert.AreEqual("MANGO and orange", coreTextbox.GetLineText(1));
        Assert.AreEqual("cherry and MANGO", coreTextbox.GetLineText(2));

        // A single Undo must revert all replacements across the entire document
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual(3, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("apple and banana", coreTextbox.GetLineText(0));
        Assert.AreEqual("banana and orange", coreTextbox.GetLineText(1));
        Assert.AreEqual("cherry and banana", coreTextbox.GetLineText(2));

        // Redo reapplies all replacements
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual(3, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("apple and MANGO", coreTextbox.GetLineText(0));
        Assert.AreEqual("MANGO and orange", coreTextbox.GetLineText(1));
        Assert.AreEqual("cherry and MANGO", coreTextbox.GetLineText(2));
    }

    [UITestMethod]
    public void ReplaceAll_WordNotFound_DoesNotPushUndoItem()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("hello world");
        coreTextbox.undoRedo.ClearAll();

        var result = coreTextbox.replaceManager.ReplaceAll("nonexistent", "replacement", matchCase: true, wholeWord: true);

        Assert.AreEqual(SearchResult.NotFound, result);
        Assert.AreEqual("hello world", coreTextbox.GetText());
        Assert.IsFalse(coreTextbox.undoRedo.CanUndo);
    }

    [UITestMethod]
    public void AutoPairing_WithoutSelection_UndoRemovesPairAndRestoresCursor()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.DoAutoPairing = true;
        coreTextbox.AutoPairOnlyOnSelection = false;
        coreTextbox.SyntaxHighlighting = new SyntaxHighlightLanguage
        {
            AutoPairingPair = [new AutoPairingPair("(", ")")]
        };
        coreTextbox.SetText("Call");
        coreTextbox.SetCursorPosition(0, 4);
        coreTextbox.undoRedo.ClearAll();

        // Type '(' -> AutoPairing inserts "()" and puts cursor between them at col 5
        coreTextbox.textActionManager.AddCharacter("(");

        Assert.AreEqual("Call()", coreTextbox.GetLineText(0));
        Assert.AreEqual(5, coreTextbox.cursorManager.CharacterPosition);

        // Undo must remove both '(' and ')' and restore cursor to col 4
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual("Call", coreTextbox.GetLineText(0));
        Assert.AreEqual(4, coreTextbox.cursorManager.CharacterPosition);

        // Redo restores "()" and cursor at 5
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual("Call()", coreTextbox.GetLineText(0));
        Assert.AreEqual(5, coreTextbox.cursorManager.CharacterPosition);
    }

    [UITestMethod]
    public void AutoPairing_WithSelection_UndoRestoresSelectionAndOriginalText()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.DoAutoPairing = true;
        coreTextbox.SyntaxHighlighting = new SyntaxHighlightLanguage
        {
            AutoPairingPair = [new AutoPairingPair("(", ")")]
        };
        coreTextbox.SetText("Hello Target World");
        // Select "Target" (0, 6) to (0, 12)
        coreTextbox.SetSelection(0, 6, 0, 12);
        coreTextbox.undoRedo.ClearAll();

        // Type '(' over selection -> wraps selection in parentheses: "Hello (Target) World"
        coreTextbox.textActionManager.AddCharacter("(");

        Assert.AreEqual("Hello (Target) World", coreTextbox.GetText());

        // Undo must restore unwrapped text AND restore the exact selection
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual("Hello Target World", coreTextbox.GetText());
        Assert.IsTrue(coreTextbox.selectionManager.HasSelection);

        var sel = coreTextbox.selectionManager.OrderTextSelectionSeparated();
        Assert.AreEqual(0, sel.startLine);
        Assert.AreEqual(6, sel.startChar);
        Assert.AreEqual(0, sel.endLine);
        Assert.AreEqual(12, sel.endChar);

        // Redo restores wrapped text
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual("Hello (Target) World", coreTextbox.GetText());
    }

    [UITestMethod]
    public void EmptyDocument_AddCharacter_UndoLeavesEmptyDocumentWithoutCrash()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("");
        coreTextbox.undoRedo.ClearAll();

        Assert.AreEqual(1, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("", coreTextbox.GetText());

        coreTextbox.textActionManager.AddCharacter("A");

        Assert.AreEqual(1, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("A", coreTextbox.GetText());

        // Undo must revert to empty document (1 line with "", not 0 lines, no exception)
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual(1, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("", coreTextbox.GetText());

        // Redo restores "A"
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual(1, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("A", coreTextbox.GetText());
    }

    [UITestMethod]
    public void EmptyDocument_AddNewLine_UndoRestoresSingleEmptyLine()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("");
        coreTextbox.undoRedo.ClearAll();

        // Add newline in empty document -> creates 2 empty lines
        coreTextbox.textActionManager.AddNewLine();

        Assert.AreEqual(2, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("", coreTextbox.GetLineText(0));
        Assert.AreEqual("", coreTextbox.GetLineText(1));

        // Undo restores single empty line
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual(1, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("", coreTextbox.GetLineText(0));

        // Redo restores 2 empty lines
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual(2, coreTextbox.textManager.LinesCount);
    }

    [UITestMethod]
    public void RemoveText_MergeWithEmptyLine_UndoRestoresEmptyLine()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("First Line\n");
        Assert.AreEqual(2, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("", coreTextbox.GetLineText(1));

        // Cursor at start of empty line 1
        coreTextbox.SetCursorPosition(1, 0);
        coreTextbox.undoRedo.ClearAll();

        // Backspace merges empty line 1 into line 0
        coreTextbox.textActionManager.RemoveText();

        Assert.AreEqual(1, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("First Line", coreTextbox.GetLineText(0));
        Assert.AreEqual(10, coreTextbox.cursorManager.CharacterPosition);

        // Undo must restore the empty line at index 1 and cursor at (1, 0)
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual(2, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("First Line", coreTextbox.GetLineText(0));
        Assert.AreEqual("", coreTextbox.GetLineText(1));
        Assert.AreEqual(1, coreTextbox.cursorManager.LineNumber);
        Assert.AreEqual(0, coreTextbox.cursorManager.CharacterPosition);

        // Redo re-merges
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual(1, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("First Line", coreTextbox.GetLineText(0));
    }

    [UITestMethod]
    public void DeleteText_MergeWithEmptyLine_UndoRestoresEmptyLine()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("First Line\n");
        Assert.AreEqual(2, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("", coreTextbox.GetLineText(1));

        // Cursor at end of line 0
        coreTextbox.SetCursorPosition(0, 10);
        coreTextbox.undoRedo.ClearAll();

        // Delete key merges empty line 1 into line 0
        coreTextbox.textActionManager.DeleteText();

        Assert.AreEqual(1, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("First Line", coreTextbox.GetLineText(0));

        // Undo restores empty line 1
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual(2, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("First Line", coreTextbox.GetLineText(0));
        Assert.AreEqual("", coreTextbox.GetLineText(1));

        // Redo re-deletes
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual(1, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("First Line", coreTextbox.GetLineText(0));
    }

    [UITestMethod]
    public void ShiftTab_MultiLineSelection_UnindentsAndUndoRestoresSelection()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.tabSpaceManager.UseSpacesInsteadTabs = true;
        coreTextbox.tabSpaceManager.NumberOfSpaces = 4;
        coreTextbox.SetText("    Line A\n    Line B\n    Line C");
        coreTextbox.undoRedo.ClearAll();

        // Select across all 3 lines
        coreTextbox.SetSelection(0, 0, 2, 10);

        // Shift+Tab unindents all 3 lines
        coreTextbox.tabSpaceManager.MoveTabBack();

        Assert.AreEqual("Line A", coreTextbox.GetLineText(0));
        Assert.AreEqual("Line B", coreTextbox.GetLineText(1));
        Assert.AreEqual("Line C", coreTextbox.GetLineText(2));

        // Undo must restore all 4 spaces on each line AND restore selection
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual("    Line A", coreTextbox.GetLineText(0));
        Assert.AreEqual("    Line B", coreTextbox.GetLineText(1));
        Assert.AreEqual("    Line C", coreTextbox.GetLineText(2));
        Assert.IsTrue(coreTextbox.selectionManager.HasSelection);

        // Redo unindents again
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual("Line A", coreTextbox.GetLineText(0));
        Assert.AreEqual("Line B", coreTextbox.GetLineText(1));
        Assert.AreEqual("Line C", coreTextbox.GetLineText(2));
    }

    [UITestMethod]
    public void AddNewLine_WithAutoIndention_UndoRemovesIndentedLineAndIndentation()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.tabSpaceManager.UseSpacesInsteadTabs = true;
        coreTextbox.tabSpaceManager.NumberOfSpaces = 4;
        coreTextbox.SetText("    Indented Text");
        coreTextbox.SetCursorPosition(0, 17);
        coreTextbox.undoRedo.ClearAll();

        // Enter on indented line -> line 1 inherits 4 spaces indentation
        coreTextbox.textActionManager.AddNewLine();

        Assert.AreEqual(2, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("    Indented Text", coreTextbox.GetLineText(0));
        Assert.AreEqual("    ", coreTextbox.GetLineText(1));
        Assert.AreEqual(1, coreTextbox.cursorManager.LineNumber);
        Assert.AreEqual(4, coreTextbox.cursorManager.CharacterPosition);

        // Undo must remove line 1 and restore cursor to line 0 col 17
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual(1, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("    Indented Text", coreTextbox.GetLineText(0));
        Assert.AreEqual(0, coreTextbox.cursorManager.LineNumber);
        Assert.AreEqual(17, coreTextbox.cursorManager.CharacterPosition);

        // Redo restores indented line 1
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual(2, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("    ", coreTextbox.GetLineText(1));
    }

    [UITestMethod]
    public void BranchingHistory_NewEditAfterMultipleUndos_ClearsRedoAndCanUndoNewBranch()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("");
        coreTextbox.undoRedo.ClearAll();

        // Edit 1: "One"
        foreach (char c in "One") coreTextbox.textActionManager.AddCharacter(c.ToString());
        // Edit 2: " "
        coreTextbox.textActionManager.AddCharacter(" ");
        // Edit 3: "Two"
        foreach (char c in "Two") coreTextbox.textActionManager.AddCharacter(c.ToString());

        Assert.AreEqual("One Two", coreTextbox.GetText());

        // Undo 2 steps -> reverts "Two" then reverts " " -> leaves "One"
        coreTextbox.Undo();
        coreTextbox.Undo();
        Assert.AreEqual("One", coreTextbox.GetText());
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);

        // Type a new branch: " Three" -> Redo stack MUST be cleared immediately!
        foreach (char c in " Three") coreTextbox.textActionManager.AddCharacter(c.ToString());

        Assert.AreEqual("One Three", coreTextbox.GetText());
        Assert.IsFalse(coreTextbox.undoRedo.CanRedo);

        // Undo new branch: reverts "Three", then " ", then "One"
        coreTextbox.Undo(); // reverts "Three"
        coreTextbox.Undo(); // reverts " "
        Assert.AreEqual("One", coreTextbox.GetText());

        coreTextbox.Undo(); // reverts "One"
        Assert.AreEqual("", coreTextbox.GetText());

        // Redo forward along the new branch
        coreTextbox.Redo(); // restores "One"
        Assert.AreEqual("One", coreTextbox.GetText());

        coreTextbox.Redo(); // restores " "
        coreTextbox.Redo(); // restores "Three"
        Assert.AreEqual("One Three", coreTextbox.GetText());
    }

    [UITestMethod]
    public void AddCharacter_MultiLinePasteInMiddleOfLine_UndoRestoresExactLine()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("Start End");
        // Place cursor at col 6 (between "Start " and "End")
        coreTextbox.SetCursorPosition(0, 6);
        coreTextbox.undoRedo.ClearAll();

        string multiLinePaste = coreTextbox.stringManager.CleanUpString("LineA\nLineB");
        coreTextbox.textActionManager.AddCharacter(multiLinePaste);

        Assert.AreEqual(2, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Start LineA", coreTextbox.GetLineText(0));
        Assert.AreEqual("LineBEnd", coreTextbox.GetLineText(1));

        // Undo must restore "Start End" and cursor at col 6
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual(1, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Start End", coreTextbox.GetLineText(0));
        Assert.AreEqual(0, coreTextbox.cursorManager.LineNumber);
        Assert.AreEqual(6, coreTextbox.cursorManager.CharacterPosition);

        // Redo restores the multi-line paste
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual(2, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Start LineA", coreTextbox.GetLineText(0));
        Assert.AreEqual("LineBEnd", coreTextbox.GetLineText(1));
    }

    [UITestMethod]
    public void ReplaceMultiLineSelection_WithMultiLineText_UndoRestoresAllOriginalLinesAndSelection()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("L0\nL1-prefix L1-target\nL2-target\nL3-target L3-suffix\nL4");
        Assert.AreEqual(5, coreTextbox.textManager.LinesCount);
        coreTextbox.undoRedo.ClearAll();

        // Select from (1, 10) to (3, 9) (covers the target portions across 3 lines)
        coreTextbox.SetSelection(1, 10, 3, 9);

        string replacement = coreTextbox.stringManager.CleanUpString("New1\nNew2\nNew3");
        coreTextbox.textActionManager.AddCharacter(replacement);

        // Line 1 becomes "L1-prefix New1", Line 2 is "New2", Line 3 is "New3 L3-suffix"
        Assert.AreEqual(5, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("L0", coreTextbox.GetLineText(0));
        Assert.AreEqual("L1-prefix New1", coreTextbox.GetLineText(1));
        Assert.AreEqual("New2", coreTextbox.GetLineText(2));
        Assert.AreEqual("New3 L3-suffix", coreTextbox.GetLineText(3));
        Assert.AreEqual("L4", coreTextbox.GetLineText(4));

        // Undo must restore all 5 original lines AND original selection
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual(5, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("L1-prefix L1-target", coreTextbox.GetLineText(1));
        Assert.AreEqual("L2-target", coreTextbox.GetLineText(2));
        Assert.AreEqual("L3-target L3-suffix", coreTextbox.GetLineText(3));
        Assert.IsTrue(coreTextbox.selectionManager.HasSelection);

        var sel = coreTextbox.selectionManager.OrderTextSelectionSeparated();
        Assert.AreEqual(1, sel.startLine);
        Assert.AreEqual(10, sel.startChar);
        Assert.AreEqual(3, sel.endLine);
        Assert.AreEqual(9, sel.endChar);

        // Redo re-replaces
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual("L1-prefix New1", coreTextbox.GetLineText(1));
        Assert.AreEqual("New2", coreTextbox.GetLineText(2));
        Assert.AreEqual("New3 L3-suffix", coreTextbox.GetLineText(3));
    }

    [UITestMethod]
    public void SetLineText_UndoRedo_RestoresLineAndCursor_NoOpDoesNotPushUndo()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("Line 0\nLine 1\nLine 2");
        coreTextbox.undoRedo.ClearAll();

        bool set = coreTextbox.textActionManager.SetLineText(1, "Updated Line 1");
        Assert.IsTrue(set);
        Assert.AreEqual("Updated Line 1", coreTextbox.GetLineText(1));

        // Undo restores original line 1
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual("Line 1", coreTextbox.GetLineText(1));

        // Redo restores "Updated Line 1"
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual("Updated Line 1", coreTextbox.GetLineText(1));

        // Setting identical text must not push a new undo item
        coreTextbox.undoRedo.ClearAll();
        coreTextbox.textActionManager.SetLineText(1, "Updated Line 1");
        Assert.IsFalse(coreTextbox.undoRedo.CanUndo);
    }

    [UITestMethod]
    public void ActionGroup_EmptyGroup_DoesNotCorruptStackOrCrash()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("Line 0\nLine 1");
        coreTextbox.undoRedo.ClearAll();

        coreTextbox.BeginActionGroup();
        // Zero actions inside group
        coreTextbox.EndActionGroup();

        Assert.IsFalse(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo(); // Should safely no-op
        Assert.AreEqual(coreTextbox.stringManager.CleanUpString("Line 0\nLine 1"), coreTextbox.GetText());
    }

    [UITestMethod]
    public void ActionGroup_MixedOperations_SingleUndoRestoresAllAtomically()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        string initialText = "Line 0\nLine 1\nLine 2";
        coreTextbox.SetText(initialText);
        coreTextbox.undoRedo.ClearAll();

        coreTextbox.ExecuteActionGroup(() =>
        {
            coreTextbox.textActionManager.AddLine(1, "Inserted Line");
            coreTextbox.textActionManager.SetLineText(0, "Modified Line 0");
            coreTextbox.textActionManager.DeleteLine(3); // deletes "Line 2"
        });

        Assert.AreEqual(3, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Modified Line 0", coreTextbox.GetLineText(0));
        Assert.AreEqual("Inserted Line", coreTextbox.GetLineText(1));
        Assert.AreEqual("Line 1", coreTextbox.GetLineText(2));

        // Exactly ONE Undo must revert all 3 operations atomically back to initialText
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();

        Assert.AreEqual(3, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Line 0", coreTextbox.GetLineText(0));
        Assert.AreEqual("Line 1", coreTextbox.GetLineText(1));
        Assert.AreEqual("Line 2", coreTextbox.GetLineText(2));

        // Exactly ONE Redo reapplies all 3 operations
        Assert.IsTrue(coreTextbox.undoRedo.CanRedo);
        coreTextbox.Redo();

        Assert.AreEqual("Modified Line 0", coreTextbox.GetLineText(0));
        Assert.AreEqual("Inserted Line", coreTextbox.GetLineText(1));
        Assert.AreEqual("Line 1", coreTextbox.GetLineText(2));
    }

    [UITestMethod]
    public void DuplicateLine_ConsecutiveCalls_UndoRevertsEachIndependently()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("Line A\nLine B");
        coreTextbox.undoRedo.ClearAll();

        // Duplicate line 0 twice
        coreTextbox.DuplicateLine(0); // "Line A", "Line A", "Line B"
        coreTextbox.DuplicateLine(0); // "Line A", "Line A", "Line A", "Line B"

        Assert.AreEqual(4, coreTextbox.textManager.LinesCount);

        // Undo 1 reverts second duplicate
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();
        Assert.AreEqual(3, coreTextbox.textManager.LinesCount);

        // Undo 2 reverts first duplicate
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();
        Assert.AreEqual(2, coreTextbox.textManager.LinesCount);
        Assert.AreEqual("Line A", coreTextbox.GetLineText(0));
        Assert.AreEqual("Line B", coreTextbox.GetLineText(1));

        // Redo restores step by step
        coreTextbox.Redo();
        Assert.AreEqual(3, coreTextbox.textManager.LinesCount);

        coreTextbox.Redo();
        Assert.AreEqual(4, coreTextbox.textManager.LinesCount);
    }

    [UITestMethod]
    public void RemoveText_ConsecutiveBackspaces_CreateIndependentUndoSteps()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("ABCDE");
        coreTextbox.SetCursorPosition(0, 5);
        coreTextbox.undoRedo.ClearAll();

        // Two backspaces
        coreTextbox.textActionManager.RemoveText(); // removes 'E' -> "ABCD"
        coreTextbox.textActionManager.RemoveText(); // removes 'D' -> "ABC"

        Assert.AreEqual("ABC", coreTextbox.GetText());

        // Undo #1 restores 'D'
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();
        Assert.AreEqual("ABCD", coreTextbox.GetText());
        Assert.AreEqual(4, coreTextbox.cursorManager.CharacterPosition);

        // Undo #2 restores 'E'
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);
        coreTextbox.Undo();
        Assert.AreEqual("ABCDE", coreTextbox.GetText());
        Assert.AreEqual(5, coreTextbox.cursorManager.CharacterPosition);

        // Redo #1 re-deletes 'E'
        coreTextbox.Redo();
        Assert.AreEqual("ABCD", coreTextbox.GetText());

        // Redo #2 re-deletes 'D'
        coreTextbox.Redo();
        Assert.AreEqual("ABC", coreTextbox.GetText());
    }

    [UITestMethod]
    public void ReadOnlyMode_UndoAndRedoDoNothing()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox(addNewLines: 0);
        coreTextbox.SetText("Base Text");
        coreTextbox.SetCursorPosition(0, 9);
        coreTextbox.undoRedo.ClearAll();

        coreTextbox.textActionManager.AddCharacter(" Extra");
        Assert.AreEqual("Base Text Extra", coreTextbox.GetText());
        Assert.IsTrue(coreTextbox.undoRedo.CanUndo);

        // Enable read-only mode
        coreTextbox.IsReadOnly = true;

        // Calling Undo in read-only mode must NOT change text
        coreTextbox.Undo();
        Assert.AreEqual("Base Text Extra", coreTextbox.GetText());

        // Calling Redo in read-only mode must NOT change text
        coreTextbox.Redo();
        Assert.AreEqual("Base Text Extra", coreTextbox.GetText());

        // Disabling read-only mode allows Undo to work again
        coreTextbox.IsReadOnly = false;
        coreTextbox.Undo();
        Assert.AreEqual("Base Text", coreTextbox.GetText());
    }
}



