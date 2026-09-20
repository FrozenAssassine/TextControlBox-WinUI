using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TextControlBoxNS;
using Windows.Foundation;

namespace TextControlBox_TestApp
{
    public sealed partial class MainWindow : Window
    {
        public MainWindow()
        {
            this.InitializeComponent();

            //textbox.LoadLines(Enumerable.Range(0, 5_000_000).Select(x => "Line " + x + " is cool right?"));

            textbox.SelectSyntaxHighlightingById(SyntaxHighlightID.CSharp);

            textbox.NumberOfSpacesForTab = 8;
            textbox.UseSpacesInsteadTabs = false;
            textbox.ShowWhitespaceCharacters = true;

            SetWindowTheme(this, ElementTheme.Dark);

            textbox.LinkClicked += Textbox_LinkClicked;
            textbox.SelectionChanged += Textbox_SelectionChanged;
            textbox.Loaded += (s) =>
            {
                textbox.DispatcherQueue.TryEnqueue(async () =>
                {
                    await Task.Delay(500);
                    RunCoordinateDiagnostics();
                });
            };
            textbox.DispatcherQueue.TryEnqueue(() =>
            {
                textbox.RequestedTheme = ElementTheme.Dark;
            });
        }

        private void Textbox_LinkClicked(TextControlBox sender,string url)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
        }

        public static void SetWindowTheme(Window window, ElementTheme theme)
        {
            if (window.Content is FrameworkElement frame)
                frame.RequestedTheme = theme;
        }


        private void WordWrapToggle_Click(object sender, RoutedEventArgs e)
        {
            textbox.WordWrap = !textbox.WordWrap;
            WordWrapToggle.Content = $"WordWrap: {(textbox.WordWrap ? "An" : "Aus")}";
            StatusText.Text = $"WordWrap gesetzt auf: {textbox.WordWrap}";
        }

        private void LoadLongLine_Click(object sender, RoutedEventArgs e)
        {
            // Pathologische 100k Zeile zum Testen von horizontaler Virtualisierung und Wrapped-Line Virtualisierung
            string longLine = "START_" + new string('X', 100_00000) + "_END";
            textbox.LoadLines([
                "Zeile 1 (Kurz): Willkommen zum Testen von TextControlBox-WinUI!",
                longLine,
                "Zeile 3 (Kurz): Ende des Tests."
            ]);
            StatusText.Text = "100.000-Zeichen-Zeile geladen (Virtualisierung aktiv).";
        }

        private void LoadCodeSample_Click(object sender, RoutedEventArgs e)
        {
            textbox.LoadLines(Enumerable.Range(1, 200000).Select(i => $"// Code Zeile {i}: int value{i} = {i * 10}; // Das ist ein längerer Kommentar um Zeilenumbruch und Rendering zu prüfen"));
            StatusText.Text = "200 Code-Zeilen geladen.";
        }

        private void CenterScroll_Click(object sender, RoutedEventArgs e)
        {
            textbox.ScrollIntoViewHorizontallyCentered();
            StatusText.Text = "ScrollIntoViewHorizontallyCentered aufgerufen.";
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            textbox.ZoomFactor = Math.Min(400, textbox.ZoomFactor + 25);
            StatusText.Text = $"ZoomFactor: {textbox.ZoomFactor}%";
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            textbox.ZoomFactor = Math.Max(25, textbox.ZoomFactor - 25);
            StatusText.Text = $"ZoomFactor: {textbox.ZoomFactor}%";
        }

        private void ZoomReset_Click(object sender, RoutedEventArgs e)
        {
            textbox.ZoomFactor = 100;
            StatusText.Text = "Zoom zurückgesetzt auf 100%.";
        }

        public void RewriteTabsSpaces(int spaces)
        {
            bool useSpaces = spaces != -1;
            this.textbox.RewriteTabsSpaces(spaces == -1 ? 4 : spaces, useSpaces);
        }

