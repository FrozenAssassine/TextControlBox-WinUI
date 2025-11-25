using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextControlBoxNS.Core;
using TextControlBoxNS.Core.Text;
using Collections.Pooled;
using System;
using System.Linq;
using System.Text;
using TextControlBoxNS;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;

namespace TextControlBox.Tests;

[TestClass]
public class HelperTests
{
    private CoreTextControlBox CreateCore()
    {
        var core = new CoreTextControlBox();
        core.InitialiseOnStart();
        return core;
    }

    [UITestMethod]
    public void LineEndings_CleanLineEndings_Works()
    {
        string original_LF = "Line1\nLine2";
        string original_CR = "Line1\rLine2";
        string original_CRLF = "Line1\r\nLine2";

        Assert.AreEqual(original_CR, LineEndings.CleanLineEndings(original_LF, LineEnding.CR));
        Assert.AreEqual(original_CRLF, LineEndings.CleanLineEndings(original_LF, LineEnding.CRLF));
        Assert.AreEqual(original_LF, LineEndings.CleanLineEndings(original_CRLF, LineEnding.LF));
        Assert.AreEqual(original_CR, LineEndings.CleanLineEndings(original_CRLF, LineEnding.CR));
        Assert.AreEqual(original_LF, LineEndings.CleanLineEndings(original_CR, LineEnding.LF));
        Assert.AreEqual(original_CRLF, LineEndings.CleanLineEndings(original_CR, LineEnding.CRLF));
    }

    [UITestMethod]
    public void StringManager_CleanUpString_LineEndings()
    {
        var core = CreateCore();

        string original_LF = "Line1\nLine2";
        string original_CR = "Line1\rLine2";
        string original_CRLF = "Line1\r\nLine2";

        core.LineEnding = LineEnding.CRLF;
        Assert.AreEqual(original_CRLF, core.stringManager.CleanUpString(original_LF));
        Assert.AreEqual(original_CRLF, core.stringManager.CleanUpString(original_CR));

        core.LineEnding = LineEnding.LF;
        Assert.AreEqual(original_LF, core.stringManager.CleanUpString(original_CRLF));
        Assert.AreEqual(original_LF, core.stringManager.CleanUpString(original_CR));

        core.LineEnding = LineEnding.CR;
        Assert.AreEqual(original_CR, core.stringManager.CleanUpString(original_CRLF));
        Assert.AreEqual(original_CR, core.stringManager.CleanUpString(original_LF));

        core.LineEnding = LineEnding.CRLF;
    }

    [UITestMethod]
    public void LongestLineManager_GetLongestLineLength_Works()
    {
        var core = CreateCore();

        (string text, int longest) MakeText(LineEnding lineEnding)
        {
            int longest = new Random().Next(0, 200);
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
        Assert.AreEqual(c1.text.Split('\n')[c1.longest].Length, core.longestLineManager.GetLongestLineLength(c1.text));

        var c2 = MakeText(LineEnding.CR);
        Assert.AreEqual(c2.text.Split('\r')[c2.longest].Length, core.longestLineManager.GetLongestLineLength(c2.text));

        var c3 = MakeText(LineEnding.CRLF);
        Assert.AreEqual(c3.text.Split(new string[] { "\r\n" }, StringSplitOptions.None)[c3.longest].Length, core.longestLineManager.GetLongestLineLength(c3.text));
    }

    [UITestMethod]
    public void LongestLineManager_GetLongestLineIndex_Works()
    {
        var core = CreateCore();

        PooledList<string> list = new();
        list.AddRange(Enumerable.Range(0, 100).Select(i => $"Line {i} is an awesome line!"));

        int longest = new Random().Next(0, 99);
        list.Insert(longest, "This is the longest line of the text. At least it should be!");

        int longestIndex = core.longestLineManager.GetLongestLineIndex(list);
        Assert.AreEqual(longest, longestIndex);
    }
}
