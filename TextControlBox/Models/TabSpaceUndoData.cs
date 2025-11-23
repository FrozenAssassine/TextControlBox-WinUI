namespace TextControlBoxNS.Models;

internal class TabSpaceUndoData(int SpacesTabsBefore, int DocumentSpacesTabsBefore, int SpacesTabsAfter, int DocumentSpacesTabsAfter)
{
    public int SpacesTabsBefore { get; set; } = SpacesTabsBefore;
    public int DocumentSpacesTabsBefore { get; set; } = DocumentSpacesTabsBefore;
    public int SpacesTabsAfter { get; set; } = SpacesTabsAfter;
    public int DocumentSpacesTabsAfter { get; set; } = DocumentSpacesTabsAfter;
}
