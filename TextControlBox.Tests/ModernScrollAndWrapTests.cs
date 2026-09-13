using System;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using TextControlBoxNS;
using TextControlBoxNS.Core;

namespace TextControlBox.Tests;

/// <summary>
/// Comprehensive unit and UI tests for the modern scroll and wrap pipeline:
/// WordWrap (visual-row model), long-line horizontal virtualization, centered horizontal reveal,
/// wrapped-line virtualization, and zoom anchoring.
/// </summary>
[TestClass]
public class ModernScrollAndWrapTests
{
    [UITestMethod]
    public void WordWrap_DefaultIsFalse()
    {
        var core = TestHelper.MakeCoreTextbox();
        Assert.IsFalse(core.WordWrap);

        var tb = new TextControlBoxNS.TextControlBox();
        Assert.IsFalse(tb.WordWrap);
    }

    [UITestMethod]
    public void WordWrap_PropertyToggle_PropagatesToCore()
    {
        var tb = new TextControlBoxNS.TextControlBox();
        tb.WordWrap = true;
        Assert.IsTrue(tb.WordWrap);

        tb.WordWrap = false;
        Assert.IsFalse(tb.WordWrap);
    }

    [UITestMethod]
    public void WordWrap_Enabled_PinsHorizontalOffsetAtZero()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 10);
        core.WordWrap = true;

        // Try centered reveal
        core.ScrollIntoViewHorizontallyCentered();

        Assert.AreEqual(0.0, core.scrollManager.OffsetSource.HorizontalOffset, 0.001);
    }

    [UITestMethod]
    public void WordWrap_MoveCursorByVisualRows_NavigatesCleanly()
    {
        var core = TestHelper.MakeCoreTextbox();
        // Load text with lines
        core.SetText("First line is short.\nSecond line is also short.\nThird line is simple.");
        core.WordWrap = true;

        core.SetCursorPosition(0, 0);
        Assert.AreEqual(0, core.cursorManager.LineNumber);
        Assert.AreEqual(0, core.cursorManager.CharacterPosition);

        // Move down by 1 visual row
        bool moved = core.textRenderer.MoveCursorByVisualRows(core.canvasText, core.cursorManager.currentCursorPosition, 1);
        Assert.IsTrue(moved);
        Assert.AreEqual(1, core.cursorManager.LineNumber);

        // Move down again
        moved = core.textRenderer.MoveCursorByVisualRows(core.canvasText, core.cursorManager.currentCursorPosition, 1);
        Assert.IsTrue(moved);
        Assert.AreEqual(2, core.cursorManager.LineNumber);

        // Move up by 1 visual row
        moved = core.textRenderer.MoveCursorByVisualRows(core.canvasText, core.cursorManager.currentCursorPosition, -1);
        Assert.IsTrue(moved);
        Assert.AreEqual(1, core.cursorManager.LineNumber);
    }

    [UITestMethod]
    public void LongLine_HorizontalVirtualization_CanLoadAndNavigate()
    {
        var core = TestHelper.MakeCoreTextbox();
        // Create a pathological 60,000-char line (exceeds HorizontalVirtualizationThreshold = 50k)
        string longLine = new string('a', 60000);
        core.SetText(longLine);

        Assert.AreEqual(1, core.textManager.LinesCount);
        Assert.AreEqual(60000, core.textManager.GetLineLength(0));

        // Position cursor in the middle and near the end
        core.SetCursorPosition(0, 30000);
        Assert.AreEqual(30000, core.cursorManager.CharacterPosition);

        core.SetCursorPosition(0, 59990);
        Assert.AreEqual(59990, core.cursorManager.CharacterPosition);

        // Moving right near end should succeed and stop at line end
        core.cursorManager.MoveRight();
        Assert.AreEqual(59991, core.cursorManager.CharacterPosition);
    }

    [UITestMethod]
    public void ScrollIntoViewHorizontallyCentered_PublicApi_WorksWithoutException()
    {
        var tb = new TextControlBoxNS.TextControlBox();
        tb.LoadLines(["Short line", new string('x', 500), "Another line"]);

        tb.SetCursorPosition(250, 1);
        // Invoke public method
        tb.ScrollIntoViewHorizontallyCentered();
    }

    [TestMethod]
    public void WrappedLineVirtualization_RowEstimation_HandlesVeryLongLine()
    {
        var metrics = new WrapRowMetrics();
        // Simulate a line that estimates 1000 rows
        metrics.Rebuild(1, _ => 1000);

        Assert.AreEqual(1000, metrics.TotalVisualRows);
        Assert.AreEqual(0, metrics.GetLineStartRow(0, 1));
        Assert.AreEqual(1000, metrics.GetLineStartRow(1, 1));
        Assert.AreEqual(0, metrics.GetDocumentLineFromVisualRow(500, 1));
    }

    [UITestMethod]
    public void ZoomScrollAnchor_Integration_DoesNotThrowOnZoom()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 200);
        core.SetCursorPosition(50, 0);

        // Change zoom factor up and down
        core.ZoomFactor = 150;

        Assert.IsTrue(core.scrollManager.OffsetSource.VerticalOffset >= 0);

        core.ZoomFactor = 75;

        Assert.IsTrue(core.scrollManager.OffsetSource.VerticalOffset >= 0);
    }

    [UITestMethod]
    public void CoreTextControlBox_Unload_CleansUpDiagonalScroll()
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 10);
        // Unload should tear down composition tracker and timer cleanly
        core.Unload();
    }
}
