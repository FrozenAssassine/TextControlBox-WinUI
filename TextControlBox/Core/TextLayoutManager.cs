using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using System;
using TextControlBoxNS.Core.Text;
using Windows.Foundation;

namespace TextControlBoxNS.Core;

internal class TextLayoutManager
{
    /// <summary>Pixels added to the font size to form the line height (<c>LineSpacing</c>), so
    /// <c>SingleLineHeight = ZoomedFontSize + LineSpacingPadding</c>. Kept as a named constant so the zoom
    /// anchor can compute the post-zoom line height before the format is rebuilt.</summary>
    public const float LineSpacingPadding = 2f;

    private TextManager textManager;
    private ZoomManager zoomManager;

    /// <summary>When true, the main text format wraps long lines at the layout width instead of clipping.
    /// The line-number format always stays <see cref="CanvasWordWrapping.NoWrap"/>.</summary>
    public bool WordWrap;

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
        float fontSize = zoomManager.ZoomedFontSize > 0 ? zoomManager.ZoomedFontSize : Math.Max(1, textManager._FontSize);
        return CreateCanvasTextFormat(fontSize, fontSize + LineSpacingPadding, textManager._FontFamily);
    }

    public CanvasTextFormat CreateCanvasTextFormat(float zoomedFontSize, float lineSpacing, FontFamily fontFamily)
    {
        CanvasTextFormat textFormat = new CanvasTextFormat()
        {
            FontSize = zoomedFontSize,
            HorizontalAlignment = CanvasHorizontalAlignment.Left,
            VerticalAlignment = CanvasVerticalAlignment.Top,
            WordWrapping = WordWrap ? CanvasWordWrapping.Wrap : CanvasWordWrapping.NoWrap,
            LineSpacing = lineSpacing,
        };
        textFormat.IncrementalTabStop = (float)Math.Round(zoomedFontSize * 3f); //default 137px
        textFormat.FontFamily = fontFamily.Source;
        textFormat.TrimmingGranularity = CanvasTextTrimmingGranularity.None;
        textFormat.TrimmingSign = CanvasTrimmingSign.None;
        return textFormat;
    }
    private static ICanvasResourceCreator ResolveResourceCreator(ICanvasResourceCreator resourceCreator)
    {
        if (resourceCreator is CanvasControl cc)
        {
            try
            {
                if (cc != null && cc.ReadyToDraw && cc.Device != null)
                    return cc;

                return CanvasDevice.GetSharedDevice();
            }
            catch
            {
                return CanvasDevice.GetSharedDevice();
            }
        }
        return resourceCreator ?? CanvasDevice.GetSharedDevice();
    }

    public CanvasTextLayout CreateTextLayout(ICanvasResourceCreator resourceCreator, CanvasTextFormat textFormat, string text, Size canvasSize)
    {
        return new CanvasTextLayout(ResolveResourceCreator(resourceCreator), text, textFormat, (float)canvasSize.Width, (float)canvasSize.Height);
    }
    public CanvasTextLayout CreateTextLayout(ICanvasResourceCreator resourceCreator, CanvasTextFormat textFormat, string text, float width, float height)
    {
        return new CanvasTextLayout(ResolveResourceCreator(resourceCreator), text, textFormat, width, height);
    }
    public CanvasTextFormat CreateLinenumberTextFormat()
    {
        float fontSize = zoomManager?.ZoomedFontSize > 0 ? zoomManager.ZoomedFontSize : Math.Max(1, textManager._FontSize);
        CanvasTextFormat textFormat = new CanvasTextFormat()
        {
            FontSize = fontSize,
            HorizontalAlignment = CanvasHorizontalAlignment.Right,
            VerticalAlignment = CanvasVerticalAlignment.Top,
            WordWrapping = CanvasWordWrapping.NoWrap,
            LineSpacing = fontSize + LineSpacingPadding,
        };
        textFormat.FontFamily = textManager._FontFamily.Source;
        textFormat.TrimmingGranularity = CanvasTextTrimmingGranularity.None;
        textFormat.TrimmingSign = CanvasTrimmingSign.None;
        return textFormat;
    }

    public (CanvasTextLayout spaceGlyph, CanvasTextLayout tabGlyph) CreateGlyphs(ICanvasResourceCreator resourceCreator, CanvasTextFormat textFormat)
    {
        float fontSize = textFormat?.FontSize ?? (zoomManager?.ZoomedFontSize > 0 ? zoomManager.ZoomedFontSize : Math.Max(1, textManager._FontSize));
        float width = fontSize * 2;
        float height = fontSize * 2;
        var creator = ResolveResourceCreator(resourceCreator);
        CanvasTextLayout spaceGlyph = new CanvasTextLayout(creator, "·", textFormat, width, height);
        CanvasTextLayout tabGlyph = new CanvasTextLayout(creator, "→", textFormat, width, height);

        return (spaceGlyph, tabGlyph);
    }
}
