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
            if (textRenderer.IsWordWrapEnabled)
            {
                GenerateWrappedLineNumberText(renderedLines, startLine);
                return;
            }

            for (int i = 0; i < renderedLines; i++)
            {
                LineNumberContent.AppendLine((i + 1 + startLine).ToString());
            }
            LineNumberTextToRender = LineNumberContent.ToString();
            LineNumberContent.Clear();
        }

        private void GenerateWrappedLineNumberText(int renderedLines, int startLine)
        {
            if (textRenderer.IsVirtualizedWrappedLine)
            {
                int vLine = textRenderer.VirtualizedLineIndex >= 0 ? textRenderer.VirtualizedLineIndex : startLine;
                if (vLine == startLine)
                {
                    int vRows = Math.Max(1, textRenderer.VirtualizedWrappedRowsToRender);
                    if (textRenderer.VirtualizedLineSliceStart == 0)
                    {
                        LineNumberContent.AppendLine((startLine + 1).ToString());
                        for (int r = 1; r < vRows; r++)
                            LineNumberContent.AppendLine();
                    }
                    else
                    {
                        for (int r = 0; r < vRows; r++)
                            LineNumberContent.AppendLine();
                    }

                    for (int i = startLine + 1; i < startLine + renderedLines && i < textManager.LinesCount; i++)
                    {
                        int rowCount = textRenderer.GetWrappedRowCount(i);
                        LineNumberContent.AppendLine((i + 1).ToString());
                        for (int r = 1; r < rowCount; r++)
                            LineNumberContent.AppendLine();
                    }
                }
                else
                {
                    int totalEmittedRows = 0;
                    for (int i = startLine; i < startLine + renderedLines && i < textManager.LinesCount; i++)
                    {
                        if (i == vLine)
                        {
                            int remainingRows = Math.Max(1, textRenderer.VirtualizedWrappedRowsToRender - totalEmittedRows);
                            LineNumberContent.AppendLine((i + 1).ToString());
                            for (int r = 1; r < remainingRows; r++)
                                LineNumberContent.AppendLine();
                            break;
                        }
                        int rowCount = textRenderer.GetWrappedRowCount(i);
                        LineNumberContent.AppendLine((i + 1).ToString());
                        for (int r = 1; r < rowCount; r++)
                            LineNumberContent.AppendLine();
                        totalEmittedRows += rowCount;
                    }
                }
                LineNumberTextToRender = LineNumberContent.ToString();
                LineNumberContent.Clear();
                return;
            }

            for (int i = startLine; i < startLine + renderedLines && i < textManager.LinesCount; i++)
            {
                int rowCount = textRenderer.GetWrappedRowCount(i);
                LineNumberContent.AppendLine((i + 1).ToString());
                for (int r = 1; r < rowCount; r++)
                {
                    LineNumberContent.AppendLine();
                }
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

            float lineNumberWidth = (float)Utils.MeasureTextSize(args.DrawingSession.Device, (textManager.LinesCount).ToString(), LineNumberTextFormat).Width;
            float targetCanvasWidth = lineNumberWidth + 10 + spaceBetweenCanvasAndText;
            if (Math.Abs(canvas.Width - targetCanvasWidth) > 0.5f)
            {
                canvas.Width = targetCanvasWidth;
            }

            float posX = (float)canvas.Size.Width - spaceBetweenCanvasAndText;
            if (posX < 0) 
                posX = 0;

            OldLineNumberTextToRender = LineNumberTextToRender;

            float drawLineNumberOffsetY = textRenderer.IsWordWrapEnabled
                ? (textRenderer.IsVirtualizedWrappedLine
                    ? textRenderer.SingleLineHeight
                    : textRenderer.SingleLineHeight - (textRenderer.WrappedStartRowOffset * textRenderer.SingleLineHeight))
                : textRenderer.SingleLineHeight;

            int renderedVisualRows = textRenderer.IsWordWrapEnabled
                ? (textRenderer.IsVirtualizedWrappedLine
                    ? textRenderer.VirtualizedWrappedRowsToRender + (textRenderer.NumberOfRenderedLines > 1 ? textRenderer.GetRenderedVisualRowCount(textRenderer.NumberOfStartLine + 1, textRenderer.NumberOfRenderedLines - 1) : 0)
                    : textRenderer.GetRenderedVisualRowCount(textRenderer.NumberOfStartLine, textRenderer.NumberOfRenderedLines))
                : textRenderer.NumberOfRenderedLines;

            float layoutHeight = Math.Max((float)canvas.Size.Height, (renderedVisualRows + 2) * textRenderer.SingleLineHeight);

            LineNumberTextLayout?.Dispose();
            LineNumberTextLayout = textLayoutManager.CreateTextLayout(canvas, LineNumberTextFormat, LineNumberTextToRender, posX, layoutHeight);

            args.DrawingSession.DrawTextLayout(
                LineNumberTextLayout,
                10,
                drawLineNumberOffsetY,
                designHelper.LineNumberColorBrush);
        }

        public void CreateLineNumberTextFormat()
        {
            if (lineNumberManager._ShowLineNumbers)
            {
                LineNumberTextFormat?.Dispose();
                LineNumberTextFormat = textLayoutManager.CreateLinenumberTextFormat();
            }
        }

        public void CheckDispose()
        {
            LineNumberTextLayout?.Dispose();
            LineNumberTextFormat?.Dispose();
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
