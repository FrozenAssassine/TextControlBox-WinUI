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
            originalText = coreTextbox.GetText();

            //return [this.Test_7];
            return [
                this.Test_1,
                this.Test_2,
                this.Test_3,
                this.Test_4,
                this.Test_5,
                this.Test_6,
                this.Test_7,
                this.Test_8,
                this.Test_9,
                this.Test_10,
                this.Test_11,
                this.Test_12,
                this.Test_13,
                this.Test_14,
                this.Test_15,
                this.Test_16,
                this.Test_17,
                this.Test_18,
                ];
        }


        public (bool undo, bool redo) CheckUndoRedo()
        {
            string textBefore = coreTextbox.GetText();

            coreTextbox.Undo();
            string textAfter = coreTextbox.GetText();

            bool undoRes = !textAfter.Equals(textBefore);

            coreTextbox.Redo();
            bool redoRes = coreTextbox.GetText().Equals(textBefore);

            Debug.Write($" (Undo: {undoRes} Redo:{redoRes})");
            Debug.Assert(undoRes && redoRes);

            return (undoRes, redoRes);
        }

        public bool Test_1()
        {
            Debug.Write("Clear selection");

            Random r = new Random();
            coreTextbox.SetSelection(r.Next(0, 10), r.Next(10, 50));
            coreTextbox.ClearSelection();

            var sel = coreTextbox.selectionManager.currentTextSelection;
            return sel.StartPosition.IsNull && sel.EndPosition.IsNull && !coreTextbox.selectionManager.HasSelection;
        }

        public bool Test_2()
        {
            Debug.Write("Delete Line 5");

            int linesBefore = coreTextbox.NumberOfLines;
            coreTextbox.textActionManager.DeleteLine(5);

            CheckUndoRedo();

            return coreTextbox.NumberOfLines == linesBefore - 1;
        }

        public bool Test_3()
        {
            Debug.Write("Add Line 5");

            int linesBefore = coreTextbox.NumberOfLines;
            var sel = coreTextbox.textActionManager.AddLine(5, "Hello World this is the text of line 5");

            CheckUndoRedo();

            return coreTextbox.NumberOfLines == linesBefore + 1 && coreTextbox.GetLineText(5).Equals("Hello World this is the text of line 5");
        }

        public bool Test_4()
        {
            Debug.Write("Add Character (single line text, no selection)");

            string textBefore = coreTextbox.GetLineText(10);
            string textToAdd = "Add single line character";

            coreTextbox.SetCursorPosition(10, 0);
            coreTextbox.textActionManager.AddCharacter(coreTextbox.stringManager.CleanUpString(textToAdd));

            var cur = coreTextbox.cursorManager.currentCursorPosition;

            CheckUndoRedo();

            return
                cur.CharacterPosition == textToAdd.Length && 
                cur.LineNumber == 10 &&
                coreTextbox.GetLineText(10).Equals(textToAdd + textBefore);
        }

        public bool Test_5()
        {
            Debug.Write("Add Character (multi line text, no selection)");

            string textToAdd = "Add Line 1\nAdd Line 2\nAdd Line 3";
            coreTextbox.SetCursorPosition(10, 10);
            coreTextbox.textActionManager.AddCharacter(coreTextbox.stringManager.CleanUpString(textToAdd));

            CheckUndoRedo();

            var cur = coreTextbox.cursorManager.currentCursorPosition;
            return
                cur.LineNumber == 12 &&
                coreTextbox.GetLineText(10).Substring(10, 10).Equals("Add Line 1") &&
                coreTextbox.GetLineText(11).Substring(0, 10).Equals("Add Line 2") &&
                coreTextbox.GetLineText(12).Substring(0, 10).Equals("Add Line 3") &&
                cur.CharacterPosition == 10;
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
                cur.CharacterPosition == 11;

            coreTextbox.SetText(originalText); //reset content

            return res;
        }

        public bool Test_9()
        {
            Debug.Write("Add Character (add multiline line, single line selected)");

            coreTextbox.SetSelection(5, 10);
            coreTextbox.textActionManager.AddCharacter(coreTextbox.stringManager.CleanUpString("Line1\nLine2\nLine3"));
            
            CheckUndoRedo();

            var cur = coreTextbox.cursorManager.currentCursorPosition;
            bool res =
                cur.LineNumber == 2 &&
                cur.CharacterPosition == 11;

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

            coreTextbox.SelectLines(5, 3);
            coreTextbox.textActionManager.DeleteSelection();

            CheckUndoRedo();

            var cur = coreTextbox.cursorManager.currentCursorPosition;
            bool res =
                cur.LineNumber == 5 &&
                cur.CharacterPosition == 0 &&
                coreTextbox.GetLineText(5).Length == 0 && coreTextbox.GetLineText(6) == originalText.Split(coreTextbox.textManager.NewLineCharacter)[9];

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
                coreTextbox.GetLineText(0).Length == 0;

            coreTextbox.SetText(originalText); //reset content

            return res;
        }

        public bool Test_14()
        {
            Debug.Write("Delete Selection (lines selected completely (not whole text selected!))");

            coreTextbox.SetSelection(0, 0, 2, coreTextbox.GetLineText(2).Length);
            coreTextbox.textActionManager.DeleteSelection();

            CheckUndoRedo();

            var lineText = coreTextbox.GetLineText(0);
            bool res = coreTextbox.textManager.LinesCount == 18 &&
                       lineText == "";

            coreTextbox.SetText(originalText); // reset content
            return res;
        }

        public bool Test_15()
        {
            Debug.Write("Delete Selection (only start line fully selected)");

            coreTextbox.SetSelection(0, 0, 1, 5);
            coreTextbox.textActionManager.DeleteSelection();

            CheckUndoRedo();

            string expected = TestHelper.GetLineText(originalText, coreTextbox, 1).Substring(5);
            bool res = coreTextbox.GetLineText(0) == expected &&
                       coreTextbox.textManager.LinesCount == 19;

            coreTextbox.SetText(originalText); // reset content
            return res;
        }

        public bool Test_16()
        {
            Debug.Write("Delete Selection (only end line fully selected)");

            coreTextbox.SetSelection(1, 3, 2, coreTextbox.GetLineText(2).Length);
            coreTextbox.textActionManager.DeleteSelection();

            CheckUndoRedo();
            
            var text = coreTextbox.GetLineText(1);
            string expected = TestHelper.GetLineText(originalText, coreTextbox, 1).Substring(0, 3);

            bool res = text.Equals(expected) && 
                coreTextbox.textManager.LinesCount == 19;

            coreTextbox.SetText(originalText); // reset content
            return res;
        }

        public bool Test_17()
        {
            Debug.Write("Delete Selection (neither start nor end line fully selected)");

            coreTextbox.SetSelection(1, 2, 2, 4);
            coreTextbox.textActionManager.DeleteSelection();

            CheckUndoRedo();

            string expected = TestHelper.GetLineText(originalText, coreTextbox, 1).Substring(0, 2) + TestHelper.GetLineText(originalText, coreTextbox, 2).Substring(4);
            bool res = coreTextbox.GetLineText(1) == expected &&
                       coreTextbox.textManager.LinesCount == 19;

            coreTextbox.SetText(originalText); // reset content
            return res;
        }
        public bool Test_18()
        {
            Debug.Write("Duplicate Line");

            coreTextbox.SetCursorPosition(6, 10);
            coreTextbox.DuplicateLine(6);

            CheckUndoRedo();

            bool res = coreTextbox.GetLineText(6).Equals(coreTextbox.GetLineText(7)) &&
                       coreTextbox.textManager.LinesCount == 21 && coreTextbox.CursorPosition.LineNumber == 7 && coreTextbox.CursorPosition.CharacterPosition == 10;

            coreTextbox.SetText(originalText); // reset content
            return res;
        }

        public bool Test_19()
        {
            Debug.Write("Duplicate Current Line");

            int lineNbr = coreTextbox.CursorPosition.LineNumber;
            int charPos = coreTextbox.CursorPosition.CharacterPosition;
            int linesBef = coreTextbox.textManager.LinesCount;

            for(int i = 0; i< 10; i++)
            {
                coreTextbox.DuplicateCurrentLine();
            }

            CheckUndoRedo();

            bool res = coreTextbox.GetLineText(lineNbr).Equals(coreTextbox.GetLineText(lineNbr + 10)) &&
                       coreTextbox.textManager.LinesCount == linesBef + 10 && coreTextbox.CursorPosition.LineNumber == lineNbr + 10 && coreTextbox.CursorPosition.CharacterPosition == charPos;

            coreTextbox.SetText(originalText); // reset content
            return res;
        }
    }
}
