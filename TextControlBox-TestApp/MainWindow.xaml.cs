using Microsoft.UI.Xaml;
using System.IO;
using System.Text;
using TextControlBoxNS;

namespace TextControlBox_TestApp
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            StringBuilder sb = new StringBuilder();
            for(int i = 0; i<10; i++)
            {
                sb.AppendLine("This is the best line of the textbox " + i);
            }

            textbox.SetText(sb.ToString());

            textbox.SelectSyntaxHighlightingById(SyntaxHighlightID.CSharp);

            textbox.UseSpacesInsteadTabs = true;
            textbox.NumberOfSpacesForTab = 4;
            textbox.Loaded += Textbox_Loaded;
        }

        private void Textbox_Loaded(object sender, RoutedEventArgs e)
        {
            textbox.LoadLines(File.ReadAllLines("C:\\Users\\Juliu\\Desktop\\Cable_Clip_Large.gcode"));

            //textbox.SetCursorPosition(10000, 40);
            //textbox.GetLinesText(0, textbox.NumberOfLines);

            //textbox.EnableSyntaxHighlighting = true;
        }
    }
}
