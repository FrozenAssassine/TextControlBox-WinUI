using Collections.Pooled;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Helper;

namespace TextControlBoxNS.Core;

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

    //Get the longest line in the textbox
    private int GetLongestLineIndex(PooledList<string> totalLines)
    {
        int longestIndex = 0;
        int oldLenght = 0;
        for (int i = 0; i < totalLines.Count; i++)
        {
            var lenght = totalLines[i].Length;
            if (lenght > oldLenght)
            {
                longestIndex = i;
                oldLenght = lenght;
            }
        }
        return longestIndex;
    }
    private int GetLongestLineLength(string text)
    {
        var splitted = text.Split("\n");
        int oldLenght = 0;
        for (int i = 0; i < splitted.Length; i++)
        {
            var lenght = splitted[i].Length;
            if (lenght > oldLenght)
            {
                oldLenght = lenght;
            }
        }
        return oldLenght;
    }

    public void CheckRecalculateLongestLine(string text)
    {
        if (GetLongestLineLength(text) > longestLineLength)
        {
            needsRecalculation = true;
        }
    }

    public void CheckRecalculateLongestLine()
    {
        if (needsRecalculation)
        {
            needsRecalculation = false;
            longestIndex= GetLongestLineIndex(textManager.totalLines);
        }

    }

    public void CheckSelection()
    {
        if (selManager.currentTextSelection.IsLineInSelection(longestIndex))
            needsRecalculation = true;
    }
}
