using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Xaml.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;

namespace TextControlBox.Tests.RegressionTests;

[TestClass]
public class SelectionRenderingTests
{
    //The selection box sat a fixed 2px (scaled by zoom) below the line top, which was too high for the
    //drawn text: descenders like "_" or "j" stuck out below the selection while there was room above.
    [UITestMethod]
    public void SelectionBox_IsVerticallyCenteredOnTheDrawnText()
    {
        foreach (string font in new[] { "Consolas", "Cascadia Mono", "Courier New" })
        {
            foreach (int zoom in new[] { 50, 100, 150, 250 })
            {
                var core = TestHelper.MakeCoreTextbox(3);
                core.FontFamily = new FontFamily(font);
                core.ZoomFactor = zoom;
                core.textRenderer.EnsureTextFormat();

                float lineHeight = core.textRenderer.SingleLineHeight;
                float textOffsetY = lineHeight - core.textRenderer.TextVerticalAdjustment;

                using var layout = new CanvasTextLayout(CanvasDevice.GetSharedDevice(), "Xj_gÅ|", core.textRenderer.TextFormat, 10000, 10000);
                var ink = layout.DrawBounds;
                double inkTop = textOffsetY + ink.Y;
                double inkBottom = textOffsetY + ink.Y + ink.Height;

                //line 0 without scrolling: the selection covers [margin, margin + lineHeight]
                float boxTop = core.textRenderer.GetSelectionTopMargin();
                float boxBottom = boxTop + lineHeight;

                double spaceAbove = inkTop - boxTop;
                double spaceBelow = boxBottom - inkBottom;
                Assert.IsTrue(System.Math.Abs(spaceAbove - spaceBelow) <= 1.01,
                    $"{font} {zoom}%: selection [{boxTop}, {boxBottom}] is not centered on the text ink [{inkTop:0.00}, {inkBottom:0.00}]");
            }
        }
    }
}
