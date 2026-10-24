using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextControlBoxNS;
using TextControlBoxNS.Languages;
using static TextControlBox.Tests.SyntaxHighlighting.HighlightSimulator;

namespace TextControlBox.Tests.SyntaxHighlighting;

// Every test describes what a rule is meant for, using the dark theme colors from Languages.cs.
// null means the text stays in the default text color.

[TestClass]
public class CSharpHighlightTests
{
    private static readonly SyntaxHighlightLanguage Lang = new CSharp();
    private const string Number = "#ff00ff", Function = "#ffbb00", Keyword = "#00ffff", Type = "#ff9900",
        TryCatch = "#6666ff", Region = "#ff0000", String = "#00FF00", Comment = "#646464";

    [TestMethod] public void Keywords() { AssertColor(Lang, "public class Foo", "public", Keyword); AssertColor(Lang, "public class Foo", "class", Keyword); AssertColor(Lang, "public class Foo", "Foo", null); }
    [TestMethod] public void KeywordInsideIdentifier_IsNotHighlighted() => AssertColor(Lang, "var classic = 1;", "classic", null);
    [TestMethod] public void Numbers() { AssertColor(Lang, "int x = 42;", "42", Number); AssertColor(Lang, "var h = 0xFF;", "0xFF", Number); AssertColor(Lang, "var f = 1.5f;", "1.5f", Number); }
    [TestMethod] public void DigitInsideIdentifier_IsNotANumber() => AssertColor(Lang, "var item2 = a;", "item2", null);
    [TestMethod] public void FunctionCall() => AssertColor(Lang, "DoWork();", "DoWork", Function);
    [TestMethod] public void CommonTypes() => AssertColor(Lang, "Console.WriteLine(x);", "Console", Type);
    [TestMethod] public void TryCatchFinally() { const string t = "try { } catch { } finally { }"; AssertColor(Lang, t, "try", TryCatch); AssertColor(Lang, t, "catch", TryCatch); AssertColor(Lang, t, "finally", TryCatch); }
    [TestMethod] public void RegionDirective() => AssertColor(Lang, "#region Init", "#region Init", Region);
    [TestMethod] public void StringLiteral() => AssertColor(Lang, "string s = \"hello world\";", "\"hello world\"", String);
    [TestMethod] public void StringWithEscapedQuotes() => AssertColor(Lang, "string s = \"say \\\"hi\\\"\";", "\"say \\\"hi\\\"\"", String);
    [TestMethod] public void VerbatimString() => AssertColor(Lang, "var p = @\"C:\\temp\\\";", "@\"C:\\temp\\\"", String);
    [TestMethod] public void CharLiteral() => AssertColor(Lang, "char c = 'a';", "'a'", String);
    [TestMethod] public void LineComment() => AssertColor(Lang, "x = y; // return 42 class", "// return 42 class", Comment);
    [TestMethod] public void BlockComment_OverMultipleLines() => AssertColor(Lang, "/* a\nint b */", "int b */", Comment);
    [TestMethod] public void SlashesInsideString_AreNotAComment() => AssertColor(Lang, "var url = \"https://example.com\";", "\"https://example.com\"", String);
}

[TestClass]
public class CppHighlightTests
{
    private static readonly SyntaxHighlightLanguage Lang = new Cpp();
    private const string Number = "#00ff00", Function = "#ffbb00", Keyword = "#dd00dd", Preprocessor = "#999999", String = "#00FF00", Comment = "#646464";

    [TestMethod] public void Include() => AssertColor(Lang, "#include <vector>", "#include", Preprocessor);
    [TestMethod] public void IndentedDefine() => AssertColor(Lang, "int a;\n  #define MAX 10", "#define", Preprocessor);
    [TestMethod] public void KeywordsAndFunction() { const string t = "int main() {"; AssertColor(Lang, t, "int", Keyword); AssertColor(Lang, t, "main", Function); }
    [TestMethod] public void Numbers() { AssertColor(Lang, "return 0;", "0", Number); AssertColor(Lang, "float f = 1.5f;", "1.5f", Number); AssertColor(Lang, "x = 0x1F;", "0x1F", Number); }
    [TestMethod] public void StringWithEscapedQuote() => AssertColor(Lang, "s = \"a\\\"b\";", "\"a\\\"b\"", String);
    [TestMethod] public void LineComment() => AssertColor(Lang, "x++; // int", "// int", Comment);
    [TestMethod] public void BlockComment() => AssertColor(Lang, "/* multi\nline int */", "line int */", Comment);
    [TestMethod] public void SlashesInsideString_AreNotAComment() => AssertColor(Lang, "printf(\"%d // not a comment\", x);", "\"%d // not a comment\"", String);
}

[TestClass]
public class JavaHighlightTests
{
    private static readonly SyntaxHighlightLanguage Lang = new Java();
    private const string Number = "#ff00ff", Function = "#ffbb00", System = "#ff9900", Keyword = "#00ffff", String = "#00FF00", Comment = "#646464";

