using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;
using TextControlBox_WinUI.Models;

namespace TextControlBox_WinUI.Helper
{
    internal class TextLayoutHelper
    {
        public static CanvasTextLayout CreateTextResource(ICanvasResourceCreatorWithDpi ResourceCreator, CanvasTextLayout TextLayout, CanvasTextFormat TextFormat, string Text, Size targetSize, float ZoomedFontSize)
        {
            if (TextLayout != null)
                TextLayout.Dispose();

            TextLayout = CreateTextLayout(ResourceCreator, TextFormat, Text, targetSize);
            TextLayout.Options = CanvasDrawTextOptions.EnableColorFont;
            return TextLayout;
        }
        public static CanvasTextFormat CreateCanvasTextFormat(TextControlBoxProperties textboxProperties)
        {
            CanvasTextFormat textFormat;
            textFormat = new CanvasTextFormat()
            {
                FontSize = textboxProperties.ZoomedFontSize,
                HorizontalAlignment = CanvasHorizontalAlignment.Left,
                VerticalAlignment = CanvasVerticalAlignment.Top,
                WordWrapping = CanvasWordWrapping.NoWrap,
                LineSpacing = textboxProperties.ZoomedFontSize + 2,
            };
            textFormat.IncrementalTabStop = textboxProperties.ZoomedFontSize * 3; //default 137px
            textFormat.FontFamily = textboxProperties.FontFamily.Source;
            textFormat.TrimmingGranularity = CanvasTextTrimmingGranularity.None;
            textFormat.TrimmingSign = CanvasTrimmingSign.None;
            return textFormat;
        }
        public static CanvasTextLayout CreateTextLayout(ICanvasResourceCreator ResourceCreator, CanvasTextFormat TextFormat, string Text, Size CanvasSize)
        {
            return new CanvasTextLayout(ResourceCreator, Text, TextFormat, (float)CanvasSize.Width, (float)CanvasSize.Height);
        }
        public static CanvasTextLayout CreateTextLayout(ICanvasResourceCreator ResourceCreator, CanvasTextFormat TextFormat, string Text, float Width, float Height)
        {
            return new CanvasTextLayout(ResourceCreator, Text, TextFormat, Width, Height);
        }
        public static CanvasTextFormat CreateLinenumberTextFormat(float FontSize, FontFamily FontFamily)
        {
            CanvasTextFormat textFormat;

            textFormat = new CanvasTextFormat()
            {
                FontSize = FontSize,
                HorizontalAlignment = CanvasHorizontalAlignment.Right,
                VerticalAlignment = CanvasVerticalAlignment.Top,
                WordWrapping = CanvasWordWrapping.NoWrap,
                LineSpacing = FontSize + 2,
            };
            textFormat.FontFamily = FontFamily.Source;
            textFormat.TrimmingGranularity = CanvasTextTrimmingGranularity.None;
            textFormat.TrimmingSign = CanvasTrimmingSign.None;
            return textFormat;
        }
    }
}
