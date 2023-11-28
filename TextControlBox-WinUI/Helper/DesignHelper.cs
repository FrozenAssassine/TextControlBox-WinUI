using Microsoft.Graphics.Canvas.Brushes;
using Microsoft.Graphics.Canvas;
using TextControlBox_WinUI.Models;
using Microsoft.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas.UI.Xaml;

namespace TextControlBox_WinUI.Helper
{
    internal class DesignHelper
    {
        public TextControlBoxProperties textboxProperties;
        public bool ColorResourcesCreated = false;
        public TextControlBoxDesign CurrentDesign;
        public bool UseDefaultDesign { get; set; }

        private bool _NeedsUpdateColorResources = true;

        public DesignHelper(TextControlBoxProperties textboxProperties)
        {
            this.textboxProperties = textboxProperties;
        }

        public void NeedsUpdateColorResources()
        {
            _NeedsUpdateColorResources = true;
        }

        public void UpdateColorResources(ICanvasResourceCreatorWithDpi resourceCreator, CanvasControl canvas_text, Grid mainGrid)
        {
            if (!_NeedsUpdateColorResources)
                return;

            canvas_text.ClearColor = CurrentDesign.LineNumberBackground;
            mainGrid.Background = CurrentDesign.Background;
            textboxProperties.TextColorBrush = new CanvasSolidColorBrush(resourceCreator, CurrentDesign.TextColor);
            textboxProperties.CursorColorBrush = new CanvasSolidColorBrush(resourceCreator, CurrentDesign.CursorColor);
            textboxProperties.LineNumberColorBrush = new CanvasSolidColorBrush(resourceCreator, CurrentDesign.LineNumberColor);
            textboxProperties.LineHighlighterBrush = new CanvasSolidColorBrush(resourceCreator, CurrentDesign.LineHighlighterColor);
            _NeedsUpdateColorResources = false;
        }
    }
}