    [TestMethod]
    public void MainMethod()
    {
        const string t = "public static void main(String[] args) {";
        AssertColor(Lang, t, "public", Keyword);
        AssertColor(Lang, t, "static", Keyword);
        AssertColor(Lang, t, "void", Keyword);
        AssertColor(Lang, t, "main", Function);
    }
    [TestMethod]
    public void Println()
    {
        const string t = "System.out.println(\"Hi\");";
        AssertColor(Lang, t, "System.out", System);
        AssertColor(Lang, t, "println", Function);
        AssertColor(Lang, t, "\"Hi\"", String);
    }
    [TestMethod] public void NumberAndComment() { const string t = "int n = 10; // count 5"; AssertColor(Lang, t, "10", Number); AssertColor(Lang, t, "// count 5", Comment); }
    [TestMethod] public void JavaDoc() => AssertColor(Lang, "/** javadoc int */", "/** javadoc int */", Comment);
    [TestMethod] public void BooleanLiteral() => AssertColor(Lang, "boolean flag = true;", "true", Keyword);
    [TestMethod] public void StringWithEscapedQuotes() => AssertColor(Lang, "String s = \"a \\\"b\\\" c\";", "\"a \\\"b\\\" c\"", String);
    [TestMethod] public void SlashesInsideString_AreNotAComment() => AssertColor(Lang, "String url = \"http://x.org\";", "\"http://x.org\"", String);
}

[TestClass]
public class JavascriptHighlightTests
{
    private static readonly SyntaxHighlightLanguage Lang = new Javascript();
    private const string Operator = "#77FF77", Number = "#ff00ff", Function = "#ffbb00", Keyword = "#00ffff", Dom = "#33BB00",
        RegexLiteral = "#FFFF00", String = "#00FF00", Comment = "#646464";

    [TestMethod] public void Declaration() { const string t = "const x = 5;"; AssertColor(Lang, t, "const", Keyword); AssertColor(Lang, t, "=", Operator); AssertColor(Lang, t, "5", Number); AssertColor(Lang, t, "x", null); }
    [TestMethod] public void FunctionDeclaration() { const string t = "function greet(name) {"; AssertColor(Lang, t, "function", Keyword); AssertColor(Lang, t, "greet", Function); }
    [TestMethod] public void DomGlobals() { const string t = "document.getElementById(\"app\");"; AssertColor(Lang, t, "document", Dom); AssertColor(Lang, t, "getElementById", Function); AssertColor(Lang, t, "\"app\"", String); }
    [TestMethod] public void RegexLiteralIsHighlighted() => AssertColor(Lang, "const re = /ab+c/i;", "/ab+c/i", RegexLiteral);
    [TestMethod] public void TwoDivisions_AreNotARegexLiteral() => AssertColor(Lang, "let half = a / 2, third = b / 3;", "third", null);
    [TestMethod] public void LineComment() => AssertColor(Lang, "x(); // comment let", "// comment let", Comment);
    [TestMethod] public void ApostropheInsideDoubleQuotedString() => AssertColor(Lang, "const s = \"it's\";", "\"it's\"", String);
    [TestMethod] public void SlashesInsideString_AreNotAComment() => AssertColor(Lang, "const url = \"https://a.b\";", "\"https://a.b\"", String);
}

[TestClass]
public class JsonHighlightTests
{
    private static readonly SyntaxHighlightLanguage Lang = new Json();
    private const string Number = "#ff00ff", Literal = "#0099ff", Punctuation = "#646464", Key = "#dddd00", String = "#00FF00";

    [TestMethod] public void Key_HasKeyColor() => AssertColor(Lang, "{\"name\": \"Julius\"}", "\"name\"", Key);
    [TestMethod] public void Key_IsDistinguishableFromValue() => Assert.AreNotEqual(ColorAt(Lang, "{\"name\": \"Julius\"}", "\"name\""), ColorAt(Lang, "{\"name\": \"Julius\"}", "\"Julius\""));
    [TestMethod] public void StringValue() => AssertColor(Lang, "{\"name\": \"Julius\"}", "\"Julius\"", String);
    [TestMethod] public void NumberValue() { AssertColor(Lang, "{\"age\": 42}", "42", Number); AssertColor(Lang, "{\"r\": 1.5e3}", "1.5e3", Number); }
    [TestMethod] public void Literals() { AssertColor(Lang, "{\"ok\": true, \"v\": null}", "true", Literal); AssertColor(Lang, "{\"ok\": true, \"v\": null}", "null", Literal); }
    [TestMethod] public void LiteralsAndNumbersInsideString_AreString() => AssertColor(Lang, "{\"v\": \"contains true and 42\"}", "\"contains true and 42\"", String);
    [TestMethod] public void StringWithEscapedQuotes() => AssertColor(Lang, "{\"q\": \"say \\\"hi\\\"\"}", "\"say \\\"hi\\\"\"", String);
    [TestMethod] public void Punctuation_() { AssertColor(Lang, "[1, 2]", "[", Punctuation); AssertColor(Lang, "[1, 2]", ",", Punctuation); }
}

[TestClass]
public class PythonHighlightTests
{
    private static readonly SyntaxHighlightLanguage Lang = new Python();
    private const string Number = "#ff00ff", Keyword = "#cc00ff", Function = "#ffbb00", String = "#00FF00", Comment = "#646464";

