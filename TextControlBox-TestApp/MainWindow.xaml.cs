using Microsoft.UI.Xaml;
using System.Diagnostics;
using System.IO;
using TextControlBoxNS;

namespace TextControlBox_TestApp
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            Debug.WriteLine("started");
            this.InitializeComponent();

            Debug.WriteLine("Second");

            textbox.LoadLines(File.ReadAllLines("C:\\Users\\Juliu\\Desktop\\Cable_Clip_Large.gcode"));

            textbox.SelectSyntaxHighlightingById(SyntaxHighlightID.CSharp);

            textbox.UseSpacesInsteadTabs = true;
            textbox.NumberOfSpacesForTab = 4;
            textbox.Loaded += Textbox_Loaded;
        }

        private void Textbox_Loaded(TextControlBox sender)
        {
            Debug.WriteLine("Third");

            textbox.SetCursorPosition(100, 5);
        }
    }
}
