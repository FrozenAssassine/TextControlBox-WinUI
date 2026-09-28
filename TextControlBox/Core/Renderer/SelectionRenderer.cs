using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TextControlBoxNS.Core.Selection;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Helper;
using Windows.Foundation;
using Windows.UI;

namespace TextControlBoxNS.Core.Renderer
{
    internal class SelectionRenderer
    {
        public int renderedSelectionLength = 0;
        public int renderedSelectionStart = 0;

        private SelectionManager selectionManager;
        private TextRenderer textRenderer;
        private EventsManager eventsManager;
        private ScrollManager scrollManager;
        private ZoomManager zoomManager;
        private DesignHelper designHelper;
        private TextManager textManager;
        public void Init(
            SelectionManager selectionManager,
            TextRenderer textRenderer,
            EventsManager eventsManager,
            ScrollManager scrollManager,
            ZoomManager zoomManager,
            DesignHelper designHelper,
            TextManager textManager
            )
        {
            this.selectionManager = selectionManager;
            this.textRenderer = textRenderer;
            this.eventsManager = eventsManager;
            this.scrollManager = scrollManager;
            this.zoomManager = zoomManager;
            this.designHelper = designHelper;
            this.textManager = textManager;
        }

        public void DrawSelection(
            CanvasTextLayout textLayout,
            CanvasDrawEventArgs args,
            float marginLeft,
            float marginTop,
            int unrenderedLinesToRenderStart,
            int numberOfRenderedLines,
            float fontSize,
            Color selectionColor
            )
        {
            if (textLayout == null || numberOfRenderedLines <= 0 || textRenderer.RenderedText == null)
            {
                selectionManager.currentTextSelection.renderedIndex = 0;
                selectionManager.currentTextSelection.renderedLength = 0;
                return;
            }

            if (textRenderer.IsVirtualizedWrappedLine)
            {
                DrawVirtualizedWrappedLineSelection(textLayout, args, marginLeft, marginTop, fontSize, selectionColor);
                return;
            }

            var (startNull, endNull, startLine, characterPosStart, endLine, characterPosEnd) = selectionManager.OrderTextSelectionSeparated();
            if (startNull || endNull)
            {
                selectionManager.currentTextSelection.renderedIndex = 0;
                selectionManager.currentTextSelection.renderedLength = 0;
                return;
            }

            int linesCount = textManager.totalLines.Count;
            if (linesCount == 0)
            {
                selectionManager.currentTextSelection.renderedIndex = 0;
                selectionManager.currentTextSelection.renderedLength = 0;
                return;
            }

            startLine = Math.Clamp(startLine, 0, linesCount - 1);
            endLine = Math.Clamp(endLine, 0, linesCount - 1);
            characterPosStart = Math.Clamp(characterPosStart, 0, textManager.totalLines.Span[startLine].Length);
            characterPosEnd = Math.Clamp(characterPosEnd, 0, textManager.totalLines.Span[endLine].Length);

            int lastRenderedLine = unrenderedLinesToRenderStart + numberOfRenderedLines - 1;

            // Selection completely outside visible vertical range
            if (endLine < unrenderedLinesToRenderStart || startLine > lastRenderedLine)
            {
                selectionManager.currentTextSelection.renderedIndex = 0;
                selectionManager.currentTextSelection.renderedLength = 0;
                return;
            }

            // Detect actual line ending length matching the currently rendered layout
            int lineEndingLength = textManager.NewLineCharacter.Length;
            if (!string.IsNullOrEmpty(textRenderer.RenderedText))
            {
                if (textRenderer.RenderedText.Contains("\r\n"))
                    lineEndingLength = 2;
                else if (textRenderer.RenderedText.Contains("\n") || textRenderer.RenderedText.Contains("\r"))
                    lineEndingLength = 1;
            }

            int clampedStartLine = startLine;
            int clampedStartChar = characterPosStart;
            if (clampedStartLine < unrenderedLinesToRenderStart)
            {
                clampedStartLine = unrenderedLinesToRenderStart;
                clampedStartChar = 0;
            }

            int clampedEndLine = endLine;
            int clampedEndChar = characterPosEnd;
            if (clampedEndLine > lastRenderedLine)
            {
                clampedEndLine = lastRenderedLine;
                clampedEndChar = textManager.totalLines.Span[clampedEndLine].Length;
            }

            int selStartIndex = 0;
            int selEndIndex = 0;

            if (textRenderer.IsHorizontallyVirtualized)
            {
                int startLineLen = textManager.totalLines.Span[clampedStartLine].Length;
                int endLineLen = textManager.totalLines.Span[clampedEndLine].Length;
                selStartIndex = textRenderer.GetRenderedLayoutIndexForDocument(clampedStartLine, Math.Min(clampedStartChar, startLineLen));
                selEndIndex = textRenderer.GetRenderedLayoutIndexForDocument(clampedEndLine, Math.Min(clampedEndChar, endLineLen));

                if (selStartIndex < 0 || selEndIndex < 0)
                {
                    selectionManager.currentTextSelection.renderedIndex = 0;
                    selectionManager.currentTextSelection.renderedLength = 0;
                    return;
                }
            }
            else if (clampedStartLine == clampedEndLine)
            {
                int lengthToLine = 0;
                for (int i = 0; i < clampedStartLine - unrenderedLinesToRenderStart; i++)
                {
                    if (i < numberOfRenderedLines)
                    {
                        lengthToLine += textManager.totalLines.Span[textRenderer.NumberOfStartLine + i].Length + lineEndingLength;
                    }
                }

                int currentLineLen = textManager.totalLines.Span[clampedStartLine].Length;
                selStartIndex = Math.Min(clampedStartChar, currentLineLen) + lengthToLine;
                selEndIndex = Math.Min(clampedEndChar, currentLineLen) + lengthToLine;
            }
            else
            {
                for (int i = 0; i < clampedStartLine - unrenderedLinesToRenderStart; i++)
                {
                    if (i >= numberOfRenderedLines)
                        break;
                    selStartIndex += textManager.totalLines.Span[textRenderer.NumberOfStartLine + i].Length + lineEndingLength;
                }

                int startLineLen = textManager.totalLines.Span[clampedStartLine].Length;
                selStartIndex += Math.Min(clampedStartChar, startLineLen);

                for (int i = 0; i < clampedEndLine - unrenderedLinesToRenderStart; i++)
                {
                    if (i >= numberOfRenderedLines)
                        break;
                    selEndIndex += textManager.totalLines.Span[textRenderer.NumberOfStartLine + i].Length + lineEndingLength;
                }

                int endLineLen = textManager.totalLines.Span[clampedEndLine].Length;
                selEndIndex += Math.Min(clampedEndChar, endLineLen);
            }

            int textLength = textRenderer.RenderedText?.Length ?? 0;
            renderedSelectionStart = Math.Clamp(Math.Min(selStartIndex, selEndIndex), 0, textLength);
            int maxEnd = Math.Clamp(Math.Max(selStartIndex, selEndIndex), 0, textLength);
            renderedSelectionLength = maxEnd - renderedSelectionStart;

            if (renderedSelectionLength <= 0)
            {
                selectionManager.currentTextSelection.renderedIndex = 0;
                selectionManager.currentTextSelection.renderedLength = 0;
                return;
            }

            using CanvasCommandList canvasCommandList = new CanvasCommandList(args.DrawingSession);
            using (var ccls = canvasCommandList.CreateDrawingSession())
            {
                CanvasTextLayoutRegion[] regions = textLayout.GetCharacterRegions(renderedSelectionStart, renderedSelectionLength);
                float width = fontSize / (scrollManager == null ? 4 : Math.Max(1, scrollManager.DefaultVerticalScrollSensitivity));
                RenderSelectionRegions(ccls, regions, width, marginLeft, marginTop, selectionColor);
            }
            args.DrawingSession.DrawImage(canvasCommandList);

            selectionManager.currentTextSelection.renderedIndex = renderedSelectionStart;
            selectionManager.currentTextSelection.renderedLength = renderedSelectionLength;
        }


