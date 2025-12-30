using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TextControlBoxNS.Core;

namespace TextControlBox.Tests
{
    internal class TestHelper
    {
        public static (CoreTextControlBox coreTextBox, string text) MakeCoreTextboxWithText()
        {
            var core = MakeCoreTextbox();
            return (core, core.GetText());
        }

        public static TextControlBoxNS.TextControlBox MakeTextbox(int addNewLines = 100)
        {
            TextControlBoxNS.TextControlBox textbox = new TextControlBoxNS.TextControlBox();

            if (addNewLines == 0)
            {
                textbox.LoadLines([]);
            }
            textbox.LoadLines(Enumerable.Range(0, addNewLines).Select(x => "Line " + x + " is cool right?"));
            return textbox;
        }


        public static IEnumerable<string> MakeLines(int count)
        {
            return Enumerable.Range(0, count).Select(x => "Line " + x + " is cool right?");
        }

        public static CoreTextControlBox MakeCoreTextbox(int addNewLines = 100)
        {
            CoreTextControlBox textbox = new CoreTextControlBox();
            textbox.InitialiseOnStart();
            if (addNewLines == 0)
            {
                textbox.LoadLines([]);
            }
            textbox.LoadLines(TestHelper.MakeLines(addNewLines));
            return textbox;
        }
        public static (bool undo, bool redo) CheckUndoRedo(CoreTextControlBox textbox, Action action, int count = 1)
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
        public static string GetLineText(string text, CoreTextControlBox coreTextBox, int line)
        {
            return text.Split(coreTextBox.textManager.NewLineCharacter)[line];
        }
        public static ReadOnlySpan<char> GetFirstLine(string text, string newLineCharacter)
        {
            var span = text.AsSpan();
            int startIndex = span.IndexOf(newLineCharacter);
            if (startIndex == -1)
                return span;

            return span.Slice(0, startIndex + newLineCharacter.Length);
        }
    }
}
