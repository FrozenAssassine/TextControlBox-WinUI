using Microsoft.UI.Input;
using Microsoft.UI.Xaml;
using TextControlBoxNS.Controls;

namespace TextControlBoxNS.Core;

internal class FocusManager
{
    public bool HasFocus = false;

    private EventsManager eventsManager;
    private CanvasUpdateManager canvasUpdateManager;
    private InputHandlerControl inputHandler;
    private CoreTextControlBox coreTextbox;
    public void Init(CoreTextControlBox coreTextbox, CanvasUpdateManager canvasUpdateManager, InputHandlerControl inputHandler, EventsManager eventsManager)
    {
        this.inputHandler = inputHandler;
        this.canvasUpdateManager = canvasUpdateManager;
        this.eventsManager = eventsManager;
        this.coreTextbox = coreTextbox;
    }

    public void SetFocus()
    {
        if (!HasFocus)
            eventsManager.CallGotFocus();
        HasFocus = true;

        canvasUpdateManager.UpdateCursor();
        inputHandler.Focus(FocusState.Programmatic);
        coreTextbox.ChangeCursor(InputSystemCursorShape.IBeam);
    }
    public void RemoveFocus()
    {
        if (HasFocus)
            eventsManager.CallLostFocus();
        canvasUpdateManager.UpdateCursor();

        HasFocus = false;
        coreTextbox.ChangeCursor(InputSystemCursorShape.Arrow);
    }

}
