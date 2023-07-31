using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Xaml.Media;
using Windows.Foundation;

namespace TextControlBoxNS.Helper
{
    internal class TextLayoutHelper
    {
        public static CanvasTextLayout CreateTextResource(ICanvasResourceCreatorWithDpi resourceCreator, CanvasTextLayout textLayout, CanvasTextFormat textFormat, string text, Size targetSize)
        {
            if (textLayout != null)
                textLayout.Dispose();

            textLayout = CreateTextLayout(resourceCreator, textFormat, text, targetSize);
            textLayout.Options = CanvasDrawTextOptions.EnableColorFont;
            return textLayout;
        }
        public static CanvasTextFormat CreateCanvasTextFormat(float zoomedFontSize, FontFamily fontFamily)
        {
            return CreateCanvasTextFormat(zoomedFontSize, zoomedFontSize + 2, fontFamily);
        }

        public static CanvasTextFormat CreateCanvasTextFormat(float zoomedFontSize, float lineSpacing, FontFamily fontFamily)
        {
            CanvasTextFormat textFormat = new CanvasTextFormat()
            {
                FontSize = zoomedFontSize,
                HorizontalAlignment = CanvasHorizontalAlignment.Left,
                VerticalAlignment = CanvasVerticalAlignment.Top,
                WordWrapping = CanvasWordWrapping.NoWrap,
                LineSpacing = lineSpacing,
            };
            textFormat.IncrementalTabStop = zoomedFontSize * 3; //default 137px
            textFormat.FontFamily = fontFamily.Source;
            textFormat.TrimmingGranularity = CanvasTextTrimmingGranularity.None;
            textFormat.TrimmingSign = CanvasTrimmingSign.None;
            return textFormat;
        }
        public static CanvasTextLayout CreateTextLayout(ICanvasResourceCreator resourceCreator, CanvasTextFormat textFormat, string text, Size canvasSize)
        {
            return new CanvasTextLayout(resourceCreator, text, textFormat, (float)canvasSize.Width, (float)canvasSize.Height);
        }
        public static CanvasTextLayout CreateTextLayout(ICanvasResourceCreator resourceCreator, CanvasTextFormat textFormat, string text, float width, float height)
        {
            return new CanvasTextLayout(resourceCreator, text, textFormat, width, height);
        }
        public static CanvasTextFormat CreateLinenumberTextFormat(float fontSize, FontFamily fontFamily)
        {
            CanvasTextFormat textFormat = new CanvasTextFormat()
            {
                FontSize = fontSize,
                HorizontalAlignment = CanvasHorizontalAlignment.Right,
                VerticalAlignment = CanvasVerticalAlignment.Top,
                WordWrapping = CanvasWordWrapping.NoWrap,
                LineSpacing = fontSize + 2,
            };
            textFormat.FontFamily = fontFamily.Source;
            textFormat.TrimmingGranularity = CanvasTextTrimmingGranularity.None;
            textFormat.TrimmingSign = CanvasTrimmingSign.None;
            return textFormat;
        }
    }
}
