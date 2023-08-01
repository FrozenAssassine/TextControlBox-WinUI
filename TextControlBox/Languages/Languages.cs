namespace TextControlBoxNS.Languages
{
    internal class Batch : CodeLanguage
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
    internal class ConfigFile : CodeLanguage
    {
        public ConfigFile()
        {
            this.Name = "ConfigFile";
            this.Author = "Finn Freitag";
            this.Filter = new string[1] { ".ini" };
            this.Description = "Syntax highlighting for configuration files";
            this.AutoPairingPair = new AutoPairingPair[]
            {
                new AutoPairingPair("[", "]")
            };
            this.Highlights = new SyntaxHighlights[]
            {
                new SyntaxHighlights("[\\[\\]]", "#0000FF", "#0000FF"),
                new SyntaxHighlights("\\[[\\t\\s]*(\\w+)[\\t\\s]*\\]", "#9900FF", "#9900FF"),
                new SyntaxHighlights("(\\w+)\\=", "#DDDD00", "#DDDD00"),
                new SyntaxHighlights("\\=(.+)", "#EE0000", "#EE0000"),
                new SyntaxHighlights(";.*", "#888888", "#888888"),
            };
        }
    }
    internal class Cpp : CodeLanguage
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
                new SyntaxHighlights("\\b([+-]?(?=\\.\\d|\\d)(?:\\d+)?(?:\\.?\\d*))(?:[eE]([+-]?\\d+))?\\b", "#dd00dd", "#00ff00"),
                new SyntaxHighlights("(?<!(def\\s))(?<=^|\\s|.)[a-zA-Z_][\\w_]*(?=\\()", "#4455ff", "#ffbb00"),
                new SyntaxHighlights("\\b(string|uint16_t|uint8_t|alignas|alignof|and|and_eq|asm|auto|bitand|bitor|bool|break|case|catch|char|char8_t|char16_t|char32_t|class|compl|concept|const|const_cast|consteval|constexpr|constinit|continue|co_await|co_return|co_yield|decltype|default|delete|do|double|dynamic_cast|else|enum|explicit|export|extern|false|float|for|friend|goto|if|inline|int|long|mutable|namespace|new|noexcept|not|not_eq|nullptr|operator|or|or_eq|private|protected|public|register|reinterpret_cast|requires|return|short|signed|sizeof|static|static_assert|static_cast|struct|switch|template|this|thread_local|throw|true|try|typedef|typeid|typename|union|unsigned|using|virtual|void|volatile|wchar_t|while|xor|xor_eq)\\b", "#dd00dd", "#dd00dd"),
                new SyntaxHighlights("\\B#(define|elif|else|endif|error|ifndef|ifdef|if|import|include|line|pragma|region|undef|using)", "#bbbbbb", "#999999"),
                new SyntaxHighlights("(\\\".+?\\\"|\\'.+?\\')", "#ffff00", "#ff000f"),
                new SyntaxHighlights("\"[^\\n]*?\"", "#00CA00", "#00FF00"),
                new SyntaxHighlights("'[^\\n]*?'", "#00CA00", "#00FF00"),
                new SyntaxHighlights("/\\*[^*]*\\*+(?:[^/*][^*]*\\*+)*/", "#888888", "#646464"),
                new SyntaxHighlights("\\/\\/.*", "#888888", "#646464"),
            };
        }
    }
    internal class CSharp : CodeLanguage
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
                new SyntaxHighlights("\\b([+-]?(?=\\.\\d|\\d)(?:\\d+)?(?:\\.?\\d*))(?:[eE]([+-]?\\d+))?\\b", "#ff00ff", "#ff00ff"),
                new SyntaxHighlights("(?<!(def\\s))(?<=^|\\s|.)[a-zA-Z_][\\w_]*(?=\\()", "#880088", "#ffbb00"),
                new SyntaxHighlights("\\b(abstract|as|base|bool|break|byte|case|catch|char|checked|class|const|continue|decimal|default|delegate|do|double|else|enum|event|explicit|extern|external|false|final|finally|fixed|float|for|foreach|get|goto|if|implicit|in|int|interface|internal|is|lock|long|namespace|new|null|object|operator|out|override|params|partial|private|protected|public|readonly|ref|return|sbyte|sealed|set|short|sizeof|stackalloc|static|string|struct|switch|this|throw|true|try|typeof|uint|ulong|unchecked|unsafe|ushort|using|value|var|virtual|void|volatile|while)\\b", "#0066bb", "#00ffff"),
                new SyntaxHighlights("\\b(List|Color|Console|Debug|Dictionary|Stack|Queue|GC)\\b", "#008000", "#ff9900"),
                new SyntaxHighlights("\\b(try|catch|finally)\\b", "#9922ff", "#6666ff"),
                new SyntaxHighlights("(#region)+(.*?)($|\n)", "#ff0000", "#ff0000", true),
                new SyntaxHighlights("#endregion", "#ff0000", "#ff0000", true),
                new SyntaxHighlights("\"[^\\n]*?\"", "#ff5500", "#00FF00"),
                new SyntaxHighlights("'[^\\n]*?'", "#00CA00", "#00FF00"),
                new SyntaxHighlights("\\/\\/.*", "#888888", "#646464"),
                new SyntaxHighlights("/\\*[^*]*\\*+(?:[^/*][^*]*\\*+)*/", "#888888", "#646464"),
            };
        }
    }
    internal class GCode : CodeLanguage
    {
        public GCode()
        {
            this.Name = "GCode";
            this.Author = "Julius Kirsch";
            this.Filter = new string[5] { ".ngc", ".tap", ".gcode", ".nc", ".cnc" };
            this.Description = "Syntax highlighting for GCode language";
            this.Highlights = new SyntaxHighlights[]
            {
                new SyntaxHighlights("\\bY(?=([0-9]|(\\+|\\-)[0-9]))", "#00ff00", "#00ff00"),
                new SyntaxHighlights("\\bX(?=([0-9]|(\\+|\\-)[0-9]))", "#ff0000", "#ff0000"),
                new SyntaxHighlights("\\bZ(?=([0-9]|(\\+|\\-)[0-9]))", "#0077ff", "#0077ff"),
                new SyntaxHighlights("\\bA(?=([0-9]|(\\+|\\-)[0-9]))", "#ff00ff", "#ff00ff"),
                new SyntaxHighlights("\\b(E|F)(?=([0-9]|(\\+|\\-)[0-9]))", "#ffAA00", "#ffAA00"),
                new SyntaxHighlights("\\b(S|T)(?=([0-9]|(\\+|\\-)[0-9]))", "#ffff00", "#ffff00"),
                new SyntaxHighlights("([+-]?(?=\\.\\d|\\d)(?:\\d+)?(?:\\.?\\d*))(?:[eE]([+-]?\\d+))?", "#ff00ff", "#9f009f"),
                new SyntaxHighlights("[G|M][0-999].*?[\\s|\\n]", "#00aaaa", "#00ffff"),
                new SyntaxHighlights("(;|\\/\\/|\\brem\\b).*", "#888888", "#888888"),
            };
        }
    }
    internal class HexFile : CodeLanguage
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
    internal class Html : CodeLanguage
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
    internal class Java : CodeLanguage
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
    internal class Javascript : CodeLanguage
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
    internal class Json : CodeLanguage
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
    internal class PHP : CodeLanguage
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
    internal class QSharp : CodeLanguage
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
    internal class XML : CodeLanguage
    {
        public XML()
        {
            this.Name = "XML";
            this.Author = "Julius Kirsch";
            this.Filter = new string[2] { ".xml", ".xaml" };
            this.Description = "Syntax highlighting for Xml language";
            this.Highlights = new SyntaxHighlights[]
            {
                new SyntaxHighlights("\\b([+-]?(?=\\.\\d|\\d)(?:\\d+)?(?:\\.?\\d*))(?:[eE]([+-]?\\d+))?\\b", "#dd00dd", "#ff00ff"),
                new SyntaxHighlights("<([^ >!\\/]+)[^>]*>", "#969696", "#0099ff"),
                new SyntaxHighlights("<+[/]+[a-zA-Z0-9:?\\-_]+>", "#969696", "#0099ff"),
                new SyntaxHighlights("<[a-zA-Z0-9:?\\-]+?.*\\/>", "#969696", "#0099ff"),
                new SyntaxHighlights("[-A-Za-z_]+\\=", "#00CA00", "#Ff0000"),
                new SyntaxHighlights("\"[^\\n]*?\"", "#00CA00", "#00FF00"),
                new SyntaxHighlights("'[^\\n]*?'", "#00CA00", "#00FF00"),
                new SyntaxHighlights("<!--[\\s\\S]*?-->", "#888888", "#888888"),
            };
        }
    }
    internal class Python : CodeLanguage
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
                new SyntaxHighlights("(?<!(def\\s))(?<=^|\\s|.)[a-zA-Z_][\\w_]*(?=\\()", "#cc9900", "#ffbb00"),
                new SyntaxHighlights("\"[^\\n]*?\"", "#ff5500", "#00FF00"),
                new SyntaxHighlights("'[^\\n]*?'", "#00CA00", "#00FF00"),
                new SyntaxHighlights("\\#.*", "#888888", "#646464"),
                new SyntaxHighlights("\\\"\"\"(.|[\\r\\n])*\\\"\"\"", "#888888", "#646464"),
            };
        }
    }
    internal class CSV : CodeLanguage
    {
        public CSV()
        {
            this.Name = "CSV";
            this.Author = "Finn Freitag";
            this.Filter = new string[1] { ".csv" };
            this.Description = "Syntax highlighting for CSV language";
            this.Highlights = new SyntaxHighlights[]
            {
                new SyntaxHighlights("[\\:\\,\\;]", "#1b9902", "#1b9902")
            };
        }
    }
    internal class LaTex : CodeLanguage
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
    internal class TOML : CodeLanguage
    {
        public TOML()
        {
            this.Name = "TOML";
            this.Author = "Finn Freitag";
            this.Filter = new string[1] { ".toml" };
            this.Description = "Syntax highlighting for TOML language";
            this.AutoPairingPair = new AutoPairingPair[]
            {
                new AutoPairingPair("[", "]"),
                new AutoPairingPair("\""),
            };
            this.Highlights = new SyntaxHighlights[]
            {
                new SyntaxHighlights("\\[.*\\]", "#0000FF", "#0000FF"),
                new SyntaxHighlights("\\[[\\t\\s]*(\\w+)[\\t\\s]*\\]", "#9900FF", "#9900FF"),
                new SyntaxHighlights("(\\w+)[\\s\\t]*\\=", "#DDDD00", "#DDDD00"),
                new SyntaxHighlights("\\=\\s+(.+)", "#EE0000", "#EE0000"),
                new SyntaxHighlights("[\\[\\]]", "#FFFF00", "#FFFF00"),
                new SyntaxHighlights("\\b(true|false)\\b", "#00bb66", "#00ff00"),
                new SyntaxHighlights("[\"'][^\\n]*?[\"']", "#D69D84", "#D69D84"),
                new SyntaxHighlights("#.*", "#888888", "#888888")
            };
        }
    }
    internal class Markdown : CodeLanguage
    {
        public Markdown()
        {
            this.Name = "Markdown";
            this.Author = "Finn Freitag";
            this.Filter = new string[1] { ".md" };
            this.Description = "Syntax highlighting for Markdown language";
            this.Highlights = new SyntaxHighlights[]
            {
                new SyntaxHighlights("[>~\"'`\\-\\+|\\^\\!_]", "#FF0000", "#FF0000"),
                new SyntaxHighlights("#", "#0000FF", "#0000FF"),
                new SyntaxHighlights("\\*.*\\*", "#000000", "#FFFFFF", false, true),
                new SyntaxHighlights("_.*_", "#000000", "#FFFFFF", false, true),
                new SyntaxHighlights("\\*\\*.*\\*\\*", "#000000", "#FFFFFF", true),
                new SyntaxHighlights("__.*__", "#000000", "#FFFFFF", true),
                new SyntaxHighlights("\\d+\\.", "#00FF00", "#00FF00"),
                new SyntaxHighlights("[\\[\\]\\(\\)]", "#FFFF00", "#FFFF00")
            };
        }
    }
    internal class CSS : CodeLanguage
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
}