    [TestMethod] public void Def() { const string t = "def greet(name):"; AssertColor(Lang, t, "def", Keyword); AssertColor(Lang, t, "greet", Function); }
    [TestMethod] public void Keywords() { AssertColor(Lang, "return None", "return", Keyword); AssertColor(Lang, "return None", "None", Keyword); }
    [TestMethod] public void KeywordInsideIdentifier_IsNotHighlighted() => AssertColor(Lang, "classic = 1", "classic", null);
    [TestMethod] public void NumberLiteral() => AssertColor(Lang, "x = 3.14", "3.14", Number);
    [TestMethod] public void Strings() { AssertColor(Lang, "s = \"hello\"", "\"hello\"", String); AssertColor(Lang, "s = 'hi'", "'hi'", String); }
    [TestMethod] public void Comment_() => AssertColor(Lang, "x = 1  # comment with if", "# comment with if", Comment);
    [TestMethod] public void Docstring_OverMultipleLines() => AssertColor(Lang, "\"\"\"doc\nstring with def\"\"\"", "string with def", Comment);
    [TestMethod] public void HashInsideString_IsNotAComment() => AssertColor(Lang, "print(\"#not a comment\")", "\"#not a comment\"", String);
}

[TestClass]
public class LuaHighlightTests
{
    private static readonly SyntaxHighlightLanguage Lang = new Lua();
    private const string Number = "#ff00ff", Keyword = "#cc00ff", Function = "#ffbb00", String = "#00FF00", Comment = "#646464";

    [TestMethod] public void LocalFunction() { const string t = "local function add(a, b)"; AssertColor(Lang, t, "local", Keyword); AssertColor(Lang, t, "function", Keyword); AssertColor(Lang, t, "add", Function); }
    [TestMethod] public void ReturnEnd() { AssertColor(Lang, "return a end", "return", Keyword); AssertColor(Lang, "return a end", "end", Keyword); }
    [TestMethod] public void Numbers() { AssertColor(Lang, "x = 0x1F", "0x1F", Number); AssertColor(Lang, "x = 3.5", "3.5", Number); }
    [TestMethod] public void Strings() { AssertColor(Lang, "print(\"hi\")", "\"hi\"", String); AssertColor(Lang, "s = 'a\\'b'", "'a\\'b'", String); }
    [TestMethod] public void LineComment() => AssertColor(Lang, "x = 1 -- a comment local", "-- a comment local", Comment);
    [TestMethod] public void BlockComment() => AssertColor(Lang, "--[[ multi\nline local ]]", "line local ]]", Comment);
    [TestMethod] public void DashesInsideString_AreNotAComment() => AssertColor(Lang, "local s = \"a -- b\"", "\"a -- b\"", String);
}

[TestClass]
public class CssHighlightTests
{
    private static readonly SyntaxHighlightLanguage Lang = new CSS();
    private const string Property = "#00ffff", Function = "#00ff99", Pseudo = "#fffd00", Selector = "#44ff00", Unit = "#ff0000",
        Number = "#cc00ff", AtRule = "#ff0000", Hex = "#cc00ff", String = "#ff8800", Special = "#bbbbbb", Comment = "#888888";

    [TestMethod] public void ClassSelector() => AssertColor(Lang, ".button {", ".button", Selector);
    [TestMethod] public void Property_() { const string t = ".a {\n  color: red;\n}"; AssertColor(Lang, t, "color", Property); AssertColor(Lang, t, ":", Special); AssertColor(Lang, t, ";", Special); }
    [TestMethod] public void NumberWithUnit() { const string t = "p {\n  margin: 10px;\n}"; AssertColor(Lang, t, "10", Number); AssertColor(Lang, t, "px", Unit); }
    [TestMethod] public void HexColor() => AssertColor(Lang, "p {\n  background: #fff;\n}", "#fff", Hex);
    [TestMethod] public void PseudoClass() => AssertColor(Lang, "a:hover {", ":hover", Pseudo);
    [TestMethod] public void AtRule_() => AssertColor(Lang, "@media screen {", "@media", AtRule);
    [TestMethod] public void Function_() => AssertColor(Lang, "p {\n  width: calc(100% - 10px);\n}", "calc", Function);
    [TestMethod] public void String_() => AssertColor(Lang, "p {\n  content: \"x\";\n}", "\"x\"", String);
    [TestMethod] public void Comment_() => AssertColor(Lang, "/* color: red; */", "/* color: red; */", Comment);
    [TestMethod] public void SecondSelectorOnOneLine() => AssertColor(Lang, ".a { color: red; } .b { margin: 0; }", ".b", Selector);
}

[TestClass]
public class HtmlHighlightTests
{
    private static readonly SyntaxHighlightLanguage Lang = new Html();
    private const string Attribute = "#9CDCFE", Tag = "#569CD6", String = "#CE9178", Comment = "#6A9955";

    [TestMethod] public void OpeningTag() { const string t = "<div class=\"box\">"; AssertColor(Lang, t, "<div", Tag); AssertColor(Lang, t, "\"box\"", String); }
    [TestMethod] public void AttributeName() => AssertColor(Lang, "<div class=\"box\">", "class=", Attribute);
    [TestMethod] public void ClosingAndSelfClosingTag() { AssertColor(Lang, "</div>", "</div>", Tag); AssertColor(Lang, "<br/>", "<br/>", Tag); }
    [TestMethod] public void Comment_() => AssertColor(Lang, "<!-- <p>comment</p> -->", "<!-- <p>comment</p> -->", Comment);
    [TestMethod] public void TextBetweenTags_IsNotHighlighted() => AssertColor(Lang, "<p>Hello</p><br/>", "Hello", null);
    [TestMethod] public void ApostrophesInText_AreNotAString() => AssertColor(Lang, "<p>It's Bob's car</p>", "s Bob", null);
}

