using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using TextControlBoxNS;
using TextControlBoxNS.Core;
using TextControlBoxNS.Core.Selection;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Helper;

namespace TextControlBox.Tests;

[TestClass]
public class SelectionLineEndingTests
{
    private static readonly string[] SampleLines = [
        "falsch: 14:53 ",
        "richtig: 12:30",
        "2h20",
        "",
        "\u200F\u200E14:13:36",
        "13:40:35",
        "",
        "33 minuten",
        "",
        "DSCF5143.JPG",
        "13:12, 1asdasd",
        "\u200F\u200E13:45",
        "",
        "\u200F\u200E\u200F\u200E\u200F\u200E\u200F\u200E",
        "14:33:55",
        "14:24",
        "",
        "",
        " 14:43",
        "17:15",
        "2h32min"
    ];

    [TestMethod]
    [DataRow(LineEnding.CRLF, 2)]
    [DataRow(LineEnding.LF, 1)]
    [DataRow(LineEnding.CR, 1)]
    public void Selection_FullDocument_SelectsEntireRenderedText_WithoutMissingPiece(LineEnding lineEnding, int lineEndingLength)
    {
        string newline = LineEndings.LineEndingToString(lineEnding);
        string joinedText = string.Join(newline, SampleLines);
        int totalDocumentLines = SampleLines.Length;

        // Select all: from (0, 0) to (lastLine, lastLine.Length)
        int lastLineIndex = totalDocumentLines - 1;
        int lastLineLength = SampleLines[lastLineIndex].Length;

        var (selStart, selLength) = SelectionHelper.CalculateRenderedSelectionIndices(
            unrenderedLinesToRenderStart: 0,
            numberOfRenderedLines: totalDocumentLines,
            lineEndingLength: lineEndingLength,
            lines: SampleLines,
            startLine: 0,
            characterPosStart: 0,
            endLine: lastLineIndex,
            characterPosEnd: lastLineLength,
            textLength: joinedText.Length);

        Assert.AreEqual(0, selStart, $"Expected selection to start at index 0 for {lineEnding}");
        Assert.AreEqual(joinedText.Length, selLength,
            $"Expected selection length to exactly equal rendered text length for {lineEnding}, but was {selLength} vs {joinedText.Length}");
    }

    [TestMethod]
    [DataRow(LineEnding.CRLF, 2)]
    [DataRow(LineEnding.LF, 1)]
    [DataRow(LineEnding.CR, 1)]
    public void Selection_PartialRange_CalculatesExactCharacterOffsets(LineEnding lineEnding, int lineEndingLength)
    {
        string newline = LineEndings.LineEndingToString(lineEnding);
        string joinedText = string.Join(newline, SampleLines);

        // Select from line 1 (character 3) to line 2 (character 2)
        int startLine = 1;
        int startChar = 3;
        int endLine = 2;
        int endChar = 2;

        var (selStart, selLength) = SelectionHelper.CalculateRenderedSelectionIndices(
            unrenderedLinesToRenderStart: 0,
            numberOfRenderedLines: SampleLines.Length,
            lineEndingLength: lineEndingLength,
            lines: SampleLines,
            startLine: startLine,
            characterPosStart: startChar,
            endLine: endLine,
            characterPosEnd: endChar,
            textLength: joinedText.Length);

        // Expected start: line 0 length + newline + startChar
        int expectedStart = SampleLines[0].Length + lineEndingLength + startChar;
        // Expected end: line 0 length + newline + line 1 length + newline + endChar
        int expectedEnd = SampleLines[0].Length + lineEndingLength + SampleLines[1].Length + lineEndingLength + endChar;
        int expectedLength = expectedEnd - expectedStart;

        Assert.AreEqual(expectedStart, selStart);
        Assert.AreEqual(expectedLength, selLength);

        // Verify the extracted substring matches the expected slice
        string actualSelectedSubstring = joinedText.Substring(selStart, selLength);
        string expectedSelectedSubstring = SampleLines[1].Substring(startChar) + newline + SampleLines[2].Substring(0, endChar);
        Assert.AreEqual(expectedSelectedSubstring, actualSelectedSubstring);
    }

    [TestMethod]
    public void Selection_BackwardsSelection_NormalizesEndpoints()
    {
        string[] lines = ["LineA", "LineB", "LineC"];
        string text = string.Join("\n", lines);

        // Select backwards from line 2 char 3 to line 0 char 1
        var (selStart, selLength) = SelectionHelper.CalculateRenderedSelectionIndices(
            unrenderedLinesToRenderStart: 0,
            numberOfRenderedLines: 3,
            lineEndingLength: 1,
            lines: lines,
            startLine: 2,
            characterPosStart: 3,
            endLine: 0,
            characterPosEnd: 1,
            textLength: text.Length);

        int expectedStart = 1; // "ineA\nLineB\nLin"
        int expectedEnd = lines[0].Length + 1 + lines[1].Length + 1 + 3;
        int expectedLength = expectedEnd - expectedStart;

        Assert.AreEqual(expectedStart, selStart);
        Assert.AreEqual(expectedLength, selLength);
    }

