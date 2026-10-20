using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using System.Linq;

namespace TextControlBox.Tests;

// From 10k lines on, the wrapped row count of a line was estimated (length / chars per row) instead of measured.
// DirectWrite wraps at word boundaries, so a wrapped line often needs one row more than the estimate. Line numbers,
// caret and selection were placed with the estimate while the text was drawn with the real wrap, so everything
// below such a line was shifted by a row (seen in a generated 35k line .g.cs file).
[TestClass]
public class LargeDocumentWrapTests
{
    private static readonly string[] CodeLines =
    [
        "        [global::System.CodeDom.Compiler.GeneratedCodeAttribute(\"Microsoft.UI.Xaml.Markup.Compiler\",\" 3.0.0.2512\")]",
        "        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]",
        "        static void Main(string[] args)",
        "        {",
        "            global::Microsoft.UI.Xaml.Application.Start((p) => {",
        "                var context = new global::Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext(global::Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());",
        "                global::System.Threading.SynchronizationContext.SetSynchronizationContext(context);",
        "                new App();",
        "            });",
        "        }",
    ];

    private static TextControlBoxNS.Core.CoreTextControlBox MakeWrappedDocument(int lineCount)
    {
        var core = TestHelper.MakeCoreTextbox(0);
        core.LoadLines(Enumerable.Range(0, lineCount).Select(i => CodeLines[i % CodeLines.Length]));
        core.WordWrap = true;
        core.textRenderer.EnsureWrapMetrics(core.canvasText);
        return core;
    }

    [UITestMethod]
    public void LargeDocument_RowCountsOfTheVisibleLines_MatchTheDrawnLayout()
    {
        var core = MakeWrappedDocument(35_000);
        var renderer = core.textRenderer;

        foreach (int start in new[] { 0, 17_500, 34_980 })
        {
            //20 lines of at most 4 rows each
            renderer.EnsureExactWrapRows(core.canvasText, start, 80);
            for (int i = start; i < start + 20; i++)
            {
                string line = core.textManager.GetLineText(i);
                Assert.AreEqual(renderer.MeasureWrappedRowCountWithLayout(core.canvasText, line), renderer.GetWrappedRowCount(i), $"line {i}: '{line}'");
            }
        }
    }

    // The same through the render path: whatever is on screen after scrolling has exact row counts.
    [UITestMethod]
    public void LargeDocument_ScrolledIntoTheMiddle_RenderedLinesAreExact()
    {
        var core = MakeWrappedDocument(35_000);
        var renderer = core.textRenderer;
        renderer.UpdateRenderedLineRange(core.canvasText);

        foreach (double fraction in new[] { 0.5, 0.9, 0.1 })
        {
            core.scrollManager.VerticalScroll = core.scrollManager.verticalScrollBar.Maximum * fraction;
            renderer.UpdateRenderedLineRange(core.canvasText);

            Assert.IsGreaterThan(1_000, renderer.NumberOfStartLine, "scrolled away from the top");
            for (int i = renderer.NumberOfStartLine; i < renderer.NumberOfStartLine + renderer.NumberOfRenderedLines; i++)
            {
                string line = core.textManager.GetLineText(i);
                Assert.AreEqual(renderer.MeasureWrappedRowCountWithLayout(core.canvasText, line), renderer.GetWrappedRowCount(i), $"line {i}: '{line}'");
            }
        }
    }

    [UITestMethod]
    public void SmallDocument_RowCounts_MatchTheDrawnLayout()
    {
        var core = MakeWrappedDocument(500);
        var renderer = core.textRenderer;
        for (int i = 0; i < 500; i++)
        {
            string line = core.textManager.GetLineText(i);
            Assert.AreEqual(renderer.MeasureWrappedRowCountWithLayout(core.canvasText, line), renderer.GetWrappedRowCount(i), $"line {i}: '{line}'");
        }
    }
}
