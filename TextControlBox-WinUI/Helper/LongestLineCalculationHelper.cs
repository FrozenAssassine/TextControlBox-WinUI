using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using TextControlBox.Helper;
using TextControlBox.Renderer;
using Windows.Foundation;

namespace TextControlBox_WinUI.Helper
{
    internal class LongestLineCalculationHelper
    {
        private bool _NeedsRecalculate = false;
        private TextManager textManager;
        private TextRenderer textRenderer;
        public int LongestLineIndex = 0;
        public int LongestLineLength = 0;
        
        public LongestLineCalculationHelper(TextManager textManager, TextRenderer textRenderer)
        {
            this.textManager = textManager;
            this.textRenderer = textRenderer;
        }

        public bool EqualsIndex(int index)
        {
            return LongestLineIndex == index;
        }

        public void ChangeIndex(int newIndex)
        {
            LongestLineIndex = newIndex;
        }

        public void CheckNeedsRecalculate(string text)
        {
            if (Utils.GetLongestLineLength(text) > LongestLineLength)
                NeedsRecalculate();
        }

        public void NeedsRecalculate(bool needsRecalculate = true)
        {
            _NeedsRecalculate = needsRecalculate;
        }


        public int GetLongestLineIndex()
        {
            if (!_NeedsRecalculate)
                return LongestLineIndex;

            int OldLenght = 0;
            for (int i = 0; i < textManager.Lines.Count; i++)
            {
                var lenght = textManager.Lines[i].Length;
                if (lenght > OldLenght)
                {
                    LongestLineIndex = i;
                    OldLenght = lenght;
                }
            }
            _NeedsRecalculate = false;
            return LongestLineIndex; 
        }

        public int GetLongestLineLength()
        {
            if (_NeedsRecalculate)
                GetLongestLineIndex();

            return LongestLineLength = textManager.Lines[LongestLineIndex].Length;
        }

        public Size GetActualLongestLineLength()
        {
            if(_NeedsRecalculate)
                GetLongestLineIndex();

            var line = textManager.Lines[LongestLineIndex];
            return Utils.MeasureLineLenght(CanvasDevice.GetSharedDevice(), line, textRenderer.textFormat);
        }
    }
}