    [TestMethod]
    public void Selection_IntervalMerging_ZeroWidthMarkWithText_DoesNotDuplicate()
    {
        // Line with \u200F at X=0 followed by text of width 79.2 at X=0
        var spans = new List<(float startX, float endX)>
        {
            (0f, 0f),      // zero-width mark
            (0f, 79.2f)    // actual text
        };

        var merged = SelectionHelper.MergeSelectionIntervals(spans, defaultEmptyLineWidth: 11f);

        Assert.AreEqual(1, merged.Count, "Should merge into exactly 1 interval");
        Assert.AreEqual(0f, merged[0].startX, 0.01f);
        Assert.AreEqual(79.2f, merged[0].endX, 0.01f);
    }

    [TestMethod]
    public void Selection_IntervalMerging_MultipleZeroWidthMarksOnEmptyLine_DrawsSingleIndicator()
    {
        // 8 RTL/LTR marks on empty line, each zero-width at X=0
        var spans = new List<(float startX, float endX)>
        {
            (0f, 0f), (0f, 0f), (0f, 0f), (0f, 0f),
            (0f, 0f), (0f, 0f), (0f, 0f), (0f, 0f)
        };

        var merged = SelectionHelper.MergeSelectionIntervals(spans, defaultEmptyLineWidth: 11f);

        Assert.AreEqual(1, merged.Count, "Should produce exactly 1 indicator interval for empty line with invisible marks");
        Assert.AreEqual(0f, merged[0].startX, 0.01f);
        Assert.AreEqual(11f, merged[0].endX, 0.01f);
    }

    [TestMethod]
    public void Selection_IntervalMerging_OverlappingRuns_MergesCleanly()
    {
        var spans = new List<(float startX, float endX)>
        {
            (0f, 50f),
            (45f, 100f)
        };

        var merged = SelectionHelper.MergeSelectionIntervals(spans, defaultEmptyLineWidth: 11f);

        Assert.AreEqual(1, merged.Count);
        Assert.AreEqual(0f, merged[0].startX, 0.01f);
        Assert.AreEqual(100f, merged[0].endX, 0.01f);
    }

    [TestMethod]
    public void Selection_IntervalMerging_DisjointSpans_MaintainsSeparateIntervals()
    {
        var spans = new List<(float startX, float endX)>
        {
            (0f, 30f),
            (60f, 90f)
        };

        var merged = SelectionHelper.MergeSelectionIntervals(spans, defaultEmptyLineWidth: 11f);

        Assert.AreEqual(2, merged.Count);
        Assert.AreEqual(0f, merged[0].startX, 0.01f);
        Assert.AreEqual(30f, merged[0].endX, 0.01f);
        Assert.AreEqual(60f, merged[1].startX, 0.01f);
        Assert.AreEqual(90f, merged[1].endX, 0.01f);
    }

    [TestMethod]
    [DataRow(LineEnding.CRLF, 2)]
    [DataRow(LineEnding.LF, 1)]
    [DataRow(LineEnding.CR, 1)]
    public void TextManager_CountCharacters_MatchesStringLength_ForAllLineEndings(LineEnding lineEnding, int lineEndingLength)
    {
        var textManager = new TextManager();
        var eventsManager = new EventsManager();
        textManager.Init(eventsManager);

        textManager.totalLines.Clear();
        textManager.totalLines.AddRange(SampleLines);
        textManager.LineEnding = lineEnding;

        string joined = string.Join(LineEndings.LineEndingToString(lineEnding), SampleLines);
        int counted = textManager.CountCharacters();

        Assert.AreEqual(joined.Length, counted, $"CountCharacters failed for {lineEnding}");
    }

    [TestMethod]
    [DataRow(LineEnding.CRLF)]
    [DataRow(LineEnding.LF)]
    [DataRow(LineEnding.CR)]
    public void SelectionManager_CalculateSelectionStartLength_FullSelection_MatchesTotalCharacters(LineEnding lineEnding)
    {
        var textManager = new TextManager();
        var eventsManager = new EventsManager();
        var cursorManager = new CursorManager();
        var selectionManager = new SelectionManager();

        textManager.Init(eventsManager);
        selectionManager.Init(textManager, cursorManager, eventsManager);

        textManager.totalLines.Clear();
        textManager.totalLines.AddRange(SampleLines);
        textManager.LineEnding = lineEnding;

        int lastLine = SampleLines.Length - 1;
        int lastLineLen = SampleLines[lastLine].Length;
        selectionManager.SetSelection(0, 0, lastLine, lastLineLen);

        var (start, length) = selectionManager.CalculateSelectionStartLength();

        int expectedTotal = textManager.CountCharacters();
        Assert.AreEqual(0, start);
        Assert.AreEqual(expectedTotal, length, $"CalculateSelectionStartLength failed to match total characters for {lineEnding}");
    }
}
