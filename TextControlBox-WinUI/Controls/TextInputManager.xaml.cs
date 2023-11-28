using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System.Diagnostics;

namespace TextControlBox_WinUI.Controls
{
    public sealed partial class TextInputManager : UserControl
    {

        public delegate void TextChangedEvent(string text);
        public event TextChangedEvent TextChanged;

        public delegate void GotFocusEvent();
        public new event GotFocusEvent GotFocus;

        public delegate void LostFocusEvent();
        public new event LostFocusEvent LostFocus;

        public new delegate void KeyDownEvent(object sender, KeyRoutedEventArgs e);
        public new event KeyDownEvent KeyDown;
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

        private void inputTextbox_LostFocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            LostFocus?.Invoke();
        }

        private void inputTextbox_GotFocus(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            GotFocus?.Invoke();
        }

        private void inputTextbox_PreviewKeyDown(object sender, Microsoft.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            KeyDown?.Invoke(sender, e);
        }
    }
}
