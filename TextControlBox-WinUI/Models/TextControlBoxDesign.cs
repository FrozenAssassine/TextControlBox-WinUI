using Microsoft.UI.Xaml.Media;
using Windows.UI;

namespace TextControlBox_WinUI.Models
{
    public class TextControlBoxDesign
    {
        public TextControlBoxDesign(TextControlBoxDesign Design)
        {
            this.Background = Design.Background;
            this.TextColor = Design.TextColor;
            this.SelectionColor = Design.SelectionColor;
            this.CursorColor = Design.CursorColor;
            this.LineHighlighterColor = Design.LineHighlighterColor;
            this.LineNumberColor = Design.LineNumberColor;
            this.LineNumberBackground = Design.LineNumberBackground;
        }

        /// <summary>
        /// Create a new instance of the TextControlBoxDesign class
        /// </summary>
        /// <param name="Background">The background color of the textbox</param>
        /// <param name="TextColor">The color of the text</param>
        /// <param name="SelectionColor">The color of the selection</param>
        /// <param name="CursorColor">The color of the cursor</param>
        /// <param name="LineHighlighterColor">The color of the linehighlighter</param>
        /// <param name="LineNumberColor">The color of the linenumber</param>
        /// <param name="LineNumberBackground">The background color of the linenumbers</param>
        public TextControlBoxDesign(Brush Background, Color TextColor, Color SelectionColor, Color CursorColor, Color LineHighlighterColor, Color LineNumberColor, Color LineNumberBackground, Color SearchHighlightColor)
        {
            this.Background = Background;
            this.TextColor = TextColor;
            this.SelectionColor = SelectionColor;
            this.CursorColor = CursorColor;
            this.LineHighlighterColor = LineHighlighterColor;
            this.LineNumberColor = LineNumberColor;
            this.LineNumberBackground = LineNumberBackground;
            this.SearchHighlightColor = SearchHighlightColor;
        }

        public Brush Background { get; set; }
        public Color TextColor { get; set; }
        public Color SelectionColor { get; set; }
        public Color CursorColor { get; set; }
        public Color LineHighlighterColor { get; set; }
        public Color LineNumberColor { get; set; }
        public Color LineNumberBackground { get; set; }
        public Color SearchHighlightColor { get; set; }
    }
}