        public void Draw(CanvasControl canvasSelection, CanvasDrawEventArgs args)
        {
            if (!selectionManager.selectionStart.IsNull && !selectionManager.selectionEnd.IsNull)
                selectionManager.HasSelection = SelectionHelper.TextIsSelected(selectionManager.selectionStart, selectionManager.selectionEnd);
            else
                selectionManager.HasSelection = false;

            if (selectionManager.HasSelection)
            {
                DrawSelection(
                    textRenderer.DrawnTextLayout,
                    args,
                    textRenderer.IsWordWrapEnabled ? 0 : textRenderer.HorizontalOffset,
                    GetSelectionTopMargin(),
                    textRenderer.NumberOfStartLine,
                    textRenderer.NumberOfRenderedLines,
                    zoomManager.ZoomedFontSize,
                    designHelper._Design.SelectionColor
                );
            }
            else
            {
                renderedSelectionLength = 0;
                renderedSelectionStart = 0;
                selectionManager.currentTextSelection.renderedIndex = 0;
                selectionManager.currentTextSelection.renderedLength = 0;
            }

            if (selectionManager.HasSelection)
            {
                if (!selectionManager.Equals(selectionManager.OldTextSelection, selectionManager.currentTextSelection))
                {
                    //Update the variables
                    selectionManager.OldTextSelection.EndPosition.SetChangeValues(selectionManager.currentTextSelection.EndPosition);
                    selectionManager.OldTextSelection.StartPosition.SetChangeValues(selectionManager.currentTextSelection.StartPosition);
                    eventsManager.CallSelectionChanged();
                }
            }
            else
            {
                if (!selectionManager.OldTextSelection.StartPosition.IsNull || !selectionManager.OldTextSelection.EndPosition.IsNull)
                {
                    selectionManager.OldTextSelection.StartPosition.IsNull = true;
                    selectionManager.OldTextSelection.EndPosition.IsNull = true;
                    eventsManager.CallSelectionChanged();
                }
            }
        }

