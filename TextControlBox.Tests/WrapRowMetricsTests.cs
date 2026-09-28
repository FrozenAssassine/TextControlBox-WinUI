using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextControlBoxNS.Core;

namespace TextControlBox.Tests;

/// <summary>
/// Unit tests for the pure word-wrap visual-row bookkeeping (<see cref="WrapRowMetrics"/>): the prefix-sum
/// mapping between document lines and visual rows, and its incremental update — the arithmetic where an
/// off-by-one silently corrupts wrap-mode scrolling or caret placement.
/// </summary>
[TestClass]
public class WrapRowMetricsTests
{
    // Document with per-line wrapped row counts: line0=1, line1=3, line2=1, line3=2.  Total visual rows = 7.
    // Cumulative start rows: [0, 1, 4, 5, 7].
    private static WrapRowMetrics BuildSample(out int[] rows)
    {
        rows = new[] { 1, 3, 1, 2 };
        int[] local = rows;
        var m = new WrapRowMetrics();
        m.Rebuild(local.Length, i => local[i]);
        return m;
    }

    [TestMethod]
    public void Rebuild_ComputesTotalRowsAndPerLineCounts()
    {
        var m = BuildSample(out var rows);
        Assert.AreEqual(7, m.TotalVisualRows);
        for (int i = 0; i < rows.Length; i++)
            Assert.AreEqual(rows[i], m.GetRowCount(i));
    }

    [TestMethod]
    public void GetLineStartRow_IsCumulativePrefix()
    {
        var m = BuildSample(out _);
        Assert.AreEqual(0, m.GetLineStartRow(0, 4));
        Assert.AreEqual(1, m.GetLineStartRow(1, 4));
        Assert.AreEqual(4, m.GetLineStartRow(2, 4));
        Assert.AreEqual(5, m.GetLineStartRow(3, 4));
        Assert.AreEqual(7, m.GetLineStartRow(4, 4)); // end sentinel == total rows
    }

    [TestMethod]
    public void IsValidFor_MatchesLineCount()
    {
        var m = BuildSample(out _);
        Assert.IsTrue(m.IsValidFor(4));
        Assert.IsFalse(m.IsValidFor(3));
        Assert.IsFalse(m.IsValidFor(5));
    }

    [TestMethod]
    public void GetDocumentLineFromVisualRow_MapsEveryRowToItsOwningLine()
    {
        var m = BuildSample(out _);
        // rows: line0=[0], line1=[1,2,3], line2=[4], line3=[5,6]
        var expected = new[] { 0, 1, 1, 1, 2, 3, 3 };
        for (int row = 0; row < expected.Length; row++)
            Assert.AreEqual(expected[row], m.GetDocumentLineFromVisualRow(row, 4), $"row {row}");
    }

    [TestMethod]
    public void GetDocumentLineFromVisualRow_ClampsOutOfRange()
    {
        var m = BuildSample(out _);
        Assert.AreEqual(0, m.GetDocumentLineFromVisualRow(-5, 4));
        Assert.AreEqual(3, m.GetDocumentLineFromVisualRow(999, 4));
    }

    [TestMethod]
    public void GetRenderedVisualRowCount_SpansTheGivenLines()
    {
        var m = BuildSample(out _);
        Assert.AreEqual(4, m.GetRenderedVisualRowCount(0, 2, 4)); // lines 0..1 => 1 + 3
        Assert.AreEqual(4, m.GetRenderedVisualRowCount(1, 2, 4)); // lines 1..2 => 3 + 1
        Assert.AreEqual(7, m.GetRenderedVisualRowCount(0, 4, 4)); // whole doc
    }

