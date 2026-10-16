using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextControlBoxNS.Core;

namespace TextControlBox.Tests;

/// <summary>Unit tests for the pure zoom-anchor arithmetic (<see cref="ZoomScrollAnchor"/>).</summary>
[TestClass]
public class ZoomScrollAnchorTests
{
    [TestMethod]
    public void AnchorVerticalOffset_KeepsTopRowStationary()
    {
        // Old line height 20, vertical offset 100 => top row is 5.
        // After line height 40, top row must remain 5 => offset 200.
        double oldLineHeight = 20, newLineHeight = 40, offset = 100;
        double result = ZoomScrollAnchor.AnchorVerticalOffset(offset, oldLineHeight, newLineHeight);
        Assert.AreEqual(200, result, 0.001);

        // Directly anchor row 150 across various line heights:
        Assert.AreEqual(3000, ZoomScrollAnchor.AnchorVerticalOffset(topRow: 150, newLineHeight: 20), 0.001);
        Assert.AreEqual(4500, ZoomScrollAnchor.AnchorVerticalOffset(topRow: 150, newLineHeight: 30), 0.001);
        Assert.AreEqual(6000, ZoomScrollAnchor.AnchorVerticalOffset(topRow: 150, newLineHeight: 40), 0.001);
    }

    [TestMethod]
    public void AnchorVerticalOffset_ClampsAtZero()
    {
        // Offset 0 stays 0 across any zoom change.
        double result = ZoomScrollAnchor.AnchorVerticalOffset(verticalOffset: 0, oldLineHeight: 40, newLineHeight: 10);
        Assert.AreEqual(0, result, 0.001);

        double negResult = ZoomScrollAnchor.AnchorVerticalOffset(verticalOffset: -10, oldLineHeight: 40, newLineHeight: 10);
        Assert.AreEqual(0, negResult, 0.001);

        double negRowResult = ZoomScrollAnchor.AnchorVerticalOffset(topRow: -5, newLineHeight: 30);
        Assert.AreEqual(0, negRowResult, 0.001);
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
