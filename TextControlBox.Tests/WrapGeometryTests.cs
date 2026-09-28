using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextControlBoxNS.Core;

namespace TextControlBox.Tests;

/// <summary>Unit tests for the pure word-wrap pointer geometry (<see cref="WrapGeometry"/>).</summary>
[TestClass]
public class WrapGeometryTests
{
    // singleLineHeight = 20 => topInset = 20 (SingleLineHeight).
    private const float LineHeight = 20f;
    private const int Sensitivity = 4;

    [TestMethod]
    public void CalculateVisualRowFromPointY_AddsStartVisualRow()
    {
        // y = 0 -> relative row 0; startVisualRow 10 -> 10.
        Assert.AreEqual(10, WrapGeometry.CalculateVisualRowFromPointY(0, 10, LineHeight, Sensitivity));
    }

    [TestMethod]
    public void CalculateVisualRowFromPointY_CountsRowsBelowInset()
    {
        // y = 2*20 = 40 -> relative row 2; startVisualRow 3 -> 5.
        Assert.AreEqual(5, WrapGeometry.CalculateVisualRowFromPointY(40, 3, LineHeight, Sensitivity));
    }

    [TestMethod]
    public void CalculateVisualRowFromPointY_AboveInset_ClampsToStartRow()
    {
        Assert.AreEqual(7, WrapGeometry.CalculateVisualRowFromPointY(-10, 7, LineHeight, Sensitivity));
    }

    [TestMethod]
    public void CalculateWrappedLineHitTestY_WithinLine_ReturnsRelativeY()
    {
        // lineTopY = 40, y = 60, topInset = 0 => 60 - 40 = 20 (row 1 of the line).
        float hit = WrapGeometry.CalculateWrappedLineHitTestYFromPointY(60, 40, LineHeight, Sensitivity, wrappedRowCount: 3);
        Assert.AreEqual(20f, hit, 0.001f);
    }

    [TestMethod]
    public void CalculateWrappedLineHitTestY_BelowLastRow_ClampsToExtent()
    {
        // A 2-row line spans [0, 40); a click well below clamps just under 40.
        float hit = WrapGeometry.CalculateWrappedLineHitTestYFromPointY(1000, 0, LineHeight, Sensitivity, wrappedRowCount: 2);
        Assert.IsTrue(hit < 40f && hit > 39f, $"expected just under 40, got {hit}");
    }

    [TestMethod]
    public void CalculateWrappedLineHitTestY_AboveLine_ClampsToZero()
    {
        float hit = WrapGeometry.CalculateWrappedLineHitTestYFromPointY(0, 100, LineHeight, Sensitivity, wrappedRowCount: 2);
        Assert.AreEqual(0f, hit);
    }
}
