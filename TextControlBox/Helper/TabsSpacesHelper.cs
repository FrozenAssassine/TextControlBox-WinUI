using Collections.Pooled;
using Microsoft.UI.Xaml.Shapes;
using System.Collections.Generic;
using System.Linq;
using TextControlBoxNS.Core.Text;

namespace TextControlBoxNS.Helper;

internal class TabsSpacesHelper
{
    public const int DefaultSpaces = 4;
    public const bool DefaultUseSpacesInsteadTabs = false;

    // Compute Greatest Common Divisor
    private static int GCD(int a, int b)
    {
        while (b != 0)
        {
            int t = b;
            b = a % b;
            a = t;
        }
        return a;
    }

    private static int CountSpaces(PooledList<string> lines)
    {
        if (lines.Count == 0)
            return DefaultSpaces;

        var spaceCounts = new List<int>();
        foreach (var line in lines)
        {
            int count = 0;
            foreach (char c in line)
            {
                if (c == ' ') count++;
                else break;
            }
            if (count > 0) spaceCounts.Add(count);
        }

        if (spaceCounts.Count == 0)
            return DefaultSpaces;
        
        return spaceCounts.Aggregate(GCD);
    }

    private static int CountSpaces(string text)
    {
        if(text.Length == 0)
            return DefaultSpaces;

        var spaceCounts = new List<int>();
        int count = 0;
        foreach (char c in text)
        {
            if (c == ' ') count++;
            else if (c == '\r' || c == '\n')
            {
                if (count > 0) spaceCounts.Add(count);
                count = 0;
                continue;
            }
        }

        if (spaceCounts.Count == 0)
            return DefaultSpaces;
        return spaceCounts.Aggregate(GCD);
    }

    public static (bool spacesInsteadTabs, int spaces) DetectTabsSpaces(string text, LineEnding lineEnding)
    {
        string lineEndingStr = LineEndings.LineEndingToString(lineEnding);

        int leadingTabs = 0;
        int leadingSpaces = 0;
        int tabs = 0;
        int spaces = 0;
        foreach (var c in text)
        {
            if(c == '\t') tabs++;
            else if (c == ' ') spaces++;
            else if (c == '\r' || c == '\n')
            {
                leadingTabs += tabs;
                leadingSpaces += spaces;
                tabs = spaces = 0;
                continue;
            }
        }

        bool useTabs = leadingTabs > leadingSpaces;
        if (useTabs)
            return (false, 4);

        return (true, CountSpaces(text));
    }

    public static (bool spacesInsteadTabs, int spaces) DetectTabsSpaces(PooledList<string> lines)
    {
        int leadingTabs = 0;
        int leadingSpaces = 0;

        foreach (var line in lines)
        {
            int tabs = 0;
            int spaces = 0;
            foreach (char c in line)
            {
                if (c == '\t') tabs++;
                else if (c == ' ') spaces++;
                else break;
            }

            leadingTabs += tabs;
            leadingSpaces += spaces;
        }

        bool useTabs = leadingTabs > leadingSpaces;
        if (useTabs)
            return (false, 4);

        return (true, CountSpaces(lines));
    }
}
