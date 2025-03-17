using System;
using System.Collections.Generic;
using System.Diagnostics;
using TextControlBoxNS.Core;

namespace TextControlBoxNS.Test
{
    internal class TestHelper
    {
        public CoreTextControlBox coreTextbox;

        List<TestCase> TestCases;

        public TestHelper(CoreTextControlBox coreTextbox)
        {
            this.coreTextbox = coreTextbox;
            TestCases = [ new HelperTest(coreTextbox), new TextTests("Text Tests", coreTextbox), new EndUserFunctionsTest("End User Functions", coreTextbox), new UndoRedoTests("Undo Reddo Test", coreTextbox) ];
        }

        public void Evaluate()
        {
            foreach (var test in TestCases)
            {
                Debug.WriteLine("\n");
                Debug.WriteLine(test.name);
                test.Evaluate();
            }
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
