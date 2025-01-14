namespace TextControlBoxNS;

/// <summary>
/// Represents a pair of characters used for auto-pairing in text.
/// </summary>
public class AutoPairingPair
{
    /// <summary>
    /// Initializes a new instance of the AutoPairingPair class with the same value for both the opening and closing characters.
    /// </summary>
    /// <param name="value">The character value to be used for both opening and closing.</param>
    public AutoPairingPair(string value)
    {
        this.Value = this.Pair = value;
    }

    /// <summary>
    /// Initializes a new instance of the AutoPairingPair class with different values for the opening and closing characters.
    /// </summary>
    /// <param name="value">The character value to be used as the opening character.</param>
    /// <param name="pair">The character value to be used as the closing character.</param>
    public AutoPairingPair(string value, string pair)
    {
        this.Value = value;
        this.Pair = pair;
    }

    /// <summary>
    /// Checks if the provided input contains the opening character of the pair.
    /// </summary>
    /// <param name="input">The input string to check for the opening character.</param>
    /// <returns>True if the input contains the opening character; otherwise, false.</returns>
    public bool Matches(string input) => input.Contains(this.Value);

    /// <summary>
    /// Gets or sets the character value used for the opening part of the auto-pairing pair.
    /// </summary>
    public string Value { get; set; }

    /// <summary>
    /// Gets or sets the character value used for the closing part of the auto-pairing pair.
    /// </summary>
    public string Pair { get; set; }
}
