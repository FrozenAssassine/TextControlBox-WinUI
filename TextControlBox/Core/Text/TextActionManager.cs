﻿using Microsoft.UI.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using TextControlBoxNS.Core.Renderer;
using TextControlBoxNS.Core.Selection;
using TextControlBoxNS.Core.Text.TextActions;
using TextControlBoxNS.Extensions;
using TextControlBoxNS.Helper;
using TextControlBoxNS.Models;
using Windows.ApplicationModel.DataTransfer;

namespace TextControlBoxNS.Core.Text
{
    internal class TextActionManager
    {
        private CanvasUpdateManager canvasUpdateManager;
        private TextManager textManager;
        private SelectionRenderer selectionRenderer;
        private SelectionManager selectionManager;
        private CursorManager cursorManager;
        private CoreTextControlBox coreTextbox;
        private UndoRedo undoRedo;
        private LongestLineManager longestLineManager;
        private CurrentLineManager currentLineManager;
        private ScrollManager scrollManager;
        private EventsManager eventsManager;
        private TextRenderer textRenderer;
        private StringManager stringManager;
        private AutoIndentionManager autoIndentionManager;

        private readonly AddCharacterTextAction addCharacterTextAction = new AddCharacterTextAction();
        private readonly DeleteTextAction deleteTextAction = new DeleteTextAction();
        private readonly AddNewLineTextAction addNewLineTextAction = new AddNewLineTextAction();
        private readonly RemoveTextAction removeTextAction = new RemoveTextAction();

        public void Init(
            CoreTextControlBox coreTextbox,
            TextRenderer textRenderer,
            UndoRedo undoRedo,
            CurrentLineManager currentLineManager,
            LongestLineManager longestLineManager,
            CanvasUpdateManager canvasUpdateHelper,
            TextManager textManager,
            SelectionRenderer selectionRenderer,
            CursorManager cursorManager,
            ScrollManager scrollManager,
            EventsManager eventsManager,
            StringManager stringManager,
            SelectionManager selectionManager,
            AutoIndentionManager autoIndentationManager
            )
        {
            this.canvasUpdateManager = canvasUpdateHelper;
            this.textManager = textManager;
            this.selectionRenderer = selectionRenderer;
            this.cursorManager = cursorManager;
            this.coreTextbox = coreTextbox;
            this.undoRedo = undoRedo;
            this.longestLineManager = longestLineManager;
            this.currentLineManager = currentLineManager;
            this.scrollManager = scrollManager;
            this.eventsManager = eventsManager;
            this.textRenderer = textRenderer;
            this.stringManager = stringManager;
            this.selectionManager = selectionManager;
            this.autoIndentionManager = autoIndentationManager;

            removeTextAction.Init(textManager, undoRedo, currentLineManager, longestLineManager, cursorManager);
            deleteTextAction.Init(textManager, coreTextbox, undoRedo, currentLineManager, longestLineManager, cursorManager);
            addCharacterTextAction.Init(textManager, coreTextbox, undoRedo, currentLineManager, longestLineManager, cursorManager, selectionManager, canvasUpdateHelper);
            addNewLineTextAction.Init(textManager, undoRedo, currentLineManager, cursorManager, eventsManager, canvasUpdateManager, selectionManager, autoIndentionManager);
        }

        public void SelectAll()
        {
            //No selection can be shown
            if (textManager.LinesCount == 1 && textManager.GetLineLength(0) == 0)
                return;

            selectionManager.SetSelection(0, 0, textManager.LinesCount - 1, textManager.GetLineLength(-1));
            cursorManager.SetCursorPositionCopyValues(selectionManager.selectionEnd);
            canvasUpdateManager.UpdateSelection();
            canvasUpdateManager.UpdateCursor();
        }

        private void ResetUndoRedoSelection(CursorPosition cursor, TextSelection selection)
        {
            if (cursor != null)
                cursorManager.SetCursorPositionCopyValues(cursor);

            if (selection != null)
                selectionManager.SetSelection(selection);
            else
                selectionManager.ClearSelection();
        }

