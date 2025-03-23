using Collections.Pooled;
using System.Collections.Generic;
using System;

namespace TextControlBoxNS.Core.Text;

internal struct CommentCharacterPosition
{
    public int line;
    public int character;
};

internal class MultilineCommentManager
{
    private CoreTextControlBox coreTextbox;
    private HashSet<string> searchChars = new();
    public List<CommentCharacterPosition> MultilineCommentCharacterLines = new();
    private bool MultiLineCommentsEnabled = false;

    private CommentCharacterPosition[] FindCharacters(string text)
    {
        List<CommentCharacterPosition> positions = new();
        if (!MultiLineCommentsEnabled || string.IsNullOrEmpty(text))
            return positions.ToArray();

        var span = text.AsSpan();
        for (int i = 0; i < span.Length; i++)
        {
            foreach (var searchChar in searchChars)
            {
                if (span.Slice(i).StartsWith(searchChar.AsSpan(), StringComparison.Ordinal))
                {
                    positions.Add(new CommentCharacterPosition { line = 0, character = i });
                    i += searchChar.Length - 1;
                    break;
                }
            }
        }
        return positions.ToArray();
    }

    private CommentCharacterPosition[] FindCharacters(PooledList<string> totalLines)
    {
        List<CommentCharacterPosition> positions = new();
        if (!MultiLineCommentsEnabled || totalLines == null || totalLines.Count == 0)
            return positions.ToArray();

        for (int lineIndex = 0; lineIndex < totalLines.Count; lineIndex++)
        {
            var span = totalLines[lineIndex].AsSpan();
            for (int i = 0; i < span.Length; i++)
            {
                foreach (var searchChar in searchChars)
                {
                    if (span.Slice(i).StartsWith(searchChar.AsSpan(), StringComparison.Ordinal))
                    {
                        positions.Add(new CommentCharacterPosition { line = lineIndex, character = i });
                        i += searchChar.Length - 1;
                        break;
                    }
                }
            }
        }
        return positions.ToArray();
    }

    private void AddText(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        var newPositions = FindCharacters(text);
        MultilineCommentCharacterLines.AddRange(newPositions);
    }

    private void RemoveText(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        var removedPositions = FindCharacters(text);
        foreach (var pos in removedPositions)
        {
            MultilineCommentCharacterLines.RemoveAll(p => p.line == pos.line && p.character == pos.character);
        }
    }

    private void RemoveText(PooledList<string> totalLines, int start, int count)
    {

    }

    public void ChangeSyntaxHighlighting()
    {
        MultilineCommentCharacterLines.Clear();
        searchChars.Clear();

        if (coreTextbox.SyntaxHighlighting == null)
        {
            MultiLineCommentsEnabled = false;
            return;
        }

        var pairs = coreTextbox.SyntaxHighlighting.CodeCommentPairs;
        if (pairs == null || pairs.Length == 0)
        {
            MultiLineCommentsEnabled = false;
            return;
        }

        MultiLineCommentsEnabled = true;
        foreach (var pair in pairs)
        {
            searchChars.Add(pair.StartCharacter);
            searchChars.Add(pair.EndCharacter);
        }
    }
}
