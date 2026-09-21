using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using TextControlBoxNS;

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

        [UITestMethod]
        public void SearchHighlight_MatchesSelectionVerticalPosition()
        {
            var core = TestHelper.MakeCoreTextbox();
            core.SetText("<Project Sdk=\"Microsoft.NET.Sdk\">\n  <PropertyGroup>");
            core.textRenderer.EnsureTextFormat();
            core.searchManager.BeginSearch("Project", false, false);

            var (startLine, linesToRender) = core.textRenderer.CalculateLinesToRender();
            var renderTextData = core.textManager.GetLinesForRendering(startLine, linesToRender);
            var drawnLayout = core.textLayoutManager.CreateTextResource(core.canvasText, null, core.textRenderer.TextFormat, renderTextData.Text, new Windows.Foundation.Size(500, 500));

            var matches = System.Text.RegularExpressions.Regex.Matches(renderTextData.Text, core.searchManager.searchParameter.SearchExpression);
            var match = matches[0];
            var layoutRegion = drawnLayout.GetCharacterRegions(match.Index, match.Length);

            float searchHighlightOffsetY = core.textRenderer.GetSelectionTopMargin();
            float selectionTopMargin = core.textRenderer.GetSelectionTopMargin();

            var highlightRect = TextControlBoxNS.Helper.Utils.CreateRect(layoutRegion[0].LayoutBounds, 0, searchHighlightOffsetY);
            var selectionRect = TextControlBoxNS.Helper.Utils.CreateRect(layoutRegion[0].LayoutBounds, 0, selectionTopMargin);

            Assert.AreEqual(selectionRect.Top, highlightRect.Top, "Search highlight Top must align with selection Top");
            Assert.AreEqual(selectionRect.Bottom, highlightRect.Bottom, "Search highlight Bottom must align with selection Bottom");
            Assert.AreEqual(core.textRenderer.TopInset, highlightRect.Top, "Line 0 highlight should sit at TopInset");
        }

        [UITestMethod]
        public void SearchDirection_ToggleNextAndPrevious_NavigatesImmediatelyInSingleStep()
        {
            var core = TestHelper.MakeCoreTextbox();
            core.SetText("word1 word2 word1 word3 word1");
            core.SetCursorPosition(0, 0);
            core.BeginSearch("word1", false, false);

            // 1st FindNext -> match 0 (index 0..5)
            var res1 = core.FindNext();
            Assert.AreEqual(SearchResult.Found, res1);
            Assert.AreEqual(0, core.selectionManager.selectionStart.CharacterPosition);
            Assert.AreEqual(5, core.selectionManager.selectionEnd.CharacterPosition);

            // 2nd FindNext -> match 1 (index 12..17)
            var res2 = core.FindNext();
            Assert.AreEqual(SearchResult.Found, res2);
            Assert.AreEqual(12, core.selectionManager.selectionStart.CharacterPosition);
            Assert.AreEqual(17, core.selectionManager.selectionEnd.CharacterPosition);

            // Switch direction to FindPrevious: MUST immediately jump to match 0 in ONE press
            var resPrev = core.FindPrevious();
            Assert.AreEqual(SearchResult.Found, resPrev);
            Assert.AreEqual(0, core.selectionManager.selectionStart.CharacterPosition, "FindPrevious after FindNext should immediately jump to previous occurrence");
            Assert.AreEqual(5, core.selectionManager.selectionEnd.CharacterPosition);

            // Switch direction back to FindNext: MUST immediately jump to match 1 in ONE press
            var resNextAgain = core.FindNext();
            Assert.AreEqual(SearchResult.Found, resNextAgain);
            Assert.AreEqual(12, core.selectionManager.selectionStart.CharacterPosition, "FindNext after FindPrevious should immediately jump to next occurrence");
            Assert.AreEqual(17, core.selectionManager.selectionEnd.CharacterPosition);
        }
    }
}
