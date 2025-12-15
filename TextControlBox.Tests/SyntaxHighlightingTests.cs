using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using TextControlBox.Tests;

namespace TextControlBoxNS.Tests;

[TestClass]
public class SyntaxHighlightingTests
{
    #region Helper Methods

    private void AssertHighlight(SyntaxHighlightID language, string text, string expectedColor)
    {
        var coreTextbox = TestHelper.MakeCoreTextbox();
        coreTextbox.SelectSyntaxHighlightingById(language);
        coreTextbox.LoadText(text);
        TestHelper.AssertHighlightExists(coreTextbox, expectedColor);
    }

    #endregion

    #region Batch Language Tests

    [UITestMethod]
    public void Batch_AllPatterns_ShouldHighlight()
    {
        // Numeric literals
        AssertHighlight(SyntaxHighlightID.Batch, "123 456.789 -42 +3.14 1.5e10 2E-5", "#dd00dd");

        // Keywords
        AssertHighlight(SyntaxHighlightID.Batch, "set VAR=value", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Batch, "echo Hello World", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Batch, "for %%i in (*.txt) do", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Batch, "pause", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Batch, "exit /b 0", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Batch, "cd C:\\Windows", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Batch, "if exist file.txt", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Batch, "else", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Batch, "goto label", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Batch, "del file.txt", "#dd00dd");

        // Labels
        AssertHighlight(SyntaxHighlightID.Batch, ":start", "#00C000");
        AssertHighlight(SyntaxHighlightID.Batch, ":loop_section", "#00C000");

        // Strings
        AssertHighlight(SyntaxHighlightID.Batch, "\"quoted string\"", "#00C000");
        AssertHighlight(SyntaxHighlightID.Batch, "'single quoted'", "#00C000");

        // Special characters @ and %
        AssertHighlight(SyntaxHighlightID.Batch, "@echo off", "#dd0077");
        AssertHighlight(SyntaxHighlightID.Batch, "%PATH%", "#dd0077");
        AssertHighlight(SyntaxHighlightID.Batch, "%1 %2", "#dd0077");

        // Wildcards
        AssertHighlight(SyntaxHighlightID.Batch, "*.*", "#dd0077");

        // Comments
        AssertHighlight(SyntaxHighlightID.Batch, "rem This is a comment", "#888888");
        AssertHighlight(SyntaxHighlightID.Batch, "REM Another comment", "#888888");
    }

    #endregion

    #region INI Language Tests

    [UITestMethod]
    public void INI_AllPatterns_ShouldHighlight()
    {
        // Section names
        AssertHighlight(SyntaxHighlightID.Inifile, "[General]", "#7C4DFF");
        AssertHighlight(SyntaxHighlightID.Inifile, "[Database_Settings]", "#7C4DFF");

        // Brackets
        AssertHighlight(SyntaxHighlightID.Inifile, "[Section]", "#5C6BC0");

        // Keys
        AssertHighlight(SyntaxHighlightID.Inifile, "key = value", "#E65100");
        AssertHighlight(SyntaxHighlightID.Inifile, "long key name = another", "#E65100");

        // String values
        AssertHighlight(SyntaxHighlightID.Inifile, "name = \"John Doe\"", "#00796B");
        AssertHighlight(SyntaxHighlightID.Inifile, "path = 'C:\\Program Files'", "#00796B");

        // Numeric values
        AssertHighlight(SyntaxHighlightID.Inifile, "port = 8080", "#1565C0");
        AssertHighlight(SyntaxHighlightID.Inifile, "version = 1.5", "#1565C0");
        AssertHighlight(SyntaxHighlightID.Inifile, "hex = 0xFF", "#1565C0");
        AssertHighlight(SyntaxHighlightID.Inifile, "binary = 0b1010", "#1565C0");
        AssertHighlight(SyntaxHighlightID.Inifile, "scientific = 1.5e10", "#1565C0");

        // Boolean values
        AssertHighlight(SyntaxHighlightID.Inifile, "enabled = yes", "#FF5722");
        AssertHighlight(SyntaxHighlightID.Inifile, "debug = true", "#FF5722");
        AssertHighlight(SyntaxHighlightID.Inifile, "active = on", "#FF5722");
        AssertHighlight(SyntaxHighlightID.Inifile, "status = false", "#FF5722");

        // Comments
        AssertHighlight(SyntaxHighlightID.Inifile, "; This is a comment", "#9E9E9E");
        AssertHighlight(SyntaxHighlightID.Inifile, "# Another comment", "#9E9E9E");
    }

    #endregion

    #region TOML Language Tests