[TestClass]
public class XmlHighlightTests
{
    private static readonly SyntaxHighlightLanguage Lang = new XML();
    private const string Tag = "#4EC9B0", Attribute = "#9CDCFE", String = "#CE9178", Comment = "#6A9955";

    [TestMethod] public void TagWithAttribute() { const string t = "<root attr=\"1\">"; AssertColor(Lang, t, "<root", Tag); AssertColor(Lang, t, "attr=", Attribute); AssertColor(Lang, t, "\"1\"", String); }
    [TestMethod] public void ClosingAndSelfClosing() { AssertColor(Lang, "</root>", "</root>", Tag); AssertColor(Lang, "<item/>", "<item/>", Tag); }
    [TestMethod] public void NamespacedTag() => AssertColor(Lang, "<ns:el>", "<ns:el>", Tag);
    [TestMethod] public void Comment_() => AssertColor(Lang, "<!-- <x a=\"1\"/> -->", "<!-- <x a=\"1\"/> -->", Comment);
    [TestMethod] public void TextContent_IsNotHighlighted() => AssertColor(Lang, "<a>hello</a>", "hello", null);
}

[TestClass]
public class SqlHighlightTests
{
    private static readonly SyntaxHighlightLanguage Lang = new SQL();
    private const string Keyword = "#FF6A00", String = "#42C22B", Number = "#2B3BFF", Aggregate = "#11C9DB";

    [TestMethod]
    public void UppercaseQuery()
    {
        const string t = "SELECT name FROM users WHERE id = 5;";
        AssertColor(Lang, t, "SELECT", Keyword);
        AssertColor(Lang, t, "FROM", Keyword);
        AssertColor(Lang, t, "WHERE", Keyword);
        AssertColor(Lang, t, "5", Number);
        AssertColor(Lang, t, "name", null);
    }
    [TestMethod] public void LowercaseQuery() { const string t = "select name from users where id = 5;"; AssertColor(Lang, t, "select", Keyword); AssertColor(Lang, t, "from", Keyword); }
    [TestMethod] public void String_() => AssertColor(Lang, "WHERE name = 'Bob'", "'Bob'", String);
    [TestMethod] public void AggregateFunction() => AssertColor(Lang, "SELECT COUNT(*) FROM t", "COUNT", Aggregate);
    [TestMethod] public void MultiWordKeyword() => AssertColor(Lang, "ORDER BY name", "ORDER BY", Keyword);
    [TestMethod] public void KeywordInsideLineComment_IsNotAKeyword() => AssertNotColor(Lang, "-- SELECT everything", "SELECT", Keyword);
}

[TestClass]
public class MarkdownHighlightTests
{
    private static readonly SyntaxHighlightLanguage Lang = new Markdown();
    private const string Header = "#FF2E7E", Bold = "#E18800", Italic = "#C61AFF", InlineCode = "#F1C40F", CodeBlock = "#F39C12",
        Quote = "#8DA284", List = "#2ECC71", Link = "#3498DB", Image = "#FF6F61";

    [TestMethod] public void Header_() => AssertColor(Lang, "# Title", "Title", Header);
    [TestMethod] public void Bold_() => AssertColor(Lang, "a **bold** b", "bold", Bold);
    [TestMethod] public void Italic_() => AssertColor(Lang, "a *italic* b", "italic", Italic);
    [TestMethod] public void InlineCode_() => AssertColor(Lang, "a `code` b", "code", InlineCode);
    [TestMethod] public void Quote_() => AssertColor(Lang, "> quote", "quote", Quote);
    [TestMethod] public void Lists() { AssertColor(Lang, "- item", "item", List); AssertColor(Lang, "1. first", "first", List); }
    [TestMethod] public void Link_() => AssertColor(Lang, "see [link](http://x)", "link", Link);
    [TestMethod] public void Image_() => AssertColor(Lang, "![img](a.png)", "img", Image);
    [TestMethod] public void UnderscoresInsideWords_AreNotItalic() => AssertColor(Lang, "call my_variable_name here", "variable", null);
    [TestMethod] public void ContentOfCodeBlock_IsNotMarkdown() => AssertColor(Lang, "```\n- not a list\n```", "not a list", CodeBlock);
}

[TestClass]
public class IniHighlightTests
{
    private static readonly SyntaxHighlightLanguage Lang = new IniHighlighter();
    private const string Section = "#B890FF", Bracket = "#9FA8DA", Key = "#FFB74D", String = "#80CBC4", Number = "#64B5F6", Bool = "#FFAB91", Comment = "#BDBDBD";

    [TestMethod] public void Section_() { AssertColor(Lang, "[General]", "General", Section); AssertColor(Lang, "[General]", "[", Bracket); }
    [TestMethod] public void Key_() { AssertColor(Lang, "name = Julius", "name", Key); AssertColor(Lang, "a = 1\n  indented key = 1", "indented key", Key); }
    [TestMethod] public void Values() { AssertColor(Lang, "count = 42", "42", Number); AssertColor(Lang, "enabled = true", "true", Bool); AssertColor(Lang, "path = \"C:\\x\"", "\"C:\\x\"", String); }
    [TestMethod] public void Comments() { AssertColor(Lang, "; comment = 1", "; comment = 1", Comment); AssertColor(Lang, "# comment", "# comment", Comment); }
    [TestMethod] public void KeyNeverSpansMultipleLines() => AssertColor(Lang, "[a]\nfoo\nbar = 1", "foo", null);
}

