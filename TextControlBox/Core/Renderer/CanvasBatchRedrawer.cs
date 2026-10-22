using Microsoft.Graphics.Canvas.UI.Xaml;

namespace TextControlBoxNS.Core.Renderer
{
    internal class CanvasBatchRedrawer
    {
        // Invalidate right away: CanvasControl already merges all invalidations until its next frame into one
        // Draw. The previous 16 ms DispatcherTimer added a whole timer tick (often 30 ms) to every keystroke and,
        // because timer messages only run when no input is waiting, fast typing could hold back the redraw
        // until the typing paused, so words appeared late.
        public void RequestRedraw(CanvasControl canvas)
        {
            canvas.Invalidate();
        }
    }
}
