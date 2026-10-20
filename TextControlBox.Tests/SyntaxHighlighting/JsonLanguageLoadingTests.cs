using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextControlBoxNS;
using TextControlBoxNS.Core.Renderer;
using static TextControlBox.Tests.SyntaxHighlighting.HighlightSimulator;

namespace TextControlBox.Tests.SyntaxHighlighting;

// Languages saved by apps (e.g. FastEdit) were written for the Newtonsoft.Json based loader, so these files have to keep loading.
[TestClass]
public class JsonLanguageLoadingTests
{
    private const string FullLanguage = """
        {
            "Name": "MyLang",
            "Description": "A test language",
            "Author": "Julius",
            "Filter": ".my|.mine",
            "Highlights": [
                { "Pattern": "\\b(let|fn)\\b", "ColorLight": "#0000FF", "ColorDark": "#00FFFF", "Bold": true },
                { "Pattern": "\"[^\"]*\"", "ColorLight": "#008000", "ColorDark": "#00FF00", "Italic": true, "Underlined": true },
                { "Pattern": "//.*", "ColorLight": "#808080", "ColorDark": "#646464" }
            ]
        }
        """;

    private static SyntaxHighlightLanguage Load(string json)
    {
        var result = SyntaxHighlightingRenderer.GetSyntaxHighlightingFromJson(json);
        Assert.IsTrue(result.Succeed, "the language did not load");
        Assert.IsNotNull(result.SyntaxHighlighting);
        return result.SyntaxHighlighting;
    }

    [TestMethod]
    public void LoadsAllProperties()
    {
        var language = Load(FullLanguage);

        Assert.AreEqual("MyLang", language.Name);
        Assert.AreEqual("A test language", language.Description);
        Assert.AreEqual("Julius", language.Author);
        CollectionAssert.AreEqual(new[] { ".my", ".mine" }, language.Filter);
        Assert.HasCount(3, language.Highlights);

        var keyword = language.Highlights[0];
        Assert.AreEqual(@"\b(let|fn)\b", keyword.Pattern);
        Assert.AreEqual("#0000FF", ToHex(keyword.ColorLight_Clr));
        Assert.AreEqual("#00FFFF", ToHex(keyword.ColorDark_Clr));
        Assert.IsTrue(keyword.CodeStyle.Bold);
        Assert.IsFalse(keyword.CodeStyle.Italic);

        var str = language.Highlights[1];
        Assert.IsTrue(str.CodeStyle.Italic);
        Assert.IsTrue(str.CodeStyle.Underlined);
        Assert.IsFalse(str.CodeStyle.Bold);

        Assert.IsNull(language.Highlights[2].CodeStyle);
    }

    [TestMethod]
    public void LoadedLanguage_Highlights()
    {
        var language = Load(FullLanguage);
        const string text = "let x = \"hi\" // note";
        AssertColor(language, text, "let", "#00FFFF");
        AssertColor(language, text, "\"hi\"", "#00FF00");
        AssertColor(language, text, "// note", "#646464");
        AssertColor(language, text, "x", null);
    }

    [TestMethod]
    public void PropertyNames_AreCaseInsensitive()
    {
        var language = Load("""
            { "name": "lower", "filter": ".x", "highlights": [ { "pattern": "a", "colorLight": "#111111", "colordark": "#222222", "bold": true } ] }
            """);

        Assert.AreEqual("lower", language.Name);
        Assert.AreEqual("a", language.Highlights[0].Pattern);
        Assert.AreEqual("#222222", ToHex(language.Highlights[0].ColorDark_Clr));
        Assert.IsTrue(language.Highlights[0].CodeStyle.Bold);
    }

    [TestMethod]
    public void CodeStyleObject_IsRead()
    {
        var language = Load("""
            { "Name": "x", "Filter": ".x", "Highlights": [ { "Pattern": "a", "ColorLight": "#111111", "ColorDark": "#222222", "CodeStyle": { "Italic": true, "Underlined": true } } ] }
            """);

        Assert.IsTrue(language.Highlights[0].CodeStyle.Italic);
        Assert.IsTrue(language.Highlights[0].CodeStyle.Underlined);
        Assert.IsFalse(language.Highlights[0].CodeStyle.Bold);
    }

    [TestMethod]
    public void UnknownProperties_AreIgnored()
    {
        var language = Load("""
            { "Name": "x", "Version": 3, "Filter": ".x", "Highlights": [ { "Pattern": "a", "ColorLight": "#111111", "ColorDark": "#222222", "Comment": "keyword", "ColorDark_Clr": { "A": 1 } } ] }
            """);

        Assert.AreEqual("#222222", ToHex(language.Highlights[0].ColorDark_Clr));
    }

    [TestMethod]
    public void CommentsAndTrailingCommas_AreAccepted()
    {
        var language = Load("""
            {
                // the name
                "Name": "x",
                "Filter": ".x", /* filters */
                "Highlights": [ { "Pattern": "a", "ColorLight": "#111111", "ColorDark": "#222222", }, ],
            }
            """);

        Assert.HasCount(1, language.Highlights);
    }

    [TestMethod]
    public void NamedColors_AreAccepted()
    {
        var language = Load("""
            { "Name": "x", "Filter": ".x", "Highlights": [ { "Pattern": "a", "ColorLight": "Red", "ColorDark": "Lime" } ] }
            """);

        Assert.AreEqual("#FF0000", ToHex(language.Highlights[0].ColorLight_Clr));
        Assert.AreEqual("#00FF00", ToHex(language.Highlights[0].ColorDark_Clr));
    }

    [TestMethod]
    public void MissingDescriptionAndAuthor_AreNull()
    {
        var language = Load("""{ "Name": "x", "Filter": ".x", "Highlights": [] }""");
        Assert.IsNull(language.Description);
        Assert.IsNull(language.Author);
        Assert.IsEmpty(language.Highlights);
    }

    // The Newtonsoft based loader threw a NullReferenceException here instead of loading the language.
    [TestMethod]
    public void MissingFilter_LoadsWithoutFilter()
    {
        var language = Load("""{ "Name": "x", "Highlights": [] }""");
        Assert.IsEmpty(language.Filter);
    }

    // These threw an exception out of GetSyntaxHighlightingFromJson before.
    [TestMethod]
    public void NullOrBadColor_DoesNotSucceed()
    {
        Assert.IsFalse(SyntaxHighlightingRenderer.GetSyntaxHighlightingFromJson("null").Succeed);
        Assert.IsFalse(SyntaxHighlightingRenderer.GetSyntaxHighlightingFromJson("""{ "Name": "x", "Filter": ".x", "Highlights": [ { "Pattern": "a", "ColorLight": "#GGGGGG", "ColorDark": "#222222" } ] }""").Succeed);
        Assert.IsFalse(SyntaxHighlightingRenderer.GetSyntaxHighlightingFromJson("""{ "Name": "x", "Filter": ".x", "Highlights": [ { "Pattern": "a" } ] }""").Succeed);
    }

    [TestMethod]
    public void InvalidJson_DoesNotSucceed()
    {
        Assert.IsFalse(SyntaxHighlightingRenderer.GetSyntaxHighlightingFromJson("{ \"Name\": ").Succeed);
        Assert.IsFalse(SyntaxHighlightingRenderer.GetSyntaxHighlightingFromJson("not json").Succeed);
        Assert.IsFalse(SyntaxHighlightingRenderer.GetSyntaxHighlightingFromJson("""{ "Name": "x", "Filter": ".x", "Highlights": 5 }""").Succeed);
    }
}
