using System.Collections.Generic;
using TextControlBoxNS;
using TextControlBoxNS.Languages;

namespace TextControlBox.Tests.SyntaxHighlighting;

/// <summary>A realistic snippet for every built-in language, used to build a full screen or a very long line of code.</summary>
internal static class LanguageSamples
{
    public static readonly (SyntaxHighlightID Id, SyntaxHighlightLanguage Language, string Sample)[] All =
    [
        (SyntaxHighlightID.x86Assembly, new x86Assembly(), """
            _start:
                mov eax, [ebx+4]    ; load value
                add eax, 0x10
                rep movsb
                int 0x80
            """),
        (SyntaxHighlightID.Batch, new Batch(), """
            @echo off
            :start
            set NAME="World" & echo Hello %NAME%
            if exist *.tmp del *.tmp
            rem cleanup done
            goto start
            """),
        (SyntaxHighlightID.Cpp, new Cpp(), """
            #include <vector>
            int main(int argc, char* argv[]) {
                std::vector<int> values = { 1, 2, 0x1F, 3.5f };
                printf("count: %d\n", (int)values.size()); // print
                /* block comment */
                return 0;
            }
            """),
        (SyntaxHighlightID.CSharp, new CSharp(), """
            public async Task<int> LoadAsync(string path, int retries = 3)
            {
                var list = new List<string> { "a", "b\"c" }; // comment
                if (retries > 0 && path != null) { Debug.WriteLine($"{path}: {retries * 2.5f}"); }
                /* block */ char c = 'x'; return list.Count + 0xFF;
            }
            """),
        (SyntaxHighlightID.Klipper, new KlipperHighlighter(), """
            [stepper_x]
            step_pin: PF13
            endstop_pin: ^PG6
            rotation_distance: 40
            [gcode_macro START_PRINT]
            gcode:
                G28
                M104 S{params.TEMP|default(200)} # heat
            """),
        (SyntaxHighlightID.Inifile, new IniHighlighter(), """
            [General]
            name = "TextControlBox"
            count = 42
            enabled = true
            ; comment = 1
            path = C:\Program Files\app
            """),
        (SyntaxHighlightID.CSS, new CSS(), """
            .button:hover, #main > a { color: #ff00aa; margin: 10px 2em; }
            @media screen and (max-width: 600px) {
              body { font-family: "Segoe UI", sans-serif; width: calc(100% - 20px); }
            }
            /* comment */
            """),
        (SyntaxHighlightID.CSV, new CSV(), """
            id,name,price,description
            1,Apple,0.5,"red, sweet"
            2,Banana;0.25;yellow|long
            """),
        (SyntaxHighlightID.CSVImproved, new CSVEnhanced(), """
            id,name,price,description
            1,Apple,0.5,"red, sweet"
            2,Banana,0.25,yellow
            """),
        (SyntaxHighlightID.GCode, new GCode(), """
            G28 ; home
            G1 X10.5 Y-20 Z0.3 F3000 E1.2
            M104 S200 T0
            G0 A-5.0
            """),
        (SyntaxHighlightID.Gitignore, new GitIgnore(), """
            # build output
            bin/
            obj/
            *.log
            !keep.log
            file[0-9].txt
            \#notacomment
            """),
        (SyntaxHighlightID.HexFile, new HexFile(), """
            :10010000214601360121470136007EFE09D2190140
            :100110002146017E17C20001FF5F16002148011928
            :00000001FF
            """),
        (SyntaxHighlightID.Html, new Html(), """
            <!DOCTYPE html>
            <div class="box" style="width: 100px;" data-id='7'>
              <p>Hello <b>World</b> 2024</p><br/>
              <!-- comment -->
            </div>
            """),
        (SyntaxHighlightID.Java, new Java(), """
            public class Main {
                public static void main(String[] args) {
                    int count = 10; // comment
                    System.out.println("Count: " + count + 'c');
                    /* block */ return;
                }
            }
            """),
        (SyntaxHighlightID.Javascript, new Javascript(), """
            function greet(name) {
              const re = /ab+c/i; let total = 1.5 + 2;
              document.getElementById("app").textContent = 'Hi ' + name;
              try { return total; } catch (e) { /* ignore */ } // done
            }
            """),
        (SyntaxHighlightID.Json, new Json(), """
            {"name": "TextControlBox", "version": 1.7, "tags": ["editor", "winui"],
             "enabled": true, "nested": {"value": null, "count": -42, "ratio": 1e-3}},
            """),
        (SyntaxHighlightID.Latex, new LaTex(), """
            \documentclass{article}
            \begin{document}
            \section[short]{Intro} % comment
            $x^2 + y^2 = z^2$
            \end{document}
            """),
        (SyntaxHighlightID.Log, new LogHighlighter(), """
            2024-01-15 10:23:45.123 [main] INFO  Server started on 192.168.1.10:8080 in 1500ms
            2024-01-15 10:23:46.001 [worker-1] ERROR Request failed id=550e8400-e29b-41d4-a716-446655440000 path=/var/log/app.log
            "GET /api/users HTTP/1.1" 404 512 "WARN" retry=0x1F
               at App.Service.Run() in C:\src\Service.cs:line 42
            """),
        (SyntaxHighlightID.Lua, new Lua(), """
            local function add(a, b)
              -- add two numbers
              local s = "sum: " .. (a + b)
              print(s, 0x1F, 3.5)
              return a + b
            end
            """),
        (SyntaxHighlightID.Markdown, new Markdown(), """
            # Title
            Some **bold** and *italic* text with `code` and a [link](https://example.com).
            - item one
            1. first
            > quote
            ![image](img.png)
            """),
        (SyntaxHighlightID.PHP, new PHP(), """
            <?php
            function greet($name) {
                $count = 5 * 2; // comment
                echo "Hello " . $name;
                if ($count > 3) { return true; }
            }
            ?>
            """),
        (SyntaxHighlightID.Python, new Python(), """"
            def greet(name, count=3):
                """Docstring"""
                for i in range(count):  # loop
                    print("Hello", name, 'x', i * 2.5)
                return None
            """"),
        (SyntaxHighlightID.QSharp, new QSharp(), """
            namespace Quantum {
                open Microsoft.Quantum.Intrinsic;
                operation Measure(q : Qubit) : Result {
                    H(q); // superposition
                    return M(q);
                }
            }
            """),
        (SyntaxHighlightID.SQL, new SQL(), """
            SELECT u.name, COUNT(o.id) FROM users u
            INNER JOIN orders o ON o.user_id = u.id
            WHERE u.name LIKE 'A%' AND o.total > 10.5
            GROUP BY u.name ORDER BY u.name;
            """),
        (SyntaxHighlightID.TOML, new TomlHighlighter(), """
            [server]
            host = "localhost"
            port = 8080
            debug = false
            created = 1979-05-27T07:32:00Z
            point = { x = 1, y = 2 }
            [[products]]
            name = "Hammer" # comment
            """),
        (SyntaxHighlightID.XML, new XML(), """
            <?xml version="1.0" encoding="utf-8"?>
            <root xmlns:x="urn:test">
              <item id="1" name='first'>Text 42</item>
              <empty/>
              <!-- comment -->
            </root>
            """),
    ];

    /// <summary>Repeats the sample until it fills a typical editor viewport (about 60 lines).</summary>
    public static string FullScreen(string sample, int lines = 60)
    {
        var sampleLines = sample.Replace("\r\n", "\n").Split('\n');
        var result = new List<string>(lines);
        for (int i = 0; i < lines; i++)
            result.Add(sampleLines[i % sampleLines.Length]);
        return string.Join("\n", result);
    }

    /// <summary>The sample joined to one line of the given length, like a minified file.</summary>
    public static string LongLine(string sample, int length)
    {
        var line = sample.Replace("\r\n", " ").Replace("\n", " ");
        var builder = new System.Text.StringBuilder(length + line.Length);
        while (builder.Length < length)
            builder.Append(line).Append(' ');
        return builder.ToString(0, length);
    }
}
