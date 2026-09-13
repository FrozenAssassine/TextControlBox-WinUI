using System;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TextControlBoxNS;

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
            string longLine = "START_" + new string('X', 100_000) + "_END";
            textbox.LoadLines([
                "Zeile 1 (Kurz): Willkommen zum Testen von TextControlBox-WinUI!",
                longLine,
                "Zeile 3 (Kurz): Ende des Tests."
            ]);
            StatusText.Text = "100.000-Zeichen-Zeile geladen (Virtualisierung aktiv).";
        }

        private void LoadCodeSample_Click(object sender, RoutedEventArgs e)
        {
            textbox.LoadLines(Enumerable.Range(1, 200).Select(i => $"// Code Zeile {i}: int value{i} = {i * 10}; // Das ist ein längerer Kommentar um Zeilenumbruch und Rendering zu prüfen"));
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
    }
}