        public void Undo()
        {
            if (coreTextbox.IsReadonly || !undoRedo.CanUndo)
                return;

            //Do the Undo
            coreTextbox.ChangeCursor(InputSystemCursorShape.Wait);
            var(cursor, selection) = undoRedo.Undo(stringManager);
            eventsManager.CallTextChanged();
            coreTextbox.ChangeCursor(InputSystemCursorShape.IBeam);

            longestLineManager.needsRecalculation = true;

            ResetUndoRedoSelection(cursor, selection);

            scrollManager.UpdateScrollToShowCursor(false);
            canvasUpdateManager.UpdateAll();
        }
        public void Redo()
        {
            if (coreTextbox.IsReadonly || !undoRedo.CanRedo)
                return;

            //Do the Redo
            coreTextbox.ChangeCursor(InputSystemCursorShape.Wait);
            var (cursor, selection) = undoRedo.Redo(stringManager);
            eventsManager.CallTextChanged();
            coreTextbox.ChangeCursor(InputSystemCursorShape.IBeam);

            longestLineManager.needsRecalculation = true;

            ResetUndoRedoSelection(cursor, selection);

            scrollManager.UpdateScrollToShowCursor(false);
            canvasUpdateManager.UpdateAll();
        }

        //Trys running the code and clears the memory if OutOfMemoryException gets thrown
        public async void Safe_Paste(bool handleException = true)
        {
            try
            {
                DataPackageView dataPackageView = Clipboard.GetContent();
                if (dataPackageView.Contains(StandardDataFormats.Text))
                {
                    string text = null;
                    try
                    {
                        text = await dataPackageView.GetTextAsync();
                    }
                    catch (Exception ex) //When longer holding Ctrl + V the clipboard may throw an exception:
                    {
                        Debug.WriteLine("Clipboard exception: " + ex.Message);
                        return;
                    }

                    if (await Utils.IsOverTextLimit(text.Length))
                        return;

                    AddCharacter(stringManager.CleanUpString(text));
                }
            }
            catch (OutOfMemoryException)
            {
                if (handleException)
                {
                    textManager.CleanUp();
                    Safe_Paste(false);
                    return;
                }
                throw new OutOfMemoryException();
            }
        }
        public string Safe_Gettext(bool handleException = true)
        {
            try
            {
                return textManager.GetLinesAsString();
            }
            catch (OutOfMemoryException)
            {
                if (handleException)
                {
                    textManager.CleanUp();
                    return Safe_Gettext(false);
                }
                throw new OutOfMemoryException();
            }
        }
        public void Safe_Cut(bool handleException = true)
        {
            try
            {
                DataPackage dataPackage = new DataPackage();
                dataPackage.SetText(coreTextbox.SelectedText);
                if (!selectionManager.HasSelection)
                    DeleteLine(cursorManager.LineNumber); //Delete the line
                else
                    DeleteText(); //Delete the selected text

                dataPackage.RequestedOperation = DataPackageOperation.Move;
                Clipboard.SetContent(dataPackage);
                selectionManager.ForceClearSelection(canvasUpdateManager);
            }
            catch (OutOfMemoryException)
            {
                if (handleException)
                {
                    textManager.CleanUp();
                    Safe_Cut(false);
                    return;
                }
                throw new OutOfMemoryException();
            }
        }
        public void Safe_Copy(bool handleException = true)
        {
            try
            {
                DataPackage dataPackage = new DataPackage();
                dataPackage.SetText(coreTextbox.SelectedText);
                dataPackage.RequestedOperation = DataPackageOperation.Copy;
                Clipboard.SetContent(dataPackage);
            }
            catch (OutOfMemoryException)
            {
                if (handleException)
                {
                    textManager.CleanUp();
                    Safe_Copy(false);
                    return;
                }
                throw new OutOfMemoryException();
            }
        }
        public void Safe_LoadLines(IEnumerable<string> lines, LineEnding lineEnding = LineEnding.CRLF, bool HandleException = true)
        {
            try
            {
                selectionManager.ClearSelection();
                undoRedo.ClearAll();

                textManager.ClearText();
                textManager.totalLines.AddRange(lines);

                textManager.LineEnding = lineEnding;

                longestLineManager.needsRecalculation = true;
                canvasUpdateManager.UpdateAll();
            }
            catch (OutOfMemoryException)
            {
                if (HandleException)
                {
                    textManager.CleanUp();
                    Safe_LoadLines(lines, lineEnding, false);
                    return;
                }
                throw new OutOfMemoryException();
            }
        }
        public async void Safe_LoadText(string text, bool handleException = true)
        {
            try
            {
                if (await Utils.IsOverTextLimit(text.Length))
                    return;

                //Get the LineEnding
                textManager.LineEnding = LineEndings.FindLineEnding(text);

                selectionManager.ClearSelection();
                undoRedo.ClearAll();

                longestLineManager.needsRecalculation = true;

                if (text.Length == 0)
                    textManager.ClearText(true);
                else
                    selectionManager.ReplaceLines(0, textManager.LinesCount, stringManager.CleanUpString(text).Split(textManager.NewLineCharacter));

                canvasUpdateManager.UpdateAll();
            }
            catch (OutOfMemoryException)
            {
                if (handleException)
                {
                    textManager.CleanUp();
                    Safe_LoadText(text, false);
                    return;
                }
                throw new OutOfMemoryException();
            }
        }
        public async void Safe_SetText(string text, bool handleException = true)
        {
            try
            {
                if (await Utils.IsOverTextLimit(text.Length))
                    return;

                longestLineManager.needsRecalculation = true;
                undoRedo.RecordUndoAction(() =>
                {
                    selectionManager.ClearSelection();

                    selectionManager.ReplaceLines(0, textManager.LinesCount, stringManager.CleanUpString(text).Split(textManager.NewLineCharacter));
                    if (text.Length == 0) //Create a new line when the text gets cleared
                        textManager.AddLine();

                }, 0, textManager.LinesCount, text.CountLines(textManager.NewLineCharacter));

                canvasUpdateManager.UpdateAll();
            }
            catch (OutOfMemoryException)
            {
                if (handleException)
                {
                    textManager.CleanUp();
                    Safe_SetText(text, false);
                    return;
                }
                throw new OutOfMemoryException();
            }
        }

