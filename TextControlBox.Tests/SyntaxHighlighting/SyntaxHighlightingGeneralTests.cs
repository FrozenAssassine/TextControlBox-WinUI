using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using TextControlBoxNS;
using TextControlBoxNS.Core;
using TextControlBoxNS.Core.Renderer;

namespace TextControlBox.Tests.SyntaxHighlighting;

[TestClass]
public class SyntaxHighlightingGeneralTests
{
    [UITestMethod]
    public void EverySyntaxHighlightID_HasALanguage()
    {
        foreach (SyntaxHighlightID id in Enum.GetValues<SyntaxHighlightID>())
        {
            Assert.IsTrue(CoreTextControlBox.SyntaxHighlightings.TryGetValue(id, out var language), $"{id} is not registered");
            if (id == SyntaxHighlightID.None)
                Assert.IsNull(language);
            else
                Assert.IsNotNull(language, $"{id} has no language");
        }
    }

    // Apps store the id as a number (settings, session files). A new language must never shift the existing ones.
    [TestMethod]
    public void SyntaxHighlightIDs_KeepTheirNumbers()
    {
        SyntaxHighlightID[] releasedOrder =
        [
            SyntaxHighlightID.x86Assembly, SyntaxHighlightID.Batch, SyntaxHighlightID.Cpp, SyntaxHighlightID.CSharp, SyntaxHighlightID.Klipper,
            SyntaxHighlightID.Inifile, SyntaxHighlightID.CSS, SyntaxHighlightID.CSV, SyntaxHighlightID.CSVImproved, SyntaxHighlightID.GCode,
            SyntaxHighlightID.Gitignore, SyntaxHighlightID.HexFile, SyntaxHighlightID.Html, SyntaxHighlightID.Java, SyntaxHighlightID.Javascript,
            SyntaxHighlightID.Json, SyntaxHighlightID.Latex, SyntaxHighlightID.Lua, SyntaxHighlightID.Markdown, SyntaxHighlightID.PHP,
            SyntaxHighlightID.Python, SyntaxHighlightID.QSharp, SyntaxHighlightID.SQL, SyntaxHighlightID.TOML, SyntaxHighlightID.XML,
            SyntaxHighlightID.None, SyntaxHighlightID.Log, SyntaxHighlightID.Generic,
        ];

        for (int i = 0; i < releasedOrder.Length; i++)
            Assert.AreEqual(i, (int)releasedOrder[i], $"{releasedOrder[i]} changed its number");
    }

    [TestMethod]
    public void EveryLanguage_HasSampleCoverage()
    {
        var covered = LanguageSamples.All.Select(x => x.Id).ToHashSet();
        var missing = Enum.GetValues<SyntaxHighlightID>().Where(id => id != SyntaxHighlightID.None && !covered.Contains(id)).ToList();
        Assert.IsEmpty(missing, "Languages without samples: " + string.Join(", ", missing));
    }

