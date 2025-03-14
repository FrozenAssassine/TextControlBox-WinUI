using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Xaml;
using System;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;

namespace TextControlBoxNS.Helper;

internal class Utils
{
    public static Size MeasureTextSize(CanvasDevice device, string text, CanvasTextFormat textFormat, float limitedToWidth = 0.0f, float limitedToHeight = 0.0f)
    {
        CanvasTextLayout layout = new CanvasTextLayout(device, text, textFormat, limitedToWidth, limitedToHeight);
        return new Size(layout.DrawBounds.Width, layout.DrawBounds.Height);
    }

    public static Size MeasureLineLenght(CanvasDevice device, string text, CanvasTextFormat textFormat)
    {
        if (text.Length == 0)
            return new Size(0, 0);

        //If the text starts with a tab or a whitespace, replace it with the last character of the line, to
        //get the actual width of the line, because tabs and whitespaces at the beginning are not counted to the lenght
        //Do the same for the end
        double placeholderWidth = 0;

        ReadOnlySpan<char> span = text.AsSpan();
        bool startsWithWhitespace = span[0] == '\t' || span[0] == ' ';
        bool endsWithWhitespace = span[^1] == '\t' || span[^1] == ' ';

        if (startsWithWhitespace || endsWithWhitespace)
        {
            int newLength = text.Length + (startsWithWhitespace ? 1 : 0) + (endsWithWhitespace ? 1 : 0);
            Span<char> newText = stackalloc char[newLength];
            int index = 0;

            if (startsWithWhitespace)
            {
                newText[index++] = '|';
                placeholderWidth += MeasureTextSize(device, "|", textFormat).Width;
            }

            span.CopyTo(newText.Slice(index));
            index += span.Length;

            if (endsWithWhitespace)
            {
                newText[index] = '|';
                placeholderWidth += MeasureTextSize(device, "|", textFormat).Width;
            }

            text = new string(newText);
        }

        CanvasTextLayout layout = new CanvasTextLayout(device, text, textFormat, 0, 0);
        return new Size(layout.DrawBounds.Width - placeholderWidth, layout.DrawBounds.Height);
    }

    public static bool IsKeyPressed(VirtualKey key)
    {
        return Microsoft.UI.Input.InputKeyboardSource.GetKeyStateForCurrentThread(key).HasFlag(CoreVirtualKeyStates.Down);
    }
    public static async Task<bool> IsOverTextLimit(int textLength)
    {
        if (textLength > 100000000)
        {
            await new MessageDialog("Current textlimit is 100 million characters, but your file has " + textLength + " characters").ShowAsync();
            return true;
        }
        return false;
    }

    public static ApplicationTheme ConvertTheme(ElementTheme theme)
    {
        switch (theme)
        {
            case ElementTheme.Light: return ApplicationTheme.Light;
            case ElementTheme.Dark: return ApplicationTheme.Dark;
            case ElementTheme.Default:
                var defaultTheme = new Windows.UI.ViewManagement.UISettings();
                return defaultTheme.GetColorValue(Windows.UI.ViewManagement.UIColorType.Background).ToString() == "#FF000000"
                    ? ApplicationTheme.Dark : ApplicationTheme.Light;

            default: return ApplicationTheme.Light;
        }
    }

    public static Rect CreateRect(Rect rect, float marginLeft = 0, float marginTop = 0)
    {
        return new Rect(
            new Point(
                Math.Floor(rect.Left + marginLeft),//X
                Math.Floor(rect.Top + marginTop)), //Y
            new Point(
                Math.Ceiling(rect.Right + marginLeft), //Width
                Math.Ceiling(rect.Bottom + marginTop))); //Height
    }
}
