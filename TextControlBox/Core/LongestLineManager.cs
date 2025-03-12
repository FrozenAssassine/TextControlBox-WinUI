using Collections.Pooled;
using Microsoft.Graphics.Canvas;
using System;
using System.Diagnostics;
using TextControlBoxNS.Core.Renderer;
using TextControlBoxNS.Core.Selection;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Helper;
using Windows.Foundation;

namespace TextControlBoxNS.Core;

internal class LongestLineManager
{
    public int longestLineLength = 0;

    private int _longestIndex = 0;
    public int longestIndex
    {
        get => _longestIndex;
        set
        {
            _longestIndex = value;
            Recalculate(value);
        }
    }
    public bool needsRecalculation = true;

    public Size longestLineWidth { get; private set; }

    public bool HasLongestLineChanged = false;
    private SelectionManager selManager;
    private TextManager textManager;
    private TextRenderer textRenderer;

    public void Init(SelectionManager selManager, TextManager textManager, TextRenderer textRenderer)
    {
        this.selManager = selManager;
        this.textManager = textManager;
        this.textRenderer = textRenderer;
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

    public void Recalculate(int index = -1)
    {
        needsRecalculation = false;
        if (index == -1)
            _longestIndex = GetLongestLineIndex(textManager.totalLines);
        else
            _longestIndex = index;

        if (_longestIndex >= textManager.LinesCount || textManager.LinesCount == 0)
            return;

        longestLineLength = textManager.totalLines[_longestIndex].Length;
        if(textRenderer.TextFormat != null)
            longestLineWidth = Utils.MeasureLineLenght(CanvasDevice.GetSharedDevice(), textManager.totalLines[longestIndex], textRenderer.TextFormat);
        HasLongestLineChanged = true;
    }

    public void CheckRecalculateLongestLine(string text)
    {
        int lengt = GetLongestLineLength(text);
        if (lengt > longestLineLength)
        {
            Recalculate();
        }
    }

    public void CheckRecalculateLongestLine(bool force = false)
    {
        if (needsRecalculation || force)
        {
            Recalculate();
        }
    }
    public void CheckSelection()
    {
        if (selManager.currentTextSelection.IsLineInSelection(_longestIndex))
            needsRecalculation = true;
    }
}
