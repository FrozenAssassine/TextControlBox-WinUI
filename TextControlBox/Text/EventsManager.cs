using TextControlBoxNS.Helper;
using TextControlBoxNS.Renderer;

namespace TextControlBoxNS.Text
{
    internal class EventsManager
    {
        private readonly SearchManager searchManager;
        private readonly CursorManager cursorManager;
        private readonly SelectionRenderer selectionRenderer;
        private readonly TextManager textManager;
        private readonly TextControlBox textbox;
        private readonly SelectionManager selectionManager;

        public delegate void ZoomChangedEvent(int zoomFactor);
        public event ZoomChangedEvent ZoomChanged;

        public delegate void TextChangedEvent();
        public event TextChangedEvent TextChanged;
        
        public delegate void SelectionChangedEvent(SelectionChangedEventHandler args);
        public event SelectionChangedEvent SelectionChanged;

        public EventsManager(SearchManager searchManager, SelectionManager selectionManager, CursorManager cursorManager, SelectionRenderer selectionRenderer, TextManager textManager)
        {
            this.searchManager = searchManager;
            this.cursorManager = cursorManager;
            this.selectionRenderer = selectionRenderer;
            this.textManager = textManager;
        }

        public void CallTextChanged()
        {
            if (searchManager.IsSearchOpen)
                searchManager.UpdateSearchLines();

            TextChanged?.Invoke();
        }

        public void CallCursorChanged()
        {
            SelectionChangedEventHandler args = new SelectionChangedEventHandler
            {
                CharacterPositionInLine = cursorManager.GetCurPosInLine() + 1,
                LineNumber = cursorManager.LineNumber,
            };
            if (selectionRenderer.SelectionStartPosition != null && selectionRenderer.SelectionEndPosition != null)
            {
                var sel = selectionManager.GetIndexOfSelection(new TextSelection(selectionRenderer.SelectionStartPosition, selectionRenderer.SelectionEndPosition));
                args.SelectionLength = sel.Length;
                args.SelectionStartIndex = sel.Index;
            }
            else
            {
                args.SelectionLength = 0;
                args.SelectionStartIndex = cursorManager.CursorPositionToIndex(new CursorPosition { CharacterPosition = cursorManager.GetCurPosInLine(), LineNumber = cursorManager.LineNumber });
            }
            SelectionChanged?.Invoke(args);
        }

        public void CallZoomChanged(int zoomFactor)
        {
            ZoomChanged?.Invoke(zoomFactor);
        }
    }
}
