using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TextControlBoxNS.Core.Renderer
{
    internal class CanvasBatchRedrawer
    {
        private readonly Dictionary<CanvasControl, CanvasRedrawState> _redrawRequests = new();
        private readonly int _batchIntervalMs;

        private class CanvasRedrawState
        {
            public bool NeedsRedraw;
            public DispatcherQueueTimer? Timer;
        }

        public CanvasBatchRedrawer(int batchIntervalMs = 16)
        {
            _batchIntervalMs = batchIntervalMs;
        }

        public void RegisterCanvas(CanvasControl canvas)
        {
            if (!_redrawRequests.ContainsKey(canvas))
            {
                _redrawRequests[canvas] = new CanvasRedrawState();
            }
        }

        public void RequestRedraw(CanvasControl canvas)
        {
            if (!_redrawRequests.ContainsKey(canvas)) return;

            var state = _redrawRequests[canvas];
            state.NeedsRedraw = true;

            if (state.Timer == null)
            {
                state.Timer = DispatcherQueue.GetForCurrentThread().CreateTimer();
                state.Timer.Interval = TimeSpan.FromMilliseconds(_batchIntervalMs);
                state.Timer.Tick += (s, e) =>
                {
                    if (state.NeedsRedraw)
                    {
                        state.NeedsRedraw = false;
                        canvas.Invalidate();
                    }
                    else
                    {
                        state.Timer?.Stop();
                        state.Timer = null;
                    }
                };
                state.Timer.Start();
            }
        }
    }
}