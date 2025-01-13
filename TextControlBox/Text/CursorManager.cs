using System;
using TextControlBoxNS.Helper;

namespace TextControlBoxNS.Text;

internal class CursorManager
{
    public CursorPosition oldCursorPosition = null;
    public CursorPosition currentCursorPosition { get; private set; } = new CursorPosition(0, 0);
    public int LineNumber { get => currentCursorPosition.LineNumber; set => currentCursorPosition.LineNumber = value; }
    public int CharacterPosition { get => currentCursorPosition.CharacterPosition; set { currentCursorPosition.CharacterPosition = value; } }

    private TextManager textManager;
    private CurrentLineManager currentLineManager;
    public void Init(TextManager textManager, CurrentLineManager currentLineManager)
    {
        this.textManager = textManager;
        this.currentLineManager = currentLineManager;
    }

    public void SetCursorPosition(CursorPosition cursorPosition)
    {
        this.currentCursorPosition = cursorPosition;
    }

    public void SetCursorPositionCopyValues(CursorPosition cursorPosition)
    {
        this.currentCursorPosition.LineNumber = cursorPosition.LineNumber;
        this.currentCursorPosition.CharacterPosition = cursorPosition.CharacterPosition;
    }


    public int GetCurPosInLine()
    {
        int curLineLength = currentLineManager.Length;

        if (CharacterPosition > curLineLength)
            return curLineLength;
        return CharacterPosition;
    }


    private int CheckIndex(string str, int index) => Math.Clamp(index, 0, str.Length - 1);
    public int CursorPositionToIndex(CursorPosition cursorPosition)
    {
        int cursorIndex = cursorPosition.CharacterPosition;
        int lineNumber = cursorPosition.LineNumber < textManager.LinesCount ? cursorIndex : textManager.LinesCount - 1;
        for (int i = 0; i < lineNumber; i++)
        {
            cursorIndex += textManager.GetLineLength(i) + 1;
        }
        return cursorIndex;
    }
    public bool Equals(CursorPosition curPos1, CursorPosition curPos2)
    {
        if (curPos1 == null || curPos2 == null)
            return false;

        if (curPos1.LineNumber == curPos2.LineNumber)
            return curPos1.CharacterPosition == curPos2.CharacterPosition;
        return false;
    }

    //Calculate the number of characters from the cursorposition to the next character or digit to the left and to the right
    public int CalculateStepsToMoveLeft2(int cursorCharPosition)
    {
        if (currentLineManager.Length == 0)
            return 0;

        int stepsToMove = 0;
        for (int i = cursorCharPosition - 1; i >= 0; i--)
        {
            char currentCharacter = currentLineManager.CurrentLine[CheckIndex(currentLineManager.CurrentLine, i)];
            if (char.IsLetterOrDigit(currentCharacter) || currentCharacter == '_')
                stepsToMove++;
            else if (i == cursorCharPosition - 1 && char.IsWhiteSpace(currentCharacter))
                return 0;
            else
                break;
        }
        return stepsToMove;
    }
    public int CalculateStepsToMoveRight2(int cursorCharPosition)
    {
        if (currentLineManager.Length == 0)
            return 0;

        int stepsToMove = 0;
        for (int i = cursorCharPosition; i < currentLineManager.Length; i++)
        {
            char currentCharacter = currentLineManager.CurrentLine[CheckIndex(currentLineManager.CurrentLine, i)];
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
    public int CalculateStepsToMoveLeft(int cursorCharPosition)
    {
        if (!Utils.IsKeyPressed(Windows.System.VirtualKey.Control))
            return 1;

        int stepsToMove = 0;
        for (int i = cursorCharPosition - 1; i >= 0; i--)
        {
            char CurrentCharacter = currentLineManager.CurrentLine[CheckIndex(currentLineManager.CurrentLine, i)];
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
    public int CalculateStepsToMoveRight(int cursorCharPosition)
    {
        if (!Utils.IsKeyPressed(Windows.System.VirtualKey.Control))
            return 1;

        int stepsToMove = 0;
        for (int i = cursorCharPosition; i < currentLineManager.Length; i++)
        {
            char CurrentCharacter = currentLineManager.CurrentLine[CheckIndex(currentLineManager.CurrentLine, i)];
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
    public void MoveLeft(CursorPosition currentCursorPosition)
    {
        if (currentCursorPosition.LineNumber < 0)
            return;

        int currentLineLength = textManager.GetLineLength(currentCursorPosition.LineNumber);
        if (currentCursorPosition.CharacterPosition == 0 && currentCursorPosition.LineNumber > 0)
        {
            currentCursorPosition.CharacterPosition = textManager.GetLineLength(currentCursorPosition.LineNumber - 1);
            currentCursorPosition.LineNumber -= 1;
        }
        else if (currentCursorPosition.CharacterPosition > currentLineLength)
            currentCursorPosition.CharacterPosition = currentLineLength - 1;
        else if (currentCursorPosition.CharacterPosition > 0)
            currentCursorPosition.CharacterPosition -= CalculateStepsToMoveLeft(currentCursorPosition.CharacterPosition);
    }
    public void MoveRight(CursorPosition currentCursorPosition)
    {
        int lineLength = textManager.GetLineLength(currentCursorPosition.LineNumber);

        if (currentCursorPosition.LineNumber > textManager.LinesCount- 1)
            return;

        if (currentCursorPosition.CharacterPosition == lineLength && currentCursorPosition.LineNumber < textManager.LinesCount - 1)
        {
            currentCursorPosition.CharacterPosition = 0;
            currentCursorPosition.LineNumber += 1;
        }
        else if (currentCursorPosition.CharacterPosition < lineLength)
            currentCursorPosition.CharacterPosition += CalculateStepsToMoveRight(currentCursorPosition.CharacterPosition);

        if (currentCursorPosition.CharacterPosition > lineLength)
            currentCursorPosition.CharacterPosition = lineLength;
    }
    public void MoveDown(CursorPosition currentCursorPosition)
    {
        if (currentCursorPosition.LineNumber < textManager.LinesCount - 1)
            currentCursorPosition.LineNumber += 1;
    }
    public void MoveUp(CursorPosition currentCursorPosition)
    {
        if (currentCursorPosition.LineNumber > 0)
            currentCursorPosition.LineNumber -= 1;
    }
    public void MoveToLineEnd(CursorPosition cursorPosition)
    {
        cursorPosition.CharacterPosition = currentLineManager.Length;
    }
    public void MoveToLineStart(CursorPosition cursorPosition)
    {
        cursorPosition.CharacterPosition = 0;
    }
}