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

## ❤️ Support my work  
<a href='https://ko-fi.com/K3K819KSLG' target='_blank'>  
    <img height='36' style='border:0px;height:36px;' src='https://storage.ko-fi.com/cdn/kofi6.png?v=6' border='0' alt='Buy Me a Coffee at ko-fi.com' />
</a>

## 🏗️ Getting Started

### Installation
### 📥 [Download Nuget.org](https://www.nuget.org/packages/TextControlBox.WinUI.JuliusKirsch/1.1.0-alpha)

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

## 🚀 Events

TextControlBox provides several events to handle user interactions:

- `TextChanged`: Triggered when the text content changes.
- `SelectionChanged`: Fires when the user changes the selected text.
- `ZoomChanged`: Called when the zoom factor is adjusted.
- `GotFocus` / `LostFocus`: Handle focus changes.

## 🎨 Syntax Highlighting

TextControlBox includes built-in support for multiple languages:

- C#, C++, Java, Python, JavaScript, JSON, HTML, CSS, SQL, Markdown, Batch, Config, CSV, LaTex, PHP, QSharp, TOML, XML

Enable syntax highlighting:

```csharp
textBox.SelectSyntaxHighlightingById(SyntaxHighlightID.CSharp);
```

## 🔎 Search & Replace

Start a search:

```csharp
textBox.BeginSearch("example", wholeWord: false, matchCase: true);
```

Replace occurrences:

```csharp
textBox.ReplaceAll("oldText", "newText", matchCase: true, wholeWord: false);
```

## 👨‍👩‍👧‍👦 Contributing

Contributions are welcome! Feel free to submit a pull request or open an issue.

## 🧾 License

This project is licensed under the MIT License.
