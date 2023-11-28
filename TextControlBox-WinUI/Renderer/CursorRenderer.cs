using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using System.Numerics;
using TextControlBox.Text;
using TextControlBox_WinUI.Helper;
using TextControlBox_WinUI.Models;

namespace TextControlBox.Renderer
{
    internal class CursorRenderer
    {
        private void DrawCursor(CanvasTextLayout textLayout, int characterPosition, float xOffset, float y, float fontSize, CursorSize customSize, CanvasDrawEventArgs args, CanvasSolidColorBrush cursorColorBrush)
        {
            if (textLayout == null)
                return;

            Vector2 vector = textLayout.GetCaretPosition(characterPosition < 0 ? 0 : characterPosition, false);
            if (customSize == null)
                args.DrawingSession.FillRectangle(vector.X + xOffset, y, 1, fontSize, cursorColorBrush);
            else
                args.DrawingSession.FillRectangle(vector.X + xOffset + customSize.OffsetX, y + customSize.OffsetY, (float)customSize.Width, (float)customSize.Height, cursorColorBrush);
        }
        public void Render(TextControlBox textbox, CanvasDrawEventArgs args, CursorPosition position, TextRenderer textRenderer, TextControlBoxProperties props, TextManager textManager, CurrentWorkingLine workingLine)
        {
            workingLine.Update(textManager, position);

            int currentLineLength = workingLine.Text.Length;
            if (position.LineNumber >= textManager.Lines.Count)
            {
                position.LineNumber = textManager.Lines.Count - 1;
                position.CharacterPosition = currentLineLength;
            }

            float renderPosY = (float)((position.LineNumber - textRenderer.RenderStartLine) * props.SingleLineHeight) + props.SingleLineHeight / props.DefaultVerticalScrollSensitivity;
            if (renderPosY > textRenderer.RenderLineCount * props.SingleLineHeight || renderPosY < 0)
                return;

            workingLine.UpdateTextLayout(position);

            int characterPos = position.CharacterPosition;
            if (characterPos > currentLineLength)
                characterPos = currentLineLength;

            DrawCursor(workingLine.TextLayout, characterPos, (float)-textbox.HorizontalScroll, renderPosY,
                props.ZoomedFontSize, props._CursorSize, args, props.CursorColorBrush);
        }
    }
}
