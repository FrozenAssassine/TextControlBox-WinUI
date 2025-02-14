using System;
using System.Linq;
using System.Text;
using TextControlBoxNS.Core.Renderer;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Extensions;
using TextControlBoxNS.Helper;
using TextControlBoxNS.Models;

namespace TextControlBoxNS.Core.Selection;

internal class SelectionManager
{
    private TextManager textManager;
    private CursorManager cursorManager;
    private SelectionRenderer selectionRenderer;
    private EventsManager eventsManager;

    private readonly ReplaceSelectionManager replaceSelectionManager = new ReplaceSelectionManager();
    private readonly RemoveSelectionManager removeSelectionManager = new RemoveSelectionManager();

    public readonly TextSelection OldTextSelection = new TextSelection();
    public readonly TextSelection currentTextSelection = new TextSelection();

    public readonly CursorPosition selectionStart;
    public readonly CursorPosition selectionEnd;

    public bool IsSelectingOverLinenumbers { get; set; } = false;
    public bool IsSelecting { get; set; } = false;
    public bool HasSelection { get; set; } = false;
    
    public SelectionManager()
    {
        selectionStart = currentTextSelection.StartPosition;
        selectionEnd = currentTextSelection.EndPosition;
    }

    public void Init(
        TextManager textManager, 
        CursorManager cursorManager, 
        SelectionRenderer selectionRenderer, 
        EventsManager eventsManager
        )
    {
        this.textManager = textManager;
        this.cursorManager = cursorManager;
        this.selectionRenderer = selectionRenderer;
        this.eventsManager = eventsManager;

        replaceSelectionManager.Init(textManager, cursorManager);
        removeSelectionManager.Init(cursorManager, textManager);
    }

    public void SetSelection(TextSelection selection)
    {
        if (!selection.HasSelection)
            return;

        SetSelection(selection.StartPosition, selection.EndPosition);
    }
    public void SetSelection(int startLine, int startChar, int endLine, int endChar)
    {
        IsSelecting = true;
        selectionStart.SetChangeValues(startLine, startChar);
        selectionEnd.SetChangeValues(endLine, endChar);

        selectionEnd.IsNull = false;
        selectionStart.IsNull = false;

        IsSelecting = false;
        HasSelection = SelectionHelper.TextIsSelected(selectionStart, selectionEnd);
    }

    public void SetSelection(CursorPosition startPosition, CursorPosition endPosition)
    {
        IsSelecting = true;
        selectionStart.SetChangeValues(startPosition);
        selectionEnd.SetChangeValues(endPosition);

        selectionEnd.IsNull = false;
        selectionStart.IsNull = false;

        IsSelecting = false;
        HasSelection = SelectionHelper.TextIsSelected(selectionStart, selectionEnd);
    }
    public void SetSelectionStart(CursorPosition startPosition)
    {
        IsSelecting = true;
        selectionStart.SetChangeValues(startPosition);

        IsSelecting = false;
        HasSelection = SelectionHelper.TextIsSelected(selectionStart, selectionEnd);
    }
    public void SetSelectionEnd(CursorPosition endPosition)
    {
        IsSelecting = true;
        selectionEnd.SetChangeValues(endPosition);

        IsSelecting = false;
        HasSelection = SelectionHelper.TextIsSelected(selectionStart, selectionEnd);
    }
    public void SetSelectionStart(int startPos, int characterPos)
    {
        selectionStart.CharacterPosition = characterPos;
        selectionStart.LineNumber = startPos;
        selectionStart.IsNull = false;

        HasSelection = SelectionHelper.TextIsSelected(selectionStart, selectionEnd);
    }
    public void SetSelectionEnd(int endPos, int characterPos)
    {
        selectionEnd.CharacterPosition = characterPos;
        selectionEnd.LineNumber = endPos;
        selectionEnd.IsNull = false;

        HasSelection = SelectionHelper.TextIsSelected(selectionStart, selectionEnd);
    }

    public void ClearSelection()
    {
        HasSelection = false;
        IsSelecting = false;
        selectionEnd.IsNull = true;
        selectionStart.IsNull = true;

        eventsManager.CallSelectionChanged();
    }


    public void ForceClearSelection(CanvasUpdateManager canvasHelper)
    {
        ClearSelection();
        canvasHelper.UpdateSelection();
    }

