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
            if (input == null)
                return string.Empty;
            //unify lineendings
            return LineEndings.CleanLineEndings(input, textManager.LineEnding);
        }

        public string RemoveMultilineCharacters(string input)
        {
            if (input == null)
                return string.Empty;
            //remove all the \n and \r characters from the given string
            return LineEndings.RemoveLineEndings(input);
        }

        public bool HasMultilineCharacters(string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            return LineEndings.ContainsLineEndings(input);
        }
    }
}
