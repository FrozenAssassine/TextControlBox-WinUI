using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using Windows.Storage.Pickers;

namespace TextControlBox_TestApp
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            textbox.LoadLines(TestLines());
        }

        private IEnumerable<string> TestLines()
        {
            for(int i = 0; i<100; i++)
            {
                yield return "Line " + i;
            }
        }

        private async void PickAFileButton_Click(object sender, RoutedEventArgs e)
        {
            // Create a file picker
            var openPicker = new Windows.Storage.Pickers.FileOpenPicker();

            // Retrieve the window handle (HWND) of the current WinUI 3 window.
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);

            // Initialize the file picker with the window handle (HWND).
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            // Set options for your file picker
            openPicker.ViewMode = PickerViewMode.Thumbnail;
            openPicker.FileTypeFilter.Add("*");

            // Open the picker for the user to pick a file
            var file = await openPicker.PickSingleFileAsync();
            //textbox.LoadLines(File.ReadLines(file.Path));
        }
    }
}
