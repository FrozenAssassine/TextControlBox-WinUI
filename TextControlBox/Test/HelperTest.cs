using System;
using System.Diagnostics;
using System.Linq;
using TextControlBoxNS.Core;
using TextControlBoxNS.Core.Text;

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
    }
}
