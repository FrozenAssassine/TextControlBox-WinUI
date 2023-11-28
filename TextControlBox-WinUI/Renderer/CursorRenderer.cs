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
        private void DrawCursor(CanvasTextLayout TextLayout, int CharacterPosition, float XOffset, float Y, float FontSize, CursorSize CustomSize, CanvasDrawEventArgs args, CanvasSolidColorBrush CursorColorBrush)
        {
            if (TextLayout == null)
                return;

            Vector2 vector = TextLayout.GetCaretPosition(CharacterPosition < 0 ? 0 : CharacterPosition, false);
            if (CustomSize == null)
                args.DrawingSession.FillRectangle(vector.X + XOffset, Y, 1, FontSize, CursorColorBrush);
            else
                args.DrawingSession.FillRectangle(vector.X + XOffset + CustomSize.OffsetX, Y + CustomSize.OffsetY, (float)CustomSize.Width, (float)CustomSize.Height, CursorColorBrush);
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

            workingLine.UpdateTextLayout(textManager, position);

            int characterPos = position.CharacterPosition;
            if (characterPos < currentLineLength)
                characterPos = currentLineLength;

            DrawCursor(workingLine.TextLayout, characterPos, (float)-textbox.HorizontalScroll, renderPosY,
                props.ZoomedFontSize, props._CursorSize, args, props.CursorColorBrush);
        }
    }
}
