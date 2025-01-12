using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml;
using Windows.UI;

namespace TextControlBoxNS.Helper
{
    internal class DesignHelper
    {
        public bool UseDefaultDesign = true;

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
    }
}
