namespace TextControlBox.Text
{
    public class SelectionChangedEventHandler
    {
        /// <summary>
        /// The cursorposition in the current line
        /// </summary>
        public int CharacterPositionInLine;
        /// <summary>
        /// The linenumber where the cursor is currently in
        /// </summary>
        public int LineNumber;
        /// <summary>
        /// The start index of the selection
        /// </summary>
        public int SelectionStartIndex;
        /// <summary>
        /// The lenght of the selection
        /// </summary>
        public int SelectionLength;
    }
}