    [UITestMethod]
    public void TOML_AllPatterns_ShouldHighlight()
    {
        // Array table headers
        AssertHighlight(SyntaxHighlightID.TOML, "[[array.of.tables]]", "#9C27B0");
        AssertHighlight(SyntaxHighlightID.TOML, "[[products]]", "#9C27B0");

        // Section headers
        AssertHighlight(SyntaxHighlightID.TOML, "[owner]", "#7C4DFF");
        AssertHighlight(SyntaxHighlightID.TOML, "[database.connection]", "#7C4DFF");

        // Brackets and braces
        AssertHighlight(SyntaxHighlightID.TOML, "[[table]]", "#5C6BC0");
        AssertHighlight(SyntaxHighlightID.TOML, "[simple]", "#5C6BC0");
        AssertHighlight(SyntaxHighlightID.TOML, "{ inline = true }", "#5C6BC0");

        // Keys
        AssertHighlight(SyntaxHighlightID.TOML, "name = \"value\"", "#E65100");
        AssertHighlight(SyntaxHighlightID.TOML, "dotted.key.name = 123", "#E65100");
        AssertHighlight(SyntaxHighlightID.TOML, "{ key = \"val\" }", "#E65100");

        // Multiline strings
        AssertHighlight(SyntaxHighlightID.TOML, "'''multi\nline'''", "#00796B");
        AssertHighlight(SyntaxHighlightID.TOML, "\"\"\"another\nmulti\"\"\"", "#00796B");

        // Basic strings
        AssertHighlight(SyntaxHighlightID.TOML, "name = \"value\"", "#00796B");
        AssertHighlight(SyntaxHighlightID.TOML, "path = 'C:\\path'", "#00796B");

        // Dates and times
        AssertHighlight(SyntaxHighlightID.TOML, "date = 2024-01-15", "#1565C0");
        AssertHighlight(SyntaxHighlightID.TOML, "datetime = 2024-01-15T10:30:00Z", "#1565C0");

        // Floats
        AssertHighlight(SyntaxHighlightID.TOML, "pi = 3.14159", "#1565C0");
        AssertHighlight(SyntaxHighlightID.TOML, "scientific = 1.5e10", "#1565C0");
        AssertHighlight(SyntaxHighlightID.TOML, "special = +inf", "#1565C0");
        AssertHighlight(SyntaxHighlightID.TOML, "nan_val = -nan", "#1565C0");

        // Integers
        AssertHighlight(SyntaxHighlightID.TOML, "dec = 42", "#1565C0");
        AssertHighlight(SyntaxHighlightID.TOML, "hex = 0xDEADBEEF", "#1565C0");
        AssertHighlight(SyntaxHighlightID.TOML, "oct = 0o755", "#1565C0");
        AssertHighlight(SyntaxHighlightID.TOML, "bin = 0b1010", "#1565C0");

        // Booleans
        AssertHighlight(SyntaxHighlightID.TOML, "enabled = true", "#FF5722");
        AssertHighlight(SyntaxHighlightID.TOML, "disabled = false", "#FF5722");

        // Commas
        AssertHighlight(SyntaxHighlightID.TOML, "array = [1, 2, 3]", "#5C6BC0");

        // Comments
        AssertHighlight(SyntaxHighlightID.TOML, "# This is a comment", "#9E9E9E");
    }

    #endregion

    #region Klipper Language Tests

    [UITestMethod]
    public void Klipper_AllPatterns_ShouldHighlight()
    {
        // Section names
        AssertHighlight(SyntaxHighlightID.Klipper, "[stepper_x]", "#7C4DFF");
        AssertHighlight(SyntaxHighlightID.Klipper, "[gcode_macro START]", "#7C4DFF");

        // Brackets
        AssertHighlight(SyntaxHighlightID.Klipper, "[section]", "#5C6BC0");

        // Keys
        AssertHighlight(SyntaxHighlightID.Klipper, "step_pin: PA1", "#E65100");
        AssertHighlight(SyntaxHighlightID.Klipper, "rotation_distance: 40", "#E65100");

        // Pin names
        AssertHighlight(SyntaxHighlightID.Klipper, "pin: PA1", "#00897B");
        AssertHighlight(SyntaxHighlightID.Klipper, "pin: ^PA2", "#00897B");
        AssertHighlight(SyntaxHighlightID.Klipper, "pin: ~!PB3", "#00897B");
        AssertHighlight(SyntaxHighlightID.Klipper, "pin: ar54", "#00897B");

        // Gcode commands
        AssertHighlight(SyntaxHighlightID.Klipper, "G0 X10 Y20", "#AD1457");
        AssertHighlight(SyntaxHighlightID.Klipper, "G1 F3000", "#AD1457");
        AssertHighlight(SyntaxHighlightID.Klipper, "M104 S200", "#AD1457");

        // String values
        AssertHighlight(SyntaxHighlightID.Klipper, "description: \"Home XY\"", "#00796B");
        AssertHighlight(SyntaxHighlightID.Klipper, "name: 'Printer'", "#00796B");

        // Numeric values with units
        AssertHighlight(SyntaxHighlightID.Klipper, "distance: 100mm", "#1565C0");
        AssertHighlight(SyntaxHighlightID.Klipper, "time: 0.2s", "#1565C0");
        AssertHighlight(SyntaxHighlightID.Klipper, "angle: 45deg", "#1565C0");
        AssertHighlight(SyntaxHighlightID.Klipper, "percent: 50%", "#1565C0");

        // Boolean values
        AssertHighlight(SyntaxHighlightID.Klipper, "enable: true", "#FF5722");
        AssertHighlight(SyntaxHighlightID.Klipper, "active: false", "#FF5722");

        // Template expressions
        AssertHighlight(SyntaxHighlightID.Klipper, "{variable_name}", "#6A1B9A");
        AssertHighlight(SyntaxHighlightID.Klipper, "{printer.toolhead.position}", "#6A1B9A");

        // Commas
        AssertHighlight(SyntaxHighlightID.Klipper, "points: 50,50 150,50", "#5C6BC0");

        // Comments
        AssertHighlight(SyntaxHighlightID.Klipper, "# Configuration comment", "#9E9E9E");
    }

    #endregion

    #region C++ Language Tests

    [UITestMethod]
    public void Cpp_AllPatterns_ShouldHighlight()
    {
        // Numeric literals
        AssertHighlight(SyntaxHighlightID.Cpp, "42", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Cpp, "3.14159", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Cpp, "0xFF", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Cpp, "0b1010", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Cpp, "1.5e10", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Cpp, "42L", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Cpp, "3.14f", "#dd00dd");

        // Function calls
        AssertHighlight(SyntaxHighlightID.Cpp, "printf(\"Hello\")", "#4455ff");
        AssertHighlight(SyntaxHighlightID.Cpp, "std::cout", "#4455ff");

        // Keywords
        AssertHighlight(SyntaxHighlightID.Cpp, "int main()", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Cpp, "class MyClass", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Cpp, "if (condition)", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Cpp, "for (int i = 0)", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Cpp, "return 0", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Cpp, "const char*", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Cpp, "namespace std", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Cpp, "template<typename T>", "#dd00dd");

        // Preprocessor directives
        AssertHighlight(SyntaxHighlightID.Cpp, "#include <iostream>", "#5F5E5E");
        AssertHighlight(SyntaxHighlightID.Cpp, "#define MAX 100", "#5F5E5E");
        AssertHighlight(SyntaxHighlightID.Cpp, "#ifndef HEADER_H", "#5F5E5E");
        AssertHighlight(SyntaxHighlightID.Cpp, "#pragma once", "#5F5E5E");

        // Strings and chars
        AssertHighlight(SyntaxHighlightID.Cpp, "\"Hello World\"", "#D98300");
        AssertHighlight(SyntaxHighlightID.Cpp, "'c'", "#D98300");
        AssertHighlight(SyntaxHighlightID.Cpp, "\"escaped \\\" quote\"", "#D98300");

        // Block comments
        AssertHighlight(SyntaxHighlightID.Cpp, "/* block comment */", "#6B6A6A");
        AssertHighlight(SyntaxHighlightID.Cpp, "/* multi\nline\ncomment */", "#6B6A6A");

        // Line comments
        AssertHighlight(SyntaxHighlightID.Cpp, "// line comment", "#6B6A6A");
    }

