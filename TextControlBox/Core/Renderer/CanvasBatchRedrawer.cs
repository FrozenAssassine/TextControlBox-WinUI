using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.UI.Dispatching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using TextControlBoxNS.Languages;

namespace TextControlBoxNS.Core.Renderer
{
    internal class CanvasBatchRedrawer
    {
        private readonly Dictionary<CanvasControl, bool> _redrawRequests = new();
        private bool _isBatching;
        private readonly int _batchIntervalMs;

        public CanvasBatchRedrawer(int batchIntervalMs = 16)
        {
            _batchIntervalMs = batchIntervalMs;
        }

        public void RegisterCanvas(CanvasControl canvas)
        {
            if (!_redrawRequests.ContainsKey(canvas))
            {
                _redrawRequests[canvas] = false;
            }
        }

        public void RequestRedraw(CanvasControl canvas)
        {
            if (_redrawRequests.ContainsKey(canvas))
            {
                _redrawRequests[canvas] = true;
                StartBatching();
            }
        }

        private DispatcherQueueTimer? _batchTimer;

        private void StartBatching()
        {
            if (_batchTimer != null) return; // Prevent multiple timers

            _batchTimer = DispatcherQueue.GetForCurrentThread().CreateTimer();
            _batchTimer.Interval = TimeSpan.FromMilliseconds(_batchIntervalMs);
            _batchTimer.Tick += (s, e) =>
            {
                foreach (var canvas in _redrawRequests.Keys.ToList())
                {
                    if (_redrawRequests[canvas])
                    {
                        _redrawRequests[canvas] = false;
                        canvas.Invalidate();
                    }
                }

                if (!_redrawRequests.Values.Contains(true))
                {
                    _batchTimer?.Stop();
                    _batchTimer = null;
                }
            };

            _batchTimer.Start();
        }
    }
}