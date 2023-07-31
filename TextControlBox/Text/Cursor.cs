using Collections.Pooled;
using System;
using TextControlBoxNS.Extensions;
using TextControlBoxNS.Helper;

namespace TextControlBoxNS.Text
{
    internal class Cursor
    {
        private static int CheckIndex(string str, int index) => Math.Clamp(index, 0, str.Length - 1);
        public static int CursorPositionToIndex(PooledList<string> totalLines, CursorPosition cursorPosition)
        {
            int cursorIndex = cursorPosition.CharacterPosition;
            int lineNumber = cursorPosition.LineNumber < totalLines.Count ? cursorIndex : totalLines.Count - 1;
            for (int i = 0; i < lineNumber; i++)
            {
                cursorIndex += totalLines.GetLineLength(i) + 1;
            }
            return cursorIndex;
        }
        public static bool Equals(CursorPosition curPos1, CursorPosition curPos2)
        {
            if (curPos1 == null || curPos2 == null)
                return false;

            if (curPos1.LineNumber == curPos2.LineNumber)
                return curPos1.CharacterPosition == curPos2.CharacterPosition;
            return false;
        }

        //Calculate the number of characters from the cursorposition to the next character or digit to the left and to the right
        public static int CalculateStepsToMoveLeft2(string currentLine, int cursorCharPosition)
        {
            if (currentLine.Length == 0)
                return 0;

            int stepsToMove = 0;
            for (int i = cursorCharPosition - 1; i >= 0; i--)
            {
                char currentCharacter = currentLine[CheckIndex(currentLine, i)];
                if (char.IsLetterOrDigit(currentCharacter) || currentCharacter == '_')
                    stepsToMove++;
                else if (i == cursorCharPosition - 1 && char.IsWhiteSpace(currentCharacter))
                    return 0;
                else
                    break;
            }
            return stepsToMove;
        }
        public static int CalculateStepsToMoveRight2(string currentLine, int cursorCharPosition)
        {
            if (currentLine.Length == 0)
                return 0;

            int stepsToMove = 0;
            for (int i = cursorCharPosition; i < currentLine.Length; i++)
            {
                char currentCharacter = currentLine[CheckIndex(currentLine, i)];
                if (char.IsLetterOrDigit(currentCharacter) || currentCharacter == '_')
                    stepsToMove++;
                else if (i == cursorCharPosition && char.IsWhiteSpace(currentCharacter))
                    return 0;
                else
                    break;
            }
            return stepsToMove;
        }

        //Calculates how many characters the cursor needs to move if control is pressed
        //Returns 1 when control is not pressed
        public static int CalculateStepsToMoveLeft(string currentLine, int cursorCharPosition)
        {
            if (!Utils.IsKeyPressed(Windows.System.VirtualKey.Control))
                return 1;

            int stepsToMove = 0;
            for (int i = cursorCharPosition - 1; i >= 0; i--)
            {
                char CurrentCharacter = currentLine[CheckIndex(currentLine, i)];
                if (char.IsLetterOrDigit(CurrentCharacter) || CurrentCharacter == '_')
                    stepsToMove++;
                else if (i == cursorCharPosition - 1 && char.IsWhiteSpace(CurrentCharacter))
                    stepsToMove++;
                else
                    break;
            }
            //If it ignores the ControlKey return the real value of stepsToMove otherwise
            //return 1 if stepsToMove is 0
            return stepsToMove == 0 ? 1 : stepsToMove;
        }
        public static int CalculateStepsToMoveRight(string currentLine, int cursorCharPosition)
        {
            if (!Utils.IsKeyPressed(Windows.System.VirtualKey.Control))
                return 1;

            int stepsToMove = 0;
            for (int i = cursorCharPosition; i < currentLine.Length; i++)
            {
                char CurrentCharacter = currentLine[CheckIndex(currentLine, i)];
                if (char.IsLetterOrDigit(CurrentCharacter) || CurrentCharacter == '_')
                    stepsToMove++;
                else if (i == cursorCharPosition && char.IsWhiteSpace(CurrentCharacter))
                    stepsToMove++;
                else
                    break;
            }
            //If it ignores the ControlKey return the real value of stepsToMove otherwise
            //return 1 if stepsToMove is 0
            return stepsToMove == 0 ? 1 : stepsToMove;
        }

        //Move cursor:
        public static void MoveLeft(CursorPosition currentCursorPosition, PooledList<string> totalLines, string currentLine)
        {
            if (currentCursorPosition.LineNumber < 0)
                return;

            int currentLineLength = totalLines.GetLineLength(currentCursorPosition.LineNumber);
            if (currentCursorPosition.CharacterPosition == 0 && currentCursorPosition.LineNumber > 0)
            {
                currentCursorPosition.CharacterPosition = totalLines.GetLineLength(currentCursorPosition.LineNumber - 1);
                currentCursorPosition.LineNumber -= 1;
            }
            else if (currentCursorPosition.CharacterPosition > currentLineLength)
                currentCursorPosition.CharacterPosition = currentLineLength - 1;
            else if (currentCursorPosition.CharacterPosition > 0)
                currentCursorPosition.CharacterPosition -= CalculateStepsToMoveLeft(currentLine, currentCursorPosition.CharacterPosition);
        }
        public static void MoveRight(CursorPosition currentCursorPosition, PooledList<string> totalLines, string currentLine)
        {
            int lineLength = totalLines.GetLineLength(currentCursorPosition.LineNumber);

            if (currentCursorPosition.LineNumber > totalLines.Count - 1)
                return;

            if (currentCursorPosition.CharacterPosition == lineLength && currentCursorPosition.LineNumber < totalLines.Count - 1)
            {
                currentCursorPosition.CharacterPosition = 0;
                currentCursorPosition.LineNumber += 1;
            }
            else if (currentCursorPosition.CharacterPosition < lineLength)
                currentCursorPosition.CharacterPosition += CalculateStepsToMoveRight(currentLine, currentCursorPosition.CharacterPosition);

            if (currentCursorPosition.CharacterPosition > lineLength)
                currentCursorPosition.CharacterPosition = lineLength;
        }
        public static CursorPosition MoveDown(CursorPosition currentCursorPosition, int totalLinesLength)
        {
            CursorPosition returnValue = new CursorPosition(currentCursorPosition);
            if (currentCursorPosition.LineNumber < totalLinesLength - 1)
                returnValue = CursorPosition.ChangeLineNumber(currentCursorPosition, currentCursorPosition.LineNumber + 1);
            return returnValue;
        }
        public static CursorPosition MoveUp(CursorPosition currentCursorPosition)
        {
            CursorPosition returnValue = new CursorPosition(currentCursorPosition);
            if (currentCursorPosition.LineNumber > 0)
                returnValue = CursorPosition.ChangeLineNumber(returnValue, currentCursorPosition.LineNumber - 1);
            return returnValue;
        }
        public static void MoveToLineEnd(CursorPosition cursorPosition, string currentLine)
        {
            cursorPosition.CharacterPosition = currentLine.Length;
        }
        public static void MoveToLineStart(CursorPosition cursorPosition)
        {
            cursorPosition.CharacterPosition = 0;
        }
    }
}