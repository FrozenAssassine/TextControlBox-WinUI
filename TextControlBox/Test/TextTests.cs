using System;
using System.Diagnostics;
using TextControlBoxNS.Core;
using TextControlBoxNS.Core.Text;

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

        public override Func<bool>[] GetAllTests()
        {
            return [
                this.Test_1,
                this.Test_2,
                this.Test_3,
                this.Test_4,
                this.Test_5,
                this.Test_6,
                ];
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
            return coreTextbox.NumberOfLines == linesBefore - 1;
        }

        public bool Test_3()
        {
            Debug.Write("Add Line 5");

            int linesBefore = coreTextbox.NumberOfLines;

            var sel = coreTextbox.textActionManager.AddLine(5, "Hello World this is the text of line 5");

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

            var cur = coreTextbox.cursorManager.currentCursorPosition;
            return
                cur.LineNumber == 1 && 
                coreTextbox.GetText().Length == 0 && 
                cur.CharacterPosition == 0;
        }
    }
}
