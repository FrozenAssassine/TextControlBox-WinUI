namespace TextControlBoxNS
{
    /// <summary>
    /// Represents a code language configuration used for syntax highlighting and auto-pairing in the text content.
    /// </summary>
    public class CodeLanguage
    {
        /// <summary>
        /// Gets or sets the name of the code language.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the description of the code language.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets an array of file filters for the code language.
        /// </summary>
        public string[] Filter { get; set; }

        /// <summary>
        /// Gets or sets the author of the code language definition.
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// Gets or sets an array of syntax highlights for the code language.
        /// </summary>
        public SyntaxHighlights[] Highlights { get; set; }

        /// <summary>
        /// Gets or sets an array of auto-pairing pairs for the code language.
        /// </summary>
        public AutoPairingPair[] AutoPairingPair { get; set; }
    }

}
