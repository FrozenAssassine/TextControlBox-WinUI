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
        var coreTextbox = TestHelper.MakeCoreTextbox();

        string original_LF = "Line1\nLine2";
        string original_CR = "Line1\rLine2";
        string original_CRLF = "Line1\r\nLine2";

        coreTextbox.LineEnding = LineEnding.CRLF;
        Assert.AreEqual(original_CRLF, coreTextbox.stringManager.CleanUpString(original_LF));
        Assert.AreEqual(original_CRLF, coreTextbox.stringManager.CleanUpString(original_CR));

        coreTextbox.LineEnding = LineEnding.LF;
        Assert.AreEqual(original_LF, coreTextbox.stringManager.CleanUpString(original_CRLF));
        Assert.AreEqual(original_LF, coreTextbox.stringManager.CleanUpString(original_CR));

        coreTextbox.LineEnding = LineEnding.CR;
        Assert.AreEqual(original_CR, coreTextbox.stringManager.CleanUpString(original_CRLF));
        Assert.AreEqual(original_CR, coreTextbox.stringManager.CleanUpString(original_LF));

        coreTextbox.LineEnding = LineEnding.CRLF;
    }

    [UITestMethod]
    public void LongestLineManager_GetLongestLineLength_Works()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();

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
        Assert.AreEqual(c1.text.Split('\n')[c1.longest].Length, coreTextbox.longestLineManager.GetLongestLineLength(c1.text));

        var c2 = MakeText(LineEnding.CR);
        Assert.AreEqual(c2.text.Split('\r')[c2.longest].Length, coreTextbox.longestLineManager.GetLongestLineLength(c2.text));

        var c3 = MakeText(LineEnding.CRLF);
        Assert.AreEqual(c3.text.Split(new string[] { "\r\n" }, StringSplitOptions.None)[c3.longest].Length, coreTextbox.longestLineManager.GetLongestLineLength(c3.text));
    }

    [UITestMethod]
    public void LongestLineManager_GetLongestLineIndex_Works()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();

        PooledList<string> list = new();
        list.AddRange(Enumerable.Range(0, 100).Select(i => $"Line {i} is an awesome line!"));

        int longest = new Random().Next(0, 99);
        list.Insert(longest, "This is the longest line of the text. At least it should be!");

        int longestIndex = coreTextbox.longestLineManager.GetLongestLineIndex(list);
        Assert.AreEqual(longest, longestIndex);
    }

    [UITestMethod]
    public void Utils_ConvertTheme_ResolvesCorrectly()
    {
        Assert.AreEqual(Microsoft.UI.Xaml.ApplicationTheme.Light, TextControlBoxNS.Helper.Utils.ConvertTheme(Microsoft.UI.Xaml.ElementTheme.Light));
        Assert.AreEqual(Microsoft.UI.Xaml.ApplicationTheme.Dark, TextControlBoxNS.Helper.Utils.ConvertTheme(Microsoft.UI.Xaml.ElementTheme.Dark));
        Assert.AreEqual(Microsoft.UI.Xaml.ApplicationTheme.Dark, TextControlBoxNS.Helper.Utils.ConvertTheme(Microsoft.UI.Xaml.ElementTheme.Default, Microsoft.UI.Xaml.ElementTheme.Dark));
        Assert.AreEqual(Microsoft.UI.Xaml.ApplicationTheme.Light, TextControlBoxNS.Helper.Utils.ConvertTheme(Microsoft.UI.Xaml.ElementTheme.Default, Microsoft.UI.Xaml.ElementTheme.Light));
    }

    [UITestMethod]
    public void DesignHelper_ThemeAndDesignChanges_UpdateStateAndInvalidateLayout()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();

        // Switching theme to Dark
        coreTextbox.RequestedTheme = Microsoft.UI.Xaml.ElementTheme.Dark;
        Assert.AreEqual(Microsoft.UI.Xaml.ElementTheme.Dark, coreTextbox.RequestedTheme);
        Assert.IsFalse(coreTextbox.designHelper.ColorResourcesCreated);
        Assert.IsTrue(coreTextbox.textRenderer.NeedsUpdateTextLayout);

        // Switching theme to Light
        coreTextbox.RequestedTheme = Microsoft.UI.Xaml.ElementTheme.Light;
        Assert.AreEqual(Microsoft.UI.Xaml.ElementTheme.Light, coreTextbox.RequestedTheme);
        Assert.IsFalse(coreTextbox.designHelper.ColorResourcesCreated);
        Assert.IsTrue(coreTextbox.textRenderer.NeedsUpdateTextLayout);

        // Setting a custom design
        var customDesign = new TextControlBoxDesign(
            coreTextbox.designHelper.DarkDesign.Background,
            Windows.UI.Color.FromArgb(255, 12, 34, 56),
            Windows.UI.Color.FromArgb(255, 0, 0, 255),
            Windows.UI.Color.FromArgb(255, 255, 255, 255),
            Windows.UI.Color.FromArgb(50, 100, 100, 100),
            Windows.UI.Color.FromArgb(255, 100, 100, 100),
            Windows.UI.Color.FromArgb(0, 0, 0, 0),
            Windows.UI.Color.FromArgb(100, 160, 80, 0),
            Windows.UI.Color.FromArgb(180, 100, 100, 100)
        );

        coreTextbox.Design = customDesign;
        Assert.AreEqual(Windows.UI.Color.FromArgb(255, 12, 34, 56), coreTextbox.TextColor);
        Assert.IsFalse(coreTextbox.designHelper.ColorResourcesCreated);
        Assert.IsTrue(coreTextbox.textRenderer.NeedsUpdateTextLayout);

        // Resetting design to null (uses default theme design)
        coreTextbox.Design = null;
        Assert.IsFalse(coreTextbox.designHelper.ColorResourcesCreated);
        Assert.IsTrue(coreTextbox.textRenderer.NeedsUpdateTextLayout);
        Assert.AreEqual(coreTextbox.designHelper.LightDesign.TextColor, coreTextbox.TextColor);
    }
}
