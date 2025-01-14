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
        public CursorPosition SelectionStartPosition = null;
        public CursorPosition SelectionEndPosition = null;
        public int SelectionLength = 0;
        public int SelectionStart = 0;

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
            if (HasSelection && SelectionEndPosition != null && SelectionStartPosition != null)
            {
                int selStartIndex = 0;
                int selEndIndex = 0;
                int characterPosStart = SelectionStartPosition.CharacterPosition;
                int characterPosEnd = SelectionEndPosition.CharacterPosition;

                //Render the selection on position 0 if the user scrolled the start away
                if (SelectionEndPosition.LineNumber < SelectionStartPosition.LineNumber)
                {
                    if (SelectionEndPosition.LineNumber < unrenderedLinesToRenderStart)
                        characterPosEnd = 0;
                    if (SelectionStartPosition.LineNumber < unrenderedLinesToRenderStart + 1)
                        characterPosStart = 0;
                }
                else if (SelectionEndPosition.LineNumber == SelectionStartPosition.LineNumber)
                {
                    if (SelectionStartPosition.LineNumber < unrenderedLinesToRenderStart)
                        characterPosStart = 0;
                    if (SelectionEndPosition.LineNumber < unrenderedLinesToRenderStart)
                        characterPosEnd = 0;
                }
                else
                {
                    if (SelectionStartPosition.LineNumber < unrenderedLinesToRenderStart)
                        characterPosStart = 0;
                    if (SelectionEndPosition.LineNumber < unrenderedLinesToRenderStart + 1)
                        characterPosEnd = 0;
                }

                if (SelectionStartPosition.LineNumber == SelectionEndPosition.LineNumber)
                {
                    int lenghtToLine = 0;
                    for (int i = 0; i < SelectionStartPosition.LineNumber - unrenderedLinesToRenderStart; i++)
                    {
                        if (i < numberOfRenderedLines)
                        {
                            lenghtToLine += renderedLines.ElementAt(i).Length + 1;
                        }
                    }

                    selStartIndex = characterPosStart + lenghtToLine;
                    selEndIndex = characterPosEnd + lenghtToLine;
                }
                else
                {
                    for (int i = 0; i < SelectionStartPosition.LineNumber - unrenderedLinesToRenderStart; i++)
                    {
                        if (i >= numberOfRenderedLines) //Out of range of the List (do nothing)
                            break;
                        selStartIndex += renderedLines.ElementAt(i).Length + 1;
                    }
                    selStartIndex += characterPosStart;

                    for (int i = 0; i < SelectionEndPosition.LineNumber - unrenderedLinesToRenderStart; i++)
                    {
                        if (i >= numberOfRenderedLines) //Out of range of the List (do nothing)
                            break;

                        selEndIndex += renderedLines.ElementAt(i).Length + 1;
                    }
                    selEndIndex += characterPosEnd;
                }

                SelectionStart = Math.Min(selStartIndex, selEndIndex);

                if (SelectionStart < 0)
                    SelectionStart = 0;
                if (SelectionLength < 0)
                    SelectionLength = 0;

                if (selEndIndex > selStartIndex)
                    SelectionLength = selEndIndex - selStartIndex;
                else
                    SelectionLength = selStartIndex - selEndIndex;

                CanvasTextLayoutRegion[] descriptions = textLayout.GetCharacterRegions(SelectionStart, SelectionLength);
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
                return new TextSelection(SelectionStart, SelectionLength, new CursorPosition(SelectionStartPosition), new CursorPosition(SelectionEndPosition));
            }
            return null;
        }

        //Clear the selection
        public void ClearSelection()
        {
            HasSelection = false;
            IsSelecting = false;
            SelectionEndPosition = null;
            SelectionStartPosition = null;
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
            SelectionStartPosition = startPosition;
            SelectionEndPosition = endPosition;
            IsSelecting = false;
            HasSelection = true;
        }
        public void SetSelectionStart(CursorPosition startPosition)
        {
            IsSelecting = true;
            SelectionStartPosition = startPosition;
            IsSelecting = false;
            HasSelection = true;
        }
        public void SetSelectionEnd(CursorPosition endPosition)
        {
            IsSelecting = true;
            SelectionEndPosition = endPosition;
            IsSelecting = false;
            HasSelection = true;
        }

        public void Draw(CanvasControl canvasSelection, CanvasDrawEventArgs args)
        {
            if (SelectionStartPosition != null && SelectionEndPosition != null)
            {
                HasSelection =
                    !(SelectionStartPosition.LineNumber == SelectionEndPosition.LineNumber &&
                    SelectionStartPosition.CharacterPosition == SelectionEndPosition.CharacterPosition);
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
                selectionManager.OldTextSelection = new TextSelection(selectionManager.currentTextSelection);
                eventsManager.CallSelectionChanged();
            }
        }
    }
}