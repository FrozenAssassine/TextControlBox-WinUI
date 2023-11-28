namespace TextControlBox_WinUI.Models
{
    public class ScrollBarPosition
    {
        public ScrollBarPosition(ScrollBarPosition ScrollBarPosition)
        {
            this.ValueX = ScrollBarPosition.ValueX;
            this.ValueY = ScrollBarPosition.ValueY;
        }

        /// <summary>
        /// Creates a new instance of the ScrollBarPosition class
        /// </summary>
        /// <param name="ValueX">The horizontal amount scrolled</param>
        /// <param name="ValueY">The vertical amount scrolled</param>
        public ScrollBarPosition(double ValueX = 0, double ValueY = 0)
        {
            this.ValueX = ValueX;
            this.ValueY = ValueY;
        }

        /// <summary>
        /// The amount scrolled horizontally
        /// </summary>
        public double ValueX { get; set; }

        /// <summary>
        /// The amount scrolled vertically
        /// </summary>
        public double ValueY { get; set; }
    }
}
