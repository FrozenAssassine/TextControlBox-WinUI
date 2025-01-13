using Collections.Pooled;
using System;
using TextControlBoxNS.Text;

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

        private readonly TextManager textManager;

        public void UpdateNumberOfSpaces()
        {
            ReplaceSpacesToSpaces();
        }

        public void UpdateTabs()
        {
            if (UseSpacesInsteadTabs)
                ReplaceTabsToSpaces();
            else
                ReplaceSpacesToTabs();
        }
        public string UpdateTabs(string input)
        {
            if (UseSpacesInsteadTabs)
                return Replace(input, Tab, Spaces);
            return Replace(input, Spaces, Tab);
        }

        private void ReplaceSpacesToSpaces()
        {
            for (int i = 0; i < textManager.LinesCount; i++)
            {
                textManager.totalLines[i] = Replace(textManager.totalLines[i], OldSpaces, Spaces);
            }
        }
        private void ReplaceSpacesToTabs()
        {
            for (int i = 0; i < textManager.LinesCount; i++)
            {
                textManager.totalLines[i] = Replace(textManager.totalLines[i], Spaces, Tab);
            }
        }
        private void ReplaceTabsToSpaces()
        {
            for (int i = 0; i < textManager.LinesCount; i++)
            {
                textManager.totalLines[i] = Replace(textManager.totalLines[i], "\t", Spaces);
            }
        }
        public string Replace(string input, string find, string replace)
        {
            return input.Replace(find, replace, StringComparison.OrdinalIgnoreCase);
        }
    }
}
