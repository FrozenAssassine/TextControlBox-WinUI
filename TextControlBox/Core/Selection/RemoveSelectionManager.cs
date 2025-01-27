using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Extensions;

namespace TextControlBoxNS.Core.Selection;

internal class RemoveSelectionManager
{
    private CursorManager cursorManager;
    private TextManager textManager;

    public void Init(CursorManager cursorManager, TextManager textManager)
    {
        this.cursorManager = cursorManager;
        this.textManager = textManager;
    }

    //handle removal when start and end -line are the same
    public void HandleSingleLineRemoval(int line, int startPosition, int endPosition)
    {
        string lineText = textManager.GetLineText(line);

        string updatedText =
            startPosition == 0 && endPosition == lineText.Length
                ? ""
                : lineText.SafeRemove(startPosition, endPosition - startPosition);

        textManager.SetLineText(line, updatedText);
    }

    //handle remove when the whole text is selected
    public void HandleWholeTextRemoval()
    {
        textManager.ClearText(true);
        cursorManager.SetCursorPosition(textManager.LinesCount - 1, 0);
    }

    //handle remove across multiple lines
    public void HandleMultiLineRemoval(int startLine, int endLine, int startPosition, int endPosition)
    {
        string startLineText = textManager.GetLineText(startLine);
        string endLineText = textManager.GetLineText(endLine);

        if (startPosition == 0 && endPosition == endLineText.Length)
        {
            //all lines selected
            textManager.Safe_RemoveRange(startLine, endLine - startLine + 1);
        }
        else if (startPosition == 0 && endPosition != endLineText.Length)
        {
            //only start line fully selected
            textManager.SetLineText(endLine, endLineText.Safe_Substring(endPosition));
            textManager.Safe_RemoveRange(startLine, endLine - startLine);
        }
        else if (startPosition != 0 && endPosition == endLineText.Length)
        {
            //only end line fully selected
            textManager.SetLineText(startLine, startLineText.SafeRemove(startPosition));
            textManager.Safe_RemoveRange(startLine + 1, endLine - startLine);
        }
        else
        {
            //neither start nor end line fully selected
            string mergedText = startLineText.SafeRemove(startPosition) + endLineText.Safe_Substring(endPosition);
            textManager.SetLineText(startLine, mergedText);
            textManager.Safe_RemoveRange(startLine + 1, endLine - startLine);
        }
    }
}
