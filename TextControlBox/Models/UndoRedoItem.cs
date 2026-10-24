using System;
using TextControlBoxNS.Core.Text;

namespace TextControlBoxNS.Models;

internal class UndoRedoItem
{
    public int StartLine { get; set; }
    public string UndoText { get; set; }
    public string RedoText { get; set; }
    public int UndoCount { get; set; }
    public int RedoCount { get; set; }
    public TextSelection SelectionBefore { get; set; }
    public TextSelection SelectionAfter { get; set; }
    public CursorPosition CursorBefore { get; set; }
    public CursorPosition CursorAfter { get; set; }
    public bool HandleNextItemToo { get; set; }
    public object AdditionalData { get; set; } = null;
    public bool CanBatch { get; set; } = false;
    public CharClass BatchCategory { get; set; } = CharClass.Word;

    // A change of a single line only keeps the part that changed. Keeping the whole line before and after made
    // every typed word on a long line (minified json, logs) cost twice the line length in memory, forever.
    // The unchanged start and end are taken from the line itself when the item is applied.
    private int _prefixLength;
    private int _suffixLength;
    private string _changedBefore;
    private string _changedAfter;

    public bool IsLineDiff { get; private set; }

    public void SetLineDiff(string lineBefore, string lineAfter)
    {
        int prefix = lineBefore.AsSpan().CommonPrefixLength(lineAfter);
        int maxSuffix = Math.Min(lineBefore.Length, lineAfter.Length) - prefix;
        int suffix = 0;
        while (suffix < maxSuffix && lineBefore[lineBefore.Length - 1 - suffix] == lineAfter[lineAfter.Length - 1 - suffix])
            suffix++;

        _prefixLength = prefix;
        _suffixLength = suffix;
        _changedBefore = lineBefore.Substring(prefix, lineBefore.Length - prefix - suffix);
        _changedAfter = lineAfter.Substring(prefix, lineAfter.Length - prefix - suffix);
        UndoText = RedoText = null;
        IsLineDiff = true;
    }

    /// <summary>The line before the change, built from the line as it is after the change.</summary>
    public string GetUndoLine(string lineAfterChange) => Rebuild(lineAfterChange, _changedBefore);

    /// <summary>The line after the change, built from the line as it is before the change.</summary>
    public string GetRedoLine(string lineBeforeChange) => Rebuild(lineBeforeChange, _changedAfter);

    /// <summary>True when <paramref name="line"/> looks like the line after the change, a cheap check instead of comparing the whole line.</summary>
    public bool MatchesLineAfterChange(string line)
    {
        return line.Length == _prefixLength + _changedAfter.Length + _suffixLength
            && line.AsSpan(_prefixLength, _changedAfter.Length).SequenceEqual(_changedAfter);
    }

    /// <summary>Stores both sides as full text again, for the rare case the line itself no longer exists.</summary>
    public void ToFullText(string undoText, string redoText)
    {
        UndoText = undoText;
        RedoText = redoText;
        IsLineDiff = false;
    }

    private string Rebuild(string currentLine, string replacement)
    {
        //the line should always match, clamp anyway so a broken history never throws
        int prefix = Math.Min(_prefixLength, currentLine.Length);
        int suffix = Math.Min(_suffixLength, currentLine.Length - prefix);
        return string.Concat(currentLine.AsSpan(0, prefix), replacement, currentLine.AsSpan(currentLine.Length - suffix));
    }
}