        public void DeleteSelection()
        {
            if (!selectionManager.HasSelection)
                return;

            //line gets deleted -> recalculate the longest line:
            longestLineManager.CheckSelection();

            undoRedo.RecordUndoAction(() =>
            {
                selectionManager.Remove();
                selectionManager.ClearSelection();

            }, selectionManager.currentTextSelection, 0);

            canvasUpdateManager.UpdateSelection();
            canvasUpdateManager.UpdateCursor();
        }

        public void RemoveText(bool controlIsPressed = false)
        {
            currentLineManager.UpdateCurrentLine(cursorManager.LineNumber);

            if (textManager._IsReadonly)
                return;

            if (selectionManager.HasSelection)
            {
                DeleteSelection();
            }
            else
            {
                removeTextAction.HandleTextRemoval(controlIsPressed);
            }

            eventsManager.CallTextChanged();

            scrollManager.UpdateScrollToShowCursor();
            canvasUpdateManager.UpdateText();
            canvasUpdateManager.UpdateCursor();
        }
        public void AddNewLine()
        {
            if (textManager._IsReadonly)
                return;

            if (addNewLineTextAction.HandleEmptyDocument()) return;

            bool hasSelection = selectionManager.HasSelection;

            if (addNewLineTextAction.HandleFullTextSelection()) return;

            if (!hasSelection)
            {
                addNewLineTextAction.ApplyLineSplitWithIndentation();
            }
            else
            {
                addNewLineTextAction.ReplaceSelectionWithNewLine();
            }

            selectionManager.ForceClearSelection(canvasUpdateManager);
            if (!selectionManager.HasSelection &&
                cursorManager.LineNumber == textRenderer.NumberOfRenderedLines + textRenderer.NumberOfStartLine)
            {
                scrollManager.ScrollOneLineDown();
            }
            else
            {
                scrollManager.UpdateScrollToShowCursor();
            }

            eventsManager.CallTextChanged();
            canvasUpdateManager.UpdateAll();
        }
        public void DeleteText(bool controlIsPressed = false, bool shiftIsPressed = false)
        {
            currentLineManager.UpdateCurrentLine(cursorManager.LineNumber);

            if (textManager._IsReadonly)
                return;

            if (shiftIsPressed && !selectionManager.HasSelection)
            {
                deleteTextAction.DeleteCurrentLine();
            }
            else if (selectionManager.HasSelection)
            {
                DeleteSelection();
            }
            else
            {
                deleteTextAction.DeleteTextInLine(controlIsPressed);
            }

            eventsManager.CallTextChanged();
            scrollManager.UpdateScrollToShowCursor();
            canvasUpdateManager.UpdateText();
            canvasUpdateManager.UpdateCursor();
        }

