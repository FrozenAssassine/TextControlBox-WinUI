using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;

namespace TextControlBox.Tests.RegressionTests
{
    [TestClass]
    public class CursorMovementTests
    {
        [UITestMethod]
        public void MoveDown_DoesNotGoPastLastLine()
        {
            var core = TestHelper.MakeCoreTextbox();
            core.SetText("Line1\nLine2\nLine3");

            int last = core.textManager.LinesCount - 1;
            core.SetCursorPosition(last, 0);

            // try moving down multiple times
            core.cursorManager.MoveDown();
            core.cursorManager.MoveDown();

            Assert.AreEqual(last, core.cursorManager.LineNumber, "Cursor should not move past the last line");
        }

        [UITestMethod]
        public void MoveLeft_FromEmptyLine_DoesNotBecomeNegative()
        {
            var core = TestHelper.MakeCoreTextbox();

            // create a known multiline text with an empty line in the middle
            core.SetText("LongLineHere12345\n\nAnotherLine");

            // position cursor on the first (long) line beyond the empty line length
            core.SetCursorPosition(0, 15);

            // move down onto the empty line
            core.cursorManager.MoveDown();

            // perform a left move which previously could set CharacterPosition to -1 for empty lines
            core.cursorManager.MoveLeft();

            int charPos = core.cursorManager.CharacterPosition;
            int lineLen = core.textManager.GetLineLength(core.cursorManager.LineNumber);

            Assert.IsTrue(charPos >= 0, "CharacterPosition must not be negative after moving left on an empty line");
            Assert.IsTrue(charPos <= lineLen, "CharacterPosition must be within line bounds after moving left");
        }

        [UITestMethod]
        public void MoveRight_FromLineEnd_MovesToNextLineStart()
        {
            var core = TestHelper.MakeCoreTextbox();
            core.SetText("Hi\nWorld");

            // put cursor at end of first line
            core.SetCursorPosition(0, core.textManager.GetLineLength(0));

            core.cursorManager.MoveRight();

            Assert.AreEqual(1, core.cursorManager.LineNumber, "Cursor should move to the next line");
            Assert.AreEqual(0, core.cursorManager.CharacterPosition, "Cursor should be at start of the next line after moving right from line end");
        }
    }
}
