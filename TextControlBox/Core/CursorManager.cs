using System;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Helper;

namespace TextControlBoxNS.Core;

internal class CursorManager
{
    public CursorPosition oldCursorPosition = new CursorPosition(0,0);
    public CursorPosition currentCursorPosition { get; private set; } = new CursorPosition(0, 0);
    public int LineNumber { get => currentCursorPosition.LineNumber; set => currentCursorPosition.LineNumber = value; }
    public int CharacterPosition { get => currentCursorPosition.CharacterPosition; set { currentCursorPosition.CharacterPosition = value; } }

    private TextManager textManager;
    private CurrentLineManager currentLineManager;
    private TabSpaceHelper tabSpaceHelper;
    private SelectionManager selectionManager;
    private AutoIndentionManager autoIndentionManager;
    public void Init(TextManager textManager, CurrentLineManager currentLineManager, TabSpaceHelper tabSpaceHelper, SelectionManager selectionManager, AutoIndentionManager autoIndentionManager)
    {
        this.textManager = textManager;
        this.currentLineManager = currentLineManager;
        this.tabSpaceHelper = tabSpaceHelper;
        this.selectionManager = selectionManager;
        this.autoIndentionManager = autoIndentionManager;
    }

    public void SetCursorPosition(int line, int character)
    {
        this.LineNumber = line;
        this.CharacterPosition = character;
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
            cursorIndex += textManager.totalLines.Span[i].Length + 1;
        }
        return cursorIndex;
    }
    public bool Equals(CursorPosition curPos1, CursorPosition curPos2)
    {
        if (curPos1.IsNull || curPos2.IsNull)
            return false;

        if (curPos1.LineNumber == curPos2.LineNumber)
            return curPos1.CharacterPosition == curPos2.CharacterPosition;
        return false;
    }

    //Calculate the number of characters from the cursorposition to the next character or digit to the left and to the right
    public int CalculateStepsToMoveLeftNoControl(int cursorCharPosition)
    {
        if (currentLineManager.Length == 0)
            return 0;

        int stepsToMove = 0;
        for (int i = cursorCharPosition - 1; i >= 0; i--)
        {
            char currentCharacter = currentLineManager.CurrentLine[i];
            if (char.IsLetterOrDigit(currentCharacter) || currentCharacter == '_')
                stepsToMove++;
            else if (i == cursorCharPosition - 1 && char.IsWhiteSpace(currentCharacter))
                return 0;
            else
                break;
        }
        return stepsToMove;
    }
    public int CalculateStepsToMoveRightNoControl(int cursorCharPosition)
    {
        if (currentLineManager.Length == 0)
            return 0;

        int stepsToMove = 0;
        for (int i = cursorCharPosition; i < currentLineManager.Length; i++)
        {
            char currentCharacter = currentLineManager.CurrentLine[i];
            if (char.IsLetterOrDigit(currentCharacter) || currentCharacter == '_')
                stepsToMove++;
            else if (i == cursorCharPosition && char.IsWhiteSpace(currentCharacter))
                return 0;
            else
                break;
        }
        return stepsToMove;
    }

    //Calculates how many characters the cursor needs to move if control is pressed -> skip contiguous tabs and spaces as one move
    //Returns 1 if control is not pressed

    private int IsFilledWithTabsAndSpacesToCursor(string currentLine, int cursor)
    {
        string tabCharacter = tabSpaceHelper.TabCharacter;
        int tabLength = tabCharacter.Length;
        int count = 0;

        for (int i = cursor - 1; i >= 0;)
        {
            if (currentLine[i] == ' ') // Check for spaces
            {
                count++;
                i--; // Move one character back
            }
            else if (i >= tabLength - 1 && currentLine.Substring(i - tabLength + 1, tabLength) == tabCharacter)
            {
                count += tabLength;
                i -= tabLength;
            }
            else
            {
                break;
            }
        }

        return count;
    }
    private int IsFilledWithTabsAndSpacesFromCursor(string currentLine, int cursor)
    {
        string tabCharacter = tabSpaceHelper.TabCharacter;
        int tabLength = tabCharacter.Length;
        int count = 0;

        for (int i = cursor; i < currentLine.Length;)
        {
            if (currentLine[i] == ' ')
            {
                count++;
                i++;
            }
            else if (i + tabLength <= currentLine.Length && currentLine.Substring(i, tabLength) == tabCharacter)
            {
                count += tabLength;
                i += tabLength;
            }
            else
            {
                break;
            }
        }

        return count;
    }

    public int CalculateStepsToMoveLeft(int cursorCharPosition)
    {
        if (!Utils.IsKeyPressed(Windows.System.VirtualKey.Control))
            return 1;

        int filledRes = IsFilledWithTabsAndSpacesToCursor(currentLineManager.CurrentLine, cursorCharPosition);
        if (filledRes != 0)
            return filledRes;

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

        int filledRes = IsFilledWithTabsAndSpacesFromCursor(currentLineManager.CurrentLine, cursorCharPosition);
        if (filledRes != 0)
            return filledRes;

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
    public void MoveLeft()
    {
        if (LineNumber < 0)
            return;

        int currentLineLength = textManager.GetLineLength(LineNumber);
        if (CharacterPosition == 0 && LineNumber > 0)
        {
            CharacterPosition = textManager.GetLineLength(LineNumber - 1);
            LineNumber -= 1;
        }
        else if (CharacterPosition > currentLineLength)
            CharacterPosition = currentLineLength - 1;
        else if (CharacterPosition > 0)
            CharacterPosition -= CalculateStepsToMoveLeft(CharacterPosition);
    }
    public void MoveRight()
    {
        int lineLength = textManager.GetLineLength(LineNumber);

        if (LineNumber > textManager.LinesCount- 1)
            return;

        if (CharacterPosition == lineLength && LineNumber < textManager.LinesCount - 1)
        {
            CharacterPosition = 0;
            LineNumber += 1;
        }
        else if (CharacterPosition < lineLength)
            CharacterPosition += CalculateStepsToMoveRight(CharacterPosition);

        if (CharacterPosition > lineLength)
            CharacterPosition = lineLength;
    }
    public void MoveDown()
    {
        if (LineNumber < textManager.LinesCount - 1)
            LineNumber += 1;
    }
    public void MoveUp()
    {
        if (LineNumber > 0)
            LineNumber -= 1;
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