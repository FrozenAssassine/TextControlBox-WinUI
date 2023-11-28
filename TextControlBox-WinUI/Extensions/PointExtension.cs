using Windows.Foundation;

namespace TextControlBox.Extensions
{
    internal static class PointExtension
    {
        public static Point Subtract(this Point point, double subtractX, double subtractY)
        {
            return new Point(point.X - subtractX, point.Y - subtractY);
        }
        public static Point Subtract(this Point point, Point subtract)
        {
            return new Point(point.X - subtract.X, point.Y - subtract.Y);
        }
    }
}
