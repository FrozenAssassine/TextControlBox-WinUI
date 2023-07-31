using TextControlBoxNS.Helper;

namespace TextControlBoxNS.Text
{
    internal class StringManager
    {
        private TabSpaceHelper tabSpaceHelper;
        public LineEnding lineEnding;

        public StringManager(TabSpaceHelper tabSpaceHelper)
        {
            this.tabSpaceHelper = tabSpaceHelper;
        }
        public string CleanUpString(string input)
        {
            //Fix tabs and lineendings
            return tabSpaceHelper.UpdateTabs(LineEndings.CleanLineEndings(input, lineEnding));
        }
    }
}
