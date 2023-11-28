using Windows.UI;

namespace TextControlBox.Extensions
{
    internal static class ColorExtension
    {
        public static Windows.UI.Color ToMediaColor(this System.Drawing.Color drawingColor)
        {
            return Windows.UI.Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
        }
        public static string ToHex(this Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";
    }
}
