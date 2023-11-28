using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;

namespace TextControlBox_WinUI.Controls
{
    public sealed partial class TextInputManager : UserControl
    {

        public delegate void TextChangedEvent(string text);
        public event TextChangedEvent TextChanged;
        public TextInputManager()
        {
            this.InitializeComponent();
        }

        public void Focus()
        {
            inputTextbox.Focus(Microsoft.UI.Xaml.FocusState.Programmatic);
            Debug.WriteLine("Got Focus");
        }

        public void UnFocus()
        {
            Debug.WriteLine("Lost Focus");
        }

        private void InputTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (inputTextbox.Text.Length == 0)
                return;

            TextChanged?.Invoke(inputTextbox.Text);
            inputTextbox.Text = "";
        }
    }
}
