using System.Linq;

namespace TextControlBoxNS.Core.Text;

internal class AutoPairing
{
    public static (string text, int length) AutoPair(CoreTextControlBox textbox, string inputtext)
    {
        if (!textbox.DoAutoPairing || inputtext.Length != 1 || textbox.SyntaxHighlighting == null || textbox.SyntaxHighlighting.AutoPairingPair == null)
            return (inputtext, inputtext.Length);

        var res = textbox.SyntaxHighlighting.AutoPairingPair.Where(x => x.Matches(inputtext));
        if (res.Count() == 0)
            return (inputtext, inputtext.Length);

        if (res.ElementAt(0) is AutoPairingPair pair)
            return (inputtext + pair.Pair, inputtext.Length);
        return (inputtext, inputtext.Length);
    }

    public static string AutoPairSelection(CoreTextControlBox textbox, string inputtext)
    {
        if (!textbox.DoAutoPairing || inputtext.Length != 1 || textbox.SyntaxHighlighting == null || textbox.SyntaxHighlighting.AutoPairingPair == null)
            return inputtext;

        var res = textbox.SyntaxHighlighting.AutoPairingPair.Where(x => x.Value.Equals(inputtext)).ToArray();
        if (res.Length == 0)
            return inputtext;

        if (res[0] is AutoPairingPair pair)
        {
            textbox.SurroundSelectionWith(inputtext, pair.Pair);
            return null;
        }
        return inputtext;
    }
}
