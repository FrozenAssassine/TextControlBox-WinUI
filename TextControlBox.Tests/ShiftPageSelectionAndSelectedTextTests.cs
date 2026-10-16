using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using System.Linq;
using TextControlBoxNS;
using Windows.System;

namespace TextControlBox.Tests;

[TestClass]
public class ShiftPageSelectionAndSelectedTextTests
{
    [UITestMethod]
    public void SelectedText_WhenNoSelection_ReturnsEmptyString()
    {
        var textbox = TestHelper.MakeTextbox(10);
        textbox.ClearSelection();

        Assert.IsFalse(textbox.HasSelection);
        Assert.AreEqual(string.Empty, textbox.SelectedText);
    }

    [UITestMethod]
    public void SelectedText_SelfAssignment_WhenNoSelection_DoesNotDuplicateLineOrChangeText()
    {
        var textbox = TestHelper.MakeTextbox(5);
        textbox.ClearSelection();
        textbox.SetCursorPosition(2, 0);

        string originalText = textbox.GetText();
        int originalLineCount = textbox.NumberOfLines;

        // Perform self assignment without selection
        textbox.SelectedText = textbox.SelectedText;

        Assert.AreEqual(originalText, textbox.GetText(), "Text should not change after SelectedText = SelectedText with no selection");
        Assert.AreEqual(originalLineCount, textbox.NumberOfLines, "Line count should remain identical");
        Assert.IsFalse(textbox.HasSelection);
    }

    [UITestMethod]
    public void SelectedText_SetEmptyString_WhenSelectionExists_DeletesSelectedText()
    {
        var textbox = TestHelper.MakeTextbox(5);
        textbox.SelectLine(1);
        Assert.IsTrue(textbox.HasSelection);

        textbox.SelectedText = string.Empty;

        Assert.IsFalse(textbox.HasSelection);
        Assert.AreEqual(4, textbox.NumberOfLines);
    }

    [UITestMethod]
    public void SelectedText_SetNewText_WhenSelectionExists_ReplacesSelectedText()
    {
        var textbox = TestHelper.MakeTextbox(0);
        textbox.SetText("Hello World");
        textbox.SetSelection(0, 5); // Select "Hello"

        Assert.AreEqual("Hello", textbox.SelectedText);

        textbox.SelectedText = "Hi";

        Assert.AreEqual("Hi World", textbox.GetText());
    }

    [UITestMethod]
    public void ShiftPageDown_StartsSelection_AndExtendsDownwards()
    {
        var core = TestHelper.MakeCoreTextbox(100);
        core.textRenderer.NumberOfRenderedLines = 20;
        core.SetCursorPosition(0, 0);
        core.ClearSelection();

        Assert.IsFalse(core.selectionManager.HasSelection);

        // Simulate Shift + PageDown
        bool handled = core.HandleKeyDown(VirtualKey.PageDown, shift: true);

        Assert.IsTrue(handled);
        Assert.IsTrue(core.selectionManager.HasSelection, "HasSelection should be true after Shift + PageDown");
        Assert.IsNotNull(core.CurrentSelection);

        var selection = core.CurrentSelection.Value;
        Assert.AreEqual(0, selection.StartLinePos);
        Assert.AreEqual(0, selection.StartCharacterPos);
        Assert.AreEqual(20, selection.EndLinePos);
        Assert.AreEqual(core.CursorPosition.LineNumber, selection.EndLinePos);
    }

    [UITestMethod]
    public void ShiftPageDown_Repeated_ExtendsExistingSelectionFurther()
    {
        var core = TestHelper.MakeCoreTextbox(100);
        core.textRenderer.NumberOfRenderedLines = 20;
        core.SetCursorPosition(0, 0);
        core.ClearSelection();

        core.HandleKeyDown(VirtualKey.PageDown, shift: true);
        int firstEndLine = core.CurrentSelection!.Value.EndLinePos;
        Assert.AreEqual(20, firstEndLine);

        core.HandleKeyDown(VirtualKey.PageDown, shift: true);
        int secondEndLine = core.CurrentSelection!.Value.EndLinePos;
        Assert.AreEqual(40, secondEndLine);

        Assert.AreEqual(0, core.CurrentSelection.Value.StartLinePos);
        Assert.IsTrue(secondEndLine > firstEndLine, "Second Shift + PageDown should extend selection further down");
    }

