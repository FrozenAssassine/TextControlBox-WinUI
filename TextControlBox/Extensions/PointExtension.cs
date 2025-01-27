using Windows.Foundation;

namespace TextControlBoxNS.Extensions;

internal static class PointExtension
{
    public static Point Subtract(this Point point, double subtractX, double subtractY)
    {
        return new Point(point.X - subtractX, point.Y - subtractY);
    }
}
