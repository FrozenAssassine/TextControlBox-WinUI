namespace TextControlBox_WinUI.Models
{
    public class CursorSize
    {
        /// <summary>
        /// Creates a new instance of the CursorSize class
        /// </summary>
        /// <param name="Width">The width of the cursor</param>
        /// <param name="Height">The height of the cursor</param>
        /// <param name="OffsetX">The x-offset from the actual cursor position</param>
        /// <param name="OffsetY">The y-offset from the actual cursor position</param>
        public CursorSize(float Width = 0, float Height = 0, float OffsetX = 0, float OffsetY = 0)
        {
            this.Width = Width;
            this.Height = Height;
            this.OffsetX = OffsetX;
            this.OffsetY = OffsetY;
        }

        /// <summary>
        /// The width of the cursor
        /// </summary>
        public float Width { get; private set; }

        /// <summary>
        /// The height of the cursor
        /// </summary>
        public float Height { get; private set; }

        /// <summary>
        /// The left/right offset from the actual cursor position
        /// </summary>
        public float OffsetX { get; private set; }

        /// <summary>
        /// The top/bottom offset from the actual cursor position
        /// </summary>
        public float OffsetY { get; private set; }
    }
}
