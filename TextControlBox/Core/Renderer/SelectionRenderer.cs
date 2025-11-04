using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
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
            if (endLine < startLine)
            {
                if (endLine < unrenderedLinesToRenderStart)
                    characterPosEnd = 0;
                if (startLine < unrenderedLinesToRenderStart + 1)
                    characterPosStart = 0;
            }
            else if (endLine == startLine)
            {
                if (startLine < unrenderedLinesToRenderStart)
                    characterPosStart = 0;
                if (endLine < unrenderedLinesToRenderStart)
                    characterPosEnd = 0;
            }
            else
            {
                if (startLine < unrenderedLinesToRenderStart)
                    characterPosStart = 0;
                if (endLine < unrenderedLinesToRenderStart + 1)
                    characterPosEnd = 0;
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

            CanvasCommandList canvasCommandList = new CanvasCommandList(args.DrawingSession);
            using (var ccls = canvasCommandList.CreateDrawingSession())
            {
                CanvasTextLayoutRegion[] regions = textLayout.GetCharacterRegions(renderedSelectionStart, renderedSelectionLength);
                for (int i = 0; i < regions.Length; i++)
                {
                    //Change the width if selection in an empty line or starts at a line end
                    if (regions[i].LayoutBounds.Width == 0)
                    {
                        var bounds = regions[i].LayoutBounds;
                        regions[i].LayoutBounds = new Rect
                        {
                            Width = fontSize / 4,
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
                    (float)-scrollManager.HorizontalScroll,
                    textRenderer.SingleLineHeight / scrollManager.DefaultVerticalScrollSensitivity,
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