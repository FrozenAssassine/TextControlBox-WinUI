using System;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Helper;
using TextControlBoxNS.Models;

namespace TextControlBoxNS.Core;

internal class CursorManager
{
    public CursorPosition oldCursorPosition = new CursorPosition(0, 0);
    public CursorPosition currentCursorPosition { get; private set; } = new CursorPosition(0, 0);
    public int LineNumber { get => currentCursorPosition.LineNumber; set { currentCursorPosition.LineNumber = value; currentCursorPosition.IsTrailing = false; } }
    public int CharacterPosition { get => currentCursorPosition.CharacterPosition; set { currentCursorPosition.CharacterPosition = value; currentCursorPosition.IsTrailing = false; } }
    public bool IsTrailing { get => currentCursorPosition.IsTrailing; set => currentCursorPosition.IsTrailing = value; }

    private TextManager textManager;
    private CurrentLineManager currentLineManager;


    public void Init(TextManager textManager, CurrentLineManager currentLineManager)
    {
        this.textManager = textManager;
        this.currentLineManager = currentLineManager;
    }

    public int? PreferredCharacterPosition { get; set; } = null;
    public float? PreferredCaretX { get; set; } = null;

    public void ResetPreferredPosition()
    {
        PreferredCharacterPosition = null;
        PreferredCaretX = null;
    }

    public void SetCursorPosition(int line, int character)
    {
        this.LineNumber = line;
        this.CharacterPosition = character;
        this.IsTrailing = false;
        ResetPreferredPosition();
    }
    public void SetCursorPositionCopyValues(CursorPosition cursorPosition)
    {
        this.currentCursorPosition.LineNumber = cursorPosition.LineNumber;
        this.currentCursorPosition.CharacterPosition = cursorPosition.CharacterPosition;
        this.currentCursorPosition.IsTrailing = cursorPosition.IsTrailing;
        ResetPreferredPosition();
    }

    public int GetCurPosInLine()
    {
        int curLineLength = currentLineManager.Length;
        int pos = CharacterPosition + (IsTrailing ? 1 : 0);
        return Math.Clamp(pos, 0, curLineLength);
    }

    private int CheckIndex(string str, int index) => Math.Clamp(index, 0, str.Length - 1);
    public bool Equals(CursorPosition curPos1, CursorPosition curPos2)
    {
        if (curPos1 == null || curPos2 == null)
            return false;

        if (curPos1.LineNumber == curPos2.LineNumber)
            return curPos1.CharacterPosition == curPos2.CharacterPosition && curPos1.IsTrailing == curPos2.IsTrailing;
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

    CharClass GetCharClass(char c) => CharClassHelper.GetCharClass(c);
    private int CountCharactersToMoveLeft(int startPosition)
    {
        var line = currentLineManager.CurrentLine;
        int i = startPosition;
        int moved = 0;

        while (i >= 0 && char.IsWhiteSpace(line[i]))
        {
            i--;
            moved++;
        }

        if (i < 0)
            return moved;

        CharClass targetClass = GetCharClass(line[i]);

        while (i >= 0 && GetCharClass(line[i]) == targetClass)
        {
            i--;
            moved++;
        }

        return moved;
    }
    public int CalculateStepsToMoveLeft(int cursorCharPosition, bool? controlIsPressed = null)
    {
        if (!controlIsPressed.HasValue && !Utils.IsKeyPressed(Windows.System.VirtualKey.Control))
            return 1;

        if (controlIsPressed.HasValue && !controlIsPressed.Value)
            return 1;

        int startPos = cursorCharPosition - 1;
        int stepsToMove = CountCharactersToMoveLeft(startPos);

        return stepsToMove == 0 ? 1 : stepsToMove;
    }

    public int CalculateStepsToMoveRight(int cursorCharPosition, bool? controlIsPressed = null)
    {
        if (!controlIsPressed.HasValue && !Utils.IsKeyPressed(Windows.System.VirtualKey.Control))
            return 1;

        if (controlIsPressed.HasValue && !controlIsPressed.Value)
            return 1;

        var line = currentLineManager.CurrentLine;
        int length = currentLineManager.Length;

        if (cursorCharPosition >= length)
            return 0;

        int i = cursorCharPosition;
        int moved = 0;

        while (i < length && char.IsWhiteSpace(line[i]))
        {
            i++;
            moved++;
        }

        if (i >= length)
            return moved == 0 ? 1 : moved;

        CharClass targetClass = GetCharClass(line[i]);
        while (i < length && GetCharClass(line[i]) == targetClass)
        {
            i++;
            moved++;
        }

        return moved == 0 ? 1 : moved;
    }

    //Move cursor:
    public void MoveLeft()
    {
        ResetPreferredPosition();

        if (LineNumber < 0)
            return;

        if (IsTrailing)
        {
            IsTrailing = false;
            return;
        }

        int currentLineLength = textManager.GetLineLength(LineNumber);
        if (CharacterPosition == 0 && LineNumber > 0)
        {
            CharacterPosition = textManager.GetLineLength(LineNumber - 1);
            LineNumber -= 1;
        }
        else if (CharacterPosition > currentLineLength)
            CharacterPosition = currentLineLength;
        else if (CharacterPosition > 0)
            CharacterPosition -= CalculateStepsToMoveLeft(CharacterPosition);
    }
    public void MoveRight()
    {
        ResetPreferredPosition();

        if (IsTrailing)
        {
            IsTrailing = false;
            CharacterPosition++;
            return;
        }

        int lineLength = textManager.GetLineLength(LineNumber);

        if (LineNumber > textManager.LinesCount - 1)
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
        IsTrailing = false;
        if (LineNumber < textManager.LinesCount - 1)
        {
            if (!PreferredCharacterPosition.HasValue)
                PreferredCharacterPosition = CharacterPosition;

            LineNumber += 1;
            CharacterPosition = Math.Clamp(PreferredCharacterPosition.Value, 0, textManager.GetLineLength(LineNumber));
        }
    }
    public void MoveUp()
    {
        IsTrailing = false;
        if (LineNumber > 0)
        {
            if (!PreferredCharacterPosition.HasValue)
                PreferredCharacterPosition = CharacterPosition;

            LineNumber -= 1;
            CharacterPosition = Math.Clamp(PreferredCharacterPosition.Value, 0, textManager.GetLineLength(LineNumber));
        }
    }
    public void MoveToLineEnd(CursorPosition cursorPosition)
    {
        ResetPreferredPosition();
        cursorPosition.CharacterPosition = currentLineManager.Length;
    }
    public void MoveToLineStart(CursorPosition cursorPosition)
    {
        ResetPreferredPosition();
        cursorPosition.CharacterPosition = 0;
    }

    public void SetToTextEnd()
    {
        if (textManager.LinesCount == 1 && textManager.GetLineLength(0) == 0)
            SetCursorPosition(0, 0);

        int lastLine = Math.Max(0, textManager.LinesCount - 1);
        SetCursorPosition(lastLine, textManager.GetLineLength(lastLine));
    }

    public void SetToTextStart()
    {
        SetCursorPosition(0, 0);
    }
}