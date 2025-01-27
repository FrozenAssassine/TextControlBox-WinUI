namespace TextControlBoxNS;

/// <summary>
/// Represents the size of the cursor in the textbox.
/// </summary>
public class CursorSize
{
    /// <summary>
    /// Creates a new instance of the CursorSize class
    /// </summary>
    /// <param name="width">The width of the cursor</param>
    /// <param name="height">The height of the cursor</param>
    /// <param name="offsetX">The x-offset from the actual cursor position</param>
    /// <param name="offsetY">The y-offset from the actual cursor position</param>
    public CursorSize(float width = 0, float height = 0, float offsetX = 0, float offsetY = 0)
    {
        this.Width = width;
        this.Height = height;
        this.OffsetX = offsetX;
        this.OffsetY = offsetY;
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
