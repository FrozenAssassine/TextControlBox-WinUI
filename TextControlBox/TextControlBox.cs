using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using TextControlBoxNS.Core;
using TextControlBoxNS.Models;
using Windows.Foundation;

namespace TextControlBoxNS;

/// <summary>
/// A custom textbox control with a lot of features
/// </summary>
public partial class TextControlBox : UserControl
{
    private readonly CoreTextControlBox coreTextBox;

    /// <summary>
    /// Initializes a new instance of the TextControlBox class.
    /// </summary>
    public TextControlBox()
    {
        base.Loaded += TextControlBox_Loaded;

        coreTextBox = new CoreTextControlBox();
        coreTextBox.eventsManager.Loaded += EventsManager_Loaded;
        coreTextBox.eventsManager.ZoomChanged += ZoomManager_ZoomChanged;
        coreTextBox.eventsManager.TextChanged += EventsManager_TextChanged;
        coreTextBox.eventsManager.SelectionChanged += EventsManager_SelectionChanged;
        coreTextBox.eventsManager.GotFocus += EventsManager_GotFocus;
        coreTextBox.eventsManager.LostFocus += EventsManager_LostFocus;
        coreTextBox.eventsManager.TextLoaded += EventsManager_TextLoaded;
        coreTextBox.eventsManager.LinkClicked += EventsManager_LinkClicked;
        coreTextBox.eventsManager.TabsSpacesChanged += EventsManager_TabsSpacesChanged;
        coreTextBox.eventsManager.LineEndingChanged += EventsManager_LineEndingChanged;
        this.Content = coreTextBox;

        this.RequestedTheme = ElementTheme.Default;
    }

    private void EventsManager_LineEndingChanged(LineEnding lineEnding)
    {
        this.LineEndingChanged?.Invoke(this, lineEnding);
    }

    private void EventsManager_TabsSpacesChanged(bool spacesInsteadTabs, int spaces)
    {
        TabsSpacesChanged?.Invoke(this, spacesInsteadTabs, spaces);
    }

