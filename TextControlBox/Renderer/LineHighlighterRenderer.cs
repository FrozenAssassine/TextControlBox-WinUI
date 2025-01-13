using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.UI.Xaml;
using TextControlBoxNS.Text;

namespace TextControlBoxNS.Renderer
{
    internal class LineHighlighterRenderer
    {
        private LineHighlighterManager lineHighlighterManager;
        private SelectionManager selectionManager;
        private SelectionRenderer selectionRenderer;
        private TextRenderer textRenderer;
        public void Init(LineHighlighterManager lineHighlighterManager, SelectionManager selectionManager, SelectionRenderer selectionRenderer, TextRenderer textRenderer)
        {
            this.selectionManager = selectionManager;
            this.selectionRenderer = selectionRenderer;
            this.lineHighlighterManager = lineHighlighterManager;
            this.textRenderer = textRenderer;
        }

        public void Render(float canvasWidth, float y, float fontSize, CanvasDrawEventArgs args, CanvasSolidColorBrush backgroundBrush)
        {
            if (textRenderer.CurrentLineTextLayout == null)
                return;

            args.DrawingSession.FillRectangle(0, y, canvasWidth, fontSize, backgroundBrush);
        }

        public bool CanRender()
        {
            return lineHighlighterManager._ShowLineHighlighter && selectionManager.SelectionIsNull(selectionRenderer, selectionManager.currentTextSelection);
        }
    }
}
