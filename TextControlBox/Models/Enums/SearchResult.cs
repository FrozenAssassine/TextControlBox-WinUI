namespace TextControlBoxNS;

/// <summary>
/// Represents the result of a search operation in the textbox.
/// </summary>
public enum SearchResult
{
    /// <summary>
    /// The target text was found during the search operation.
    /// </summary>
    Found,

    /// <summary>
    /// The target text was not found during the search operation.
    /// </summary>
    NotFound,

    /// <summary>
    /// The search input provided for the search operation was invalid or empty.
    /// </summary>
    InvalidInput,

    /// <summary>
    /// The search operation reached the beginning of the text without finding the target text.
    /// </summary>
    ReachedBegin,

    /// <summary>
    /// The search operation reached the end of the text without finding the target text.
    /// </summary>
    ReachedEnd,

    /// <summary>
    /// The search operation was attempted, but the search was not started.
    /// </summary>
    SearchNotOpened
}
