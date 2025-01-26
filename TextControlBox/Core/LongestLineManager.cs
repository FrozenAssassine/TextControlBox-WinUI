using Collections.Pooled;
using System;
using TextControlBoxNS.Core.Selection;
using TextControlBoxNS.Core.Text;

namespace TextControlBoxNS.Core;

internal class LongestLineManager
{
    public int longestLineLength = 0;
    public int longestIndex = 0;
    public bool needsRecalculation = true;

    private SelectionManager selManager;
    private TextManager textManager;
    public void Init(SelectionManager selManager, TextManager textManager)
    {
        this.selManager = selManager;
        this.textManager = textManager;
    }

    //Get the longest line in the textbox
    private int GetLongestLineIndex(PooledList<string> totalLines)
    {
        var span = totalLines.Span;
        int longestIndex = 0;
        int oldLenght = 0;

        for (int i = 0; i < span.Length; i++)
        {
            var lenght = span[i].Length;
            if (lenght > oldLenght)
            {
                longestIndex = i;
                oldLenght = lenght;
            }
        }
        return longestIndex;
    }
    private int GetLongestLineLength(string text)
    {
        int maxLength = 0;
        int currentLength = 0;
        ReadOnlySpan<char> spanText = text.AsSpan();
        foreach (char c in spanText)
        {
            if (c == '\n')
            {
                if (currentLength > maxLength)
                    maxLength = currentLength;
                currentLength = 0;
            }
            else
            {
                currentLength++;
            }
        }
        if (currentLength > maxLength)
            maxLength = currentLength;
        return maxLength;
    }
    
    public void CheckRecalculateLongestLine(string text)
    {
        if (GetLongestLineLength(text) > longestLineLength)
        {
            needsRecalculation = true;
        }
    }
    public void CheckRecalculateLongestLine()
    {
        if (needsRecalculation)
        {
            needsRecalculation = false;
            longestIndex= GetLongestLineIndex(textManager.totalLines);
        }

    }
    public void CheckSelection()
    {
        if (selManager.currentTextSelection.IsLineInSelection(longestIndex))
            needsRecalculation = true;
    }
}
