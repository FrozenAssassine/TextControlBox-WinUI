using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;

namespace TextControlBoxNS.Controls;

internal class InputHandlerControl : TextBox
{
    public delegate void TextEnteredEvent(object sender, TextChangedEventArgs e);
    public event TextEnteredEvent TextEntered;

    public delegate void TextCompositionStartedEvent(object sender, TextCompositionStartedEventArgs e);
    public event TextCompositionStartedEvent CompositionStarted;

    public delegate void TextCompositionChangedEvent(object sender, TextCompositionChangedEventArgs e);
    public event TextCompositionChangedEvent CompositionChanged;

    private bool _isProgrammaticChange;
    private bool _isComposing;

    public bool IsComposing => _isComposing;

    public InputHandlerControl()
    {
        DesiredCandidateWindowAlignment = CandidateWindowAlignment.BottomEdge;
        TextChanged += InputHandlerControl_TextChanged;
        TextCompositionStarted += InputHandlerControl_TextCompositionStarted;
        TextCompositionChanged += InputHandlerControl_TextCompositionChanged;
        TextCompositionEnded += InputHandlerControl_TextCompositionEnded;
    }

    private void InputHandlerControl_TextCompositionStarted(TextBox sender, TextCompositionStartedEventArgs args)
    {
        _isComposing = true;
        CompositionStarted?.Invoke(this, args);
    }

    private void InputHandlerControl_TextCompositionChanged(TextBox sender, TextCompositionChangedEventArgs args)
    {
        CompositionChanged?.Invoke(this, args);
    }

    private void InputHandlerControl_TextCompositionEnded(TextBox sender, TextCompositionEndedEventArgs args)
    {
        _isComposing = false;

        if (isReadOnly || base.Text.Length == 0)
        {
            if (base.Text.Length > 0)
            {
                Text = "";
            }
            return;
        }

        if (!_isProgrammaticChange)
        {
            TextEntered?.Invoke(this, null);
        }
    }

    private void InputHandlerControl_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (isReadOnly || base.Text.Length == 0)
            return;

        if (!_isProgrammaticChange && !_isComposing)
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

    internal void ResetComposition()
    {
        _isComposing = false;
    }

    // Helper methods for unit testing / programmatic simulation of composition
    internal void SimulateUserInput(string text)
    {
        if (isReadOnly || string.IsNullOrEmpty(text))
            return;

        base.Text = text;
        if (!_isProgrammaticChange && !_isComposing)
        {
            TextEntered?.Invoke(this, null);
        }
    }

    internal void SimulateCompositionStart()
    {
        _isComposing = true;
    }

    internal void SimulateCompositionEnd(string committedText)
    {
        _isComposing = false;
        if (!string.IsNullOrEmpty(committedText))
        {
            base.Text = committedText;
            if (!isReadOnly && !_isProgrammaticChange)
            {
                TextEntered?.Invoke(this, null);
            }
        }
        else
        {
            Text = "";
        }
    }
}