using Collections.Pooled;
using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using TextControlBox.Extensions;
using TextControlBox.Helper;
using TextControlBox.Renderer;
using TextControlBox_WinUI.Helper;
using TextControlBox_WinUI.Models;

namespace TextControlBox.Text
{
    internal class Selection
    {
        public static bool Equals(TextSelection sel1, TextSelection sel2)
        {
            if (sel1 == null || sel2 == null)
                return false;

            return Cursor.Equals(sel1.StartPosition, sel2.StartPosition) &&
                Cursor.Equals(sel1.EndPosition, sel2.EndPosition);
        }

        //Order the selection that StartPosition is always smaller than EndPosition
        public static TextSelection OrderTextSelection(TextSelection selection)
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

        public static bool WholeTextSelected(TextManager textManager, TextSelection selection)
        {
            if (selection == null)
                return false;
            var sel = OrderTextSelection(selection);
            return Utils.CursorPositionsAreEqual(sel.StartPosition, new CursorPosition(0, 0)) &&
                Utils.CursorPositionsAreEqual(sel.EndPosition, new CursorPosition(textManager.Lines.GetLineLength(-1), textManager.Lines.Count - 1));
        }
        //returns whether the selection starts at character zero and ends 
        public static bool WholeLinesAreSelected(TextSelection selection, TextManager textManager)
        {
            if (selection == null)
                return false;
            var sel = OrderTextSelection(selection);
            return Utils.CursorPositionsAreEqual(sel.StartPosition, new CursorPosition(0, sel.StartPosition.LineNumber)) &&
                Utils.CursorPositionsAreEqual(sel.EndPosition, new CursorPosition(textManager.Lines.GetLineText(sel.EndPosition.LineNumber).Length, sel.EndPosition.LineNumber));
        }

        public static CursorPosition GetMax(CursorPosition pos1, CursorPosition pos2)
        {
            if (pos1.LineNumber == pos2.LineNumber)
                return pos1.CharacterPosition > pos2.CharacterPosition ? pos1 : pos2;
            return pos1.LineNumber > pos2.LineNumber ? pos1 : pos2;
        }
        public static CursorPosition GetMin(CursorPosition pos1, CursorPosition pos2)
        {
            if (pos1.LineNumber == pos2.LineNumber)
                return pos1.CharacterPosition > pos2.CharacterPosition ? pos2 : pos1;
            return pos1.LineNumber > pos2.LineNumber ? pos2 : pos1;
        }
        public static CursorPosition GetMin(TextSelection selection)
        {
            return GetMin(selection.StartPosition, selection.EndPosition);
        }
        public static CursorPosition GetMax(TextSelection selection)
        {
            return GetMax(selection.StartPosition, selection.EndPosition);
        }

