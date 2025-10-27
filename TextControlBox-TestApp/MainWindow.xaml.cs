using Microsoft.UI.Xaml;
using System.IO;
using System.Linq;
using TextControlBoxNS;

namespace TextControlBox_TestApp
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            textbox.LoadLines(Enumerable.Range(0, 1_000_000).Select(x => "Line " + x + " is cool right?"));

            //textbox.LoadLines(File.ReadAllLines("C:\\Users\\Juliu\\Desktop\\Cable_Clip_Large.gcode"));

            textbox.SelectSyntaxHighlightingById(SyntaxHighlightID.CSharp);

            textbox.UseSpacesInsteadTabs = false;
            textbox.NumberOfSpacesForTab = 4;
            textbox.ShowWhitespaceCharacters = true;
            textbox.Loaded += Textbox_Loaded;
        }

        private void Textbox_Loaded(TextControlBox sender)
        {
            textbox.SetCursorPosition(100, 5);
        }
    }
}
