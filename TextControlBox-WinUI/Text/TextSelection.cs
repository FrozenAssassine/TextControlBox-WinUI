namespace TextControlBox.Text
{
    internal class TextSelection
    {
        public TextSelection()
        {
            Index = 0;
            Length = 0;
            StartPosition = null;
            EndPosition = null;
        }
        public TextSelection(int index = 0, int length = 0, CursorPosition startPosition = null, CursorPosition endPosition = null)
        {
            Index = index;
            Length = length;
            StartPosition = startPosition;
            EndPosition = endPosition;
        }
        public TextSelection(CursorPosition startPosition = null, CursorPosition endPosition = null)
        {
            StartPosition = startPosition;
            EndPosition = endPosition;
        }
        public TextSelection(TextSelection textSelection)
        {
            StartPosition = new CursorPosition(textSelection.StartPosition);
            EndPosition = new CursorPosition(textSelection.EndPosition);
            Index = textSelection.Index;
            Length = textSelection.Length;
        }

        public int Index { get; set; }
        public int Length { get; set; }

        public CursorPosition StartPosition { get; set; }
        public CursorPosition EndPosition { get; set; }

        public new string ToString()
        {
            return StartPosition.LineNumber + ":" + StartPosition.CharacterPosition + " | " + EndPosition.LineNumber + ":" + EndPosition.CharacterPosition;
        }

        public bool IsLineInSelection(int index)
        {
            if (this.StartPosition != null && this.EndPosition != null)
            {
                if (this.StartPosition.LineNumber > this.EndPosition.LineNumber)
                {
                    return this.StartPosition.LineNumber < index && this.EndPosition.LineNumber > index;
                }
                else if (this.StartPosition.LineNumber == this.EndPosition.LineNumber)
                    return this.StartPosition.LineNumber != index;
                else
                    return this.StartPosition.LineNumber > index && this.EndPosition.LineNumber < index;
            }
            return false;
        }
    }

    public class TextSelectionPosition
    {
        public TextSelectionPosition(int Index = 0, int Length = 0)
        {
            this.Index = Index;
            this.Length = Length;
        }
        public int Index { get; set; }
        public int Length { get; set; }
    }
}