        public static CursorPosition InsertText(TextManager textManager, TextControlBoxProperties textBoxProps, TextSelection selection, CursorPosition cursorPosition, string text)
        {
            if (selection != null)
                return Replace(textManager, textBoxProps, selection, text);

            string curLine = textManager.Lines.GetLineText(cursorPosition.LineNumber);

            string[] lines = text.Split(textBoxProps.NewLineCharacter);

            //Singleline
            if (lines.Length == 1 && text != string.Empty)
            {
                text = text.Replace("\r", string.Empty).Replace("\n", string.Empty);
                textManager.Lines.SetLineText(-1, textManager.Lines.GetLineText(-1).AddText(text, cursorPosition.CharacterPosition));
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

            textManager.Lines.DeleteAt(cursorPosition.LineNumber);
            textManager.Lines.InsertOrAddRange(ListHelper.CreateLines(lines, 0, textInFrontOfCursor, textBehindCursor), cursorPosition.LineNumber);

            return new CursorPosition(cursorPosition.CharacterPosition + lines.Length > 0 ? lines[lines.Length - 1].Length : 0, cursorPosition.LineNumber + lines.Length - 1);
        }

        public static CursorPosition Replace(TextManager textManager, TextControlBoxProperties textBoxProps, TextSelection selection, string text)
        {
            //Just delete the text if the string is empty
            if (text == "")
            {
                return Remove(textManager, selection);
            }

            selection = OrderTextSelection(selection);
            int startLine = selection.StartPosition.LineNumber;
            int endLine = selection.EndPosition.LineNumber;
            int startPosition = selection.StartPosition.CharacterPosition;
            int endPosition = selection.EndPosition.CharacterPosition;

            string[] lines = text.Split(textBoxProps.NewLineCharacter);
            string start_Line = textManager.Lines.GetLineText(startLine);

            //Selection is singleline and text to paste is also singleline
            if (startLine == endLine && lines.Length == 1)
            {
                if (startPosition == 0 && endPosition == textManager.Lines.GetLineLength(endLine))
                    start_Line = "";
                else
                    start_Line = start_Line.SafeRemove(startPosition, endPosition - startPosition);

                textManager.Lines.SetLineText(startLine, start_Line.AddText(text, startPosition));

                return new CursorPosition(startPosition + text.Length, selection.StartPosition.LineNumber);
            }
            else if (startLine == endLine && lines.Length > 1 && (startPosition != 0 && endPosition != start_Line.Length))
            {
                string textTo = start_Line == "" ? "" : startPosition >= start_Line.Length ? start_Line : start_Line.Safe_Substring(0, startPosition);
                string textFrom = start_Line == "" ? "" : endPosition >= start_Line.Length ? start_Line : start_Line.Safe_Substring(endPosition);

                textManager.Lines.SetLineText(startLine, (textTo + lines[0]));

                textManager.Lines.InsertOrAddRange(ListHelper.CreateLines(lines, 1, "", textFrom), startLine + 1);
                //ListHelper.Insert(textManager.Lines, new Line(lines[lines.Length - 1] + TextFrom), StartLine + 1);

                return new CursorPosition(endPosition + text.Length, startLine + lines.Length - 1);
            }
            else if (WholeTextSelected(textManager, selection))
            {
                if (lines.Length < textManager.Lines.Count)
                {
                    ListHelper.Clear(textManager.Lines);
                    textManager.Lines.InsertOrAddRange(lines, 0);
                }
                else
                    ReplaceLines(textManager, 0, textManager.Lines.Count, lines);

                return new CursorPosition(textManager.Lines.GetLineLength(-1), textManager.Lines.Count - 1);
            }
            else
            {
                string end_Line = textManager.Lines.GetLineText(endLine);

                //All lines are selected from start to finish
                if (startPosition == 0 && endPosition == end_Line.Length)
                {
                    textManager.Lines.Safe_RemoveRange(startLine, endLine - startLine + 1);
                    textManager.Lines.InsertOrAddRange(lines, startLine);
                }
                //Only the startline is completely selected
                else if (startPosition == 0 && endPosition != end_Line.Length)
                {
                    textManager.Lines.SetLineText(endLine, end_Line.Substring(endPosition).AddToStart(lines[lines.Length - 1]));

                    textManager.Lines.Safe_RemoveRange(startLine, endLine - startLine);
                    textManager.Lines.InsertOrAddRange(lines.Take(lines.Length - 1), startLine);
                }
                //Only the endline is completely selected
                else if (startPosition != 0 && endPosition == end_Line.Length)
                {
                    textManager.Lines.SetLineText(startLine, start_Line.SafeRemove(startPosition).AddToEnd(lines[0]));

                    textManager.Lines.Safe_RemoveRange(startLine + 1, endLine - startLine);
                    textManager.Lines.InsertOrAddRange(lines.Skip(1), startLine + 1);
                }
                else
                {
                    //Delete the selected parts
                    start_Line = start_Line.SafeRemove(startPosition);
                    end_Line = end_Line.Safe_Substring(endPosition);

                    //Only one line to insert
                    if (lines.Length == 1)
                    {
                        textManager.Lines.SetLineText(startLine, start_Line.AddToEnd(lines[0] + end_Line));
                        textManager.Lines.Safe_RemoveRange(startLine + 1, endLine - startLine < 0 ? 0 : endLine - startLine);
                    }
                    else
                    {
                        textManager.Lines.SetLineText(startLine, start_Line.AddToEnd(lines[0]));
                        textManager.Lines.SetLineText(endLine, end_Line.AddToStart(lines[lines.Length - 1]));

                        textManager.Lines.Safe_RemoveRange(startLine + 1, endLine - startLine - 1 < 0 ? 0 : endLine - startLine - 1);
                        if (lines.Length > 2)
                        {
                            textManager.Lines.InsertOrAddRange(lines.GetLines(1, lines.Length - 2), startLine + 1);
                        }
                    }
                }
                return new CursorPosition(start_Line.Length + end_Line.Length - 1, startLine + lines.Length - 1);
            }
        }

        public static CursorPosition Remove(TextManager textManager, TextSelection selection)
        {
            selection = OrderTextSelection(selection);
            int startLine = selection.StartPosition.LineNumber;
            int endLine = selection.EndPosition.LineNumber;
            int startPosition = selection.StartPosition.CharacterPosition;
            int endPosition = selection.EndPosition.CharacterPosition;

            string start_Line = textManager.Lines.GetLineText(startLine);
            string end_Line = textManager.Lines.GetLineText(endLine);

            if (startLine == endLine)
            {
                if (startPosition == 0 && endPosition == end_Line.Length)
                    textManager.Lines.SetLineText(startLine, "");
                else
                    textManager.Lines.SetLineText(startLine, start_Line.SafeRemove(startPosition, endPosition - startPosition));
            }
            else if (WholeTextSelected(textManager, selection))
            {
                ListHelper.Clear(textManager.Lines, true);
                return new CursorPosition(0, textManager.Lines.Count - 1);
            }
            else
            {
                //Whole lines are selected from start to finish
                if (startPosition == 0 && endPosition == end_Line.Length)
                {
                    textManager.Lines.Safe_RemoveRange(startLine, endLine - startLine + 1);
                }
                //Only the startline is completely selected
                else if (startPosition == 0 && endPosition != end_Line.Length)
                {
                    textManager.Lines.SetLineText(endLine, end_Line.Safe_Substring(endPosition));
                    textManager.Lines.Safe_RemoveRange(startLine, endLine - startLine);
                }
                //Only the endline is completely selected
                else if (startPosition != 0 && endPosition == end_Line.Length)
                {
                    textManager.Lines.SetLineText(startLine, start_Line.SafeRemove(startPosition));
                    textManager.Lines.Safe_RemoveRange(startLine + 1, endLine - startLine);
                }
                //Both startline and endline are not completely selected
                else
                {
                    textManager.Lines.SetLineText(startLine, start_Line.SafeRemove(startPosition) + end_Line.Safe_Substring(endPosition));
                    textManager.Lines.Safe_RemoveRange(startLine + 1, endLine - startLine);
                }
            }

            if (textManager.Lines.Count == 0)
                textManager.Lines.AddLine();

            return new CursorPosition(startPosition, startLine);
        }

        public static TextSelectionPosition GetIndexOfSelection(TextManager textManager, TextSelection selection)
        {
            var sel = OrderTextSelection(selection);
            int startIndex = Cursor.CursorPositionToIndex(textManager.Lines, sel.StartPosition);
            int endIndex = Cursor.CursorPositionToIndex(textManager.Lines, sel.EndPosition);

            int selectionLength;
            if (endIndex > startIndex)
                selectionLength = endIndex - startIndex;
            else
                selectionLength = startIndex - endIndex;

            return new TextSelectionPosition(Math.Min(startIndex, endIndex), selectionLength);
        }

        public static TextSelection GetSelectionFromPosition(TextManager textManager, int startPosition, int length, int numberOfCharacters)
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
                {
                    length = numberOfCharacters - startPosition;
                }
            }

