using Microsoft.UI.Xaml;
using System;

namespace TextControlBox_WinUI.Helper
{
    internal class PointerManager
    {
        public int LeftPointerClickCount { get; private set; }
        private DispatcherTimer PointerClickTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 200) };

        public PointerManager()
        {

        }


        public void ResetClicks()
        {
            LeftPointerClickCount = 0;
            PointerClickTimer.Stop();
        }
        public void LeftDown()
        {
            LeftPointerClickCount++;

            PointerClickTimer.Start();
            PointerClickTimer.Tick += PointerClickTimer_Tick;
        }

        private void PointerClickTimer_Tick(object sender, object e)
        {
            PointerClickTimer.Stop();
            LeftPointerClickCount = 0;
        }
    }
}
