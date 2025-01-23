using Microsoft.UI.Xaml;
using System.Diagnostics;
using System.Threading.Tasks;
using TextControlBoxNS.Core.Renderer;
using TextControlBoxNS.Helper;
using TextControlBoxNS.Models;

namespace TextControlBoxNS.Core
{
    internal class EventsManager
    {
        private SearchManager searchManager;
        private CursorManager cursorManager;
        private SelectionRenderer selectionRenderer;
        private SelectionManager selectionManager;

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

        public void Init(SearchManager searchManager, SelectionManager selectionManager, CursorManager cursorManager, SelectionRenderer selectionRenderer)
        {
            this.searchManager = searchManager;
            this.cursorManager = cursorManager;
            this.selectionRenderer = selectionRenderer;
            this.selectionManager = selectionManager;
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
}
