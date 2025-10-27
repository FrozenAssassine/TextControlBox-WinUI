using System;
using System.Diagnostics;
using TextControlBoxNS.Core;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Extensions;

namespace TextControlBoxNS.Test
{
    internal class TextTests : TestCase
    {
        public CoreTextControlBox coreTextbox;

        public TextTests(string name, CoreTextControlBox coreTextbox)
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

            //return [this.Test_20];
            return [
                this.Test_1,
                this.Test_2,
                this.Test_3,
                this.Test_4,
                this.Test_5,
                this.Test_6,
                this.Test_7,
                this.Test_8,
                this.Test_10,
                this.Test_11,
                this.Test_12,
                this.Test_13,
                this.Test_14,
                this.Test_15,
                this.Test_16,
                this.Test_17,
                this.Test_18,
                this.Test_19,
                this.Test_20,
                this.Test_21,
                this.Test_22,
                this.Test_23,
                this.Test_24,
                this.Test_30,
                this.Test_31,
                this.Test_32,
                this.Test_33,
                this.Test_34,
                this.Test_35,
                ];
        }


        public (bool undo, bool redo) CheckUndoRedo(int count = 1)
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

        public bool Test_1()
        {
            Debug.Write("Clear selection");

            Random r = new Random();
            coreTextbox.SetSelection(r.Next(0, 10), r.Next(10, 50));
            coreTextbox.ClearSelection();

            var sel = coreTextbox.selectionManager.currentTextSelection;
            bool res = sel.StartPosition.IsNull && sel.EndPosition.IsNull && !coreTextbox.selectionManager.HasSelection;

            coreTextbox.SetText(originalText); //reset content
            return res;
        }

        public bool Test_2()
        {
            Debug.Write("Delete Line 5");

            int linesBefore = coreTextbox.NumberOfLines;
            coreTextbox.textActionManager.DeleteLine(4);

            CheckUndoRedo();

            bool res = coreTextbox.NumberOfLines == linesBefore - 1;

            coreTextbox.SetText(originalText); //reset content
            return res;
        }

        public bool Test_3()
        {
            Debug.Write("Add Line 5");

            int linesBefore = coreTextbox.NumberOfLines;
            var sel = coreTextbox.textActionManager.AddLine(3, "Hello World this is the text of line 3");

            CheckUndoRedo();

            bool res = coreTextbox.NumberOfLines == linesBefore + 1 && coreTextbox.GetLineText(3).Equals("Hello World this is the text of line 3");

            coreTextbox.SetText(originalText); //reset content
            return res;
        }

        public bool Test_4()
        {
            Debug.Write("Add Character (single line text, no selection)");

            string textBefore = coreTextbox.GetLineText(3);
            string textToAdd = "Add single line character";

            coreTextbox.SetCursorPosition(3, 0);
            int lineBefore = coreTextbox.cursorManager.currentCursorPosition.LineNumber;
            coreTextbox.textActionManager.AddCharacter(coreTextbox.stringManager.CleanUpString(textToAdd));

            var cur = coreTextbox.cursorManager.currentCursorPosition;

            CheckUndoRedo();

            bool res =
                cur.CharacterPosition == textToAdd.Length &&
                cur.LineNumber == lineBefore &&
                coreTextbox.GetLineText(3).Equals(textToAdd + textBefore);

            coreTextbox.SetText(originalText); //reset content
            return res;
        }

        public bool Test_5()
        {
            Debug.Write("Add Character (multi line text, no selection)");

            string textToAdd = "Add Line 1\nAdd Line 2\nAdd Line 3";
            coreTextbox.SetCursorPosition(2, 10);
            int lineBefore = coreTextbox.cursorManager.currentCursorPosition.LineNumber;
            coreTextbox.textActionManager.AddCharacter(coreTextbox.stringManager.CleanUpString(textToAdd));

            CheckUndoRedo();

            var cur = coreTextbox.cursorManager.currentCursorPosition;
            bool res =
                cur.LineNumber == lineBefore + 2 &&
                coreTextbox.GetLineText(2).Substring(10, 10).Equals("Add Line 1") &&
                coreTextbox.GetLineText(3).Substring(0, 10).Equals("Add Line 2") &&
                coreTextbox.GetLineText(4).Substring(0, 10).Equals("Add Line 3") &&
                cur.CharacterPosition == 10;

            coreTextbox.SetText(originalText); //reset content
            return res;
        }

