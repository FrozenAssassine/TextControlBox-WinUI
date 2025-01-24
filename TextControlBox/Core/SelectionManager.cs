using System;
using System.Linq;
using System.Text;
using TextControlBoxNS.Extensions;
using TextControlBoxNS.Helper;
using TextControlBoxNS.Core.Renderer;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Models;
using System.Diagnostics;

namespace TextControlBoxNS.Core;

internal class SelectionManager
{
    private TextManager textManager;
    private CursorManager cursorManager;
    private SelectionRenderer selectionRenderer;

    public TextSelection OldTextSelection = new TextSelection();
    public TextSelection currentTextSelection = new TextSelection();

    public bool TextSelIsNull => currentTextSelection.IsNull;
    public void Init(TextManager textManager, CursorManager cursorManager, SelectionRenderer selectionRenderer)
    {
        this.textManager = textManager;
        this.cursorManager = cursorManager;
        this.selectionRenderer = selectionRenderer;
    }

    public void ForceClearSelection(CanvasUpdateManager canvasHelper)
    {
        selectionRenderer.ClearSelection();
        this.currentTextSelection.IsNull = true;
        canvasHelper.UpdateSelection();
    }

    public void SetCurrentTextSelection(TextSelection textSelection)
    {
        this.currentTextSelection.SetChangedValues(textSelection.StartPosition, textSelection.EndPosition);
    }

    public void StartSelectionIfNeeded()
    {
        if (this.currentTextSelection.IsNull)
        {
            selectionRenderer.SetSelectionEnd(cursorManager.currentCursorPosition.LineNumber, cursorManager.currentCursorPosition.CharacterPosition);
            selectionRenderer.SetSelectionStart(cursorManager.currentCursorPosition.LineNumber, cursorManager.currentCursorPosition.CharacterPosition);

            currentTextSelection.SetChangedValues(selectionRenderer.renderedSelectionStartPosition, selectionRenderer.renderedSelectionEndPosition);
        }
    }

    public bool Equals(TextSelection sel1, TextSelection sel2)
    {
        if (sel1.IsNull || sel2.IsNull)
            return false;

        return cursorManager.Equals(sel1.StartPosition, sel2.StartPosition) &&
            cursorManager.Equals(sel1.EndPosition, sel2.EndPosition);
    }

    //Order the selection that StartPosition is always smaller than EndPosition
    public TextSelection OrderTextSelection(TextSelection selection)
    {
        if (selection.IsNull)
            return selection;

        int startLine = selection.GetMinLine();
        int endLine = selection.GetMaxLine();
        int startPosition;
        int endPosition;
        if (startLine == endLine)
        {
            startPosition = selection.GetMinChar();
            endPosition = selection.GetMaxChar();
        }
        else
        {
            if (selection.StartPosition.LineNumber < selection.EndPosition.LineNumber)
            {
                endPosition = selection.EndPosition.CharacterPosition;
                startPosition = selection.StartPosition.CharacterPosition;
            }
            else
            {
                endPosition = selection.StartPosition.CharacterPosition;
                startPosition = selection.EndPosition.CharacterPosition;
            }
        }

        return new TextSelection(selection.renderedIndex, selection.renderedLength, startLine, startPosition, endLine, endPosition);
    }

    public (bool startNull, bool endNull, int startLine, int startChar, int endLine, int endChar) OrderTextSelectionSeparated(TextSelection selection)
    {
        if (selection.IsNull)
            return (true, true, - 1, -1, -1, -1);

        if (selection.EndPosition.IsNull && !selection.StartPosition.IsNull)
            return (false, true, selection.StartPosition.LineNumber, selection.StartPosition.CharacterPosition, -1, -1);

        if (!selection.EndPosition.IsNull && selection.StartPosition.IsNull)
            return (false, true, selection.EndPosition.LineNumber, selection.EndPosition.CharacterPosition, -1, -1);

        int startLine = selection.GetMinLine();
        int endLine = selection.GetMaxLine();
        int startPosition;
        int endPosition;
        
        if (startLine == endLine)
        {
            startPosition = selection.GetMinChar();
            endPosition = selection.GetMaxChar();
        }
        else
        {
            if (selection.StartPosition.LineNumber < selection.EndPosition.LineNumber)
            {
                endPosition = selection.EndPosition.CharacterPosition;
                startPosition = selection.StartPosition.CharacterPosition;
            }
            else
            {
                endPosition = selection.StartPosition.CharacterPosition;
                startPosition = selection.EndPosition.CharacterPosition;
            }
        }

        return (false, false, startLine, startPosition, endLine, endPosition);
    }

