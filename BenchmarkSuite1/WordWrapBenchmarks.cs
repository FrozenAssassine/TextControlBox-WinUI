using BenchmarkDotNet.Attributes;
using Collections.Pooled;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using System.Runtime.Serialization;
using TextControlBoxNS.Core.Renderer;
using TextControlBoxNS.Core.Text;
using TextControlBoxNS.Models;
using Microsoft.VSDiagnostics;

namespace TextControlBoxNS.Benchmarks;
[CPUUsageDiagnoser]
public class WordWrapBenchmarks
{
    private CanvasDevice device = null!;
    private CanvasTextFormat textFormat = null!;
    private CanvasTextLayout layout = null!;
    private CanvasRenderTarget renderTarget = null!;
    private TextManager textManager = null!;
    private VisualLineMap visualLineMap = null!;
    private SyntaxHighlightLanguage syntaxHighlightingLanguage = null!;
    private string renderedText = string.Empty;
    [GlobalSetup]
    public void Setup()
    {
        device = CanvasDevice.GetSharedDevice();
        textFormat = new CanvasTextFormat
        {
            FontFamily = "Consolas",
            FontSize = 14,
            LineSpacing = 16,
            WordWrapping = CanvasWordWrapping.Wrap,
            TrimmingGranularity = CanvasTextTrimmingGranularity.None,
            TrimmingSign = CanvasTrimmingSign.None,
            HorizontalAlignment = CanvasHorizontalAlignment.Left,
            VerticalAlignment = CanvasVerticalAlignment.Top,
        };
        textManager = (TextManager)FormatterServices.GetUninitializedObject(typeof(TextManager));
        textManager.totalLines = new PooledList<string>(0);
        textManager.NewLineCharacter = "\r\n";
        for (int i = 0; i < 2000; i++)
        {
            textManager.AddLine($"public void Method{i}() {{ var value = {i}; var longer = \"This is a longer line to trigger wrapping and highlighting.\"; }}");
        }

        renderedText = textManager.GetLinesAsString();
        layout = new CanvasTextLayout(device, renderedText, textFormat, 600f, float.MaxValue);
        renderTarget = new CanvasRenderTarget(device, 800, 600, 96);
        visualLineMap = new VisualLineMap();
        syntaxHighlightingLanguage = new SyntaxHighlightLanguage
        {
            Highlights =
            [
                new SyntaxHighlights(@"\bpublic\b", "#0000FF", "#0000FF"),
                new SyntaxHighlights(@"\bvar\b", "#008000", "#008000"),
                new SyntaxHighlights(@"\bclass\b", "#2B91AF", "#2B91AF"),
            ]
        };
        syntaxHighlightingLanguage.CompileAllRegex();
    }

    [Benchmark]
    public void UpdateVisualLineMap()
    {
        visualLineMap.Update(layout, textManager);
    }

    [Benchmark]
    public void UpdateSyntaxHighlighting()
    {
        var slice = new LineSliceResult(renderedText, textManager.totalLines.Span);
        SyntaxHighlightingRenderer.UpdateSyntaxHighlighting(slice, textManager.NewLineCharacter, layout, ApplicationTheme.Light, syntaxHighlightingLanguage, true);
    }

    [Benchmark]
    public void DrawTextLayout()
    {
        using var ds = renderTarget.CreateDrawingSession();
        ds.DrawTextLayout(layout, 0, 0, Colors.Black);
    }
}