
namespace TextControlBoxNS.Core;

internal class InitializationManager
{
    private EventsManager eventsManager;

    public bool initDone = false;
    public int[] canvasDrawed = [0, 0, 0];
    private bool loaded =false;
    public void Init(EventsManager eventsManager)
    {
        this.eventsManager = eventsManager;
    }

    public void TextboxInitDone() 
    {
        initDone = true;
        CheckInitialized();
    }

    public void CanvasDrawed(int index)
    {
        canvasDrawed[index] = 1;
        CheckInitialized();
    }

    public void CheckInitialized()
    {
        if(!loaded && initDone && canvasDrawed[0] == canvasDrawed[1] && canvasDrawed[1] == canvasDrawed[2] && canvasDrawed[2] == 1)
        {
            loaded = true;
            eventsManager.CallLoaded();
        }
    }
}
