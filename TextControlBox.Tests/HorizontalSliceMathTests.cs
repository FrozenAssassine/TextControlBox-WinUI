using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextControlBoxNS.Core;

namespace TextControlBox.Tests;

/// <summary>
/// Unit tests for the pure horizontal-virtualization arithmetic (<see cref="HorizontalSliceMath"/>). These
/// cover the slice-window computation, the reuse "safe zone" check, and the document-column ↔ rendered-index
/// mapping — the places where an off-by-one silently misplaces the caret or selection on very long lines.
/// </summary>
[TestClass]
public class HorizontalSliceMathTests
{
    [TestMethod]
    public void VisibleCharRange_AtOrigin_StartsAtZeroWithRightPadding()
    {
        // charWidth 10 => 800px viewport spans 80 chars; +100 right padding, left padding clamped at 0.
        var (start, end) = HorizontalSliceMath.VisibleCharRange(horizontalScrollPixels: 0, viewportWidthPixels: 800, charWidth: 10);
        Assert.AreEqual(0, start);
        Assert.AreEqual(0 + 80 + 100, end);
    }

    [TestMethod]
    public void VisibleCharRange_Scrolled_AppliesLeftPadding()
    {
        // scroll 1000px / 10 = column 100, minus 50 left padding => 50.
        var (start, end) = HorizontalSliceMath.VisibleCharRange(horizontalScrollPixels: 1000, viewportWidthPixels: 800, charWidth: 10);
        Assert.AreEqual(50, start);
        Assert.AreEqual(50 + 80 + 100, end);
    }

    [TestMethod]
    public void VisibleCharRange_ZeroCharWidth_DoesNotDivideByZero()
    {
        var (start, end) = HorizontalSliceMath.VisibleCharRange(horizontalScrollPixels: 20, viewportWidthPixels: 30, charWidth: 0);
        // charWidth falls back to 1: 20 - 50 clamped to 0; end = 0 + 30 + 100.
        Assert.AreEqual(0, start);
        Assert.AreEqual(130, end);
    }

    [TestMethod]
    public void CanReuseWindow_InsideSafeZone_True()
    {
        Assert.IsTrue(HorizontalSliceMath.CanReuseWindow(visibleStart: 60, visibleEnd: 140, safeStart: 50, safeEnd: 200));
    }

    [TestMethod]
    public void CanReuseWindow_ScrolledBeforeStart_False()
    {
        Assert.IsFalse(HorizontalSliceMath.CanReuseWindow(visibleStart: 40, visibleEnd: 140, safeStart: 50, safeEnd: 200));
    }

    [TestMethod]
    public void CanReuseWindow_ScrolledPastEnd_False()
    {
        Assert.IsFalse(HorizontalSliceMath.CanReuseWindow(visibleStart: 60, visibleEnd: 210, safeStart: 50, safeEnd: 200));
    }

    [TestMethod]
    public void ComputeWindow_BuffersEitherSideAndRecordsSafeZone()
    {
        // visible [100, 200) => 100 chars, buffer = 300.
        var (sliceStart, sliceLen, safeStart, safeEnd) = HorizontalSliceMath.ComputeWindow(visibleStart: 100, visibleEnd: 200);
        Assert.AreEqual(0, sliceStart);                              // max(0, 100 - 300) = 0
        Assert.AreEqual(100 + 300 * 2, sliceLen);                    // 700
        Assert.AreEqual(100, safeStart);                             // inner safe zone start = visibleStart
        Assert.AreEqual(200 + 300, safeEnd);                         // visibleEnd + buffer
    }

    [TestMethod]
    public void ComputeWindow_FarScroll_SliceStartIsPositive()
    {
        // visible [1000, 1100) => 100 chars, buffer = 300 => sliceStart = 700.
        var (sliceStart, sliceLen, safeStart, safeEnd) = HorizontalSliceMath.ComputeWindow(visibleStart: 1000, visibleEnd: 1100);
        Assert.AreEqual(700, sliceStart);
        Assert.AreEqual(700, sliceLen);
        Assert.AreEqual(1000, safeStart);
        Assert.AreEqual(1400, safeEnd);
    }

