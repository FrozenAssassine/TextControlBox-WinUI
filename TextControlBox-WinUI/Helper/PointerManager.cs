using Microsoft.UI.Xaml;
using System;

namespace TextControlBox_WinUI.Helper
{
    internal class PointerManager
    {
        public int PointerClickCount { get; private set; }
        private DispatcherTimer PointerClickTimer = new DispatcherTimer { Interval = new TimeSpan(0, 0, 0, 0, 200) };

        public PointerManager()
        {

        }


        public void ResetClicks()
        {
            PointerClickCount = 0;
            PointerClickTimer.Stop();
        }
        public void LeftDown()
        {
            PointerClickCount++;

            PointerClickTimer.Start();
            PointerClickTimer.Tick += PointerClickTimer_Tick;
        }

        private void PointerClickTimer_Tick(object sender, object e)
        {
            PointerClickTimer.Stop();
            PointerClickCount = 0;
        }
    }
}
