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

            if (textRenderer.IsHorizontallyVirtualized)
            {
                // Every visible line is sliced to the same horizontal window; map both endpoints into the
                // multi-line sliced layout via the per-line prefix offsets. This replaces the cumulative
                // full-line-length math below, which would over-count because the rendered prior lines are
                // sliced (shorter) than their document length.
                selStartIndex = textRenderer.GetRenderedLayoutIndexForDocument(startLine, characterPosStart);
                selEndIndex = textRenderer.GetRenderedLayoutIndexForDocument(endLine, characterPosEnd);
                if (selStartIndex < 0 || selEndIndex < 0)
                {
                    selectionManager.currentTextSelection.renderedIndex = 0;
                    selectionManager.currentTextSelection.renderedLength = 0;
                    return;
                }
            }
            else if (startLine == endLine)
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

            if (selectionManager.HasSelection && !selectionManager.Equals(selectionManager.OldTextSelection, selectionManager.currentTextSelection))
            {
                //Update the variables
                selectionManager.OldTextSelection.EndPosition.SetChangeValues(selectionManager.currentTextSelection.EndPosition);
                selectionManager.OldTextSelection.StartPosition.SetChangeValues(selectionManager.currentTextSelection.StartPosition);
                eventsManager.CallSelectionChanged();
            }
        }

        // Top margin for the selection regions. In wrap mode the whole layout is nudged up by the rows of the
        // first visible line scrolled above the viewport, matching the wrapped text draw offset.
        private float GetSelectionTopMargin()
        {
            float topInset = textRenderer.SingleLineHeight / scrollManager.DefaultVerticalScrollSensitivity;
            if (!textRenderer.IsWordWrapEnabled)
                return topInset;
            return topInset - (textRenderer.WrappedStartRowOffset * textRenderer.SingleLineHeight);
        }
    }
}