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

            textbox.LoadLines(Enumerable.Range(0, 1_000_000).Select(x => "Line " + x + " is cool right?"));

            textbox.SelectSyntaxHighlightingById(SyntaxHighlightID.CSharp);

            textbox.UseSpacesInsteadTabs = false;
            textbox.NumberOfSpacesForTab = 4;
            textbox.ShowWhitespaceCharacters = true;
        }
    }
}
