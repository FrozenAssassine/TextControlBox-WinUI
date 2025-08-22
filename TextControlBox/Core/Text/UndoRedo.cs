using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        public bool EnableCombineNextUndoItems = false;

        private bool _isGroupingActions = false;

        private bool _UndoRedoEnabled = true;
        public bool UndoRedoEnabled
        {
            get => _UndoRedoEnabled;
            set
            {
                _UndoRedoEnabled = value;

                //clear the items, since change on the text, while undo redo disabled
                //break the text. They depend on eachother.
                UndoStack.Clear();
                RedoStack.Clear();
            }
        }

        public void Init(TextManager textManager, SelectionManager selectionManager, CursorManager cursorManager)
        {
            this.textManager = textManager;
            this.selectionManager = selectionManager;
            this.cursorManager = cursorManager;
        }

        public void BeginActionGroup()
        {
            if (!UndoRedoEnabled)
                return;

            _isGroupingActions = true;
        }

        public void EndActionGroup()
        {
            if (!UndoRedoEnabled)
                return;

            _isGroupingActions = false;

            if (UndoStack.Count > 0)
            {
                var lastItem = UndoStack.Pop();
                lastItem.HandleNextItemToo = false;
                UndoStack.Push(lastItem);
            }
        }

        public void ExecuteActionGroup(Action actionGroup)
        {
            if (!UndoRedoEnabled)
            {
                actionGroup.Invoke();
                return;
            }

            BeginActionGroup();
            try
            {
                actionGroup.Invoke();
            }
            finally
            {
                EndActionGroup();
            }
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
                HandleNextItemToo = _isGroupingActions || EnableCombineNextUndoItems
            });
        }

        private bool BeforeAndAfterAreEqual(string linesBefore, string linesAfter)
        {
            if (linesBefore.Length != linesAfter.Length)
                return false;

            return linesBefore.Equals(linesAfter, StringComparison.Ordinal);
        }

        private void RecordSingleLine(Action action, int startline)
        {
            if (!UndoRedoEnabled)
            {
                action.Invoke();
                return;
            }

            var cursorBefore = new CursorPosition(cursorManager.currentCursorPosition);
            var lineBefore = textManager.GetLineText(startline);
            action.Invoke();
            selectionManager.ClearSelection();

            var lineAfter = textManager.GetLineText(startline);
            var cursorAfter = new CursorPosition(cursorManager.currentCursorPosition);

            if (BeforeAndAfterAreEqual(lineBefore, lineAfter))
                return;

            AddUndoItem(startline, lineBefore, lineAfter, 1, 1, cursorBefore, cursorAfter);
        }

        public void RecordUndoAction(Action action, int startline, int undocount, int redoCount)
        {
            if (!UndoRedoEnabled)
            {
                action.Invoke();
                return;
            }

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

            if (linesBefore.Length > 0 && linesAfter.Length > 0 && BeforeAndAfterAreEqual(linesBefore, linesAfter))
                return;

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

        public void RecordUndoActionTab(Action action, TextSelection selection, int numberOfAddedRemovedLines)
        {
            if (!UndoRedoEnabled)
            {
                action.Invoke();
                return;
            }
            var orderedSel = SelectionHelper.OrderTextSelectionSeparated(selection);
            var cursorBefore = new CursorPosition(cursorManager.currentCursorPosition);
            var selectionBefore = new TextSelection(selection);
            var linesBefore = textManager.GetLinesAsString(orderedSel.startLine, numberOfAddedRemovedLines);

            action.Invoke();

            var linesAfter = textManager.GetLinesAsString(orderedSel.startLine, numberOfAddedRemovedLines);
            var selectionAfter = new TextSelection(selection);
            var cursorAfter = new CursorPosition(cursorManager.currentCursorPosition);

            if (BeforeAndAfterAreEqual(linesBefore, linesAfter))
                return;

            AddUndoItem(
                orderedSel.startLine,
                linesBefore,
                linesAfter,
                numberOfAddedRemovedLines,
                numberOfAddedRemovedLines,
                cursorBefore,
                cursorAfter,
                selectionBefore,
                selectionAfter
                );
        }

        public void RecordUndoAction(Action action, TextSelection selection, int numberOfAddedLines, int numberOfRemovedLines = -1)
        {
            if (!UndoRedoEnabled)
            {
                action.Invoke();
                return;
            }

            var orderedSel = SelectionHelper.OrderTextSelectionSeparated(selection);
            if (numberOfRemovedLines == -1)
                numberOfRemovedLines = orderedSel.endLine - orderedSel.startLine + 1;

            var cursorBefore = new CursorPosition(cursorManager.currentCursorPosition);
            var selectionBefore = new TextSelection(selection);
            var linesBefore = textManager.GetLinesAsString(orderedSel.startLine, numberOfRemovedLines);
            action.Invoke();

            var linesAfter = textManager.GetLinesAsString(orderedSel.startLine, numberOfAddedLines);
            var selectionAfter = new TextSelection(selection);
            var cursorAfter = new CursorPosition(cursorManager.currentCursorPosition);

            if (BeforeAndAfterAreEqual(linesBefore, linesAfter))
                return;

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
            if (!UndoRedoEnabled)
                return (null, null);

            if (UndoStack.Count < 1)
                return (null, null);

            if (HasRedone)
            {
                HasRedone = false;
                RedoStack.Clear();
            }

            var item = UndoStack.Pop();
            //calculate actual lines that can be removed
            int actualLinesToRemove = Math.Min(item.RedoCount, textManager.LinesCount - item.StartLine);

            //record an adjusted redo item if needed
            if (actualLinesToRemove < item.RedoCount)
            {
                string currentText = "";
                if (actualLinesToRemove > 0)
                    currentText = textManager.GetLinesAsString(item.StartLine, actualLinesToRemove);

                item.RedoText = currentText;
                item.RedoCount = actualLinesToRemove;
            }

            RecordRedo(item);

            //faster for singleline
            if (item.UndoCount == 1 && item.RedoCount == 1)
            {
                textManager.SetLineText(item.StartLine, stringManager.CleanUpString(item.UndoText));
            }
            else
            {
                if (actualLinesToRemove > 0)
                {
                    textManager.RemoveRange(item.StartLine, actualLinesToRemove);
                }

                if (item.UndoCount > 0)
                {
                    var cleanedLines = ListHelper.GetLinesFromString(
                        stringManager.CleanUpString(item.UndoText),
                        textManager.NewLineCharacter
                    );

                    //prevent over inserting
                    if (item.StartLine <= textManager.LinesCount)
                        textManager.InsertOrAddRange(cleanedLines, item.StartLine);
                }
            }

            if (UndoStack.Count > 0)
            {
                var nextItem = UndoStack.Peek();
                if (nextItem.HandleNextItemToo)
                {
                    Undo(stringManager);
                }
            }

            return (item.CursorBefore, item.SelectionBefore);
        }
        public (CursorPosition cursor, TextSelection selection) Redo(StringManager stringManager)
        {
            if (!UndoRedoEnabled)
                return (null, null);

            if (RedoStack.Count < 1)
                return (null, null);

            UndoRedoItem item = RedoStack.Pop();

            //calculate how many lines can actually be removed
            int actualLinesToRemove = Math.Min(item.UndoCount, textManager.LinesCount - item.StartLine);

            if (actualLinesToRemove < item.UndoCount)
            {
                string currentText = "";
                if (actualLinesToRemove > 0)
                    currentText = textManager.GetLinesAsString(item.StartLine, actualLinesToRemove);

                item.UndoText = currentText;
                item.UndoCount = actualLinesToRemove;
            }
            RecordUndo(item);

            HasRedone = true;

            if (item.UndoCount == 1 && item.RedoCount == 1)
            {
                textManager.SetLineText(item.StartLine, stringManager.CleanUpString(item.RedoText));
            }
            else
            {
                //remove only what exists
                if (actualLinesToRemove > 0)
                {
                    textManager.RemoveRange(item.StartLine, actualLinesToRemove);
                }

                if (item.RedoCount > 0)
                {
                    var cleanedLines = ListHelper.GetLinesFromString(
                        stringManager.CleanUpString(item.RedoText),
                        textManager.NewLineCharacter
                    );
                    textManager.InsertOrAddRange(cleanedLines, item.StartLine);
                }
            }

            if (item.HandleNextItemToo)
            {
                Redo(stringManager);
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

        /// <summary>
        /// Gets if an action group is currently being recorded
        /// </summary>
        public bool IsGroupingActions { get => _isGroupingActions; }
    }
}