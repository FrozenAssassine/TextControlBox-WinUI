using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TextControlBoxNS.Core.Renderer
{
    internal class CanvasBatchRedrawer
    {
        private readonly HashSet<CanvasControl> _redrawRequests = new();
        private readonly DispatcherQueueTimer _timer;

        public CanvasBatchRedrawer(int batchIntervalMs = 16)
        {
            _timer = DispatcherQueue.GetForCurrentThread().CreateTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(batchIntervalMs);
            _timer.Tick += (s, e) =>
            {
                foreach (var canvas in _redrawRequests)
                {
                    canvas.Invalidate();
                }
                _redrawRequests.Clear();
                _timer.Stop();
            };
        }

        public void RequestRedraw(CanvasControl canvas)
        {
            _redrawRequests.Add(canvas);

            if (!_timer.IsRunning)
                _timer.Start();
        }
    }
}