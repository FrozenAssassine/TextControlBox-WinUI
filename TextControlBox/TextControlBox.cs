using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System.Collections.Generic;
using TextControlBoxNS.Core;
using Windows.Foundation;

namespace TextControlBoxNS;

/// <summary>
/// 
/// </summary>
public partial class TextControlBox : UserControl
{
    private CoreTextControlBox coreTextBox = new CoreTextControlBox();

    /// <summary>
    /// Initializes a new instance of the TextControlBox class.
    /// </summary>
    public TextControlBox()
    {
        coreTextBox.eventsManager.ZoomChanged += ZoomManager_ZoomChanged;
        coreTextBox.eventsManager.TextChanged += EventsManager_TextChanged;
        coreTextBox.eventsManager.SelectionChanged += EventsManager_SelectionChanged;
        coreTextBox.eventsManager.GotFocus += EventsManager_GotFocus;
        coreTextBox.eventsManager.LostFocus += EventsManager_LostFocus;

        this.Content = coreTextBox;
    }

    private void EventsManager_LostFocus()
    {
        LostFocus?.Invoke(this);
    }
    private void EventsManager_GotFocus()
    {
        GotFocus?.Invoke(this);
    }
    private void EventsManager_SelectionChanged(SelectionChangedEventHandler args)
    {
        SelectionChanged?.Invoke(this, args);
    }
    private void EventsManager_TextChanged()
    {
        TextChanged?.Invoke(this);
    }
    private void ZoomManager_ZoomChanged(int zoomFactor)
    {
        ZoomChanged?.Invoke(this, zoomFactor);
    }

    /// <summary>
    /// Selects the entire line specified by its index.
    /// </summary>
    /// <param name="line">The index of the line to select.</param>
    public void SelectLine(int line)
    {
        coreTextBox.SelectLine(line);
    }

    /// <summary>
    /// Moves the cursor to the beginning of the specified line by its index.
    /// </summary>
    /// <param name="line">The index of the line to navigate to.</param>
    public void GoToLine(int line)
    {
        coreTextBox.GoToLine(line);
    }

    /// <summary>
    /// Loads the specified text into the textbox, resetting all text and undo history.
    /// </summary>
    /// <param name="text">The text to load into the textbox.</param>
    public void LoadText(string text)
    {
        coreTextBox.LoadText(text);
    }

    /// <summary>
    /// Sets the text content of the textbox, recording an undo action.
    /// </summary>
    /// <param name="text">The new text content to set in the textbox.</param>
    public void SetText(string text)
    {
        coreTextBox.SetText(text);
    }

    /// <summary>
    /// Loads the specified lines into the textbox, resetting all content and undo history.
    /// </summary>
    /// <param name="lines">An enumerable containing the lines to load into the textbox.</param>
    /// <param name="lineEnding">The line ending format used in the loaded lines (default is CRLF).</param>
    public void LoadLines(IEnumerable<string> lines, LineEnding lineEnding = LineEnding.CRLF)
    {
        coreTextBox.LoadLines(lines, lineEnding);
    }

    /// <summary>
    /// Pastes the contents of the clipboard at the current cursor position.
    /// </summary>
    public void Paste()
    {
        coreTextBox.Paste();
    }

    /// <summary>
    /// Copies the currently selected text to the clipboard.
    /// </summary>
    public void Copy()
    {
        coreTextBox.Copy();
    }

    /// <summary>
    /// Cuts the currently selected text and copies it to the clipboard.
    /// </summary>
    public void Cut()
    {
        coreTextBox.Cut();
    }

    /// <summary>
    /// Gets the entire text content of the textbox.
    /// </summary>
    /// <returns>The complete text content of the textbox as a string.</returns>

    public string GetText()
    {
        return coreTextBox.GetText();
    }

    /// <summary>
    /// Sets the text selection in the textbox starting from the specified index and with the given length.
    /// </summary>
    /// <param name="start">The index of the first character of the selection.</param>
    /// <param name="length">The length of the selection in number of characters.</param>
    public void SetSelection(int start, int length)
    {
        coreTextBox.SetSelection(start, length);
    }

    /// <summary>
    /// Selects all the text in the textbox.
    /// </summary>
    public void SelectAll()
    {
        coreTextBox.SelectAll();
    }

    /// <summary>
    /// Clears the current text selection in the textbox.
    /// </summary>
    public void ClearSelection()
    {
        coreTextBox.ClearSelection();
    }

