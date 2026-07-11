using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextControlBoxNS.Core;

namespace TextControlBox.Tests;

/// <summary>
/// Pure conversions between the pixel scroll seam (IScrollOffsetSource) and the legacy ScrollBar
/// backing store. These are the exact place a "scroll is N× too fast/slow" regression would hide,
/// so they are unit-tested in isolation (a ScrollBar cannot be instantiated headless).
/// </summary>
[TestClass]
public class ScrollOffsetMathTests
{
    [TestMethod]
    public void NormalizeSensitivity_FloorsAtOne()
    {
        Assert.AreEqual(1, ScrollOffsetMath.NormalizeSensitivity(0));
        Assert.AreEqual(1, ScrollOffsetMath.NormalizeSensitivity(-5));
        Assert.AreEqual(4, ScrollOffsetMath.NormalizeSensitivity(4));
    }

    [TestMethod]
    public void VerticalValueToPixels_MultipliesBySensitivity()
    {
        Assert.AreEqual(400.0, ScrollOffsetMath.VerticalValueToPixels(100, 4));
        Assert.AreEqual(100.0, ScrollOffsetMath.VerticalValueToPixels(100, 1));
    }

    [TestMethod]
    public void PixelsToVerticalValue_DividesBySensitivity()
    {
        Assert.AreEqual(100.0, ScrollOffsetMath.PixelsToVerticalValue(400, 4));
        Assert.AreEqual(100.0, ScrollOffsetMath.PixelsToVerticalValue(100, 1));
    }

    [TestMethod]
    public void PixelsToVerticalValue_ClampsNegativeToZero()
    {
        Assert.AreEqual(0.0, ScrollOffsetMath.PixelsToVerticalValue(-500, 4));
    }

    [TestMethod]
    public void VerticalOffset_RoundTripsThroughScrollBarUnits()
    {
        // A pixel offset stored as a legacy ScrollBar.Value and read back must be unchanged.
        const int sensitivity = 4;
        foreach (double pixels in new[] { 0.0, 20.0, 399.0, 4096.0, 1_000_000.0 })
        {
            double value = ScrollOffsetMath.PixelsToVerticalValue(pixels, sensitivity);
            double roundTrip = ScrollOffsetMath.VerticalValueToPixels(value, sensitivity);
            Assert.AreEqual(pixels, roundTrip, 1e-9);
        }
    }

    [TestMethod]
    public void VerticalExtent_ConvertsBetweenMaximumAndPixels()
    {
        const int sensitivity = 4;
        const double viewport = 300.0;

        // Content taller than the viewport: Maximum (legacy units) + viewport (px) == extent (px).
        double extent = ScrollOffsetMath.VerticalMaximumToExtentPixels(500, sensitivity, viewport);
        Assert.AreEqual(500 * sensitivity + viewport, extent);

        double maximum = ScrollOffsetMath.VerticalExtentPixelsToMaximum(extent, sensitivity, viewport);
        Assert.AreEqual(500.0, maximum, 1e-9);
    }

    [TestMethod]
    public void VerticalExtentPixelsToMaximum_FloorsAtZeroWhenContentFitsViewport()
    {
        // Content shorter than the viewport is not scrollable → Maximum 0.
        Assert.AreEqual(0.0, ScrollOffsetMath.VerticalExtentPixelsToMaximum(100, 4, 300));
    }

    [TestMethod]
    public void HorizontalOffset_ClampsNegativeToZero_AndIsPixelIdentity()
    {
        Assert.AreEqual(0.0, ScrollOffsetMath.ClampHorizontalOffset(-1));
        Assert.AreEqual(42.0, ScrollOffsetMath.ClampHorizontalOffset(42));
    }

    [TestMethod]
    public void HorizontalExtent_ConvertsBetweenMaximumAndPixels()
    {
        const double viewport = 250.0;
        double extent = ScrollOffsetMath.HorizontalMaximumToExtentPixels(1000, viewport);
        Assert.AreEqual(1000 + viewport, extent);

        double maximum = ScrollOffsetMath.HorizontalExtentPixelsToMaximum(extent, viewport);
        Assert.AreEqual(1000.0, maximum, 1e-9);

        // Content narrower than the viewport → not scrollable.
        Assert.AreEqual(0.0, ScrollOffsetMath.HorizontalExtentPixelsToMaximum(100, viewport));
    }
}
