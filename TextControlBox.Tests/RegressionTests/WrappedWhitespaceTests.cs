using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;

namespace TextControlBox.Tests.RegressionTests;

[TestClass]
public class WrappedWhitespaceTests
{
    private const float WrapWidth = 300f;

    private static CanvasTextLayout CreateWrappedLayout(string text)
    {
        var core = TestHelper.MakeCoreTextbox(1);
        core.textLayoutManager.WordWrap = true;
        core.textRenderer.EnsureTextFormat();
        using var format = core.textLayoutManager.CreateCanvasTextFormat();
        return core.textLayoutManager.CreateTextLayout(CanvasDevice.GetSharedDevice(), format, text, WrapWidth, 10000);
    }

    //Pressing tab in front of text until it wrapped made DirectWrite let the indent hang past the wrap width: the
    //text moved to the next row once, but every further tab only stretched the row above into nothing.
    [UITestMethod]
    public void LeadingTabs_WrapAcrossRows_InsteadOfHangingPastTheWidth()
    {
        float previousTextY = float.MinValue;
        foreach (int tabs in new[] { 10, 20, 40, 80 })
        {
            string text = new string('\t', tabs) + "hello";
            using var layout = CreateWrappedLayout(text);

            for (int i = 0; i <= text.Length; i++)
            {
                float x = layout.GetCaretPosition(i, false).X;
                Assert.IsTrue(x <= WrapWidth + 0.5f, $"{tabs} tabs: caret {i} at x={x} hangs past the wrap width {WrapWidth}");
            }

            float textY = layout.GetCaretPosition(tabs, false).Y;
            Assert.IsTrue(textY >= previousTextY, $"{tabs} tabs: text moved up (y={textY}, before {previousTextY})");
            previousTextY = textY;
        }

        using var few = CreateWrappedLayout(new string('\t', 20) + "hello");
        using var many = CreateWrappedLayout(new string('\t', 80) + "hello");
        Assert.IsTrue(many.GetCaretPosition(80, false).Y > few.GetCaretPosition(20, false).Y, "more indentation must push the text further down");
    }

    [UITestMethod]
    public void LongSpaceRun_InMiddleOfLine_Wraps()
    {
        string text = "abc" + new string(' ', 300) + "def";
        using var layout = CreateWrappedLayout(text);
        for (int i = 0; i <= text.Length; i++)
        {
            float x = layout.GetCaretPosition(i, false).X;
            Assert.IsTrue(x <= WrapWidth + 0.5f, $"caret {i} at x={x} hangs past the wrap width {WrapWidth}");
        }
        Assert.IsTrue(layout.GetCaretPosition(text.Length - 3, false).Y > layout.GetCaretPosition(0, false).Y);
    }

    [UITestMethod]
    public void NormalWordWrap_IsUnchanged()
    {
        string text = "lorem ipsum dolor sit amet consectetur adipiscing elit sed do eiusmod tempor incididunt ut labore";
        var core = TestHelper.MakeCoreTextbox(1);
        core.textLayoutManager.WordWrap = true;
        using var format = core.textLayoutManager.CreateCanvasTextFormat();
        using var plain = new CanvasTextLayout(CanvasDevice.GetSharedDevice(), text, format, WrapWidth, 10000);
        using var fixedLayout = core.textLayoutManager.CreateTextLayout(CanvasDevice.GetSharedDevice(), format, text, WrapWidth, 10000);
        for (int i = 0; i <= text.Length; i++)
        {
            Assert.AreEqual(plain.GetCaretPosition(i, false), fixedLayout.GetCaretPosition(i, false), $"caret {i} moved");
        }
    }
}
