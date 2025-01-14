using Collections.Pooled;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Xaml;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using TextControlBoxNS.Extensions;
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
        if (text.StartsWith('\t') || text.StartsWith(' '))
        {
            text = text.Insert(0, "|");
            placeholderWidth += MeasureTextSize(device, "|", textFormat).Width;
        }
        if (text.EndsWith('\t') || text.EndsWith(' '))
        {
            text = text += "|";
            placeholderWidth += MeasureTextSize(device, "|", textFormat).Width;
        }

        CanvasTextLayout layout = new CanvasTextLayout(device, text, textFormat, 0, 0);
        return new Size(layout.DrawBounds.Width - placeholderWidth, layout.DrawBounds.Height);
    }

    //Get the longest line in the textbox
    public static int GetLongestLineIndex(PooledList<string> totalLines)
    {
        int longestIndex = 0;
        int oldLenght = 0;
        for (int i = 0; i < totalLines.Count; i++)
        {
            var lenght = totalLines[i].Length;
            if (lenght > oldLenght)
            {
                longestIndex = i;
                oldLenght = lenght;
            }
        }
        return longestIndex;
    }

    public static int GetLongestLineLength(string text)
    {
        var splitted = text.Split("\n");
        int oldLenght = 0;
        for (int i = 0; i < splitted.Length; i++)
        {
            var lenght = splitted[i].Length;
            if (lenght > oldLenght)
            {
                oldLenght = lenght;
            }
        }
        return oldLenght;
    }

    public static bool CursorPositionsAreEqual(CursorPosition first, CursorPosition second)
    {
        return first.LineNumber == second.LineNumber && first.CharacterPosition == second.CharacterPosition;
    }

    public static string[] SplitAt(string text, int index)
    {
        string first = index < text.Length ? text.SafeRemove(index) : text;
        string second = index < text.Length ? text.Safe_Substring(index) : "";
        return new string[] { first, second };
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
    public static void Benchmark(Action action, string text)
    {
        Stopwatch sw = new Stopwatch();
        sw.Start();
        action.Invoke();
        sw.Stop();

        Debug.WriteLine(text + " took " + sw.ElapsedMilliseconds + "::" + sw.ElapsedTicks);
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

    public static Point GetTextboxstartingPoint(UIElement relativeTo)
    {
        return relativeTo.TransformToVisual(Window.Current.Content).TransformPoint(new Point(0, 0));
    }
    public static int CountLines(string text, string newLineCharacter)
    {
        //is slower than normal string operation but consumes soo much less memory
        var span = text.AsSpan();
        int lineCount = 1;

        int newLineLength = newLineCharacter.Length;

        for (int i = 0; i <= span.Length - newLineLength; i++)
        {
            if (span.Slice(i, newLineLength).SequenceEqual(newLineCharacter.AsSpan()))
            {
                lineCount++;
                i += newLineLength - 1;
            }
        }

        return lineCount;
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
