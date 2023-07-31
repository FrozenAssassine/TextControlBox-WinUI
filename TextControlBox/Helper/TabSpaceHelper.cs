using Collections.Pooled;
using System;

namespace TextControlBoxNS.Helper
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

        public void UpdateNumberOfSpaces(PooledList<string> totalLines)
        {
            ReplaceSpacesToSpaces(totalLines);
        }

        public void UpdateTabs(PooledList<string> totalLines)
        {
            if (UseSpacesInsteadTabs)
                ReplaceTabsToSpaces(totalLines);
            else
                ReplaceSpacesToTabs(totalLines);
        }
        public string UpdateTabs(string input)
        {
            if (UseSpacesInsteadTabs)
                return Replace(input, Tab, Spaces);
            return Replace(input, Spaces, Tab);
        }

        private void ReplaceSpacesToSpaces(PooledList<string> totalLines)
        {
            for (int i = 0; i < totalLines.Count; i++)
            {
                totalLines[i] = Replace(totalLines[i], OldSpaces, Spaces);
            }
        }
        private void ReplaceSpacesToTabs(PooledList<string> TotalLines)
        {
            for (int i = 0; i < TotalLines.Count; i++)
            {
                TotalLines[i] = Replace(TotalLines[i], Spaces, Tab);
            }
        }
        private void ReplaceTabsToSpaces(PooledList<string> totalLines)
        {
            for (int i = 0; i < totalLines.Count; i++)
            {
                totalLines[i] = Replace(totalLines[i], "\t", Spaces);
            }
        }
        public string Replace(string input, string find, string replace)
        {
            return input.Replace(find, replace, StringComparison.OrdinalIgnoreCase);
        }
    }
}
