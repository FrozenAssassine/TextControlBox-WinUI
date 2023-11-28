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
        public Rect CreateRect(Rect r, float MarginLeft = 0, float MarginTop = 0)
        {
            return new Rect(
                new Point(
                    Math.Floor(r.Left + MarginLeft),//X
                    Math.Floor(r.Top + MarginTop)), //Y
                new Point(
                    Math.Ceiling(r.Right + MarginLeft), //Width
                    Math.Ceiling(r.Bottom + MarginTop))); //Height
        }

        //Draw the actual selection and return the cursorposition. Return -1 if no selection was drawn
        public TextSelection DrawSelection(DesignHelper designHelper, SelectionManager selectionManager, CanvasDrawEventArgs args, float MarginLeft, float MarginTop)
        {
            TextSelection selection = selectionManager.Selection;

            if (selectionManager.HasSelection && selection.EndPosition != null && selection.StartPosition != null)
            {
                int SelStartIndex = 0;
                int SelEndIndex = 0;
                int CharacterPosStart = selection.StartPosition.CharacterPosition;
                int CharacterPosEnd = selection.EndPosition.CharacterPosition;

                //Render the selection on position 0 if the user scrolled the start away
                if (selection.EndPosition.LineNumber < selection.StartPosition.LineNumber)
                {
                    if (selection.EndPosition.LineNumber < textRenderer.RenderStartLine)
                        CharacterPosEnd = 0;
                    if (selection.StartPosition.LineNumber < textRenderer.RenderStartLine + 1)
                        CharacterPosStart = 0;
                }
                else if (selection.EndPosition.LineNumber == selection.StartPosition.LineNumber)
                {
                    if (selection.StartPosition.LineNumber < textRenderer.RenderStartLine)
                        CharacterPosStart = 0;
                    if (selection.EndPosition.LineNumber < textRenderer.RenderStartLine)
                        CharacterPosEnd = 0;
                }
                else
                {
                    if (selection.StartPosition.LineNumber < textRenderer.RenderStartLine)
                        CharacterPosStart = 0;
                    if (selection.EndPosition.LineNumber < textRenderer.RenderStartLine + 1)
                        CharacterPosEnd = 0;
                }

                if (selection.StartPosition.LineNumber == selection.EndPosition.LineNumber)
                {
                    int LenghtToLine = 0;
                    for (int i = 0; i < selection.StartPosition.LineNumber - textRenderer.RenderStartLine; i++)
                    {
                        if (i < textRenderer.RenderLineCount)
                        {
                            LenghtToLine += textRenderer.renderedLines.ElementAt(i).Length + 1;
                        }
                    }

                    SelStartIndex = CharacterPosStart + LenghtToLine;
                    SelEndIndex = CharacterPosEnd + LenghtToLine;

                }
                else
                {
                    for (int i = 0; i < selection.StartPosition.LineNumber - textRenderer.RenderStartLine; i++)
                    {
                        if (i >= textRenderer.RenderLineCount) //Out of range of the List (do nothing)
                            break;
                        SelStartIndex += textRenderer.renderedLines.ElementAt(i).Length + 1;
                    }
                    SelStartIndex += CharacterPosStart;

                    for (int i = 0; i < selection.EndPosition.LineNumber - textRenderer.RenderStartLine; i++)
                    {
                        if (i >= textRenderer.RenderLineCount) //Out of range of the List (do nothing)
                            break;

                        SelEndIndex += textRenderer.renderedLines.ElementAt(i).Length + 1;
                    }
                    SelEndIndex += CharacterPosEnd;
                }

                SelectionStart = Math.Min(SelStartIndex, SelEndIndex);

                if (SelectionStart < 0)
                    SelectionStart = 0;
                if (SelectionLength < 0)
                    SelectionLength = 0;

                if (SelEndIndex > SelStartIndex)
                    SelectionLength = SelEndIndex - SelStartIndex;
                else
                    SelectionLength = SelStartIndex - SelEndIndex;

                CanvasTextLayoutRegion[] descriptions = textRenderer.textLayout.GetCharacterRegions(SelectionStart, SelectionLength);
                for (int i = 0; i < descriptions.Length; i++)
                {
                    //Change the width if selection in an emty line or starts at a line end
                    if (descriptions[i].LayoutBounds.Width == 0 && descriptions.Length > 1)
                    {
                        var bounds = descriptions[i].LayoutBounds;
                        descriptions[i].LayoutBounds = new Rect { Width = textBoxProps.ZoomedFontSize / 4, Height = bounds.Height, X = bounds.X, Y = bounds.Y };
                    }

                    args.DrawingSession.FillRectangle(CreateRect(descriptions[i].LayoutBounds, MarginLeft, MarginTop), designHelper.CurrentDesign.SelectionColor);
                }
                return new TextSelection(SelectionStart, SelectionLength, new CursorPosition(selection.StartPosition), new CursorPosition(selection.EndPosition));
            }
            return null;
        }

        //returns whether the pointer is over a selection
        public bool PointerIsOverSelection(Point PointerPosition, TextSelection Selection, CanvasTextLayout TextLayout)
        {
            if (TextLayout == null || Selection == null)
                return false;

            CanvasTextLayoutRegion[] regions = TextLayout.GetCharacterRegions(Selection.Index, Selection.Length);
            for (int i = 0; i < regions.Length; i++)
            {
                if (regions[i].LayoutBounds.Contains(PointerPosition))
                    return true;
            }
            return false;
        }

        public bool CursorIsInSelection(CursorPosition CursorPosition, TextSelection TextSelection)
        {
            if (TextSelection == null)
                return false;
            TextSelection = Selection.OrderTextSelection(TextSelection);

            //Cursorposition is smaller than the start of selection
            if (TextSelection.StartPosition.LineNumber > CursorPosition.LineNumber)
            {
                return false;
            }
            else
            {
                //Selectionend is smaller than Cursorposition -> not in selection
                if (TextSelection.EndPosition.LineNumber < CursorPosition.LineNumber)
                {
                    return false;
                }
                else
                {
                    //Selection-start line equals Cursor line:
                    if (CursorPosition.LineNumber == TextSelection.StartPosition.LineNumber)
                    {
                        return CursorPosition.CharacterPosition > TextSelection.StartPosition.CharacterPosition;
                    }
                    //Selection-end line equals Cursor line
                    else if (CursorPosition.LineNumber == TextSelection.EndPosition.LineNumber)
                    {
                        return CursorPosition.CharacterPosition < TextSelection.EndPosition.CharacterPosition;
                    }
                    return true;
                }
            }
        }
    }
}