        public void AddCharacter(string text, bool ignoreSelection = false)
        {
            if (textManager._IsReadonly)
                return;

            if (ignoreSelection)
                selectionManager.ForceClearSelection(canvasUpdateManager);

            int splittedTextLength = addCharacterTextAction.CalculateSplitTextLength(text);
            bool hasSelection = selectionManager.HasSelection;

            if (!hasSelection && splittedTextLength == 1) //add single line text -> no selection
            {
                addCharacterTextAction.HandleSingleCharacterWithoutSelection(text);
            }
            else if (!hasSelection && splittedTextLength > 1) //add multi line text -> no selection
            {
                addCharacterTextAction.HandleMultiLineTextWithoutSelection(text, splittedTextLength);
            }
            else if (selectionManager.HasSelection && text.Length == 0) //delete all text -> selection 
            {
                DeleteSelection();
            }
            else if (hasSelection) //add multiline text + selection
            {
                addCharacterTextAction.HandleTextWithSelection(text, splittedTextLength);
            }

            eventsManager.CallTextChanged();
            scrollManager.ScrollLineIntoViewIfOutside(cursorManager.LineNumber);
            canvasUpdateManager.UpdateText();
            canvasUpdateManager.UpdateCursor();
        }

        public bool DeleteLine(int line)
        {
            if (line >= textManager.LinesCount || line < 0)
                return false;

            if (line == longestLineManager.longestIndex)
                longestLineManager.needsRecalculation = true;

            undoRedo.RecordUndoAction(() =>
            {
                textManager.totalLines.RemoveAt(line);
            }, line, 2, 1);

            if (textManager.LinesCount == 0)
            {
                textManager.AddLine();
            }

            eventsManager.CallTextChanged();
            canvasUpdateManager.UpdateText();
            return true;
        }

        public bool AddLine(int line, string text)
        {
            if (line > textManager.LinesCount || line < 0)
                return false;

            if (text.Length > longestLineManager.longestLineLength)
                longestLineManager.longestIndex = line;

            undoRedo.RecordUndoAction(() =>
            {
                textManager.InsertOrAdd(line, stringManager.CleanUpString(text));

            }, line, 1, 2);

            eventsManager.CallTextChanged();
            canvasUpdateManager.UpdateText();
            return true;
        }

        public bool SetLineText(int line, string text)
        {
            if (line >= textManager.LinesCount || line < 0)
                return false;

            if (text.Length > longestLineManager.longestLineLength)
                longestLineManager.longestIndex = line;

            undoRedo.RecordUndoAction(() =>
            {
                textManager.SetLineText(line, stringManager.CleanUpString(text));
            }, line, 1, 1);

            eventsManager.CallTextChanged();
            canvasUpdateManager.UpdateText();
            return true;
        }

        public void DuplicateLine(int line)
        {
            undoRedo.RecordUndoAction(() =>
            {
                textManager.InsertOrAdd(line, textManager.GetLineText(line));
                cursorManager.LineNumber += 1;
            }, line, 1, 2);

            if (textRenderer.OutOfRenderedArea(line))
                scrollManager.ScrollBottomIntoView(false);

            eventsManager.CallTextChanged();
            canvasUpdateManager.UpdateAll();
        }
    }
}