    public bool WholeTextSelected()
    {
        var sel = OrderTextSelectionSeparated(currentTextSelection);
        if (sel.startNull && sel.endNull)
            return false;

        return sel.startLine == 0 && sel.startChar == 0 && sel.endLine == textManager.LinesCount - 1 && sel.endChar == textManager.GetLineLength(-1);
    }

    //returns whether the selection starts at character zero and ends
    //needs to pass any selection object, since undo/redo uses different textselection
    public bool WholeLinesAreSelected(TextSelection selection)
    {
        if (selection.IsNull)
            return false;

        var sel = OrderTextSelectionSeparated(currentTextSelection);
        if (sel.startNull && sel.endNull)
            return false;

        return sel.startChar == 0 && sel.endChar == textManager.GetLineLength(sel.endLine);
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
        if (!currentTextSelection.IsNull)
            this.Replace(text);

        string curLine = textManager.GetLineText(cursorManager.LineNumber);

        string[] lines = text.Split(textManager.NewLineCharacter);

        //Singleline
        if (lines.Length == 1 && text != string.Empty)
        {
            text = text.Replace("\r", string.Empty).Replace("\n", string.Empty);
            textManager.SetLineText(-1, textManager.GetLineText(-1).AddText(text, cursorManager.CharacterPosition));
            cursorManager.CharacterPosition += text.Length;
            return;
        }

        //Multiline:
        int curPos = cursorManager.CharacterPosition;
        if (curPos > curLine.Length)
            curPos = curLine.Length;

        //GEt the text in front of the cursor
        string textInFrontOfCursor = curLine.Substring(0, curPos < 0 ? 0 : curPos);
        //Get the text behind the cursor
        string textBehindCursor = curLine.SafeRemove(0, curPos < 0 ? 0 : curPos);

        textManager.DeleteAt(cursorManager.LineNumber);
        textManager.InsertOrAddRange(ListHelper.CreateLines(lines, 0, textInFrontOfCursor, textBehindCursor), cursorManager.LineNumber);

       cursorManager.SetCursorPosition(cursorManager.LineNumber + lines.Length - 1, cursorManager.CharacterPosition + lines.Length > 0 ? lines[lines.Length - 1].Length : 0);
    }
    public void Replace(string text)
    {
        //Just delete the text if the string is emty
        if (text.Length == 0)
            this.Remove();

        var selection = OrderTextSelectionSeparated(currentTextSelection);
        int startLine = selection.startLine;
        int endLine = selection.endLine;
        int startPosition = selection.startChar;
        int endPosition = selection.endChar;

        string[] lines = text.Split(textManager.NewLineCharacter);
        string start_Line = textManager.GetLineText(startLine);

        //Selection is singleline and text to paste is also singleline
        if (startLine == endLine && lines.Length == 1)
        {
            start_Line =
                (startPosition == 0 && endPosition == textManager.GetLineLength(endLine)) ?
                "" :
                start_Line.SafeRemove(startPosition, endPosition - startPosition
                );

            textManager.SetLineText(startLine, start_Line.AddText(text, startPosition));

            cursorManager.SetCursorPosition(selection.startLine, startPosition + text.Length);
        }
        else if (startLine == endLine && lines.Length > 1 && (startPosition != 0 && endPosition != start_Line.Length))
        {
            string textTo = start_Line == "" ? "" : startPosition >= start_Line.Length ? start_Line : start_Line.Safe_Substring(0, startPosition);
            string textFrom = start_Line == "" ? "" : endPosition >= start_Line.Length ? start_Line : start_Line.Safe_Substring(endPosition);

            textManager.SetLineText(startLine, (textTo + lines[0]));
            textManager.InsertOrAddRange(ListHelper.CreateLines(lines, 1, "", textFrom), startLine + 1);

            cursorManager.SetCursorPosition(startLine + lines.Length - 1, endPosition + text.Length);
        }
        else if (this.WholeTextSelected())
        {
            if (lines.Length < textManager.LinesCount)
            {
                textManager.ClearText();
                textManager.InsertOrAddRange(lines, 0);
            }
            else
                this.ReplaceLines(0, textManager.LinesCount, lines);

            cursorManager.SetCursorPosition(textManager.LinesCount - 1, textManager.GetLineLength(-1));
        }
        else
        {
            string end_Line = textManager.GetLineText(endLine);

            //All lines are selected from start to finish
            if (startPosition == 0 && endPosition == end_Line.Length)
            {
                textManager.Safe_RemoveRange(startLine, endLine - startLine + 1);
                textManager.InsertOrAddRange(lines, startLine);
            }
            //Only the startline is completely selected
            else if (startPosition == 0 && endPosition != end_Line.Length)
            {
                //TODO Out of range -> multiline text => Ctrl + A => new text:
                textManager.SetLineText(endLine, end_Line.Substring(endPosition).AddToStart(lines[lines.Length - 1]));

                textManager.Safe_RemoveRange(startLine, endLine - startLine);
                textManager.InsertOrAddRange(lines.Take(lines.Length - 1), startLine);
            }
            //Only the endline is completely selected
            else if (startPosition != 0 && endPosition == end_Line.Length)
            {
                textManager.SetLineText(startLine, start_Line.SafeRemove(startPosition).AddToEnd(lines[0]));

                textManager.Safe_RemoveRange(startLine + 1, endLine - startLine);
                textManager.InsertOrAddRange(lines.Skip(1), startLine + 1);
            }
            else
            {
                //Delete the selected parts
                start_Line = start_Line.SafeRemove(startPosition);
                end_Line = end_Line.Safe_Substring(endPosition);

                //Only one line to insert
                if (lines.Length == 1)
                {
                    textManager.SetLineText(startLine, start_Line.AddToEnd(lines[0] + end_Line));
                    textManager.Safe_RemoveRange(startLine + 1, endLine - startLine < 0 ? 0 : endLine - startLine);
                }
                else
                {
                    textManager.SetLineText(startLine, start_Line.AddToEnd(lines[0]));
                    textManager.SetLineText(endLine, end_Line.AddToStart(lines[lines.Length - 1]));

                    textManager.Safe_RemoveRange(startLine + 1, endLine - startLine - 1 < 0 ? 0 : endLine - startLine - 1);
                    if (lines.Length > 2)
                        textManager.InsertOrAddRange(lines.GetLines(1, lines.Length - 2), startLine + 1);
                }
            }
            cursorManager.SetCursorPosition(startLine + lines.Length - 1, start_Line.Length + end_Line.Length - 1);
        }
    }
    public void Remove()
    {
        var selection = OrderTextSelectionSeparated(currentTextSelection);
        int startLine = selection.startLine;
        int endLine = selection.endLine;
        int startPosition = selection.startChar;
        int endPosition = selection.endChar;

        string start_Line = textManager.GetLineText(startLine);
        string end_Line = textManager.GetLineText(endLine);

        if (startLine == endLine)
        {
            string text =
                startPosition == 0 && endPosition == end_Line.Length ?
                "" :
                start_Line.SafeRemove(startPosition, endPosition - startPosition);

            textManager.SetLineText(startLine, text);
        }
        else if (this.WholeTextSelected())
        {
            textManager.ClearText(true);
            cursorManager.SetCursorPosition(textManager.LinesCount - 1, 0);
        }
        else
        {
            //Whole lines are selected from start to finish
            if (startPosition == 0 && endPosition == end_Line.Length)
            {
                textManager.Safe_RemoveRange(startLine, endLine - startLine + 1);
            }
            //Only the startline is completely selected
            else if (startPosition == 0 && endPosition != end_Line.Length)
            {
                textManager.SetLineText(endLine, end_Line.Safe_Substring(endPosition));
                textManager.Safe_RemoveRange(startLine, endLine - startLine);
            }
            //Only the endline is completely selected
            else if (startPosition != 0 && endPosition == end_Line.Length)
            {
                textManager.SetLineText(startLine, start_Line.SafeRemove(startPosition));
                textManager.Safe_RemoveRange(startLine + 1, endLine - startLine);
            }
            //Both startline and endline are not completely selected
            else
            {
                textManager.SetLineText(startLine, start_Line.SafeRemove(startPosition) + end_Line.Safe_Substring(endPosition));
                textManager.Safe_RemoveRange(startLine + 1, endLine - startLine);
            }
        }

        if (textManager.LinesCount == 0)
            textManager.AddLine();

        cursorManager.SetCursorPosition(startLine, startPosition);
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
        if (currentTextSelection.IsNull)
            return textManager.GetLineText(currentLineIndex) + textManager.NewLineCharacter;

        int startLine = currentTextSelection.GetMinLine();
        int endLine = currentTextSelection.GetMaxLine();
        int endIndex = currentTextSelection.GetMaxChar();
        int startIndex = currentTextSelection.GetMinChar();

        StringBuilder stringBuilder = new StringBuilder();

        if (startLine == endLine) //Singleline
        {
            string line = textManager.GetLineText(startLine < textManager.LinesCount? startLine : textManager.LinesCount - 1);

            if (startIndex == 0 && endIndex != line.Length)
                stringBuilder.Append(line.SafeRemove(endIndex));
            else if (endIndex == line.Length && startIndex != 0)
                stringBuilder.Append(line.Safe_Substring(startIndex));
            else if (startIndex == 0 && endIndex == line.Length)
                stringBuilder.Append(line);
            else stringBuilder.Append(line.SafeRemove(endIndex).Substring(startIndex));
        }
        else if (this.WholeTextSelected())
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

    /// <summary>
    /// Replaces the lines in TotalLines, starting by Start replacing Count number of items, with the string in SplittedText
    /// All lines that can be replaced get replaced all lines that are needed additionally get added
    /// </summary>
    /// <param name="start"></param>
    /// <param name="count"></param>
    /// <param name="splittedText"></param>
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
        if (selectionRenderer.HasSelection && !selectionRenderer.IsSelecting)
        {
            textbox.ClearSelection();
        }
    }

