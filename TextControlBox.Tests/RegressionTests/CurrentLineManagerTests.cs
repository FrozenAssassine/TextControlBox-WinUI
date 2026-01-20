using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;

namespace TextControlBox.Tests.RegressionTests;

[TestClass]
public class CurrentLineManagerTests
{
    [UITestMethod]
    public void GetCurrentLineText_ReturnsCorrectContent()
    {
        var core = TestHelper.MakeCoreTextbox();
        core.LoadLines([ "Line 0", "Line 1", "Line 2" ]);

        // Move cursor to line 1
        core.currentLineManager.UpdateCurrentLine(1);

        Assert.AreEqual("Line 1", core.currentLineManager.CurrentLine);
    }

    [UITestMethod]
    public void SetCurrentLine_UpdatesTextManager()
    {
        var core = TestHelper.MakeCoreTextbox();
        core.LoadLines(["Original"]);
        core.currentLineManager.UpdateCurrentLine(0);

        core.currentLineManager.CurrentLine = "Updated";

        Assert.AreEqual("Updated", core.textManager.totalLines[0]);
    }

    [UITestMethod]
    public void AddToEnd_AppendsTextCorrectly()
    {
        var core = TestHelper.MakeCoreTextbox();
        core.LoadLines(["Hello"]);

        core.currentLineManager.UpdateCurrentLine(0);

        core.currentLineManager.AddToEnd(" World");

        Assert.AreEqual("Hello World", core.currentLineManager.CurrentLine);
    }

    [UITestMethod]
    public void AddText_InsertInMiddle_WorksCorrectly()
    {
        var core = TestHelper.MakeCoreTextbox();
        core.LoadLines(["ac"]);
        core.currentLineManager.UpdateCurrentLine(0);

        // Insert 'b' at index 1
        core.currentLineManager.AddText("b", 1);

        Assert.AreEqual("abc", core.currentLineManager.CurrentLine);
    }

    [UITestMethod]
    public void AddText_PositionOutOfBounds_AppendsToEnd()
    {
        var core = TestHelper.MakeCoreTextbox();
        core.LoadLines(["Base"]);
        core.currentLineManager.UpdateCurrentLine(0);

        // Position 100 is way beyond "Base" length (4)
        core.currentLineManager.AddText("Plus", 100);

        Assert.AreEqual("BasePlus", core.currentLineManager.CurrentLine);
    }

    [UITestMethod]
    public void AddText_NegativePosition_InsertsAtStart()
    {
        var core = TestHelper.MakeCoreTextbox();
        core.LoadLines(["End"]);
        core.currentLineManager.UpdateCurrentLine(0);

        core.currentLineManager.AddText("Start", -5);

        // Your code sets position to 0 if < 0, then uses .Insert(0, add)
        Assert.AreEqual("StartEnd", core.currentLineManager.CurrentLine);
    }

    [UITestMethod]
    public void Length_ReturnsCorrectValue()
    {
        var core = TestHelper.MakeCoreTextbox();
        string testText = "CountMe";
        core.textManager.SetLineText(0, testText);
        core.currentLineManager.UpdateCurrentLine(0);

        Assert.AreEqual(testText.Length, core.currentLineManager.Length);
    }

    [UITestMethod]
    public void Length_EmptyLines_ReturnsZero()
    {
        var core = TestHelper.MakeCoreTextbox();
        core.textManager.totalLines.Clear(); // No lines at all

        Assert.AreEqual(0, core.currentLineManager.Length);
    }
}