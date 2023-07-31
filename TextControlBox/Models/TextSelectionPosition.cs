namespace TextControlBoxNS
{
    /// <summary>
    /// Represents the position and length of a text selection in the textbox.
    /// </summary>
    public class TextSelectionPosition
    {
        /// <summary>
        /// Initializes a new instance of the TextSelectionPosition class with the specified index and length.
        /// </summary>
        /// <param name="index">The start index of the text selection.</param>
        /// <param name="length">The length of the text selection.</param>
        public TextSelectionPosition(int index = 0, int length = 0)
        {
            this.Index = index;
            this.Length = length;
        }

        /// <summary>
        /// Gets or sets the start index of the text selection.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Gets or sets the length of the text selection.
        /// </summary>
        public int Length { get; set; }
    }
}
