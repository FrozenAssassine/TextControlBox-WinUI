using Microsoft.UI.Dispatching;
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
    // A composition ended, but its text was not handed over yet. See InputHandlerControl_TextCompositionEnded.
    private bool _commitPending;

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
        //the previous composition is finished for sure, when the next one starts
        if (_commitPending)
            CommitPendingText();

        _isComposing = true;
        CompositionStarted?.Invoke(this, args);
    }

    private void InputHandlerControl_TextCompositionChanged(TextBox sender, TextCompositionChangedEventArgs args)
    {
        CompositionChanged?.Invoke(this, args);
    }

    // The text can not be taken (and cleared) right here: when a candidate gets picked from the IME window,
    // the composition ends first and the IME replaces the typed characters with the candidate afterwards.
    // Reading the text at this point returned the raw input ("あｓｄ") and clearing it threw the candidate away.
    // So the text is handed over by whatever comes next: the text change of the candidate, the start of the
    // next composition or, when nothing else happens, the queued callback.
    private void InputHandlerControl_TextCompositionEnded(TextBox sender, TextCompositionEndedEventArgs args)
    {
        _isComposing = false;
        _commitPending = true;

        if (DispatcherQueue == null || !DispatcherQueue.TryEnqueue(DispatcherQueuePriority.Low, CommitPendingTextIfIdle))
            CommitPendingText();
    }

    private void CommitPendingTextIfIdle()
    {
        if (_commitPending && !_isComposing)
            CommitPendingText();
    }

    private void CommitPendingText()
    {
        _commitPending = false;

        if (base.Text.Length == 0)
            return;

        if (isReadOnly)
        {
            Text = "";
            return;
        }

        if (!_isProgrammaticChange)
            TextEntered?.Invoke(this, null);
    }

    private void InputHandlerControl_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (isReadOnly || base.Text.Length == 0)
            return;

        if (!_isProgrammaticChange && !_isComposing)
        {
            _commitPending = false;
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
