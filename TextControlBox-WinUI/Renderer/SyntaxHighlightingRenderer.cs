using Microsoft.Graphics.Canvas.Text;
using Microsoft.UI.Xaml;
using Newtonsoft.Json;
using System;
using System.Drawing;
using System.Text.RegularExpressions;
using TextControlBox.Extensions;
using Windows.UI.Text;
using Windows.UI.Xaml;

namespace TextControlBox.Renderer
{
    internal class SyntaxHighlightingRenderer
    {
        public static FontWeight BoldFont = new FontWeight { Weight = 600 };
        public static FontStyle ItalicFont = FontStyle.Italic;

        public static void UpdateSyntaxHighlighting(CanvasTextLayout DrawnTextLayout, ApplicationTheme theme, CodeLanguage CodeLanguage, bool SyntaxHighlighting, string RenderedText)
        {
            if (CodeLanguage == null || !SyntaxHighlighting)
                return;

            var highlights = CodeLanguage.Highlights;
            for (int i = 0; i < highlights.Length; i++)
            {
                var matches = Regex.Matches(RenderedText, highlights[i].Pattern, RegexOptions.Compiled);
                var highlight = highlights[i];
                var color = theme == ApplicationTheme.Light ? highlight.ColorLight_Clr : highlight.ColorDark_Clr;

                for (int j = 0; j < matches.Count; j++)
                {
                    var match = matches[j];
                    int index = match.Index;
                    int length = match.Length;
                    DrawnTextLayout.SetColor(index, length, color);
                    if (highlight.CodeStyle != null)
                    {
                        if (highlight.CodeStyle.Italic)
                            DrawnTextLayout.SetFontStyle(index, length, ItalicFont);
                        if (highlight.CodeStyle.Bold)
                            DrawnTextLayout.SetFontWeight(index, length, BoldFont);
                        if (highlight.CodeStyle.Underlined)
                            DrawnTextLayout.SetUnderline(index, length, true);
                    }
                }
            }
        }

        public static JsonLoadResult GetCodeLanguageFromJson(string Json)
        {
            try
            {
                var jsonCodeLanguage = JsonConvert.DeserializeObject<JsonCodeLanguage>(Json);
                //Apply the filter as an array
                var codelanguage = new CodeLanguage
                {
                    Author = jsonCodeLanguage.Author,
                    Description = jsonCodeLanguage.Description,
                    Highlights = jsonCodeLanguage.Highlights,
                    Name = jsonCodeLanguage.Name,
                    Filter = jsonCodeLanguage.Filter.Split("|", StringSplitOptions.RemoveEmptyEntries),
                };
                return new JsonLoadResult(true, codelanguage);
            }
            catch (JsonReaderException)
            {
                return new JsonLoadResult(false, null);
            }
            catch (JsonSerializationException)
            {
                return new JsonLoadResult(false, null);
            }
        }
    }
    public class JsonLoadResult
    {
        public JsonLoadResult(bool Succeed, CodeLanguage CodeLanguage)
        {
            this.Succeed = Succeed;
            this.CodeLanguage = CodeLanguage;
        }
        /// <summary>
        /// true if the loading succeed
        /// </summary>
        public bool Succeed { get; set; }
        /// <summary>
        /// The codelanguage loaded
        /// </summary>
        public CodeLanguage CodeLanguage { get; set; }
    }
    public class SyntaxHighlights
    {
        private readonly ColorConverter ColorConverter = new ColorConverter();

        public SyntaxHighlights(string Pattern, string ColorLight, string ColorDark, bool Bold = false, bool Italic = false, bool Underlined = false)
        {
            this.Pattern = Pattern;
            this.ColorDark = ColorDark;
            this.ColorLight = ColorLight;
            if (Underlined || Italic || Bold)
                this.CodeStyle = new CodeFontStyle(Underlined, Italic, Bold);
        }
        public CodeFontStyle CodeStyle { get; set; } = null;
        public string Pattern { get; set; }
        public Windows.UI.Color ColorDark_Clr { get; private set; }
        public Windows.UI.Color ColorLight_Clr { get; private set; }
        public string ColorDark
        {
            set => ColorDark_Clr = ((System.Drawing.Color)ColorConverter.ConvertFromString(value)).ToMediaColor();
        }
        public string ColorLight
        {
            set => ColorLight_Clr = ((System.Drawing.Color)ColorConverter.ConvertFromString(value)).ToMediaColor();
        }
    }
    public class CodeFontStyle
    {
        public CodeFontStyle(bool Underlined, bool Italic, bool Bold)
        {
            this.Italic = Italic;
            this.Bold = Bold;
            this.Underlined = Underlined;
        }
        public bool Underlined { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
    }
    public class CodeLanguage
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string[] Filter { get; set; }
        public string Author { get; set; }
        public SyntaxHighlights[] Highlights;
    }
    //Used to create a CodeLanguage class from the JsonCodeLanguage class returned by the json load function:
    internal class JsonCodeLanguage
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Filter { get; set; }
        public string Author { get; set; }
        public SyntaxHighlights[] Highlights { get; set; }
    }
}
