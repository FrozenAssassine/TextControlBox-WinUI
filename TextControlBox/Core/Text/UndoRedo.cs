﻿using System;
using System.Collections.Generic;
using TextControlBoxNS.Core.Selection;
using TextControlBoxNS.Helper;
using TextControlBoxNS.Models;

namespace TextControlBoxNS.Core.Text
{
    internal class UndoRedo
    {
        private Stack<UndoRedoItem> UndoStack = new Stack<UndoRedoItem>();
        private Stack<UndoRedoItem> RedoStack = new Stack<UndoRedoItem>();

        private bool HasRedone = false;
        private TextManager textManager;
        private SelectionManager selectionManager;
        public void Init(TextManager textManager, SelectionManager selectionManager)
        {
            this.textManager = textManager;
            this.selectionManager = selectionManager;
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
            var lineBefore = textManager.GetLineText(startline);
            action.Invoke();
            var lineAfter = textManager.GetLineText(startline);
            AddUndoItem(null, startline, lineBefore, lineAfter, 1, 1);
        }

        public void RecordUndoAction(Action action, int startline, int undocount, int redoCount, CursorPosition cursorposition = null)
        {
            if (undocount == 1 && redoCount == 1)
            {
                RecordSingleLine(action, startline);
                return;
            }

            var linesBefore = textManager.GetLinesAsString(startline, undocount);
            action.Invoke();
            var linesAfter = textManager.GetLinesAsString(startline, redoCount);

            AddUndoItem(
                cursorposition == null ? null : new TextSelection(cursorposition, null),
                startline,
                linesBefore,
                linesAfter,
                undocount,
                redoCount
                );
        }
        public void RecordUndoAction(Action action, TextSelection selection, int numberOfAddedLines)
        {
            var orderedSel = SelectionHelper.OrderTextSelectionSeparated(selection);
            int numberOfRemovedLines = orderedSel.endLine - orderedSel.startLine + 1;

            //TODO! maybe combine this, but not sure whether it is working properly yet!
            if (numberOfAddedLines == 0 && numberOfRemovedLines == 1)
            {
                numberOfAddedLines += 1;
            }
            else if (numberOfAddedLines == 0 && !SelectionHelper.WholeLinesAreSelected(selection, textManager))
            {
                numberOfAddedLines += 1;
            }

            var linesBefore = textManager.GetLinesAsString(orderedSel.startLine, numberOfRemovedLines);
            action.Invoke();
            var linesAfter = textManager.GetLinesAsString(orderedSel.startLine, numberOfAddedLines);

            AddUndoItem(
                new TextSelection(selection),
                orderedSel.startLine,
                linesBefore,
                linesAfter,
                numberOfRemovedLines,
                numberOfAddedLines
                );
        }

        public TextSelection Undo(StringManager stringManager)
        {
            if (UndoStack.Count < 1)
                return null;

            if (HasRedone)
            {
                HasRedone = false;
                RedoStack.Clear();
            }

            var item = UndoStack.Pop();
            RecordRedo(item);

            //Faster for singleline
            if (item.UndoCount == 1 && item.RedoCount == 1)
            {
                textManager.SetLineText(item.StartLine, stringManager.CleanUpString(item.UndoText));
            }
            else
            {
                textManager.Safe_RemoveRange(item.StartLine, item.RedoCount);
                if (item.UndoCount > 0)
                {
                    var cleanedLines = ListHelper.GetLinesFromString(
                        stringManager.CleanUpString(item.UndoText),
                        textManager.NewLineCharacter
                    );

                    textManager.InsertOrAddRange(cleanedLines, item.StartLine);
                }
            }

            return item.Selection;
        }

        public TextSelection Redo(StringManager stringManager)
        {
            if (RedoStack.Count < 1)
                return null;

            UndoRedoItem item = RedoStack.Pop();
            RecordUndo(item);
            HasRedone = true;

            //Faster for singleline
            if (item.UndoCount == 1 && item.RedoCount == 1)
            {
                textManager.SetLineText(item.StartLine, stringManager.CleanUpString(item.RedoText));
            }
            else
            {
                textManager.Safe_RemoveRange(item.StartLine, item.UndoCount);
                if (item.RedoCount > 0)
                {
                    var cleanedLines = ListHelper.GetLinesFromString(
                        stringManager.CleanUpString(item.RedoText),
                        textManager.NewLineCharacter
                    );
                    textManager.InsertOrAddRange(cleanedLines, item.StartLine);
                }
            }
            return item.Selection;
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