    [TestMethod]
    public void ComputeWindow_ReuseCheckHoldsForItsOwnSafeZone()
    {
        // A freshly computed window's safe zone must contain the viewport that produced it.
        var (_, _, safeStart, safeEnd) = HorizontalSliceMath.ComputeWindow(visibleStart: 500, visibleEnd: 600);
        Assert.IsTrue(HorizontalSliceMath.CanReuseWindow(500, 600, safeStart, safeEnd));
    }

    [TestMethod]
    public void SlicedLineLength_LineInsideWindow_ReturnsRemainderCappedAtWindow()
    {
        // Window [200, 200+500). A 400-char line contributes 400-200 = 200.
        Assert.AreEqual(200, HorizontalSliceMath.SlicedLineLength(fullLineLength: 400, sliceStart: 200, sliceLen: 500));
    }

    [TestMethod]
    public void SlicedLineLength_LongLine_CapsAtWindowWidth()
    {
        // Window width 500; a 100000-char line contributes only the window's worth.
        Assert.AreEqual(500, HorizontalSliceMath.SlicedLineLength(fullLineLength: 100000, sliceStart: 200, sliceLen: 500));
    }

    [TestMethod]
    public void SlicedLineLength_LineEndsBeforeWindow_ReturnsZero()
    {
        Assert.AreEqual(0, HorizontalSliceMath.SlicedLineLength(fullLineLength: 150, sliceStart: 200, sliceLen: 500));
    }

    [TestMethod]
    public void SlicedLineLength_ZeroWindow_ReturnsZero()
    {
        Assert.AreEqual(0, HorizontalSliceMath.SlicedLineLength(fullLineLength: 400, sliceStart: 0, sliceLen: 0));
    }

    [TestMethod]
    public void RenderedIndexForColumn_SubtractsSliceStart()
    {
        int slicedLen = HorizontalSliceMath.SlicedLineLength(fullLineLength: 1000, sliceStart: 200, sliceLen: 500);
        Assert.AreEqual(100, HorizontalSliceMath.RenderedIndexForColumn(characterPosition: 300, sliceStart: 200, slicedLineLength: slicedLen));
    }

    [TestMethod]
    public void RenderedIndexForColumn_ClampsBelowZeroAndAboveSlice()
    {
        Assert.AreEqual(0, HorizontalSliceMath.RenderedIndexForColumn(characterPosition: 10, sliceStart: 200, slicedLineLength: 500));
        Assert.AreEqual(500, HorizontalSliceMath.RenderedIndexForColumn(characterPosition: 999999, sliceStart: 200, slicedLineLength: 500));
    }

    [TestMethod]
    public void ColumnForRenderedIndex_AddsSliceStart()
    {
        Assert.AreEqual(300, HorizontalSliceMath.ColumnForRenderedIndex(renderedIndex: 100, sliceStart: 200, fullLineLength: 1000));
    }

    [TestMethod]
    public void ColumnForRenderedIndex_ClampsToLineLength()
    {
        Assert.AreEqual(1000, HorizontalSliceMath.ColumnForRenderedIndex(renderedIndex: 999999, sliceStart: 200, fullLineLength: 1000));
    }

    [TestMethod]
    public void ColumnAndRenderedIndex_RoundTripInsideSlice()
    {
        const int sliceStart = 200;
        const int fullLine = 1000;
        int slicedLen = HorizontalSliceMath.SlicedLineLength(fullLine, sliceStart, sliceLen: 500);
        for (int column = sliceStart; column <= sliceStart + slicedLen; column += 37)
        {
            int rendered = HorizontalSliceMath.RenderedIndexForColumn(column, sliceStart, slicedLen);
            int roundTrip = HorizontalSliceMath.ColumnForRenderedIndex(rendered, sliceStart, fullLine);
            Assert.AreEqual(column, roundTrip);
        }
    }
}
