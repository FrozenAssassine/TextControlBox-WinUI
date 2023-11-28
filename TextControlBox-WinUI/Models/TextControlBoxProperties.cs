using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Media;
using TextControlBox.Text;
using Windows.UI;

namespace TextControlBox_WinUI.Models
{
    internal class TextControlBoxProperties
    {
        public int ZoomedFontSize = 24;
        public FontFamily FontFamily = new FontFamily("Consolas");
        public float SingleLineHeight = 0;
        public float DefaultVerticalScrollSensitivity = 1;
        public float DefaultHorizontalScrollSensitivity = 1;
        public CanvasSolidColorBrush TextColorBrush;
        public CanvasSolidColorBrush LineNumberColorBrush;
        public CanvasSolidColorBrush CursorColorBrush;
        public CanvasSolidColorBrush LineHighlighterBrush;
        public ElementTheme RequestedTheme = ElementTheme.Default;
        public ApplicationTheme AppTheme = ApplicationTheme.Dark;
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
        public bool DoWordWrap = false;
        public CursorSize _CursorSize;
        public float ZoomFactor = 0;
        public double HorizontalScrollSensitivity = 0;
        public double VerticalScrollSensitivity = 0;
        public string NewLineCharacter = "\n";
    }
}
