using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using TextControlBoxNS;

namespace TextControlBox.Tests.RegressionTests
{
    [TestClass]
    public class UndoRedoRegressionTests
    {
        [UITestMethod]
        public void UndoRedo_MoveTabBack_NoSelection_StartLineInvalid()
        {
            // Regression test for GetLinesAsString(-1, ...) exception
            var textbox = TestHelper.MakeCoreTextbox(0);
            
            // Setup text with indentation
            textbox.LoadText("    Line 1"); // 4 spaces
            textbox.SetCursorPosition(0, 4); // Line 0, Col 4 (after spaces)
            textbox.ClearSelection();

            // Verify initial state
            Assert.AreEqual("    Line 1", textbox.GetText());
             
            // Action: Shift+Tab behavior (MoveTabBack)
            // This calls MoveTabBackSingleLine internally
            textbox.tabSpaceManager.MoveTabBack();

            // Verification 1: Text should be unindented (4 spaces removed)
            Assert.AreEqual("Line 1", textbox.GetText());

            // Verification 2: Undo should work and not crash
            Assert.IsTrue(textbox.CanUndo);
            textbox.Undo();

            // Verification 3: Text should be back to original
            Assert.AreEqual("    Line 1", textbox.GetText());
        }
    }
}