            void GetIndexInLine(int CurrentIndex, int CurrentTotalLength)
            {
                int position = Math.Abs(CurrentTotalLength - startPosition);

                returnValue.StartPosition =
                    new CursorPosition(position, CurrentIndex);

                if (length == 0)
                    returnValue.EndPosition = new CursorPosition(returnValue.StartPosition);
                else
                {
                    int lengthCount = 0;
                    for (int i = CurrentIndex; i < textManager.Lines.Count; i++)
                    {
                        int lineLength = textManager.Lines[i].Length + 1;
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
            for (int i = 0; i < textManager.Lines.Count; i++)
            {
                int lineLength = textManager.Lines[i].Length + 1;
                if (totalLength + lineLength > startPosition)
                {
                    GetIndexInLine(i, totalLength);
                    break;
                }

                totalLength += lineLength;
            }
            return returnValue;
        }

        public static string GetSelectedText(TextManager textManager, TextControlBoxProperties textBoxProps, TextSelection textSelection, int currentLineIndex)
        {
            //return the current line, if no text is selected:
            if (textSelection == null)
            {
                return textManager.Lines.GetLineText(currentLineIndex) + textBoxProps.NewLineCharacter;
            }

            int startLine = Math.Min(textSelection.StartPosition.LineNumber, textSelection.EndPosition.LineNumber);
            int endLine = Math.Max(textSelection.StartPosition.LineNumber, textSelection.EndPosition.LineNumber);
            int endIndex = Math.Max(textSelection.StartPosition.CharacterPosition, textSelection.EndPosition.CharacterPosition);
            int startIndex = Math.Min(textSelection.StartPosition.CharacterPosition, textSelection.EndPosition.CharacterPosition);

            StringBuilder stringBuilder = new StringBuilder();

            if (startLine == endLine) //Singleline
            {
                string line = textManager.Lines.GetLineText(startLine < textManager.Lines.Count ? startLine : textManager.Lines.Count - 1);

                if (startIndex == 0 && endIndex != line.Length)
                    stringBuilder.Append(line.SafeRemove(endIndex));
                else if (endIndex == line.Length && startIndex != 0)
                    stringBuilder.Append(line.Safe_Substring(startIndex));
                else if (startIndex == 0 && endIndex == line.Length)
                    stringBuilder.Append(line);
                else stringBuilder.Append(line.SafeRemove(endIndex).Substring(startIndex));
            }
            else if (WholeTextSelected(textManager, textSelection))
            {
                stringBuilder.Append(ListHelper.GetLinesAsString(textManager.Lines, textBoxProps.NewLineCharacter));
            }
            else //Multiline
            {
                //StartLine
                stringBuilder.Append(textManager.Lines.GetLineText(startLine).Substring(startIndex) + textBoxProps.NewLineCharacter);

                //Other lines
                if (endLine - startLine > 1)
                    stringBuilder.Append(textManager.Lines.GetLines(startLine + 1, endLine - startLine - 1).GetString(textBoxProps.NewLineCharacter) + textBoxProps.NewLineCharacter);

                //Endline
                string CurrentLine = textManager.Lines.GetLineText(endLine);

                stringBuilder.Append(endIndex >= CurrentLine.Length ? CurrentLine : CurrentLine.SafeRemove(endIndex));
            }
            return stringBuilder.ToString();
        }

        /// <summary>
        /// Replaces the lines in textManager.Lines, starting by Start replacing Count number of items, with the string in SplittedText
        /// All lines that can be replaced get replaced all lines that are needed additionally get added
        /// </summary>
        /// <param name="textManager.Lines"></param>
        /// <param name="start"></param>
        /// <param name="count"></param>
        /// <param name="splittedText"></param>
        public static void ReplaceLines(TextManager textManager, int start, int count, string[] splittedText)
        {
            if (splittedText.Length == 0)
            {
                textManager.Lines.Safe_RemoveRange(start, count);
                return;
            }

            //Same line-length -> check for any differences in the individual lines
            if (count == splittedText.Length)
            {
                for (int i = 0; i < count; i++)
                {
                    if (!textManager.Lines.GetLineText(i).Equals(splittedText[i], StringComparison.Ordinal))
                    {
                        textManager.Lines.SetLineText(start + i, splittedText[i]);
                    }
                }
            }
            //Delete items from Start to Count; Insert SplittedText at Start
            else if (count > splittedText.Length)
            {
                for (int i = 0; i < count; i++)
                {
                    if (i < splittedText.Length)
                    {
                        textManager.Lines.SetLineText(start + i, splittedText[i]);
                    }
                    else
                    {
                        textManager.Lines.Safe_RemoveRange(start + i, count - i);
                        break;
                    }
                }
            }
            //Replace all items from Start - Count with existing (add more if out of range)
            else //SplittedText.Length > Count:
            {
                for (int i = 0; i < splittedText.Length; i++)
                {
                    //replace all possible lines
                    if (i < count)
                    {
                        textManager.Lines.SetLineText(start + i, splittedText[i]);
                    }
                    else //Add new lines
                    {
                        textManager.Lines.InsertOrAddRange(splittedText.Skip(start + i), start + i);
                        break;
                    }
                }
            }
        }
        public static bool MoveLinesUp(TextManager textManager, TextSelection selection, CursorPosition cursorposition)
        {
            //Move single line
            if (selection == null)
            {
                if (cursorposition.LineNumber > 0)
                {
                    textManager.Lines.SwapLines(cursorposition.LineNumber, cursorposition.LineNumber - 1);
                    cursorposition.LineNumber -= 1;
                    return true;
                }
            }
            //Can not move whole text
            //else if (WholeTextSelected(selection, textManager.Lines))
            //{
            //    return null;
            //}
            ////Move selected lines
            //else
            //{
            //    selection = OrderTextSelection(selection);
            //    if (selection.StartPosition.LineNumber > 0)
            //    {
            //        string aboveLineText = textManager.Lines.GetLineText(selection.StartPosition.LineNumber - 1);
            //        textManager.Lines.RemoveAt(selection.StartPosition.LineNumber - 1);
            //        textManager.Lines.InsertOrAdd(selection.EndPosition.LineNumber, aboveLineText);

            //        selection.StartPosition.ChangeLineNumber(selection.StartPosition.LineNumber - 1);
            //        selection.EndPosition.ChangeLineNumber(selection.EndPosition.LineNumber - 1);
            //        return selection;
            //    }
            //}
            return false;
        }
        public static bool MoveLinesDown(TextManager textManager, TextSelection selection, CursorPosition cursorposition)
        {
            //Move single line
            if (selection == null || selection.StartPosition.LineNumber == selection.EndPosition.LineNumber)
            {
                if (cursorposition.LineNumber < textManager.Lines.Count)
                {
                    textManager.Lines.SwapLines(cursorposition.LineNumber, cursorposition.LineNumber + 1);
                    cursorposition.LineNumber += 1;
                    return true;
                }
            }
            //Can not move whole text
            //else if (WholeTextSelected(selection, textManager.Lines))
            //{
            //    return null;
            //}
            //Move selected lines
            //else
            //{
            //    selection = OrderTextSelection(selection);
            //    if (selection.EndPosition.LineNumber + 1 < textManager.Lines.Count)
            //    {
            //        string aboveLineText = textManager.Lines.GetLineText(selection.EndPosition.LineNumber + 1);
            //        textManager.Lines.RemoveAt(selection.EndPosition.LineNumber + 1);
            //        textManager.Lines.InsertOrAdd(selection.StartPosition.LineNumber, aboveLineText);

            //        selection.StartPosition.ChangeLineNumber(selection.StartPosition.LineNumber + 1);
            //        selection.EndPosition.ChangeLineNumber(selection.EndPosition.LineNumber + 1);
            //        return selection;
            //    }
            //}
            return false;
        }
        public static void SelectSingleWord(CanvasUpdateHelper canvasUpdater, SelectionManager selectionManager, CurrentWorkingLine workingLine, CursorPosition cursorPosition)
        {
            int characterpos = cursorPosition.CharacterPosition;
            //Update variables
            selectionManager.Selection.StartPosition =
                new CursorPosition(characterpos - Cursor.CalculateStepsToMoveLeft2(workingLine.Text, characterpos), cursorPosition.LineNumber);

            selectionManager.Selection.EndPosition =
                new CursorPosition(characterpos + Cursor.CalculateStepsToMoveRight2(workingLine.Text, characterpos), cursorPosition.LineNumber);

            cursorPosition.CharacterPosition = selectionManager.Selection.EndPosition.CharacterPosition;

            //Render it
            canvasUpdater.UpdateSelection().UpdateCursor();
        }
    }
}