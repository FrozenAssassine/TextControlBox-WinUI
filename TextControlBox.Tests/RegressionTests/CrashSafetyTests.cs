using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using System;
using System.Diagnostics;
using System.Linq;
using TextControlBoxNS;
using TextControlBoxNS.Core;
using TextControlBoxNS.Models;
using Windows.System;

namespace TextControlBox.Tests.RegressionTests;

[TestClass]
public class CrashSafetyTests
{
    private static CoreTextControlBox MakeCore(string text)
    {
        var core = TestHelper.MakeCoreTextbox(addNewLines: 0);
        core.SetText(text);
        core.ClearUndoRedoHistory();
        return core;
    }

    private static void AssertStateIsInsideDocument(CoreTextControlBox core)
    {
        Assert.IsTrue(core.CursorPosition.LineNumber >= 0 && core.CursorPosition.LineNumber < core.NumberOfLines, "cursor line " + core.CursorPosition);
        Assert.IsTrue(core.CursorPosition.CharacterPosition <= core.GetLineText(core.CursorPosition.LineNumber).Length, "cursor character " + core.CursorPosition);
    }

    //Shift+Del left the cursor column behind the end of the (shorter) next line. A selection started
    //from there and the next typed character threw an ArgumentOutOfRangeException.
    [UITestMethod]
    public void CutLine_ThenSelectToTextStart_ThenType_DoesNotThrow()
    {
        var core = MakeCore("x\na long line here\nab");
        core.SetCursorPosition(1, 14, scrollIntoView: false);

        core.HandleKeyDown(VirtualKey.Delete, shift: true);
        AssertStateIsInsideDocument(core);

        core.HandleKeyDown(VirtualKey.Home, shift: true, ctrl: true);
        core.textActionManager.AddCharacter("q");

        Assert.AreEqual("q", core.GetText());
        while (core.CanUndo) core.Undo();
        Assert.AreEqual(core.stringManager.CleanUpString("x\na long line here\nab"), core.GetText());
    }

    //Redo restored a selection that was recorded for a bigger document.
    [UITestMethod]
    public void SelectAll_Enter_DeleteLine_UndoUndoRedo_ThenType_DoesNotThrow()
    {
        var core = MakeCore("one\ntwo\nthree\nfour");

        core.HandleKeyDown(VirtualKey.A, ctrl: true);
        core.HandleKeyDown(VirtualKey.Enter);
        core.HandleKeyDown(VirtualKey.Delete, shift: true);
        core.HandleKeyDown(VirtualKey.Z, ctrl: true);
        core.HandleKeyDown(VirtualKey.Z, ctrl: true);
        core.HandleKeyDown(VirtualKey.Y, ctrl: true);
        AssertStateIsInsideDocument(core);

        core.textActionManager.AddCharacter("q");
        AssertStateIsInsideDocument(core);
    }

    //Redo of a deleted last line put the cursor behind the document.
    [UITestMethod]
    public void DeleteLastLine_UndoRedo_KeepsCursorInsideDocument()
    {
        var core = MakeCore("one\ntwo\nthree");
        core.SetCursorPosition(2, 1, scrollIntoView: false);

        core.HandleKeyDown(VirtualKey.Delete, shift: true);
        AssertStateIsInsideDocument(core);
        core.HandleKeyDown(VirtualKey.Z, ctrl: true);
        AssertStateIsInsideDocument(core);
        core.HandleKeyDown(VirtualKey.Y, ctrl: true);
        AssertStateIsInsideDocument(core);

        core.HandleKeyDown(VirtualKey.Left);
        core.textActionManager.AddCharacter("q");
        Assert.AreEqual(2, core.NumberOfLines);
    }

    //API edits moved lines below an active selection, typing afterwards read out of range.
    [UITestMethod]
    public void ApiEdits_WithActiveSelection_ThenType_DoNotThrow()
    {
        Action<CoreTextControlBox>[] edits =
        [
            c => c.DeleteLine(0),
            c => c.DeleteLine(c.NumberOfLines - 1),
            c => c.DuplicateLine(1),
            c => c.AddLine(0, "new"),
            c => c.SetLineText(3, ""),
            c => c.ReplaceAll("o", "", true, false),
            c => c.ReplaceAll("e", "\n", true, false),
        ];

        foreach (var edit in edits)
        {
            var core = MakeCore("one\ntwo\nthree\nfour");
            core.SetSelection(0, 2, 3, 4);

            edit(core);
            AssertStateIsInsideDocument(core);

            core.textActionManager.AddCharacter("q");
            AssertStateIsInsideDocument(core);
            string selected = core.SelectedText;
        }
    }

