using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using System.Linq;
using TextControlBoxNS.Core;
using Windows.Foundation;

namespace TextControlBox.Tests.RegressionTests;

[TestClass]
public class FinalReviewTests
{
    private static CoreTextControlBox MakeCore(string text)
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText(text);
        core.ClearUndoRedoHistory();
        return core;
    }

    private static Point PointAtCharacter(CoreTextControlBox core, int line, int character)
    {
        core.textRenderer.EnsureTextFormat();
        core.textRenderer.CalculateLinesToRender();
        core.SetCursorPosition(line, 0, scrollIntoView: false);
        core.textRenderer.UpdateCurrentLineTextLayout(core.canvasText);
        var caret = core.textRenderer.CurrentLineTextLayout.GetCaretPosition(character, false);
        float y = core.textRenderer.TopInset + core.textRenderer.SingleLineHeight * (line + 0.5f);
        return new Point(caret.X + 2, y);
    }

    //A cursor with trailing affinity sits behind the character at CharacterPosition. Backspace removed
    //that character but moved the cursor one further to the left.
    [UITestMethod]
    public void Backspace_WithTrailingCursor_KeepsCursorAtTheRemovedCharacter()
    {
        var core = MakeCore("abcdef");
        core.cursorManager.SetCursorPosition(0, 2);
        core.cursorManager.IsTrailing = true;

        core.textActionManager.RemoveText();

        Assert.AreEqual("abdef", core.GetText());
        Assert.AreEqual(2, core.cursorManager.GetCurPosInLine());
    }

    //Enter at the start of an indented line added the indentation in front of the existing one.
    [UITestMethod]
    public void Enter_AtStartOfIndentedLine_KeepsIndentation()
    {
        var core = MakeCore("    foo");
        core.UseSpacesInsteadTabs = true;
        core.NumberOfSpacesForTab = 4;
        core.SetCursorPosition(0, 0, scrollIntoView: false);

        core.textActionManager.AddNewLine();

        Assert.AreEqual("", core.GetLineText(0));
        Assert.AreEqual("    foo", core.GetLineText(1));
    }

    [UITestMethod]
    public void Enter_InsideIndentation_KeepsIndentation()
    {
        var core = MakeCore("    foo");
        core.UseSpacesInsteadTabs = true;
        core.NumberOfSpacesForTab = 4;
        core.SetCursorPosition(0, 2, scrollIntoView: false);

        core.textActionManager.AddNewLine();

        Assert.AreEqual("    foo", core.GetLineText(1));
    }

    //The release of the double tap reset the tap counter, so the third tap never selected the line.
    [UITestMethod]
    public void TouchTripleTap_SelectsLine()
    {
        var core = MakeCore("hello world test\nsecond line");
        var point = PointAtCharacter(core, 0, 8);

        for (int i = 0; i < 3; i++)
        {
            core.pointerActionsManager.HandleTouchPressed(core, pointerId: 1, point);
            core.pointerActionsManager.HandleTouchReleased(pointerId: 1, point);
        }

        Assert.AreEqual(core.stringManager.CleanUpString("hello world test\n"), core.SelectedText);
    }

    [UITestMethod]
    public void PageDown_IntoEmojiLine_DoesNotSplitSurrogatePair()
    {
        var core = MakeCore("abc\n\U0001F600\U0001F600");
        core.SetCursorPosition(0, 3, scrollIntoView: false);

        core.ScrollPageDown();

        Assert.AreEqual(1, core.cursorManager.LineNumber);
        Assert.AreEqual(0, core.cursorManager.CharacterPosition % 2, "cursor is inside a surrogate pair");
    }

    [UITestMethod]
    public void PageUp_IntoEmojiLine_DoesNotSplitSurrogatePair()
    {
        var core = MakeCore("\U0001F600\U0001F600\nabc");
        core.SetCursorPosition(1, 3, scrollIntoView: false);

        core.ScrollPageUp();

        Assert.AreEqual(0, core.cursorManager.LineNumber);
        Assert.AreEqual(0, core.cursorManager.CharacterPosition % 2, "cursor is inside a surrogate pair");
    }

    //Losing the pointer capture between press and the first move (window switch, popup) left the pending
    //cursor placement active, so the next mouse hover without any button started a selection.
    [UITestMethod]
    public void MouseHover_AfterCaptureLost_DoesNotSelect()
    {
        var core = MakeCore("hello world\nsecond line");
        var press = PointAtCharacter(core, 0, 2);
        var hover1 = PointAtCharacter(core, 0, 6);
        var hover2 = PointAtCharacter(core, 1, 4);

        core.pointerActionsManager.PointerPressedAction(core, press, leftButtonPressed: true, rightButtonPressed: false);
        core.pointerActionsManager.HandleTouchCanceled();

        core.pointerActionsManager.PointerMovedAction(hover1);
        core.pointerActionsManager.PointerMovedAction(hover2);

        Assert.IsFalse(core.selectionManager.HasSelection);
        Assert.IsFalse(core.selectionManager.IsSelecting);
    }

    //A missed release (the button is not pressed anymore) must end the drag selection.
    [UITestMethod]
    public void MouseMove_WithoutPressedButton_EndsDragSelection()
    {
        var core = MakeCore("hello world\nsecond line");
        var press = PointAtCharacter(core, 0, 2);
        var drag = PointAtCharacter(core, 0, 6);
        var hover = PointAtCharacter(core, 1, 4);

        core.pointerActionsManager.PointerPressedAction(core, press, leftButtonPressed: true, rightButtonPressed: false);
        core.pointerActionsManager.PointerMovedAction(drag);
        core.pointerActionsManager.PointerMovedAction(drag);
        Assert.IsTrue(core.selectionManager.HasSelection);
        var selectedWhileDragging = core.SelectedText;

        core.pointerActionsManager.PointerMovedAction(hover, leftButtonPressed: false);
        core.pointerActionsManager.PointerMovedAction(hover, leftButtonPressed: false);

        Assert.IsFalse(core.selectionManager.IsSelecting);
        Assert.AreEqual(selectedWhileDragging, core.SelectedText);
    }

    //A second finger during a touch word selection turned it into a pinch, and IsSelecting stayed true:
    //arrow keys did not clear the selection anymore and a later mouse hover extended it.
    [UITestMethod]
    public void PinchDuringTouchSelection_EndsTheSelectionGesture()
    {
        var core = MakeCore("hello world test");
        var finger1 = PointAtCharacter(core, 0, 8);
        var finger2 = new Point(finger1.X + 80, finger1.Y + 80);

        core.pointerActionsManager.HandleTouchPressed(core, pointerId: 1, finger1);
        core.pointerActionsManager.HandleTouchReleased(pointerId: 1, finger1);
        core.pointerActionsManager.HandleTouchPressed(core, pointerId: 1, finger1);
        Assert.IsTrue(core.selectionManager.IsSelecting);

        core.pointerActionsManager.HandleTouchPressed(core, pointerId: 2, finger2);
        core.pointerActionsManager.HandleTouchReleased(pointerId: 2, finger2);
        core.pointerActionsManager.HandleTouchReleased(pointerId: 1, finger1);

        Assert.IsFalse(core.selectionManager.IsSelecting);
        core.HandleKeyDown(Windows.System.VirtualKey.Left);
        Assert.IsFalse(core.selectionManager.HasSelection);
    }

    //ReplaceNext did not unify the line endings of the replacement, a "\n" in a CRLF document ended up
    //inside a single line.
    [UITestMethod]
    public void ReplaceNext_WithForeignLineEnding_SplitsIntoLines()
    {
        var core = MakeCore("foo bar foo");
        core.LineEnding = TextControlBoxNS.LineEnding.CRLF;
        core.BeginSearch("bar", false, false);
        core.SetCursorPosition(0, 0, scrollIntoView: false);

        core.ReplaceNext("x\ny");

        Assert.AreEqual(2, core.NumberOfLines);
        Assert.IsFalse(core.Lines.Any(l => l.Contains('\n') || l.Contains('\r')));
        Assert.AreEqual("foo x\r\ny foo", core.GetText());
    }

    [UITestMethod]
    public void Search_FindsMatchesAddedByLaterEdits()
    {
        var core = MakeCore("zzz\nnothing\nnothing");
        core.BeginSearch("zzz", false, false);

        core.SetLineText(2, "zzz again");
        core.SetCursorPosition(1, 0, scrollIntoView: false);
        var result = core.FindNext();

        Assert.AreEqual(TextControlBoxNS.SearchResult.Found, result);
        Assert.AreEqual(2, core.CursorPosition.LineNumber);
    }

    //AddLines took lines with line breaks inside, which broke the line model.
    [UITestMethod]
    public void AddLines_WithLineBreakInsideALine_Throws()
    {
        var core = MakeCore("a\nb");
        Assert.ThrowsExactly<System.ArgumentException>(() => core.AddLines(1, ["x\ny"]));
        Assert.AreEqual(2, core.NumberOfLines);
    }

    [UITestMethod]
    public void AddLines_Null_ReturnsFalse()
    {
        var core = MakeCore("a\nb");
        Assert.IsFalse(core.AddLines(1, null));
    }

    [UITestMethod]
    public void CtrlLeftRight_UsesTheGivenModifier()
    {
        var core = MakeCore("hello world test");
        core.SetCursorPosition(0, 0, scrollIntoView: false);

        core.HandleKeyDown(Windows.System.VirtualKey.Right, ctrl: true);
        Assert.AreEqual(5, core.CursorPosition.CharacterPosition);

        core.HandleKeyDown(Windows.System.VirtualKey.Left, ctrl: true);
        Assert.AreEqual(0, core.CursorPosition.CharacterPosition);
    }

    //Escape without a selection has nothing to do and must reach the host (closing a search box, a dialog).
    [UITestMethod]
    public void Escape_WithoutSelection_IsNotHandled()
    {
        var core = MakeCore("hello");
        Assert.IsFalse(core.HandleKeyDown(Windows.System.VirtualKey.Escape));

        core.SetSelection(0, 0, 0, 3);
        Assert.IsTrue(core.HandleKeyDown(Windows.System.VirtualKey.Escape));
        Assert.IsFalse(core.HasSelection);
    }

    [UITestMethod]
    [DataRow(5, -3)]
    [DataRow(-10, 3)]
    [DataRow(-10, -3)]
    [DataRow(15, -10)]
    [DataRow(0, int.MinValue)]
    [DataRow(int.MaxValue, 5)]
    [DataRow(3, int.MaxValue)]
    [DataRow(int.MaxValue, int.MaxValue)]
    public void SetSelection_WithInvalidRange_DoesNotThrow(int start, int length)
    {
        var core = MakeCore("hello world\nsecond line\nthird");
        core.SetSelection(start, length);

        _ = core.SelectedText;
        _ = core.CalculateSelectionPosition();
        core.textActionManager.AddCharacter("x");
        Assert.IsTrue(core.NumberOfLines >= 1);
    }

    //Shift+click anchored the selection at the last mouse click, also when the cursor was moved with the keyboard since then.
    [UITestMethod]
    public void ShiftClick_AfterKeyboardNavigation_AnchorsAtTheCursor()
    {
        var core = MakeCore("hello world\nsecond line\nthird line");
        var click = PointAtCharacter(core, 0, 2);
        core.pointerActionsManager.PointerPressedAction(core, click, leftButtonPressed: true, rightButtonPressed: false);
        core.pointerActionsManager.PointerReleasedAction(click);

        core.HandleKeyDown(Windows.System.VirtualKey.Down);
        core.HandleKeyDown(Windows.System.VirtualKey.Right);
        Assert.AreEqual(1, core.CursorPosition.LineNumber);
        Assert.AreEqual(3, core.CursorPosition.CharacterPosition);

        var shiftClick = PointAtCharacter(core, 2, 5);
        core.SetCursorPosition(1, 3, scrollIntoView: false);
        core.pointerActionsManager.PointerPressedAction(core, shiftClick, leftButtonPressed: true, rightButtonPressed: false, shiftPressed: true);

        var sel = core.selectionManager.OrderTextSelectionSeparated();
        Assert.AreEqual(1, sel.startLine);
        Assert.AreEqual(3, sel.startChar);
        Assert.AreEqual(2, sel.endLine);
    }
}
