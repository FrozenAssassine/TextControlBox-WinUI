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

        //Whole line selected triple click
        if (startPosition == 0 && endPosition == lineText.Length + 1)
        {
            if (textManager.LinesCount == 1)
            {
                textManager.SetLineText(line, "");
            }
            else
            {
                textManager.DeleteAt(line);
            }
        }
        else
        {
            string updatedText =
                startPosition == 0 && endPosition == lineText.Length
                    ? ""
                    : lineText.SafeRemove(startPosition, endPosition - startPosition);

            textManager.SetLineText(line, updatedText);
        }
    }

    //handle remove when the whole text is selected
    public void HandleWholeTextRemoval()
    {
        textManager.ClearText(true);
        cursorManager.SetCursorPosition(0, 0);
    }

    //handle remove across multiple lines
    public void HandleMultiLineRemoval(int startLine, int endLine, int startPosition, int endPosition)
    {
        string startLineText = textManager.GetLineText(startLine);
        string endLineText = textManager.GetLineText(endLine);

        // Special case: full lines selected including trailing newline:
        // (startLine, 0) to (endLine, 0) means all lines from startLine to endLine - 1 are completely deleted.
        // endLine is untouched and shifts up to become startLine.
        if (startPosition == 0 && endPosition == 0)
        {
            textManager.RemoveRange(startLine, endLine - startLine);
            return;
        }

        string prefix = startPosition > 0 ? startLineText.SafeRemove(startPosition) : "";
        string suffix = endPosition < endLineText.Length ? endLineText.Safe_Substring(endPosition) : "";

        textManager.SetLineText(startLine, prefix + suffix);
        textManager.RemoveRange(startLine + 1, endLine - startLine);
    }
}