    //A pointer selection can end one character behind an empty line ("including the line break").
    [UITestMethod]
    public void SelectionEndingBehindAnEmptyLine_ThenType_DoesNotThrow()
    {
        var core = MakeCore("first\n\nthird");
        core.SetSelection(0, 0, 1, 1);

        core.textActionManager.AddCharacter("q");

        Assert.AreEqual("qthird", core.GetText());
        core.Undo();
        Assert.AreEqual(core.stringManager.CleanUpString("first\n\nthird"), core.GetText());
    }

    //The highlighting regexes run on the UI thread for every rebuild of the visible text.
    [UITestMethod]
    public void HighlightRule_WithCatastrophicPattern_GivesUpInsteadOfBlocking()
    {
        var rule = new RegexHighlightRule(new SyntaxHighlights("(a+)+$", "#000000", "#ffffff"));
        string text = new string('a', 5000) + "!";

        var stopwatch = Stopwatch.StartNew();
        var highlights = rule.GetHighlights(ReadOnlySpan<string>.Empty, text, "\r\n");
        stopwatch.Stop();

        Assert.IsNotNull(highlights);
        Assert.IsTrue(stopwatch.ElapsedMilliseconds < 1000, $"took {stopwatch.ElapsedMilliseconds} ms");
    }

    [UITestMethod]
    public void BuiltInLanguages_StayFastOnLongLines()
    {
        const int length = 19_000;
        string Repeat(string part) => string.Concat(Enumerable.Repeat(part, length / part.Length + 1)).Substring(0, length);

        var cases = new (SyntaxHighlightID id, string text)[]
        {
            (SyntaxHighlightID.Json, "[" + Repeat("\"abcdef\",") + "]"),
            (SyntaxHighlightID.Json, Repeat("\"")),
            (SyntaxHighlightID.CSS, "  background:url(data:image/png;base64," + Repeat("iVBORw0KGgoAAAANSUhEUgAA") + ");"),
            (SyntaxHighlightID.CSS, Repeat(" ")),
            (SyntaxHighlightID.Html, Repeat("a < b and ")),
            (SyntaxHighlightID.Inifile, Repeat("some long value text ")),
            (SyntaxHighlightID.Inifile, Repeat(" ")),
        };

        foreach (var (id, text) in cases)
        {
            var language = CoreTextControlBox.GetSyntaxHighlightingFromID(id);
            language.CompileAllRegex();

            var stopwatch = Stopwatch.StartNew();
            foreach (var highlight in language.Highlights)
            {
                try { highlight.PrecompiledRegex.Count(text); }
                catch (System.Text.RegularExpressions.RegexMatchTimeoutException) { }
            }
            stopwatch.Stop();

            //every rule is limited by the match timeout, the rewritten rules must not even get close to it
            Assert.IsTrue(stopwatch.ElapsedMilliseconds < 80 * language.Highlights.Length, $"{id}: {stopwatch.ElapsedMilliseconds} ms");
        }
    }

    [UITestMethod]
    public void JsonKeyRule_HighlightsOnlyTheKey()
    {
        var language = CoreTextControlBox.GetSyntaxHighlightingFromID(SyntaxHighlightID.Json);
        language.CompileAllRegex();
        var keyRule = language.Highlights.Single(x => x.Pattern.EndsWith("\\:"));

        var matches = keyRule.PrecompiledRegex.Matches("{\"name\": \"va:lue\", \"other\" : [\"a\", \"b\"], \"esc\\\"aped\":1}");

        CollectionAssert.AreEqual(new[] { "\"name\":", "\"other\" :", "\"esc\\\"aped\":" }, matches.Select(m => m.Value).ToArray());
    }

    [UITestMethod]
    public void CssSelectorRule_StillMatchesSelectors()
    {
        var language = CoreTextControlBox.GetSyntaxHighlightingFromID(SyntaxHighlightID.CSS);
        language.CompileAllRegex();
        var selectorRule = language.Highlights.Single(x => x.Pattern.EndsWith("\\s*{"));

        var matches = selectorRule.PrecompiledRegex.Matches(".a, #b:hover {\r\n  color: red;\r\n}\r\nbody\r\n{\r\n}\r\n.c{x:1}.d{y:2}");

        CollectionAssert.AreEqual(new[] { ".a, #b:hover {", "body\r\n{", ".c{", ".d{" }, matches.Select(m => m.Value).ToArray());
    }

    //The first match of a compiled regex includes generating its code. For the big keyword rules that took
    //longer than the match timeout, so the first drawn frame lost its keyword colors.
    [UITestMethod]
    public void HighlightRules_AreWarmedUp_SoTheFirstMatchDoesNotTimeOut()
    {
        string code = string.Concat(Enumerable.Repeat("public static void Main(string[] args) { var x = 12.5f; // comment \"str\"\r\n", 60));

        foreach (var id in new[] { SyntaxHighlightID.Cpp, SyntaxHighlightID.CSharp, SyntaxHighlightID.Java, SyntaxHighlightID.x86Assembly })
        {
            //a fresh instance: nothing of it was compiled or matched before
            var language = (SyntaxHighlightLanguage)Activator.CreateInstance(CoreTextControlBox.GetSyntaxHighlightingFromID(id).GetType(), true);
            language.CompileAllRegex();

            foreach (var highlight in language.Highlights)
            {
                //throws a RegexMatchTimeoutException when the first match is too slow
                highlight.PrecompiledRegex.Count(code);
            }
        }
    }

