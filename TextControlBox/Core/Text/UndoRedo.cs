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

        private TextManager textManager;
        private SelectionManager selectionManager;
        private CursorManager cursorManager;
        private EventsManager eventsManager;
        private TabSpaceManager tabSpaceManager;

        public bool EnableCombineNextUndoItems = false;
        private bool _isGroupingActions = false;

        public bool UndoBatching { get; set; } = true;
        public TimeSpan BatchTimeout { get; set; } = TimeSpan.FromMilliseconds(1000);
        private long _lastTypingTimestamp = 0;

        private bool _UndoRedoEnabled = true;
        public bool UndoRedoEnabled
        {
            get => _UndoRedoEnabled;
            set
            {
                _UndoRedoEnabled = value;

                //clear the items, since change on the text, while undo redo disabled
                //break the text. They depend on eachother.
                EndBatch();
                UndoStack.Clear();
                RedoStack.Clear();
            }
        }

        public void Init(TextManager textManager, SelectionManager selectionManager, CursorManager cursorManager, EventsManager eventsManager, TabSpaceManager tabSpaceManager)
        {
            this.textManager = textManager;
            this.selectionManager = selectionManager;
            this.cursorManager = cursorManager;
            this.eventsManager = eventsManager;
            this.tabSpaceManager = tabSpaceManager;
        }

        public void EndBatch()
        {
            if (UndoStack != null && UndoStack.Count > 0)
            {
                UndoStack.Peek().CanBatch = false;
            }
            _lastTypingTimestamp = 0;
        }

        public void BeginActionGroup()
        {
            EndBatch();
            if (!UndoRedoEnabled)
                return;

            _isGroupingActions = true;
        }

        public void EndActionGroup()
        {
            EndBatch();
            if (!UndoRedoEnabled)
                return;

            _isGroupingActions = false;

            if (UndoStack.Count > 0)
            {
                var lastItem = UndoStack.Pop();
                lastItem.HandleNextItemToo = false;
                lastItem.CanBatch = false;
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

        private void AddUndoItem(int startLine, string undoText, string redoText, int undoCount, int redoCount, CursorPosition cursorBefore, CursorPosition cursorAfter, TextSelection selectionBefore = null, TextSelection selectionAfter = null, object additionalData = null)
        {
            EndBatch();
            RedoStack.Clear();

            var item = new UndoRedoItem
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
                HandleNextItemToo = _isGroupingActions || EnableCombineNextUndoItems,
                AdditionalData = additionalData,
                CanBatch = false
            };
            if (undoCount == 1 && redoCount == 1)
                item.SetLineDiff(undoText, redoText);

            UndoStack.Push(item);
        }

        public void RecordTypingAction(Action action, int startline, string typedText, bool isAutoPaired = false)
        {
            if (!UndoRedoEnabled || startline < 0)
            {
                action.Invoke();
                return;
            }

            if (typedText == null || typedText.Length == 0)
            {
                action.Invoke();
                return;
            }

            if (!UndoBatching || _isGroupingActions || EnableCombineNextUndoItems)
            {
                EndBatch();
                RecordSingleLine(action, startline);
                return;
            }

            CharClass category = CharClassHelper.GetCharClass(typedText[0]);
            var curPosInLine = cursorManager.GetCurPosInLine();

            bool canMerge = false;
            UndoRedoItem topItem = null;
            string currentLineText = null;

            if (UndoStack.Count > 0)
            {
                topItem = UndoStack.Peek();
                if (topItem.CanBatch &&
                    topItem.StartLine == startline &&
                    topItem.UndoCount == 1 &&
                    topItem.RedoCount == 1 &&
                    !topItem.HandleNextItemToo &&
                    topItem.BatchCategory == category &&
                    topItem.CursorAfter.LineNumber == cursorManager.LineNumber &&
                    topItem.CursorAfter.CharacterPosition == curPosInLine &&
                    _lastTypingTimestamp != 0 &&
                    Stopwatch.GetElapsedTime(_lastTypingTimestamp) <= BatchTimeout)
                {
                    currentLineText = textManager.GetLineText(startline);
                    if (topItem.IsLineDiff ? topItem.MatchesLineAfterChange(currentLineText) : string.Equals(currentLineText, topItem.RedoText, StringComparison.Ordinal))
                    {
                        canMerge = true;
                    }
                }
            }

            if (canMerge && topItem != null)
            {
                RedoStack.Clear();

                action.Invoke();
                selectionManager.ClearSelection();

                var lineAfter = textManager.GetLineText(startline);
                var cursorAfter = new CursorPosition(cursorManager.currentCursorPosition);

                //the item now spans from its original line to the line after this key
                string lineBeforeItem = topItem.IsLineDiff ? topItem.GetUndoLine(currentLineText) : topItem.UndoText;
                topItem.SetLineDiff(lineBeforeItem, lineAfter);
                topItem.CursorAfter = cursorAfter;
                _lastTypingTimestamp = Stopwatch.GetTimestamp();

                if (isAutoPaired)
                {
                    topItem.CanBatch = false;
                }
            }
            else
            {
                EndBatch();
                RedoStack.Clear();

                var cursorBefore = new CursorPosition(cursorManager.currentCursorPosition);
                var lineBefore = textManager.GetLineText(startline);

                action.Invoke();
                selectionManager.ClearSelection();

                var lineAfter = textManager.GetLineText(startline);
                var cursorAfter = new CursorPosition(cursorManager.currentCursorPosition);

                if (BeforeAndAfterAreEqual(lineBefore, lineAfter))
                    return;

                var item = new UndoRedoItem
                {
                    StartLine = startline,
                    UndoCount = 1,
                    RedoCount = 1,
                    CursorBefore = cursorBefore,
                    CursorAfter = cursorAfter,
                    HandleNextItemToo = false,
                    CanBatch = !isAutoPaired,
                    BatchCategory = category
                };
                item.SetLineDiff(lineBefore, lineAfter);
                UndoStack.Push(item);

                _lastTypingTimestamp = Stopwatch.GetTimestamp();
            }
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

        public void RecordUndoAction(Action action, int startline, int undocount, int redoCount, object tag = null)
        {
            if (!UndoRedoEnabled || startline < 0)
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

            //an empty line and no line at all both produce "", so the line counts have to match too
            if (undocount == redoCount && BeforeAndAfterAreEqual(linesBefore, linesAfter))
                return;

            AddUndoItem(
                startline,
                linesBefore,
                linesAfter,
                undocount,
                redoCount,
                cursorBefore,
                cursorAfter,
                null,
                null,
                tag
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
            if (orderedSel.startLine < 0)
            {
                action.Invoke();
                return;
            }

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
            if(orderedSel.startLine < 0)
            {
                action.Invoke();
                return;
            }

            if (numberOfRemovedLines == -1)
                numberOfRemovedLines = orderedSel.endLine - orderedSel.startLine + 1;

            var cursorBefore = new CursorPosition(cursorManager.currentCursorPosition);
            var selectionBefore = new TextSelection(selection);
            var linesBefore = textManager.GetLinesAsString(orderedSel.startLine, numberOfRemovedLines);
            action.Invoke();

            var linesAfter = textManager.GetLinesAsString(orderedSel.startLine, numberOfAddedLines);
            var selectionAfter = new TextSelection(selection);
            var cursorAfter = new CursorPosition(cursorManager.currentCursorPosition);

            //an empty line and no line at all both produce "", so the line counts have to match too
            if (numberOfRemovedLines == numberOfAddedLines && BeforeAndAfterAreEqual(linesBefore, linesAfter))
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

        private void ApplyTabSpaceData(TabSpaceUndoData tsud, bool undo)
        {
            int spacesTabs = undo ? tsud.SpacesTabsBefore : tsud.SpacesTabsAfter;
            int documentSpacesTabs = undo ? tsud.DocumentSpacesTabsBefore : tsud.DocumentSpacesTabsAfter;

            tabSpaceManager.UseSpacesInsteadTabs = spacesTabs > 0;
            tabSpaceManager.NumberOfSpaces = tabSpaceManager.UseSpacesInsteadTabs ? spacesTabs : 4;

            tabSpaceManager.SetDocumentVariables(documentSpacesTabs > 0 ? documentSpacesTabs : 4, documentSpacesTabs > 0);
        }

        //Grouped items are handled in a loop, a recursion overflows the stack on large action groups
        public (CursorPosition cursor, TextSelection selection) Undo(StringManager stringManager)
        {
            EndBatch();

            if (!UndoRedoEnabled)
                return (null, null);

            if (UndoStack == null || UndoStack.Count < 1)
                return (null, null);

            CursorPosition finalCursor = null;
            TextSelection finalSelection = null;
            List<TabSpaceUndoData> tabSpaceData = null;

            bool isFirstItem = true;
            while (UndoStack.Count > 0 && (isFirstItem || UndoStack.Peek().HandleNextItemToo))
            {
                var item = UndoStack.Pop();
                item.CanBatch = false;
                //calculate actual lines that can be removed
                int actualLinesToRemove = Math.Min(item.RedoCount, textManager.LinesCount - item.StartLine);

                //record an adjusted redo item if needed
                if (actualLinesToRemove < item.RedoCount)
                {
                    //the line of a single line item is gone, it can only be inserted again as a whole
                    if (item.IsLineDiff)
                        item.ToFullText(item.GetUndoLine(""), "");

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
                    string undoLine = item.IsLineDiff ? item.GetUndoLine(textManager.GetLineText(item.StartLine)) : item.UndoText;
                    textManager.SetLineText(item.StartLine, stringManager.CleanUpString(undoLine));
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

                //the oldest item of a group defines the cursor and selection
                if (isFirstItem || item.CursorBefore != null)
                    finalCursor = item.CursorBefore;
                if (isFirstItem || item.SelectionBefore != null)
                    finalSelection = item.SelectionBefore;

                if (item.AdditionalData is TabSpaceUndoData tsud)
                    (tabSpaceData ??= new List<TabSpaceUndoData>()).Add(tsud);

                isFirstItem = false;
            }

            if (tabSpaceData != null)
            {
                for (int i = tabSpaceData.Count - 1; i >= 0; i--)
                    ApplyTabSpaceData(tabSpaceData[i], undo: true);
            }

            return (finalCursor, finalSelection);
        }
        public (CursorPosition cursor, TextSelection selection) Redo(StringManager stringManager)
        {
            EndBatch();

            if (!UndoRedoEnabled)
                return (null, null);

            if (RedoStack == null || RedoStack.Count < 1)
                return (null, null);

            CursorPosition finalCursor = null;
            TextSelection finalSelection = null;
            List<TabSpaceUndoData> tabSpaceData = null;

            bool isFirstItem = true;
            bool handleNextItem = true;
            while (handleNextItem && RedoStack.Count > 0)
            {
                UndoRedoItem item = RedoStack.Pop();
                item.CanBatch = false;

                //calculate how many lines can actually be removed
                int actualLinesToRemove = Math.Min(item.UndoCount, textManager.LinesCount - item.StartLine);

                if (actualLinesToRemove < item.UndoCount)
                {
                    if (item.IsLineDiff)
                        item.ToFullText("", item.GetRedoLine(""));

                    string currentText = "";
                    if (actualLinesToRemove > 0)
                        currentText = textManager.GetLinesAsString(item.StartLine, actualLinesToRemove);

                    item.UndoText = currentText;
                    item.UndoCount = actualLinesToRemove;
                }
                RecordUndo(item);

                if (item.UndoCount == 1 && item.RedoCount == 1)
                {
                    string redoLine = item.IsLineDiff ? item.GetRedoLine(textManager.GetLineText(item.StartLine)) : item.RedoText;
                    textManager.SetLineText(item.StartLine, stringManager.CleanUpString(redoLine));
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

                //the newest item of a group defines the cursor and selection
                if (isFirstItem || item.CursorAfter != null)
                    finalCursor = item.CursorAfter;
                if (isFirstItem || item.SelectionAfter != null)
                    finalSelection = item.SelectionAfter;

                if (item.AdditionalData is TabSpaceUndoData tsud)
                    (tabSpaceData ??= new List<TabSpaceUndoData>()).Add(tsud);

                handleNextItem = item.HandleNextItemToo;
                isFirstItem = false;
            }

            if (tabSpaceData != null)
            {
                for (int i = tabSpaceData.Count - 1; i >= 0; i--)
                    ApplyTabSpaceData(tabSpaceData[i], undo: false);
            }

            return (finalCursor, finalSelection);
        }

        /// <summary>
        /// Clears all the items in the undo and redo stack
        /// </summary>
        public void ClearAll()
        {
            EndBatch();
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
        public bool CanUndo { get => UndoStack != null && UndoStack.Count > 0; }

        /// <summary>
        /// Gets if the redo stack contains actions
        /// </summary>
        public bool CanRedo { get => RedoStack != null && RedoStack.Count > 0; }

        /// <summary>
        /// Gets if an action group is currently being recorded
        /// </summary>
        public bool IsGroupingActions { get => _isGroupingActions; }
    }
}