using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using TextControlBoxNS.Controls;
using TextControlBoxNS.Core;

namespace TextControlBox.Tests;

[TestClass]
public class ImeCompositionTests
{
    [UITestMethod]
    public void InputHandler_NormalTyping_InvokesTextEnteredImmediately()
    {
        var input = new InputHandlerControl();
        bool textEnteredCalled = false;
        input.TextEntered += (s, e) =>
        {
            textEnteredCalled = true;
        };

        Assert.IsFalse(input.IsComposing);
        input.SimulateUserInput("a");

        Assert.IsTrue(textEnteredCalled);
        Assert.IsFalse(input.IsComposing);
    }

    [UITestMethod]
    public void InputHandler_DuringComposition_SuppressesTextEntered()
    {
        var input = new InputHandlerControl();
        bool textEnteredCalled = false;
        input.TextEntered += (s, e) =>
        {
            textEnteredCalled = true;
        };

        // Simulate IME composition start (e.g. typing Romaji or dead key)
        input.SimulateCompositionStart();
        Assert.IsTrue(input.IsComposing);

        // Intermediate keystroke during composition (e.g. "k" before "ka")
        input.SimulateUserInput("k");

        // Must NOT fire TextEntered while composing, so Text is not wiped
        Assert.IsFalse(textEnteredCalled);
        Assert.IsTrue(input.IsComposing);
    }

    [UITestMethod]
    public void InputHandler_CompositionEnded_InvokesTextEnteredWithFinalText()
    {
        var input = new InputHandlerControl();
        string? enteredText = null;
        input.TextEntered += (s, e) =>
        {
            enteredText = input.Text;
        };

        input.SimulateCompositionStart();
        Assert.IsTrue(input.IsComposing);

        // Composition completes (e.g. Kanji "東京" or Dead Key "ê")
        input.SimulateCompositionEnd("東京");

        Assert.IsFalse(input.IsComposing);
        Assert.AreEqual("東京", enteredText);
    }

    [UITestMethod]
    public void InputHandler_CompositionCancelled_DoesNotInvokeTextEntered()
    {
        var input = new InputHandlerControl();
        bool textEnteredCalled = false;
        input.TextEntered += (s, e) =>
        {
            textEnteredCalled = true;
        };

        input.SimulateCompositionStart();
        Assert.IsTrue(input.IsComposing);

        // Cancelled composition (e.g. user pressed Escape)
        input.SimulateCompositionEnd("");

        Assert.IsFalse(input.IsComposing);
        Assert.IsFalse(textEnteredCalled);
    }

    //Clearing the box after every character made the text services resync each time, which starved the
    //rendering while a key was held. The text stays in the box and only the new part is taken.
    [UITestMethod]
    public void Typing_DoesNotClearTheInputBoxPerCharacter_AndInsertsEveryCharacterOnce()
    {
        var core = TestHelper.MakeCoreTextbox(0);
        core.SetText("");

        foreach (char c in "hello")
            core.InputHandler.SimulateUserInput(c.ToString());

        Assert.AreEqual("hello", core.GetText());
        Assert.AreEqual("hello", core.InputHandler.Text);
    }

    [UITestMethod]
    public void TakeNewText_ReturnsOnlyTheNewPart_AndEverythingAfterAReplacedPart()
    {
        var input = new InputHandlerControl();
        input.SimulateUserInput("ab");
        Assert.AreEqual("ab", input.TakeNewText());
        Assert.AreEqual("", input.TakeNewText());

        input.SimulateUserInput("c");
        Assert.AreEqual("c", input.TakeNewText());

        input.Text = "";
        input.SimulateUserInput("x");
        Assert.AreEqual("x", input.TakeNewText());
    }

    [UITestMethod]
    public void Composition_AfterTypedText_InsertsOnlyTheCommittedText()
    {
        var core = TestHelper.MakeCoreTextbox(0);
        core.SetText("");

        core.InputHandler.SimulateUserInput("a");
        core.InputHandler.SimulateCompositionStart();
        core.InputHandler.SimulateCompositionEnd("東京");

        Assert.AreEqual("a東京", core.GetText());
    }

    [UITestMethod]
    public void CoreTextControlBox_UpdateInputHandlerPosition_TracksCursorCoordinates()
    {
        var core = TestHelper.MakeCoreTextbox(10);
        core.SetCursorPosition(2, 5);

        core.UpdateInputHandlerPosition();

        var cursorPos = core.GetCursorPosition();
        Assert.IsNotNull(core.InputHandler);
        Assert.AreEqual(cursorPos.X, core.InputHandler.Margin.Left, 1.0);
        Assert.AreEqual(cursorPos.Y, core.InputHandler.Margin.Top, 1.0);
        Assert.IsGreaterThan(0.0, core.InputHandler.Height);
    }
}
