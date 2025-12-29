<div align="center">
<img src="images/Icon1.png" height="150px" width="auto">
<h1>TextControlBox-WinUI</h1>
</div>

<div align="center">
<img src="https://img.shields.io/github/issues/FrozenAssassine/TextControlBox-WinUI.svg?style=flat">
<img src="https://img.shields.io/github/stars/FrozenAssassine/TextControlBox-WinUI.svg?style=flat">
<img src="https://img.shields.io/github/repo-size/FrozenAssassine/TextControlBox-WinUI?style=flat">

</div>

<br/>

## 🤔 What is TextControlBox?
TextControlBox is a powerful and highly customizable textbox control for WinUI 3 applications. It provides an advanced text editing experience with features like syntax highlighting for multiple programming languages, intuitive search and replace functionality, zooming, line numbering, and smooth scrolling. With support for undo/redo, customizable themes, and efficient performance.

## 📦 Download
NuGet Package:
[TextControlBox.WinUI.JuliusKirsch](https://www.nuget.org/packages/TextControlBox.WinUI.JuliusKirsch/1.1.0-alpha)

## ❤️ Support my work
If you find TextControlBox useful and want to support its development:

[![Donate](https://www.paypalobjects.com/en_US/i/btn/btn_donate_LG.gif)](https://www.paypal.com/donate/?hosted_button_id=PPME7KF7KF7QS)


## 🛠️ Features
- **Basic Text Editing**: Cut, copy, paste, undo, redo, and full text selection capabilities.
- **Navigation & Selection**:
  - Go to a specific line.
  - Select specific lines or the entire text.
- **Scrolling & Zooming**:
  - Scroll to specific lines or pages.
  - Zoom in and out for better readability.
- **Syntax Highlighting**:
  - Built-in support for multiple programming languages.
  - Easily toggle syntax highlighting on and off.
- **Search & Replace**:
  - Multi highlight search
  - Find and replace text with options for case sensitivity and whole-word matching.
- **Customization Options**:
  - Show or hide line numbers.
  - Customize font, theme, and cursor appearance.
  - Configure spaces vs. tabs for indentation.
- **Other Features**:
  - Drag & drop text support.
  - Surround selected text with custom characters.
  - Get cursor position and manage selections programmatically.

## Limits & Missing features
- No textwrapping (I have no idea how to implement this properly atm.)
- 200 million lines of text (20 characters per line) and it started to consume around 20GB of ram, scrolling worked, but random freezes appeared (too much overhead).
- Syntaxhighlighting for multi line comments or similar only works, if both start and end characters are in the visible view. (Due to performance I only do such actions on the visible rendered text)


## ‼️Common Pitfalls & Performance Improvements

| Scenario | ❌Common Pitfall | ✅Recommended Approach | Notes |
|----------|----------------|--------------------|-------|
| **Getting/Setting Text** | `textbox.Text = someText` | Use `LoadText(string)` or `LoadLines(IEnumerable<string>)` if you do not want to record an undo step | `Text` and `SetText` always record an undo step. Load methods bypass undo history by design. |
| **Character Count** | `textbox.Text.Length` | `textbox.CharacterCount()` | Uses spans internally; avoids temporary string allocations. |
| **Accessing Lines** | `textbox.Text.Split("\n")` | `textbox.Lines` or `textbox.NumberOfLines` | Avoids creating temporary string arrays. `Lines` exposes the internal list efficiently. |
| **Word Count** | Manual splitting or regex on `Text` | `textbox.WordCount()` | Uses spans and char iteration for allocation-free counting. |
| **Saving Text to File** | `File.WriteAllText("file.txt", textbox.GetText())` | `File.WriteAllLines("file.txt", textbox.Lines)` | Writes lines directly; avoids allocating a single large string on the heap. |
| **Text Search** | Manually iterating or substring search | Use `BeginSearch(word, wholeWord, matchCase)` + `FindNext()` / `FindPrevious()` + `EndSearch()` | Provides fast, internal search with highlights and efficient indexing. |
| **Iterating Lines** | `foreach(var line in textbox.Text.Split('\n'))` | `foreach(var line in textbox.Lines)` | Iterates without extra string allocations. |
| **Replacing Text** | Using `GetText()` and manually replacing strings | Use `ReplaceNext(replaceWord)` or `ReplaceAll(word, replaceWord)` | Efficient, allocation-free replacements with undo handling options. |

**Tips:**  
- `Text` and `SetText` are convenient but always record an undo step; use `LoadText` / `LoadLines` to avoid this.  
- Prefer `CharacterCount()`, `WordCount()`, `Lines`, and `NumberOfLines` for performance-sensitive operations.  
- Use `BeginSearch` / `FindNext` / `FindPrevious` for searching instead of manual string operations.  
- Save large text efficiently with `File.WriteAllLines(textbox.Lines)` rather than `GetText()`.
## 🏗️ Getting Started
Add TextControlBox to your WinUI 3 project and include the necessary namespace in your XAML or C# file.

```csharp
using TextControlBoxNS;
```

### Basic Usage

```csharp
TextControlBox textBox = new TextControlBox();
textBox.LoadText("Hello, world!");
textBox.ShowLineNumbers = true;
textBox.EnableSyntaxHighlighting = true;
```

<details>

<summary><h2>🛠️ All properties and functions</h2></summary>

- `Text`: Gets or sets the full content of the editor.
- `SelectedText`: Gets the currently selected text.
- `CurrentSelection`, `CurrentSelectionOrdered`: Provides selection metadata.
- `HasSelection`: Indicates if any text is currently selected.
- `CursorPosition`: Gets the current cursor position.
- `CurrentLineIndex`: Index of the line containing the cursor.
- `NumberOfLines`: Total number of lines in the editor.
- `FontFamily`: Gets or sets the font family.
- `FontSize`: Gets or sets the base font size.
- `RenderedFontSize`: Gets the final rendered font size.
- `CornerRadius`: Gets or sets the control's corner radius.
- `RequestedTheme`: Gets or sets the UI theme.
- `Design`: Gets or sets visual settings like line numbers and highlighting.
- `ShowLineNumbers`: Toggles line number visibility.
- `ShowLineHighlighter`: Toggles current line highlighting.
- `EnableSyntaxHighlighting`: Enables or disables syntax highlighting.
- `SyntaxHighlighting`: Gets or sets the current syntax highlighting language.
- `LineEnding`: Gets or sets the line ending mode.
- `SpaceBetweenLineNumberAndText`: Spacing between line numbers and text.
- `ZoomFactor`: Gets or sets the zoom factor.
- `VerticalScroll`, `HorizontalScroll`: Gets or sets scroll offsets.
- `VerticalScrollSensitivity`, `HorizontalScrollSensitivity`: Adjust scroll speed.
- `UseSpacesInsteadTabs`: Enables space-based tabulation.
- `NumberOfSpacesForTab`: Sets number of spaces per tab.
- `SearchIsOpen`: Indicates whether the search panel is open.
- `Lines`: Gets all lines as strings.
- `DoAutoPairing`: Enables auto-pairing of brackets/quotes.
- `ControlW_SelectWord`: Enables Ctrl+W to select the current word.
- `CanDragDropText`: Enables drag-and-drop support.
- `ContextFlyout`: Gets or sets the context menu flyout.
- `ContextFlyoutDisabled`: Disables the context menu.
- `IsReadonly`: Determines if the control is read-only.
- `CursorSize`: Gets or sets the cursor size.
- `UndoRedoEnabled`: Enables/disables undo/redo functionality
- `IsGroupingActions`: Gets whether grouping of undo redo is enabled or disabled
- `BeginActionGroup`: Starts the grouping of undo redo items
- `EndActionGroup`: Ends the grouping of undo redo items
- `SelectionScrollStartBorderDistance`: Gets or sets the mouse distance from the text box edge that triggers auto-scroll during selection
- `ShowWhitespaceCharacters`: Gets or sets whether spaces and tabs are visually shown in the text box (e.g. as dots or arrows)
- `HighlightLinks`: Gets or sets, whether links inside the text are highlighted and clickable
- `Focus(FocusState state)`: Sets focus to the control.
- `SelectLine(int line)`: Selects a specific line.
- `SelectLines(int start, int count)`: Selects a range of lines.
- `GoToLine(int line)`: Moves the cursor to a specific line.
- `LoadText(string text)`: Loads text into the control.
- `SetText(string text)`: Sets the text content.
- `LoadLines(IEnumerable<string> lines, LineEnding lineEnding = LineEnding.CRLF)`: Loads multiple lines.
- `Paste()`: Pastes clipboard content.
- `Copy()`: Copies the selection to clipboard.
- `Cut()`: Cuts the selected text.
- `GetText()`: Returns the current content.
- `SetSelection(int start, int length)`: Selects a specific text range.
- `SelectAll()`: Selects all text.
- `ClearSelection()`: Clears any text selection.
- `Undo()`: Undoes the last action.
- `Redo()`: Redoes the last undone action.
- `ScrollLineToCenter(int line)`: Scrolls a line to center.
- `ScrollOneLineUp()`, `ScrollOneLineDown()`: Scrolls one line vertically.
- `ScrollLineIntoView(int line)`: Brings a line into view.
- `ScrollTopIntoView()`, `ScrollBottomIntoView()`: Scrolls to top/bottom.
- `ScrollPageUp()`, `ScrollPageDown()`: Scrolls a full page.
- `ScrollIntoViewHorizontally()`: Scrolls horizontally into view.
- `GetLineText(int line)`: Returns the text of a line.
- `GetLinesText(int startLine, int length)`: Returns a range of lines.
- `SetLineText(int line, string text)`: Replaces the text of a line.
- `DeleteLine(int line)`: Deletes a specific line.
- `AddLine(int line, string text)`: Adds a new line at a position.
- `AddLines(int start, string[] lines)`: Adds the array of lines at the given position.
- `SurroundSelectionWith(string text)`: Surrounds selection with a string.
- `SurroundSelectionWith(string text1, string text2)`: Surrounds with prefix/suffix.
- `DuplicateLine(int line)`: Duplicates the specified line.
- `DuplicateCurrentLine()`: Duplicates the current line.
- `ReplaceAll(string word, string replaceWord, bool matchCase, bool wholeWord)`: Replaces all matches.
- `ReplaceNext(string replaceWord)`: Replaces the next match.
- `FindNext()`, `FindPrevious()`: Navigates search results.
- `BeginSearch(string word, bool wholeWord, bool matchCase)`: Starts a search.
- `EndSearch()`: Ends the search.
- `Unload()`: Unloads the control.
- `ClearUndoRedoHistory()`: Clears undo/redo history.
- `GetCursorPosition()`: Returns the cursor's screen position.
- `SetCursorPosition(int lineNumber, int characterPos, bool scrollIntoView = true, bool autoClamp = true)`: Sets the cursor position.
- `SelectSyntaxHighlightingById(SyntaxHighlightID languageId)`: Sets highlighting by ID.
- `CalculateSelectionPosition()`: Gets position info for selection.
- `CharacterCount()`: Returns character count.
- `WordCount()`: Returns word count.
- `TextChanged`: Event triggered when text changes.
- `SelectionChanged`: Event triggered on selection change.
- `ZoomChanged`: Event triggered when zoom changes.
- `GotFocus`, `LostFocus`, `Loaded`: Focus and lifecycle events.
- `SyntaxHighlightings`: Static dictionary of syntax languages.
- `GetSyntaxHighlightingFromID(SyntaxHighlightID languageId)`: Gets highlighting language.
- `GetSyntaxHighlightingFromJson(string json)`: Loads highlighting from JSON.
- `RewriteTabsSpaces(int spaces, bool useSpacesInsteadTabs)`: Changes the current indentation of the text to a new indentation.
- `DetectTabsSpaces()`: Detects and returns the current indentation of the text as tuple
</details>

## 🚀 Events

TextControlBox provides several events to handle user interactions:

- `TextChanged`: Triggered when the text content changes.
- `SelectionChanged`: Fires when the user changes the selected text.
- `ZoomChanged`: Called when the zoom factor is adjusted.
- `GotFocus` / `LostFocus`: Handle focus changes.
- `TextLoadedEvent`: Occurs when the TextControlBox finished loading the text.
- `Loaded`: Occurs when the TextControlBox finished loading and all components initialized
- `TabsSpacesChanged`: Occurs when the tabs or spaces change, from LoadLines/LoadText or from properties
- `LineEndingChanged`: Occurs when the line ending changes, from LoadLines/LoadText or from property
- `LinkClicked`: Occures when a link inside the textbox was clicked (contains the url as parameter)

## 🎨 Syntax Highlighting

TextControlBox includes built-in support for multiple languages:

- C#, C++, Java, Python, JavaScript, JSON, x86Assembly, HTML, CSS, SQL, Markdown, Batch, Config (Ini, Klipper -Styles), CSV, LaTex, PHP, QSharp, TOML, XML, G-Code, Lua, Hex

Enable syntax highlighting:

```csharp
textBox.SelectSyntaxHighlightingById(SyntaxHighlightID.CSharp);
```

## 👨‍👩‍👧‍👦 Contributing

Contributions are welcome! Feel free to submit a pull request or open an issue.

## 🧾 License

This project is licensed under the MIT License.
