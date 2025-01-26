using Microsoft.Graphics.Canvas.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        private async void StartBatching()
        {
            if (_isBatching) return;

            _isBatching = true;

            while (_redrawRequests.Values.Contains(true))
            {
                foreach (var canvas in _redrawRequests.Keys)
                {
                    if (_redrawRequests[canvas])
                    {
                        _redrawRequests[canvas] = false;
                        canvas.Invalidate();
                    }
                }

                await Task.Delay(_batchIntervalMs);
            }

            _isBatching = false;
        }
    }
}
