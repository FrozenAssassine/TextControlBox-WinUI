using Collections.Pooled;
using System;
using System.Diagnostics;

namespace TextControlBox.Helper
{
    internal class TabSpaceHelper
    {
        private int _NumberOfSpaces = 4;
        private string OldSpaces = "    ";

        public int NumberOfSpaces
        {
            get => _NumberOfSpaces;
            set
            {
                if (value != _NumberOfSpaces)
                {
                    OldSpaces = Spaces;
                    _NumberOfSpaces = value;
                    Spaces = new string(' ', _NumberOfSpaces);
                }
            }
        }
        public bool UseSpacesInsteadTabs = false;
        public string TabCharacter { get => UseSpacesInsteadTabs ? Spaces : Tab; }
        private string Spaces = "    ";
        private string Tab = "\t";

        public void UpdateNumberOfSpaces(PooledList<string> TotalLines)
        {
            ReplaceSpacesToSpaces(TotalLines);
        }

        public void UpdateTabs(PooledList<string> TotalLines)
        {
            if (UseSpacesInsteadTabs)
            {
                ReplaceTabsToSpaces(TotalLines);
            }
            else
                ReplaceSpacesToTabs(TotalLines);
        }
        public string UpdateTabs(string input)
        {
            if (UseSpacesInsteadTabs)
                return Replace(input, Tab, Spaces);
            return Replace(input, Spaces, Tab);
        }

        private void ReplaceSpacesToSpaces(PooledList<string> TotalLines)
        {
            Debug.WriteLine("START:" + OldSpaces + ":" + Spaces + ":");
            for (int i = 0; i < TotalLines.Count; i++)
            {
                TotalLines[i] = Replace(TotalLines[i], OldSpaces, Spaces);
            }
        }
        private void ReplaceSpacesToTabs(PooledList<string> TotalLines)
        {
            for (int i = 0; i < TotalLines.Count; i++)
            {
                TotalLines[i] = Replace(TotalLines[i], Spaces, Tab);
            }
        }
        private void ReplaceTabsToSpaces(PooledList<string> TotalLines)
        {
            for (int i = 0; i < TotalLines.Count; i++)
            {
                TotalLines[i] = Replace(TotalLines[i], "\t", Spaces);
            }
        }
        public string Replace(string input, string find, string replace)
        {
            return input.Replace(find, replace, StringComparison.OrdinalIgnoreCase);
        }
    }
}
