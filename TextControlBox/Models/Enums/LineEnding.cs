namespace TextControlBoxNS;

/// <summary>
/// Represents the three default line endings: LF (Line Feed), CRLF (Carriage Return + Line Feed), and CR (Carriage Return).
/// </summary>
/// <remarks>
/// The LineEnding enum represents the different line ending formats that can be used in the textbox.
/// The enum defines three values: LF, CRLF, and CR, corresponding to the different line ending formats.
/// - LF: Represents the Line Feed character '\n' used for line breaks in Unix-based systems.
/// - CRLF: Represents the Carriage Return + Line Feed sequence '\r\n' used for line breaks in Windows-based systems.
/// - CR: Represents the Carriage Return character '\r' used for line breaks in older Macintosh systems.
/// </remarks>
public enum LineEnding
{
    /// <summary>
    /// Line Feed ('\n')
    /// </summary>
    LF,
    /// <summary>
    /// Carriage Return + Line Feed ('\r\n')
    /// </summary>
    CRLF,
    /// <summary>
    /// Carriage Return ('\r')
    /// </summary>
    CR
}