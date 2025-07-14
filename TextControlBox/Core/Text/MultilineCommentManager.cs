using Collections.Pooled;
using System.Collections.Generic;
using System;
using System.Diagnostics;

namespace TextControlBoxNS.Core.Text;

internal struct CommentCharacterPosition
{
    public int line;
    public int character;
};

internal class MultilineCommentManager
{
    private CoreTextControlBox coreTextbox;
    private TextManager textManager;
    private HashSet<string> searchChars = new();
    public List<CommentCharacterPosition> MultilineCommentCharacterLines = new();
    private bool MultiLineCommentsEnabled = true;
    
    public void Init(CoreTextControlBox coreTextbox, TextManager textManager)
    {
        this.textManager = textManager;
        this.coreTextbox = coreTextbox;
    }

    private string lastIncompleteMatch = ""; // Track unfinished sequences

    private CommentCharacterPosition[] FindCharacters(string text)
    {
        List<CommentCharacterPosition> positions = new();
        if (!MultiLineCommentsEnabled || string.IsNullOrEmpty(text))
            return positions.ToArray();

        var span = text.AsSpan();
        int i = 0;

        // Check if lastIncompleteMatch + first character in new text completes a comment sequence
        if (!string.IsNullOrEmpty(lastIncompleteMatch))
        {
            foreach (var searchChar in searchChars)
            {
                if ((lastIncompleteMatch + span[0]).Equals(searchChar, StringComparison.Ordinal))
                {
                    positions.Add(new CommentCharacterPosition { line = 0, character = 0 });
                    i = searchChar.Length - 1; // Skip past matched sequence
                    break;
                }
            }
        }

        for (; i < span.Length;)
        {
            bool matched = false;
            foreach (var searchChar in searchChars)
            {
                if (i + searchChar.Length <= span.Length && span.Slice(i, searchChar.Length).Equals(searchChar.AsSpan(), StringComparison.Ordinal))
                {
                    positions.Add(new CommentCharacterPosition { line = 0, character = i });
                    i += searchChar.Length; // Move ahead by full match length
                    matched = true;
                    break;
                }
            }

            if (!matched)
                i++;
        }

        // Store last character in case a partial sequence is being typed
        lastIncompleteMatch = span.Length > 0 ? span[^1].ToString() : "";

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
            int i = 0;

            // Check if lastIncompleteMatch + first character in new line completes a comment sequence
            if (!string.IsNullOrEmpty(lastIncompleteMatch) && span.Length > 0)
            {
                foreach (var searchChar in searchChars)
                {
                    if ((lastIncompleteMatch + span[0]).Equals(searchChar, StringComparison.Ordinal))
                    {
                        positions.Add(new CommentCharacterPosition { line = lineIndex, character = 0 });
                        i = searchChar.Length - 1; // Move past the matched sequence
                        break;
                    }
                }
            }

            for (; i < span.Length;)
            {
                bool matched = false;
                foreach (var searchChar in searchChars)
                {
                    if (i + searchChar.Length <= span.Length && span.Slice(i, searchChar.Length).Equals(searchChar.AsSpan(), StringComparison.Ordinal))
                    {
                        positions.Add(new CommentCharacterPosition { line = lineIndex, character = i });
                        i += searchChar.Length; // Move ahead by full match length
                        matched = true;
                        break;
                    }
                }

                if (!matched)
                    i++;
            }

            // Store last character of this line in case it's part of an incomplete comment sequence
            lastIncompleteMatch = span.Length > 0 ? span[^1].ToString() : "";
        }

        return positions.ToArray();
    }

    public void FindCharacters()
    {
        MultilineCommentCharacterLines.Clear();
        MultilineCommentCharacterLines.AddRange(this.FindCharacters(textManager.totalLines));
    }

    public void AddText(string text)
    {
        if (string.IsNullOrEmpty(text)) return;
        var newPositions = FindCharacters(text);
        MultilineCommentCharacterLines.AddRange(newPositions);
    }

    public void RemoveText(string text)
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