[TestClass]
public class TomlHighlightTests
{
    private static readonly SyntaxHighlightLanguage Lang = new TomlHighlighter();
    private const string ArrayTable = "#CE93D8", Section = "#B890FF", Key = "#FFB74D", String = "#80CBC4", Value = "#64B5F6", Bool = "#FFAB91", Comment = "#BDBDBD";

    [TestMethod] public void Table() => AssertColor(Lang, "[server]", "server", Section);
    [TestMethod] public void ArrayOfTables() => AssertColor(Lang, "[[products]]", "products", ArrayTable);
    [TestMethod] public void KeyAndString() { AssertColor(Lang, "name = \"app\"", "name", Key); AssertColor(Lang, "name = \"app\"", "\"app\"", String); }
    [TestMethod] public void DottedKey() => AssertColor(Lang, "a.b.c = 1", "a.b.c", Key);
    [TestMethod] public void InlineTableKey() => AssertColor(Lang, "point = { x = 1, y = 2 }", "y", Key);
    [TestMethod] public void Values() { AssertColor(Lang, "port = 8080", "8080", Value); AssertColor(Lang, "debug = false", "false", Bool); AssertColor(Lang, "created = 1979-05-27T07:32:00Z", "1979-05-27T07:32:00Z", Value); }
    [TestMethod] public void Comment_() => AssertColor(Lang, "x = 1 # comment", "# comment", Comment);
    [TestMethod] public void HashInsideString_IsNotAComment() => AssertColor(Lang, "url = \"http://host/#anchor\"", "\"http://host/#anchor\"", String);
}

[TestClass]
public class KlipperHighlightTests
{
    private static readonly SyntaxHighlightLanguage Lang = new KlipperHighlighter();
    private const string Section = "#B890FF", Key = "#FFB74D", Pin = "#4DB6AC", GCode = "#F06292", Number = "#64B5F6", Bool = "#FFAB91", Template = "#BA68C8", Comment = "#BDBDBD";

    [TestMethod] public void Section_() => AssertColor(Lang, "[stepper_x]", "stepper_x", Section);
    [TestMethod] public void KeyAndPin() { AssertColor(Lang, "step_pin: PF13", "step_pin", Key); AssertColor(Lang, "step_pin: PF13", "PF13", Pin); AssertColor(Lang, "endstop_pin: ^PG6", "^PG6", Pin); }
    [TestMethod] public void Number_() => AssertColor(Lang, "rotation_distance: 40", "40", Number);
    [TestMethod] public void GCodeInMacro() { const string t = "gcode:\n    G28\n    M104 S200"; AssertColor(Lang, t, "G28", GCode); AssertColor(Lang, t, "M104", GCode); }
    [TestMethod] public void Bool_() => AssertColor(Lang, "invert: True", "True", Bool);
    [TestMethod] public void Template_() => AssertColor(Lang, "    M104 S{printer.temp}", "{printer.temp}", Template);
    [TestMethod] public void Comment_() => AssertColor(Lang, "# comment", "# comment", Comment);
    [TestMethod] public void KeyWithEquals() => AssertColor(Lang, "run_current = 0.8", "run_current", Key);
    [TestMethod] public void SemicolonComment() => AssertColor(Lang, "; comment", "; comment", Comment);
}

[TestClass]
public class BatchHighlightTests
{
    private static readonly SyntaxHighlightLanguage Lang = new Batch();
    private const string Keyword = "#dd00dd", Label = "#ffff00", String = "#ffff00", Special = "#dd0077", Comment = "#888888";

    [TestMethod] public void EchoOff() { AssertColor(Lang, "@echo off", "echo", Keyword); AssertColor(Lang, "@echo off", "@", Special); }
    [TestMethod] public void SetAndGoto() { AssertColor(Lang, "set NAME=value", "set", Keyword); AssertColor(Lang, "goto start", "goto", Keyword); }
    [TestMethod] public void Label_() => AssertColor(Lang, ":start", ":start", Label);
    [TestMethod] public void Rem() => AssertColor(Lang, "rem a comment", "rem a comment", Comment);
    [TestMethod] public void String_() => AssertColor(Lang, "echo \"hello\"", "\"hello\"", String);
    [TestMethod] public void Variable() => AssertColor(Lang, "echo %PATH%", "%", Special);
    [TestMethod] public void WordStartingWithRem_IsNotAComment() => AssertColor(Lang, "echo remove the files", "remove the files", null);
    [TestMethod] public void ColonInText_IsNotALabel() => AssertColor(Lang, "echo Time: now", ": now", null);
    [TestMethod] public void KeywordAtTheEndOfAWord_IsNotAKeyword() => AssertColor(Lang, "echo reset done", "reset", null);
}

[TestClass]
public class GCodeHighlightTests
{
    private static readonly SyntaxHighlightLanguage Lang = new GCode();
    private const string Y = "#00ff00", X = "#ff0000", Z = "#0077ff", Feed = "#ffAA00", SpindleTool = "#ffff00", Number = "#9f009f", Command = "#00ffff", Comment = "#888888";

