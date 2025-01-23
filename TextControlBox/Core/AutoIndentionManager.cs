using System;
using System.Diagnostics;
using TextControlBoxNS.Core;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Helper;

internal class AutoIndentionManager
{
    private TextManager textManager;
    private CursorManager cursorManager;
    private TabSpaceHelper tabSpaceHelper;


    public void Init(TextManager textManager, CursorManager cursorManager, TabSpaceHelper tabSpaceHelper)
    {
        this.textManager = textManager;
        this.cursorManager = cursorManager;
        this.tabSpaceHelper = tabSpaceHelper;
    }

    public int CalculateDepth(int startIndex)
    {
        if (startIndex < 0 || startIndex >= textManager.LinesCount)
            return 0;
        
        int currentDepth = startIndex > 0 ? GetDepth(startIndex) : 0;
        for (int i = startIndex; i < textManager.LinesCount; i++)
        {
            string line = textManager.totalLines[i];
            int lineDepth = GetIndentationLevel(line);

            if (lineDepth != currentDepth)
                break;

            currentDepth = lineDepth;
        }

        return currentDepth;
    }

    private int GetDepth(int index)
    {
        int depth = GetIndentationLevel(textManager.totalLines[index]);
        return depth;
    }

    private static int GetIndentationLevel(string line)
    {
        int level = 0;
        foreach (char c in line)
        {
            if (c == ' ') // Assume 4 spaces = 1 level
            {
                level++;
                if (level % 4 == 0) level++;
            }
            else if (c == '\t') // 1 tab = 1 level
            {
                level++;
            }
            else
            {
                break; // Stop when non-indentation characters are found
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

    public void OnCursorMoved(int currentLineIndex)
    {
        int depth = CalculateDepth(currentLineIndex);

        if(textManager.GetLineLength(currentLineIndex) > depth)
            cursorManager.SetCursorPosition(currentLineIndex, depth * tabSpaceHelper.TabCharacter.Length);
        else
            textManager.String_AddToStart(currentLineIndex, tabSpaceHelper.TabCharacter);
    }
}