        private void DrawVirtualizedWrappedLineSelection(
            CanvasTextLayout textLayout,
            CanvasDrawEventArgs args,
            float marginLeft,
            float marginTop,
            float fontSize,
            Color selectionColor)
        {
            if (textLayout == null || textRenderer.VirtualizedLineCharsPerRow <= 0)
            {
                selectionManager.currentTextSelection.renderedIndex = 0;
                selectionManager.currentTextSelection.renderedLength = 0;
                return;
            }

            int startLine = selectionManager.selectionStart.LineNumber;
            int startChar = selectionManager.selectionStart.CharacterPosition;
            int endLine = selectionManager.selectionEnd.LineNumber;
            int endChar = selectionManager.selectionEnd.CharacterPosition;

            if (startLine > endLine || (startLine == endLine && startChar > endChar))
            {
                (startLine, endLine) = (endLine, startLine);
                (startChar, endChar) = (endChar, startChar);
            }

            int startLineLen = textManager.totalLines.Span[startLine].Length;
            int endLineLen = textManager.totalLines.Span[endLine].Length;
            int clampedStartChar = Math.Min(startChar, startLineLen);
            int clampedEndChar = Math.Min(endChar, endLineLen);

            int selStartIndex = textRenderer.GetRenderedLayoutIndexForVirtualizedWrappedLine(startLine, clampedStartChar);
            int selEndIndex = textRenderer.GetRenderedLayoutIndexForVirtualizedWrappedLine(endLine, clampedEndChar);

            if (selStartIndex < 0 || selEndIndex < 0)
            {
                selectionManager.currentTextSelection.renderedIndex = 0;
                selectionManager.currentTextSelection.renderedLength = 0;
                return;
            }

            int renderedTextLen = textRenderer.RenderedText?.Length ?? 0;
            selStartIndex = Math.Clamp(selStartIndex, 0, renderedTextLen);
            selEndIndex = Math.Clamp(selEndIndex, 0, renderedTextLen);

            int length = selEndIndex - selStartIndex;
            if (length <= 0)
            {
                selectionManager.currentTextSelection.renderedIndex = 0;
                selectionManager.currentTextSelection.renderedLength = 0;
                return;
            }

            renderedSelectionStart = selStartIndex;
            renderedSelectionLength = length;

            using CanvasCommandList canvasCommandList = new CanvasCommandList(args.DrawingSession);
            using (var ccls = canvasCommandList.CreateDrawingSession())
            {
                CanvasTextLayoutRegion[] regions = textLayout.GetCharacterRegions(renderedSelectionStart, renderedSelectionLength);
                float width = fontSize / (scrollManager == null ? 4 : Math.Max(1, scrollManager.DefaultVerticalScrollSensitivity));
                RenderSelectionRegions(ccls, regions, width, marginLeft, marginTop, selectionColor);
            }
            args.DrawingSession.DrawImage(canvasCommandList);

            selectionManager.currentTextSelection.renderedIndex = renderedSelectionStart;
            selectionManager.currentTextSelection.renderedLength = renderedSelectionLength;
        }

        private static void RenderSelectionRegions(
            CanvasDrawingSession ccls,
            CanvasTextLayoutRegion[] regions,
            float defaultEmptyLineWidth,
            float marginLeft,
            float marginTop,
            Color selectionColor)
        {
            if (regions == null || regions.Length == 0)
                return;

            // Group regions by visual row (Y coordinate) with 1px floating-point tolerance
            var rows = new List<(float y, float height, List<(float startX, float endX)> spans)>();

            for (int i = 0; i < regions.Length; i++)
            {
                var bounds = regions[i].LayoutBounds;
                float y = (float)bounds.Y;
                float h = (float)bounds.Height;

                // Viewport culling to prevent Direct2D texture overflow
                if (y + marginTop > 2500 || y + h + marginTop < -500)
                    continue;

                float x = (float)bounds.X;
                float w = (float)bounds.Width;

                int rowIdx = -1;
                for (int r = 0; r < rows.Count; r++)
                {
                    if (Math.Abs(rows[r].y - y) < 1.0f)
                    {
                        rowIdx = r;
                        break;
                    }
                }

                if (rowIdx == -1)
                {
                    rows.Add((y, h, new List<(float, float)> { (x, x + w) }));
                }
                else
                {
                    rows[rowIdx].spans.Add((x, x + w));
                }
            }

            for (int r = 0; r < rows.Count; r++)
            {
                var (y, h, spans) = rows[r];
                var mergedSpans = SelectionHelper.MergeSelectionIntervals(spans, defaultEmptyLineWidth);

                for (int s = 0; s < mergedSpans.Count; s++)
                {
                    var (startX, endX) = mergedSpans[s];
                    Rect rect = new Rect(startX, y, endX - startX, h);
                    ccls.FillRectangle(Utils.CreateRect(rect, marginLeft, marginTop), selectionColor);
                }
            }
        }

        // Top margin for the selection regions. In wrap mode the whole layout is nudged up by the rows of the
        // first visible line scrolled above the viewport, matching the wrapped text draw offset.
        private float GetSelectionTopMargin() => textRenderer.GetSelectionTopMargin();
    }
}