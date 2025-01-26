
namespace TextControlBoxNS.Core;

internal class EventsManager
{
    private SearchManager searchManager;
    private CursorManager cursorManager;

    public delegate void ZoomChangedEvent(int zoomFactor);
    public event ZoomChangedEvent ZoomChanged;

    public delegate void TextChangedEvent();
    public event TextChangedEvent TextChanged;
    
    public delegate void SelectionChangedEvent(SelectionChangedEventHandler args);
    public event SelectionChangedEvent SelectionChanged;

    public delegate void GotFocusEvent();
    public event GotFocusEvent GotFocus;

    public delegate void LostFocusEvent();
    public event LostFocusEvent LostFocus;

    public void Init(SearchManager searchManager, CursorManager cursorManager)
    {
        this.searchManager = searchManager;
        this.cursorManager = cursorManager;
    }

    public void CallTextChanged()
    {
        if (TextChanged == null)
            return;

        if (searchManager.IsSearchOpen)
            searchManager.UpdateSearchLines();

        TextChanged?.Invoke();
    }

    public void CallSelectionChanged()
    {
        if (SelectionChanged == null)
            return;

        SelectionChangedEventHandler args = new SelectionChangedEventHandler
        {
            CharacterPositionInLine = cursorManager.GetCurPosInLine() + 1,
            LineNumber = cursorManager.LineNumber,
        };
        SelectionChanged.Invoke(args);
    }

    public void CallZoomChanged(int zoomFactor)
    {
        ZoomChanged?.Invoke(zoomFactor);
    }

    public void CallGotFocus()
    {
        GotFocus?.Invoke();
    }
    public void CallLostFocus()
    {
        LostFocus?.Invoke();
    }
}
