using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Helper;
using TextControlBoxNS.Models;
using Windows.Foundation;
using Windows.UI;

namespace TextControlBoxNS.Core.Renderer
{
    internal class SelectionRenderer
    {
        public bool HasSelection = false;
        public bool IsSelecting = false;
        public bool IsSelectingOverLinenumbers = false;
        public CursorPosition renderedSelectionStartPosition = new CursorPosition();
        public CursorPosition renderedSelectionEndPosition = new CursorPosition();
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


        //Draw the actual selection and return the cursorposition. Return -1 if no selection was drawn
        public TextSelection DrawSelection(
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
            if (HasSelection && !renderedSelectionEndPosition.IsNull && !renderedSelectionStartPosition.IsNull)
            {

                int selStartIndex = 0;
                int selEndIndex = 0;
                int characterPosStart = renderedSelectionStartPosition.CharacterPosition;
                int characterPosEnd = renderedSelectionEndPosition.CharacterPosition;

                //Render the selection on position 0 if the user scrolled the start away
                if (renderedSelectionEndPosition.LineNumber < renderedSelectionStartPosition.LineNumber)
                {
                    if (renderedSelectionEndPosition.LineNumber < unrenderedLinesToRenderStart)
                        characterPosEnd = 0;
                    if (renderedSelectionStartPosition.LineNumber < unrenderedLinesToRenderStart + 1)
                        characterPosStart = 0;
                }
                else if (renderedSelectionEndPosition.LineNumber == renderedSelectionStartPosition.LineNumber)
                {
                    if (renderedSelectionStartPosition.LineNumber < unrenderedLinesToRenderStart)
                        characterPosStart = 0;
                    if (renderedSelectionEndPosition.LineNumber < unrenderedLinesToRenderStart)
                        characterPosEnd = 0;
                }
                else
                {
                    if (renderedSelectionStartPosition.LineNumber < unrenderedLinesToRenderStart)
                        characterPosStart = 0;
                    if (renderedSelectionEndPosition.LineNumber < unrenderedLinesToRenderStart + 1)
                        characterPosEnd = 0;
                }

                if (renderedSelectionStartPosition.LineNumber == renderedSelectionEndPosition.LineNumber)
                {
                    int lenghtToLine = 0;
                    for (int i = 0; i < renderedSelectionStartPosition.LineNumber - unrenderedLinesToRenderStart; i++)
                    {
                        if (i < numberOfRenderedLines)
                        {
                            lenghtToLine += textManager.GetLineLength(textRenderer.NumberOfStartLine + i) + 2;
                        }
                    }

                    selStartIndex = characterPosStart + lenghtToLine;
                    selEndIndex = characterPosEnd + lenghtToLine;
                }
                else
                {
                    for (int i = 0; i < renderedSelectionStartPosition.LineNumber - unrenderedLinesToRenderStart; i++)
                    {
                        if (i >= numberOfRenderedLines) //Out of range of the List (do nothing)
                            break;
                        selStartIndex += textManager.GetLineLength(textRenderer.NumberOfStartLine + i) + 2;
                    }

                    selStartIndex += characterPosStart;

                    for (int i = 0; i < renderedSelectionEndPosition.LineNumber - unrenderedLinesToRenderStart; i++)
                    {
                        if (i >= numberOfRenderedLines) //Out of range of the List (do nothing)
                            break;

                        selEndIndex += textManager.GetLineLength(textRenderer.NumberOfStartLine + i) + 2;
                    }

                    selEndIndex += characterPosEnd;
                }

                renderedSelectionStart = Math.Min(selStartIndex, selEndIndex);

                if (renderedSelectionStart < 0)
                    renderedSelectionStart = 0;
                if (renderedSelectionLength < 0)
                    renderedSelectionLength = 0;

                if (selEndIndex > selStartIndex)
                    renderedSelectionLength = selEndIndex - selStartIndex;
                else
                    renderedSelectionLength = selStartIndex - selEndIndex;

                CanvasTextLayoutRegion[] descriptions = textLayout.GetCharacterRegions(renderedSelectionStart, renderedSelectionLength);
                for (int i = 0; i < descriptions.Length; i++)
                {
                    //Change the width if selection in an emty line or starts at a line end
                    if (descriptions[i].LayoutBounds.Width == 0 && descriptions.Length > 1)
                    {
                        var bounds = descriptions[i].LayoutBounds;
                        descriptions[i].LayoutBounds = new Rect { Width = fontSize / 4, Height = bounds.Height, X = bounds.X, Y = bounds.Y };
                    }

                    args.DrawingSession.FillRectangle(Utils.CreateRect(descriptions[i].LayoutBounds, marginLeft, marginTop), selectionColor);
                }
                return new TextSelection(renderedSelectionStart, renderedSelectionLength, new CursorPosition(renderedSelectionStartPosition), new CursorPosition(renderedSelectionEndPosition));
            }
            return null;
        }

