
using System.Diagnostics;
using System.Linq;

namespace TextControlBoxNS.Core;

internal class InitializationManager
{
    private EventsManager eventsManager;

    private bool initDone = false;
    public int[] canvasDrawed = [0, 0, 0];
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
        if(initDone && canvasDrawed[0] == canvasDrawed[1] && canvasDrawed[1] == canvasDrawed[2] && canvasDrawed[2] == 1)
        {
            eventsManager.CallLoaded();
        }
    }
}
