using Collections.Pooled;
using System;
using System.Collections.Generic;
using TextControlBox.Extensions;
using TextControlBox.Helper;
using TextControlBox.Text;

namespace TextControlBox_WinUI.Helper
{
    internal class TextManager
    {
        public PooledList<string> Lines = new PooledList<string>();

        public void InsertText(TextSelection position, string text)
        {
            if (Lines.Count == 0)
                AddLineBreak();

            var line = Lines[position.StartPosition.LineNumber];
            if(line.Length == position.StartPosition.CharacterPosition)
                Lines[position.StartPosition.LineNumber] += text;
            else
                Lines[position.StartPosition.LineNumber].Insert(position.StartPosition.CharacterPosition, text);
        }

        public void AddLineBreak(int index = 0)
        {
            Lines.Insert(index, "");
        }
        public void RemoveLine(CursorPosition position) 
        {
            Lines.RemoveAt(position.LineNumber);
        }
        public void ClearLines()
        {
            Lines.Clear();
        }
        public void ChangeLine(CursorPosition position, string text)
        {
            Lines[position.LineNumber] = text;
        }

        public IEnumerable<string> GetLines(int start, int count)
        {
            return Lines.GetLines(start, count);
        }

        public void ClearAndLoadLines(IEnumerable<string> lines)
        {
            ListHelper.Clear(Lines);
            Lines.AddRange(lines);
        }

        public void AddLineIfEmpty(string text = "")
        {
            if (Lines.Count == 0)
                Lines.Add(text);
        }
    }
}
