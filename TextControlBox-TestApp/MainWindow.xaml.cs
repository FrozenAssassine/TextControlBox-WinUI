using Microsoft.UI.Xaml;
using TextControlBoxNS;

namespace TextControlBox_TestApp
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();
            textbox.CodeLanguage = TextControlBox.GetCodeLanguageFromId(CodeLanguageId.CSharp);
        }
    }
}
