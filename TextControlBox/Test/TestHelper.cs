using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TextControlBoxNS.Core;

namespace TextControlBoxNS.Test
{
    internal class TestHelper
    {
        public CoreTextControlBox coreTextbox;
        private readonly TextControlBox textbox;
        List<TestCase> TestCases;

        public TestHelper(CoreTextControlBox coreTextbox, TextControlBox textbox)
        {
            this.textbox = textbox;
            this.coreTextbox = coreTextbox;
            TestCases = [ new HelperTest(coreTextbox), new TextTests("Text Tests", coreTextbox), new EndUserFunctionsTest("End User Functions", coreTextbox, textbox), new UndoRedoTests("Undo Reddo Test", coreTextbox) ];
        }

        public static void ResetContent(TextControlBox textbox, int addNewLines = 100)
        {
            if(addNewLines == 0)
            {
                textbox.LoadLines([]);
            }

            textbox.LoadLines(Enumerable.Range(0, addNewLines).Select(x => "Line " + x + " is cool right?"));
        }

        public void Evaluate()
        {
            foreach (var test in TestCases)
            {
                Debug.WriteLine("\n");
                Debug.WriteLine(test.name);
                test.Evaluate();
            }

            int totalFailed = 0;
            int totalTests = 0;

            foreach (var test in TestCases)
            {
                totalTests += test.totalTests;
                totalFailed += test.failRate;
            }

            Debug.WriteLine("Result");
            Debug.WriteLine($"{totalFailed}/{totalTests} Failed");

            if (totalFailed == 0)
                Debug.WriteLine("All tests passed!");
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

        public static IEnumerable<string> MakeLines(int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return $"Line {i} is cool";
            }
        }
    }
}