    /// <summary>
    /// Undoes the last action in the textbox.
    /// </summary>
    public void Undo()
    {
        coreTextBox.Undo();
    }

    /// <summary>
    /// Redoes the last undone action in the textbox.
    /// </summary>
    public void Redo()
    {
        coreTextBox.Redo();
    }

    /// <summary>
    /// Scrolls the specified line to the center of the textbox if it is out of the rendered region.
    /// </summary>
    /// <param name="line">The index of the line to center.</param>
    public void ScrollLineToCenter(int line)
    {
        coreTextBox.ScrollLineToCenter(line);
    }

    /// <summary>
    /// Scrolls the text one line up.
    /// </summary>
    public void ScrollOneLineUp()
    {
        coreTextBox.ScrollOneLineUp();

    }

    /// <summary>
    /// Scrolls the text one line down.
    /// </summary>
    public void ScrollOneLineDown()
    {
        coreTextBox.ScrollOneLineDown();
    }

    /// <summary>
    /// Forces the specified line to be scrolled into view, centering it vertically within the textbox.
    /// </summary>
    /// <param name="line">The index of the line to center.</param>
    public void ScrollLineIntoView(int line)
    {
        coreTextBox.ScrollLineIntoView(line);
    }

    /// <summary>
    /// Scrolls the first line of the visible text into view.
    /// </summary>
    public void ScrollTopIntoView()
    {
        coreTextBox.ScrollTopIntoView();
    }

    /// <summary>
    /// Scrolls the last visible line of the visible text into view.
    /// </summary>
    public void ScrollBottomIntoView()
    {
        coreTextBox.ScrollBottomIntoView();
    }

    /// <summary>
    /// Scrolls one page up, simulating the behavior of the page up key.
    /// </summary>
    public void ScrollPageUp()
    {
        coreTextBox.ScrollPageUp();
    }

    /// <summary>
    /// Scrolls one page down, simulating the behavior of the page down key.
    /// </summary>
    public void ScrollPageDown()
    {
        coreTextBox.ScrollPageDown();
    }

    /// <summary>
    /// Scrolls the textbox horizontally to the cursor position
    /// </summary>
    public void ScrollIntoViewHorizontally()
    {
        coreTextBox.ScrollIntoViewHorizontally();
    }

    /// <summary>
    /// Gets the content of the line specified by the index
    /// </summary>
    /// <param name="line">The index to get the content from</param>
    /// <returns>The text from the line specified by the index</returns>
    public string GetLineText(int line)
    {
        return coreTextBox.GetLineText(line);
    }

    /// <summary>
    /// Gets the text of multiple lines, starting from the specified line index.
    /// </summary>
    /// <param name="startLine">The index of the line to start with.</param>
    /// <param name="length">The number of lines to retrieve.</param>
    /// <returns>The concatenated text from the specified lines.</returns>
    public string GetLinesText(int startLine, int length)
    {
        return coreTextBox.GetLinesText(startLine, length);
    }

    /// <summary>
    /// Sets the content of the line specified by the index. The first line has the index 0.
    /// </summary>
    /// <param name="line">The index of the line to change the content.</param>
    /// <param name="text">The text to set for the specified line.</param>
    /// <returns>Returns true if the text was changed successfully, and false if the index was out of range.</returns>
    public bool SetLineText(int line, string text)
    {
        return coreTextBox.SetLineText(line, text);
    }

    /// <summary>
    /// Deletes the line from the textbox
    /// </summary>
    /// <param name="line">The line to delete</param>
    /// <returns>Returns true if the line was deleted successfully and false if not</returns>
    public bool DeleteLine(int line)
    {
        return coreTextBox.DeleteLine(line);
    }

    /// <summary>
    /// Adds a new line with the text specified
    /// </summary>
    /// <param name="line">The position to insert the line to</param>
    /// <param name="text">The text to put in the new line</param>
    /// <returns>Returns true if the line was added successfully and false if not</returns>
    public bool AddLine(int line, string text)
    {
        return coreTextBox.AddLine(line, text);
    }

    /// <summary>
    /// Surrounds the selection with the text specified by the text
    /// </summary>
    /// <param name="text">The text to surround the selection with</param>
    public void SurroundSelectionWith(string text)
    {
        coreTextBox.SurroundSelectionWith(text);
    }