        public bool Test_6()
        {
            Debug.Write("Add Character (no text, no selection)");

            coreTextbox.SelectAll();
            coreTextbox.textActionManager.AddCharacter("");

            CheckUndoRedo();

            var cur = coreTextbox.cursorManager.currentCursorPosition;
            bool res =
                cur.LineNumber == 0 &&
                coreTextbox.GetText().Length == 0 &&
                cur.CharacterPosition == 0;

            coreTextbox.SetText(originalText); //reset content

            return res;
        }

        public bool Test_7()
        {
            Debug.Write("Add Character (add single line, single line selected)");

            coreTextbox.SetSelection(5, 10);
            coreTextbox.textActionManager.AddCharacter("Hello World");

            CheckUndoRedo();

            var cur = coreTextbox.cursorManager.currentCursorPosition;
            bool res =
                cur.LineNumber == 0 &&
                cur.CharacterPosition == 16;

            coreTextbox.SetText(originalText); //reset content

            return true;
        }

        public bool Test_8()
        {
            Debug.Write("Add Character (add multiline line, single line selected)");

            coreTextbox.SetSelection(5, 10);
            coreTextbox.textActionManager.AddCharacter(coreTextbox.stringManager.CleanUpString("Line1\nLine2\nLine3"));

            CheckUndoRedo();

            var cur = coreTextbox.cursorManager.currentCursorPosition;
            bool res =
                cur.LineNumber == 2 &&
                cur.CharacterPosition == 5;

            coreTextbox.SetText(originalText); //reset content

            return res;
        }

        public bool Test_10()
        {
            Debug.Write("Add Character (add multi line, multi line selection, everything selected)");

            coreTextbox.SelectAll();
            string text = coreTextbox.stringManager.CleanUpString("Line1\nLine2\nLine3");
            coreTextbox.textActionManager.AddCharacter(text);

            CheckUndoRedo();

            var cur = coreTextbox.cursorManager.currentCursorPosition;
            bool res =
                cur.LineNumber == 2 &&
                cur.CharacterPosition == 5 &&
                coreTextbox.GetText().Equals(text);

            coreTextbox.SetText(originalText); //reset content

            return res;
        }

        public bool Test_11()
        {
            Debug.Write("Delete Selection (multi line selection, whole lines)");

            coreTextbox.SelectLines(1, 3);
            coreTextbox.textActionManager.DeleteSelection();

            CheckUndoRedo();

            var lines = coreTextbox.textManager.totalLines;

            var cur = coreTextbox.cursorManager.currentCursorPosition;

            bool res =
                cur.LineNumber == 1 &&
                cur.CharacterPosition == 0 &&
                coreTextbox.GetLineText(1).Length == 0 && coreTextbox.GetLineText(0) == originalText.Split(coreTextbox.textManager.NewLineCharacter)[0];

            coreTextbox.SetText(originalText); //reset content

            return res;
        }

        public bool Test_12()
        {
            coreTextbox.SetSelection(4, 10);
            coreTextbox.textActionManager.DeleteSelection();

            CheckUndoRedo();

            string line1 = TestHelper.GetFirstLine(originalText, coreTextbox.textManager.NewLineCharacter).ToString();
            var expected = line1.Substring(0, 4) + line1.Substring(14);
            var cur = coreTextbox.cursorManager.currentCursorPosition;
            var t = coreTextbox.GetLineText(0);
            bool res =
                cur.LineNumber == 0 &&
                cur.CharacterPosition == 4 &&
                t.Trim().Equals(expected.Trim());

            coreTextbox.SetText(originalText); //reset content

            return res;
        }

        public bool Test_13()
        {
            Debug.Write("Delete Selection (single line selection, whole line)");

            coreTextbox.SelectLine(0);
            coreTextbox.textActionManager.DeleteSelection();
            CheckUndoRedo();

            var cur = coreTextbox.cursorManager.currentCursorPosition;
            bool res =
                cur.LineNumber == 0 &&
                cur.CharacterPosition == 0 &&
                coreTextbox.GetLineText(0) == originalText.Split(coreTextbox.textManager.NewLineCharacter)[1];

            coreTextbox.SetText(originalText); //reset content

            return res;
        }

