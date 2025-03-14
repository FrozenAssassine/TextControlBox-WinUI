using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Xaml.Media;
using TextControlBoxNS.Core.Text;
using Windows.Foundation;

namespace TextControlBoxNS.Core;

internal class TextLayoutManager
{
    private TextManager textManager;
    private ZoomManager zoomManager;
    public void Init(TextManager textManager, ZoomManager zoomManager)
    {
        this.textManager = textManager;
        this.zoomManager = zoomManager;
    }

    public CanvasTextLayout CreateTextResource(ICanvasResourceCreatorWithDpi resourceCreator, CanvasTextLayout textLayout, CanvasTextFormat textFormat, string text, Size targetSize)
    {
        if (textLayout != null)
            textLayout.Dispose();
        
        textLayout = CreateTextLayout(resourceCreator, textFormat, text, targetSize);
        textLayout.Options = CanvasDrawTextOptions.EnableColorFont;

        return textLayout;
    }
    public CanvasTextFormat CreateCanvasTextFormat()
    {
        return CreateCanvasTextFormat(zoomManager.ZoomedFontSize, zoomManager.ZoomedFontSize + 2, textManager._FontFamily);
    }

    public CanvasTextFormat CreateCanvasTextFormat(float zoomedFontSize, float lineSpacing, FontFamily fontFamily)
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
    public CanvasTextLayout CreateTextLayout(ICanvasResourceCreator resourceCreator, CanvasTextFormat textFormat, string text, Size canvasSize)
    {
        return new CanvasTextLayout(resourceCreator, text, textFormat, (float)canvasSize.Width, (float)canvasSize.Height);
    }
    public CanvasTextLayout CreateTextLayout(ICanvasResourceCreator resourceCreator, CanvasTextFormat textFormat, string text, float width, float height)
    {
        return new CanvasTextLayout(resourceCreator, text, textFormat, width, height);
    }
    public CanvasTextFormat CreateLinenumberTextFormat()
    {
        CanvasTextFormat textFormat = new CanvasTextFormat()
        {
            FontSize = zoomManager.ZoomedFontSize,
            HorizontalAlignment = CanvasHorizontalAlignment.Right,
            VerticalAlignment = CanvasVerticalAlignment.Top,
            WordWrapping = CanvasWordWrapping.NoWrap,
            LineSpacing = zoomManager.ZoomedFontSize + 2,
        };
        textFormat.FontFamily = textManager._FontFamily.Source;
        textFormat.TrimmingGranularity = CanvasTextTrimmingGranularity.None;
        textFormat.TrimmingSign = CanvasTrimmingSign.None;
        return textFormat;
    }

}