    #endregion

    #region C# Language Tests

    [UITestMethod]
    public void CSharp_AllPatterns_ShouldHighlight()
    {
        // Numbers
        AssertHighlight(SyntaxHighlightID.CSharp, "42", "#ff00ff");
        AssertHighlight(SyntaxHighlightID.CSharp, "3.14", "#ff00ff");
        AssertHighlight(SyntaxHighlightID.CSharp, "0xFF", "#ff00ff");
        AssertHighlight(SyntaxHighlightID.CSharp, "0b1010", "#ff00ff");
        AssertHighlight(SyntaxHighlightID.CSharp, "1.5e10f", "#ff00ff");

        // Function names
        AssertHighlight(SyntaxHighlightID.CSharp, "Console.WriteLine()", "#880088");
        AssertHighlight(SyntaxHighlightID.CSharp, "MyMethod()", "#880088");

        // Keywords
        AssertHighlight(SyntaxHighlightID.CSharp, "public class Program", "#0066bb");
        AssertHighlight(SyntaxHighlightID.CSharp, "private void Method()", "#0066bb");
        AssertHighlight(SyntaxHighlightID.CSharp, "if (true) return", "#0066bb");
        AssertHighlight(SyntaxHighlightID.CSharp, "foreach (var item in list)", "#0066bb");
        AssertHighlight(SyntaxHighlightID.CSharp, "async await Task", "#0066bb");
        AssertHighlight(SyntaxHighlightID.CSharp, "null", "#0066bb");

        // Common types
        AssertHighlight(SyntaxHighlightID.CSharp, "List<int>", "#008000");
        AssertHighlight(SyntaxHighlightID.CSharp, "Dictionary<string, int>", "#008000");
        AssertHighlight(SyntaxHighlightID.CSharp, "Console.WriteLine", "#008000");
        AssertHighlight(SyntaxHighlightID.CSharp, "Color.Red", "#008000");

        // Try/Catch/Finally
        AssertHighlight(SyntaxHighlightID.CSharp, "try { }", "#9922ff");
        AssertHighlight(SyntaxHighlightID.CSharp, "catch (Exception ex)", "#9922ff");
        AssertHighlight(SyntaxHighlightID.CSharp, "finally { }", "#9922ff");

        // Preprocessor directives
        AssertHighlight(SyntaxHighlightID.CSharp, "#region MyRegion", "#ff0000");
        AssertHighlight(SyntaxHighlightID.CSharp, "#endregion", "#ff0000");

        // String literals
        AssertHighlight(SyntaxHighlightID.CSharp, "\"normal string\"", "#ff5500");
        AssertHighlight(SyntaxHighlightID.CSharp, "@\"verbatim string\"", "#ff5500");

        // Character literals
        AssertHighlight(SyntaxHighlightID.CSharp, "'a'", "#ff5500");

        // Single-line comments
        AssertHighlight(SyntaxHighlightID.CSharp, "// comment", "#888888");

        // Multi-line comments
        AssertHighlight(SyntaxHighlightID.CSharp, "/* comment */", "#888888");
    }

    #endregion

    #region GCode Language Tests

    [UITestMethod]
    public void GCode_AllPatterns_ShouldHighlight()
    {
        // Y axis
        AssertHighlight(SyntaxHighlightID.GCode, "Y10.5", "#00ff00");
        AssertHighlight(SyntaxHighlightID.GCode, "Y-20", "#00ff00");

        // X axis
        AssertHighlight(SyntaxHighlightID.GCode, "X10.5", "#ff0000");
        AssertHighlight(SyntaxHighlightID.GCode, "X+30", "#ff0000");

        // Z axis
        AssertHighlight(SyntaxHighlightID.GCode, "Z5.0", "#0077ff");
        AssertHighlight(SyntaxHighlightID.GCode, "Z-10", "#0077ff");

        // A axis
        AssertHighlight(SyntaxHighlightID.GCode, "A45", "#ff00ff");
        AssertHighlight(SyntaxHighlightID.GCode, "A-90", "#ff00ff");

        // E and F parameters
        AssertHighlight(SyntaxHighlightID.GCode, "E0.5", "#ffAA00");
        AssertHighlight(SyntaxHighlightID.GCode, "F3000", "#ffAA00");

        // S and T parameters
        AssertHighlight(SyntaxHighlightID.GCode, "S1000", "#ffff00");
        AssertHighlight(SyntaxHighlightID.GCode, "T0", "#ffff00");

        // Numeric values
        AssertHighlight(SyntaxHighlightID.GCode, "10.5", "#ff00ff");
        AssertHighlight(SyntaxHighlightID.GCode, "-20", "#ff00ff");
        AssertHighlight(SyntaxHighlightID.GCode, "1.5e2", "#ff00ff");

        // G and M commands
        AssertHighlight(SyntaxHighlightID.GCode, "G0 X10 Y20", "#00aaaa");
        AssertHighlight(SyntaxHighlightID.GCode, "G1 F3000", "#00aaaa");
        AssertHighlight(SyntaxHighlightID.GCode, "M104 S200", "#00aaaa");
        AssertHighlight(SyntaxHighlightID.GCode, "M109 S220", "#00aaaa");

        // Comments
        AssertHighlight(SyntaxHighlightID.GCode, "; comment", "#888888");
        AssertHighlight(SyntaxHighlightID.GCode, "// comment", "#888888");
        AssertHighlight(SyntaxHighlightID.GCode, "rem comment", "#888888");
    }

