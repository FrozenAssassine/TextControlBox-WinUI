using Collections.Pooled;
using System;
using System.Diagnostics;

namespace TextControlBox.Helper
{
    internal class TabSpaceHelper
    {
        private int _NumberOfSpaces = 4;
        private string oldSpaces = "    ";

        public int NumberOfSpaces
        {
            get => _NumberOfSpaces;
            set
            {
                if (value != _NumberOfSpaces)
                {
                    oldSpaces = spaces;
                    _NumberOfSpaces = value;
                    spaces = new string(' ', _NumberOfSpaces);
                }
            }
        }
        public bool UseSpacesInsteadTabs = false;
        public string TabCharacter { get => UseSpacesInsteadTabs ? spaces : tab; }
        private string spaces = "    ";
        private string tab = "\t";

        public void UpdateNumberOfSpaces(PooledList<string> lines)
        {
            ReplaceSpacesToSpaces(lines);
        }

        public void UpdateTabs(PooledList<string> lines)
        {
            if (UseSpacesInsteadTabs)
            {
                ReplaceTabsToSpaces(lines);
            }
            else
                ReplaceSpacesToTabs(lines);
        }
        public string UpdateTabs(string input)
        {
            if (UseSpacesInsteadTabs)
                return Replace(input, tab, spaces);
            return Replace(input, spaces, tab);
        }

        private void ReplaceSpacesToSpaces(PooledList<string> lines)
        {
            Debug.WriteLine("START:" + oldSpaces + ":" + spaces + ":");
            for (int i = 0; i < lines.Count; i++)
            {
                lines[i] = Replace(lines[i], oldSpaces, spaces);
            }
        }
        private void ReplaceSpacesToTabs(PooledList<string> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                lines[i] = Replace(lines[i], spaces, tab);
            }
        }
        private void ReplaceTabsToSpaces(PooledList<string> lines)
        {
            for (int i = 0; i < lines.Count; i++)
            {
                lines[i] = Replace(lines[i], "\t", spaces);
            }
        }
        public string Replace(string input, string find, string replace)
        {
            return input.Replace(find, replace, StringComparison.OrdinalIgnoreCase);
        }
    }
}
