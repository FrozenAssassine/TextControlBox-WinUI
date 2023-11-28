using Collections.Pooled;
using Microsoft.UI.Xaml;
using TextControlBox.Extensions;
using TextControlBox.Helper;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace TextControlBox.Text
{
    internal class Cursor
    {
        private static bool ControlIsPressed { get => Utils.IsKeyPressed(Windows.System.VirtualKey.Control); }

        private static int CheckIndex(string str, int index)
        {
            if (index >= str.Length)
                index = str.Length - 1;
            if (index < 0)
                return 0;
            return index;
        }
        public static int CursorPositionToIndex(PooledList<string> TotalLines, CursorPosition CursorPosition)
        {
            int CursorIndex = CursorPosition.CharacterPosition;
            int LineNumber = CursorPosition.LineNumber < TotalLines.Count ? CursorIndex : TotalLines.Count - 1;
            for (int i = 0; i < LineNumber; i++)
            {
                CursorIndex += TotalLines.GetLineLength(i) + 1;
            }
            return CursorIndex;
        }

        public static bool Equals(CursorPosition CurPos1, CursorPosition CurPos2)
        {
            if (CurPos1 == null || CurPos2 == null)
                return false;

            if (CurPos1.LineNumber == CurPos2.LineNumber)
                return CurPos1.CharacterPosition == CurPos2.CharacterPosition;
            return false;
        }

        //Calculate the number of characters from the cursorposition to the next character or digit to the left and to the right
        public static int CalculateStepsToMoveLeft2(string CurrentLine, int CursorCharPosition)
        {
            if (CurrentLine.Length == 0)
                return 0;

            int Count = 0;
            for (int i = CursorCharPosition - 1; i >= 0; i--)
            {
                char CurrentCharacter = CurrentLine[CheckIndex(CurrentLine, i)];
                if (char.IsLetterOrDigit(CurrentCharacter) || CurrentCharacter == '_')
                    Count++;
                else if (i == CursorCharPosition - 1 && char.IsWhiteSpace(CurrentCharacter))
                    return 0;
                else
                    break;
            }
            return Count;
        }
        public static int CalculateStepsToMoveRight2(string CurrentLine, int CursorCharPosition)
        {
            if (CurrentLine.Length == 0)
                return 0;

            int Count = 0;
            for (int i = CursorCharPosition; i < CurrentLine.Length; i++)
            {
                char CurrentCharacter = CurrentLine[CheckIndex(CurrentLine, i)];
                if (char.IsLetterOrDigit(CurrentCharacter) || CurrentCharacter == '_')
                    Count++;
                else if (i == CursorCharPosition && char.IsWhiteSpace(CurrentCharacter))
                    return 0;
                else
                    break;
            }
            return Count;
        }

        //Calculates how many characters the cursor needs to move if control is pressed
        //Returns 1 when control is not pressed
        public static int CalculateStepsToMoveLeft(string CurrentLine, int CursorCharPosition)
        {
            if (!ControlIsPressed)
                return 1;
            int Count = 0;
            for (int i = CursorCharPosition - 1; i >= 0; i--)
            {
                char CurrentCharacter = CurrentLine[CheckIndex(CurrentLine, i)];
                if (char.IsLetterOrDigit(CurrentCharacter) || CurrentCharacter == '_')
                    Count++;
                else if (i == CursorCharPosition - 1 && char.IsWhiteSpace(CurrentCharacter))
                    Count++;
                else
                    break;
            }
            //If it ignores the ControlKey return the real value of Count otherwise
            //return 1 if Count is 0
            return Count == 0 ? 1 : Count;
        }
        public static int CalculateStepsToMoveRight(string CurrentLine, int CursorCharPosition)
        {
            if (!ControlIsPressed)
                return 1;
            int Count = 0;
            for (int i = CursorCharPosition; i < CurrentLine.Length; i++)
            {
                char CurrentCharacter = CurrentLine[CheckIndex(CurrentLine, i)];
                if (char.IsLetterOrDigit(CurrentCharacter) || CurrentCharacter == '_')
                    Count++;
                else if (i == CursorCharPosition && char.IsWhiteSpace(CurrentCharacter))
                    Count++;
                else
                    break;
            }
            //If it ignores the ControlKey return the real value of Count otherwise
            //return 1 if Count is 0
            return Count == 0 ? 1 : Count;
        }

        //Move cursor:
        public static void MoveLeft(CursorPosition CurrentCursorPosition, PooledList<string> TotalLines, string CurrentLine)
        {
            if (CurrentCursorPosition.LineNumber < 0)
                return;
            else
            {
                int currentLineLength = TotalLines.GetLineLength(CurrentCursorPosition.LineNumber);
                if (CurrentCursorPosition.CharacterPosition == 0 && CurrentCursorPosition.LineNumber > 0)
                {
                    CurrentCursorPosition.CharacterPosition = TotalLines.GetLineLength(CurrentCursorPosition.LineNumber - 1);
                    CurrentCursorPosition.LineNumber -= 1;
                }
                else if (CurrentCursorPosition.CharacterPosition > currentLineLength)
                {
                    CurrentCursorPosition.CharacterPosition = currentLineLength - 1;
                }
                else if (CurrentCursorPosition.CharacterPosition > 0)
                {
                    CurrentCursorPosition.CharacterPosition -= CalculateStepsToMoveLeft(CurrentLine, CurrentCursorPosition.CharacterPosition);
                }
            }
        }
        public static void MoveRight(CursorPosition CurrentCursorPosition, PooledList<string> TotalLines, string CurrentLine)
        {
            int LineLength = TotalLines.GetLineLength(CurrentCursorPosition.LineNumber);

            if (CurrentCursorPosition.LineNumber > TotalLines.Count - 1)
            {
                return;
            }
            else
            {
                if (CurrentCursorPosition.CharacterPosition == LineLength && CurrentCursorPosition.LineNumber < TotalLines.Count - 1)
                {
                    CurrentCursorPosition.CharacterPosition = 0;
                    CurrentCursorPosition.LineNumber += 1;
                }
                else if (CurrentCursorPosition.CharacterPosition < LineLength)
                {
                    CurrentCursorPosition.CharacterPosition += CalculateStepsToMoveRight(CurrentLine, CurrentCursorPosition.CharacterPosition);
                }
            }

            if (CurrentCursorPosition.CharacterPosition > LineLength)
                CurrentCursorPosition.CharacterPosition = LineLength;
        }
        public static CursorPosition MoveDown(CursorPosition CurrentCursorPosition, int TotalLinesLength)
        {
            CursorPosition ReturnValue = new CursorPosition(CurrentCursorPosition);
            if (CurrentCursorPosition.LineNumber < TotalLinesLength - 1)
                ReturnValue = CursorPosition.ChangeLineNumber(CurrentCursorPosition, CurrentCursorPosition.LineNumber + 1);
            return ReturnValue;
        }
        public static CursorPosition MoveUp(CursorPosition CurrentCursorPosition)
        {
            CursorPosition ReturnValue = new CursorPosition(CurrentCursorPosition);
            if (CurrentCursorPosition.LineNumber > 0)
                ReturnValue = CursorPosition.ChangeLineNumber(ReturnValue, CurrentCursorPosition.LineNumber - 1);
            return ReturnValue;
        }
        public static void MoveToLineEnd(CursorPosition CursorPosition, string CurrentLine)
        {
            CursorPosition.CharacterPosition = CurrentLine.Length;
        }

        public static void MoveToLineStart(CursorPosition CursorPosition)
        {
            CursorPosition.CharacterPosition = 0;
        }
    }
}