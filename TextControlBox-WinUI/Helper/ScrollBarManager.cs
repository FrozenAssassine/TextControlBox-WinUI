using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using TextControlBox_WinUI.Models;

namespace TextControlBox_WinUI.Helper
{
    internal class ScrollBarManager
    {
        private Grid scrollGrid;
        private TextManager textManager;
        private TextControlBoxProperties textboxProps;

        public ScrollBarManager(Grid scrollGrid, TextManager textManager, TextControlBoxProperties textboxProps)
        {
            this.scrollGrid = scrollGrid;
            this.textManager = textManager;
            this.textboxProps = textboxProps;
        }
        public void SetVerticalScrollBounds(ScrollBar scrollbar, CanvasControl sender)
        {
            scrollbar.Maximum = ((textManager.Lines.Count + 1) * textboxProps.SingleLineHeight - scrollGrid.ActualHeight) / textboxProps.DefaultVerticalScrollSensitivity;
            scrollbar.ViewportSize = sender.ActualHeight;
        }

        public void SetHorizontalScrollBounds(ScrollBar scrollbar, CanvasControl sender, LongestLineCalculationHelper longestLineCalcHelper)
        {
            var lineLength = longestLineCalcHelper.GetActualLongestLineLength();

            scrollbar.Maximum = (lineLength.Width <= sender.ActualWidth ? 0 : lineLength.Width - sender.ActualWidth + 50);
            scrollbar.ViewportSize = sender.ActualWidth;

        }
    }
}
