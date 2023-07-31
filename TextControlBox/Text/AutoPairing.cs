using System.Linq;

namespace TextControlBoxNS.Text
{
    internal class AutoPairing
    {
        public static (string text, int length) AutoPair(TextControlBox textbox, string inputtext)
        {
            if (!textbox.DoAutoPairing || inputtext.Length != 1 || textbox.CodeLanguage == null || textbox.CodeLanguage.AutoPairingPair == null)
                return (inputtext, inputtext.Length);

            var res = textbox.CodeLanguage.AutoPairingPair.Where(x => x.Matches(inputtext));
            if (res.Count() == 0)
                return (inputtext, inputtext.Length);

            if (res.ElementAt(0) is AutoPairingPair pair)
                return (inputtext + pair.Pair, inputtext.Length);
            return (inputtext, inputtext.Length);
        }

        public static string AutoPairSelection(TextControlBox textbox, string inputtext)
        {
            if (!textbox.DoAutoPairing || inputtext.Length != 1 || textbox.CodeLanguage == null || textbox.CodeLanguage.AutoPairingPair == null)
                return inputtext;

            var res = textbox.CodeLanguage.AutoPairingPair.Where(x => x.Value.Equals(inputtext));
            if (res.Count() == 0)
                return inputtext;

            if (res.ElementAt(0) is AutoPairingPair pair)
            {
                textbox.SurroundSelectionWith(inputtext, pair.Pair);
                return null;
            }
            return inputtext;
        }
    }
}