    [UITestMethod]
    public void ShiftPageUp_FromMiddle_SelectsUpwards()
    {
        var core = TestHelper.MakeCoreTextbox(100);
        core.textRenderer.NumberOfRenderedLines = 20;
        core.SetCursorPosition(50, 0);
        core.ClearSelection();

        bool handled = core.HandleKeyDown(VirtualKey.PageUp, shift: true);

        Assert.IsTrue(handled);
        Assert.IsTrue(core.selectionManager.HasSelection);

        var selection = core.CurrentSelection!.Value;
        Assert.AreEqual(50, selection.StartLinePos);
        Assert.AreEqual(30, selection.EndLinePos);
        Assert.AreEqual(core.CursorPosition.LineNumber, selection.EndLinePos);
    }

    [UITestMethod]
    public void PageDown_WithoutShift_ClearsExistingSelection()
    {
        var core = TestHelper.MakeCoreTextbox(100);
        core.textRenderer.NumberOfRenderedLines = 20;
        core.SetCursorPosition(0, 0);

        // Start selection
        core.HandleKeyDown(VirtualKey.PageDown, shift: true);
        Assert.IsTrue(core.selectionManager.HasSelection);

        // Press PageDown without shift
        bool handled = core.HandleKeyDown(VirtualKey.PageDown, shift: false);

        Assert.IsTrue(handled);
        Assert.IsFalse(core.selectionManager.HasSelection, "PageDown without shift should clear selection");
        Assert.AreEqual(40, core.CursorPosition.LineNumber);
    }

    [UITestMethod]
    public void PageUp_WithoutShift_ClearsExistingSelection()
    {
        var core = TestHelper.MakeCoreTextbox(100);
        core.textRenderer.NumberOfRenderedLines = 20;
        core.SetCursorPosition(50, 0);

        // Start selection with Shift + PageDown
        core.HandleKeyDown(VirtualKey.PageDown, shift: true);
        Assert.IsTrue(core.selectionManager.HasSelection);

        // Press PageUp without shift
        bool handled = core.HandleKeyDown(VirtualKey.PageUp, shift: false);

        Assert.IsTrue(handled);
        Assert.IsFalse(core.selectionManager.HasSelection, "PageUp without shift should clear selection");
        Assert.AreEqual(50, core.CursorPosition.LineNumber);
    }

    [UITestMethod]
    public void ShiftArrowUp_ThenShiftArrowDown_CollapsesSelection()
    {
        var core = TestHelper.MakeCoreTextbox(10);
        core.SetCursorPosition(5, 3);
        core.ClearSelection();

        Assert.IsFalse(core.selectionManager.HasSelection);

        // Shift + Up selects line 4
        core.HandleKeyDown(VirtualKey.Up, shift: true);
        Assert.IsTrue(core.selectionManager.HasSelection);
        Assert.AreEqual(4, core.CursorPosition.LineNumber);

        // Shift + Down returns to start position, collapsing selection
        core.HandleKeyDown(VirtualKey.Down, shift: true);
        Assert.IsFalse(core.selectionManager.HasSelection, "Selection should collapse and HasSelection be false");
        Assert.AreEqual(5, core.CursorPosition.LineNumber);
        Assert.AreEqual(3, core.CursorPosition.CharacterPosition);
    }

    [UITestMethod]
    public void PasteLine_DuplicatesLineBelow_WithoutSplittingLine()
    {
        var core = TestHelper.MakeCoreTextbox(0);
        core.LoadLines(new[] { "Line 0", "Line 1: Hello World", "Line 2" });
        core.SetCursorPosition(1, 5); // Right after "Hello" in line 1
        core.ClearSelection();

        core.textActionManager.PasteLine("Line 1: Hello World\r\n");

        Assert.AreEqual(4, core.NumberOfLines);
        Assert.AreEqual("Line 0", core.GetLineText(0));
        Assert.AreEqual("Line 1: Hello World", core.GetLineText(1));
        Assert.AreEqual("Line 1: Hello World", core.GetLineText(2));
        Assert.AreEqual("Line 2", core.GetLineText(3));

        // Cursor should now be on the duplicated line below at the same character column
        Assert.AreEqual(2, core.CursorPosition.LineNumber);
        Assert.AreEqual(5, core.CursorPosition.CharacterPosition);

        // Undo should restore original lines and cursor
        core.Undo();
        Assert.AreEqual(3, core.NumberOfLines);
        Assert.AreEqual("Line 1: Hello World", core.GetLineText(1));
        Assert.AreEqual(1, core.CursorPosition.LineNumber);
        Assert.AreEqual(5, core.CursorPosition.CharacterPosition);
    }
}
