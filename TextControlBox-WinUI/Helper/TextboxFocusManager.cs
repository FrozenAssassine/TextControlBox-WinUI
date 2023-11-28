using TextControlBox_WinUI.Controls;

namespace TextControlBox_WinUI.Helper
{
    internal class TextboxFocusManager
    {
        public bool HasFocus { get; private set; }
        private TextInputManager textInputManager;

        public TextboxFocusManager(TextInputManager textInputManager)
        {
            this.textInputManager = textInputManager;
        }

        public void SetFocus()
        {
            HasFocus = true;
            textInputManager.Focus();
        }

        public void RemoveFocus()
        {
            HasFocus = false;
            textInputManager.UnFocus();
        }
    }
}
