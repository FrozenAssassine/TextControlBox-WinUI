using Microsoft.UI.Xaml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using System;
using TextControlBoxNS;

namespace TextControlBox.Tests;

[TestClass]
public class PublicApiRegressionTests
{
    [UITestMethod]
    public void SelectLine_OutOfRange_ReturnsFalse_AndDoesNotThrow()
    {
        var tb = TestHelper.MakeTextbox(3);

        Assert.IsFalse(tb.SelectLine(-1));
        Assert.IsFalse(tb.SelectLine(9999));
    }

    [UITestMethod]
    public void SelectLines_OutOfRange_ReturnsFalse_AndDoesNotThrow()
    {
        var tb = TestHelper.MakeTextbox(5);

        Assert.IsFalse(tb.SelectLines(-1, 1));
        Assert.IsFalse(tb.SelectLines(0, -1));
        Assert.IsFalse(tb.SelectLines(4, 10));
        Assert.IsFalse(tb.SelectLines(999, 1));
    }

    [UITestMethod]
    public void GoToLine_InvalidValues_DoNotChangeCursor()
    {
        var tb = TestHelper.MakeTextbox(5);
        tb.SetCursorPosition(2, 3, scrollIntoView: false);

        var before = tb.CursorPosition;

        tb.GoToLine(-1);
        Assert.AreEqual(before.LineNumber, tb.CursorPosition.LineNumber);
        Assert.AreEqual(before.CharacterPosition, tb.CursorPosition.CharacterPosition);

        tb.GoToLine(9999);
        Assert.AreEqual(before.LineNumber, tb.CursorPosition.LineNumber);
        Assert.AreEqual(before.CharacterPosition, tb.CursorPosition.CharacterPosition);
    }

    [UITestMethod]
    public void UndoRedoEnabled_DisableClearsHistory_AndPreventsRecording()
    {
        var tb = TestHelper.MakeTextbox(0);
        tb.LoadText("Hello");

        tb.SetCursorPosition(0, 5, scrollIntoView: false);
        tb.SetText("Hello World");

        Assert.IsTrue(tb.CanUndo, "Expected undo to be available after SetText.");

        tb.UndoRedoEnabled = false;
        Assert.IsFalse(tb.CanUndo);
        Assert.IsFalse(tb.CanRedo);

        tb.SetText("Hello World!!!");
        Assert.IsFalse(tb.CanUndo, "Undo should not be recorded while UndoRedoEnabled is false.");

        tb.UndoRedoEnabled = true;
        tb.SetText("abc");
        Assert.IsTrue(tb.CanUndo, "Undo should be recorded again after re-enabling.");
    }

    [UITestMethod]
    public void SelectionScrollStartBorderDistance_Setter_SetsValue()
    {
        var tb = TestHelper.MakeTextbox(0);

        var value = new Thickness(1, 2, 3, 4);
        tb.SelectionScrollStartBorderDistance = value;

        // Regression test for accidental self-assignment in wrapper property.
        Assert.AreEqual(value.Left, tb.SelectionScrollStartBorderDistance.Left);
        Assert.AreEqual(value.Top, tb.SelectionScrollStartBorderDistance.Top);
        Assert.AreEqual(value.Right, tb.SelectionScrollStartBorderDistance.Right);
        Assert.AreEqual(value.Bottom, tb.SelectionScrollStartBorderDistance.Bottom);
    }

    [UITestMethod]
    public void RewriteTabsSpaces_ReadOnly_IgnoreFlag_AllowsRewrite()
    {
        var tb = TestHelper.MakeTextbox(0);
        tb.LineEnding = LineEnding.CR;        
        tb.LoadText("\tLine1\n\tLine2");
        tb.IsReadOnly = true;
        
        // In readonly mode rewrite should only work when ignoreIsReadOnly is true.
        tb.RewriteTabsSpaces(4, useSpacesInsteadTabs: true, ignoreIsReadOnly: true);

        var text = tb.GetText();
        Assert.IsTrue(text.StartsWith("    Line1", StringComparison.Ordinal));
        Assert.IsTrue(text.Contains("\n    Line2", StringComparison.Ordinal));
    }
}
