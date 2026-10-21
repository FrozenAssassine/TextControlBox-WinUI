# Changelog

## 1.7.0-beta

### Added
- Word wrap (`WordWrap`)
- Virtualized rendering of very long wrapped lines (100k+ chars)
- Horizontal virtualization of very long lines (50k+ chars)
- Smooth scrolling (`SmoothScrolling`)
- Mouse wheel acceleration, respects the Windows wheel scroll lines setting
- Diagonal scrolling on precision touchpads
- Touchpad pinch to zoom
- Touch support: tap, double tap (word), triple tap (line), long press selection, drag scroll, pinch zoom
- Pen support
- IME composition (Japanese, Chinese, Korean), candidate window at the caret
- Emoji and surrogate pair support (cursor, selection, deletion)
- Double click selects a word, triple click selects a line, drag extends by word / line
- Shift + PageUp / PageDown selection
- Up on the first line / Down on the last line moves to text start / end
- Batched undo: typing within a word is one undo step (`UndoBatching`)
- `ExecuteActionGroup`, `BeginActionGroup`, `EndActionGroup` for grouped undo
- `ScrollIntoViewHorizontallyCentered()`
- `Delete()`
- `Dispose()` (`IDisposable`)
- `GetLineFromPoint(Point)`
- `SingleLineHeight`
- Log file syntax highlighting
- Generic language independent syntax highlighting (`SyntaxHighlightID.Generic`)
- Arrow cursor over the scrollbars
- Native AOT compatibility (`IsAotCompatible`)
- Crash safety net: an exception in an input handler repairs the state instead of ending the app

### Improved
- Undo memory: single line edits store only the changed part (190 MB -> 1.4 MB on long lines)
- Line width measured by arithmetic for monospace fonts (14 ms -> 4.4 ms per key on long lines)
- Syntax highlighting regexes no longer block the UI thread
- Zoom keeps the line in the viewport center in place
- Line selection logic (triple click, line numbers)
- Unload / Loaded handling: timers stop on inactive tabs and restart on return
- Double click time follows the Windows setting (was fixed 250 ms)
- Escape is only handled when a selection was cleared, otherwise it reaches the host
- Enter inside the indentation keeps the line's own indentation

### Changed
- `GetSyntaxHighlightingFromJson` uses System.Text.Json
- Invalid JSON language input returns `Succeed = false` instead of throwing
- `SyntaxHighlightID.Log` moved behind `None`, existing ids keep their numbers
- Scroll position stored in pixels internally
- `SetSelection(start, length)`: a negative length selects backwards, values are clamped to the text
- `AddLines` throws `ArgumentException` for lines containing line breaks, returns false for null

### Removed
- Newtonsoft.Json dependency

### Fixed
- Touch and pen taps did not place the cursor or open the touch keyboard
- IME composition was cut off after the first character
- Duplicate `GotFocus` event
- Timers and memory leaked after closing tabs
- Theme changes did not update colors
- Right click menu opened twice
- Line numbers and text were vertically offset
- Mouse to cursor offset in no-wrap mode
- Horizontal scrollbar length and scroll glitches
- Horizontal scroll broken after toggling word wrap
- Zoom glitches
- Cursor and selection placement in wrapped lines (row ends, selecting from the right)
- Cursor not drawn at the end of very long wrapped lines
- Line numbers, caret and selection rows away from the text in large wrapped documents
- Long whitespace and tab indents not wrapping
- Selection cut off early with LF / CR line endings
- Overlapping selection rectangles
- Search highlights drawn at the wrong position
- Many undo / redo edge cases (empty lines, deleted lines, tab indentation)
- Copy of a line without selection pasted at the wrong position
- Comment markers inside strings highlighted as comments
- Regex fixes in JSON, CSS, HTML, XML, SQL, Markdown, TOML, Klipper, Batch, GCode, HexFile, LaTeX, PHP, QSharp, GitIgnore, Log, JavaScript, CSV Enhanced
- A highlight rule without pattern disabled all following rules
- Enter at the start of an indented line doubled the indentation
- Backspace at a wrapped row end moved the cursor one too far
- PageUp / PageDown could place the cursor inside an emoji
- `ReplaceNext` with `\n` in a CRLF document broke the line
- `SetSelection` with negative values threw on `SelectedText`
- Hover without pressed button selected text after a lost pointer capture
- Shift + click anchored at an old click position after keyboard navigation
- Touch triple tap never selected the line
- Pinch during a touch selection left the selection active
- Dragging over line numbers did not work without focus
- Text typed with Ctrl / Alt held was inserted with the next character