        public bool Test_14()
        {
            Debug.Write("Delete Selection (lines selected completely (not whole text selected!))");
            int linesBefore = coreTextbox.textManager.LinesCount;

            coreTextbox.SetSelection(0, 0, 2, coreTextbox.GetLineText(2).Length);
            coreTextbox.textActionManager.DeleteSelection();

            CheckUndoRedo();

            var lineText = coreTextbox.GetLineText(0);
            bool res = coreTextbox.textManager.LinesCount == linesBefore - 2 &&
                       lineText == "";

            coreTextbox.SetText(originalText); // reset content
            return res;
        }

        public bool Test_15()
        {
            Debug.Write("Delete Selection (only start line fully selected)");
            int linesBefore = coreTextbox.textManager.LinesCount;

            coreTextbox.SetSelection(0, 0, 1, 5);
            coreTextbox.textActionManager.DeleteSelection();

            CheckUndoRedo();

            string expected = TestHelper.GetLineText(originalText, coreTextbox, 1).Substring(5);
            bool res = coreTextbox.GetLineText(0) == expected &&
                       coreTextbox.textManager.LinesCount == linesBefore - 1;

            coreTextbox.SetText(originalText); // reset content
            return res;
        }

        public bool Test_16()
        {
            Debug.Write("Delete Selection (only end line fully selected)");
            int linesBefore = coreTextbox.textManager.LinesCount;

            coreTextbox.SetSelection(1, 3, 2, coreTextbox.GetLineText(2).Length);
            coreTextbox.textActionManager.DeleteSelection();

            CheckUndoRedo();

            var text = coreTextbox.GetLineText(1);
            string expected = TestHelper.GetLineText(originalText, coreTextbox, 1).Substring(0, 3);

            bool res = text.Equals(expected) &&
                coreTextbox.textManager.LinesCount == linesBefore - 1;

            coreTextbox.SetText(originalText); // reset content
            return res;
        }

        public bool Test_17()
        {
            Debug.Write("Delete Selection (neither start nor end line fully selected)");
            int linesBefore = coreTextbox.textManager.LinesCount;

            coreTextbox.SetSelection(1, 2, 2, 4);
            coreTextbox.textActionManager.DeleteSelection();

            CheckUndoRedo();

            string expected = TestHelper.GetLineText(originalText, coreTextbox, 1).Substring(0, 2) + TestHelper.GetLineText(originalText, coreTextbox, 2).Substring(4);
            bool res = coreTextbox.GetLineText(1) == expected &&
                       coreTextbox.textManager.LinesCount == linesBefore - 1;

            coreTextbox.SetText(originalText); // reset content
            return res;
        }
        public bool Test_18()
        {
            Debug.Write("Duplicate Line");
            int linesBefore = coreTextbox.textManager.LinesCount;

            coreTextbox.SetCursorPosition(4, 10);
            coreTextbox.DuplicateLine(3);

            CheckUndoRedo();

            bool res = coreTextbox.GetLineText(3).Equals(coreTextbox.GetLineText(4)) &&
                       coreTextbox.textManager.LinesCount == linesBefore + 1 && coreTextbox.CursorPosition.LineNumber == 5 && coreTextbox.CursorPosition.CharacterPosition == 10;

            coreTextbox.SetText(originalText); // reset content
            return res;
        }

        public bool Test_19()
        {
            Debug.Write("Test Crash");

            coreTextbox.ClearUndoRedoHistory();

            coreTextbox.SetCursorPosition(4, 100); //end of text
            coreTextbox.textActionManager.AddNewLine();

            coreTextbox.textActionManager.AddCharacter("This is random text");

            coreTextbox.SelectAll();
            coreTextbox.textActionManager.DeleteSelection();

            coreTextbox.Undo();

            coreTextbox.Redo();
            coreTextbox.Undo();
            coreTextbox.Undo();
            coreTextbox.Undo();

            return true;
        }

        public bool Test_20()
        {
            Debug.Write("Test Crash 2");

            coreTextbox.ClearUndoRedoHistory();
            coreTextbox.SetText(originalText); // reset content

            coreTextbox.ClearUndoRedoHistory();
            coreTextbox.SetCursorPosition(4, 100); //end of text

            coreTextbox.SelectAll();
            coreTextbox.textActionManager.DeleteSelection();
            coreTextbox.Undo();
            coreTextbox.ClearSelection();

            for (int i = 0; i < 34; i++)
            {
                coreTextbox.textActionManager.RemoveText(true);
            }
            for (int i = 0; i < 34; i++)
            {
                coreTextbox.Undo();
            }
            for (int i = 0; i < 34; i++)
            {
                coreTextbox.Redo();
            }

            coreTextbox.Redo();

            return true;
        }

