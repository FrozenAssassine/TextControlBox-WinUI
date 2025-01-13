using System;
using System.Linq;
using System.Text;
using TextControlBoxNS.Extensions;
using TextControlBoxNS.Helper;
using TextControlBoxNS.Renderer;

namespace TextControlBoxNS.Text
{
    internal class SelectionManager
    {
        private TextManager textManager;
        private CursorManager cursorManager;

        public TextSelection OldTextSelection = null;
        public TextSelection currentTextSelection = null;

        public bool TextSelIsNull => currentTextSelection == null;

        public void ForceClearSelection(SelectionRenderer selectionRenderer, CanvasUpdateManager canvasHelper)
        {
            selectionRenderer.ClearSelection();
            SetCurrentTextSelection(null);
            canvasHelper.UpdateSelection();
        }

        public void SetCurrentTextSelection(TextSelection textSelection)
        {
            this.currentTextSelection = textSelection;
        }
        public void StartSelectionIfNeeded(SelectionRenderer selectionRenderer)
        {
            if (this.SelectionIsNull(selectionRenderer, currentTextSelection))
            {
                selectionRenderer.SelectionStartPosition = selectionRenderer.SelectionEndPosition = new CursorPosition(cursorManager.currentCursorPosition.CharacterPosition, cursorManager.currentCursorPosition.LineNumber);
                currentTextSelection = new TextSelection(selectionRenderer.SelectionStartPosition, selectionRenderer.SelectionEndPosition);
            }
        }
        public void Init(TextManager textManager, CursorManager cursorManager)
        {
            this.textManager = textManager;
            this.cursorManager = cursorManager;
        }

        public bool Equals(TextSelection sel1, TextSelection sel2)
        {
            if (sel1 == null || sel2 == null)
                return false;

            return cursorManager.Equals(sel1.StartPosition, sel2.StartPosition) &&
                cursorManager.Equals(sel1.EndPosition, sel2.EndPosition);
        }

