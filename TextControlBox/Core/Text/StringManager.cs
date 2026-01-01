namespace TextControlBoxNS.Core.Text
{
    internal class StringManager
    {
        private TextManager textManager;
        private TabSpaceManager tabSpaceHelper;

        public void Init(TextManager textManager, TabSpaceManager tabSpaceHelper)
        {
            this.textManager = textManager;
            this.tabSpaceHelper = tabSpaceHelper;
        }

        public string CleanUpString(string input)
        {
            //unify lineendings
            return LineEndings.CleanLineEndings(input, textManager.LineEnding);
        }

        public string RemoveMultilineCharacters(string input)
        {
            //remove all the \n and \r characters from the given string
            return LineEndings.RemoveLineEndings(input);
        }

        public bool HasMultilineCharacters(string input)
        {
            return LineEndings.ContainsLineEndings(input);
        }
    }
}