    /// <summary>
    /// Surround the selection with individual text for the left and right side.
    /// </summary>
    /// <param name="text1">The text for the left side</param>
    /// <param name="text2">The text for the right side</param>
    public void SurroundSelectionWith(string text1, string text2)
    {
        coreTextBox.SurroundSelectionWith(text1, text2);

    }

    /// <summary>
    /// Duplicates the line specified by the index into the next line
    /// </summary>
    /// <param name="line">The index of the line to duplicate</param>
    public void DuplicateLine(int line)
    {
        coreTextBox.DuplicateLine(line);
    }
    /// <summary>
    /// Duplicates the line at the current cursor position
    /// </summary>
    public void DuplicateLine()
    {
        coreTextBox.DuplicateLine();
    }

    /// <summary>
    /// Replaces all occurences in the text with another word
    /// </summary>
    /// <param name="word">The word to search for</param>
    /// <param name="replaceWord">The word to replace with</param>
    /// <param name="matchCase">Search with case sensitivity</param>
    /// <param name="wholeWord">Search for whole words</param>
    /// <returns>Found when everything was replaced and not found when nothing was replaced</returns>
    public SearchResult ReplaceAll(string word, string replaceWord, bool matchCase, bool wholeWord)
    {
        return coreTextBox.ReplaceAll(word, replaceWord, matchCase, wholeWord);
    }

    /// <summary>
    /// Searches for the next occurence. Call this after BeginSearch
    /// </summary>
    /// <returns>SearchResult.Found if the word was found</returns>
    public SearchResult FindNext()
    {
        return coreTextBox.FindNext();
    }

    /// <summary>
    /// Searches for the previous occurence. Call this after BeginSearch
    /// </summary>
    /// <returns>SearchResult.Found if the word was found</returns>
    public SearchResult FindPrevious()
    {
        return coreTextBox.FindPrevious();
    }

    /// <summary>
    /// Begins a search for the specified word in the textbox content.
    /// </summary>
    /// <param name="word">The word to search for in the textbox.</param>
    /// <param name="wholeWord">A flag indicating whether to perform a whole-word search.</param>
    /// <param name="matchCase">A flag indicating whether the search should be case-sensitive.</param>
    /// <returns>A SearchResult enum representing the result of the search.</returns>
    public SearchResult BeginSearch(string word, bool wholeWord, bool matchCase)
    {
        return coreTextBox.BeginSearch(word, wholeWord, matchCase);
    }

    /// <summary>
    /// Ends the search and removes the highlights
    /// </summary>
    public void EndSearch()
    {
        coreTextBox.EndSearch();
    }

    /// <summary>
    /// Unloads the textbox and releases all resources.
    /// Do not use the textbox afterwards.
    /// </summary>
    public void Unload()
    {
        coreTextBox.Unload();
    }

    /// <summary>
    /// Clears the undo and redo history of the textbox.
    /// </summary>
    /// <remarks>
    /// The ClearUndoRedoHistory method removes all the stored undo and redo actions, effectively resetting the history of the textbox.
    /// </remarks>
    public void ClearUndoRedoHistory()
    {
        coreTextBox.ClearUndoRedoHistory();
    }

    /// <summary>
    /// Gets the current cursor position in the textbox.
    /// </summary>
    /// <returns>The current cursor position represented by a Point object (X, Y).</returns>
    public Point GetCursorPosition()
    {
        return coreTextBox.GetCursorPosition();
    }

    /// <summary>
    /// Gets the current cursor position in the textbox.
    /// </summary>
    /// <returns>The current cursor position represented by a Point object (X, Y).</returns>
    public void SetCursorPosition(int lineNumber, int characterPos, bool scrollIntoView = true) 
    {
        coreTextBox.SetCursorPosition(lineNumber, characterPos, scrollIntoView);
    }

    /// <summary>
    /// Selects the CodeLanguage based on the specified identifier.
    /// </summary>
    /// <param name="languageId">The identifier of the syntaxhighlighting to select.</param>
    public void SelectSyntaxHighlightingById(SyntaxHighlightID languageId)
    {
        coreTextBox.SelectSyntaxHighlightingById(languageId);
    }

    /// <summary>
    /// Gets or sets a value indicating whether syntax highlighting is enabled in the textbox.
    /// </summary>
    public bool EnableSyntaxHighlighting { get => coreTextBox.EnableSyntaxHighlighting; set => coreTextBox.EnableSyntaxHighlighting = value; } 

