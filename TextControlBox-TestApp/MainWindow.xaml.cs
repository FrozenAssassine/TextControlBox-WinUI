using Microsoft.UI.Xaml;
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
            for(int i = 0; i<1_000_000; i++)
            {
                sb.AppendLine("This is the best line of the textbox " + i);
            }

            textbox.SetText(sb.ToString());

            textbox.HighlightLanguage = TextControlBox.GetCodeLanguageFromId(SyntaxHighlightID.CSharp);

            textbox.GetLinesText(0, textbox.NumberOfLines);

            textbox.SyntaxHighlighting = false;
        }
    }
}
