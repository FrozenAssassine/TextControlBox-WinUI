using TextControlBox.Helper;

namespace TextControlBox.Text
{
    internal class StringManager
    {
        private TabSpaceHelper tabSpaceHelper;
        public LineEnding lineEnding;

        public StringManager(TabSpaceHelper tsh)
        {
            tabSpaceHelper = tsh;
        }
        public string CleanUpString(string input)
        {
            //Fix tabs and lineendings
            return tabSpaceHelper.UpdateTabs(LineEndings.CleanLineEndings(input, lineEnding));
        }
    }
}
