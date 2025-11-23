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

        int spaces = spaceCounts.Aggregate(GCD);
        return spaces < 2 ? DefaultSpaces : spaces;
    }

    public static (bool spacesInsteadTabs, int spaces) DetectTabsSpaces(string text)
    {
        List<int> spaceIndents = new();
        List<int> tabIndents = new();

        int currentSpaces = 0;
        int currentTabs = 0;
        bool atLineStart = true;

        foreach (char c in text)
        {
            if (c == '\r' || c == '\n')
            {
                if (atLineStart)
                {
                    currentSpaces = currentTabs = 0;
                }
                else
                {
                    if (currentSpaces > 0) spaceIndents.Add(currentSpaces);
                    if (currentTabs > 0) tabIndents.Add(currentTabs);
                }

                currentSpaces = currentTabs = 0;
                atLineStart = true;
                continue;
            }

            if (atLineStart)
            {
                if (c == ' ') { currentSpaces++; continue; }
                if (c == '\t') { currentTabs++; continue; }

                atLineStart = false;
            }
        }

        // Handle last line without newline
        if (!atLineStart)
        {
            if (currentSpaces > 0) spaceIndents.Add(currentSpaces);
            if (currentTabs > 0) tabIndents.Add(currentTabs);
        }

        // Decide whether tabs or spaces dominate
        bool useSpaces = spaceIndents.Count >= tabIndents.Count;

        if (!useSpaces)
            return (false, 4); // your original behavior for tab-indent

        // Compute indent step using GCD of differences
        if (spaceIndents.Count == 0)
            return (true, 4); // fallback

        // Sort indents and calculate differences
        var sorted = spaceIndents.Distinct().OrderBy(x => x).ToList();
        if (sorted.Count == 1)
            return (true, sorted[0]); // only one indent depth → assume it's the indent size

        int gcd = 0;
        for (int i = 1; i < sorted.Count; i++)
        {
            int diff = sorted[i] - sorted[i - 1];
            if (diff > 0)
                gcd = GCD(gcd, diff);
        }

        // Final indent size
        return (true, gcd > 0 ? gcd : sorted[0]); // fallback to smallest leading indent
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
            return (false, DefaultSpaces);

        return (true, CountSpaces(lines));
    }
}