        //Order the selection that StartPosition is always smaller than EndPosition
        public TextSelection OrderTextSelection(TextSelection selection)
        {
            if (selection == null)
                return selection;

            int startLine = Math.Min(selection.StartPosition.LineNumber, selection.EndPosition.LineNumber);
            int endLine = Math.Max(selection.StartPosition.LineNumber, selection.EndPosition.LineNumber);
            int startPosition;
            int endPosition;
            if (startLine == endLine)
            {
                startPosition = Math.Min(selection.StartPosition.CharacterPosition, selection.EndPosition.CharacterPosition);
                endPosition = Math.Max(selection.StartPosition.CharacterPosition, selection.EndPosition.CharacterPosition);
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

            return new TextSelection(selection.Index, selection.Length, new CursorPosition(startPosition, startLine), new CursorPosition(endPosition, endLine));
        }

        public bool WholeTextSelected(TextSelection selection)
        {
            if (selection == null)
                return false;

            var sel = OrderTextSelection(selection);
            return Utils.CursorPositionsAreEqual(sel.StartPosition, new CursorPosition(0, 0)) &&
                Utils.CursorPositionsAreEqual(sel.EndPosition, new CursorPosition(textManager.GetLineLength(-1), textManager.LinesCount - 1));
        }

        //returns whether the selection starts at character zero and ends
        public bool WholeLinesAreSelected(TextSelection selection)
        {
            if (selection == null)
                return false;

            var sel = OrderTextSelection(selection);
            return Utils.CursorPositionsAreEqual(sel.StartPosition, new CursorPosition(0, sel.StartPosition.LineNumber)) &&
                Utils.CursorPositionsAreEqual(sel.EndPosition, new CursorPosition(textManager.GetLineText(sel.EndPosition.LineNumber).Length, sel.EndPosition.LineNumber));
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

        public CursorPosition InsertText(TextSelection selection, CursorPosition cursorPosition, string text)
        {
            if (selection != null)
                return this.Replace(selection, text);

            string curLine = textManager.GetLineText(cursorPosition.LineNumber);

            string[] lines = text.Split(textManager.NewLineCharacter);

            //Singleline
            if (lines.Length == 1 && text != string.Empty)
            {
                text = text.Replace("\r", string.Empty).Replace("\n", string.Empty);
                textManager.SetLineText(-1, textManager.GetLineText(-1).AddText(text, cursorPosition.CharacterPosition));
                cursorPosition.AddToCharacterPos(text.Length);
                return cursorPosition;
            }

            //Multiline:
            int curPos = cursorPosition.CharacterPosition;
            if (curPos > curLine.Length)
                curPos = curLine.Length;

            //GEt the text in front of the cursor
            string textInFrontOfCursor = curLine.Substring(0, curPos < 0 ? 0 : curPos);
            //Get the text behind the cursor
            string textBehindCursor = curLine.SafeRemove(0, curPos < 0 ? 0 : curPos);

            textManager.DeleteAt(cursorPosition.LineNumber);
            textManager.InsertOrAddRange(ListHelper.CreateLines(lines, 0, textInFrontOfCursor, textBehindCursor), cursorPosition.LineNumber);

            return new CursorPosition(cursorPosition.CharacterPosition + lines.Length > 0 ? lines[lines.Length - 1].Length : 0, cursorPosition.LineNumber + lines.Length - 1);
        }

        public CursorPosition Replace(TextSelection selection, string text)
        {
            //Just delete the text if the string is emty
            if (text == "")
                return this.Remove(selection);

            selection = OrderTextSelection(selection);
            int startLine = selection.StartPosition.LineNumber;
            int endLine = selection.EndPosition.LineNumber;
            int startPosition = selection.StartPosition.CharacterPosition;
            int endPosition = selection.EndPosition.CharacterPosition;

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

                return new CursorPosition(startPosition + text.Length, selection.StartPosition.LineNumber);
            }
            else if (startLine == endLine && lines.Length > 1 && (startPosition != 0 && endPosition != start_Line.Length))
            {
                string textTo = start_Line == "" ? "" : startPosition >= start_Line.Length ? start_Line : start_Line.Safe_Substring(0, startPosition);
                string textFrom = start_Line == "" ? "" : endPosition >= start_Line.Length ? start_Line : start_Line.Safe_Substring(endPosition);

                textManager.SetLineText(startLine, (textTo + lines[0]));
                textManager.InsertOrAddRange(ListHelper.CreateLines(lines, 1, "", textFrom), startLine + 1);

                return new CursorPosition(endPosition + text.Length, startLine + lines.Length - 1);
            }
            else if (this.WholeTextSelected(selection))
            {
                if (lines.Length < textManager.LinesCount)
                {
                    textManager.ClearText();
                    textManager.InsertOrAddRange(lines, 0);
                }
                else
                    this.ReplaceLines(0, textManager.LinesCount, lines);

                return new CursorPosition(textManager.GetLineLength(-1), textManager.LinesCount - 1);
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
                    //TODO Out of range:
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
                return new CursorPosition(start_Line.Length + end_Line.Length - 1, startLine + lines.Length - 1);
            }
        }

        public CursorPosition Remove(TextSelection selection)
        {
            selection = OrderTextSelection(selection);
            int startLine = selection.StartPosition.LineNumber;
            int endLine = selection.EndPosition.LineNumber;
            int startPosition = selection.StartPosition.CharacterPosition;
            int endPosition = selection.EndPosition.CharacterPosition;

            string start_Line = textManager.GetLineText(startLine);
            string end_Line = textManager.GetLineText(endLine);

            if (startLine == endLine)
            {
                textManager.SetLineText(startLine,
                    (startPosition == 0 && endPosition == end_Line.Length ?
                    "" :
                    start_Line.SafeRemove(startPosition, endPosition - startPosition))
                );
            }
            else if (this.WholeTextSelected(selection))
            {
                textManager.ClearText(true);
                return new CursorPosition(0, textManager.LinesCount - 1);
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

            return new CursorPosition(startPosition, startLine);
        }

        public TextSelectionPosition GetIndexOfSelection(TextSelection selection)
        {
            var sel = OrderTextSelection(selection);
            int startIndex = cursorManager.CursorPositionToIndex(sel.StartPosition);
            int endIndex = cursorManager.CursorPositionToIndex(sel.EndPosition);

            if (endIndex > startIndex)
                return new TextSelectionPosition(Math.Min(startIndex, endIndex), endIndex - startIndex);
            else
                return new TextSelectionPosition(Math.Min(startIndex, endIndex), startIndex - endIndex);
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

                returnValue.StartPosition =
                    new CursorPosition(position, currentIndex);

                if (length == 0)
                    returnValue.EndPosition = new CursorPosition(returnValue.StartPosition);
                else
                {
                    int lengthCount = 0;
                    for (int i = currentIndex; i < textManager.LinesCount; i++)
                    {
                        int lineLength = textManager.GetLineLength(i) + 1;
                        if (lengthCount + lineLength > length)
                        {
                            returnValue.EndPosition = new CursorPosition(Math.Abs(lengthCount - length) + position, i);
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

        public string GetSelectedText(TextSelection textSelection, int currentLineIndex)
        {
            //return the current line, if no text is selected:
            if (textSelection == null)
                return textManager.GetLineText(currentLineIndex) + textManager.NewLineCharacter;

            int startLine = Math.Min(textSelection.StartPosition.LineNumber, textSelection.EndPosition.LineNumber);
            int endLine = Math.Max(textSelection.StartPosition.LineNumber, textSelection.EndPosition.LineNumber);
            int endIndex = Math.Max(textSelection.StartPosition.CharacterPosition, textSelection.EndPosition.CharacterPosition);
            int startIndex = Math.Min(textSelection.StartPosition.CharacterPosition, textSelection.EndPosition.CharacterPosition);

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
            else if (this.WholeTextSelected(textSelection))
            {
                stringBuilder.Append(textManager.GetLinesAsString());
            }
            else //Multiline
            {
                //StartLine
                stringBuilder.Append(textManager.GetLineText(startLine).Substring(startIndex) + textManager.NewLineCharacter);

                //Other lines
                if (endLine - startLine > 1)
                    stringBuilder.Append(textManager.GetLines(startLine + 1, endLine - startLine - 1).GetString(textManager.NewLineCharacter) + textManager.NewLineCharacter);

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
            if (selection != null || selection.StartPosition.LineNumber != selection.EndPosition.LineNumber)
                return false;

            if (cursorposition.LineNumber < textManager.LinesCount)
            {
                textManager.SwapLines(cursorposition.LineNumber, cursorposition.LineNumber + 1);
                cursorposition.LineNumber += 1;
                return true;
            }
            return false;
        }
        public void ClearSelectionIfNeeded(CoreTextControlBox textbox, SelectionRenderer selectionrenderer)
        {
            //If the selection is visible, but is not getting set, clear the selection
            if (selectionrenderer.HasSelection && !selectionrenderer.IsSelecting)
            {
                textbox.ClearSelection();
            }
        }


        public bool SelectionIsNull(SelectionRenderer selectionrenderer, TextSelection selection)
        {
            if (selection == null)
                return true;
            return selectionrenderer.SelectionStartPosition == null || selectionrenderer.SelectionEndPosition == null;
        }
        public void SelectSingleWord(CanvasUpdateManager canvashelper, SelectionRenderer selectionrenderer, CursorPosition cursorPosition)
        {
            int characterpos = cursorPosition.CharacterPosition;
            //Update variables
            selectionrenderer.SelectionStartPosition =
                new CursorPosition(characterpos - cursorManager.CalculateStepsToMoveLeft2(characterpos), cursorPosition.LineNumber);

            selectionrenderer.SelectionEndPosition =
                new CursorPosition(characterpos + cursorManager.CalculateStepsToMoveRight2(characterpos), cursorPosition.LineNumber);

            cursorPosition.CharacterPosition = selectionrenderer.SelectionEndPosition.CharacterPosition;
            selectionrenderer.HasSelection = true;

            //Render it
            canvashelper.UpdateSelection();
            canvashelper.UpdateCursor();
        }
    }
}