    #endregion

    #region x86Assembly Language Tests

    [UITestMethod]
    public void x86Assembly_AllPatterns_ShouldHighlight()
    {
        // Prefixes
        AssertHighlight(SyntaxHighlightID.x86Assembly, "rep movsb", "#5e00c7");
        AssertHighlight(SyntaxHighlightID.x86Assembly, "repe cmpsb", "#5e00c7");
        AssertHighlight(SyntaxHighlightID.x86Assembly, "repz scasb", "#5e00c7");

        // Instructions
        AssertHighlight(SyntaxHighlightID.x86Assembly, "mov ax, bx", "#8400ff");
        AssertHighlight(SyntaxHighlightID.x86Assembly, "add eax, 10", "#8400ff");
        AssertHighlight(SyntaxHighlightID.x86Assembly, "jmp label", "#8400ff");
        AssertHighlight(SyntaxHighlightID.x86Assembly, "call function", "#8400ff");
        AssertHighlight(SyntaxHighlightID.x86Assembly, "ret", "#8400ff");
        AssertHighlight(SyntaxHighlightID.x86Assembly, "push ebp", "#8400ff");
        AssertHighlight(SyntaxHighlightID.x86Assembly, "pop ebp", "#8400ff");

        // Registers
        AssertHighlight(SyntaxHighlightID.x86Assembly, "mov eax, ebx", "#c4aa00");
        AssertHighlight(SyntaxHighlightID.x86Assembly, "add rax, rcx", "#c4aa00");
        AssertHighlight(SyntaxHighlightID.x86Assembly, "mov ax, bx", "#c4aa00");
        AssertHighlight(SyntaxHighlightID.x86Assembly, "mov al, bl", "#c4aa00");
        AssertHighlight(SyntaxHighlightID.x86Assembly, "[ebp+8]", "#c4aa00");

        // Values
        AssertHighlight(SyntaxHighlightID.x86Assembly, "mov eax, 42", "#558900");
        AssertHighlight(SyntaxHighlightID.x86Assembly, "add ebx, 0xFF", "#558900");
        AssertHighlight(SyntaxHighlightID.x86Assembly, "mov cl, 0b1010", "#558900");

        // Int instruction
        AssertHighlight(SyntaxHighlightID.x86Assembly, "int 0x80", "#005eb0");
        AssertHighlight(SyntaxHighlightID.x86Assembly, "int 21h", "#005eb0");

        // Brackets and operators
        AssertHighlight(SyntaxHighlightID.x86Assembly, "[ebp+8]", "#0075c3");
        AssertHighlight(SyntaxHighlightID.x86Assembly, "[eax*4]", "#0075c3");

        // Labels
        AssertHighlight(SyntaxHighlightID.x86Assembly, "start:", "#007a41");
        AssertHighlight(SyntaxHighlightID.x86Assembly, "loop_begin:", "#007a41");

        // Comments
        AssertHighlight(SyntaxHighlightID.x86Assembly, "; comment", "#028100");
    }

    #endregion

    #region HexFile Language Tests

    [UITestMethod]
    public void HexFile_AllPatterns_ShouldHighlight()
    {
        // Colon
        AssertHighlight(SyntaxHighlightID.HexFile, ":10", "#FFFF00");

        // Byte count
        AssertHighlight(SyntaxHighlightID.HexFile, ":10", "#00FF00");

        // Address
        AssertHighlight(SyntaxHighlightID.HexFile, ":100000", "#00FF00");

        // Record type
        AssertHighlight(SyntaxHighlightID.HexFile, ":10000000", "#FF5500");

        // Data bytes
        AssertHighlight(SyntaxHighlightID.HexFile, ":10000000AABBCCDD", "#00FFFF");

        // Checksum
        AssertHighlight(SyntaxHighlightID.HexFile, ":10000000AABBCCDDEE", "#666666");

        // Comments
        AssertHighlight(SyntaxHighlightID.HexFile, "// comment", "#666666");

        // Invalid characters
        AssertHighlight(SyntaxHighlightID.HexFile, "G", "#FF0000");
    }

    #endregion

    #region HTML Language Tests

    [UITestMethod]
    public void Html_AllPatterns_ShouldHighlight()
    {
        // Numeric values
        AssertHighlight(SyntaxHighlightID.Html, "42", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Html, "3.14", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Html, "1.5e10", "#dd00dd");

        // Attributes
        AssertHighlight(SyntaxHighlightID.Html, "class=\"test\"", "#00CA00");
        AssertHighlight(SyntaxHighlightID.Html, "id=\"main\"", "#00CA00");

        // Opening tags
        AssertHighlight(SyntaxHighlightID.Html, "<div>", "#969696");
        AssertHighlight(SyntaxHighlightID.Html, "<span class=\"test\">", "#969696");

        // Closing tags
        AssertHighlight(SyntaxHighlightID.Html, "</div>", "#969696");
        AssertHighlight(SyntaxHighlightID.Html, "</span>", "#969696");

        // Self-closing tags
        AssertHighlight(SyntaxHighlightID.Html, "<br/>", "#969696");
        AssertHighlight(SyntaxHighlightID.Html, "<img src=\"pic.jpg\"/>", "#969696");

        // Double-quoted values
        AssertHighlight(SyntaxHighlightID.Html, "class=\"container\"", "#00CA00");

        // Single-quoted values
        AssertHighlight(SyntaxHighlightID.Html, "class='container'", "#00CA00");

        // CSS units
        AssertHighlight(SyntaxHighlightID.Html, "100px", "#ff00ff");
        AssertHighlight(SyntaxHighlightID.Html, "50%", "#ff00ff");
        AssertHighlight(SyntaxHighlightID.Html, "2rem", "#ff00ff");
        AssertHighlight(SyntaxHighlightID.Html, "90deg", "#ff00ff");

        // Comments
        AssertHighlight(SyntaxHighlightID.Html, "<!-- comment -->", "#888888");
    }

    #endregion
    #region Java Language Tests