    [TestMethod]
    public void EveryLanguage_HasNameFilterAndRules()
    {
        foreach (var (id, language, _) in LanguageSamples.All)
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(language.Name), $"{id} has no name");
            Assert.IsTrue(language.Filter?.Length > 0, $"{id} has no file filter");
            Assert.IsTrue(language.Highlights?.Length > 0 || language.HighlightRules?.Length > 0, $"{id} has no highlights");
            Assert.IsTrue(language.Filter!.All(f => f.StartsWith('.')), $"{id} has a filter without a leading dot");
        }
    }

    [TestMethod]
    public void EveryPattern_Compiles()
    {
        foreach (var (id, language, _) in LanguageSamples.All)
        {
            if (language.Highlights == null)
                continue;

            language.CompileAllRegex();
            foreach (var highlight in language.Highlights)
                Assert.IsNotNull(highlight.PrecompiledRegex, $"{id}: '{highlight.Pattern}' was not compiled");
        }
    }

    [TestMethod]
    public void EverySample_IsActuallyHighlighted()
    {
        foreach (var (id, language, sample) in LanguageSamples.All)
        {
            var colors = HighlightSimulator.Colorize(language, sample.Replace("\r\n", "\n"));
            int colored = colors.Count(c => c != null);
            Assert.IsGreaterThan(sample.Length / 10, colored, $"{id}: only {colored} of {sample.Length} chars highlighted");
        }
    }

    // The renderer stops applying spans after MaxHighlightsPerFrame. The rules run in order, so the last rules
    // (comments, strings) are the ones that get dropped, which makes highlighting on a normal screen of code wrong.
    [TestMethod]
    public void FullScreenOfCode_FitsIntoTheSpanBudget()
    {
        var failures = new List<string>();
        foreach (var (id, language, sample) in LanguageSamples.All)
        {
            string screen = LanguageSamples.FullScreen(sample);
            var unlimited = HighlightSimulator.Colorize(language, screen);
            var budgeted = HighlightSimulator.Colorize(language, screen, respectSpanBudget: true);

            int differences = unlimited.Zip(budgeted).Count(x => x.First != x.Second);
            if (differences > 0)
                failures.Add($"{id}: {differences} of {screen.Length} chars are colored differently because of the {SyntaxHighlightingRenderer.MaxHighlightsPerFrame} span limit");
        }
        Assert.IsEmpty(failures, string.Join("\n", failures));
    }

    // Zoomed out on a big monitor about 150 lines are visible.
    [TestMethod]
    public void ZoomedOutScreenOfCode_FitsIntoTheSpanBudget()
    {
        var failures = new List<string>();
        foreach (var (id, language, sample) in LanguageSamples.All)
        {
            string screen = LanguageSamples.FullScreen(sample, 150);
            if (screen.Length > SyntaxHighlightingRenderer.MaxHighlightTextLength)
                continue;

            var unlimited = HighlightSimulator.Colorize(language, screen);
            var budgeted = HighlightSimulator.Colorize(language, screen, respectSpanBudget: true);

            int differences = unlimited.Zip(budgeted).Count(x => x.First != x.Second);
            if (differences > 0)
                failures.Add($"{id}: {differences} of {screen.Length} chars are colored differently because of the {SyntaxHighlightingRenderer.MaxHighlightsPerFrame} span limit");
        }
        Assert.IsEmpty(failures, string.Join("\n", failures));
    }

    // A rule that times out is silently skipped in the renderer, so the text just loses its highlighting.
    [TestMethod]
    public void NoPattern_TimesOut_OnHostileInput()
    {
        var failures = new List<string>();
        int length = SyntaxHighlightingRenderer.MaxHighlightTextLength;

        foreach (var (id, language, sample) in LanguageSamples.All)
        {
            if (language.Highlights == null)
                continue;
            language.CompileAllRegex();

            foreach (var (inputName, input) in HostileInputs(sample, length))
            {
                foreach (var highlight in language.Highlights)
                {
                    try
                    {
                        foreach (var _ in highlight.PrecompiledRegex.EnumerateMatches(input)) { }
                    }
                    catch (RegexMatchTimeoutException)
                    {
                        failures.Add($"{id}: '{highlight.Pattern}' times out on {inputName}");
                    }
                }
            }
        }
        Assert.IsEmpty(failures, string.Join("\n", failures.Distinct()));
    }

    // The renderer skips the remaining rules once a frame took longer than MaxHighlightMillisecondsPerFrame.
    [TestMethod]
    public void WholeLanguage_StaysInsideTheFrameTimeBudget()
    {
        var failures = new List<string>();
        int length = SyntaxHighlightingRenderer.MaxHighlightTextLength;

        foreach (var (id, language, sample) in LanguageSamples.All)
        {
            foreach (var (inputName, input) in HostileInputs(sample, length))
            {
                //best of three runs, so a busy test machine does not make this flaky
                double best = double.MaxValue;
                for (int run = 0; run < 3; run++)
                {
                    var watch = Stopwatch.StartNew();
                    try
                    {
                        HighlightSimulator.Colorize(language, input);
                    }
                    catch (RegexMatchTimeoutException)
                    {
                    }
                    best = Math.Min(best, watch.Elapsed.TotalMilliseconds);
                }

                if (best > SyntaxHighlightingRenderer.MaxHighlightMillisecondsPerFrame)
                    failures.Add($"{id}: {best:0}ms on {inputName}");
            }
        }
        Assert.IsEmpty(failures, string.Join("\n", failures));
    }

    // Realistic worst cases for the visible text: it never gets longer than MaxHighlightTextLength and holds at most a
    // few hundred lines, but a single line can be one long blob.
    private static IEnumerable<(string Name, string Text)> HostileInputs(string sample, int length)
    {
        yield return ("sample as one long line", LanguageSamples.LongLine(sample, length));
        yield return ("200 lines of the sample", Cut(LanguageSamples.FullScreen(sample, 200), length));

        var random = new Random(42);
        const string base64 = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/";
        yield return ("base64 blob", new string(Enumerable.Range(0, length).Select(_ => base64[random.Next(base64.Length)]).ToArray()));
        yield return ("long word", new string('a', length));
        yield return ("hex dump", Repeat("0123456789abcdef", length));
        yield return ("long indentation", new string(' ', length - 1) + "x");
        yield return ("unterminated double quoted string", "x = \"" + Repeat("lorem ipsum dolor ", length - 5));
        yield return ("unterminated single quoted string", "x = '" + Repeat("lorem ipsum dolor ", length - 5));
        yield return ("unterminated block comment", "/* " + Repeat("lorem ipsum dolor ", length - 3));
        yield return ("unterminated tag", "<" + Repeat("a b=c ", length - 1));

        foreach (string unit in new[] { "=", "-", "#", "*", "/", "_", ".", "{", "[", "<", ":", ";", "\"", "'", "\\" })
        {
            //a screen full of banner lines like "=====" or "-----"
            yield return ($"160 lines of '{unit}' x 120", Cut(string.Join("\n", Enumerable.Repeat(new string(unit[0], 120), 160)), length));
        }
    }

    private static string Repeat(string unit, int length)
    {
        var builder = new System.Text.StringBuilder(length + unit.Length);
        while (builder.Length < length)
            builder.Append(unit);
        return builder.ToString(0, length);
    }

    private static string Cut(string text, int length) => text.Length > length ? text[..length] : text;
}
