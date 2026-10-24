using TextControlBoxNS.Extensions;
using TextControlBoxNS.Helper;

namespace TextControlBoxNS.Core.Text;

internal class CurrentLineManager
{
    private CursorManager cursorManager;
    private TextManager textManager;

    public void Init(CursorManager cursorManager, TextManager textManager)
    {
        this.cursorManager = cursorManager;
        this.textManager = textManager;
    }

    public int CurrentLineIndex { get => cursorManager.currentCursorPosition.LineNumber; set => cursorManager.currentCursorPosition.LineNumber = value; }
    public string CurrentLine
    {
        get => GetCurrentLineText();
        set => SetCurrentLineText(value);
    }
    public int Length => textManager.totalLines.Count == 0 ? 0 : CurrentLine.Length;

    public string GetCurrentLineText()
    {
        return textManager.totalLines[CurrentLineIndex < textManager.LinesCount ? CurrentLineIndex : textManager.LinesCount - 1 < 0 ? 0 : textManager.LinesCount - 1];
    }
    public void SetCurrentLineText(string text)
    {
        textManager.totalLines[CurrentLineIndex < textManager.LinesCount ? CurrentLineIndex : textManager.LinesCount - 1] = text;
    }
    public void UpdateCurrentLine(int currentLine)
    {
        CurrentLineIndex = currentLine;
    }

    public void AddToEnd(string add)
    {
        CurrentLine = CurrentLine + add;
    }

    public void AddText(string add, int position)
    {
        if (position < 0)
            position = 0;

        string current = CurrentLine;
        if (position >= current.Length || current.Length <= 0)
            CurrentLine = current + add;
        else
        {
            position = TextElementHelper.SnapToTextElementStart(current, position);
            CurrentLine = current.Insert(position, add);
        }
    }

    public void SafeRemove(int start, int count = -1)
    {
        string current = CurrentLine;
        if (count > 0 && current != null)
        {
            int snappedStart = TextElementHelper.SnapToTextElementStart(current, start);
            int end = start + count;
            int snappedEnd = TextElementHelper.SnapToTextElementEnd(current, end);
            start = snappedStart;
            count = snappedEnd - snappedStart;
        }
        CurrentLine = current.SafeRemove(start, count);
    }
}
