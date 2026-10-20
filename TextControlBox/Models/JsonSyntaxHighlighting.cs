using System.Text.Json.Serialization;

namespace TextControlBoxNS
{
    // The file format of a language loaded with GetSyntaxHighlightingFromJson. It was read by Newtonsoft.Json before,
    // so the options below keep its behavior: property names in any casing, comments and trailing commas.
    internal class JsonSyntaxHighlighting
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public string Filter { get; set; }
        public string Author { get; set; }
        public JsonHighlight[] Highlights { get; set; }
    }

    internal class JsonHighlight
    {
        public string Pattern { get; set; }
        public string ColorLight { get; set; }
        public string ColorDark { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public bool Underlined { get; set; }
        public JsonCodeStyle CodeStyle { get; set; }
    }

    internal class JsonCodeStyle
    {
        public bool Underlined { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
    }

    // Source generated, so loading a language does not need reflection and works with trimming and Native AOT.
    [JsonSourceGenerationOptions(
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip,
        AllowTrailingCommas = true)]
    [JsonSerializable(typeof(JsonSyntaxHighlighting))]
    internal partial class SyntaxHighlightingJsonContext : JsonSerializerContext
    {
    }
}
