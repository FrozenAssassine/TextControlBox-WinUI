namespace TextControlBoxNS
{
    /// <summary>
    /// Represents the position of a scrollbar, containing the horizontal and vertical scroll positions.
    /// </summary>
    public class ScrollBarPosition
    {
        /// <summary>
        /// Initializes a new instance of the ScrollBarPosition class with the provided values.
        /// </summary>
        /// <param name="scrollBarPosition">The existing ScrollBarPosition object from which to copy the values.</param>

        public ScrollBarPosition(ScrollBarPosition scrollBarPosition)
        {
            this.ValueX = scrollBarPosition.ValueX;
            this.ValueY = scrollBarPosition.ValueY;
        }

        /// <summary>
        /// Creates a new instance of the ScrollBarPosition class
        /// </summary>
        /// <param name="valueX">The horizontal amount scrolled</param>
        /// <param name="valueY">The vertical amount scrolled</param>
        public ScrollBarPosition(double valueX = 0, double valueY = 0)
        {
            this.ValueX = valueX;
            this.ValueY = valueY;
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
