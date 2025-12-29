using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace TextControlBoxNS.Controls;

internal class InputHandlerControl : TextBox
{
    public delegate void TextEnteredEvent(object sender, TextChangedEventArgs e);
    public event TextEnteredEvent TextEntered;

    private bool _isProgrammaticChange;

    public InputHandlerControl()
    {
        TextChanged += InputHandlerControl_TextChanged;
    }

    private void InputHandlerControl_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (isReadOnly || base.Text.Length == 0)
            return;

        if (!_isProgrammaticChange)
        {
            TextEntered?.Invoke(this, e);
        }
    }

    public new string Text
    {
        get => base.Text;
        set
        {
            _isProgrammaticChange = true;
            base.Text = value;
            _isProgrammaticChange = false;
        }
    }

    public bool isReadOnly { get; set; } = false;

    protected override void OnKeyDown(KeyRoutedEventArgs e)
    {
        //override key down behavior if needed
        return;
    }
}