    [UITestMethod]
    public void Java_AllPatterns_ShouldHighlight()
    {
        // Numbers
        AssertHighlight(SyntaxHighlightID.Java, "42", "#ff00ff");
        AssertHighlight(SyntaxHighlightID.Java, "3.14", "#ff00ff");
        AssertHighlight(SyntaxHighlightID.Java, "1.5e10", "#ff00ff");

        // Function names
        AssertHighlight(SyntaxHighlightID.Java, "println()", "#880088");
        AssertHighlight(SyntaxHighlightID.Java, "myMethod()", "#880088");

        // Common types
        AssertHighlight(SyntaxHighlightID.Java, "System.out.println", "#008000");
        AssertHighlight(SyntaxHighlightID.Java, "Math.PI", "#008000");

        // Keywords
        AssertHighlight(SyntaxHighlightID.Java, "public class Main", "#0066bb");
        AssertHighlight(SyntaxHighlightID.Java, "private void method()", "#0066bb");
        AssertHighlight(SyntaxHighlightID.Java, "if (condition) return", "#0066bb");
        AssertHighlight(SyntaxHighlightID.Java, "for (int i = 0)", "#0066bb");
        AssertHighlight(SyntaxHighlightID.Java, "while (true)", "#0066bb");
        AssertHighlight(SyntaxHighlightID.Java, "null", "#0066bb");
        AssertHighlight(SyntaxHighlightID.Java, "true", "#0066bb");
        AssertHighlight(SyntaxHighlightID.Java, "false", "#0066bb");

        // Strings
        AssertHighlight(SyntaxHighlightID.Java, "\"Hello World\"", "#ff5500");

        // Characters
        AssertHighlight(SyntaxHighlightID.Java, "'a'", "#00CA00");

        // Block comments
        AssertHighlight(SyntaxHighlightID.Java, "/* comment */", "#888888");

        // Line comments
        AssertHighlight(SyntaxHighlightID.Java, "// comment", "#888888");
    }

    #endregion

    #region JavaScript Language Tests

    [UITestMethod]
    public void Javascript_AllPatterns_ShouldHighlight()
    {
        // Special characters
        AssertHighlight(SyntaxHighlightID.Javascript, "!", "#990033");
        AssertHighlight(SyntaxHighlightID.Javascript, "?", "#990033");

        // Operators
        AssertHighlight(SyntaxHighlightID.Javascript, "+ - * / %", "#77FF77");
        AssertHighlight(SyntaxHighlightID.Javascript, "= == ===", "#77FF77");
        AssertHighlight(SyntaxHighlightID.Javascript, "&& ||", "#77FF77");

        // Numbers
        AssertHighlight(SyntaxHighlightID.Javascript, "42", "#ff00ff");
        AssertHighlight(SyntaxHighlightID.Javascript, "3.14", "#ff00ff");
        AssertHighlight(SyntaxHighlightID.Javascript, "1.5e10", "#ff00ff");

        // Function calls
        AssertHighlight(SyntaxHighlightID.Javascript, "console.log()", "#880088");
        AssertHighlight(SyntaxHighlightID.Javascript, "myFunction()", "#880088");

        // Keywords
        AssertHighlight(SyntaxHighlightID.Javascript, "function test()", "#0066bb");
        AssertHighlight(SyntaxHighlightID.Javascript, "const x = 10", "#0066bb");
        AssertHighlight(SyntaxHighlightID.Javascript, "let y = 20", "#0066bb");
        AssertHighlight(SyntaxHighlightID.Javascript, "if (condition)", "#0066bb");
        AssertHighlight(SyntaxHighlightID.Javascript, "return value", "#0066bb");
        AssertHighlight(SyntaxHighlightID.Javascript, "class MyClass", "#0066bb");
        AssertHighlight(SyntaxHighlightID.Javascript, "await promise", "#0066bb");

        // DOM objects
        AssertHighlight(SyntaxHighlightID.Javascript, "document.getElementById", "#008000");
        AssertHighlight(SyntaxHighlightID.Javascript, "window.location", "#008000");

        // Try/Catch
        AssertHighlight(SyntaxHighlightID.Javascript, "try { }", "#9922ff");
        AssertHighlight(SyntaxHighlightID.Javascript, "catch (e)", "#9922ff");
        AssertHighlight(SyntaxHighlightID.Javascript, "finally { }", "#9922ff");

        // Regex
        AssertHighlight(SyntaxHighlightID.Javascript, "/pattern/i", "#FFFF00");

        // Strings
        AssertHighlight(SyntaxHighlightID.Javascript, "\"string\"", "#ff5500");
        AssertHighlight(SyntaxHighlightID.Javascript, "'string'", "#ff5500");

        // Block comments
        AssertHighlight(SyntaxHighlightID.Javascript, "/* comment */", "#888888");

        // Line comments
        AssertHighlight(SyntaxHighlightID.Javascript, "// comment", "#888888");
    }

    #endregion

    #region JSON Language Tests

    [UITestMethod]
    public void Json_AllPatterns_ShouldHighlight()
    {
        // Numbers
        AssertHighlight(SyntaxHighlightID.Json, "42", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Json, "3.14", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Json, "1.5e10", "#dd00dd");

        // Keywords
        AssertHighlight(SyntaxHighlightID.Json, "null", "#00AADD");
        AssertHighlight(SyntaxHighlightID.Json, "true", "#00AADD");
        AssertHighlight(SyntaxHighlightID.Json, "false", "#00AADD");

        // Structural characters
        AssertHighlight(SyntaxHighlightID.Json, "{}", "#969696");
        AssertHighlight(SyntaxHighlightID.Json, "[]", "#969696");
        AssertHighlight(SyntaxHighlightID.Json, ",", "#969696");

        // Keys
        AssertHighlight(SyntaxHighlightID.Json, "\"name\": \"value\"", "#00CA00");
        AssertHighlight(SyntaxHighlightID.Json, "\"key\": 123", "#00CA00");

        // Block comments
        AssertHighlight(SyntaxHighlightID.Json, "/* comment */", "#888888");

        // Line comments
        AssertHighlight(SyntaxHighlightID.Json, "// comment", "#888888");

        // Strings
        AssertHighlight(SyntaxHighlightID.Json, "\"string value\"", "#00CA00");
        AssertHighlight(SyntaxHighlightID.Json, "'string value'", "#00CA00");
    }

