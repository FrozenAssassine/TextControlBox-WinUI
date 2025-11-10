using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TextControlBoxNS;

namespace TextControlBox_TestApp
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            //textbox.LoadLines(Enumerable.Range(0, 1_000_000).Select(x => "Line " + x + " is cool right?"));

            textbox.SelectSyntaxHighlightingById(SyntaxHighlightID.Markdown);

            textbox.UseSpacesInsteadTabs = false;
            textbox.NumberOfSpacesForTab = 4;
            textbox.ShowWhitespaceCharacters = true;

            SetWindowTheme(this, ElementTheme.Dark);

            textbox.LinkClicked += Textbox_LinkClicked;
            textbox.DispatcherQueue.TryEnqueue(() =>
            {
                textbox.RequestedTheme = ElementTheme.Dark;
            });
        }

        private void Textbox_LinkClicked(string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        public static void SetWindowTheme(Window window, ElementTheme theme)
        {
            if (window.Content is FrameworkElement frame)
                frame.RequestedTheme = theme;
        }
    }
}
