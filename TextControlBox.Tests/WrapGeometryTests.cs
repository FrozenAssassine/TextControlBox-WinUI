using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextControlBoxNS.Core;

namespace TextControlBox.Tests;

/// <summary>Unit tests for the pure word-wrap pointer geometry (<see cref="WrapGeometry"/>).</summary>
[TestClass]
public class WrapGeometryTests
{
    // singleLineHeight = 20, sensitivity = 4 => topInset = 5.
    private const float LineHeight = 20f;
    private const int Sensitivity = 4;

    [TestMethod]
    public void CalculateVisualRowFromPointY_AddsStartVisualRow()
    {
        // y = 5 (== topInset) -> relative row 0; startVisualRow 10 -> 10.
        Assert.AreEqual(10, WrapGeometry.CalculateVisualRowFromPointY(5, 10, LineHeight, Sensitivity));
    }

    [TestMethod]
    public void CalculateVisualRowFromPointY_CountsRowsBelowInset()
    {
        // y = 5 + 2*20 = 45 -> relative row 2; startVisualRow 3 -> 5.
        Assert.AreEqual(5, WrapGeometry.CalculateVisualRowFromPointY(45, 3, LineHeight, Sensitivity));
    }

    [TestMethod]
    public void CalculateVisualRowFromPointY_AboveInset_ClampsToStartRow()
    {
        Assert.AreEqual(7, WrapGeometry.CalculateVisualRowFromPointY(0, 7, LineHeight, Sensitivity));
    }

    [TestMethod]
    public void CalculateWrappedLineHitTestY_WithinLine_ReturnsRelativeY()
    {
        // lineTopY = 40, y = 65, topInset = 5 => 65 - 40 - 5 = 20 (row 1 of the line).
        float hit = WrapGeometry.CalculateWrappedLineHitTestYFromPointY(65, 40, LineHeight, Sensitivity, wrappedRowCount: 3);
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