        //Clear the selection
        public void ClearSelection()
        {
            HasSelection = false;
            IsSelecting = false;
            renderedSelectionEndPosition.IsNull = true;
            renderedSelectionStartPosition.IsNull = true;
            eventsManager.CallSelectionChanged();
        }

        public void SetSelection(TextSelection selection)
        {
            if (selection == null)
                return;

            SetSelection(selection.StartPosition, selection.EndPosition);
        }
        public void SetSelection(CursorPosition startPosition, CursorPosition endPosition)
        {
            IsSelecting = true;
            renderedSelectionStartPosition.SetChangeValues(startPosition);
            renderedSelectionEndPosition.SetChangeValues(endPosition);

            renderedSelectionEndPosition.IsNull = false;
            renderedSelectionStartPosition.IsNull = false;

            IsSelecting = false;
            HasSelection = true;
        }
        public void SetSelectionStart(CursorPosition startPosition)
        {
            IsSelecting = true;
            renderedSelectionStartPosition.SetChangeValues(startPosition);
            renderedSelectionStartPosition.IsNull = false;

            IsSelecting = false;
            HasSelection = true;
        }
        public void SetSelectionEnd(CursorPosition endPosition)
        {
            IsSelecting = true;
            renderedSelectionEndPosition.SetChangeValues(endPosition);
            renderedSelectionEndPosition.IsNull = false;

            IsSelecting = false;
            HasSelection = true;
        }

        public void SetSelectionStart(int startPos, int characterPos)
        {
            renderedSelectionStartPosition.CharacterPosition = characterPos;
            renderedSelectionStartPosition.LineNumber = startPos;
            renderedSelectionStartPosition.IsNull = false;
        }

        public void SetSelectionEnd(int endPos, int characterPos)
        {
            renderedSelectionEndPosition.CharacterPosition = characterPos;
            renderedSelectionEndPosition.LineNumber = endPos;
            renderedSelectionEndPosition.IsNull = false;
        }


        public void Draw(CanvasControl canvasSelection, CanvasDrawEventArgs args)
        {
            if (!renderedSelectionStartPosition.IsNull && !renderedSelectionEndPosition.IsNull)
            {
                HasSelection =
                    !(renderedSelectionStartPosition.LineNumber == renderedSelectionEndPosition.LineNumber &&
                    renderedSelectionStartPosition.CharacterPosition == renderedSelectionEndPosition.CharacterPosition);
            }
            else
                HasSelection = false;

            if (HasSelection)
            {
                var selection = DrawSelection(
                    textRenderer.DrawnTextLayout, 
                    args, 
                    (float)-scrollManager.HorizontalScroll, 
                    textRenderer.SingleLineHeight / scrollManager.DefaultVerticalScrollSensitivity,
                    textRenderer.NumberOfStartLine,
                    textRenderer.NumberOfRenderedLines,
                    zoomManager.ZoomedFontSize,
                    designHelper._Design.SelectionColor
                    );

                selectionManager.SetCurrentTextSelection(selection);
            }

            if (!selectionManager.TextSelIsNull && !selectionManager.Equals(selectionManager.OldTextSelection, selectionManager.currentTextSelection))
            {
                //Update the variables
                selectionManager.OldTextSelection.EndPosition.SetChangeValues(selectionManager.currentTextSelection.EndPosition);
                selectionManager.OldTextSelection.StartPosition.SetChangeValues(selectionManager.currentTextSelection.StartPosition);
                eventsManager.CallSelectionChanged();
            }
        }
    }
}