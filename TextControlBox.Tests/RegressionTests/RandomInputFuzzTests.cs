using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TextControlBoxNS;
using TextControlBoxNS.Core;
using Windows.Foundation;
using Windows.System;

namespace TextControlBox.Tests.RegressionTests;

/// <summary>
/// Throws random but reproducible (seeded) keyboard, pointer and API actions at the textbox and checks
/// after every single one that nothing threw, the cursor is inside the document and no line contains a
/// line break. At the end everything gets undone and has to result in the initial text again.
/// When this test fails, the message contains the seed and the last actions to reproduce it.
/// </summary>
[TestClass]
public class RandomInputFuzzTests
{
    private const int SeedsPerConfiguration = 40;
    private const int ActionsPerRun = 220;

    private static readonly string[] Chars = ["a", "b", " ", "_", "(", "{", "\"", "\t", "ðŸ˜€", "Ã©", ".", "x"];
    private static readonly VirtualKey[] Keys = [VirtualKey.Enter, VirtualKey.Back, VirtualKey.Delete, VirtualKey.Left, VirtualKey.Right, VirtualKey.Up, VirtualKey.Down,
        VirtualKey.Home, VirtualKey.End, VirtualKey.PageUp, VirtualKey.PageDown, VirtualKey.Escape];
    //no C, X and V: the clipboard is shared with the machine running the tests
    private static readonly VirtualKey[] CtrlKeys = [VirtualKey.Z, VirtualKey.Y, VirtualKey.A, VirtualKey.W, VirtualKey.Up, VirtualKey.Down, VirtualKey.Left, VirtualKey.Right,
        VirtualKey.Back, VirtualKey.Delete, VirtualKey.Home, VirtualKey.End];

    private static string Normalize(string text) => text.Replace("\r\n", "\n").Replace("\r", "\n");

    private static string DoRandomAction(CoreTextControlBox core, Random random)
    {
        int lines = Math.Max(1, core.NumberOfLines);
        int RandomLine() => random.Next(lines);
        int RandomChar(int line) => random.Next(core.textManager.GetLineLength(Math.Clamp(line, 0, core.NumberOfLines - 1)) + 2);
        Point RandomPoint() => new Point(random.Next(-20, 700), random.Next(-40, 700));
        string newLine = core.textManager.NewLineCharacter;

        int action = random.Next(100);
        if (action < 22) { string text = Chars[random.Next(Chars.Length)]; core.textActionManager.AddCharacter(text); return "type '" + text + "'"; }
        if (action < 50)
        {
            var key = Keys[random.Next(Keys.Length)];
            bool shift = random.Next(3) == 0;
            core.HandleKeyDown(key, shift, false, false);
            return $"key {key} shift={shift}";
        }
        if (action < 60)
        {
            var key = CtrlKeys[random.Next(CtrlKeys.Length)];
            bool shift = random.Next(4) == 0 && key != VirtualKey.Z && key != VirtualKey.Y && key != VirtualKey.A && key != VirtualKey.W;
            core.HandleKeyDown(key, shift, true, false);
            return $"ctrl+{key} shift={shift}";
        }
        if (action < 63) { var key = random.Next(2) == 0 ? VirtualKey.Up : VirtualKey.Down; core.HandleKeyDown(key, false, false, true); return "alt+" + key; }
        if (action < 67) { bool back = random.Next(2) == 0; if (back) core.tabSpaceManager.MoveTabBack(); else core.tabSpaceManager.MoveTab(); return back ? "shift+tab" : "tab"; }
        if (action < 70) { core.textActionManager.AddCharacter("x" + newLine + "yy" + newLine); return "insert multiline text"; }
        if (action < 72) { int line = RandomLine(); core.SelectLine(line); return "SelectLine " + line; }
        if (action < 76)
        {
            //the character may be one behind the line end, like a selection made with the pointer
            int line1 = RandomLine(), line2 = RandomLine();
            int char1 = RandomChar(line1), char2 = RandomChar(line2);
            core.SetSelection(line1, char1, line2, char2);
            return $"SetSelection {line1},{char1},{line2},{char2}";
        }
        if (action < 78) { core.HandleKeyDown(VirtualKey.Delete, true, false, false); return "shift+del"; }
        if (action < 80) { int line = RandomLine(); core.DeleteLine(line); return "DeleteLine " + line; }
        if (action < 81) { int line = RandomLine(); core.DuplicateLine(line); return "DuplicateLine " + line; }
        if (action < 82) { int line = RandomLine(); core.AddLine(line, random.Next(2) == 0 ? "" : "added"); return "AddLine " + line; }
        if (action < 83) { core.Delete(); return "Delete()"; }
        if (action < 84) { core.SelectAll(); return "SelectAll"; }
        if (action < 90)
        {
            var point1 = RandomPoint(); var point2 = RandomPoint(); int clicks = random.Next(1, 4);
            for (int i = 0; i < clicks; i++) core.pointerActionsManager.PointerPressedAction(core, point1, true, false);
            bool move = random.Next(2) == 0;
            if (move) { core.selectionManager.IsSelecting = true; core.pointerActionsManager.PointerMovedAction(point2); }
            core.pointerActionsManager.PointerReleasedAction(point2);
            return $"pointer clicks={clicks} {point1} move={move} {point2}";
        }
        if (action < 92) { bool empty = random.Next(2) == 0; core.textActionManager.PasteLine(empty ? newLine : "pasted" + newLine); return "PasteLine empty=" + empty; }
        if (action < 94) { core.BeginSearch("a", false, false); var result = random.Next(3) == 0 ? core.FindPrevious() : core.FindNext(); return "search " + result; }
        if (action < 95) { core.BeginSearch("a", false, false); core.ReplaceNext(random.Next(2) == 0 ? "ZZ" : "q" + newLine + "q"); return "ReplaceNext"; }
        if (action < 96) { core.ReplaceAll("b", random.Next(2) == 0 ? "" : "B" + newLine, true, false); return "ReplaceAll"; }
        if (action < 97) { core.EndSearch(); return "EndSearch"; }
        if (action < 98) { core.WordWrap = !core.WordWrap; return "toggle wordwrap"; }
        if (action < 99) { core.LineEnding = core.LineEnding == LineEnding.LF ? LineEnding.CRLF : LineEnding.LF; return "toggle lineending"; }
        core.Undo(); core.Undo(); core.Redo(); return "undo undo redo";
    }

