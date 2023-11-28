using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using TextControlBox.Extensions;
using TextControlBox.Renderer;
using TextControlBox.Text;

namespace TextControlBox_WinUI.Helper
{
    internal class CurrentWorkingLine
    {
        private int workingLineIndex = 0;
        private string _Text = "";
        public string Text { get => _Text; set { _Text = value; textManager.Lines.SetLineText(workingLineIndex, value); } }
        public CanvasTextLayout TextLayout;
        private CanvasControl canvas_text;
        private TextRenderer textRenderer;
        private TextManager textManager;
        public CurrentWorkingLine(CanvasControl canvas_text, TextRenderer textRenderer, TextManager textManager)
        {
            this.canvas_text = canvas_text;
            this.textRenderer = textRenderer;
            this.textManager = textManager;
        }

        public void UpdateTextLayout(CursorPosition position)
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
            workingLineIndex = position.LineNumber;
        }
    }
}