    [TestMethod]
    public void ApplyIncremental_PatchesRowCountAndShiftsPrefix()
    {
        var m = BuildSample(out var rows);
        // line1 now wraps to 5 rows instead of 3 (delta +2). Total 7 -> 9.
        rows[1] = 5;
        bool patched = m.ApplyIncremental(4, new[] { 1 }, 1, 64, i => rows[i]);
        Assert.IsTrue(patched);
        Assert.AreEqual(5, m.GetRowCount(1));
        Assert.AreEqual(9, m.TotalVisualRows);
        Assert.AreEqual(6, m.GetLineStartRow(2, 4)); // 1 + 5
        Assert.AreEqual(7, m.GetLineStartRow(3, 4));
        Assert.AreEqual(9, m.GetLineStartRow(4, 4));
    }

    [TestMethod]
    public void ApplyIncremental_MatchesRebuildAfterEdit()
    {
        var m = BuildSample(out var rows);
        rows[2] = 4;
        m.ApplyIncremental(4, new[] { 2 }, 1, 64, i => rows[i]);

        var fresh = new WrapRowMetrics();
        fresh.Rebuild(4, i => rows[i]);

        Assert.AreEqual(fresh.TotalVisualRows, m.TotalVisualRows);
        for (int row = 0; row < fresh.TotalVisualRows; row++)
            Assert.AreEqual(fresh.GetDocumentLineFromVisualRow(row, 4), m.GetDocumentLineFromVisualRow(row, 4), $"row {row}");
    }

    [TestMethod]
    public void ApplyIncremental_ReturnsFalse_WhenTooManyDirtyLines()
    {
        var m = BuildSample(out var rows);
        var dirty = Enumerable.Range(0, 4).ToArray();
        Assert.IsFalse(m.ApplyIncremental(4, dirty, dirty.Length, remeasureLimit: 2, i => rows[i]));
    }

    [TestMethod]
    public void ApplyIncremental_ReturnsFalse_WhenCacheStale()
    {
        var m = BuildSample(out var rows);
        Assert.IsFalse(m.ApplyIncremental(5, new[] { 0 }, 1, 64, i => i < rows.Length ? rows[i] : 1));
    }

    [TestMethod]
    public void Clear_ResetsToSingleRow()
    {
        var m = BuildSample(out _);
        m.Clear();
        Assert.AreEqual(1, m.TotalVisualRows);
        Assert.IsFalse(m.IsValidFor(4));
    }

    [TestMethod]
    public void MeasureRows_BelowOne_IsClampedToOne()
    {
        var m = new WrapRowMetrics();
        m.Rebuild(3, _ => 0); // pathological measurer
        Assert.AreEqual(3, m.TotalVisualRows); // each line at least 1 row
        Assert.AreEqual(1, m.GetRowCount(0));
    }

    [TestMethod]
    public void Rebuild_LargeScale_IsFastAndAccurate()
    {
        const int lineCount = 200_000;
        var m = new WrapRowMetrics();

        // 200k lines where every 100th line wraps to 3 rows, others are 1 row
        m.Rebuild(lineCount, i => (i % 100 == 0) ? 3 : 1);

        int expectedTotal = (lineCount - (lineCount / 100)) + (lineCount / 100 * 3);
        Assert.AreEqual(expectedTotal, m.TotalVisualRows);
        Assert.IsTrue(m.IsValidFor(lineCount));

        // Fast row count check
        Assert.AreEqual(3, m.GetRowCount(0));
        Assert.AreEqual(1, m.GetRowCount(1));
        Assert.AreEqual(3, m.GetRowCount(100));
        Assert.AreEqual(1, m.GetRowCount(101));

        // Start row lookup
        Assert.AreEqual(0, m.GetLineStartRow(0, lineCount));
        Assert.AreEqual(3, m.GetLineStartRow(1, lineCount));
        Assert.AreEqual(4, m.GetLineStartRow(2, lineCount));

        // Binary search inverse mapping
        Assert.AreEqual(0, m.GetDocumentLineFromVisualRow(0, lineCount));
        Assert.AreEqual(0, m.GetDocumentLineFromVisualRow(1, lineCount));
        Assert.AreEqual(0, m.GetDocumentLineFromVisualRow(2, lineCount));
        Assert.AreEqual(1, m.GetDocumentLineFromVisualRow(3, lineCount));
    }
}
