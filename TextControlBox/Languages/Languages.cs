using TextControlBoxNS.Models;

namespace TextControlBoxNS.Languages
{
    internal class Batch : SyntaxHighlightLanguage
    {
        public Batch()
        {
            this.Name = "Batch";
            this.Author = "Julius Kirsch";
            this.Filter = new string[1] { ".bat" };
            this.Description = "Syntax highlighting for Batch language";
            this.Highlights = new SyntaxHighlights[]
            {
                new SyntaxHighlights("\\b([+-]?(?=\\.\\d|\\d)(?:\\d+)?(?:\\.?\\d*))(?:[eE]([+-]?\\d+))?\\b", "#dd00dd", "#ff00ff"),
                new SyntaxHighlights("(?i)(set|echo|for|pushd|popd|pause|exit|cd|if|else|goto|del)\\s", "#dd00dd", "#dd00dd"),
                new SyntaxHighlights("(:.*)", "#00C000", "#ffff00"),
                new SyntaxHighlights("(\\\".+?\\\"|\\'.+?\\')", "#00C000", "#ffff00"),
                new SyntaxHighlights("(@|%)", "#dd0077", "#dd0077"),
                new SyntaxHighlights("(\\*)", "#dd0077", "#dd0077"),
                new SyntaxHighlights("((?i)rem.*)", "#888888", "#888888"),
            };
        }
    }
    internal class IniHighlighter : SyntaxHighlightLanguage
    {
        public IniHighlighter()
        {
            this.Name = "INI";
            this.Filter = new[] { ".ini" };
            this.Description = "Syntax highlighting for INI configuration files";
            this.AutoPairingPair = new AutoPairingPair[]
            {
            new AutoPairingPair("[", "]")
            };
            this.Highlights = new SyntaxHighlights[]
            {
            // Section names [section_name]
            new SyntaxHighlights(@"(?<=\[)[^\]]+(?=\])", "#7C4DFF", "#B890FF"),
            
            // Brackets [ and ]
            new SyntaxHighlights(@"\[|\]", "#5C6BC0", "#9FA8DA"),
            
            // Keys before = (supports spaces in keys)
            new SyntaxHighlights(@"(?m)^\s*([^=;\[\]#]+?)(?=\s*=)", "#E65100", "#FFB74D"),
            
            // String values (quoted)
            new SyntaxHighlights(@"(?<=[:=]\s*)([""'])(.*?)(\1)", "#00796B", "#80CBC4"),
            
            // Numeric values
            new SyntaxHighlights(@"\b(?:0[xX][0-9A-Fa-f]+(?:_[0-9A-Fa-f]+)*|0[bB][01]+(?:_[01]+)*|\d+(?:_\d+)*\.\d*(?:_\d*)?(?:[eE][+-]?\d+(?:_\d+)*)?|\.\d+(?:_\d*)?(?:[eE][+-]?\d+(?:_\d+)*)?|\d+(?:_\d+)*(?:[eE][+-]?\d+(?:_\d+)*)?)(?:[fFlLuU]{0,3})\b",
                "#1565C0", "#64B5F6"
                ),
            
            // Boolean-like values (yes/no, true/false, on/off, 0/1)
            new SyntaxHighlights(@"(?i)(?<=[:=]\s*)(yes|no|true|false|on|off|enabled?|disabled?)\b", "#FF5722", "#FFAB91"),
            
            // Comments starting with ; or #
            new SyntaxHighlights(@"[;#].*", "#9E9E9E", "#BDBDBD"),
            };
        }
    }
    internal class TomlHighlighter : SyntaxHighlightLanguage
    {
        public TomlHighlighter()
        {
            this.Name = "TOML";
            this.Filter = new[] { ".toml" };
            this.Description = "Syntax highlighting for TOML configuration files";
            this.AutoPairingPair = new AutoPairingPair[]
            {
            new AutoPairingPair("[", "]"),
            new AutoPairingPair("{", "}")
            };
            this.Highlights = new SyntaxHighlights[]
            {
            // Table headers [[array.of.tables]]
            new SyntaxHighlights(@"(?<=\[\[)[^\]]+(?=\]\])", "#9C27B0", "#CE93D8"),
            
            // Section headers [table.name]
            new SyntaxHighlights(@"(?<=\[)[^\]]+(?=\])", "#7C4DFF", "#B890FF"),
            
            // Double brackets for arrays of tables
            new SyntaxHighlights(@"\[\[|\]\]", "#5C6BC0", "#9FA8DA"),
            
            // Single brackets
            new SyntaxHighlights(@"\[|\]", "#5C6BC0", "#9FA8DA"),
            
            // Braces { and } for inline tables
            new SyntaxHighlights(@"\{|\}", "#5C6BC0", "#9FA8DA"),
            
            // Keys before = (dotted keys supported)
            new SyntaxHighlights(@"(?m)^\s*([\w\.-]+)(?=\s*=)", "#E65100", "#FFB74D"),
            
            // Keys in inline tables
            new SyntaxHighlights(@"(?<=[\{,]\s*)([\w\.-]+)(?=\s*=)", "#E65100", "#FFB74D"),
            
            // Multi-line strings '''..''' or """..."""
            new SyntaxHighlights(@"('''[\s\S]*?'''|""""""[\s\S]*?"""""")", "#00796B", "#80CBC4"),
            
            // Basic strings "..." or '...'
            new SyntaxHighlights(@"([""'])((?:\\.|(?!\1).)*?)\1", "#00796B", "#80CBC4"),
            
            // Dates and times (ISO 8601)
            new SyntaxHighlights(@"\d{4}-\d{2}-\d{2}([T ]\d{2}:\d{2}:\d{2}(\.\d+)?(Z|[+-]\d{2}:\d{2})?)?", "#1565C0", "#64B5F6"),
            
            // Floats (including scientific notation, inf, nan)
            new SyntaxHighlights(@"(?i)(?<=[:=]\s*)([-+]?(\d+\.\d*|\.\d+|\d+)([eE][-+]?\d+)?|[-+]?inf|[-+]?nan)\b", "#1565C0", "#64B5F6"),
            
            // Integers (including hex, octal, binary)
            new SyntaxHighlights(@"(?<=[:=]\s*)([-+]?(0x[0-9a-fA-F_]+|0o[0-7_]+|0b[01_]+|\d[0-9_]*))\b", "#1565C0", "#64B5F6"),
            
            // Booleans
            new SyntaxHighlights(@"(?<=[:=]\s*)(true|false)\b", "#FF5722", "#FFAB91"),
            
            // Commas
            new SyntaxHighlights(@",", "#5C6BC0", "#9FA8DA"),
            
            // Comments starting with #
            new SyntaxHighlights(@"#.*", "#9E9E9E", "#BDBDBD"),
            };
        }
    }
    internal class KlipperHighlighter : SyntaxHighlightLanguage
    {
        public KlipperHighlighter()
        {
            this.Name = "Klipper";
            this.Filter = new[] { ".cfg", ".conf" };
            this.Description = "Syntax highlighting for Klipper 3D printer configuration files";
            this.AutoPairingPair = new AutoPairingPair[]
            {
            new AutoPairingPair("[", "]")
            };
            this.Highlights = new SyntaxHighlights[]
            {
            // Section names with optional parameters [stepper_x], [gcode_macro NAME]
            new SyntaxHighlights(@"(?<=\[)[^\]]+(?=\])", "#7C4DFF", "#B890FF"),
            
            // Brackets [ and ]
            new SyntaxHighlights(@"\[|\]", "#5C6BC0", "#9FA8DA"),
            
            // Keys before : (Klipper uses colons)
            new SyntaxHighlights(@"(?m)^\s*([a-zA-Z_][\w]*?)(?=\s*:)", "#E65100", "#FFB74D"),
            
            // Pin names and hardware references (e.g., PA1, ar54, ^PA2, ~!PB3)
            new SyntaxHighlights(@"(?<=[:\s])([~^!]*[PZ][A-Z]\d+|ar\d+)\b", "#00897B", "#4DB6AC"),
            
            // Gcode commands (G0, G1, M104, etc.)
            new SyntaxHighlights(@"\b[GM]\d+\b", "#AD1457", "#F06292"),
            
            // String values (quoted)
            new SyntaxHighlights(@"([""'])((?:\\.|(?!\1).)*?)\1", "#00796B", "#80CBC4"),
            
            // Numeric values with units (e.g., 100.0, 50mm, 0.2s, 45deg)
            new SyntaxHighlights(@"(?<=[:\s])([-+]?\d*\.?\d+)\s*(mm|s|deg|%|°)?", "#1565C0", "#64B5F6"),
            
            // Boolean values (True/False, true/false)
            new SyntaxHighlights(@"(?i)(?<=[:\s])(true|false)\b", "#FF5722", "#FFAB91"),
            
            // Template expressions {variable_name}
            new SyntaxHighlights(@"\{[^}]+\}", "#6A1B9A", "#BA68C8"),
            
            // Commas in lists
            new SyntaxHighlights(@",", "#5C6BC0", "#9FA8DA"),
            
            // Comments starting with #
            new SyntaxHighlights(@"#.*", "#9E9E9E", "#BDBDBD"),
            };
        }
    }
    internal class Cpp : SyntaxHighlightLanguage
    {
        public Cpp()
        {
            this.Name = "C++";
            this.Author = "Julius Kirsch";
            this.Filter = new string[6] { ".cpp", ".cxx", ".cc", ".hpp", ".h", ".c" };
            this.Description = "Syntax highlighting for C++ language";
            this.AutoPairingPair = new AutoPairingPair[5]
            {
                new AutoPairingPair("{", "}"),
                new AutoPairingPair("[", "]"),
                new AutoPairingPair("(", ")"),
                new AutoPairingPair("\""),
                new AutoPairingPair("\'")
            };
            this.Highlights = new SyntaxHighlights[]
            {
                new SyntaxHighlights(
                    // Numeric literals (int, float, hex, binary, scientific)
                    @"\b(?:0[xX][0-9A-Fa-f]+(?:_[0-9A-Fa-f]+)*|0[bB][01]+(?:_[01]+)*|\d+(?:_\d+)*\.\d*(?:_\d*)?(?:[eE][+-]?\d+(?:_\d+)*)?|\.\d+(?:_\d*)?(?:[eE][+-]?\d+(?:_\d+)*)?|\d+(?:_\d+)*(?:[eE][+-]?\d+(?:_\d+)*)?)(?:[fFlLuU]{0,3})\b",
                    "#dd00dd", "#00ff00"
                ),
                new SyntaxHighlights(
                    // Function calls (identifier followed by parentheses)
                    @"(?<!\w)([a-zA-Z_]\w*)(?=\s*\()",
                    "#4455ff", "#ffbb00"
                ),

                new SyntaxHighlights(
                    // Keywords
                    @"\b(string|uint16_t|uint8_t|alignas|alignof|and|and_eq|asm|auto|bitand|bitor|bool|break|case|catch|char|char8_t|char16_t|char32_t|class|compl|concept|const|const_cast|consteval|constexpr|constinit|continue|co_await|co_return|co_yield|decltype|default|delete|do|double|dynamic_cast|else|enum|explicit|export|extern|false|float|for|friend|goto|if|inline|int|long|mutable|namespace|new|noexcept|not|not_eq|nullptr|operator|or|or_eq|private|protected|public|register|reinterpret_cast|requires|return|short|signed|sizeof|static|static_assert|static_cast|struct|switch|template|this|thread_local|throw|true|try|typedef|typeid|typename|union|unsigned|using|virtual|void|volatile|wchar_t|while|xor|xor_eq)\b",
                    "#dd00dd", "#dd00dd"
                ),

                new SyntaxHighlights(
                    // Preprocessor directives (#include, #define, etc.)
                    @"^\s*#\s*(define|elif|else|endif|error|ifndef|ifdef|if|import|include|line|pragma|region|undef|using)\b",
                    "#5F5E5E", "#999999"
                ),

                new SyntaxHighlights(
                    // Strings and chars (handles escaped quotes)
                    @"(""([^""\\]|\\.)*""|'([^'\\]|\\.)*')",
                    "#D98300", "#00FF00"
                ),

                new SyntaxHighlights(
                    // Block comments (/* ... */)
                    @"/\*[\s\S]*?\*/",
                    "#6B6A6A", "#646464"
                ),

                new SyntaxHighlights(
                    // Line comments (// ...)
                    @"//.*",
                    "#6B6A6A", "#646464"
                ),
            };
        }
    }
    internal class CSharp : SyntaxHighlightLanguage
    {
        public CSharp()
        {
            this.Name = "C#";
            this.Author = "Julius Kirsch";
            this.Filter = new string[1] { ".cs" };
            this.Description = "Syntax highlighting for C# language";
            this.AutoPairingPair = new AutoPairingPair[5]
            {
                new AutoPairingPair("{", "}"),
                new AutoPairingPair("[", "]"),
                new AutoPairingPair("(", ")"),
                new AutoPairingPair("\""),
                new AutoPairingPair("\'")
            };
            this.Highlights = new SyntaxHighlights[]
            {
                //Number Matching
                new SyntaxHighlights(
                    @"\b(?:0[xX][0-9A-Fa-f]+(?:_[0-9A-Fa-f]+)*|0[bB][01]+(?:_[01]+)*|\d+(?:_\d+)*\.\d*(?:_\d*)?(?:[eE][+-]?\d+(?:_\d+)*)?|\.\d+(?:_\d*)?(?:[eE][+-]?\d+(?:_\d+)*)?|\d+(?:_\d+)*(?:[eE][+-]?\d+(?:_\d+)*)?)(?:[fFlLuU]{0,3})\b",
                    "#ff00ff", "#ff00ff"),
            
                //Function Names
                new SyntaxHighlights("\\b[a-zA-Z_]\\w*(?=\\()", "#880088", "#ffbb00"),
            
                //C# Keywords
                new SyntaxHighlights("\\b(abstract|as|async|await|base|bool|break|byte|case|catch|char|checked|class|const|continue|decimal|default|delegate|do|double|else|enum|event|explicit|extern|external|false|final|finally|fixed|float|for|foreach|get|goto|if|implicit|in|int|interface|internal|is|lock|long|namespace|new|null|object|operator|out|override|params|partial|private|protected|public|readonly|ref|return|sbyte|sealed|set|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|value|var|virtual|void|volatile|while)\\b", "#0066bb", "#00ffff"),
            
                //Common C# Types
                new SyntaxHighlights("\\b(List|Color|Console|Debug|Dictionary|Stack|Queue|GC)\\b", "#008000", "#ff9900"),
            
                //Try/Catch/Finally Blocks
                new SyntaxHighlights("\\b(try|catch|finally)\\b", "#9922ff", "#6666ff"),
                        
                //Preprocessor Directives
                new SyntaxHighlights("#region.*$", "#ff0000", "#ff0000", true),
                new SyntaxHighlights("#endregion", "#ff0000", "#ff0000", true),
            
                //String Literals
                new SyntaxHighlights("@\".*?\"|\"(?:\\\\.|[^\"\\\\])*\"", "#ff5500", "#00FF00"),
            
                //Character Literals
                new SyntaxHighlights("'[^\\n]*?'", "#ff5500", "#00FF00"),
            
                //Single-line Comments
                new SyntaxHighlights("\\/\\/.*", "#888888", "#646464"),
            
                //Multi-line Comments
                new SyntaxHighlights("/\\*(?:[^*]|\\*[^/])*\\*/", "#888888", "#646464"),
            };
        }
    }
    internal class GCode : SyntaxHighlightLanguage
    {
        public GCode()
        {
            this.Name = "GCode";
            this.Author = "Julius Kirsch";
            this.Filter = new string[5] { ".ngc", ".tap", ".gcode", ".nc", ".cnc" };
            this.Description = "Syntax highlighting for GCode language";
            this.Highlights = new SyntaxHighlights[]
            { 
                new SyntaxHighlights("\\bY(?=([0-9]|(\\.|\\+|\\-)[0-9]))", "#00ff00", "#00ff00"),
                new SyntaxHighlights("\\bX(?=([0-9]|(\\.|\\+|\\-)[0-9]))", "#ff0000", "#ff0000"),
                new SyntaxHighlights("\\bZ(?=([0-9]|(\\.|\\+|\\-)[0-9]))", "#0077ff", "#0077ff"),
                new SyntaxHighlights("\\bA(?=([0-9]|(\\+|\\-)[0-9]))", "#ff00ff", "#ff00ff"),
                new SyntaxHighlights("\\b(E|F)(?=(\\-.|\\.|[0-9]|(\\+|\\-)[0-9]))", "#ffAA00", "#ffAA00"),
                new SyntaxHighlights("\\b(S|T)(?=(\\-.|\\.|[0-9]|(\\+|\\-)[0-9]))", "#ffff00", "#ffff00"),
                new SyntaxHighlights("([+-]?(?=\\.\\d|\\d)(?:\\d+)?(?:\\.?\\d*))(?:[eE]([+-]?\\d+))?", "#ff00ff", "#9f009f"),
                new SyntaxHighlights("[G|M][0-999].*?[\\s|\\n]", "#00aaaa", "#00ffff"),
                new SyntaxHighlights("(;|\\/\\/|\\brem\\b).*", "#888888", "#888888"),
            };
        }
    }
    internal class x86Assembly : SyntaxHighlightLanguage
    {
        public x86Assembly()
        {
            this.Name = "x86Assembly";
            this.Author = "Eustathios Koutsos";
            this.Filter = new string[1] { ".asm" };
            this.Description = "Syntax highlighting for the original x86 ISA assembly";
            this.Highlights = new SyntaxHighlights[]
            {
                new SyntaxHighlights("^((?i)rep|^(?i)repe|^(?i)repne|^(?i)repnz|^(?i)repz)($|\\s)", "#5e00c7", "#983cff", true), // prefixes
                new SyntaxHighlights(@"(?i)\b(aaa|aad|aam|aas|adc|add|and|call|cbw|clc|cld|cli|cmc|cmp|cmpsb|cmpsw|cwd|daa|das|dec|div|esc|hlt|idiv|imul|in|inc|int|into|iret|ja|jae|jb|jbe|jc|je|jg|jge|jl|jle|jna|jnae|jnb|jnbe|jnc|jne|jng|jnge|jnl|jnle|jno|jnp|jns|jnz|jo|jp|jpe|jpo|js|jz|jcxz|jmp|lahf|lds|lea|les|lock|lodsb|lodsw|loop|mov|movsb|movsw|mul|neg|nop|not|or|out|pop|popf|push|pushf|rcl|rcr|ret|retn|retf|rol|ror|sahf|sal|sar|sbb|scasb|scasw|shl|shr|stc|std|sti|stosb|stosw|sub|test|wait|xchg|xlat|xor)($|\s)", "#8400ff", "#8400ff", true), // instructions
                new SyntaxHighlights(@"(?i)(?<=^|\\s|\\[|\\]|\\*|\\+|-)(rax|eax|ax|ah|al|rbx|ebx|bx|bh|bl|rcx|ecx|cx|ch|cl|rdx|edx|dx|dh|dl|rdi|edi|di|dil|rsi|esi|si|sil|cs|ds|ss|es|fs|rbp|ebp|bp|bpl|rip|eip|ip|rsp|esp|sp|spl)\b", "#c4aa00", "#c4aa00"), // registers
                new SyntaxHighlights("0b|0[xX][0-9a-fA-F]+|[0-9]+|0[bB][0-1]+", "#558900", "#74bd00"), // values
                new SyntaxHighlights("^\\s*(?i)int", "#005eb0", "#1994ff", true), // int
                new SyntaxHighlights("(\\[|\\]|\\*|\\+|-])", "#0075c3", "#0099ff"), // brackets
                new SyntaxHighlights("^\\s*[aA-zZ]+:", "#007a41", "#00ab5b", italic: true), // labels
                new SyntaxHighlights("\\s*;(.*)", "#028100", "#04d400") // comments
            };
        }
    }
    internal class HexFile : SyntaxHighlightLanguage
    {
        public HexFile()
        {
            this.Name = "HexFile";
            this.Author = "Finn Freitag";
            this.Filter = new string[2] { ".hex", ".bin" };
            this.Description = "Syntaxhighlighting for hex and binary code.";
            this.Highlights = new SyntaxHighlights[]
            {
                new SyntaxHighlights("\\:", "#FFFF00", "#FFFF00"),
                new SyntaxHighlights("\\:([0-9A-Fa-f]{2})", "#00FF00", "#00FF00"),
                new SyntaxHighlights("\\:[0-9A-Fa-f]{2}([0-9A-Fa-f]{4})", "#00FF00", "#00FF00"),
                new SyntaxHighlights("\\:[0-9A-Fa-f]{6}([0-9A-Fa-f]{2})", "#FF5500", "#FF5500"),
                new SyntaxHighlights("\\:[0-9A-Fa-f]{8}([0-9A-Fa-f]*)[0-9A-Fa-f]{2}", "#00FFFF", "#00FFFF"),
                new SyntaxHighlights("\\:[0-9A-Fa-f]{8}[0-9A-Fa-f]*([0-9A-Fa-f]{2})", "#666666", "#666666"),
                new SyntaxHighlights("//.*", "#666666", "#666666"),
                new SyntaxHighlights("[^0-9A-Fa-f\\:\\n]", "#FF0000", "#FF0000", false, false, true),
            };
        }
    }
    internal class Html : SyntaxHighlightLanguage
    {
        public Html()
        {
            this.Name = "Html";
            this.Author = "Julius Kirsch";
            this.Filter = new string[2] { ".html", ".htm" };
            this.Description = "Syntax highlighting for Html language";

            this.Highlights = new SyntaxHighlights[]
            {
                new SyntaxHighlights("\\b([+-]?(?=\\.\\d|\\d)(?:\\d+)?(?:\\.?\\d*))(?:[eE]([+-]?\\d+))?\\b", "#dd00dd", "#ff00ff"),
                new SyntaxHighlights("[-A-Za-z_]+\\=", "#00CA00", "#Ff0000"),
                new SyntaxHighlights("<([^ >!\\/]+)[^>]*>", "#969696", "#0099ff"),
                new SyntaxHighlights("<+[/]+[a-zA-Z0-9:?\\-_]+>", "#969696", "#0099ff"),
                new SyntaxHighlights("<[a-zA-Z0-9:?\\-]+?.*\\/>", "#969696", "#0099ff"),
                new SyntaxHighlights("\"[^\\n]*?\"", "#00CA00", "#00FF00"),
                new SyntaxHighlights("'[^\\n]*?'", "#00CA00", "#00FF00"),
                new SyntaxHighlights("[0-9]+(px|rem|em|vh|vw|px|pt|pc|in|mm|cm|deg|%)", "#ff00ff", "#dd00dd"),
                new SyntaxHighlights("<!--[\\s\\S]*?-->", "#888888", "#888888"),
            };
        }
    }
    internal class Java : SyntaxHighlightLanguage
    {
        public Java()
        {
            this.Name = "Java";
            this.Author = "Julius Kirsch";
            this.Filter = new string[2] { ".java", ".class" };
            this.Description = "Syntax highlighting for Java language";
            this.AutoPairingPair = new AutoPairingPair[5]
            {
                new AutoPairingPair("{", "}"),
                new AutoPairingPair("[", "]"),
                new AutoPairingPair("(", ")"),
                new AutoPairingPair("\""),
                new AutoPairingPair("\'")
            };
            this.Highlights = new SyntaxHighlights[]
            {
                new SyntaxHighlights("\\b([+-]?(?=\\.\\d|\\d)(?:\\d+)?(?:\\.?\\d*))(?:[eE]([+-]?\\d+))?\\b", "#ff00ff", "#ff00ff"),
                new SyntaxHighlights("(?<!(def\\s))(?<=^|\\s|.)[a-zA-Z_][\\w_]*(?=\\()", "#880088", "#ffbb00"),
                new SyntaxHighlights("\\b(System.out|System|Math)\\b", "#008000", "#ff9900"),
                new SyntaxHighlights("\\b(abstract|assert|boolean|break|byte|case|catch|char|class|const|continue|default|do|double|else|enum|extends|final|finally|float|for|goto|if|implements|import|instanceof|int|interface|long|native|new|package|private|protected|public|return|short|static|super|switch|synchronized|this|throw|throws|transient|try|void|volatile|while|exports|modle|non-sealed|open|opens|permits|provides|record|requires|sealed|to|transitive|uses|var|with|yield|true|false|null)\\b", "#0066bb", "#00ffff"),
                new SyntaxHighlights("\"[^\\n]*?\"", "#ff5500", "#00FF00"),
                new SyntaxHighlights("'[^\\n]*?'", "#00CA00", "#00FF00"),
                new SyntaxHighlights("/\\*[^*]*\\*+(?:[^/*][^*]*\\*+)*/", "#888888", "#646464"),
                new SyntaxHighlights("\\/\\/.*", "#888888", "#646464"),
            };
        }
    }
    internal class Javascript : SyntaxHighlightLanguage
    {
        public Javascript()
        {
            this.Name = "Javascript";
            this.Author = "Finn Freitag";
            this.Filter = new string[1] { ".js" };
            this.Description = "Syntax highlighting for Javascript language";
            this.AutoPairingPair = new AutoPairingPair[6]
            {
                new AutoPairingPair("{", "}"),
                new AutoPairingPair("[", "]"),
                new AutoPairingPair("(", ")"),
                new AutoPairingPair("`", "`"),
                new AutoPairingPair("\""),
                new AutoPairingPair("\'")
            };

            this.Highlights = new SyntaxHighlights[]
            {
                new SyntaxHighlights("\\W", "#990033", "#CC0066"),
                new SyntaxHighlights("(\\+|\\-|\\*|/|%|\\=|\\:|\\!|>|\\<|\\?|&|\\||\\~|\\^)", "#77FF77", "#77FF77"),
                new SyntaxHighlights("\\b([+-]?(?=\\.\\d|\\d)(?:\\d+)?(?:\\.?\\d*))(?:[eE]([+-]?\\d+))?\\b", "#ff00ff", "#ff00ff"),
                new SyntaxHighlights("(?<!(def\\s))(?<=^|\\s|.)[a-zA-Z_][\\w_]*(?=\\()", "#880088", "#ffbb00"),
                new SyntaxHighlights("\\b(goto|in|instanceof|static|arguments|public|do|else|const|function|class|return|let|eval|for|if|this|break|debugger|yield|extends|enum|continue|export|null|switch|private|new|throw|while|case|await|delete|super|default|void|var|protected|package|interface|false|typeof|implements|with|import|true)\\b", "#0066bb", "#00ffff"),
                new SyntaxHighlights("\\b(document|window|screen)\\b", "#008000", "#33BB00"),
                new SyntaxHighlights("\\b(try|catch|finally)\\b", "#9922ff", "#6666ff"),
                new SyntaxHighlights("/[^\\n]*/i{0,1}", "#FFFF00", "#FFFF00"),
                new SyntaxHighlights("[\"'][^\\n]*?[\"']", "#ff5500", "#00FF00"),
                new SyntaxHighlights("/\\*[^*]*\\*+(?:[^/*][^*]*\\*+)*/", "#888888", "#646464"),
                new SyntaxHighlights("\\/\\/.*", "#888888", "#646464"),
            };
        }
    }
    internal class Json : SyntaxHighlightLanguage
    {
        public Json()
        {
            this.Name = "Json";
            this.Author = "Julius Kirsch";
            this.Filter = new string[1] { ".json" };
            this.Description = "Syntax highlighting for Json language";
            this.AutoPairingPair = new AutoPairingPair[4]
            {
                new AutoPairingPair("{", "}"),
                new AutoPairingPair("[", "]"),
                new AutoPairingPair("\""),
                new AutoPairingPair("\'")
            };

            this.Highlights = new SyntaxHighlights[]
            {
                new SyntaxHighlights("\\b([+-]?(?=\\.\\d|\\d)(?:\\d+)?(?:\\.?\\d*))(?:[eE]([+-]?\\d+))?\\b", "#dd00dd", "#ff00ff"),
                new SyntaxHighlights("(null|true|false)", "#00AADD", "#0099ff"),
                new SyntaxHighlights("(,|{|}|\\[|\\])", "#969696", "#646464"),
                new SyntaxHighlights("(\".+\")\\:", "#00CA00", "#dddd00"),
                new SyntaxHighlights("/\\*[^*]*\\*+(?:[^/*][^*]*\\*+)*/", "#888888", "#646464"),
                new SyntaxHighlights("\\/\\/.*", "#888888", "#646464"),
                new SyntaxHighlights("'[^\\n]*?'", "#00CA00", "#00FF00"),
                new SyntaxHighlights("\"[^\\n]*?\"", "#00CA00", "#00FF00"),
            };
        }
    }
    internal class PHP : SyntaxHighlightLanguage
    {
        public PHP()
        {
            this.Name = "PHP";
            this.Author = "Finn Freitag";
            this.Filter = new string[1] { ".php" };
            this.Description = "Syntax highlighting for PHP language";
            this.AutoPairingPair = new AutoPairingPair[2]
            {
                new AutoPairingPair("\""),
                new AutoPairingPair("\'")
            };

            this.Highlights = new SyntaxHighlights[]
            {
                new SyntaxHighlights("\\<\\?php", "#FF0000", "#FF0000"),
                new SyntaxHighlights("\\?\\>", "#FF0000", "#FF0000"),
                new SyntaxHighlights("(?<!(def\\s))(?<=^|\\s|.)[a-zA-Z_][\\w_]*(?=\\()", "#3300FF", "#aa00FF"),
                new SyntaxHighlights("\\b(echo|if|case|while|else|switch|foreach|function|default|break|null|true|false)\\b", "#0077FF", "#0077FF"),
                new SyntaxHighlights("\\b([+-]?(?=\\.\\d|\\d)(?:\\d+)?(?:\\.?\\d*))(?:[eE]([+-]?\\d+))?\\b", "#ff00ff", "#ff00ff"),
                new SyntaxHighlights("(\\+|\\-|\\*|/|%|\\=|\\:|\\!|>|\\<|\\?|&|\\||\\~|\\^)", "#77FF77", "#77FF77"),
                new SyntaxHighlights("\\$\\w+", "#440044", "#FFBBFF"),
                new SyntaxHighlights("\"[^\\n]*?\"", "#ff5500", "#00FF00"),
                new SyntaxHighlights("\\'[^\\n]*?\\'", "#ff5500", "#00FF00"),
                new SyntaxHighlights("\"/[^\\n]*/i{0,1}\"", "#ff5500", "#00FF00"),
                new SyntaxHighlights("/\\*[^*]*\\*+(?:[^/*][^*]*\\*+)*/", "#888888", "#646464"),
                new SyntaxHighlights("\\/\\/.*", "#888888", "#646464"),
            };
        }
    }
    internal class QSharp : SyntaxHighlightLanguage
    {
        public QSharp()
        {
            this.Name = "QSharp";
            this.Author = "Finn Freitag";
            this.Filter = new string[1] { ".qs" };
            this.Description = "Syntax highlighting for QSharp language";
            this.AutoPairingPair = new AutoPairingPair[5]
            {
                new AutoPairingPair("{", "}"),
                new AutoPairingPair("[", "]"),
                new AutoPairingPair("(", ")"),
                new AutoPairingPair("\""),
                new AutoPairingPair("\'")
            };

            this.Highlights = new SyntaxHighlights[]
            {
                new SyntaxHighlights("\\W", "#BB0000", "#BB0000"),
                new SyntaxHighlights("\\/\\/.*", "#888888", "#646464"),
                new SyntaxHighlights("\\b(namespace|open|operation|using|let|H|M|Reset|return)\\b", "#0066bb", "#00ffff"),
                new SyntaxHighlights("\\b(Qubit|Result)\\b", "#00bb66", "#00ff00"),
            };
        }
    }
    internal class XML : SyntaxHighlightLanguage
    {
        public XML()
        {
            this.Name = "XML";
            this.Author = "Julius Kirsch";
            this.Filter = new string[2] { ".xml", ".xaml" };
            this.Description = "Syntax highlighting for Xml language";
            this.Highlights = new SyntaxHighlights[]
            {
                //numeric values (handles floating point and scientific notation)
                new SyntaxHighlights("[+-]?(?:\\d*\\.\\d+|\\d+)(?:[eE][+-]?\\d+)?", "#dd00dd", "#ff00ff"),
            
                //opening tags
                new SyntaxHighlights("<([a-zA-Z_:][\\w:.-]*)[^>]*>", "#969696", "#0099ff"),
            
                //Closing tags
                new SyntaxHighlights("</([a-zA-Z_:][\\w:.-]*)>", "#969696", "#0099ff"),
            
                //Self-closing tags
                new SyntaxHighlights("<([a-zA-Z_:][\\w:.-]*)[^>]*/>", "#969696", "#0099ff"),
            
                //Attributes
                new SyntaxHighlights("[a-zA-Z_:][\\w:.-]*=", "#00CA00", "#ff0000"),
            
                //Double-quoted attribute values
                new SyntaxHighlights("\"[^\n]*?\"", "#00CA00", "#00FF00"),
            
                //Single-quoted attribute values
                new SyntaxHighlights("'[^\n]*?'", "#00CA00", "#00FF00"),
            
                //Comments
                new SyntaxHighlights("<!--[\\s\\S]*?-->", "#888888", "#888888"),
            };
        }
    }
    internal class Python : SyntaxHighlightLanguage
    {
        public Python()
        {
            this.Name = "Python";
            this.Author = "Julius Kirsch";
            this.Filter = new string[1] { ".py" };
            this.Description = "Syntax highlighting for Python language";
            this.AutoPairingPair = new AutoPairingPair[6]
            {
                new AutoPairingPair("{", "}"),
                new AutoPairingPair("[", "]"),
                new AutoPairingPair("(", ")"),
                new AutoPairingPair("`", "`"),
                new AutoPairingPair("\""),
                new AutoPairingPair("\'")
            };

            this.Highlights = new SyntaxHighlights[]
            {
                new SyntaxHighlights("\\b([+-]?(?=\\.\\d|\\d)(?:\\d+)?(?:\\.?\\d*))(?:[eE]([+-]?\\d+))?\\b", "#dd00dd", "#ff00ff"),
                new SyntaxHighlights("\\b(and|as|assert|break|class|continue|def|del|elif|else|except|False|finally|for|from|global|if|import|in|is|lambda|None|nonlocal|not|or|pass|raise|return|True|try|while|with|yield)\\b", "#aa00cc", "#cc00ff"),
                new SyntaxHighlights("\\b(?<=def )\\w+(?=\\()|\\b\\w+(?=\\()", "#cc9900", "#ffbb00"),
                new SyntaxHighlights("\"[^\\n]*?\"", "#ff5500", "#00FF00"),
                new SyntaxHighlights("'[^\\n]*?'", "#00CA00", "#00FF00"),
                new SyntaxHighlights("\\#.*", "#888888", "#646464"),
                new SyntaxHighlights(@"\""\""\""[\s\S]*?\""\""\""", "#888888", "#646464"),
            };
        }
    }
    internal class CSV : SyntaxHighlightLanguage
    {
        public CSV()
        {
            this.Name = "CSV";
            this.Author = "Finn Freitag";
            this.Filter = new string[1] { ".csv" };
            this.Description = "Syntax highlighting for CSV language";
            this.Highlights = new SyntaxHighlights[]
            {
                new SyntaxHighlights("[\\:\\,\\;\\|]", "#1b9902", "#1b9902")
            };
        }
    }
    internal class CSVEnhanced : SyntaxHighlightLanguage
    {
        public CSVEnhanced()
        {
            this.Name = "CSV Enhanced";
            this.Author = "Julius Kirsch";
            this.Filter = new string[1] { ".csv" };
            this.Description = "Enhanced syntax highlighting for CSV with alternating row colors";
            
            this.HighlightRules = new IHighlightRule[]
            {
                new CsvColumnHighlightRule(),
                
                new RegexHighlightRule(
                    new SyntaxHighlights("[\\:\\,\\;\\|]", "#bd0020", "#f14260")
                )
            };
        }
    }
    internal class LaTex : SyntaxHighlightLanguage
    {
        public LaTex()
        {
            this.Name = "LaTex";
            this.Author = "Finn Freitag";
            this.Filter = new string[2] { ".latex", ".tex" };
            this.Description = "Syntax highlighting for LaTex language";
            this.Highlights = new SyntaxHighlights[]
            {
                new SyntaxHighlights("\\\\[a-z]+", "#0033aa", "#0088ff"),
                new SyntaxHighlights("%.*", "#888888", "#646464"),
                new SyntaxHighlights("[\\[\\]]", "#FFFF00", "#FFFF00"),
                new SyntaxHighlights("[\\{\\}]", "#FF0000", "#FF0000"),
                new SyntaxHighlights("\\$", "#00bb00", "#00FF00")
            };
        }
    }
    internal class Markdown : SyntaxHighlightLanguage
    {
        public Markdown()
        {
            this.Name = "Markdown";
            this.Author = "Julius Kirsch";
            this.Filter = new string[1] { ".md" };
            this.Description = "Syntax highlighting for Markdown language";
            this.Highlights = new SyntaxHighlights[]
            {
            // Headers (#, ##, ### etc.) - vibrant purple
            new SyntaxHighlights(@"(?m)^#{1,6} .*$", "#FF2E7E", "#FF2E7E", true),

            // Bold **text** or __text__ - bright pink
            new SyntaxHighlights(@"\*\*(.*?)\*\*", "#E18800", "#E18800", true),
            new SyntaxHighlights(@"__(.*?)__", "#E18800", "#E18800", true),

            // Italic *text* or _text_ - soft magenta
            new SyntaxHighlights(@"\*(.*?)\*", "#C61AFF", "#C61AFF", false, true),
            new SyntaxHighlights(@"_(.*?)_", "#C61AFF", "#C61AFF", false, true),

            // Bold + Italic ***text*** or ___text___ - vibrant magenta
            new SyntaxHighlights(@"\*\*\*(.*?)\*\*\*", "#C61AFF", "#C61AFF", true, true),
            new SyntaxHighlights(@"___(.*?)___", "#C61AFF", "#C61AFF", true, true),

            // Inline code `code` - soft yellow
            new SyntaxHighlights(@"`.*?`", "#F1C40F", "#F1C40F"),

            // Code blocks ```code``` - golden yellow
            new SyntaxHighlights(@"```[\s\S]*?```", "#F39C12", "#F39C12"),

            // Blockquotes > text - soft teal
            new SyntaxHighlights(@"(?m)^> .*", "#8DA284", "#8DA284"),

            // Lists - numbers or bullets - light green
            new SyntaxHighlights(@"(?m)^\d+\..*", "#2ECC71", "#2ECC71"),
            new SyntaxHighlights(@"(?m)^[-\+\*] .*", "#2ECC71", "#2ECC71"),

            // Links [text](url) - light sky blue
            new SyntaxHighlights(@"\[.*?\]\(.*?\)", "#3498DB", "#3498DB"),

            // Images ![alt](url) - pink-orange
            new SyntaxHighlights(@"!\[.*?\]\(.*?\)", "#FF6F61", "#FF6F61"),

            // Inline special characters - soft red
            new SyntaxHighlights(@"[~`_\^\*\+\-\!\|]", "#E74C3C", "#E74C3C")
            };
        }
    }
    internal class CSS : SyntaxHighlightLanguage
    {
        public CSS()
        {
            this.Name = "CSS";
            this.Author = "Julius Kirsch";
            this.Filter = new string[2] { ".css", ".scss" };
            this.Description = "Syntax highlighting for CSS language";
            this.AutoPairingPair = new AutoPairingPair[5]
            {
                new AutoPairingPair("{", "}"),
                new AutoPairingPair("[", "]"),
                new AutoPairingPair("(", ")"),
                new AutoPairingPair("\""),
                new AutoPairingPair("\'")
            };
            this.Highlights = new SyntaxHighlights[]
            {
                new SyntaxHighlights("[a-zA-Z-]+.*;", "#ff5500", "#00ffff"),//properties
                new SyntaxHighlights("\\b([a-zA-Z_-][a-zA-Z0-9_-]*)(?=\\()", "#bb00bb", "#00ff99"),//functions
                new SyntaxHighlights(":[a-z].*(?={)", "#0033ff", "#fffd00"),//pseudo classes/elements
                new SyntaxHighlights("(.|#|^).*\\s*{", "#227700", "#44ff00"),//classname
                new SyntaxHighlights("(?<=\\d)(?:px|%|em|rem|in|cm|mm|pt|pc|ex|ch|vw|vh|vmin|vmax|ms|s)", "#cc0000", "#ff0000"),//units
                new SyntaxHighlights("\\b-?\\d+(?:\\.\\d+)?", "#0000ff", "#cc00ff"),//numbers
                new SyntaxHighlights("@([^ ]+)", "#8800ff", "#ff0000"),//first word after the @
                new SyntaxHighlights("#[0-9A-Fa-f]{1,8}\\b", "#00bb55", "#cc00ff"),//hexadecimal
                new SyntaxHighlights("(\".+?\"|\'.+?\')", "#00aaff", "#ff8800"),//strings
                new SyntaxHighlights("(;|:|{|}|,)", "#777777", "#bbbbbb"),//special characters
                new SyntaxHighlights("\\/\\*(.|\\n)*?\\*\\/", "#555555", "#888888"),//comments
            };
        }
    }
    internal class SQL : SyntaxHighlightLanguage
    {
        public SQL()
        {
            this.Name = "SQL";
            this.Author = "Finn Freitag";
            this.Filter = new string[1] { ".sql" };
            this.Description = "Syntax highlightung for SQL";
            this.Highlights = new SyntaxHighlights[]
            {
                new SyntaxHighlights("\\b(ADD|ADD CONSTRAINT|ALL|ALTER|ALTER COLUMN|ALTER TABLE|AND|ANY|AS|ASC|BACKUP DATABASE|BETWEEN|CASE|CHECK|COLUMN|CONSTRAINT|CREATE|CREATE DATABASE|CREATE INDEX|CREATE OR REPLACE VIEW|CREATE TABLE|CREATE PROCEDURE|CREATE UNIQUE INDEX|CREATE VIEW|DATABASE|DEFAULT|DELETE|DESC|DISTINCT|DROP|DROP COLUMN|DROP CONSTRAINT|DROP DATABASE|DROP DEFAULT|DROP INDEX|DROP TABLE|DROP VIEW|EXEC|EXISTS|FOREIGN KEY|FROM|FULL OUTER JOIN|GROUP BY|HAVING|IN|INDEX|INNER JOIN|INSERT INTO|INSERT INTO SELECT|IS NULL|IS NOT NULL|JOIN|LEFT JOIN|LIKE|LIMIT|NOT|NOT NULL|OR|ORDER BY|OUTER JOIN|PRIMARY KEY|PROCEDURE|RIGHT JOIN|ROWNUM|SELECT|SELECT DISTINCT|SELECT INTO|SELCET TOP|SET|TABLE|TOP|TRUNCATE TABLE|UNION|UNION ALL|UNIQUE|UPDATE|USE|VALUES|VIEW|WHERE)\\b","#FF6A00","#FF6A00",true,true),
                new SyntaxHighlights("\"[^\\n]*?\"","#42C22B","#42C22B"),
                new SyntaxHighlights("'[^\\n]*?'","#42C22B","#42C22B"),
                new SyntaxHighlights("\\b([+-]?(?=\\.\\d|\\d)(?:\\d+)?(?:\\.?\\d*))(?:[eE]([+-]?\\d+))?\\b", "#2B3BFF","#2B3BFF"),
                new SyntaxHighlights("\\b(MIN|MAX|COUNT|SUM|AVG)\\b","#11C9DB","#11C9DB",false,true),
                new SyntaxHighlights("\\.","#901F9E","#901F9E",true)
            };
        }
    }
    internal class Lua : SyntaxHighlightLanguage
    {
        public Lua()
        {
            this.Name = "Lua";
            this.Author = "Julius Kirsch";
            this.Filter = new string[1] { ".lua" };
            this.Description = "Syntax highlighting for Lua language";
            this.AutoPairingPair = new AutoPairingPair[6]
            {
            new AutoPairingPair("{", "}"),
            new AutoPairingPair("[", "]"),
            new AutoPairingPair("(", ")"),
            new AutoPairingPair("`", "`"),
            new AutoPairingPair("\""),
            new AutoPairingPair("\'")
            };

            this.Highlights = new SyntaxHighlights[]
            {
            // Numbers (decimal, hexadecimal, with/without decimal point)
            new SyntaxHighlights("\\b(0x[\\da-fA-F]+|\\d+\\.?\\d*|\\.\\d+)([eE][+-]?\\d+)?\\b", "#dd00dd", "#ff00ff"),

            // Keywords
            new SyntaxHighlights("\\b(and|break|do|else|elseif|end|false|for|function|goto|if|in|local|nil|not|or|repeat|return|then|true|until|while)\\b", "#aa00cc", "#cc00ff"),

            // Function names (assuming they are followed by `(` and may be prefixed like `mod.func`)
            new SyntaxHighlights("\\b\\w+(?=\\s*\\()", "#cc9900", "#ffbb00"),

            // Strings (single-line)
            new SyntaxHighlights("\"([^\"\\\\]|\\\\.)*\"", "#ff5500", "#00FF00"),
            new SyntaxHighlights("'([^'\\\\]|\\\\.)*'", "#00CA00", "#00FF00"),

            // Comments (single-line starting with `--`)
            new SyntaxHighlights("--.*", "#888888", "#646464"),

            // Multi-line comments (between --[[ and ]])
            new SyntaxHighlights("--\\[\\[(.|\\r|\\n)*?\\]\\]", "#888888", "#646464")
            };
        }
    }
}