    private void TextControlBox_Loaded(object sender, RoutedEventArgs e)
    {
        if (coreTextBox.IsLoaded)
        {
            this.Focus(FocusState.Programmatic);
            return;
        }

        coreTextBox.InitialiseOnStart();
    }
    private void EventsManager_LinkClicked(string url)
    {
        LinkClicked?.Invoke(this, url);
    }
    private void EventsManager_Loaded()
    {
        Loaded?.Invoke(this);
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
    private void EventsManager_TextLoaded()
    {
        TextLoaded?.Invoke(this);
    }

    /// <summary>
    /// Sets the focus to the textbox
    /// </summary>
    /// <param name="state"></param>
    public new void Focus(FocusState state)
    {
        coreTextBox.Focus(state);
    }

    /// <summary>
    /// Selects the entire line specified by its index.
    /// </summary>
    /// <param name="line">The index of the line to select.</param>
    /// <returns>Whether the line at the specified index could be selected</returns>
    public bool SelectLine(int line)
    {
        return coreTextBox.SelectLine(line);
    }

    /// <summary>
    /// Selects the entire line specified by its index.
    /// </summary>
    /// <param name="start">The zero based index of the first line to select</param>
    /// <param name="count">The number of lines to select</param>
    /// <returns>Whether the lines specified could be selected</returns>
    public bool SelectLines(int start, int count)
    {
        return coreTextBox.SelectLines(start, count);
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
    /// <param name="autodetectTabsSpaces" >Whether to autodetect tabs and spaces settings from the text</param>
    public void LoadText(string text, bool autodetectTabsSpaces = true)
    {
        coreTextBox.LoadText(text, autodetectTabsSpaces);
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
    /// <param name="autodetectTabsSpaces" >Whether to autodetect tabs and spaces settings from the lines</param>
    public void LoadLines(IEnumerable<string> lines, bool autodetectTabsSpaces = true, LineEnding lineEnding = LineEnding.CRLF)
    {
        coreTextBox.LoadLines(lines, autodetectTabsSpaces , lineEnding);
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
    /// Do not call this to count the characters, lines, words or anything else. Use the functions provided.
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
    /// Gets the content of the line specified by the zero based index
    /// </summary>
    /// <param name="line">The zero based index of the line</param>
    /// <returns>The text from the line at the specified index</returns>
    public string GetLineText(int line)
    {
        return coreTextBox.GetLineText(line);
    }

    /// <summary>
    /// Gets the text of multiple lines, starting from the specified line index.
    /// </summary>
    /// <param name="startLine">The zero based index of the line to start with.</param>
    /// <param name="length">The number of lines to retrieve.</param>
    /// <returns>The concatenated text from the specified lines.</returns>
    public string GetLinesText(int startLine, int length)
    {
        return coreTextBox.GetLinesText(startLine, length);
    }

    /// <summary>
    /// Sets the content of the line specified by the index. The first line has the index 0.
    /// Setting the text also records an undo step
    /// </summary>
    /// <param name="line">The zero based index of the line to change the content.</param>
    /// <param name="text">The text to set for the specified line.</param>
    /// <returns>Returns true if the text was changed successfully, and false if the index was out of range.</returns>
    public bool SetLineText(int line, string text)
    {
        return coreTextBox.SetLineText(line, text);
    }

    /// <summary>
    /// Deletes the line specified by the zero based index from the textbox
    /// This action will record an undo step
    /// </summary>
    /// <param name="line">The line to delete</param>
    /// <returns>Returns true if the line was deleted successfully and false if not</returns>
    public bool DeleteLine(int line)
    {
        return coreTextBox.DeleteLine(line);
    }

    /// <summary>
    /// Adds a new line with the text specified
    /// This action will record an undo step
    /// </summary>
    /// <param name="line">The zero based position to insert the line to</param>
    /// <param name="text">The text to put into the new line</param>
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
    /// Duplicates the line specified by the zero based index into the next line
    /// This action records an undo step
    /// </summary>
    /// <param name="line">The zero based index of the line to duplicate</param>
    public void DuplicateLine(int line)
    {
        coreTextBox.DuplicateLine(line);
    }
    /// <summary>
    /// Duplicates the line at the current cursor position
    /// </summary>
    public void DuplicateCurrentLine()
    {
        coreTextBox.DuplicateCurrentLine();
    }

    /// <summary>
    /// Replaces all occurences of the specified word with the replae word
    /// </summary>
    /// <param name="word">The word to search for</param>
    /// <param name="replaceWord">The word to replace with</param>
    /// <param name="matchCase">Search with case sensitivity</param>
    /// <param name="wholeWord">Search for whole words</param>
    /// <param name="ignoreIsReadonly">Ignores the isReadonly property of the textbox.</param>
    /// <returns>Found if everything was replaced and not found if nothing was replaced</returns>
    public SearchResult ReplaceAll(string word, string replaceWord, bool matchCase, bool wholeWord, bool ignoreIsReadonly = false)
    {
        return coreTextBox.ReplaceAll(word, replaceWord, matchCase, wholeWord, ignoreIsReadonly);
    }

    /// <summary>
    /// Replaces the next occurnce in the text with the replaceWord
    /// </summary>
    /// <param name="replaceWord">The string to replace the searched string with</param>
    /// <param name="ignoreIsReadonly">Ignores the isReadonly property of the textbox.</param>
    /// <returns>Found if the next occurence got replaced and not found if nothing got replaced</returns>
    public SearchResult ReplaceNext(string replaceWord, bool ignoreIsReadonly = false)
    {
        return coreTextBox.ReplaceNext(replaceWord, ignoreIsReadonly);
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
    /// Set the position of the cursor. 
    /// If autoclamp is set to false and invalid values are provided it throws IndexOutOfRangeException.
    /// </summary>
    /// <param name="lineNumber">Line number to move the cursor to</param>
    /// <param name="characterPos">Character position to move the cursor to</param>
    /// <param name="scrollIntoView">Scroll the cursor into view</param>
    /// <param name="autoClamp">Automatically clamp invalid values to the correct bounds</param>
    public void SetCursorPosition(int lineNumber, int characterPos, bool scrollIntoView = true, bool autoClamp = true)
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
    /// Calculates the index and length of the currently selected text.
    /// Important: resource heavy for very long text!
    /// </summary>
    /// <returns>An instance of the TextSelectionPosition object containing the Index and Length properties</returns>
    public TextSelectionPosition CalculateSelectionPosition()
    {
        return coreTextBox.CalculateSelectionPosition();
    }

    /// <summary>
    /// Counts the total number of characters in the textbox.
    /// Important: resource heavy for very long text!
    /// </summary>
    public int CharacterCount()
    {
        return coreTextBox.CharacterCount();
    }

    /// <summary>
    /// Counts the total number of words in the textbox.
    /// </summary>
    /// <returns></returns>
    public int WordCount()
    {
        return coreTextBox.WordCount();
    }

    /// <summary>
    /// Executes multiple text operations as a single undo/redo action
    /// </summary>
    /// <param name="actionGroup">A delegate containing all the operations to group</param>
    public void ExecuteActionGroup(Action actionGroup)
    {
        coreTextBox.ExecuteActionGroup(actionGroup);
    }

    /// <summary>
    /// Begins a group of actions that will be treated as a single undo/redo operation.
    /// Must be paired with EndActionGroup().
    /// </summary>
    public void BeginActionGroup()
    {
        coreTextBox.BeginActionGroup();
    }

    /// <summary>
    /// Ends the current action group and records it as a single undo item
    /// </summary>
    public void EndActionGroup()
    {
        coreTextBox.EndActionGroup();
    }

    /// <summary>
    /// Gets whether an action group is currently being recorded
    /// </summary>
    public bool IsGroupingActions => coreTextBox.IsGroupingActions;

    /// <summary>
    /// Add lines starting at start
    /// </summary>
    /// <param name="start">The zero based index to start from</param>
    /// <param name="text">The array of lines to add</param>
    /// <returns>True if successfull</returns>
    public bool AddLines(int start, string[] text)
    {
        return coreTextBox.AddLines(start, text);
    }

    /// <summary>
    /// returns the current tabs and spaces detected from the loaded document.
    /// with useSpacesInsteadTabs indicates whether spaces are used instead of tabs and with spaces the number of spaces used for a tab
    /// </summary>
    /// <returns></returns>
    public (bool useSpacesInsteadTabs, int spaces) DetectTabsSpaces()
    {
        return coreTextBox.DetectTabsSpaces();
    }

    /// <summary>
    /// Replaces tab characters with spaces or spaces with tabs in the text, according to the specified settings.
    /// </summary>
    /// <param name="spaces">The number of spaces to use when converting tabs to spaces. Must be greater than zero.</param>
    /// <param name="useSpacesInsteadTabs">Indicates whether tabs should be replaced with spaces.</param>
    /// <param name="ignoreIsReadonly">Ignores the isReadonly property of the textbox.</param>
    public void RewriteTabsSpaces(int spaces, bool useSpacesInsteadTabs, bool ignoreIsReadonly = false)
    {
        coreTextBox.RewriteTabsSpaces(spaces, useSpacesInsteadTabs);
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
    /// Setting the text records an undo step. Use LoadLines/LoadText function to load an initial text.
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
    /// Readonly only prevents the user from entering and modifying text. 
    /// The developer can still call many functions to modify the text
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
    /// Gets whether the textbox has an active selection.
    /// </summary>
    public bool HasSelection => coreTextBox.HasSelection;

    /// <summary>
    /// Gets or sets a value indicating whether the line highlighter shows up even with no focus on the textbox
    /// </summary>
    public bool HighlightLineWhenNotFocused
    {
        get => coreTextBox.HighlightLineWhenNotFocused;
        set => coreTextBox.HighlightLineWhenNotFocused = value;
    }

    /// <summary>
    /// Get the current selection of the textbox in any order. Start may be greater than the end position. 
    /// Returns null if no text is selected
    /// </summary>
    public TextControlBoxSelection? CurrentSelection => coreTextBox.CurrentSelection;

    /// <summary>
    /// Get the current selection of the textbox ordered, so start is always smaller than the end position.
    /// Returns null if no text is selected
    /// </summary>
    public TextControlBoxSelection? CurrentSelectionOrdered => coreTextBox.CurrentSelectionOrdered;

    /// <summary>
    /// Enabled or disable undo redo collection and execution. 
    /// Changing this at runtime, clears the existing undo/redo items, 
    /// since changes on the text while disabled break the system.
    /// </summary>
    public bool UndoRedoEnabled { get => coreTextBox.undoRedo.UndoRedoEnabled; set => coreTextBox.undoRedo.UndoRedoEnabled = value; }

    /// <summary>
    /// Gets or sets a value indicating whether whitespace characters (spaces and tabs)
    /// are visually displayed in the text box (e.g., as dots or arrows).
    /// </summary>
    public bool ShowWhitespaceCharacters
    {
        get => coreTextBox.ShowWhitespaceCharacters;
        set => coreTextBox.ShowWhitespaceCharacters = value;
    }

    /// <summary>
    /// Gets or sets the distance from the text box borders within which the mouse must be positioned 
    /// to start automatic scrolling during text selection. 
    /// This acts like a padding zone that defines how close the mouse needs to be to the edges 
    /// before the text begins to scroll up, down, left, or right.
    /// </summary>
    public Thickness SelectionScrollStartBorderDistance 
    { 
        get => coreTextBox.SelectionScrollStartBorderDistance; 
        set => SelectionScrollStartBorderDistance = value;
    }

    /// <summary>
    /// Gets or sets a value indicating whether hyperlinks within the text are highlighted.
    /// </summary>
    public bool HighlightLinks 
    {
        get => coreTextBox.HighlightLinks;
        set => coreTextBox.HighlightLinks = value;
    }

    /// <summary>
    /// Gets a value indicating whether the current text content can be undone.
    /// </summary>
    public bool CanUndo => coreTextBox.CanUndo;

    /// <summary>
    /// Gets a value indicating whether the current text content can be redone.
    /// </summary>
    public bool CanRedo => coreTextBox.CanRedo;


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

    /// <summary>
    /// Occurs when the TextControlBox finished loading and all components initialized
    /// </summary>
    /// <param name="sender">The instance of the loaded TextControlBox</param>
    public delegate void LoadedEvent(TextControlBox sender);

    /// <summary>
    /// Occurs when the TextControlBox finished loading and all components initialized
    /// </summary>
    public new event LoadedEvent Loaded;

    /// <summary>
    /// Occurs when the TextControlBox finished loading the text.
    /// !Does NOT work with SetText function. 
    /// Works with LoadText and LoadLines function.
    /// </summary>
    /// <param name="sender">The TextControlBox instance which loaded the text</param>
    public delegate void TextLoadedEvent(TextControlBox sender);

    /// <summary>
    /// Occurs when the TextControlBox finished loading all the text
    /// </summary>
    public event TextLoadedEvent TextLoaded;

    /// <summary>
    /// Delegate for handling link click events.
    /// </summary>
    /// <param name="sender">The instance of the TextControlBox that raised the event.</param>
    /// <param name="url">The URL of the clicked link.</param>
    public delegate void LinkClickedEvent(TextControlBox sender, string url);

    /// <summary>
    /// Occurs when a link is clicked within the textbox.
    /// </summary>
    public event LinkClickedEvent LinkClicked;

    /// <summary>
    /// Represents the method that handles events when the indentation style or number of spaces per indent changes in a
    /// text control box.
    /// </summary>
    /// <param name="sender">The instance of the TextControlBox that raised the event.</param>
    /// <param name="spacesInsteadTabs">Indicates whether spaces are used instead of tabs for indentation.</param>
    /// <param name="spaces">The number of spaces used for a single indentation level.</param>
    public delegate void TabsSpacesChangedEvent(TextControlBox sender, bool spacesInsteadTabs, int spaces);

    /// <summary>
    /// Occurs when the tabs or spaces configuration changes.
    /// </summary>
    public event TabsSpacesChangedEvent TabsSpacesChanged;

    /// <summary>
    /// Represents the method that handles an event when the line ending style changes.
    /// </summary>
    /// <param name="sender">The instance of the TextControlBox that raised the event.</param>
    /// <param name="lineEnding">The new line ending style that has been applied.</param>
    public delegate void LineEndingChangedEvent(TextControlBox sender, LineEnding lineEnding);

    /// <summary>
    /// Occurs when the line ending style of the document changes.
    /// </summary>
    public event LineEndingChangedEvent LineEndingChanged;

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