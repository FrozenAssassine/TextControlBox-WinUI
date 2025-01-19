using System;
using System.Linq;
using System.Text;
using TextControlBoxNS.Extensions;
using TextControlBoxNS.Helper;
using TextControlBoxNS.Core.Renderer;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Models;

namespace TextControlBoxNS.Core;

internal class SelectionManager
{
    private TextManager textManager;
    private CursorManager cursorManager;
    private SelectionRenderer selectionRenderer;

    public TextSelection selection = new TextSelection();
    public bool TextSelIsNull => !selection.HasStartPos || !selection.HasEndPos;
    
    public void Init(TextManager textManager, CursorManager cursorManager, SelectionRenderer selectionRenderer)
    {
        this.textManager = textManager;
        this.cursorManager = cursorManager;
        this.selectionRenderer = selectionRenderer;
    }

    public void ForceClearSelection(CanvasUpdateManager canvasHelper)
    {
        selectionRenderer.ClearSelection();
        SetCurrentTextSelection(null);
        canvasHelper.UpdateSelection();
    }

    public void SetCurrentTextSelection(TextSelection textSelection)
    {
        this.selection = textSelection;
    }
    
    public void StartSelectionIfNeeded()
    {
        if (this.SelectionIsStarted(selection))
        {
            selection.HasStartPos = true;
            selection.HasEndPos = true;
        }
    }

    public bool Equals(TextSelection sel1, TextSelection sel2)
    {
        if (sel1 == null || sel2 == null)
            return false;

        return sel1.OrderedStartLine == sel2.OrderedStartLine &&
            sel1.OrderedEndLine == sel2.OrderedEndLine &&
            sel1.OrderedStartCharacterPos == sel2.OrderedStartCharacterPos &&
            sel1.OrderedEndCharacterPos == sel2.OrderedEndCharacterPos;
    }

    public bool WholeTextSelected()
    {
        if (!selection.HasSelection)
            return false;

        return selection.OrderedStartLine == 0 && selection.OrderedStartCharacterPos == 0 &&
            selection.OrderedEndLine == textManager.LinesCount - 1 && selection.OrderedEndCharacterPos == textManager.GetLineLength(-1);
    }

    //returns whether the selection starts at character zero and ends
    public bool WholeLinesAreSelected(TextSelection selection)
    {
        if (selection == null)
            return false;

        return selection.OrderedStartCharacterPos == 0 && selection.OrderedEndCharacterPos == textManager.GetLineText(selection.OrderedEndLine).Length;
    }

    public void InsertText(string text)
    {
        if (selection != null)
            this.Replace(text);

        string curLine = textManager.GetLineText(selection.OrderedStartLine);

        string[] lines = text.Split(textManager.NewLineCharacter);

        //Singleline
        if (lines.Length == 1 && text != string.Empty)
        {
            text = text.Replace("\r", string.Empty).Replace("\n", string.Empty);
            textManager.SetLineText(-1, textManager.GetLineText(-1).AddText(text, selection.OrderedStartLine));
            selection.ActualStartCharacterPos += text.Length;
        }

        //Multiline:
        int curPos = selection.OrderedStartCharacterPos;
        if (curPos > curLine.Length)
            curPos = curLine.Length;

        //GEt the text in front of the cursor
        string textInFrontOfCursor = curLine.Substring(0, curPos < 0 ? 0 : curPos);
        //Get the text behind the cursor
        string textBehindCursor = curLine.SafeRemove(0, curPos < 0 ? 0 : curPos);

        textManager.DeleteAt(cursorManager.LineNumber);
        textManager.InsertOrAddRange(ListHelper.CreateLines(lines, 0, textInFrontOfCursor, textBehindCursor), cursorManager.LineNumber);

        cursorManager.SetCursorPosition(cursorManager.CharacterPosition + lines.Length > 0 ? lines[lines.Length - 1].Length : 0, cursorManager.LineNumber + lines.Length - 1);
    }

