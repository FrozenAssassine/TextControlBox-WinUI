using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
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
            if (textRenderer.WordWrapEnabled)
            {
                DrawWordWrapSelection(textLayout, args, marginLeft, marginTop, fontSize, selectionColor);
                return;
            }

            int selStartIndex = 0;
            int selEndIndex = 0;
            int characterPosStart = selectionManager.selectionStart.CharacterPosition;
            int characterPosEnd = selectionManager.selectionEnd.CharacterPosition;
            int startLine = selectionManager.selectionStart.LineNumber;
            int endLine = selectionManager.selectionEnd.LineNumber;

            int lineEndingLength = textManager.NewLineCharacter.Length;

            if (endLine > textManager.totalLines.Count)
                endLine = textManager.totalLines.Count - 1;

            if (characterPosStart > textManager.totalLines.Span[startLine].Length)
                characterPosStart = textManager.totalLines.Span[startLine].Length;

            if (characterPosEnd > textManager.totalLines.Span[endLine].Length)
                characterPosEnd = textManager.totalLines.Span[endLine].Length;

            //Render the selection on position 0 if the user scrolled the start away
            if (startLine < unrenderedLinesToRenderStart)
            {
                startLine = unrenderedLinesToRenderStart;
                characterPosStart = 0;
            }

            if (endLine < unrenderedLinesToRenderStart)
            {
                endLine = unrenderedLinesToRenderStart;
                characterPosEnd = 0;
            }

            // If start is beyond visible area, clamp it to the end of the visible region
            int lastRenderedLine = unrenderedLinesToRenderStart + numberOfRenderedLines - 1;
            if (startLine > lastRenderedLine)
            {
                startLine = lastRenderedLine;
                characterPosStart = textManager.totalLines.Span[startLine].Length;
            }

            if (endLine > lastRenderedLine)
            {
                endLine = lastRenderedLine;
                characterPosEnd = textManager.totalLines.Span[endLine].Length;
            }

            if (startLine == endLine)
            {
                int lenghtToLine = 0;
                for (int i = 0; i < startLine - unrenderedLinesToRenderStart; i++)
                {
                    if (i < numberOfRenderedLines)
                    {
                        lenghtToLine += textManager.totalLines.Span[textRenderer.NumberOfStartLine + i].Length + lineEndingLength;
                    }
                }

                selStartIndex = characterPosStart + lenghtToLine;
                selEndIndex = characterPosEnd + lenghtToLine;
            }
            else
            {
                for (int i = 0; i < startLine - unrenderedLinesToRenderStart; i++)
                {
                    if (i >= numberOfRenderedLines) //Out of range of the List (do nothing)
                        break;
                    selStartIndex += textManager.totalLines.Span[textRenderer.NumberOfStartLine + i].Length + lineEndingLength;
                }

                selStartIndex += characterPosStart;

                for (int i = 0; i < endLine - unrenderedLinesToRenderStart; i++)
                {
                    if (i >= numberOfRenderedLines) //Out of range of the List (do nothing)
                        break;

                    selEndIndex += textManager.totalLines.Span[textRenderer.NumberOfStartLine + i].Length + lineEndingLength;
                }

                selEndIndex += characterPosEnd;
            }

            renderedSelectionStart = Math.Max(0, Math.Min(selStartIndex, selEndIndex));

            renderedSelectionLength = selEndIndex > selStartIndex ?
                selEndIndex - selStartIndex :
                selStartIndex - selEndIndex;

            //no selection can be rendered. 
            //GetCharacterRegions(0,0) still returns a "ghost" region, so stop rendering here
            if(renderedSelectionLength == 0)
            {
                selectionManager.currentTextSelection.renderedIndex = 0;
                selectionManager.currentTextSelection.renderedLength = 0;
                return;
            }

            using CanvasCommandList canvasCommandList = new CanvasCommandList(args.DrawingSession);
            using (var ccls = canvasCommandList.CreateDrawingSession())
            {
                CanvasTextLayoutRegion[] regions = textLayout.GetCharacterRegions(renderedSelectionStart, renderedSelectionLength);
                float width = fontSize / scrollManager.DefaultVerticalScrollSensitivity;
                for (int i = 0; i < regions.Length; i++)
                {
                    //Change the width if selection in an empty line or starts at a line end
                    if (regions[i].LayoutBounds.Width == 0)
                    {
                        var bounds = regions[i].LayoutBounds;
                        regions[i].LayoutBounds = new Rect
                        {
                            Width = width,
                            Height = bounds.Height,
                            X = bounds.X,
                            Y = bounds.Y
                        };
                    }

                    ccls.FillRectangle(Utils.CreateRect(regions[i].LayoutBounds, marginLeft, marginTop), selectionColor);
                }
            }
            args.DrawingSession.DrawImage(canvasCommandList);

            selectionManager.currentTextSelection.renderedIndex = renderedSelectionStart;
            selectionManager.currentTextSelection.renderedLength = renderedSelectionLength;
        }

        private void DrawWordWrapSelection(
            CanvasTextLayout textLayout,
            CanvasDrawEventArgs args,
            float marginLeft,
            float marginTop,
            float fontSize,
            Color selectionColor)
        {
            if (textLayout == null)
                return;

            int startLine = selectionManager.selectionStart.LineNumber;
            int endLine = selectionManager.selectionEnd.LineNumber;
            int startChar = selectionManager.selectionStart.CharacterPosition;
            int endChar = selectionManager.selectionEnd.CharacterPosition;

            if (startLine < 0 || endLine < 0)
                return;

            startLine = Math.Clamp(startLine, 0, textManager.LinesCount - 1);
            endLine = Math.Clamp(endLine, 0, textManager.LinesCount - 1);

            int startLineLength = textManager.GetLineLength(startLine);
            int endLineLength = textManager.GetLineLength(endLine);

            startChar = Math.Clamp(startChar, 0, startLineLength);
            endChar = Math.Clamp(endChar, 0, endLineLength);

            int selStartIndex = textManager.GetGlobalIndex(startLine, startChar);
            int selEndIndex = textManager.GetGlobalIndex(endLine, endChar, includeLineBreak: endChar >= endLineLength);

            renderedSelectionStart = Math.Min(selStartIndex, selEndIndex);
            renderedSelectionLength = Math.Abs(selEndIndex - selStartIndex);

            if (renderedSelectionLength == 0)
            {
                selectionManager.currentTextSelection.renderedIndex = 0;
                selectionManager.currentTextSelection.renderedLength = 0;
                return;
            }

            int selectionStart = renderedSelectionStart;
            int selectionEnd = renderedSelectionStart + renderedSelectionLength;
            var visualLineMap = textRenderer.VisualLineMap;
            if (visualLineMap.TotalVisualLines == 0)
                return;

            int visualStart = Math.Clamp(textRenderer.NumberOfStartLine, 0, visualLineMap.TotalVisualLines - 1);
            int visualEnd = Math.Clamp(textRenderer.NumberOfStartLine + textRenderer.NumberOfRenderedLines - 1, 0, visualLineMap.TotalVisualLines - 1);

            using CanvasCommandList canvasCommandList = new CanvasCommandList(args.DrawingSession);
            using (var ccls = canvasCommandList.CreateDrawingSession())
            {
                float width = fontSize / scrollManager.DefaultVerticalScrollSensitivity;
                for (int visualIndex = visualStart; visualIndex <= visualEnd; visualIndex++)
                {
                    var lineInfo = visualLineMap.VisualLines[visualIndex];
                    int logicalLineStart = textManager.GetGlobalIndex(lineInfo.LogicalLineIndex, 0);
                    int segmentLength = lineInfo.Length == 0 ? Math.Max(1, lineInfo.LayoutLength) : lineInfo.Length;
                    int lineStart = logicalLineStart + lineInfo.StartChar;
                    int lineEnd = lineStart + segmentLength;

                    int rangeStart = Math.Max(selectionStart, lineStart);
                    int rangeEnd = Math.Min(selectionEnd, lineEnd);
                    if (rangeEnd <= rangeStart)
                        continue;

                    CanvasTextLayoutRegion[] regions = textLayout.GetCharacterRegions(rangeStart, rangeEnd - rangeStart);
                    for (int i = 0; i < regions.Length; i++)
                    {
                        if (regions[i].LayoutBounds.Width == 0)
                        {
                            var bounds = regions[i].LayoutBounds;
                            regions[i].LayoutBounds = new Rect
                            {
                                Width = width,
                                Height = bounds.Height,
                                X = bounds.X,
                                Y = bounds.Y
                            };
                        }

                        ccls.FillRectangle(Utils.CreateRect(regions[i].LayoutBounds, marginLeft, marginTop), selectionColor);
                    }
                }
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
                    textRenderer.HorizontalOffset,
                    (textRenderer.WordWrapEnabled ? -textRenderer.VerticalScrollPixels : 0 )+ textRenderer.SingleLineHeight / scrollManager.DefaultVerticalScrollSensitivity,
                    textRenderer.NumberOfStartLine,
                    textRenderer.NumberOfRenderedLines,
                    zoomManager.ZoomedFontSize,
                    designHelper._Design.SelectionColor
                );
            }

            if (selectionManager.HasSelection && !selectionManager.Equals(selectionManager.OldTextSelection, selectionManager.currentTextSelection))
            {
                //Update the variables
                selectionManager.OldTextSelection.EndPosition.SetChangeValues(selectionManager.currentTextSelection.EndPosition);
                selectionManager.OldTextSelection.StartPosition.SetChangeValues(selectionManager.currentTextSelection.StartPosition);
                eventsManager.CallSelectionChanged();
            }
        }
    }
}