    public void StartSelectionIfNeeded()
    {
        if (!HasSelection)
        {
            //TODO! 
            SetSelectionEnd(cursorManager.currentCursorPosition.LineNumber, cursorManager.currentCursorPosition.CharacterPosition);
            SetSelectionStart(cursorManager.currentCursorPosition.LineNumber, cursorManager.currentCursorPosition.CharacterPosition);
        }
    }

    public bool Equals(TextSelection sel1, TextSelection sel2)
    {
        return cursorManager.Equals(sel1.StartPosition, sel2.StartPosition) &&
            cursorManager.Equals(sel1.EndPosition, sel2.EndPosition);
    }

    //Order the selection that StartPosition is always smaller than EndPosition
    public (bool startNull, bool endNull, int startLine, int startChar, int endLine, int endChar) OrderTextSelectionSeparated()
    {
        return SelectionHelper.OrderTextSelectionSeparated(currentTextSelection, HasSelection);
    }

    public bool WholeTextSelected()
    {
        var sel = OrderTextSelectionSeparated();
        if (sel.startNull && sel.endNull)
            return false;

        return sel.startLine == 0 && sel.startChar == 0 && sel.endLine == textManager.LinesCount - 1 && sel.endChar == textManager.GetLineLength(-1);
    }

    public CursorPosition GetMax(CursorPosition pos1, CursorPosition pos2)
    {
        if (pos1.LineNumber == pos2.LineNumber)
            return pos1.CharacterPosition > pos2.CharacterPosition ? pos1 : pos2;
        return pos1.LineNumber > pos2.LineNumber ? pos1 : pos2;
    }
    public CursorPosition GetMin(CursorPosition pos1, CursorPosition pos2)
    {
        if (pos1.LineNumber == pos2.LineNumber)
            return pos1.CharacterPosition > pos2.CharacterPosition ? pos2 : pos1;
        return pos1.LineNumber > pos2.LineNumber ? pos2 : pos1;
    }
    public CursorPosition GetMin(TextSelection selection)
    {
        return GetMin(selection.StartPosition, selection.EndPosition);
    }
    public CursorPosition GetMax(TextSelection selection)
    {
        return GetMax(selection.StartPosition, selection.EndPosition);
    }

    public void InsertText(string text)
    {
        if (currentTextSelection.HasSelection)
            Replace(text);

        string curLine = textManager.GetLineText(cursorManager.LineNumber);
        string[] lines = text.Split(textManager.NewLineCharacter);

        //handle single line
        if (lines.Length == 1 && text != string.Empty)
        {
            textManager.SetLineText(-1, textManager.GetLineText(-1).AddText(text, cursorManager.CharacterPosition));
            cursorManager.CharacterPosition += text.Length;
            return;
        }

        //handle multi line:
        int curPos = cursorManager.CharacterPosition;
        if (curPos > curLine.Length)
            curPos = curLine.Length < 0 ? 0 : curLine.Length;

        string textInFrontOfCursor = curLine.Substring(0, curPos);
        string textBehindCursor = curLine.SafeRemove(0, curPos);

        textManager.DeleteAt(cursorManager.LineNumber);
        textManager.InsertOrAddRange(ListHelper.CreateLines(lines, 0, textInFrontOfCursor, textBehindCursor), cursorManager.LineNumber);

        //calculate the cursor position:
        int cursorLine = cursorManager.LineNumber + lines.Length - 1;
        int cursorChar = text.GetLastLine(textManager.NewLineCharacter).Length;
        cursorManager.SetCursorPosition(cursorLine, cursorChar);
    }
    public void Remove()
    {
        var selection = OrderTextSelectionSeparated();
        int startLine = selection.startLine;
        int endLine = selection.endLine;
        int startPosition = selection.startChar;
        int endPosition = selection.endChar;

        if (startLine == endLine)
        {
            removeSelectionManager.HandleSingleLineRemoval(startLine, startPosition, endPosition);
        }
        else if (WholeTextSelected())
        {
            removeSelectionManager.HandleWholeTextRemoval();
        }
        else
        {
            removeSelectionManager.HandleMultiLineRemoval(startLine, endLine, startPosition, endPosition);
        }

        if (textManager.LinesCount == 0)
        {
            textManager.AddLine();
        }

        cursorManager.SetCursorPosition(startLine, startPosition);
    }
    public void Replace(string text)
    {
        //Just remove the text if the text to replace with is empty
        if (text.Length == 0)
        {
            Remove();
            return;
        }

        var selection = OrderTextSelectionSeparated();
        int startLine = selection.startLine;
        int endLine = selection.endLine;
        int startPosition = selection.startChar;
        int endPosition = selection.endChar;

        string[] lines = text.Split(textManager.NewLineCharacter);
        string start_Line = textManager.GetLineText(startLine);

        //Case1: singleline selection and singleline replacement text
        if (startLine == endLine && lines.Length == 1)
        {
            replaceSelectionManager.ReplaceSingleLineSelection(startLine, startPosition, endPosition, text, start_Line);
            return;
        }

        //Case2: singleline selection and multiline replacement text
        if (startLine == endLine && lines.Length > 1)
        {
            replaceSelectionManager.ReplaceSingleLineWithMultiLine(startLine, startPosition, endPosition, lines, start_Line);
            return;
        }

        //Case3: whole text selected
        if (WholeTextSelected())
        {
            replaceSelectionManager.ReplaceWholeText(lines);
            return;
        }

        //Case4: multiline selection
        replaceSelectionManager.ReplaceMultiLineSelection(startLine, endLine, startPosition, endPosition, lines, start_Line);
    }

