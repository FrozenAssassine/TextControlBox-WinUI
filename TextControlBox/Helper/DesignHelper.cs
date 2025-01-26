using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using TextControlBoxNS.Core;
using TextControlBoxNS.Core.Renderer;
using Windows.UI;

namespace TextControlBoxNS.Helper;

internal class DesignHelper
{
    public bool UseDefaultDesign = true;
    public bool ColorResourcesCreated = false;
    public TextControlBoxDesign _Design { get; set; }
    public ElementTheme _RequestedTheme = ElementTheme.Default;
    public ApplicationTheme _AppTheme = ApplicationTheme.Light;
    public TextControlBoxDesign LightDesign = new TextControlBoxDesign(
        new SolidColorBrush(Color.FromArgb(0, 255, 255, 255)),
        Color.FromArgb(255, 50, 50, 50),
        Color.FromArgb(100, 0, 100, 255),
        Color.FromArgb(255, 0, 0, 0),
        Color.FromArgb(50, 200, 200, 200),
        Color.FromArgb(255, 180, 180, 180),
        Color.FromArgb(0, 0, 0, 0),
        Color.FromArgb(100, 200, 120, 0)
        );
    public TextControlBoxDesign DarkDesign = new TextControlBoxDesign(
        new SolidColorBrush(Color.FromArgb(0, 30, 30, 30)),
        Color.FromArgb(255, 255, 255, 255),
        Color.FromArgb(100, 0, 100, 255),
        Color.FromArgb(255, 255, 255, 255),
        Color.FromArgb(50, 100, 100, 100),
        Color.FromArgb(255, 100, 100, 100),
        Color.FromArgb(0, 0, 0, 0),
        Color.FromArgb(100, 160, 80, 0)
        );

    //Colors:
    public CanvasSolidColorBrush TextColorBrush;
    public CanvasSolidColorBrush CursorColorBrush;
    public CanvasSolidColorBrush LineNumberColorBrush;
    public CanvasSolidColorBrush LineHighlighterBrush;

    private CoreTextControlBox coreTextbox;
    private TextRenderer textRenderer;
    private CanvasUpdateManager canvasUpdateManager;

    public void Init(CoreTextControlBox coreTextbox, TextRenderer textRenderer, CanvasUpdateManager canvasUpdateManager)
    {
        this.coreTextbox = coreTextbox;
        this.textRenderer = textRenderer;
        this.canvasUpdateManager = canvasUpdateManager;
    }

    public void CreateColorResources(ICanvasResourceCreatorWithDpi resourceCreator)
    {
        if (ColorResourcesCreated)
            return;

        coreTextbox.canvasLineNumber.ClearColor = _Design.LineNumberBackground;
        coreTextbox.mainGrid.Background = _Design.Background;
        TextColorBrush = new CanvasSolidColorBrush(resourceCreator, _Design.TextColor);
        CursorColorBrush = new CanvasSolidColorBrush(resourceCreator, _Design.CursorColor);
        LineNumberColorBrush = new CanvasSolidColorBrush(resourceCreator, _Design.LineNumberColor);
        LineHighlighterBrush = new CanvasSolidColorBrush(resourceCreator, _Design.LineHighlighterColor);
        ColorResourcesCreated = true;
    }

    public ElementTheme RequestedTheme
    {
        get => _RequestedTheme;
        set
        {
            _RequestedTheme = value;
            _AppTheme = Utils.ConvertTheme(value);

            if (UseDefaultDesign)
                _Design = _AppTheme == ApplicationTheme.Light ? LightDesign : DarkDesign;

            coreTextbox.Background = _Design.Background;
            ColorResourcesCreated = false;
            textRenderer.NeedsUpdateTextLayout = true;
            canvasUpdateManager.UpdateAll();
        }
    }

    public TextControlBoxDesign Design
    {
        get => UseDefaultDesign ? null : _Design;
        set
        {
            _Design = value != null ? value : _AppTheme == ApplicationTheme.Dark ? DarkDesign : LightDesign;
            UseDefaultDesign = value == null;

            coreTextbox.Background = _Design.Background;
            ColorResourcesCreated = false;
            canvasUpdateManager.UpdateAll();
        }
    }
}
