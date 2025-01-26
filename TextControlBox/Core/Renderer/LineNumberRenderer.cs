using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Text;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Helper;

namespace TextControlBoxNS.Core.Renderer
{
    internal class LineNumberRenderer
    {
        public CanvasTextLayout LineNumberTextLayout = null;
        public CanvasTextFormat LineNumberTextFormat = null;

        public string LineNumberTextToRender;
        public string OldLineNumberTextToRender;

        private readonly StringBuilder LineNumberContent = new StringBuilder();
        private bool needsUpdate = false;

        private TextManager textManager;
        private TextRenderer textRenderer;
        private DesignHelper designHelper;
        private LineNumberManager lineNumberManager;
        private TextLayoutManager textLayoutManager;
        public void Init(TextManager textManager, TextLayoutManager textLayoutManager, TextRenderer textRenderer, DesignHelper designHelper, LineNumberManager lineNumberManager)
        {
            this.textManager = textManager;
            this.textRenderer = textRenderer;
            this.designHelper = designHelper;
            this.lineNumberManager = lineNumberManager;
            this.textLayoutManager = textLayoutManager;
        }

        public void GenerateLineNumberText(int renderedLines, int startLine)
        {
            //TODO! check performance:
            for (int i = 0; i < renderedLines; i++)
            {
                LineNumberContent.AppendLine((i + 1 + startLine).ToString());
            }
            LineNumberTextToRender = LineNumberContent.ToString();
            LineNumberContent.Clear();
        }

        public bool CanUpdateCanvas()
        {
            return needsUpdate || OldLineNumberTextToRender == null ||
                LineNumberTextToRender == null ||
                !OldLineNumberTextToRender.Equals(LineNumberTextToRender, StringComparison.OrdinalIgnoreCase);
        }

        public void NeedsUpdateLineNumbers()
        {
            this.needsUpdate = true;
        }

        public void HideLineNumbers(CanvasControl canvas, float spaceBetweenCanvasAndText)
        {
            canvas.Width = spaceBetweenCanvasAndText;
        }

        public void Draw(CanvasControl canvas, CanvasDrawEventArgs args, float spaceBetweenCanvasAndText)
        {
            if (LineNumberTextToRender == null || LineNumberTextToRender.Length == 0)
                return;

            //Calculate the linenumbers             
            float lineNumberWidth = (float)Utils.MeasureTextSize(CanvasDevice.GetSharedDevice(), (textManager.LinesCount).ToString(), LineNumberTextFormat).Width;
            canvas.Width = lineNumberWidth + 10 + spaceBetweenCanvasAndText;

            float posX = (float)canvas.Size.Width - spaceBetweenCanvasAndText;
            if (posX < 0)
                posX = 0;

            OldLineNumberTextToRender = LineNumberTextToRender;
            LineNumberTextLayout = textLayoutManager.CreateTextLayout(canvas, LineNumberTextFormat, LineNumberTextToRender, posX, (float)canvas.Size.Height);
            args.DrawingSession.DrawTextLayout(LineNumberTextLayout, 10, textRenderer.SingleLineHeight, designHelper.LineNumberColorBrush);
        }

        public void CreateLineNumberTextFormat()
        {
            if (lineNumberManager._ShowLineNumbers)
                LineNumberTextFormat = textLayoutManager.CreateLinenumberTextFormat();
        }

        public void CheckGenerateLineNumberText()
        {
            if (lineNumberManager._ShowLineNumbers)
            {
                GenerateLineNumberText(textRenderer.NumberOfRenderedLines, textRenderer.NumberOfStartLine);
            }
        }
    }
}
