namespace TextControlBoxNS.Models.Structs;

/// <summary>
/// Describes the offset between content and vertical scroll area borders. 
/// Two Double values describe the Top and Bottom offsets, respectively.
/// </summary>
public struct VerticalScrollOffset
{
    /// <summary>
    /// The top offset of content
    /// </summary>
    public double Top { get; set; }

    /// <summary>
    /// The bottom offset of content
    /// </summary>

    public double Bottom { get; set; }

    /// <summary>
    /// Creates new VerticalScrollOffset object
    /// </summary>
    /// <param name="uniformLength">The top and bottom content offset</param>
    public VerticalScrollOffset(double uniformLength)
    {
        Top = (Bottom = uniformLength);
    }

    /// <summary>
    /// Creates new VerticalScrollOffset object
    /// </summary>
    /// <param name="top">The top offset of content</param>
    /// <param name="bottom">The bottom offset of content</param>
    public VerticalScrollOffset(double top, double bottom)
    {
        this.Top = top;
        this.Bottom = bottom;
    }


    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Top}, {Bottom}";
    }

    /// <inheritdoc/>
    public override bool Equals(object obj)
    {
        if (obj is VerticalScrollOffset verticalScrollOffset)
        {
            return this == verticalScrollOffset;
        }

        return false;
    }
    
    /// <summary>
    /// Indicates whether this instance and a specified object are equal.
    /// </summary>
    /// <returns>true if verticalScrollOffset and this instance are represent the same value; otherwise, false.</returns>
    public readonly bool Equals(VerticalScrollOffset verticalScrollOffset)
    {
        return this == verticalScrollOffset;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Top.GetHashCode() ^ Bottom.GetHashCode();
    }

    /// <summary>
    /// Compares two VerticalScrollOffset objects
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <returns>true if offsets are same; otherwise, false.</returns>
    public static bool operator ==(VerticalScrollOffset t1, VerticalScrollOffset t2)
    {
        if (t1.Top == t2.Top)
        {
            return t1.Bottom == t2.Bottom;
        }

        return false;
    }

    /// <summary>
    /// Compares two VerticalScrollOffset objects
    /// </summary>
    /// <param name="t1"></param>
    /// <param name="t2"></param>
    /// <returns>true if offsets are different; otherwise, false.</returns>
    public static bool operator !=(VerticalScrollOffset t1, VerticalScrollOffset t2)
    {
        return !(t1 == t2);
    }
}
