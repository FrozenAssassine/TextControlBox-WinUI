using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextControlBoxNS.Helper;
using TextControlBoxNS.Renderer;

namespace TextControlBoxNS.Text
{
    internal class ZoomManager
    {
        public float ZoomedFontSize = 0;
        public int _ZoomFactor = 100; //%
        public int OldZoomFactor = 0;

        public delegate void ZoomChangedEvent(int zoomFactor);
        public event ZoomChangedEvent ZoomChanged;

        private readonly TextManager textManager;
        private readonly TextRenderer textRenderer;
        public ZoomManager(TextManager textManager, TextRenderer textRenderer)
        {
            this.textManager = textManager;
        }

        private void UpdateZoom()
        {
            ZoomedFontSize = Math.Clamp(textManager._FontSize * (float)_ZoomFactor / 100, textManager.MinFontSize, textManager.MaxFontsize);
            _ZoomFactor = Math.Clamp(_ZoomFactor, 4, 400);

            if (_ZoomFactor != OldZoomFactor)
            {
                textRenderer.NeedsUpdateTextLayout = true;
                OldZoomFactor = _ZoomFactor;
                ZoomChanged?.Invoke(_ZoomFactor);
            }

            textRenderer.NeedsTextFormatUpdate = true;

            ScrollLineIntoView(CursorPosition.LineNumber);
            lineNumberRenderer.NeedsUpdateLineNumbers();
            canvasHelper.UpdateAll();
        }
    }
}