    /// <summary>
    /// Gets or sets the style to use for the syntaxhighlighting and auto pairing.
    /// </summary>
    public SyntaxHighlightLanguage SyntaxHighlighting
    {
        get => coreTextBox.SyntaxHighlighting;
        set => coreTextBox.SyntaxHighlighting = value;
    }

    /// <summary>
    /// Gets or sets the line ending style used in the textbox.
    /// </summary>
    /// <remarks>
    /// The LineEnding property represents the line ending style for the text.
    /// Possible values are LineEnding.CRLF (Carriage Return + Line Feed), LineEnding.LF (Line Feed), or LineEnding.CR (Carriage Return).
    /// </remarks>
    public LineEnding LineEnding
    {
        get => coreTextBox.LineEnding;
        set => coreTextBox.LineEnding = value;
    }

    /// <summary>
    /// Gets or sets the space between the line number and the text in the textbox.
    /// </summary>
    public float SpaceBetweenLineNumberAndText { get => coreTextBox.SpaceBetweenLineNumberAndText; set => coreTextBox.SpaceBetweenLineNumberAndText = value; }

    /// <summary>
    /// Gets or sets the current cursor position in the textbox.
    /// </summary>
    /// <remarks>
    /// The cursor position is represented by a <see cref="CursorPosition"/> object, which includes the character position within the text and the line number.
    /// </remarks>
    public CursorPosition CursorPosition
    {
        get => coreTextBox.CursorPosition;
        set => coreTextBox.CursorPosition = value;
    }

    /// <summary>
    /// Gets or sets the font family used for displaying text in the textbox.
    /// </summary>
    public new FontFamily FontFamily
    {
        get => coreTextBox.FontFamily;
        set => coreTextBox.FontFamily = value;
    }
    /// <summary>
    /// Gets or sets the font size used for displaying text in the textbox.
    /// </summary>

    public new int FontSize
    {
        get => coreTextBox.FontSize;
        set => coreTextBox.FontSize = value;
    }
    /// <summary>
    /// Gets the actual rendered size of the font in pixels.
    /// </summary>
    public float RenderedFontSize => coreTextBox.RenderedFontSize;

    /// <summary>
    /// Gets or sets the text displayed in the textbox.
    /// </summary>
    public string Text
    {
        get => coreTextBox.Text;
        set => coreTextBox.Text = value;
    }
    /// <summary>
    /// Gets or sets the requested theme for the textbox.
    /// </summary>
    public new ElementTheme RequestedTheme
    {
        get => coreTextBox.RequestedTheme;
        set => coreTextBox.RequestedTheme = value;
    }