    public void Replace(string text)
    {
        //Just delete the text if the string is emty
        if (text.Length == 0)
            this.Remove();

        int startLine = selection.OrderedStartLine;
        int endLine = selection.OrderedEndLine;
        int startPosition = selection.OrderedStartCharacterPos;
        int endPosition = selection.OrderedEndCharacterPos;

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

            selection.SetStartPos(selection.ActualStartLine, startPosition + text.Length, true);
        }
        else if (startLine == endLine && lines.Length > 1 && (startPosition != 0 && endPosition != start_Line.Length))
        {
            string textTo = start_Line == "" ? "" : startPosition >= start_Line.Length ? start_Line : start_Line.Safe_Substring(0, startPosition);
            string textFrom = start_Line == "" ? "" : endPosition >= start_Line.Length ? start_Line : start_Line.Safe_Substring(endPosition);

            textManager.SetLineText(startLine, (textTo + lines[0]));
            textManager.InsertOrAddRange(ListHelper.CreateLines(lines, 1, "", textFrom), startLine + 1);

            selection.SetStartPos(startLine + lines.Length - 1, endPosition + text.Length, true);
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
            
            selection.SetStartPos(textManager.LinesCount - 1, textManager.GetLineLength(-1));
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

            selection.SetStartPos(startLine + lines.Length - 1, start_Line.Length + end_Line.Length - 1, true);
        }
    }

    public void Remove()
    {
        int startLine = selection.OrderedStartLine;
        int endLine = selection.OrderedEndLine;
        int startPosition = selection.OrderedStartCharacterPos;
        int endPosition = selection.OrderedEndCharacterPos;

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
            cursorManager.LineNumber  = cursorManager.CharacterPosition = 0;
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

    public (int start, int length) GetIndexOfSelection(TextSelection selection)
    {
        int startIndex = cursorManager.CursorPositionToIndex(selection.OrderedStartLine, selection.OrderedStartCharacterPos);
        int endIndex = cursorManager.CursorPositionToIndex(selection.OrderedEndLine, selection.OrderedEndCharacterPos);

        if (endIndex > startIndex)
            return (Math.Min(startIndex, endIndex), endIndex - startIndex);
        else
            return (Math.Min(startIndex, endIndex), startIndex - endIndex);
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

            returnValue.ActualStartLine = currentIndex;
            returnValue.ActualStartCharacterPos = position;

            if (length == 0)
            {
                returnValue.ActualEndLine = returnValue.ActualStartLine;
                returnValue.ActualEndCharacterPos = returnValue.ActualEndCharacterPos;
            }
            else
            {
                int lengthCount = 0;
                for (int i = currentIndex; i < textManager.LinesCount; i++)
                {
                    int lineLength = textManager.GetLineLength(i) + 1;
                    if (lengthCount + lineLength > length)
                    {
                        returnValue.ActualEndCharacterPos = Math.Abs(lengthCount - length) + position;
                        returnValue.ActualEndLine = i;
                        break;
                    }
                    lengthCount += lineLength;
                }
            }
        }

        //Get the Length
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
        if (selection.HasSelection)
            return textManager.GetLineText(currentLineIndex) + textManager.NewLineCharacter;

        int startLine = selection.OrderedStartLine;
        int startIndex = selection.OrderedStartCharacterPos;
        int endLine = selection.OrderedEndLine;
        int endIndex = selection.OrderedEndCharacterPos;

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
    /// <param name="totalLines"></param>
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
        else //SplittedText.Length > Count:
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

    public bool MoveLinesUp()
    {
        //Move single line
        if (selection.HasSelection)
            return false;

        if (cursorManager.LineNumber > 0)
        {
            textManager.SwapLines(cursorManager.LineNumber, cursorManager.LineNumber - 1);
            cursorManager.LineNumber -= 1;
            return true;
        }
        return false;
    }
    
    public bool MoveLinesDown()
    {
        //Move single line
        if (selection.HasSelection)
            return false;

        if (cursorManager.LineNumber < textManager.LinesCount)
        {
            textManager.SwapLines(cursorManager.LineNumber, cursorManager.LineNumber + 1);
            cursorManager.LineNumber += 1;
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
    
    public bool SelectionIsStarted(TextSelection selection)
    {
        if (selection.HasSelection)
            return false;
        return selection.HasStartPos || selection.HasEndPos;
    }
    
    public void SelectSingleWord(CanvasUpdateManager canvashelper)
    {
        int characterpos = cursorManager.CharacterPosition;

        //Update variables
        selection.ActualStartCharacterPos = characterpos - cursorManager.CalculateStepsToMoveLeft2(characterpos);
        cursorManager.CharacterPosition = selection.ActualEndCharacterPos = characterpos + cursorManager.CalculateStepsToMoveRight2(characterpos);
        
        //Render it
        canvashelper.UpdateSelection();
        canvashelper.UpdateCursor();
    }
}