    [TestMethod]
    public void Move()
    {
        const string t = "G1 X10.5 Y-20 Z0.3 F3000\n";
        AssertColor(Lang, t, "G1", Command);
        AssertColor(Lang, t, "X", X);
        AssertColor(Lang, t, "Y", Y);
        AssertColor(Lang, t, "Z", Z);
        AssertColor(Lang, t, "F", Feed);
        AssertColor(Lang, t, "10.5", Number);
    }
    [TestMethod] public void SpindleCommand() { AssertColor(Lang, "M104 S200\n", "M104", Command); AssertColor(Lang, "M104 S200\n", "S", SpindleTool); }
    [TestMethod] public void Comment_() => AssertColor(Lang, "; comment X10", "; comment X10", Comment);
    [TestMethod] public void CommandOnTheLastLine() => AssertColor(Lang, "G1 X1\nG28", "G28", Command);
}

[TestClass]
public class X86AssemblyHighlightTests
{
    private static readonly SyntaxHighlightLanguage Lang = new x86Assembly();
    private const string Prefix = "#983cff", Instruction = "#8400ff", Register = "#c4aa00", Value = "#74bd00", Int = "#1994ff", Label = "#00ab5b", Comment = "#04d400";

    [TestMethod] public void Instruction_() => AssertColor(Lang, "mov eax, ebx", "mov", Instruction);
    [TestMethod] public void Registers() { AssertColor(Lang, "mov eax, ebx", "eax", Register); AssertColor(Lang, "mov eax, ebx", "ebx", Register); AssertColor(Lang, "mov eax, [ebx+4]", "ebx", Register); }
    [TestMethod] public void Value_() => AssertColor(Lang, "add eax, 0x10", "0x10", Value);
    [TestMethod] public void Label_() => AssertColor(Lang, "start:", "start:", Label);
    [TestMethod] public void Comment_() => AssertColor(Lang, "nop ; comment mov", "; comment mov", Comment);
    [TestMethod] public void RepPrefix() => AssertColor(Lang, "rep movsb", "rep", Prefix);
    [TestMethod] public void Interrupt() => AssertColor(Lang, "int 0x80", "int", Int);
}

[TestClass]
public class HexFileHighlightTests
{
    private static readonly SyntaxHighlightLanguage Lang = new HexFile();
    private const string Colon = "#FFFF00", ByteCount = "#00FF00", Address = "#00FF00", RecordType = "#FF5500", Data = "#00FFFF", Checksum = "#666666";
    // : 10 0100 00 214601360121470136007EFE09D21901 40
    private const string Record = ":10010000214601360121470136007EFE09D2190140";

    [TestMethod] public void StartCode() => AssertColor(Lang, Record, ":", Colon);
    [TestMethod] public void ByteCount_() => AssertColor(Lang, Record, "10", ByteCount);
    [TestMethod] public void Address_() => AssertColor(Lang, Record, "0100", Address);
    [TestMethod] public void RecordType_() => AssertColor(Lang, Record, "00", RecordType, occurrence: 3);
    [TestMethod] public void Data_() => AssertColor(Lang, Record, "214601360121470136007EFE09D21901", Data);
    [TestMethod] public void Checksum_() => AssertColor(Lang, Record, "40", Checksum, occurrence: 0);
}

[TestClass]
public class LatexHighlightTests
{
    private static readonly SyntaxHighlightLanguage Lang = new LaTex();
    private const string Command = "#0088ff", Comment = "#646464", Bracket = "#FFFF00", Brace = "#FF0000", Math = "#00FF00";

    [TestMethod] public void Command_() { AssertColor(Lang, "\\section{Intro}", "\\section", Command); AssertColor(Lang, "\\section{Intro}", "{", Brace); }
    [TestMethod] public void Comment_() => AssertColor(Lang, "text % comment", "% comment", Comment);
    [TestMethod] public void MathAndBrackets() { AssertColor(Lang, "$x^2$", "$", Math); AssertColor(Lang, "\\section[short]{A}", "[", Bracket); }
    [TestMethod] public void EscapedPercent_IsNotAComment() => AssertColor(Lang, "50\\% done", "done", null);
    [TestMethod] public void UppercaseCommand() => AssertColor(Lang, "\\LaTeX and \\Delta", "\\Delta", Command);
}

[TestClass]
public class PhpHighlightTests
{
    private static readonly SyntaxHighlightLanguage Lang = new PHP();
    private const string Tag = "#FF0000", Function = "#aa00FF", Keyword = "#0077FF", Variable = "#FFBBFF", String = "#00FF00", Comment = "#646464";

    [TestMethod] public void Tags() { AssertColor(Lang, "<?php", "<?php", Tag); AssertColor(Lang, "x;\n?>", "?>", Tag); }
    [TestMethod] public void VariableAndString() { const string t = "$name = \"Bob\";"; AssertColor(Lang, t, "$name", Variable); AssertColor(Lang, t, "\"Bob\"", String); }
    [TestMethod] public void FunctionDeclaration() { const string t = "function greet($n) {"; AssertColor(Lang, t, "function", Keyword); AssertColor(Lang, t, "greet", Function); }
    [TestMethod] public void Comment_() => AssertColor(Lang, "$a = 1; // comment", "// comment", Comment);
    [TestMethod] public void Return() => AssertColor(Lang, "return $n;", "return", Keyword);
}

[TestClass]
public class QSharpHighlightTests
{
    private static readonly SyntaxHighlightLanguage Lang = new QSharp();
    private const string Keyword = "#00ffff", Type = "#00ff00", Comment = "#646464";