    #endregion

    #region PHP Language Tests

    [UITestMethod]
    public void PHP_AllPatterns_ShouldHighlight()
    {
        // PHP tags
        AssertHighlight(SyntaxHighlightID.PHP, "<?php", "#FF0000");
        AssertHighlight(SyntaxHighlightID.PHP, "?>", "#FF0000");

        // Function names
        AssertHighlight(SyntaxHighlightID.PHP, "echo()", "#3300FF");
        AssertHighlight(SyntaxHighlightID.PHP, "myFunction()", "#3300FF");

        // Keywords
        AssertHighlight(SyntaxHighlightID.PHP, "echo 'Hello'", "#0077FF");
        AssertHighlight(SyntaxHighlightID.PHP, "if (condition)", "#0077FF");
        AssertHighlight(SyntaxHighlightID.PHP, "while (true)", "#0077FF");
        AssertHighlight(SyntaxHighlightID.PHP, "foreach ($array as $item)", "#0077FF");
        AssertHighlight(SyntaxHighlightID.PHP, "function test()", "#0077FF");

        // Numbers
        AssertHighlight(SyntaxHighlightID.PHP, "42", "#ff00ff");
        AssertHighlight(SyntaxHighlightID.PHP, "3.14", "#ff00ff");

        // Operators
        AssertHighlight(SyntaxHighlightID.PHP, "+ - * /", "#77FF77");
        AssertHighlight(SyntaxHighlightID.PHP, "== !=", "#77FF77");

        // Variables
        AssertHighlight(SyntaxHighlightID.PHP, "$variable", "#440044");
        AssertHighlight(SyntaxHighlightID.PHP, "$myVar", "#440044");

        // Double-quoted strings
        AssertHighlight(SyntaxHighlightID.PHP, "\"string\"", "#ff5500");

        // Single-quoted strings
        AssertHighlight(SyntaxHighlightID.PHP, "'string'", "#ff5500");

        // Regex in strings
        AssertHighlight(SyntaxHighlightID.PHP, "\"/pattern/i\"", "#ff5500");

        // Block comments
        AssertHighlight(SyntaxHighlightID.PHP, "/* comment */", "#888888");

        // Line comments
        AssertHighlight(SyntaxHighlightID.PHP, "// comment", "#888888");
    }

    #endregion

    #region Q# Language Tests

    [UITestMethod]
    public void QSharp_AllPatterns_ShouldHighlight()
    {
        // Special characters
        AssertHighlight(SyntaxHighlightID.QSharp, "!", "#BB0000");
        AssertHighlight(SyntaxHighlightID.QSharp, "?", "#BB0000");

        // Line comments
        AssertHighlight(SyntaxHighlightID.QSharp, "// comment", "#888888");

        // Keywords
        AssertHighlight(SyntaxHighlightID.QSharp, "namespace Quantum", "#0066bb");
        AssertHighlight(SyntaxHighlightID.QSharp, "open Microsoft.Quantum", "#0066bb");
        AssertHighlight(SyntaxHighlightID.QSharp, "operation Test()", "#0066bb");
        AssertHighlight(SyntaxHighlightID.QSharp, "using (q = Qubit())", "#0066bb");
        AssertHighlight(SyntaxHighlightID.QSharp, "let x = 5", "#0066bb");
        AssertHighlight(SyntaxHighlightID.QSharp, "H(q)", "#0066bb");
        AssertHighlight(SyntaxHighlightID.QSharp, "M(q)", "#0066bb");
        AssertHighlight(SyntaxHighlightID.QSharp, "Reset(q)", "#0066bb");
        AssertHighlight(SyntaxHighlightID.QSharp, "return result", "#0066bb");

        // Types
        AssertHighlight(SyntaxHighlightID.QSharp, "Qubit q", "#00bb66");
        AssertHighlight(SyntaxHighlightID.QSharp, "Result r", "#00bb66");
    }

    #endregion

    #region XML Language Tests

    [UITestMethod]
    public void XML_AllPatterns_ShouldHighlight()
    {
        // Numeric values
        AssertHighlight(SyntaxHighlightID.XML, "42", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.XML, "3.14", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.XML, "1.5e10", "#dd00dd");

        // Opening tags
        AssertHighlight(SyntaxHighlightID.XML, "<root>", "#969696");
        AssertHighlight(SyntaxHighlightID.XML, "<element attr=\"val\">", "#969696");

        // Closing tags
        AssertHighlight(SyntaxHighlightID.XML, "</root>", "#969696");
        AssertHighlight(SyntaxHighlightID.XML, "</element>", "#969696");

        // Self-closing tags
        AssertHighlight(SyntaxHighlightID.XML, "<item/>", "#969696");
        AssertHighlight(SyntaxHighlightID.XML, "<br />", "#969696");

        // Attributes
        AssertHighlight(SyntaxHighlightID.XML, "name=\"value\"", "#00CA00");
        AssertHighlight(SyntaxHighlightID.XML, "id=\"test\"", "#00CA00");

        // Double-quoted values
        AssertHighlight(SyntaxHighlightID.XML, "attr=\"value\"", "#00CA00");

        // Single-quoted values
        AssertHighlight(SyntaxHighlightID.XML, "attr='value'", "#00CA00");

        // Comments
        AssertHighlight(SyntaxHighlightID.XML, "<!-- comment -->", "#888888");
    }

    #endregion

    #region Python Language Tests

