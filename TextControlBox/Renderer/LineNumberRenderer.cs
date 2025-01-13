using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextControlBoxNS.Helper;

namespace TextControlBoxNS.Renderer
{
    internal class LineNumberRenderer
    {
        public string LineNumberTextToRender;
        public string OldLineNumberTextToRender;

        private StringBuilder LineNumberContent = new StringBuilder();
        private bool needsUpdate = false;

        public void GenerateLineNumberText(int renderedLines, int startLine)
        {
            //todo check performance:
            for (int i = 0; i < renderedLines; i++)
            {
                LineNumberContent.AppendLine((i + 1 + startLine).ToString());
            }
            LineNumberTextToRender = LineNumberContent.ToString();
            LineNumberContent.Clear();
        }

        public bool CanUpdateCanvas()
        {
            return OldLineNumberTextToRender == null ||
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
            float lineNumberWidth = (float)Utils.MeasureTextSize(CanvasDevice.GetSharedDevice(), (TotalLines.Count).ToString(), LineNumberTextFormat).Width;
            canvas.Width = lineNumberWidth + 10 + spaceBetweenCanvasAndText;

            float posX = (float)canvas.Size.Width - spaceBetweenCanvasAndText;
            if (posX < 0)
                posX = 0;

            OldLineNumberTextToRender = LineNumberTextToRender;
            LineNumberTextLayout = TextLayoutHelper.CreateTextLayout(sender, LineNumberTextFormat, LineNumberTextToRender, posX, (float)sender.Size.Height);
            args.DrawingSession.DrawTextLayout(LineNumberTextLayout, 10, SingleLineHeight, LineNumberColorBrush);
        }

    }
}
