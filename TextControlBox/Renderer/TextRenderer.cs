using Microsoft.Graphics.Canvas.Text;
using System.Collections.Generic;


namespace TextControlBoxNS.Renderer;

internal class TextRenderer
{
    public CanvasTextFormat TextFormat = null;
    public CanvasTextLayout DrawnTextLayout = null;

    public bool NeedsUpdateTextLayout = true;
    public bool NeedsTextFormatUpdate = true;
    public float SingleLineHeight { get => TextFormat == null ? 0 : TextFormat.LineSpacing; }

    public int NumberOfStartLine = 0;
    public int LongestLineLength = 0;
    public int LongestLineIndex = 0;
    public int NumberOfRenderedLines = 0;
    public IEnumerable<string> RenderedLines;


    public string RenderedText = "";
    public string OldRenderedText = null;

    //Check whether the current line is outside the bounds of the visible area
    public bool OutOfRenderedArea(int line)
    {
        return line < NumberOfStartLine || line >= NumberOfStartLine + NumberOfRenderedLines;
    }
}
