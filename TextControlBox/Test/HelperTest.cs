using Collections.Pooled;
using System;
using System.Collections;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TextControlBoxNS.Core;
using TextControlBoxNS.Core.Text;
using Windows.ApplicationModel.Activation;

namespace TextControlBoxNS.Test
{
    internal class HelperTest : TestCase
    {
        private CoreTextControlBox coreTextbox;
        public HelperTest (CoreTextControlBox coreTextbox)
        {
            this.coreTextbox = coreTextbox;
            this.name = "Test Helpers";
        }

        public override string name { get; set; }

        public override Func<bool>[] GetAllTests()
        {
            return [
                this.Test_1,
                this.Test_2,
                this.Test_3,
                this.Test_4,
                ];
        }

        private bool Test_1()
        {
            Debug.Write("Test LineEndings.CleanLineEndings");
            string original_LF = "Line1\nLine2";
            string original_CR = "Line1\rLine2";
            string original_CRLF = "Line1\r\nLine2";

            bool[] res =
            [
                LineEndings.CleanLineEndings(original_LF, LineEnding.CR) == original_CR,
                LineEndings.CleanLineEndings(original_LF, LineEnding.CRLF) == original_CRLF,
                LineEndings.CleanLineEndings(original_CRLF, LineEnding.LF) == original_LF,
                LineEndings.CleanLineEndings(original_CRLF, LineEnding.CR) == original_CR,
                LineEndings.CleanLineEndings(original_CR, LineEnding.LF) == original_LF,
                LineEndings.CleanLineEndings(original_CR, LineEnding.CRLF) == original_CRLF,
            ];

            return res.All(x => x);
        }

        private bool Test_2()
        {
            Debug.Write("Test stringManager.CleanUpString LineEndings");

            string original_LF = "Line1\nLine2";
            string original_CR = "Line1\rLine2";
            string original_CRLF = "Line1\r\nLine2";

            bool[] res = new bool[6];

            coreTextbox.LineEnding = LineEnding.CRLF;
            res[0] = coreTextbox.stringManager.CleanUpString(original_LF) == original_CRLF;
            coreTextbox.LineEnding = LineEnding.CRLF;
            res[1] = coreTextbox.stringManager.CleanUpString(original_CR) == original_CRLF;

            coreTextbox.LineEnding = LineEnding.LF;
            res[2] = coreTextbox.stringManager.CleanUpString(original_CRLF) == original_LF;
            coreTextbox.LineEnding = LineEnding.LF;
            res[3] = coreTextbox.stringManager.CleanUpString(original_CR) == original_LF;

            coreTextbox.LineEnding = LineEnding.CR;
            res[4] = coreTextbox.stringManager.CleanUpString(original_CRLF) == original_CR;
            coreTextbox.LineEnding = LineEnding.CR;
            res[5] = coreTextbox.stringManager.CleanUpString(original_LF) == original_CR;

            coreTextbox.LineEnding = LineEnding.CRLF;
            return res.All(x => x);
        }

        private bool Test_3()
        {
            (string text, int longest) MakeText(LineEnding lineEnding)
            {
                int longest = Random.Shared.Next(0, 200);
                StringBuilder sb = new StringBuilder();
                string le = LineEndings.LineEndingToString(lineEnding);
                for (int i = 0; i < 200; i++)
                {
                    if (i == longest)
                        sb.Append($"This is the longest line with a more content {i * 100} {le}");
                    else
                        sb.Append($"This is the content of line {i} {le}");
                }
                return (sb.ToString(), longest);
            }

            var c1 = MakeText(LineEnding.LF);
            bool res1 = coreTextbox.longestLineManager.GetLongestLineLength(c1.text) == c1.text.Split("\n")[c1.longest].Length;

            var c2 = MakeText(LineEnding.CR);
            bool res2 = coreTextbox.longestLineManager.GetLongestLineLength(c2.text) == c2.text.Split("\r")[c2.longest].Length;

            var c3 = MakeText(LineEnding.CRLF);
            bool res3 = coreTextbox.longestLineManager.GetLongestLineLength(c3.text) == c3.text.Split("\r\n")[c3.longest].Length;

            return res1 && res2 && res3;
        }

        private bool Test_4()
        {
            PooledList<string> list = new();
            list.AddRange(Enumerable.Range(0, 100).Select(i => $"Line {i} is an awesome line!"));

            int longest = Random.Shared.Next(0, 99);
            list.Insert(longest, "This is the longest line of the text. At least it should be!");

            int longestIndex = coreTextbox.longestLineManager.GetLongestLineIndex(list);
            return longestIndex == longest;
        }
    }
}
