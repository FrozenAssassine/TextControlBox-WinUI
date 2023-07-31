using TextControlBoxNS.Text;

namespace TextControlBoxNS.Models
{
    internal struct UndoRedoItem
    {
        public int StartLine { get; set; }
        public string UndoText { get; set; }
        public string RedoText { get; set; }
        public int UndoCount { get; set; }
        public int RedoCount { get; set; }
        public TextSelection Selection { get; set; }
    }
}
