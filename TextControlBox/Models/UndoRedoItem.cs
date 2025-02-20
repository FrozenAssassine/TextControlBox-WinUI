
namespace TextControlBoxNS.Models;

internal class UndoRedoItem
{
    public int StartLine { get; set; }
    public string UndoText { get; set; }
    public string RedoText { get; set; }
    public int UndoCount { get; set; }
    public int RedoCount { get; set; }
    public TextSelection SelectionBefore { get; set; }
    public TextSelection SelectionAfter { get; set; }
    public CursorPosition CursorBefore { get; set; }
    public CursorPosition CursorAfter { get; set; }
    public bool HandleNextItemToo { get; set; }
}