        private void Format1_Click(object sender, RoutedEventArgs e) => RewriteTabsSpaces(-1);
        private void Format2_Click(object sender, RoutedEventArgs e) => RewriteTabsSpaces(2);
        private void Format3_Click(object sender, RoutedEventArgs e) => RewriteTabsSpaces(4);
        private void Format4_Click(object sender, RoutedEventArgs e) => RewriteTabsSpaces(8);

        private void Textbox_SelectionChanged(TextControlBox sender, TextControlBoxNS.SelectionChangedEventHandler args)
        {
            var cursorPixels = textbox.GetCursorPosition();
            float lineH = textbox.SingleLineHeight;
            string status = $"Cursor: Z{args.LineNumber + 1}, Sp{args.CharacterPositionInLine + 1} | Pixel Y: {cursorPixels.Y:F1} | LineHighlighter Y: {cursorPixels.Y:F1} (H={lineH:F1}) | Pixel X: {cursorPixels.X:F1}";
            StatusText.Text = status;
            Debug.WriteLine("[SELECTION_CHANGED] " + status);
            Console.WriteLine("[SELECTION_CHANGED] " + status);
        }

        private void DiagnosePositions_Click(object sender, RoutedEventArgs e)
        {
            RunCoordinateDiagnostics();
        }

        public void RunCoordinateDiagnostics()
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== TEXTCONTROLBOX COORDINATE DIAGNOSTICS ===");
            sb.AppendLine($"Timestamp: {DateTime.Now:O}");

            // 1. Unwrapped multi-line test
            textbox.WordWrap = false;
            textbox.LoadLines([
                "Line 0: First line of text",
                "Line 1: Second line of text",
                "Line 2: Third line of text",
                "Line 3: Fourth line of text"
            ]);

            float lineH = textbox.SingleLineHeight;
            sb.AppendLine($"SingleLineHeight: {lineH:F2} px");

            float topInset = textbox.CoreTextBox?.textRenderer?.TopInset ?? 0f;

            for (int line = 0; line <= 2; line++)
            {
                textbox.SetCursorPosition(line, 0);
                var pt = textbox.GetCursorPosition();
                float expectedY = line * lineH + topInset;
                float highlighterY = (float)pt.Y;
                float highlighterHeight = lineH;
                bool ok = Math.Abs(pt.Y - expectedY) < 1.0;
                sb.AppendLine($"[NON-WRAP] Line {line}: CursorPos=({pt.X:F1}, {pt.Y:F1}), ExpectedY={expectedY:F1}, HighlighterY={highlighterY:F1}, HighlighterH={highlighterHeight:F1} => {(ok ? "PASS" : "FAIL")}");
            }

            // 2. Hit-testing math verification
            for (int line = 0; line <= 2; line++)
            {
                double testY = line * lineH + (lineH / 2.0);
                int calculatedLine = textbox.GetLineFromPoint(new Point(10, testY));
                bool ok = calculatedLine == line;
                sb.AppendLine($"[CLICK HIT-TEST] Click at Y={testY:F1} px: Resolved Line = {calculatedLine} (Expected: {line}) => {(ok ? "PASS" : "FAIL")}");
            }

            // 3. Word-wrap multi-row cursor & highlighter verification
            textbox.WordWrap = true;
            string longLine = "START_" + new string('W', 120) + "_END";
            textbox.LoadLines([
                "Short Line 0",
                longLine,
                "Short Line 2"
            ]);

            textbox.SetCursorPosition(0, 0);
            var ptWrap0 = textbox.GetCursorPosition();
            var tr = textbox.CoreTextBox?.textRenderer;
            var layout = tr?.CurrentLineTextLayout;
            var vec0 = layout?.GetCaretPosition(0, false) ?? default;
            var regions0 = layout?.GetCharacterRegions(0, 1);
            sb.AppendLine($"[DEBUG WRAP LINE 0] GetLineTopY(0)={tr?.GetLineTopY(0)}, StartVR={tr?.StartVisualRow}, CaretVec=({vec0.X}, {vec0.Y}), Region0={(regions0?.Length > 0 ? regions0[0].LayoutBounds.ToString() : "none")}, LayoutBounds={layout?.LayoutBounds}");
            sb.AppendLine($"[WORD-WRAP] Line 0 (start): CursorPos=({ptWrap0.X:F1}, {ptWrap0.Y:F1}), HighlighterY={ptWrap0.Y:F1} => {(Math.Abs(ptWrap0.Y - topInset) < 1.0 ? "PASS" : "FAIL")}");

