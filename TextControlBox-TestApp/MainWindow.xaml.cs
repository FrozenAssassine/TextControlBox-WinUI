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

            textbox.LoadLines(Enumerable.Range(0, 5).Select(x => "Line " + x + " is cool right?"));

            //textbox.LoadLines(File.ReadAllLines("C:\\Users\\Juliu\\Desktop\\Cable_Clip_Large.gcode"));

            textbox.SelectSyntaxHighlightingById(SyntaxHighlightID.CSharp);

            textbox.UseSpacesInsteadTabs = true;
            textbox.NumberOfSpacesForTab = 4;
            textbox.Loaded += Textbox_Loaded;

            textbox.DeleteLines(0, 5);

            //ActionGrouping();
        }

        private void Textbox_Loaded(TextControlBox sender)
        {
            textbox.SetCursorPosition(100, 5);
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
