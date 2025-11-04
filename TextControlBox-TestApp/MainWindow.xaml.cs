using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
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

            textbox.SetText(new string('\t', 10));

            //textbox.LoadLines(File.ReadAllLines("C:\\Users\\Juliu\\Desktop\\Cable_Clip_Large.gcode"));

            textbox.SelectSyntaxHighlightingById(SyntaxHighlightID.CSharp);

            textbox.UseSpacesInsteadTabs = false;
            textbox.NumberOfSpacesForTab = 4;
            textbox.ShowWhitespaceCharacters = true;

            File.WriteAllText("C:\\Users\\Juliu\\Desktop\\mixedLineEndings.txt", "Hello World\nThis is file content \r\n And this is another line \r");

            //ActionGrouping();
        }

        void ActionGrouping()
        {

            textbox.BeginActionGroup();

            textbox.DeleteLine(3);
            textbox.DeleteLine(0);
            textbox.AddLine(3, "New");
            textbox.SetLineText(1, "Edit");

            for (int i = 0; i < 10; i++)
            {
                textbox.AddLines(4, ["Hello", "Baum", "Nudel", "Kuchen"]);
            }

            for (int i = 5; i < 20; i++)
            {
                textbox.DeleteLine(i);
            }

            textbox.EndActionGroup();
        }

    }
}
