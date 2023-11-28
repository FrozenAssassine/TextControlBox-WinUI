using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Collections.Generic;
using TextControlBox.Extensions;
using TextControlBox_WinUI.Helper;
using TextControlBox_WinUI.Models;

namespace TextControlBox.Renderer
{
    internal class TextRenderer
    {
        public CanvasTextFormat textFormat;
        public IEnumerable<string> renderedLines;
        public int RenderStartLine = 0;
        public int RenderLineCount = 0;
        public CanvasTextLayout textLayout;
        private string renderedText;

        public void NeedsTextFormatUpdate()
        {
            textFormat = null;
        }

        public CanvasTextLayout Render(CanvasControl canvas, TextManager textManager, ScrollBar VerticalScrollbar, TextControlBoxProperties textboxProperties)
        {
            if(textFormat == null)
            {
                textFormat = TextLayoutHelper.CreateCanvasTextFormat(textboxProperties);
            }

            textboxProperties.SingleLineHeight = textFormat.LineSpacing;

            RenderLineCount = (int)(canvas.ActualHeight / textboxProperties.SingleLineHeight);
            RenderStartLine = (int)(VerticalScrollbar.Value * textboxProperties.DefaultVerticalScrollSensitivity / textboxProperties.SingleLineHeight);
            
            renderedLines = textManager.GetLines(RenderStartLine, RenderLineCount);

            if (textboxProperties.DoWordWrap)
            {
                int charactersInLine = 40;
                renderedText = WordWrapHelper.WordWrap(renderedLines, charactersInLine).GetString("\n");
            
            }
            else
            {
                renderedText = renderedLines.GetString("\n");
            }

            textLayout = TextLayoutHelper.CreateTextResource(canvas, textLayout, textFormat, renderedText, canvas.Size, textboxProperties.ZoomedFontSize);
            
            return textLayout;
        }

    }
}
