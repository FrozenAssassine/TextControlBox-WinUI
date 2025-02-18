﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.ConstrainedExecution;
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
        private CursorManager cursorManager;

        public void Init(TextManager textManager, SelectionManager selectionManager, CursorManager cursorManager)
        {
            this.textManager = textManager;
            this.selectionManager = selectionManager;
            this.cursorManager = cursorManager;
        }

        private void RecordRedo(UndoRedoItem item)
        {
            RedoStack.Push(item);
        }
        private void RecordUndo(UndoRedoItem item)
        {
            UndoStack.Push(item);
        }

        private void AddUndoItem(int startLine, string undoText, string redoText, int undoCount, int redoCount, CursorPosition cursorBefore, CursorPosition cursorAfter, TextSelection selectionBefore = null, TextSelection selectionAfter = null)
        {
            UndoStack.Push(new UndoRedoItem
            {
                RedoText = redoText,
                UndoText = undoText,
                SelectionBefore = selectionBefore,
                SelectionAfter = selectionAfter,
                CursorBefore = cursorBefore,
                CursorAfter = cursorAfter,
                StartLine = startLine,
                UndoCount = undoCount,
                RedoCount = redoCount,
            });
        }

        private void RecordSingleLine(Action action, int startline)
        {
            var cursorBefore = new CursorPosition(cursorManager.currentCursorPosition);
            var lineBefore = textManager.GetLineText(startline);
            action.Invoke();
            selectionManager.ClearSelection();

            var lineAfter = textManager.GetLineText(startline);
            var cursorAfter = new CursorPosition(cursorManager.currentCursorPosition);

            AddUndoItem(startline, lineBefore, lineAfter, 1, 1, cursorBefore, cursorAfter);
        }

        public void RecordUndoAction(Action action, int startline, int undocount, int redoCount)
        {
            if (undocount == 1 && redoCount == 1)
            {
                RecordSingleLine(action, startline);
                return;
            }
            var cursorBefore = new CursorPosition(cursorManager.currentCursorPosition);
            undocount = Math.Min(undocount, textManager.LinesCount - startline);
            var linesBefore = textManager.GetLinesAsString(startline, undocount);
            
            action.Invoke();

            redoCount = Math.Min(redoCount, textManager.LinesCount - startline);
            var linesAfter = textManager.GetLinesAsString(startline, redoCount);
            var cursorAfter = new CursorPosition(cursorManager.currentCursorPosition);

            AddUndoItem(
                startline,
                linesBefore,
                linesAfter,
                undocount,
                redoCount,
                cursorBefore,
                cursorAfter
                );
        }
        public void RecordUndoAction(Action action, TextSelection selection, int numberOfAddedLines)
        {
            var orderedSel = SelectionHelper.OrderTextSelectionSeparated(selection);
            int numberOfRemovedLines = orderedSel.endLine - orderedSel.startLine + 1;

            //TODO! combine things, not sure whether it is working properly yet! 
            //More tests to come!
            if (numberOfAddedLines == 0 && numberOfRemovedLines == 1)
            {
                numberOfAddedLines += 1;
            }
            else if (numberOfAddedLines == 0 && !SelectionHelper.WholeLinesAreSelected(selection, textManager))
            {
                numberOfAddedLines += 1;
            }
            else if (numberOfAddedLines == 0 && SelectionHelper.WholeLinesAreSelected(selection, textManager))
            {
                //triggers, when deleting multiple completely selected lines 
                numberOfAddedLines += 1;
            }

            //triple click selection is one character longer than normal selection and removes the whole line instead of the line content
            if(orderedSel.startLine == orderedSel.endLine && orderedSel.startChar == 0 && orderedSel.endChar == textManager.GetLineLength(orderedSel.startLine) + 1)
            {
                numberOfRemovedLines += 1;
            }

            var cursorBefore = new CursorPosition(cursorManager.currentCursorPosition);
            var selectionBefore = new TextSelection(selection);
            var linesBefore = textManager.GetLinesAsString(orderedSel.startLine, numberOfRemovedLines);
            action.Invoke();

            var linesAfter = textManager.GetLinesAsString(orderedSel.startLine, numberOfAddedLines);
            var selectionAfter = new TextSelection(selection);
            var cursorAfter = new CursorPosition(cursorManager.currentCursorPosition);

            AddUndoItem(
                orderedSel.startLine,
                linesBefore,
                linesAfter,
                numberOfRemovedLines,
                numberOfAddedLines,
                cursorBefore,
                cursorAfter,
                selectionBefore,
                selectionAfter
                );
        }

        public (CursorPosition cursor, TextSelection selection) Undo(StringManager stringManager)
        {
            if (UndoStack.Count < 1)
                return (null, null);

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
                textManager.RemoveRange(item.StartLine, item.RedoCount);
                if (item.UndoCount > 0)
                {
                    var cleanedLines = ListHelper.GetLinesFromString(
                        stringManager.CleanUpString(item.UndoText),
                        textManager.NewLineCharacter
                    );

                    textManager.InsertOrAddRange(cleanedLines, item.StartLine);
                }
            }

            return (item.CursorBefore, item.SelectionBefore);
        }

        public (CursorPosition cursor, TextSelection selection) Redo(StringManager stringManager)
        {
            if (RedoStack.Count < 1)
                return (null, null);

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
                textManager.RemoveRange(item.StartLine, item.UndoCount);
                if (item.RedoCount > 0)
                {
                    var cleanedLines = ListHelper.GetLinesFromString(
                        stringManager.CleanUpString(item.RedoText),
                        textManager.NewLineCharacter
                    );
                    textManager.InsertOrAddRange(cleanedLines, item.StartLine);
                }
            }
            return (item.CursorAfter, item.SelectionAfter);
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