        public bool Test_21()
        {
            Debug.WriteLine("Load Lines empty array");

            coreTextbox.LoadLines([]);
            coreTextbox.LoadLines(null);

            return coreTextbox.textManager.totalLines.Count == 1;
        }

        public bool Test_22()
        {
            Debug.WriteLine("Load Text null + empty string");

            coreTextbox.LoadText(null);
            coreTextbox.LoadText("");

            return coreTextbox.textManager.totalLines.Count == 1;
        }

        public bool Test_23()
        {
            Debug.WriteLine("Load Text null + empty string");

            coreTextbox.SetText(null);
            coreTextbox.SetText("");

            return coreTextbox.textManager.totalLines.Count == 1;
        }

        public bool Test_24()
        {
            Debug.WriteLine("Move selection with Tab");
            coreTextbox.SetText(originalText); // reset content

            coreTextbox.SetCursorPosition(0, 0);
            coreTextbox.SetSelection(0, 40); //whole text

            for (int i = 0; i < 3; i++)
                coreTextbox.tabSpaceHelper.MoveTab();

            coreTextbox.Undo();
            coreTextbox.Undo();
            coreTextbox.Undo();

            if (coreTextbox.Text == originalText)
                return true;

            return false;
        }

        public bool Test_30()
        {
            Debug.WriteLine("Surround Selection");
            coreTextbox.SetText(originalText); // reset content

            coreTextbox.SetCursorPosition(0, 0);
            coreTextbox.SetSelection(0, 87); //whole text

            coreTextbox.SurroundSelectionWith("<div>", "</div>");
            return true;
        }

        public bool Test_31()
        {
            Debug.WriteLine("Add Lines at index 3");

            coreTextbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");
            string textBefore = coreTextbox.GetText();

            coreTextbox.AddLines(3, ["Hello", "Baum", "Nudel", "Kuchen", "Wurst"]);

            var res = CheckUndoRedo();
            coreTextbox.Undo();

            return res.undo && res.redo && textBefore.Equals(coreTextbox.GetText());
        }

        public bool Test_32()
        {
            Debug.WriteLine("Add Lines at beginning");

            coreTextbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");
            string textBefore = coreTextbox.GetText();

            coreTextbox.AddLines(0, ["Hello", "Baum", "Nudel", "Kuchen", "Wurst"]);

            var res = CheckUndoRedo();
            coreTextbox.Undo();

            return res.undo && res.redo && textBefore.Equals(coreTextbox.GetText());
        }

        public bool Test_33()
        {
            Debug.WriteLine("Add Lines at end");

            coreTextbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");
            string textBefore = coreTextbox.GetText();

            coreTextbox.AddLines(4, ["Hello", "Baum", "Nudel", "Kuchen", "Wurst"]);

            var res = CheckUndoRedo();
            coreTextbox.Undo();

            return res.undo && res.redo && textBefore.Equals(coreTextbox.GetText());
        }


        public bool Test_34()
        {
            Debug.WriteLine("Undo Grouping");

            coreTextbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");
            string textBefore = coreTextbox.GetText();

            coreTextbox.BeginActionGroup();

            coreTextbox.DeleteLine(3);
            coreTextbox.AddLine(3, "New");
            coreTextbox.SetLineText(1, "Edit");
            coreTextbox.AddLines(4, ["Hello", "Baum", "Nudel", "Kuchen", "Wurst"]);

            coreTextbox.EndActionGroup();

            var res = CheckUndoRedo();
            coreTextbox.Undo();

            return res.undo && res.redo && textBefore.Equals(coreTextbox.GetText());
        }
        public bool Test_35()
        {
            Debug.WriteLine("Undo Grouping Extended");

            coreTextbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");
            string textBefore = coreTextbox.GetText();

            coreTextbox.BeginActionGroup();

            coreTextbox.DeleteLine(3);
            coreTextbox.DeleteLine(0);
            coreTextbox.AddLine(3, "New");
            coreTextbox.SetLineText(1, "Edit");

            for(int i = 0; i<10; i++)
            {
                coreTextbox.AddLines(4, ["Hello", "Baum", "Nudel", "Kuchen", "Wurst"]);
            }

            for(int i =5; i<20; i++)
            {
                coreTextbox.DeleteLine(i);
            }

            coreTextbox.EndActionGroup();

            var res = CheckUndoRedo();
            coreTextbox.Undo();

            return res.undo && res.redo && textBefore.Equals(coreTextbox.GetText());
        }
    }
}