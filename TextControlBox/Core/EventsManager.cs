
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

    public delegate void LoadedEvent();
    public event LoadedEvent Loaded;

    public delegate void TextLoadedEvent();
    public event TextLoadedEvent TextLoaded;

    public delegate void LinkClickedEvent(string url);
    public event LinkClickedEvent LinkClicked;

    public delegate void TabsSpacesChangedEvent(bool spacesInsteadTabs, int spaces);
    public event TabsSpacesChangedEvent TabsSpacesChanged;

    public delegate void LineEndingChangedEvent(LineEnding lineEnding);
    public event LineEndingChangedEvent LineEndingChanged;


    private SelectionChangedEventHandler args = new SelectionChangedEventHandler();


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

        args.CharacterPositionInLine = cursorManager.GetCurPosInLine() + 1;
        args.LineNumber = cursorManager.LineNumber;

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

    public void CallLoaded()
    {
        Loaded?.Invoke();
    }
    public void CallTextLoaded()
    {
        TextLoaded?.Invoke();
    }

    public void CallLinkClicked(string url)
    {
        LinkClicked?.Invoke(url);
    }
  
    public void CallTabsSpacesChanged(bool spacesInsteadTabs, int spaces)
    {
        TabsSpacesChanged?.Invoke(spacesInsteadTabs, spaces);
    }

    public void CallLineEndingChanged(LineEnding lineEnding)
    {
        this.LineEndingChanged?.Invoke(lineEnding);
    }
}
