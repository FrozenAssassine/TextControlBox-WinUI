using System;
using System.Collections.Generic;
using TextControlBox.Extensions;
using TextControlBox.Helper;
using TextControlBox_WinUI.Helper;
using TextControlBox_WinUI.Models;

namespace TextControlBox.Text
{
    internal class UndoRedoManager
    {
        private Stack<UndoRedoItem> UndoStack = new Stack<UndoRedoItem>();
        private Stack<UndoRedoItem> RedoStack = new Stack<UndoRedoItem>();

        private bool HasRedone = false;

        private readonly TextManager textManager;
        private readonly TextControlBoxProperties textBoxProps;

        public UndoRedoManager(TextManager textManager, TextControlBoxProperties textBoxProps)
        {
            this.textManager = textManager;
            this.textBoxProps = textBoxProps;
        }

        private void RecordRedo(UndoRedoItem item)
        {
            RedoStack.Push(item);
        }
        private void RecordUndo(UndoRedoItem item)
        {
            UndoStack.Push(item);
        }

        private void AddUndoItem(TextSelection selection, int startLine, string undoText, string redoText, int undoCount, int redoCount)
        {
            UndoStack.Push(new UndoRedoItem
            {
                RedoText = redoText,
                UndoText = undoText,
                Selection = selection,
                StartLine = startLine,
                UndoCount = undoCount,
                RedoCount = redoCount,
            });
        }

        private void RecordSingleLine(Action action, int startline)
        {
            var lineBefore = textManager.Lines.GetLineText(startline);
            action.Invoke();
            var lineAfter = textManager.Lines.GetLineText(startline);
            AddUndoItem(null, startline, lineBefore, lineAfter, 1, 1);
        }

        public void RecordUndoAction(Action action, int startline, int undocount, int redoCount, CursorPosition cursorposition = null)
        {
            if (undocount == redoCount && redoCount == 1)
            {
                RecordSingleLine(action, startline);
                return;
            }

            var linesBefore = textManager.Lines.GetLines(startline, undocount).GetString(textBoxProps.NewLineCharacter);
            action.Invoke();
            var linesAfter = textManager.Lines.GetLines(startline, redoCount).GetString(textBoxProps.NewLineCharacter);

            AddUndoItem(cursorposition == null ? null : new TextSelection(new CursorPosition(cursorposition), null), startline, linesBefore, linesAfter, undocount, redoCount);
        }
        public void RecordUndoAction(Action action, TextSelection selection, int NumberOfAddedLines)
        {
            var orderedSel = Selection.OrderTextSelection(selection);
            if (orderedSel.StartPosition.LineNumber == orderedSel.EndPosition.LineNumber && orderedSel.StartPosition.LineNumber == 1)
            {
                RecordSingleLine(action, orderedSel.StartPosition.LineNumber);
                return;
            }

            int NumberOfRemovedLines = orderedSel.EndPosition.LineNumber - orderedSel.StartPosition.LineNumber + 1;

            if (NumberOfAddedLines == 0 && !Selection.WholeLinesAreSelected(selection, textManager) ||
                orderedSel.StartPosition.LineNumber == orderedSel.EndPosition.LineNumber && orderedSel.Length == textManager.Lines.GetLineLength(orderedSel.StartPosition.LineNumber))
                NumberOfAddedLines += 1;

            var linesBefore = textManager.Lines.GetLines(orderedSel.StartPosition.LineNumber, NumberOfRemovedLines).GetString(textBoxProps.NewLineCharacter);
            action.Invoke();
            var linesAfter = textManager.Lines.GetLines(orderedSel.StartPosition.LineNumber, NumberOfAddedLines).GetString(textBoxProps.NewLineCharacter);

            AddUndoItem(
                selection,
                orderedSel.StartPosition.LineNumber,
                linesBefore,
                linesAfter,
                NumberOfRemovedLines,
                NumberOfAddedLines
                );
        }

        /// <summary>
        /// Excecutes the undo and applys the changes to the text
        /// </summary>
        /// <param name="NewLineCharacter">The current line-ending character either CR, LF or CRLF</param>
        /// <returns>A class containing the start and end-position of the selection</returns>
        public TextSelection Undo(StringManager stringManager)
        {
            if (UndoStack.Count < 1)
                return null;

            if (HasRedone)
            {
                HasRedone = false;
                while (RedoStack.Count > 0)
                {
                    var redoItem = RedoStack.Pop();
                    redoItem.UndoText = redoItem.RedoText = null;
                }
            }

            UndoRedoItem item = UndoStack.Pop();
            RecordRedo(item);

            //Faster for singleline
            if (item.UndoCount == 1 && item.RedoCount == 1)
            {
                textManager.Lines.SetLineText(item.StartLine, stringManager.CleanUpString(item.UndoText));
            }
            else
            {
                textManager.Lines.Safe_RemoveRange(item.StartLine, item.RedoCount);
                if (item.UndoCount > 0)
                    textManager.Lines.InsertOrAddRange(ListHelper.GetLinesFromString(stringManager.CleanUpString(item.UndoText), textBoxProps.NewLineCharacter), item.StartLine);

            }

            return item.Selection;
        }

        /// <summary>
        /// Excecutes the redo and apply the changes to the text
        /// </summary>
        /// <param name="NewLineCharacter">The current line-ending character either CR, LF or CRLF</param>
        /// <returns>A class containing the start and end-position of the selection</returns>
        public TextSelection Redo(StringManager StringManager)
        {
            if (RedoStack.Count < 1)
                return null;

            UndoRedoItem item = RedoStack.Pop();
            RecordUndo(item);
            HasRedone = true;

            //Faster for singleline
            if (item.UndoCount == 1 && item.RedoCount == 1)
            {
                textManager.Lines.SetLineText(item.StartLine, StringManager.CleanUpString(item.RedoText));
            }
            else
            {
                textManager.Lines.Safe_RemoveRange(item.StartLine, item.UndoCount);
                if (item.RedoCount > 0)
                    textManager.Lines.InsertOrAddRange(ListHelper.GetLinesFromString(StringManager.CleanUpString(item.RedoText), textBoxProps.NewLineCharacter), item.StartLine);
            }
            return null;
        }

        /// <summary>
        /// Clears all the items in the undo and redo stack
        /// </summary>
        public void ClearAll()
        {
            UndoStack.Clear();
            RedoStack.Clear();
            UndoStack.TrimExcess();
            RedoStack.TrimExcess();

            GC.Collect(GC.GetGeneration(UndoStack), GCCollectionMode.Optimized);
            GC.Collect(GC.GetGeneration(RedoStack), GCCollectionMode.Optimized);
        }

        public void NullAll()
        {
            UndoStack = null;
            RedoStack = null;
        }

        /// <summary>
        /// Gets if the undo stack contains actions
        /// </summary>
        public bool CanUndo { get => UndoStack.Count > 0; }

        /// <summary>
        /// Gets if the redo stack contains actions
        /// </summary>
        public bool CanRedo { get => RedoStack.Count > 0; }
    }
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