    //A triple click selects (line, 0) to (line + 1, 0). Tab indented the following line as well,
    //although none of its characters is selected.
    [UITestMethod]
    public void Tab_OnWholeLineSelections_IndentsOnlyTheSelectedLines()
    {
        (string name, Action<CoreTextControlBox> select, string expected)[] cases =
        [
            ("triple click", c => c.SetSelection(1, 0, 2, 0), "a\n\tb\nc\nd"),
            ("triple click, last line", c => c.SetSelection(3, 0, 3, 1), "a\nb\nc\n\td"),
            ("line number click", c => c.SelectLine(1), "a\n\tb\nc\nd"),
            ("line number drag over two lines", c => c.SetSelection(1, 0, 3, 0), "a\n\tb\n\tc\nd"),
            ("normal multi line selection", c => c.SetSelection(1, 0, 2, 1), "a\n\tb\n\tc\nd"),
        ];

        foreach (var (name, select, expected) in cases)
        {
            var core = MakeCore("a\nb\nc\nd");
            core.UseSpacesInsteadTabs = false;
            select(core);

            core.HandleTabKey(false);
            Assert.AreEqual(core.stringManager.CleanUpString(expected), core.GetText(), name + ": Tab");
            Assert.IsTrue(core.HasSelection, name + ": the selection stays");

            core.HandleTabKey(false);
            core.HandleTabKey(true);
            core.HandleTabKey(true);
            Assert.AreEqual(core.stringManager.CleanUpString("a\nb\nc\nd"), core.GetText(), name + ": Tab Tab Shift+Tab Shift+Tab");

            while (core.CanUndo) core.Undo();
            Assert.AreEqual(core.stringManager.CleanUpString("a\nb\nc\nd"), core.GetText(), name + ": undo");
        }

        var partial = MakeCore("abc\nd");
        partial.UseSpacesInsteadTabs = false;
        partial.SetSelection(0, 1, 0, 2);
        partial.HandleTabKey(false);
        Assert.AreEqual(partial.stringManager.CleanUpString("a\tc\nd"), partial.GetText(), "a part of a line is replaced by the tab");
    }

    //In word wrap mode the row count of every line is cached. Edits that change other lines than the
    //cursor line did not update it, so scrolling, clicking and the caret used outdated positions.
    [UITestMethod]
    public void WordWrap_RowCounts_AreUpdatedByEditsAwayFromTheCursorLine()
    {
        string longLine = string.Join(" ", Enumerable.Repeat("the quick brown fox jumps over the lazy dog", 12));

        (string name, Action<CoreTextControlBox> edit)[] edits =
        [
            ("ReplaceAll", c => c.ReplaceAll("the quick brown fox jumps over the lazy dog", "x", true, false)),
            ("SetLineText", c => c.SetLineText(3, "tiny")),
            ("move line down", c => c.HandleKeyDown(VirtualKey.Down, false, false, true)),
            ("move line up", c => { c.SetCursorPosition(2, 0, false); c.HandleKeyDown(VirtualKey.Up, false, false, true); }),
            ("indent selection", c => { c.SetSelection(0, 0, 4, 3); for (int i = 0; i < 6; i++) c.HandleTabKey(false); }),
            ("unindent selection", c => { c.SetSelection(0, 0, 4, 3); for (int i = 0; i < 6; i++) c.HandleTabKey(false); for (int i = 0; i < 6; i++) c.HandleTabKey(true); }),
            ("RewriteTabsSpaces", c => { c.SetLineText(1, "\t\t\t\t" + c.GetLineText(1)); c.RewriteTabsSpaces(8, true); }),
        ];

        foreach (var (name, edit) in edits)
        {
            var core = MakeCore(string.Join("\n", new[] { "short", longLine, "short b", longLine, longLine, "end" }));
            core.WordWrap = true;
            core.textRenderer.EnsureTextFormat();
            core.SetCursorPosition(0, 0, scrollIntoView: false);
            core.textRenderer.EnsureWrapMetrics(core.canvasText);

            edit(core);

            string RowCounts()
            {
                core.textRenderer.EnsureWrapMetrics(core.canvasText);
                return string.Join(",", Enumerable.Range(0, core.NumberOfLines).Select(core.textRenderer.GetWrappedRowCount));
            }
            string cached = RowCounts();
            core.textRenderer.InvalidateWrapMetrics();
            string measured = RowCounts();

            Assert.AreEqual(measured, cached, name);
        }
    }
}
