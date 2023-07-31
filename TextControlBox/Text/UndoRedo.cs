using Collections.Pooled;
using System;
using System.Collections.Generic;
using TextControlBoxNS.Extensions;
using TextControlBoxNS.Helper;
using TextControlBoxNS.Models;

namespace TextControlBoxNS.Text
{
    internal class UndoRedo
    {
        private Stack<UndoRedoItem> UndoStack = new Stack<UndoRedoItem>();
        private Stack<UndoRedoItem> RedoStack = new Stack<UndoRedoItem>();

        private bool HasRedone = false;

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

        private void RecordSingleLine(Action action, PooledList<string> totalLines, int startline)
        {
            var lineBefore = totalLines.GetLineText(startline);
            action.Invoke();
            var lineAfter = totalLines.GetLineText(startline);
            AddUndoItem(null, startline, lineBefore, lineAfter, 1, 1);
        }

        public void RecordUndoAction(Action action, PooledList<string> totalLines, int startline, int undocount, int redoCount, string newLineCharacter, CursorPosition cursorposition = null)
        {
            if (undocount == redoCount && redoCount == 1)
            {
                RecordSingleLine(action, totalLines, startline);
                return;
            }

            var linesBefore = totalLines.GetLines(startline, undocount).GetString(newLineCharacter);
            action.Invoke();
            var linesAfter = totalLines.GetLines(startline, redoCount).GetString(newLineCharacter);

            AddUndoItem(cursorposition == null ? null : new TextSelection(new CursorPosition(cursorposition), null), startline, linesBefore, linesAfter, undocount, redoCount);
        }
        public void RecordUndoAction(Action action, PooledList<string> totalLines, TextSelection selection, int numberOfAddedLines, string newLineCharacter)
        {
            var orderedSel = Selection.OrderTextSelection(selection);
            if (orderedSel.StartPosition.LineNumber == orderedSel.EndPosition.LineNumber && orderedSel.StartPosition.LineNumber == 1)
            {
                RecordSingleLine(action, totalLines, orderedSel.StartPosition.LineNumber);
                return;
            }

            int numberOfRemovedLines = orderedSel.EndPosition.LineNumber - orderedSel.StartPosition.LineNumber + 1;

            if (numberOfAddedLines == 0 && !Selection.WholeLinesAreSelected(selection, totalLines) ||
                orderedSel.StartPosition.LineNumber == orderedSel.EndPosition.LineNumber && orderedSel.Length == totalLines.GetLineLength(orderedSel.StartPosition.LineNumber))
                numberOfAddedLines += 1;

            var linesBefore = totalLines.GetLines(orderedSel.StartPosition.LineNumber, numberOfRemovedLines).GetString(newLineCharacter);
            action.Invoke();
            var linesAfter = totalLines.GetLines(orderedSel.StartPosition.LineNumber, numberOfAddedLines).GetString(newLineCharacter);

            AddUndoItem(
                selection,
                orderedSel.StartPosition.LineNumber,
                linesBefore,
                linesAfter,
                numberOfRemovedLines,
                numberOfAddedLines
                );
        }

        public TextSelection Undo(PooledList<string> totalLines, StringManager stringManager, string newLineCharacter)
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
                totalLines.SetLineText(item.StartLine, stringManager.CleanUpString(item.UndoText));
            }
            else
            {
                totalLines.Safe_RemoveRange(item.StartLine, item.RedoCount);
                if (item.UndoCount > 0)
                    totalLines.InsertOrAddRange(ListHelper.GetLinesFromString(stringManager.CleanUpString(item.UndoText), newLineCharacter), item.StartLine);
            }

            return item.Selection;
        }

        public TextSelection Redo(PooledList<string> totalLines, StringManager stringmanager, string newLineCharacter)
        {
            if (RedoStack.Count < 1)
                return null;

            UndoRedoItem item = RedoStack.Pop();
            RecordUndo(item);
            HasRedone = true;

            //Faster for singleline
            if (item.UndoCount == 1 && item.RedoCount == 1)
            {
                totalLines.SetLineText(item.StartLine, stringmanager.CleanUpString(item.RedoText));
            }
            else
            {
                totalLines.Safe_RemoveRange(item.StartLine, item.UndoCount);
                if (item.RedoCount > 0)
                    totalLines.InsertOrAddRange(ListHelper.GetLinesFromString(stringmanager.CleanUpString(item.RedoText), newLineCharacter), item.StartLine);
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
}