    public TextSelection GetSelectionFromPosition(int startPosition, int length, int numberOfCharacters)
    {
        TextSelection returnValue = new TextSelection();

        if (startPosition + length > numberOfCharacters)
        {
            if (startPosition > numberOfCharacters)
            {
                startPosition = numberOfCharacters;
                length = 0;
            }
            else
                length = numberOfCharacters - startPosition;
        }

        void GetIndexInLine(int currentIndex, int currentTotalLength)
        {
            int position = Math.Abs(currentTotalLength - startPosition);

            returnValue.StartPosition.SetChangeValues(currentIndex, position);

            if (length == 0)
                returnValue.EndPosition.SetChangeValues(returnValue.StartPosition);
            else
            {
                int lengthCount = 0;
                for (int i = currentIndex; i < textManager.LinesCount; i++)
                {
                    int lineLength = textManager.GetLineLength(i) + 1;
                    if (lengthCount + lineLength > length)
                    {
                        returnValue.EndPosition.SetChangeValues(i, Math.Abs(lengthCount - length) + position);
                        break;
                    }
                    lengthCount += lineLength;
                }
            }
        }

        //Get the renderedLength
        int totalLength = 0;
        for (int i = 0; i < textManager.LinesCount; i++)
        {
            int lineLength = textManager.GetLineLength(i) + 1;
            if (totalLength + lineLength > startPosition)
            {
                GetIndexInLine(i, totalLength);
                break;
            }

            totalLength += lineLength;
        }
        return returnValue;
    }

    public string GetSelectedText(int currentLineIndex)
    {
        //return the current line, if no text is selected:
        if (!currentTextSelection.HasSelection)
            return textManager.GetLineText(currentLineIndex) + textManager.NewLineCharacter;

        int startLine = currentTextSelection.GetMinLine();
        int endLine = currentTextSelection.GetMaxLine();
        int endIndex = currentTextSelection.GetMaxChar();
        int startIndex = currentTextSelection.GetMinChar();

        StringBuilder stringBuilder = new StringBuilder();

        if (startLine == endLine) //Singleline
        {
            string line = textManager.GetLineText(startLine < textManager.LinesCount ? startLine : textManager.LinesCount - 1);

            if (startIndex == 0 && endIndex != line.Length)
                stringBuilder.Append(line.SafeRemove(endIndex));
            else if (endIndex == line.Length && startIndex != 0)
                stringBuilder.Append(line.Safe_Substring(startIndex));
            else if (startIndex == 0 && endIndex == line.Length)
                stringBuilder.Append(line);
            else stringBuilder.Append(line.SafeRemove(endIndex).Substring(startIndex));
        }
        else if (WholeTextSelected())
        {
            stringBuilder.Append(textManager.GetLinesAsString());
        }
        else //Multiline
        {
            //StartLine
            stringBuilder.Append(textManager.GetLineText(startLine).Substring(startIndex) + textManager.NewLineCharacter);

            //Other lines
            if (endLine - startLine > 1)
                stringBuilder.Append(textManager.GetLinesAsString(startLine + 1, endLine - startLine - 1) + textManager.NewLineCharacter);

            //Endline
            string currentLine = textManager.GetLineText(endLine);

            stringBuilder.Append(endIndex >= currentLine.Length ? currentLine : currentLine.SafeRemove(endIndex));
        }
        return stringBuilder.ToString();
    }

