﻿using TextControlBoxNS.Helper;

namespace TextControlBoxNS.Text
{
    internal class StringManager
    {
        private TextManager textManager;
        private TabSpaceHelper tabSpaceHelper;

        public StringManager stringManager;

        public string CleanUpString(string input)
        {
            //Fix tabs and lineendings
            return tabSpaceHelper.UpdateTabs(LineEndings.CleanLineEndings(input, textManager._LineEnding));
        }
    }
}
