using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Linq;
using TextControlBox.Text;
using TextControlBox_WinUI.Helper;
using TextControlBox_WinUI.Models;
using Windows.Foundation;

namespace TextControlBox.Renderer
{
    internal class SelectionRenderer
    {
        public int SelectionLength = 0;
        public int SelectionStart = 0;
        private readonly TextRenderer textRenderer;
        private readonly TextControlBoxProperties textBoxProps;
        private TextSelection OldSelection = null;

        public SelectionRenderer(TextRenderer textRenderer, TextControlBoxProperties textBoxProps)
        {
            this.textRenderer = textRenderer;
            this.textBoxProps = textBoxProps;
        }

        public TextSelection Render(TextControlBox textBox, DesignHelper designHelper, SelectionManager selectionManager, CanvasDrawEventArgs args)
        {
            bool canDrawSelection;
            var selection = selectionManager.Selection;
            if (selection.StartPosition != null && selection.EndPosition != null)
            {
                canDrawSelection =
                    !(selection.StartPosition.LineNumber == selection.EndPosition.LineNumber &&
                    selection.StartPosition.CharacterPosition == selection.EndPosition.CharacterPosition);
            }
            else
                canDrawSelection = false;

            if (canDrawSelection)
            {
                selection = DrawSelection(designHelper, selectionManager, args, (float)-textBox.HorizontalScroll, textBoxProps.SingleLineHeight / textBoxProps.DefaultVerticalScrollSensitivity);
            }

            if (selection != null && !Selection.Equals(OldSelection, selection))
            {
                OldSelection = new TextSelection(selection);
                //Internal_CursorChanged();
            }
            return selection;
        }

        //Create the rect, to render
        public Rect CreateRect(Rect rect, float marginLeft = 0, float marginTop = 0)
        {
            return new Rect(
                new Point(
                    Math.Floor(rect.Left + marginLeft),//X
                    Math.Floor(rect.Top + marginTop)), //Y
                new Point(
                    Math.Ceiling(rect.Right + marginLeft), //Width
                    Math.Ceiling(rect.Bottom + marginTop))); //Height
        }

        //Draw the actual selection and return the cursorposition. Return -1 if no selection was drawn
        public TextSelection DrawSelection(DesignHelper designHelper, SelectionManager selectionManager, CanvasDrawEventArgs args, float marginLeft, float marginTop)
        {
            TextSelection selection = selectionManager.Selection;

            if (selectionManager.HasSelection && selection.EndPosition != null && selection.StartPosition != null)
            {
                int selStartIndex = 0;
                int selEndIndex = 0;
                int characterPosStart = selection.StartPosition.CharacterPosition;
                int characterPosEnd = selection.EndPosition.CharacterPosition;

                //Render the selection on position 0 if the user scrolled the start away
                if (selection.EndPosition.LineNumber < selection.StartPosition.LineNumber)
                {
                    if (selection.EndPosition.LineNumber < textRenderer.RenderStartLine)
                        characterPosEnd = 0;
                    if (selection.StartPosition.LineNumber < textRenderer.RenderStartLine + 1)
                        characterPosStart = 0;
                }
                else if (selection.EndPosition.LineNumber == selection.StartPosition.LineNumber)
                {
                    if (selection.StartPosition.LineNumber < textRenderer.RenderStartLine)
                        characterPosStart = 0;
                    if (selection.EndPosition.LineNumber < textRenderer.RenderStartLine)
                        characterPosEnd = 0;
                }
                else
                {
                    if (selection.StartPosition.LineNumber < textRenderer.RenderStartLine)
                        characterPosStart = 0;
                    if (selection.EndPosition.LineNumber < textRenderer.RenderStartLine + 1)
                        characterPosEnd = 0;
                }

                if (selection.StartPosition.LineNumber == selection.EndPosition.LineNumber)
                {
                    int lenghtToLine = 0;
                    for (int i = 0; i < selection.StartPosition.LineNumber - textRenderer.RenderStartLine; i++)
                    {
                        if (i < textRenderer.RenderLineCount)
                        {
                            lenghtToLine += textRenderer.renderedLines.ElementAt(i).Length + 1;
                        }
                    }

                    selStartIndex = characterPosStart + lenghtToLine;
                    selEndIndex = characterPosEnd + lenghtToLine;
                }
                else
                {
                    for (int i = 0; i < selection.StartPosition.LineNumber - textRenderer.RenderStartLine; i++)
                    {
                        if (i >= textRenderer.RenderLineCount) //Out of range of the List (do nothing)
                            break;
                        selStartIndex += textRenderer.renderedLines.ElementAt(i).Length + 1;
                    }
                    selStartIndex += characterPosStart;

                    for (int i = 0; i < selection.EndPosition.LineNumber - textRenderer.RenderStartLine; i++)
                    {
                        if (i >= textRenderer.RenderLineCount) //Out of range of the List (do nothing)
                            break;

                        selEndIndex += textRenderer.renderedLines.ElementAt(i).Length + 1;
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

                CanvasTextLayoutRegion[] descriptions = textRenderer.textLayout.GetCharacterRegions(SelectionStart, SelectionLength);
                for (int i = 0; i < descriptions.Length; i++)
                {
                    //Change the width if selection in an emty line or starts at a line end
                    if (descriptions[i].LayoutBounds.Width == 0 && descriptions.Length > 1)
                    {
                        var bounds = descriptions[i].LayoutBounds;
                        descriptions[i].LayoutBounds = new Rect { Width = textBoxProps.ZoomedFontSize / 4, Height = bounds.Height, X = bounds.X, Y = bounds.Y };
                    }

                    args.DrawingSession.FillRectangle(CreateRect(descriptions[i].LayoutBounds, marginLeft, marginTop), designHelper.CurrentDesign.SelectionColor);
                }
                return new TextSelection(SelectionStart, SelectionLength, new CursorPosition(selection.StartPosition), new CursorPosition(selection.EndPosition));
            }
            return null;
        }

        //returns whether the pointer is over a selection
        public bool PointerIsOverSelection(Point pointerPos, TextSelection selection, CanvasTextLayout textLayout)
        {
            if (textLayout == null || selection.HasSelection())
                return false;

            CanvasTextLayoutRegion[] regions = textLayout.GetCharacterRegions(selection.Index, selection.Length);
            for (int i = 0; i < regions.Length; i++)
            {
                if (regions[i].LayoutBounds.Contains(pointerPos))
                    return true;
            }
            return false;
        }

        public bool CursorIsInSelection(CursorPosition cursorPos, TextSelection selection)
        {
            if (selection.HasSelection())
                return false;
            selection = Selection.OrderTextSelection(selection);

            //Cursorposition is smaller than the start of selection
            if (selection.StartPosition.LineNumber > cursorPos.LineNumber)
                return false;
            //Selectionend is smaller than Cursorposition -> not in selection
            if (selection.EndPosition.LineNumber < cursorPos.LineNumber)
                return false;

            //Selection-start line equals Cursor line:
            if (cursorPos.LineNumber == selection.StartPosition.LineNumber)
                return cursorPos.CharacterPosition > selection.StartPosition.CharacterPosition;
                
            //Selection-end line equals Cursor line
            if (cursorPos.LineNumber == selection.EndPosition.LineNumber)
                return cursorPos.CharacterPosition < selection.EndPosition.CharacterPosition;
            return true;
        }
    }
}