    public bool TextIsSelected()
    {
        return TextIsSelected(currentTextSelection.StartPosition, currentTextSelection.EndPosition);
    }
    public bool TextIsSelected(CursorPosition start, CursorPosition end)
    {
        if (start.IsNull || end.IsNull)
            return false;

        return start.LineNumber != end.LineNumber || 
            start.CharacterPosition != end.CharacterPosition;
    }
    public void SelectSingleWord(CanvasUpdateManager canvashelper)
    {
        int characterpos = cursorManager.CharacterPosition;

        //Update variables
        selectionRenderer.SetSelectionStart(cursorManager.LineNumber, characterpos - cursorManager.CalculateStepsToMoveLeftNoControl(characterpos));
        selectionRenderer.SetSelectionEnd(cursorManager.LineNumber, characterpos + cursorManager.CalculateStepsToMoveRightNoControl(characterpos));

        cursorManager.CharacterPosition = selectionRenderer.renderedSelectionEndPosition.CharacterPosition;
        selectionRenderer.HasSelection = true;

        //Render it
        canvashelper.UpdateSelection();
        canvashelper.UpdateCursor();
    }

    public (int start, int length) CalculateSelectionStartLength()
    {
        //order the selection and check if it's null

        var (startNull, endNull, startLine, startChar, endLine, endChar) = OrderTextSelectionSeparated(this.currentTextSelection);
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