    [UITestMethod]
    public void Python_AllPatterns_ShouldHighlight()
    {
        // Numbers
        AssertHighlight(SyntaxHighlightID.Python, "42", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Python, "3.14", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Python, "1.5e10", "#dd00dd");

        // Keywords
        AssertHighlight(SyntaxHighlightID.Python, "def function():", "#aa00cc");
        AssertHighlight(SyntaxHighlightID.Python, "class MyClass:", "#aa00cc");
        AssertHighlight(SyntaxHighlightID.Python, "if condition:", "#aa00cc");
        AssertHighlight(SyntaxHighlightID.Python, "for i in range(10):", "#aa00cc");
        AssertHighlight(SyntaxHighlightID.Python, "return value", "#aa00cc");
        AssertHighlight(SyntaxHighlightID.Python, "import module", "#aa00cc");
        AssertHighlight(SyntaxHighlightID.Python, "True", "#aa00cc");
        AssertHighlight(SyntaxHighlightID.Python, "False", "#aa00cc");
        AssertHighlight(SyntaxHighlightID.Python, "None", "#aa00cc");

        // Function names
        AssertHighlight(SyntaxHighlightID.Python, "print()", "#cc9900");
        AssertHighlight(SyntaxHighlightID.Python, "my_function()", "#cc9900");

        // Double-quoted strings
        AssertHighlight(SyntaxHighlightID.Python, "\"string\"", "#ff5500");

        // Single-quoted strings
        AssertHighlight(SyntaxHighlightID.Python, "'string'", "#00CA00");

        // Comments
        AssertHighlight(SyntaxHighlightID.Python, "# comment", "#888888");

        // Multi-line strings
        AssertHighlight(SyntaxHighlightID.Python, "\"\"\"docstring\"\"\"", "#888888");
    }

    #endregion

    #region CSV Language Tests

    [UITestMethod]
    public void CSV_AllPatterns_ShouldHighlight()
    {
        // Delimiters
        AssertHighlight(SyntaxHighlightID.CSV, ":", "#1b9902");
        AssertHighlight(SyntaxHighlightID.CSV, ",", "#1b9902");
        AssertHighlight(SyntaxHighlightID.CSV, ";", "#1b9902");
        AssertHighlight(SyntaxHighlightID.CSV, "|", "#1b9902");
    }

    #endregion

    #region LaTeX Language Tests

    [UITestMethod]
    public void LaTeX_AllPatterns_ShouldHighlight()
    {
        // Commands
        AssertHighlight(SyntaxHighlightID.Latex, "\\documentclass", "#0033aa");
        AssertHighlight(SyntaxHighlightID.Latex, "\\begin", "#0033aa");
        AssertHighlight(SyntaxHighlightID.Latex, "\\end", "#0033aa");
        AssertHighlight(SyntaxHighlightID.Latex, "\\section", "#0033aa");

        // Comments
        AssertHighlight(SyntaxHighlightID.Latex, "% comment", "#888888");

        // Square brackets
        AssertHighlight(SyntaxHighlightID.Latex, "[option]", "#FFFF00");

        // Curly braces
        AssertHighlight(SyntaxHighlightID.Latex, "{content}", "#FF0000");

        // Dollar signs
        AssertHighlight(SyntaxHighlightID.Latex, "$x = y$", "#00bb00");
    }

    #endregion

    #region Markdown Language Tests

    [UITestMethod]
    public void Markdown_AllPatterns_ShouldHighlight()
    {
        // Headers
        AssertHighlight(SyntaxHighlightID.Markdown, "# Header 1", "#FF2E7E");
        AssertHighlight(SyntaxHighlightID.Markdown, "## Header 2", "#FF2E7E");
        AssertHighlight(SyntaxHighlightID.Markdown, "### Header 3", "#FF2E7E");

        // Bold
        AssertHighlight(SyntaxHighlightID.Markdown, "**bold**", "#E18800");
        AssertHighlight(SyntaxHighlightID.Markdown, "__bold__", "#E18800");

        // Italic
        AssertHighlight(SyntaxHighlightID.Markdown, "*italic*", "#C61AFF");
        AssertHighlight(SyntaxHighlightID.Markdown, "_italic_", "#C61AFF");

        // Bold + Italic
        AssertHighlight(SyntaxHighlightID.Markdown, "***bold italic***", "#C61AFF");
        AssertHighlight(SyntaxHighlightID.Markdown, "___bold italic___", "#C61AFF");

        // Inline code
        AssertHighlight(SyntaxHighlightID.Markdown, "`code`", "#F1C40F");

        // Code blocks
        AssertHighlight(SyntaxHighlightID.Markdown, "```code block```", "#F39C12");

        // Blockquotes
        AssertHighlight(SyntaxHighlightID.Markdown, "> quote", "#8DA284");

        // Numbered lists
        AssertHighlight(SyntaxHighlightID.Markdown, "1. item", "#2ECC71");

        // Bullet lists
        AssertHighlight(SyntaxHighlightID.Markdown, "- item", "#2ECC71");
        AssertHighlight(SyntaxHighlightID.Markdown, "* item", "#2ECC71");
        AssertHighlight(SyntaxHighlightID.Markdown, "+ item", "#2ECC71");

        // Links
        AssertHighlight(SyntaxHighlightID.Markdown, "[text](url)", "#3498DB");

        // Images
        AssertHighlight(SyntaxHighlightID.Markdown, "![alt](url)", "#FF6F61");

        // Special characters
        AssertHighlight(SyntaxHighlightID.Markdown, "~", "#E74C3C");
        AssertHighlight(SyntaxHighlightID.Markdown, "`", "#E74C3C");
        AssertHighlight(SyntaxHighlightID.Markdown, "*", "#E74C3C");
    }

    #endregion

    #region CSS Language Tests

