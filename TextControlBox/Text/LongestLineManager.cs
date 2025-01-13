using TextControlBoxNS.Helper;

namespace TextControlBoxNS.Text;

internal class LongestLineManager
{
    public int longestLineLength = 0;
    public int longestIndex = 0;
    public bool needsRecalculation = true;

    private SelectionManager selManager;
    private TextManager textManager;
    public void Init(SelectionManager selManager, TextManager textManager)
    {
        this.selManager = selManager;
        this.textManager = textManager;
    }

    public void CheckRecalculateLongestLine(string text)
    {
        if (Utils.GetLongestLineLength(text) > longestLineLength)
        {
            needsRecalculation = true;
        }
    }

    public void CheckRecalculateLongestLine()
    {
        if (needsRecalculation)
        {
            needsRecalculation = false;
            longestIndex= Utils.GetLongestLineIndex(textManager.totalLines);
        }

    }

    public void CheckSelection()
    {
        if (selManager.currentTextSelection.IsLineInSelection(longestIndex))
            needsRecalculation = true;
    }
}
