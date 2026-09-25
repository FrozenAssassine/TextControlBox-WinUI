using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using TextControlBoxNS;
using TextControlBoxNS.Core;
using TextControlBoxNS.Helper;

namespace TextControlBox.Tests;

[TestClass]
public class EmojiAndSurrogateTests
{
    [TestMethod]
    public void TextElementHelper_GetNextTextElementLength_SingleSurrogatePair()
    {
        string text = "a😀b";
        // 'a' at 0 -> length 1
        Assert.AreEqual(1, TextElementHelper.GetNextTextElementLength(text, 0));
        // '😀' at 1 -> length 2
        Assert.AreEqual(2, TextElementHelper.GetNextTextElementLength(text, 1));
        // 'b' at 3 -> length 1
        Assert.AreEqual(1, TextElementHelper.GetNextTextElementLength(text, 3));
    }

    [TestMethod]
    public void TextElementHelper_GetPreviousTextElementLength_SingleSurrogatePair()
    {
        string text = "a😀b";
        // before 'a' (index 0) -> 0
        Assert.AreEqual(0, TextElementHelper.GetPreviousTextElementLength(text, 0));
        // after 'a' (index 1) -> 1
        Assert.AreEqual(1, TextElementHelper.GetPreviousTextElementLength(text, 1));
        // after '😀' (index 3) -> 2
        Assert.AreEqual(2, TextElementHelper.GetPreviousTextElementLength(text, 3));
        // after 'b' (index 4) -> 1
        Assert.AreEqual(1, TextElementHelper.GetPreviousTextElementLength(text, 4));
    }

    [TestMethod]
    public void TextElementHelper_SnapToTextElementStartAndEnd()
    {
        string text = "a😀b";
        // Index 2 is in the middle of '😀' (between high and low surrogate)
        Assert.AreEqual(1, TextElementHelper.SnapToTextElementStart(text, 2));
        Assert.AreEqual(3, TextElementHelper.SnapToTextElementEnd(text, 2));

        // Boundaries are untouched
        Assert.AreEqual(1, TextElementHelper.SnapToTextElementStart(text, 1));
        Assert.AreEqual(3, TextElementHelper.SnapToTextElementEnd(text, 3));
    }

    [TestMethod]
    public void TextElementHelper_ComplexEmoji_GraphemeCluster()
    {
        // Family emoji: 👨‍👩‍👧‍👦 (length 11 in UTF-16 code units)
        string family = "👨‍👩‍👧‍👦";
        Assert.AreEqual(11, family.Length);
        Assert.AreEqual(11, TextElementHelper.GetNextTextElementLength(family, 0));
        Assert.AreEqual(11, TextElementHelper.GetPreviousTextElementLength(family, 11));

        // Flag: 🇩🇪 (length 4)
        string flag = "🇩🇪";
        Assert.AreEqual(4, flag.Length);
        Assert.AreEqual(4, TextElementHelper.GetNextTextElementLength(flag, 0));
        Assert.AreEqual(4, TextElementHelper.GetPreviousTextElementLength(flag, 4));
    }

    [UITestMethod]
    public void CursorManager_MoveRight_AdvancesPastEmojiInSingleStep()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.LoadText("text 😀 more");
        // "text " is 5 chars. Emoji starts at index 5 and ends at index 7.
        coreTextbox.CursorPosition = new CursorPosition(5, 0);

        coreTextbox.cursorManager.MoveRight();

        // Cursor should be at 7 (past the emoji), NOT 6 (stuck in middle of emoji)
        Assert.AreEqual(7, coreTextbox.CursorPosition.CharacterPosition);
    }

    [UITestMethod]
    public void CursorManager_MoveLeft_MovesPastEmojiInSingleStep()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.LoadText("text 😀 more");
        // Emoji ends at index 7.
        coreTextbox.CursorPosition = new CursorPosition(7, 0);

        coreTextbox.cursorManager.MoveLeft();

        // Cursor should be at 5 (before the emoji), NOT 6 (stuck in middle of emoji)
        Assert.AreEqual(5, coreTextbox.CursorPosition.CharacterPosition);
    }

    [UITestMethod]
    public void CursorManager_FourEmojis_NavigateToLineEnd()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.LoadText("😀😁😂🤣"); // 4 emojis, each 2 chars => total 8 chars
        Assert.AreEqual(8, coreTextbox.GetText().Length);

        coreTextbox.CursorPosition = new CursorPosition(0, 0);

        coreTextbox.cursorManager.MoveRight();
        Assert.AreEqual(2, coreTextbox.CursorPosition.CharacterPosition);

        coreTextbox.cursorManager.MoveRight();
        Assert.AreEqual(4, coreTextbox.CursorPosition.CharacterPosition);

        coreTextbox.cursorManager.MoveRight();
        Assert.AreEqual(6, coreTextbox.CursorPosition.CharacterPosition);

        coreTextbox.cursorManager.MoveRight();
        // Crucial test: must reach 8 (after the 4th emoji, at text end)
        Assert.AreEqual(8, coreTextbox.CursorPosition.CharacterPosition);
    }

    [UITestMethod]
    public void TextRemoval_Backspace_DeletesEntireEmojiWithoutOrphanSurrogates()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.LoadText("text 😀");
        // "text " (5) + "😀" (2) = 7 chars
        coreTextbox.CursorPosition = new CursorPosition(7, 0);

        // Perform Backspace (without ctrl)
        coreTextbox.textActionManager.RemoveText(false);

        // Entire emoji must be deleted; "text " remaining (5 chars)
        Assert.AreEqual("text ", coreTextbox.GetText());
        Assert.AreEqual(5, coreTextbox.CursorPosition.CharacterPosition);

        // Ensure no invalid surrogate characters remain
        foreach (char c in coreTextbox.GetText())
        {
            Assert.IsFalse(char.IsSurrogate(c), "Orphan surrogate found in text after backspace!");
        }
    }

    [UITestMethod]
    public void TextDeletion_DeleteKey_DeletesEntireEmojiWithoutOrphanSurrogates()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.LoadText("text 😀 more");
        // Cursor placed at 5 (immediately before emoji)
        coreTextbox.CursorPosition = new CursorPosition(5, 0);

        // Perform Delete (without ctrl/shift)
        coreTextbox.textActionManager.DeleteText(false, false);

        // Entire emoji must be deleted; "text  more" remaining
        Assert.AreEqual("text  more", coreTextbox.GetText());
        Assert.AreEqual(5, coreTextbox.CursorPosition.CharacterPosition);

        // Ensure no invalid surrogate characters remain
        foreach (char c in coreTextbox.GetText())
        {
            Assert.IsFalse(char.IsSurrogate(c), "Orphan surrogate found in text after delete!");
        }
    }

    [UITestMethod]
    public void CurrentLineManager_AddText_DoesNotSplitEmoji()
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.LoadText("😀");
        // If someone accidentally passes position 1 (inside the emoji surrogate pair)
        coreTextbox.currentLineManager.AddText("X", 1);

        // AddText snaps to start of emoji so the surrogate pair is never split
        string line = coreTextbox.currentLineManager.CurrentLine;
        Assert.AreEqual("X😀", line);
    }
}
