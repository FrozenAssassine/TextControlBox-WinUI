using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using System;
using System.Linq;
using System.Text;
using TextControlBoxNS.Helper;

namespace TextControlBox.Tests;

[TestClass]
public class PerformanceRegressionTests
{
    private static string RandomCodeLine(Random random)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789 (){}[];.,=+-*/\"'<>!&|#_";
        var builder = new StringBuilder();
        int indent = random.Next(0, 5);
        for (int i = 0; i < indent; i++)
            builder.Append(random.Next(3) == 0 ? "    " : "\t");

        int length = random.Next(0, 140);
        for (int i = 0; i < length; i++)
            builder.Append(random.Next(25) == 0 ? '\t' : chars[random.Next(chars.Length)]);
        return builder.ToString();
    }

    private static long RetainedMemory()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        return GC.GetTotalMemory(true);
    }

    // Typing on a long line kept the whole line twice per undo step: 100 words on a 500k line held 190 MB.
    [UITestMethod]
    public void TypingWordsIntoALongLine_KeepsTheUndoHistorySmall()
    {
        var core = TestHelper.MakeCoreTextbox(0);
        core.LoadLines([new string('a', 500_000)]);
        core.SetCursorPosition(0, 250_000, false);

        long before = RetainedMemory();
        for (int i = 0; i < 100; i++)
        {
            core.textActionManager.AddCharacter(" ");
            core.textActionManager.AddCharacter("w");
        }
        long retainedMB = (RetainedMemory() - before) / 1024 / 1024;

        Assert.IsLessThan(10, retainedMB, $"{retainedMB} MB retained by 100 typed words");
        GC.KeepAlive(core);
    }

    [UITestMethod]
    public void TypingIntoALongLine_UndoAndRedo_RestoreTheExactText()
    {
        var core = TestHelper.MakeCoreTextbox(0);
        string original = new string('x', 20_000) + "MIDDLE" + new string('y', 20_000);
        core.LoadLines([original, "second line"]);
        core.SetCursorPosition(0, 20_003, false);

        foreach (string typed in new[] { "h", "e", "l", "l", "o", " ", "w", "o", "r", "l", "d", "!", "😀" })
            core.textActionManager.AddCharacter(typed);
        string afterTyping = core.GetText();

        int undoSteps = 0;
        while (core.CanUndo && core.GetText() != original.Replace("\n", "") + core.textManager.NewLineCharacter + "second line")
        {
            core.Undo();
            undoSteps++;
        }
        Assert.AreEqual(original + core.textManager.NewLineCharacter + "second line", core.GetText());
        Assert.IsGreaterThan(1, undoSteps, "words and punctuation are separate undo steps");

        for (int i = 0; i < undoSteps; i++)
            core.Redo();
        Assert.AreEqual(afterTyping, core.GetText());
    }

    [UITestMethod]
    public void BackspaceAndDeleteOnALongLine_UndoAndRedo_RestoreTheExactText()
    {
        var core = TestHelper.MakeCoreTextbox(0);
        string original = string.Concat(Enumerable.Range(0, 3_000).Select(i => "word" + i + " "));
        core.LoadLines([original]);
        core.SetCursorPosition(0, 5_000, false);

        for (int i = 0; i < 20; i++)
            core.textActionManager.RemoveText();
        for (int i = 0; i < 5; i++)
            core.textActionManager.AddCharacter("Z");
        string edited = core.GetText();

        while (core.CanUndo)
            core.Undo();
        Assert.AreEqual(original, core.GetText());

        while (core.CanRedo)
            core.Redo();
        Assert.AreEqual(edited, core.GetText());
    }

    // The arithmetic width must match DirectWrite, otherwise the horizontal scroll range or the wrapping is off.
    [UITestMethod]
    public void MonospaceLineWidth_MatchesDirectWrite()
    {
        var core = TestHelper.MakeCoreTextbox(0);
        var renderer = core.textRenderer;
        Assert.IsGreaterThan(0f, renderer.MonospaceCharWidth, "Consolas is monospace");

        var random = new Random(7);
        for (int i = 0; i < 300; i++)
        {
            string line = RandomCodeLine(random);
            float computed = renderer.GetMonospaceLineWidth(line);

            using var layout = new CanvasTextLayout(CanvasDevice.GetSharedDevice(), line, renderer.TextFormat, 0, 0);
            layout.WordWrapping = CanvasWordWrapping.NoWrap;
            float measured = layout.GetCaretPosition(line.Length, false).X;

            Assert.AreEqual(measured, computed, 0.5f, $"line {i}: '{line.Replace("\t", "\\t")}'");
        }
    }

    [UITestMethod]
    public void MonospaceLineWidth_RefusesWhatItCanNotCompute()
    {
        var renderer = TestHelper.MakeCoreTextbox(0).textRenderer;
        Assert.AreEqual(-1f, renderer.GetMonospaceLineWidth("emoji 😀"));
        Assert.AreEqual(-1f, renderer.GetMonospaceLineWidth("umlaut ä"));
        Assert.AreEqual(-1f, renderer.GetMonospaceLineWidth("日本語"));
        Assert.AreEqual(0f, renderer.GetMonospaceLineWidth(""));
    }

    [UITestMethod]
    public void ProportionalFont_IsNotTakenForMonospace()
    {
        var core = TestHelper.MakeCoreTextbox(0);
        core.FontFamily = new Microsoft.UI.Xaml.Media.FontFamily("Segoe UI");
        Assert.AreEqual(0f, core.textRenderer.MonospaceCharWidth);
        Assert.AreEqual(-1f, core.textRenderer.GetMonospaceLineWidth("abc"));
    }

    // Word wrap only measures lines with a layout when the arithmetic says they might wrap. Both must agree.
    [UITestMethod]
    public void WrappedRowCounts_MatchTheMeasuredLayout()
    {
        var core = TestHelper.MakeCoreTextbox(0);
        var random = new Random(11);
        var lines = Enumerable.Range(0, 400).Select(_ => RandomCodeLine(random)).ToArray();
        core.LoadLines(lines);
        core.WordWrap = true;
        var renderer = core.textRenderer;
        renderer.EnsureWrapMetrics(core.canvasText);

        for (int i = 0; i < lines.Length; i++)
        {
            int measured = lines[i].Length == 0 ? 1 : renderer.MeasureWrappedRowCountWithLayout(core.canvasText, lines[i]);
            Assert.AreEqual(measured, renderer.GetWrappedRowCount(i), $"line {i}: '{lines[i].Replace("\t", "\\t")}'");
        }
    }

    [UITestMethod]
    public void LongestLineWidth_MatchesTheMeasuredWidth()
    {
        var core = TestHelper.MakeCoreTextbox(0);
        string longest = "\tpublic static void Main(string[] args) { Console.WriteLine(\"hello\"); }";
        core.LoadLines(["short", longest, "mid length line"]);
        core.longestLineManager.Recalculate();

        var measured = Utils.MeasureLineLenght(CanvasDevice.GetSharedDevice(), longest, core.textRenderer.TextFormat);
        Assert.AreEqual(measured.Width, core.longestLineManager.longestLineWidth.Width, core.textRenderer.MonospaceCharWidth);
    }
}
