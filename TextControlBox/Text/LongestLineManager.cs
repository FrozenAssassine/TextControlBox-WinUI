using TextControlBoxNS.Helper;

namespace TextControlBoxNS.Text;

internal class LongestLineManager
{
    public int longestLineLength = 0;
    public int longestIndex = 0;
    public bool NeedsRecalculation = true;

    private readonly SelectionManager selManager;
    public LongestLineManager(SelectionManager selManager)
    {
        this.selManager = selManager;
    }

    public void CheckRecalculateLongestLine(string text)
    {
        if (Utils.GetLongestLineLength(text) > longestLineLength)
        {
            NeedsRecalculation = true;
        }
    }

    public void CheckSelection()
    {
        if (selManager.currentTextSelection.IsLineInSelection(longestIndex))
            NeedsRecalculation = true;
    }
}
