using Microsoft.VisualStudio.TestTools.UnitTesting;
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


        public static void AssertHighlightExists(CoreTextControlBox coreTextbox, string expectedColor)
        {
            // Get all highlights from the textbox
            var highlights = coreTextbox.SyntaxHighlighting.Highlights;

            // Check if any highlight has the expected color
            bool colorExists = highlights.Any(h =>
                h.ColorDark.Equals(expectedColor, System.StringComparison.OrdinalIgnoreCase));

            Assert.IsTrue(colorExists,
                $"Expected color '{expectedColor}' was not found in the highlights. " +
                $"Available colors: {string.Join(", ", highlights.Select(h => h.Color).Distinct())}");
        }

        /// <summary>
        /// Asserts that a specific highlight color exists at a specific position
        /// </summary>
        /// <param name="coreTextbox">The textbox to check</param>
        /// <param name="expectedColor">The expected color</param>
        /// <param name="startIndex">The start index of the expected highlight</param>
        /// <param name="length">The length of the expected highlight</param>
        public static void AssertHighlightExistsAt(CoreTextControlBox coreTextbox, string expectedColor, int startIndex, int length)
        {
            var highlights = coreTextbox.SyntaxHighlighting.Highlights;

            bool highlightExists = highlights.Any(h =>
                h.Color.Equals(expectedColor, System.StringComparison.OrdinalIgnoreCase) &&
                h.StartIndex == startIndex &&
                h.Length == length);

            Assert.IsTrue(highlightExists,
                $"Expected color '{expectedColor}' at position {startIndex} with length {length} was not found.");
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