    /// <summary>
    /// Gets or sets the custom design for the textbox.
    /// </summary>
    /// <remarks>
    /// Settings this null will use the default design
    /// </remarks>
    public TextControlBoxDesign Design
    {
        get => coreTextBox.Design;
        set => coreTextBox.Design = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether line numbers should be displayed in the textbox.
    /// </summary>
    public bool ShowLineNumbers
    {
        get => coreTextBox.ShowLineNumbers;
        set => coreTextBox.ShowLineNumbers = value;
    }
    /// <summary>
    /// Gets or sets a value indicating whether the line highlighter should be shown in the custom textbox.
    /// </summary>
    public bool ShowLineHighlighter
    {
        get => coreTextBox.ShowLineHighlighter;
        set => coreTextBox.ShowLineHighlighter = value;
    }

    /// <summary>
    /// Gets or sets the zoom factor in percent for the text.
    /// </summary>
    public int ZoomFactor
    {
        get => coreTextBox.ZoomFactor;
        set => coreTextBox.ZoomFactor = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the textbox is in readonly mode.
    /// </summary>
    public bool IsReadonly
    {
        get => coreTextBox.IsReadonly;
        set => coreTextBox.IsReadonly = value;
    }
    /// <summary>
    /// Gets or sets the size of the cursor in the textbox.
    /// </summary>
    public CursorSize CursorSize
    {
        get => coreTextBox.CursorSize;
        set => coreTextBox.CursorSize = value;
    }
    /// <summary>
    /// Gets or sets the context menu flyout associated with the textbox.
    /// </summary>
    /// <remarks>
    /// Setting the value to null will show the default flyout.
    /// </remarks>
    public new MenuFlyout ContextFlyout
    {
        get => coreTextBox.ContextFlyout;
        set => coreTextBox.ContextFlyout = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether the context flyout is disabled for the textbox.
    /// </summary>
    public bool ContextFlyoutDisabled
    {
        get => coreTextBox.ContextFlyoutDisabled;
        set => coreTextBox.ContextFlyoutDisabled = value;
    }
    /// <summary>
    /// Gets or sets the starting index of the selected text in the textbox.
    /// </summary>
    public int SelectionStart
    {
        get => coreTextBox.SelectionStart;
        set => coreTextBox.SelectionStart = value;
    }

    /// <summary>
    /// Gets or sets the length of the selected text in the textbox.
    /// </summary>
    public int SelectionLength
    {
        get => coreTextBox.SelectionLength;
        set => coreTextBox.SelectionLength = value;
    }

    /// <summary>
    /// Gets or sets the text that is currently selected in the textbox.
    /// </summary>
    public string SelectedText
    {
        get => coreTextBox.SelectedText;
        set => coreTextBox.SelectedText = value;
    }

    /// <summary>
    /// Gets the number of lines in the textbox.
    /// </summary>
    public int NumberOfLines => coreTextBox.NumberOfLines;

    /// <summary>
    /// Gets the index of the current line where the cursor is positioned in the textbox.
    /// </summary>
    public int CurrentLineIndex => coreTextBox.CurrentLineIndex;

    /// <summary>
    /// Gets or sets the position of the scrollbars in the textbox.
    /// </summary>
    public ScrollBarPosition ScrollBarPosition
    {
        get => coreTextBox.ScrollBarPosition;
        set => coreTextBox.ScrollBarPosition = value;
    }
    /// <summary>
    /// Gets the total number of characters in the textbox.
    /// </summary>
    public int CharacterCount => coreTextBox.CharacterCount;

    /// <summary>
    /// Gets or sets the sensitivity of vertical scrolling in the textbox.
    /// </summary>

    public double VerticalScrollSensitivity
    {
        get => coreTextBox.VerticalScrollSensitivity;
        set => coreTextBox.VerticalScrollSensitivity = value;
    }

    /// <summary>
    /// Gets or sets the sensitivity of horizontal scrolling in the textbox.
    /// </summary>
    public double HorizontalScrollSensitivity
    {
        get => coreTextBox.HorizontalScrollSensitivity;
        set => coreTextBox.HorizontalScrollSensitivity = value;
    }
    /// <summary>
    /// Gets or sets the vertical scroll position in the textbox.
    /// </summary>
    public double VerticalScroll
    {
        get => coreTextBox.VerticalScroll;
        set => coreTextBox.VerticalScroll = value;
    }
    /// <summary>
    /// Gets or sets the horizontal scroll position in the textbox.
    /// </summary>
    public double HorizontalScroll
    {
        get => coreTextBox.HorizontalScroll;
        set => coreTextBox.HorizontalScroll = value;
    }

    /// <summary>
    /// Gets or sets the corner radius for the textbox.
    /// </summary>
    public new CornerRadius CornerRadius
    {
        get => coreTextBox.CornerRadius;
        set => coreTextBox.CornerRadius = value;
    }
    /// <summary>
    /// Gets or sets a value indicating whether to use spaces instead of tabs for indentation in the textbox.
    /// </summary>

    public bool UseSpacesInsteadTabs
    {
        get => coreTextBox.UseSpacesInsteadTabs;
        set => coreTextBox.UseSpacesInsteadTabs = value;
    }
    /// <summary>
    /// Gets or sets the number of spaces used for a single tab in the textbox.
    /// </summary>
    public int NumberOfSpacesForTab
    {
        get => coreTextBox.NumberOfSpacesForTab;
        set => coreTextBox.NumberOfSpacesForTab = value;
    }
    /// <summary>
    /// Gets whether the search is currently active
    /// </summary>
    public bool SearchIsOpen => coreTextBox.SearchIsOpen;

    /// <summary>
    /// Gets an enumerable collection of all the lines in the textbox.
    /// </summary>
    /// <remarks>
    /// Use this property to access all the lines of text in the textbox. You can use this collection to save the lines to a file using functions like FileIO.WriteLinesAsync.
    /// Utilizing this property for saving will significantly improve RAM usage during the saving process.
    /// </remarks>
    public IEnumerable<string> Lines => coreTextBox.Lines;

    /// <summary>
    /// Gets or sets a value indicating whether auto-pairing is enabled.
    /// </summary>
    /// <remarks>
    /// Auto-pairing automatically pairs opening and closing symbols, such as brackets or quotation marks.
    /// </remarks>
    public bool DoAutoPairing
    {
        get => coreTextBox.DoAutoPairing;
        set => coreTextBox.DoAutoPairing = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether pressing ctrl + w selects a word or does nothing.
    /// </summary>
    /// <remarks>
    /// </remarks>
    public bool ControlW_SelectWord
    {
        get => coreTextBox.ControlW_SelectWord;
        set => coreTextBox.ControlW_SelectWord = value;
    }


    /// <summary>
    /// Represents a delegate used for handling the text changed event in the TextControlBox.
    /// </summary>
    /// <param name="sender">The instance of the TextControlBox that raised the event.</param>
    public delegate void TextChangedEvent(TextControlBox sender);
    /// <summary>
    /// Occurs when the text is changed in the TextControlBox.
    /// </summary>
    public event TextChangedEvent TextChanged;

    /// <summary>
    /// Represents a delegate used for handling the selection changed event in the TextControlBox.
    /// </summary>
    /// <param name="sender">The instance of the TextControlBox that raised the event.</param>
    /// <param name="args">The event arguments providing information about the selection change.</param>
    public delegate void SelectionChangedEvent(TextControlBox sender, SelectionChangedEventHandler args);

    /// <summary>
    /// Occurs when the selection is changed in the TextControlBox.
    /// </summary>
    public event SelectionChangedEvent SelectionChanged;

    /// <summary>
    /// Represents a delegate used for handling the zoom changed event in the TextControlBox.
    /// </summary>
    /// <param name="sender">The instance of the TextControlBox that raised the event.</param>
    /// <param name="zoomFactor">The new zoom factor value indicating the scale of the content.</param>
    public delegate void ZoomChangedEvent(TextControlBox sender, int zoomFactor);

    /// <summary>
    /// Occurs when the zoom factor is changed in the TextControlBox.
    /// </summary>
    public event ZoomChangedEvent ZoomChanged;

    /// <summary>
    /// Represents a delegate used for handling the got focus event in the TextControlBox.
    /// </summary>
    /// <param name="sender">The instance of the TextControlBox that received focus.</param>
    public delegate void GotFocusEvent(TextControlBox sender);

    /// <summary>
    /// Occurs when the TextControlBox receives focus.
    /// </summary>
    public new event GotFocusEvent GotFocus;

    /// <summary>
    /// Represents a delegate used for handling the lost focus event in the TextControlBox.
    /// </summary>
    /// <param name="sender">The instance of the TextControlBox that lost focus.</param>
    public delegate void LostFocusEvent(TextControlBox sender);

    /// <summary>
    /// Occurs when the TextControlBox loses focus.
    /// </summary>
    public new event LostFocusEvent LostFocus;

    //static functions
    /// <summary>
    /// Gets a dictionary containing the syntaxhighlightings indexed by their respective identifiers.
    /// </summary>
    /// <remarks>
    /// The syntaxhighlightings dictionary provides a collection of predefined syntaxhighlighting objects, where each object is associated with a unique identifier (SyntaxHighlightID).
    /// The dictionary is case-insensitive, and it allows quick access to the syntaxhighlighting objects based on their identifier.
    /// </remarks>
    public static Dictionary<SyntaxHighlightID, SyntaxHighlightLanguage> SyntaxHighlightings => CoreTextControlBox.SyntaxHighlightings;

    /// <summary>
    /// Retrieves a syntaxhighlighting object based on the specified identifier.
    /// </summary>
    /// <param name="languageId">The identifier of the syntaxhighlighting to retrieve.</param>
    /// <returns>The syntaxhighlighting object corresponding to the provided identifier, or null if not found.</returns>
    public static SyntaxHighlightLanguage GetSyntaxHighlightingFromID(SyntaxHighlightID languageId)
    {
        return CoreTextControlBox.GetSyntaxHighlightingFromID(languageId);
    }

    /// <summary>
    /// Retrieves a syntaxhighlighting object from a JSON representation.
    /// </summary>
    /// <param name="json">The JSON string representing the syntaxhighlighting object.</param>
    /// <returns>The deserialized syntaxhighlighting object obtained from the provided JSON, or null if the JSON is invalid or does not represent a valid syntaxhighlighting.</returns>
    public static JsonLoadResult GetSyntaxHighlightingFromJson(string json)
    {
        return CoreTextControlBox.GetSyntaxHighlightingFromJson(json);
    }
}