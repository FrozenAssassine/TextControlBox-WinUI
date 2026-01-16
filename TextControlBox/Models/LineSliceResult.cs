using System;

namespace TextControlBoxNS.Models;

internal readonly ref struct LineSliceResult(string text, ReadOnlySpan<string> lines)
{
    public readonly string Text = text;
    public readonly ReadOnlySpan<string> Lines = lines;
}
