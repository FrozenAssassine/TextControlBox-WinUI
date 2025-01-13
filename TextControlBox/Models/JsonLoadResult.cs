namespace TextControlBoxNS
{
    /// <summary>
    /// Represents the result of a JSON load operation for a code language in the textbox.
    /// </summary>
    public class JsonLoadResult
    {
        /// <summary>
        /// Initializes a new instance of the JsonLoadResult class with the specified loading status and CodeLanguage.
        /// </summary>
        /// <param name="succeed">true if the loading operation succeeded; otherwise, false.</param>
        /// <param name="codeLanguage">The CodeLanguage loaded from the JSON data.</param>
        public JsonLoadResult(bool succeed, SyntaxHighlightLanguage codeLanguage)
        {
            this.Succeed = succeed;
            this.CodeLanguage = codeLanguage;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the loading operation succeeded.
        /// </summary>
        public bool Succeed { get; set; }

        /// <summary>
        /// Gets or sets the CodeLanguage that was loaded from the JSON data.
        /// </summary>
        public SyntaxHighlightLanguage CodeLanguage { get; set; }
    }
}
