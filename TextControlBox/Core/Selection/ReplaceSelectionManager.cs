using System.Diagnostics;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Extensions;
using TextControlBoxNS.Helper;

namespace TextControlBoxNS.Core.Selection;

internal class ReplaceSelectionManager
{
    private TextManager textManager;
    private CursorManager cursorManager;
    public void Init(TextManager textManager, CursorManager cursorManager)
    {
        this.textManager = textManager;
        this.cursorManager = cursorManager;
    }

    public void ReplaceSingleLineSelection(int line, int start, int end, string text, string originalLine)
    {
        string updatedLine = originalLine.SafeRemove(start, end - start).AddText(text, start);
        textManager.SetLineText(line, updatedLine);
        cursorManager.SetCursorPosition(line, start + text.Length);
    }

    public void ReplaceSingleLineWithMultiLine(int line, int start, int end, string[] lines, string originalLine)
    {
        string prefix = originalLine.Safe_Substring(0, start);
        string suffix = originalLine.Safe_Substring(end);

        textManager.SetLineText(line, prefix + lines[0]);
        textManager.InsertOrAddRange(ListHelper.CreateLines(lines, 1, suffix, ""), line + 1);
        cursorManager.SetCursorPosition(line + lines.Length - 1, suffix.Length + lines[^1].Length);
    }

    public void ReplaceWholeText(string[] lines)
    {
        textManager.ClearText();
        textManager.InsertOrAddRange(lines, 0);
        cursorManager.SetCursorPosition(lines.Length - 1, lines[^1].Length);
    }

    public void ReplaceMultiLineSelection(int startLine, int endLine, int startPos, int endPos, string[] lines, string startLineText)
    {
        string endLineText = textManager.GetLineText(endLine);

        if (startPos == 0 && endPos == endLineText.Length) //whole text selected
        {
            ReplaceWholeText(lines);
        }
        else if (startPos == 0) //selection starts at pos 0 to any end
        {
            textManager.SetLineText(endLine, lines[^1] + endLineText.Substring(endPos));
            textManager.RemoveRange(startLine, endLine - startLine);
            textManager.InsertOrAddRange(lines[..^1], startLine);
        }
        else if (endPos == endLineText.Length) //selection ends at end of text and not stars by 0
        {
            textManager.SetLineText(startLine, startLineText.SafeRemove(startPos) + lines[0]);
            textManager.RemoveRange(startLine + 1, endLine - startLine);
            textManager.InsertOrAddRange(lines[1..], startLine + 1);
        }
        else //selection starting from anywhere going to anywhere:
        {
            startLineText = startLineText.SafeRemove(startPos);
            endLineText = endLineText.Safe_Substring(endPos);

            if (lines.Length == 1)
            {
                textManager.SetLineText(startLine, startLineText.AddToEnd(lines[0] + endLineText));
                textManager.RemoveRange(startLine + 1, endLine - startLine < 0 ? 0 : endLine - startLine);
            }
            else
            {
                textManager.SetLineText(startLine, startLineText + lines[0]);
                textManager.SetLineText(endLine, lines[^1] + endLineText);

                textManager.RemoveRange(startLine + 1, endLine - startLine - 1);
                if (lines.Length > 2)
                    textManager.InsertOrAddRange(ListHelper.GetLines(lines, 1, lines.Length - 2), startLine + 1);
            }
        }

        cursorManager.SetCursorPosition(startLine + lines.Length - 1, startPos + lines[0].Length);
    }
}
