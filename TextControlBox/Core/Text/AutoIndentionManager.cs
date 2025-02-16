using System;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Helper;

internal class AutoIndentionManager
{
    private TextManager textManager;
    private TabSpaceHelper tabSpaceHelper;

    public void Init(TextManager textManager, TabSpaceHelper tabSpaceHelper)
    {
        this.textManager = textManager;
        this.tabSpaceHelper = tabSpaceHelper;
    }

    public int CalculateDepth(int startIndex)
    {
        if (startIndex < 0 || startIndex >= textManager.LinesCount)
            throw new IndexOutOfRangeException("");

        int currentDepth = startIndex > 0 ? GetDepth(startIndex) : 0;
        var linesSpan = textManager.totalLines.Span;

        for (int i = startIndex; i < linesSpan.Length; i++)
        {
            int lineDepth = GetIndentationLevel(linesSpan[i]);

            if (lineDepth != currentDepth)
                break;

            currentDepth = lineDepth;
        }

        return currentDepth;
    }

    private int GetDepth(int index)
    {
        return GetIndentationLevel(textManager.totalLines.Span[index]);
    }

    private int GetIndentationLevel(ReadOnlySpan<char> line)
    {
        int spaces = tabSpaceHelper.NumberOfSpaces;
        char tabChar = tabSpaceHelper.Tab[0];
        int level = 0;
        int spaceCount = 0;

        foreach (char c in line)
        {
            if (c == ' ')
            {
                spaceCount++;
                if (spaceCount == spaces)
                {
                    level++;
                    spaceCount = 0;
                }
            }
            else if (c == tabChar)
            {
                level++;
            }
            else
            {
                break;
            }
        }

        return level;
    }

    public string RepeatIndentionString(int count)
    {
        string indStr = tabSpaceHelper.TabCharacter;
        if (count <= 0 || string.IsNullOrEmpty(indStr))
            return string.Empty;

        return string.Create(indStr.Length * count, (indStr, count), (span, state) =>
        {
            for (int i = 0; i < state.count; i++)
            {
                state.indStr.AsSpan().CopyTo(span.Slice(i * state.indStr.Length));
            }
        });

    }

    public int OnEnterPressed(int currentLineIndex)
    {
        return CalculateDepth(currentLineIndex);
    }
}