    [TestMethod] public void Keyword_() => AssertColor(Lang, "namespace Quantum {", "namespace", Keyword);
    [TestMethod] public void Types() { const string t = "operation Foo(q : Qubit) : Result"; AssertColor(Lang, t, "Qubit", Type); AssertColor(Lang, t, "Result", Type); AssertColor(Lang, t, "operation", Keyword); }
    [TestMethod] public void Comment_() => AssertColor(Lang, "// just text", "// just text", Comment);
    [TestMethod] public void KeywordInsideComment_IsComment() => AssertColor(Lang, "// let it be", "let", Comment);
}

[TestClass]
public class GitIgnoreHighlightTests
{
    private static readonly SyntaxHighlightLanguage Lang = new GitIgnore();
    private const string Negation = "#EF5350", Comment = "#BDBDBD", Directory = "#64B5F6", Slash = "#9FA8DA", Wildcard = "#BA68C8", CharSet = "#81C784";

    [TestMethod] public void Comment_() => AssertColor(Lang, "# comment", "# comment", Comment);
    [TestMethod] public void Negation_() => AssertColor(Lang, "!important.log", "!important.log", Negation);
    [TestMethod] public void Directory_() { AssertColor(Lang, "build/", "build", Directory); AssertColor(Lang, "build/", "/", Slash); }
    [TestMethod] public void Wildcard_() => AssertColor(Lang, "*.log", "*", Wildcard);
    [TestMethod] public void CharSet_() => AssertColor(Lang, "file[0-9].txt", "[0-9]", CharSet);
    [TestMethod] public void EscapedHash_IsNotAComment() => AssertNotColor(Lang, "\\#notacomment", "notacomment", Comment);
    [TestMethod] public void PathInsideComment_IsComment() => AssertColor(Lang, "# ignore build/ folder", "build", Comment);
}

[TestClass]
public class LogHighlightTests
{
    private static readonly SyntaxHighlightLanguage Lang = new LogHighlighter();
    private const string BracketContent = "#B0BEC5", Path = "#7986CB", Ip = "#80CBC4", Uuid = "#B39DDB", Unit = "#FFB74D", Timestamp = "#4DD0E1",
        Verb = "#4FC3F7", ClientError = "#FFB74D", Info = "#42A5F5", Warn = "#FFB74D", Error = "#EF5350", Fatal = "#FF5252", StackTrace = "#E57373", String = "#AED581";

    [TestMethod]
    public void TypicalLine()
    {
        const string t = "2024-01-15 10:23:45.123 [main] INFO Started";
        AssertColor(Lang, t, "2024-01-15 10:23:45.123", Timestamp);
        AssertColor(Lang, t, "main", BracketContent);
        AssertColor(Lang, t, "INFO", Info);
    }
    [TestMethod] public void ErrorWithIp() { const string t = "ERROR Connection to 192.168.1.10:8080 failed"; AssertColor(Lang, t, "ERROR", Error); AssertColor(Lang, t, "192.168.1.10:8080", Ip); }
    [TestMethod] public void WarnWithDuration() { const string t = "WARN slow request 1500ms"; AssertColor(Lang, t, "WARN", Warn); AssertColor(Lang, t, "1500ms", Unit); }
    [TestMethod] public void HttpRequest() { const string t = "\"GET /api/users HTTP/1.1\" 404 512"; AssertColor(Lang, t, "GET", Verb); AssertColor(Lang, t, "404", ClientError); }
    [TestMethod] public void Uuid_() => AssertColor(Lang, "id=550e8400-e29b-41d4-a716-446655440000", "550e8400-e29b-41d4-a716-446655440000", Uuid);
    [TestMethod] public void UnixPath() => AssertColor(Lang, "open /var/log/app.log", "/var/log/app.log", Path);
    [TestMethod] public void SyslogTimestamp() => AssertColor(Lang, "Sep 25 14:02:11 host sshd[1]: ok", "Sep 25 14:02:11", Timestamp);
    [TestMethod] public void Fatal_() => AssertColor(Lang, "FATAL out of memory", "FATAL", Fatal);
    [TestMethod] public void DotNetStackTrace() => AssertColor(Lang, "x\n   at Foo.Bar() in C:\\src\\x.cs:line 42", "at Foo.Bar()", StackTrace);
    [TestMethod] public void JavaCausedBy() => AssertColor(Lang, "x\nCaused by: java.io.IOException: boom", "Caused by:", StackTrace);
    [TestMethod] public void QuotedValue() => AssertColor(Lang, "loading 'config.json'", "'config.json'", String);
    [TestMethod] public void ApostropheInText_IsNotAString() => AssertColor(Lang, "Can't open file 'config.json'", "t open file", null);
}

[TestClass]
public class GenericHighlightTests
{
    private static readonly SyntaxHighlightLanguage Lang = new GenericHighlighter();
    private const string Operator = "#C586C0", Bracket = "#FFD700", Number = "#B5CEA8", Key = "#9CDCFE", Constant = "#569CD6",
        Value = "#4EC9B0", Link = "#7986CB", Comment = "#6A9955", String = "#CE9178", Marker = "#F44747";