    // Replaces the lines in TotalLines, starting by Start replacing Count number of items, with the string in SplittedText
    // All lines that can be replaced get replaced all lines that are needed additionally get added
    public void ReplaceLines(int start, int count, string[] splittedText)
    {
        if (splittedText.Length == 0)
        {
            textManager.Safe_RemoveRange(start, count);
            return;
        }

        //Same line-length -> check for any differences in the individual lines
        if (count == splittedText.Length)
        {
            for (int i = 0; i < count; i++)
            {
                if (!textManager.GetLineText(i).Equals(splittedText[i], StringComparison.Ordinal))
                {
                    textManager.SetLineText(start + i, splittedText[i]);
                }
            }
        }
        //Delete items from start to count; Insert splittedText at start
        else if (count > splittedText.Length)
        {
            for (int i = 0; i < count; i++)
            {
                if (i < splittedText.Length)
                {
                    textManager.SetLineText(start + i, splittedText[i]);
                }
                else
                {
                    textManager.Safe_RemoveRange(start + i, count - i);
                    break;
                }
            }
        }
        //Replace all items from start - count with existing (add more if out of range)
        else //SplittedText.renderedLength > Count:
        {
            for (int i = 0; i < splittedText.Length; i++)
            {
                //replace all possible lines
                if (i < count)
                {
                    textManager.SetLineText(start + i, splittedText[i]);
                }
                else //Add new lines
                {
                    textManager.InsertOrAddRange(splittedText.Skip(start + i), start + i);
                    break;
                }
            }
        }
    }

    public bool MoveLinesUp(TextSelection selection, CursorPosition cursorposition)
    {
        //Move single line
        if (selection != null)
            return false;

        if (cursorposition.LineNumber > 0)
        {
            textManager.SwapLines(cursorposition.LineNumber, cursorposition.LineNumber - 1);
            cursorposition.LineNumber -= 1;
            return true;
        }
        return false;
    }
    public bool MoveLinesDown(TextSelection selection, CursorPosition cursorposition)
    {
        //Move single line
        if (selection != null)
            return false;

        if (cursorposition.LineNumber < textManager.LinesCount)
        {
            textManager.SwapLines(cursorposition.LineNumber, cursorposition.LineNumber + 1);
            cursorposition.LineNumber += 1;
            return true;
        }
        return false;
    }
    public void ClearSelectionIfNeeded(CoreTextControlBox textbox)
    {
        //If the selection is visible, but is not getting set, clear the selection
        if (HasSelection && !IsSelecting)
        {
            textbox.ClearSelection();
        }
    }

    public void SelectSingleWord(CanvasUpdateManager canvashelper)
    {
        int characterpos = cursorManager.CharacterPosition;

        SetSelectionStart(cursorManager.LineNumber, characterpos - cursorManager.CalculateStepsToMoveLeftNoControl(characterpos));
        SetSelectionEnd(cursorManager.LineNumber, characterpos + cursorManager.CalculateStepsToMoveRightNoControl(characterpos));

        cursorManager.CharacterPosition = selectionEnd.CharacterPosition;
        HasSelection = true;

        canvashelper.UpdateSelection();
        canvashelper.UpdateCursor();
    }

    public (int start, int length) CalculateSelectionStartLength()
    {
        //order the selection and check if it's null
        var (startNull, endNull, startLine, startChar, endLine, endChar) = OrderTextSelectionSeparated();
        if (startNull && endNull)
            return (0, 0);

        int length = 0;
        int startIndex = 0;

        for (int i = 0; i < textManager.LinesCount; i++)
        {
            var currentLine = textManager.totalLines.Span[i];

            //calculate startIndex
            if (i < startLine)
                startIndex += currentLine.Length + 1;
            else if (i == startLine)
                startIndex += startChar;

            if (endNull) //no end selected:
                continue;

            //calculate length
            if (i >= startLine && i <= endLine)
            {
                if (i == startLine && i == endLine)
                    length += endChar - startChar;
                else if (i == startLine)
                    length += currentLine.Length - startChar;
                else if (i == endLine)
                    length += endChar;
                else
                    length += currentLine.Length + 1;
            }
        }
        return (startIndex, length);
    }
}