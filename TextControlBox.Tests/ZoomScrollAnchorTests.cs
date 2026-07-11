using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextControlBoxNS.Core;

namespace TextControlBox.Tests;

/// <summary>Unit tests for the pure zoom-anchor arithmetic (<see cref="ZoomScrollAnchor"/>).</summary>
[TestClass]
public class ZoomScrollAnchorTests
{
    [TestMethod]
    public void AnchorVerticalOffset_KeepsCentreRowStationary()
    {
        // Viewport 200, old line height 20 => centre pixel 100+? offset 100 => centre pixel 200 => row 10.
        // After line height 40, row 10 must remain at the viewport centre.
        double oldLineHeight = 20, newLineHeight = 40, viewport = 200, offset = 100;
        double centreRow = (offset + viewport / 2) / oldLineHeight; // (100 + 100)/20 = 10
        double result = ZoomScrollAnchor.AnchorVerticalOffset(offset, viewport, oldLineHeight, newLineHeight);
        // Row 10 at new height => centre pixel 10*40 = 400; offset = 400 - 100 = 300.
        Assert.AreEqual(300, result, 0.001);
        // The centre row is preserved: (result + viewport/2)/newLineHeight == centreRow.
        Assert.AreEqual(centreRow, (result + viewport / 2) / newLineHeight, 0.001);
    }

    [TestMethod]
    public void AnchorVerticalOffset_ClampsAtZero()
    {
        // Zooming out (smaller line height) near the top must not produce a negative offset.
        double result = ZoomScrollAnchor.AnchorVerticalOffset(verticalOffset: 5, viewportHeight: 200, oldLineHeight: 40, newLineHeight: 10);
        Assert.IsTrue(result >= 0);
        Assert.AreEqual(0, result, 0.001); // centre row ~2.6 * 10 - 100 < 0 => clamped
    }

    [TestMethod]
    public void AnchorHorizontalOffset_ScalesByFontRatio()
    {
        Assert.AreEqual(150, ZoomScrollAnchor.AnchorHorizontalOffset(horizontalOffset: 100, oldFontSize: 10, newFontSize: 15), 0.001);
        Assert.AreEqual(50, ZoomScrollAnchor.AnchorHorizontalOffset(horizontalOffset: 100, oldFontSize: 20, newFontSize: 10), 0.001);
    }

    [TestMethod]
    public void AnchorHorizontalOffset_UnusableOldSize_ReturnsInput()
    {
        Assert.AreEqual(42, ZoomScrollAnchor.AnchorHorizontalOffset(horizontalOffset: 42, oldFontSize: 0, newFontSize: 15), 0.001);
    }
}