    [TestMethod] public void Integer() => AssertColor(Lang, "count 42 items", "42", Number);
    [TestMethod] public void Decimal_() => AssertColor(Lang, "pi is 3.14", "3.14", Number);
    [TestMethod] public void Hex() => AssertColor(Lang, "mask 0x1F", "0x1F", Number);
    [TestMethod] public void Exponent() => AssertColor(Lang, "value 1.5e-3", "1.5e-3", Number);
    [TestMethod] public void NumberWithUnit() { AssertColor(Lang, "width 10px", "10px", Number); AssertColor(Lang, "load 50%", "50%", Number); }
    [TestMethod] public void DigitsInsideAWord_AreNotANumber() => AssertColor(Lang, "open file2 now", "file2", null);
    [TestMethod] public void Operators() { AssertColor(Lang, "a <= b", "<=", Operator); AssertColor(Lang, "x != y", "!=", Operator); }
    [TestMethod] public void Brackets() { AssertColor(Lang, "f(x)", "(", Bracket); AssertColor(Lang, "a[0]", "]", Bracket); }
    [TestMethod] public void DoubleQuotedString() => AssertColor(Lang, "say \"hello world\" now", "\"hello world\"", String);
    [TestMethod] public void SingleQuotedString() => AssertColor(Lang, "say 'hi' now", "'hi'", String);
    [TestMethod] public void ApostropheInText_IsNotAString() => AssertColor(Lang, "can't stop won't stop", "t stop won", null);
    [TestMethod] public void NumberInsideString_IsString() => AssertColor(Lang, "\"port 80\"", "80", String);
    [TestMethod] public void LineComments() { AssertColor(Lang, "# comment 42", "comment 42", Comment); AssertColor(Lang, "  // comment", "comment", Comment); AssertColor(Lang, "; comment", "comment", Comment); }
    [TestMethod] public void HashInsideLine_IsNotAComment() => AssertColor(Lang, "issue #12 open", "open", null);
    [TestMethod] public void BlockComment() => AssertColor(Lang, "a /* note 1 */ b", "note 1", Comment);
    [TestMethod] public void MarkerInsideComment() => AssertColor(Lang, "// TODO fix", "TODO", Marker);
    [TestMethod] public void LowercaseMarkerWord_IsNotHighlighted() => AssertColor(Lang, "a todo list", "todo", null);
    [TestMethod] public void Constants() { AssertColor(Lang, "x = true", "true", Constant); AssertColor(Lang, "y = None", "None", Constant); }
    [TestMethod] public void ProseNone_IsNotAConstant() => AssertColor(Lang, "there is none left", "none", null);
    [TestMethod] public void KeyWithEquals() => AssertColor(Lang, "max_size = 10", "max_size", Key);
    [TestMethod] public void KeyWithColon() => AssertColor(Lang, "  name: test", "name", Key);
    [TestMethod] public void UrlAtLineStart_IsNotAKey() { AssertColor(Lang, "https://example.com/a", "https", Link); }
    [TestMethod] public void Url() => AssertColor(Lang, "see https://example.com/a?b=1 now", "https://example.com/a?b=1", Link);
    [TestMethod] public void Email() => AssertColor(Lang, "mail me@example.com now", "me@example.com", Link);
    [TestMethod] public void DateTime() => AssertColor(Lang, "at 2024-01-15 10:23:45 ok", "2024-01-15 10:23:45", Value);
    [TestMethod] public void IsoDateTime() => AssertColor(Lang, "at 2024-01-15T10:23:45Z ok", "2024-01-15T10:23:45Z", Value);
    [TestMethod] public void Ip() => AssertColor(Lang, "host 192.168.1.10:8080 up", "192.168.1.10:8080", Value);
    [TestMethod] public void Guid_() => AssertColor(Lang, "id 550e8400-e29b-41d4-a716-446655440000", "550e8400-e29b-41d4-a716-446655440000", Value);
    [TestMethod] public void HexColor() => AssertColor(Lang, "color = #ff8800", "#ff8800", Value);
    [TestMethod] public void PlainProse_IsMostlyUncolored() => AssertColor(Lang, "The quick brown fox jumps over the lazy dog", "quick brown fox jumps over the lazy dog", null);
}

[TestClass]
public class CsvHighlightTests
{
    [TestMethod]
    public void Delimiters() => AssertColor(new CSV(), "a,b;c|d", ",", "#1b9902");

    [TestMethod]
    public void Enhanced_ColumnsHaveDifferentColors()
    {
        var lang = new CSVEnhanced();
        Assert.AreNotEqual(ColorAt(lang, "a,b,c", "a"), ColorAt(lang, "a,b,c", "b"));
        AssertColor(lang, "a,b,c", ",", "#f14260");
    }

    [TestMethod]
    public void Enhanced_SameColumnHasTheSameColorInEveryRow()
    {
        var lang = new CSVEnhanced();
        Assert.AreEqual(ColorAt(lang, "a,b\nc,d", "b"), ColorAt(lang, "a,b\nc,d", "d"));
        Assert.AreEqual(ColorAt(lang, "a,b\r\nc,d", "b", newLine: "\r\n"), ColorAt(lang, "a,b\r\nc,d", "d", newLine: "\r\n"));
    }

    [TestMethod]
    public void Enhanced_CommaInsideQuotes_DoesNotStartANewColumn()
    {
        var lang = new CSVEnhanced();
        const string t = "a,b\n\"x,y\",z";
        Assert.AreEqual(ColorAt(lang, t, "b"), ColorAt(lang, t, "z"));
    }
}
