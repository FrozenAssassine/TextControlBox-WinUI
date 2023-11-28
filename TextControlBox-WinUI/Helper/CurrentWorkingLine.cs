using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using TextControlBox.Extensions;
using TextControlBox.Renderer;
using TextControlBox.Text;

namespace TextControlBox_WinUI.Helper
{
    internal class CurrentWorkingLine
    {
        public string Text;
        public CanvasTextLayout TextLayout;
        private CanvasControl canvas_text;
        private TextRenderer textRenderer;
        public CurrentWorkingLine(CanvasControl canvas_text, TextRenderer textRenderer)
        {
            this.canvas_text = canvas_text;
            this.textRenderer = textRenderer;
        }

        public void UpdateTextLayout(TextManager textManager, CursorPosition position)
        {
            if (textRenderer.textFormat == null)
                return;

            TextLayout = position.LineNumber < textManager.Lines.Count ?
                TextLayoutHelper.CreateTextLayout(
                    canvas_text,
                    textRenderer.textFormat,
                    textManager.Lines.GetLineText(position.LineNumber) + "|",
                    canvas_text.Size) :
                null;
        }

        public void Update(TextManager textManager, CursorPosition position)
        {
            Text = textManager.Lines[position.LineNumber];
        }
    }
}