    internal static string RunSeed(bool wordWrap, LineEnding lineEnding, int seed, int actions = ActionsPerRun)
    {
        var core = TestHelper.MakeCoreTextbox(0);
        try
        {
            return RunSeed(core, wordWrap, lineEnding, seed, actions);
        }
        finally
        {
            //thousands of textboxes that never get released slow the test host down until it dies
            core.Dispose();
        }
    }

    private static string RunSeed(CoreTextControlBox core, bool wordWrap, LineEnding lineEnding, int seed, int actions)
    {
        var random = new Random(seed * 7919 + (wordWrap ? 1 : 0) + (lineEnding == LineEnding.LF ? 2 : 0));
        core.LineEnding = lineEnding;
        core.SetText((seed % 5) switch
        {
            0 => "",
            1 => "single line",
            _ => "first line\n\n\tindented {x}\n    spaces ðŸ˜€ emoji\n" + new string('w', 300) + " long\n\nlast"
        });
        core.WordWrap = wordWrap;
        core.textRenderer.EnsureTextFormat();
        core.textRenderer.CalculateLinesToRender();
        core.ClearUndoRedoHistory();
        string initialText = Normalize(core.GetText());

        var log = new List<string>();
        string Failure(string what) =>
            $"wordWrap={wordWrap} {lineEnding} seed={seed} after {log.Count} actions: {what}\n   last actions: {string.Join(" | ", log.Skip(Math.Max(0, log.Count - 8)))}";

        for (int i = 0; i < actions; i++)
        {
            try
            {
                log.Add(DoRandomAction(core, random));
            }
            catch (Exception ex)
            {
                log.Add("(threw)");
                string frame = (ex.StackTrace ?? "").Split('\n').Select(x => x.Trim()).FirstOrDefault(x => x.Contains("TextControlBoxNS")) ?? "";
                return Failure($"{ex.GetType().Name}: {ex.Message.Split('\n')[0]} {frame}");
            }

            if (core.NumberOfLines < 1)
                return Failure("the document has no line");
            if (core.CursorPosition.LineNumber < 0 || core.CursorPosition.LineNumber >= core.NumberOfLines)
                return Failure($"cursor line {core.CursorPosition.LineNumber} is outside of {core.NumberOfLines} lines");
            if (core.CursorPosition.CharacterPosition < 0)
                return Failure($"cursor character {core.CursorPosition.CharacterPosition}");
            if (core.Lines.Any(line => line.Contains('\n') || line.Contains('\r')))
                return Failure("a line contains a line break character");
        }

        try
        {
            int guard = 0;
            while (core.CanUndo && guard++ < 100_000)
                core.Undo();
        }
        catch (Exception ex)
        {
            return Failure($"undoing everything threw {ex.GetType().Name}: {ex.Message.Split('\n')[0]}");
        }

        string finalText = Normalize(core.GetText());
        if (finalText != initialText)
            return Failure($"undoing everything did not restore the initial text (length {initialText.Length} expected, got {finalText.Length})");

        return null;
    }

    private static void RunConfiguration(bool wordWrap, LineEnding lineEnding)
    {
        var failures = new StringBuilder();
        int failed = 0;
        for (int seed = 0; seed < SeedsPerConfiguration; seed++)
        {
            string failure = RunSeed(wordWrap, lineEnding, seed);
            if (failure != null && failed++ < 5)
                failures.AppendLine(failure);
        }
        Assert.AreEqual(0, failed, "\n" + failures);
    }

    [UITestMethod]
    public void RandomActions_NoWrap_CRLF() => RunConfiguration(false, LineEnding.CRLF);

    [UITestMethod]
    public void RandomActions_NoWrap_LF() => RunConfiguration(false, LineEnding.LF);

    [UITestMethod]
    public void RandomActions_WordWrap_CRLF() => RunConfiguration(true, LineEnding.CRLF);

    [UITestMethod]
    public void RandomActions_WordWrap_LF() => RunConfiguration(true, LineEnding.LF);
}