    [UITestMethod]
    public void CSS_AllPatterns_ShouldHighlight()
    {
        // Properties
        AssertHighlight(SyntaxHighlightID.CSS, "color: red;", "#ff5500");
        AssertHighlight(SyntaxHighlightID.CSS, "font-size: 16px;", "#ff5500");

        // Functions
        AssertHighlight(SyntaxHighlightID.CSS, "rgb(255, 0, 0)", "#bb00bb");
        AssertHighlight(SyntaxHighlightID.CSS, "calc(100% - 20px)", "#bb00bb");

        // Pseudo classes/elements
        AssertHighlight(SyntaxHighlightID.CSS, ":hover {", "#0033ff");
        AssertHighlight(SyntaxHighlightID.CSS, "::before {", "#0033ff");

        // Selectors
        AssertHighlight(SyntaxHighlightID.CSS, ".class {", "#227700");
        AssertHighlight(SyntaxHighlightID.CSS, "#id {", "#227700");
        AssertHighlight(SyntaxHighlightID.CSS, "div {", "#227700");

        // Units
        AssertHighlight(SyntaxHighlightID.CSS, "100px", "#cc0000");
        AssertHighlight(SyntaxHighlightID.CSS, "50%", "#cc0000");
        AssertHighlight(SyntaxHighlightID.CSS, "2rem", "#cc0000");
        AssertHighlight(SyntaxHighlightID.CSS, "1.5em", "#cc0000");

        // Numbers
        AssertHighlight(SyntaxHighlightID.CSS, "100", "#0000ff");
        AssertHighlight(SyntaxHighlightID.CSS, "0.5", "#0000ff");

        // At-rules
        AssertHighlight(SyntaxHighlightID.CSS, "@media", "#8800ff");
        AssertHighlight(SyntaxHighlightID.CSS, "@import", "#8800ff");
        AssertHighlight(SyntaxHighlightID.CSS, "@keyframes", "#8800ff");

        // Hex colors
        AssertHighlight(SyntaxHighlightID.CSS, "#FF0000", "#00bb55");
        AssertHighlight(SyntaxHighlightID.CSS, "#FFF", "#00bb55");

        // Strings
        AssertHighlight(SyntaxHighlightID.CSS, "\"string\"", "#00aaff");
        AssertHighlight(SyntaxHighlightID.CSS, "'string'", "#00aaff");

        // Special characters
        AssertHighlight(SyntaxHighlightID.CSS, ";", "#777777");
        AssertHighlight(SyntaxHighlightID.CSS, ":", "#777777");
        AssertHighlight(SyntaxHighlightID.CSS, "{", "#777777");
        AssertHighlight(SyntaxHighlightID.CSS, "}", "#777777");

        // Comments
        AssertHighlight(SyntaxHighlightID.CSS, "/* comment */", "#555555");
    }

    #endregion

    #region SQL Language Tests

    [UITestMethod]
    public void SQL_AllPatterns_ShouldHighlight()
    {
        // Keywords
        AssertHighlight(SyntaxHighlightID.SQL, "SELECT * FROM table", "#FF6A00");
        AssertHighlight(SyntaxHighlightID.SQL, "INSERT INTO table", "#FF6A00");
        AssertHighlight(SyntaxHighlightID.SQL, "UPDATE table SET", "#FF6A00");
        AssertHighlight(SyntaxHighlightID.SQL, "DELETE FROM table", "#FF6A00");
        AssertHighlight(SyntaxHighlightID.SQL, "WHERE condition", "#FF6A00");
        AssertHighlight(SyntaxHighlightID.SQL, "JOIN table ON", "#FF6A00");
        AssertHighlight(SyntaxHighlightID.SQL, "GROUP BY column", "#FF6A00");
        AssertHighlight(SyntaxHighlightID.SQL, "ORDER BY column", "#FF6A00");

        // Double-quoted strings
        AssertHighlight(SyntaxHighlightID.SQL, "\"string\"", "#42C22B");

        // Single-quoted strings
        AssertHighlight(SyntaxHighlightID.SQL, "'string'", "#42C22B");

        // Numbers
        AssertHighlight(SyntaxHighlightID.SQL, "42", "#2B3BFF");
        AssertHighlight(SyntaxHighlightID.SQL, "3.14", "#2B3BFF");
        AssertHighlight(SyntaxHighlightID.SQL, "1.5e10", "#2B3BFF");

        // Aggregate functions
        AssertHighlight(SyntaxHighlightID.SQL, "COUNT(*)", "#11C9DB");
        AssertHighlight(SyntaxHighlightID.SQL, "SUM(column)", "#11C9DB");
        AssertHighlight(SyntaxHighlightID.SQL, "AVG(column)", "#11C9DB");
        AssertHighlight(SyntaxHighlightID.SQL, "MIN(column)", "#11C9DB");
        AssertHighlight(SyntaxHighlightID.SQL, "MAX(column)", "#11C9DB");

        // Dot notation
        AssertHighlight(SyntaxHighlightID.SQL, "table.column", "#901F9E");
    }

    #endregion

    #region Lua Language Tests

    [UITestMethod]
    public void Lua_AllPatterns_ShouldHighlight()
    {
        // Numbers
        AssertHighlight(SyntaxHighlightID.Lua, "42", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Lua, "3.14", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Lua, "0xFF", "#dd00dd");
        AssertHighlight(SyntaxHighlightID.Lua, "1.5e10", "#dd00dd");

        // Keywords
        AssertHighlight(SyntaxHighlightID.Lua, "function test()", "#aa00cc");
        AssertHighlight(SyntaxHighlightID.Lua, "local x = 10", "#aa00cc");
        AssertHighlight(SyntaxHighlightID.Lua, "if condition then", "#aa00cc");
        AssertHighlight(SyntaxHighlightID.Lua, "for i = 1, 10 do", "#aa00cc");
        AssertHighlight(SyntaxHighlightID.Lua, "while true do", "#aa00cc");
        AssertHighlight(SyntaxHighlightID.Lua, "return value", "#aa00cc");
        AssertHighlight(SyntaxHighlightID.Lua, "nil", "#aa00cc");
        AssertHighlight(SyntaxHighlightID.Lua, "true", "#aa00cc");
        AssertHighlight(SyntaxHighlightID.Lua, "false", "#aa00cc");

        // Function names
        AssertHighlight(SyntaxHighlightID.Lua, "print()", "#cc9900");
        AssertHighlight(SyntaxHighlightID.Lua, "myFunc()", "#cc9900");

        // Double-quoted strings
        AssertHighlight(SyntaxHighlightID.Lua, "\"string\"", "#ff5500");

        // Single-quoted strings
        AssertHighlight(SyntaxHighlightID.Lua, "'string'", "#00CA00");

        // Single-line comments
        AssertHighlight(SyntaxHighlightID.Lua, "-- comment", "#888888");

        // Multi-line comments
        AssertHighlight(SyntaxHighlightID.Lua, "--[[ comment ]]", "#888888");
    }

    #endregion
}