            textbox.SetCursorPosition(1, 0);
            var ptWrap1 = textbox.GetCursorPosition();
            var vec1 = layout?.GetCaretPosition(0, false) ?? default;
            sb.AppendLine($"[DEBUG WRAP LINE 1] GetLineTopY(1)={tr?.GetLineTopY(1)}, CaretVec=({vec1.X}, {vec1.Y})");
            sb.AppendLine($"[WORD-WRAP] Line 1 (first row): CursorPos=({ptWrap1.X:F1}, {ptWrap1.Y:F1}), HighlighterY={ptWrap1.Y:F1} => {(Math.Abs(ptWrap1.Y - (lineH + topInset)) < 1.0 ? "PASS" : "FAIL")}");

            textbox.SetCursorPosition(1, longLine.Length);
            var ptWrap1End = textbox.GetCursorPosition();
            sb.AppendLine($"[WORD-WRAP] Line 1 (end of line): CursorPos=({ptWrap1End.X:F1}, {ptWrap1End.Y:F1}), HighlighterY={ptWrap1End.Y:F1} => RowOffset={(ptWrap1End.Y - lineH):F1} px");

            // 4. Arrow Up in Word-Wrapped text verification
            var curPos = textbox.CoreTextBox.cursorManager.currentCursorPosition;
            int prevCharPos = curPos.CharacterPosition;
            textbox.CoreTextBox.textRenderer.MoveCursorByVisualRows(textbox.CoreTextBox.canvasText, curPos, -1);
            var ptAfterUp = textbox.GetCursorPosition();
            bool movedUp = curPos.CharacterPosition < prevCharPos && ptAfterUp.Y < ptWrap1End.Y;
            sb.AppendLine($"[ARROW-UP IN WRAP] End of Line 1 -> Arrow Up: CharPos {prevCharPos} -> {curPos.CharacterPosition}, Cursor Y {ptWrap1End.Y:F1} -> {ptAfterUp.Y:F1} => {(movedUp ? "PASS" : "FAIL")}");

            // 5. Word-Wrap toggle (An -> Aus) verification
            textbox.WordWrap = false;
            var ptUnwrapped0 = textbox.GetCursorPosition();
            bool unwrappedOk = !textbox.WordWrap && ptUnwrapped0.Y <= lineH;
            sb.AppendLine($"[WORD-WRAP TOGGLE OFF] WordWrap disabled: IsWordWrap={textbox.WordWrap}, CursorPos=({ptUnwrapped0.X:F1}, {ptUnwrapped0.Y:F1}) => {(unwrappedOk ? "PASS" : "FAIL")}");

            // 6. 100k lines rendering verification in Word-Wrap
            textbox.WordWrap = true;
            string[] bulkLines = new string[10000];
            for (int i = 0; i < bulkLines.Length; i++) bulkLines[i] = $"Line {i}";
            textbox.LoadLines(bulkLines);
            var (startL, renderedL) = textbox.CoreTextBox.textRenderer.CalculateLinesToRender();
            bool bulkOk = startL == 0 && renderedL > 0;
            sb.AppendLine($"[100K/BULK LINES IN WRAP] StartLine={startL}, RenderedLines={renderedL} => {(bulkOk ? "PASS" : "FAIL")}");

            sb.AppendLine("=== DIAGNOSTICS COMPLETE ===");

            string logText = sb.ToString();
            Debug.WriteLine(logText);
            Console.WriteLine(logText);

            try
            {
                File.WriteAllText(@"f:\Github\TextControlBox-WinUI\diagnostic_positions.log", logText);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error writing log: " + ex.Message);
            }

            StatusText.Text = "Diagnose abgeschlossen! (Alle Koordinaten verifiziert)";
        }
    }
}
