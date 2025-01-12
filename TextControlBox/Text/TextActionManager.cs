using Microsoft.UI.Input;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextControlBoxNS.Helper;
using TextControlBoxNS.Renderer;
using Windows.ApplicationModel.DataTransfer;

namespace TextControlBoxNS.Text
{
    internal class TextActionManager
    {
        public CanvasHelper canvasHelper;
        private TextManager textManager;
        private SelectionRenderer selectionRenderer;
        private SelectionManager selectionManager;
        private CursorManager cursorManager;
        private TextControlBox textbox;
        private UndoRedo undoRedo;
        public TextActionManager(TextControlBox textbox, UndoRedo undoRedo, CanvasHelper canvasHelper, TextManager textManager, SelectionRenderer selectionRenderer, CursorManager cursorManager)
        {
            this.canvasHelper = canvasHelper;
            this.textManager = textManager;
            this.selectionRenderer = selectionRenderer;
            this.cursorManager = cursorManager;
            this.textbox = textbox;
        }

        public void SelectAll()
        {
            //No selection can be shown
            if (textManager.LinesCount== 1 && textManager.GetLineLength(0) == 0)
                return;

            selectionRenderer.SetSelection(new CursorPosition(0, 0), new CursorPosition(textManager.GetLineLength(-1), textManager.LinesCount- 1));
             = selectionRenderer.SelectionEndPosition;
            canvasHelper.UpdateSelection();
            canvasHelper.UpdateCursor();
        }
        public void Undo()
        {
            if (textbox.IsReadonly || !undoRedo.CanUndo)
                return;

            //Do the Undo
            textbox.ChangeCursor(InputSystemCursorShape.Wait);
            var sel = undoRedo.Undo(stringManager, NewLineCharacter);
            Internal_TextChanged();
            ChangeCursor(InputSystemCursorShape.IBeam);

            NeedsRecalculateLongestLineIndex = true;

            if (sel != null)
            {
                //only set cursorposition
                if (sel.StartPosition != null && sel.EndPosition == null)
                {
                    cursorManager.SetCursorPosition(sel.StartPosition);
                    canvasHelper.UpdateAll();
                    return;
                }

                selectionRenderer.SetSelection(sel);
                cursorManager.SetCursorPosition(sel.EndPosition);
            }
            else
                ForceClearSelection();
            canvasHelper.UpdateAll();
        }
        public void Redo()
        {
            if (textbox.IsReadonly || !undoRedo.CanRedo)
                return;

            //Do the Redo
            ChangeCursor(InputSystemCursorShape.Wait);
            var sel = undoRedo.Redo(TotalLines, stringManager, NewLineCharacter);
            Internal_TextChanged();
            ChangeCursor(InputSystemCursorShape.IBeam);

            NeedsRecalculateLongestLineIndex = true;

            if (sel != null)
            {
                //only set cursorposition
                if (sel.StartPosition != null && sel.EndPosition == null)
                {
                    CursorPosition = sel.StartPosition;
                    canvasHelper.UpdateAll();
                    return;
                }

                selectionrenderer.SetSelection(sel);
                CursorPosition = sel.EndPosition;
            }
            else
                ForceClearSelection();
            canvasHelper.UpdateAll();
        }

        public void ForceClearSelection()
        {
            selectionRenderer.ClearSelection();
            selectionManager.SetCurrentTextSelection(null);
            canvasHelper.UpdateSelection();
        }
        //Trys running the code and clears the memory if OutOfMemoryException gets thrown
        private async void Safe_Paste(bool handleException = true)
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
                    CleanUp();
                    Safe_Paste(false);
                    return;
                }
                throw new OutOfMemoryException();
            }
        }
        private string Safe_Gettext(bool handleException = true)
        {
            try
            {
                return textManager.totalLines.GetString(NewLineCharacter);
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
        private void Safe_Cut(bool handleException = true)
        {
            try
            {
                DataPackage dataPackage = new DataPackage();
                dataPackage.SetText(SelectedText);
                if (TextSelection == null)
                    DeleteLine(CursorPosition.LineNumber); //Delete the line
                else
                    DeleteText(); //Delete the selected text

                dataPackage.RequestedOperation = DataPackageOperation.Move;
                Clipboard.SetContent(dataPackage);
                ClearSelection();
            }
            catch (OutOfMemoryException)
            {
                if (handleException)
                {
                    CleanUp();
                    Safe_Cut(false);
                    return;
                }
                throw new OutOfMemoryException();
            }
        }
        private void Safe_Copy(bool handleException = true)
        {
            try
            {
                DataPackage dataPackage = new DataPackage();
                dataPackage.SetText(SelectedText);
                dataPackage.RequestedOperation = DataPackageOperation.Copy;
                Clipboard.SetContent(dataPackage);
            }
            catch (OutOfMemoryException)
            {
                if (handleException)
                {
                    CleanUp();
                    Safe_Copy(false);
                    return;
                }
                throw new OutOfMemoryException();
            }
        }
        private void Safe_LoadLines(IEnumerable<string> lines, LineEnding lineEnding = LineEnding.CRLF, bool HandleException = true)
        {
            try
            {
                selectionrenderer.ClearSelection();
                undoRedo.ClearAll();

                ListHelper.Clear(textManager.totalLines);
                textManager.totalLines.AddRange(lines);

                this.LineEnding = lineEnding;

                NeedsRecalculateLongestLineIndex = true;
                canvasHelper.UpdateAll();
            }
            catch (OutOfMemoryException)
            {
                if (HandleException)
                {
                    CleanUp();
                    Safe_LoadLines(lines, lineEnding, false);
                    return;
                }
                throw new OutOfMemoryException();
            }
        }
        private async void Safe_LoadText(string text, bool handleException = true)
        {
            try
            {
                if (await Utils.IsOverTextLimit(text.Length))
                    return;

                //Get the LineEnding
                LineEnding = LineEndings.FindLineEnding(text);

                selectionrenderer.ClearSelection();
                undoRedo.ClearAll();

                NeedsRecalculateLongestLineIndex = true;

                if (text.Length == 0)
                    textManager.ClearText(true);
                else
                    selectionManager.ReplaceLines(0, textManager.LinesCount, stringManager.CleanUpString(text).Split(NewLineCharacter));

                canvasHelper.UpdateAll();
            }
            catch (OutOfMemoryException)
            {
                if (handleException)
                {
                    CleanUp();
                    Safe_LoadText(text, false);
                    return;
                }
                throw new OutOfMemoryException();
            }
        }
        private async void Safe_SetText(string text, bool handleException = true)
        {
            try
            {
                if (await Utils.IsOverTextLimit(text.Length))
                    return;

                selectionrenderer.ClearSelection();
                NeedsRecalculateLongestLineIndex = true;
                undoRedo.RecordUndoAction(() =>
                {
                    SelectionManager.ReplaceLines(textManager.totalLines, 0, textManager.LinesCount, stringManager.CleanUpString(text).Split(NewLineCharacter));
                    if (text.Length == 0) //Create a new line when the text gets cleared
                    {
                        textManager.totalLines.AddLine();
                    }
                }, textManager.totalLines, 0, textManager.LinesCount, Utils.CountLines(text, NewLineCharacter), NewLineCharacter);

                canvasHelper.UpdateAll();
            }
            catch (OutOfMemoryException)
            {
                if (handleException)
                {
                    CleanUp();
                    Safe_SetText(text, false);
                    return;
                }
                throw new OutOfMemoryException();
            }
        }
    }
}
