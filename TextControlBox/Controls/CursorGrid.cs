using Microsoft.UI.Input;
using Microsoft.UI.Xaml.Controls;

namespace TextControlBoxNS.Controls;

internal class CursorGrid : Grid
{
    public CursorGrid()
    {
        this.ProtectedCursor = InputSystemCursor.Create(InputSystemCursorShape.Arrow);
    }
}
