using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextControlBox.Helper;
using TextControlBox.Renderer;
using TextControlBox.Text;

namespace TextControlBox_WinUI.Helper
{
    internal class SafeHandler
    {
        public static void Safe_LoadLines(CanvasUpdateHelper canvasUpdater, TextControlBox.TextControlBox textbox, TextManager textManager, IEnumerable<string> lines, LineEnding lineEnding = LineEnding.CRLF, bool HandleException = true)
        {
            try
            {
                //selectionrenderer.ClearSelection();
                //undoRedo.ClearAll();

                textManager.ClearAndLoadLines(lines);

                //textbox.LineEnding = lineEnding;

                canvasUpdater.UpdateAll();
            }
            catch (OutOfMemoryException)
            {
                if (HandleException)
                {
                    //CleanUp();
                    Safe_LoadLines(canvasUpdater, textbox, textManager, lines, lineEnding, false);
                    return;
                }
                throw new OutOfMemoryException();
            }
        }
    }
}
