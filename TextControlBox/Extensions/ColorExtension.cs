
namespace TextControlBoxNS.Extensions;

internal static class ColorExtension
{
    public static Windows.UI.Color ToMediaColor(this System.Drawing.Color drawingColor)
    {
        return Windows.UI.Color.FromArgb(drawingColor.A, drawingColor.R, drawingColor.G, drawingColor.B);
    }
}
