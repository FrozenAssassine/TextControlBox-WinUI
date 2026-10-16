# Production Readiness & Release TODOs

Dieses Dokument dient als Tracking-Checkliste für den stabilen Production-Release von **TextControlBox-WinUI** auf allen Zielgeräten (Desktop, Laptops, 2-in-1 Tablets, Surface mit Stift/Touch).

---

## 🚨 P0: Kritische Code-Bugs (Vor Production beheben)

- [x] **1. Touch- & Stift-Tappen zum Platzieren des Cursors reparieren**
  - **Problem**: In [PointerActionsManager.cs](file:///f:/Github/TextControlBox-WinUI/TextControlBox/Core/PointerActionsManager.cs) fing `CheckTouchInput_Click` alle Touch- und Stifteingaben ab und gab `true` zurück. Dadurch brach [CoreTextControlBox.xaml.cs](file:///f:/Github/TextControlBox-WinUI/TextControlBox/Core/CoreTextControlBox.xaml.cs) in `Canvas_Selection_PointerPressed` sofort ab.
  - **Auswirkung**: Auf Tablets/Touchscreens konnte der Cursor weder per Finger noch per Stift platziert werden, und die Bildschirmtastatur öffnete sich nicht.
  - **Lösung**: Sauberes Touch-Management in `PointerActionsManager` implementiert: Single-Tap platziert den Cursor und fokussiert das Control (Touch-Keyboard öffnet sich), Doppeltippen markiert das Wort, Dreifachtippen die Zeile, Long-Press aktiviert Selektion, Ziehen scrollt 1:1 mit Trägheit (Inertia), 2-Finger-Geste zoomt via Pinch-to-Zoom und Stifteingaben (Pen) laufen pixelgenau ohne Stubs.

- [x] **2. IME-Komposition für CJK (Japanisch, Chinesisch, Koreanisch) & Dead Keys reparieren**
  - **Problem**: In `InputHandler_TextEntered` ([CoreTextControlBox.xaml.cs](file:///f:/Github/TextControlBox-WinUI/TextControlBox/Core/CoreTextControlBox.xaml.cs)) wird `inputHandler.Text = ""` sofort beim ersten Zeichen geleert.
  - **Auswirkung**: Bricht mehrstufige IME-Kompositionen (z. B. Romaji -> Kanji oder Pinyin -> Hanzi) sofort ab. Zudem liegt das unsichtbare `InputHandlerControl` bei `Width="0" Height="0"` an einer festen Position, sodass Windows das IME-Kandidatenfenster nicht an der Cursorposition öffnen kann.
  - **Lösung**: IME-Kompositionszustand berücksichtigen bzw. das Input-Element an die reale Caret-Position nachführen oder Text erst nach Bestätigung verarbeiten.

- [x] **3. Direct2D Device Lost / Standby / Monitor-Wechsel absichern**
  - **Problem**: In [DesignHelper.cs](file:///f:/Github/TextControlBox-WinUI/TextControlBox/Helper/DesignHelper.cs) verhindert `ColorResourcesCreated = true` das erneute Erstellen von Brushes nach einem Grafiktreiber-Reset oder Ruhezustand (Modern Standby). `CanvasControl.CreateResources` wird derzeit nicht ausgewertet.
  - **Auswirkung**: Nach Aufwecken aus dem Standby oder Monitor-Wechsel (z. B. Dockingstation an-/abstecken) können Brushes ungültig werden und Zeichenoperationen fehlschlagen.
  - **Status**: Getestet mit GPU-Treiber-Reload und Monitor aus/an; läuft stabil ohne Probleme.


- [x] **4. Speicherbereinigung & Unload-Leak bei Tab-Navigation verhindern**
  - **Problem**: Bei Tab-Wechsel (z. B. in Fastedit) liefen DispatcherTimers (Caret-Blinken, Pointer-Timer) und Composition-Synchronisation für inaktive Tabs weiter. Zudem gab es keine `IDisposable`-Implementierung und keine Win2D `RemoveFromVisualTree()`-Bereinigung beim endgültigen Schließen von Tabs.
  - **Lösung**:
    - **Inaktiver Tab (`Unloaded`)**: Stoppt alle Hintergrund-Timer (`caretBlinkManager.Stop()`, `pointerActionsManager.CleanUp()`, `TeardownDiagonalScroll()`) und setzt Touch-Zustände zurück. Der Text (`totalLines`) und Undo-Stack bleiben für die Tab-Rückkehr erhalten.
    - **Tab-Rückkehr (`Loaded`)**: Reaktiviert Composition-Panning (`SetupDiagonalScroll()`), Caret-Blinken (falls fokussiert) und zeichnet die Canvases neu (`UpdateAll()`).
    - **Finales Schließen (`Dispose()` / `Unload()`)**: `IDisposable` auf `TextControlBox` und `CoreTextControlBox` implementiert. Gibt `totalLines.Dispose()` an den `ArrayPool` zurück, trennt Events, ruft `RemoveFromVisualTree()` auf allen 4 Win2D-Canvases auf und leert den Undo/Redo-Speicher. Mehrfache Aufrufe sind idempotent geschützt.

---

## ⚠️ P1: Wichtige Korrekturen & Edge Cases

- [x] **5. Doppeltes `GotFocus`-Event entfernen**
  - **Datei**: [FocusManager.cs](file:///f:/Github/TextControlBox-WinUI/TextControlBox/Core/FocusManager.cs)
  - **Problem**: `eventsManager.CallGotFocus()` wird bei `!HasFocus` einmal bedingt und direkt danach ein zweites Mal unbedringt aufgerufen.
  - **Lösung**: Den doppelten Aufruf entfernen, sodass `GotFocus` nur genau einmal pro Fokuswechsel gefeuert wird.

- [x] **6. `Shift + PageUp` und `Shift + PageDown` Selektion unterstützen**
  - **Datei**: [CoreTextControlBox.xaml.cs](file:///f:/Github/TextControlBox-WinUI/TextControlBox/Core/CoreTextControlBox.xaml.cs)
  - **Problem**: In `InputHandler_KeyDown` wird bei `PageUp` und `PageDown` nicht geprüft, ob `Shift` gedrückt ist.
  - **Lösung**: `selectionManager.StartSelectionIfNeeded()` und `SetSelectionEnd` integrieren, wenn `shift == true`.

- [x] **7. Semantik von `SelectedText` bei leerer Selektion klarstellen / korrigieren**
  - **Datei**: [SelectionManager.cs](file:///f:/Github/TextControlBox-WinUI/TextControlBox/Core/Selection/SelectionManager.cs)
  - **Problem**: Wenn nichts markiert ist, gibt `SelectedText` die komplette aktuelle Zeile zurück. Ein externer Aufruf von `textbox.SelectedText = textbox.SelectedText;` verdoppelt daher die Zeile, statt unverändert zu bleiben.
  - **Lösung**: Entscheiden, ob `SelectedText` im Standard-WinUI-Stil `""` zurückgeben soll, während Ctrl+C/Ctrl+X intern eine eigene Zeilen-Kopierlogik verwendet.

- [ ] **8. AnyCPU Build-Warnung für Win2D auflösen**
  - **Datei**: [TextControlBox.csproj](file:///f:/Github/TextControlBox-WinUI/TextControlBox/TextControlBox.csproj)
  - **Problem**: Win2D warnt bei AnyCPU-Builds (`WIN2D0001: Microsoft.Graphics.Canvas.dll cannot be referenced correctly`).
  - **Lösung**: `<Win2DWarnNoPlatform>true</Win2DWarnNoPlatform>` in die csproj aufnehmen oder Plattform-Profile bereinigen.

---

## 📋 Manuelle Test-Matrix (Vor dem Release abhaken)

### 📱 Touch, Stift & Virtuelle Tastatur (z. B. Surface Pro / Tablet)
- [x] **Touch-Cursor**: Tappen mit dem Finger platziert den Cursor exakt an der Berührungsstelle.
- [x] **Stift-Präzision**: Antippen mit Surface Pen platziert den Cursor pixelgenau.
- [x] **Bildschirmtastatur (SIP)**: Beim Antippen öffnet sich die virtuelle Windows-Tastatur automatisch.
- [ ] **Sichtfeld bei Tastatur**: Verdeckt die Bildschirmtastatur den Text am unteren Bildschirmrand, oder scrollt die Ansicht mit?
- [x] **Touch-Scrolling**: 1-Finger-Wischen scrollt das Dokument flüssig; Pinch-to-Zoom mit zwei Fingern skaliert den Text zentriert.

### 🖥️ Monitore, DPI & Skalierung
- [ ] **Multi-DPI Monitor-Wechsel**: App-Fenster von 100% Monitor (z. B. Full HD) auf 150%/200% Monitor (4K / Laptop) ziehen.
  - [ ] Text bleibt knackscharf (kein Blur durch Bitmap-Skalierung).
  - [ ] Zeilennummern-Spalte passt ihre Breite korrekt an.
  - [ ] Klick-Positionen treffen weiterhin exakt die Zeichen.
- [ ] **Windows-Skalierung im Betrieb ändern**: Skalierung unter `Einstellungen -> System -> Anzeige` live umstellen (z. B. 125% -> 175%).
- [ ] **Fenster-Größenänderung**: Schnelles Resizen des Fensters führt zu keinen Render-Glitches oder Hängern.

### ⌨️ Tastatur-Layouts & Eingabemethoden
- [ ] **AltGr-Kombinationen (Deutsch)**: `@`, `€`, `\`, `[`, `]`, `{`, `}`, `~`, `|` funktionieren zuverlässig.
- [ ] **Dead Keys**: Akzente wie `^` + `e` -> `ê`, `´` + `a` -> `á`, `` ` `` + `a` -> `à`.
- [ ] **Windows Emoji-Panel**: `Win` + `.` öffnen, Emoji auswählen -> wird an der richtigen Position eingefügt.
- [ ] **CJK / IME**: Japanische oder chinesische Tastatur aktivieren -> Zeichenumwandlung und Kandidatenfenster testen.
- [ ] **Tastatur-Navigation**:
  - [ ] `Ctrl + Left` / `Ctrl + Right` (wortweises Springen)
  - [ ] `Home` / `End` und `Ctrl + Home` / `Ctrl + End`
  - [ ] `Shift + Pfeiltasten` (Zeichen- und Zeilenselektion)
  - [ ] `Shift + PageUp / PageDown`

### 🖱️ Precision Touchpad & Maus
- [ ] **Diagonales Scrollen**: Mit 2 Fingern diagonal über das Touchpad streichen -> horizontales und vertikales Scrollen laufen synchron.
- [ ] **Pinch-to-Zoom**: Geste auf dem Touchpad zoomt flüssig mit dem Cursor als Fixpunkt.
- [ ] **Mausrad-Kombinationen**:
  - [ ] Standard-Rad: vertikales Scrollen
  - [ ] `Shift + Rad`: horizontales Scrollen
  - [ ] `Ctrl + Rad`: Zoom in / Zoom out
- [ ] **Multi-Klick**:
  - [ ] Doppelklick: Wort markieren
  - [ ] Dreifachklick: Ganze Zeile markieren
  - [ ] Klick in Zeilennummernleiste: Zeile markieren + Drag zum Erweitern

### ⚡ Performance & Extremfälle (Stresstest)
- [ ] **Große Dateien**: Datei mit 100.000+ Zeilen laden -> Scrollen muss flüssig (60 FPS) bleiben.
- [ ] **Lange Zeilen (Horizontal Virtualization)**: Minifizierte Datei mit 50.000–200.000 Zeichen in einer einzigen Zeile laden.
  - [ ] Horizontales Scrollen reagiert ohne Ruckler.
  - [ ] Cursor-Navigation entlang der Endlos-Zeile funktioniert.
- [ ] **Gemischte Zeilenenden**: Datei mit CRLF und LF gemischt öffnen -> Selektion und Copy-Paste bleiben deckungsgleich.
- [ ] **Massives Copy & Paste**: 100.000 Zeilen kopieren und einfügen; Undo (`Ctrl + Z`) und Redo (`Ctrl + Y`) prüfen.
- [ ] **Schnelles Scrollen**: Scrollbalken-Daumen mit der Maus greifen und schnell auf und ab ziehen.

### 🔄 System- & Hardware-Ereignisse
- [ ] **Standby / Ruhezustand**: Gerät zuklappen/sperren, wieder aufwecken -> Control zeichnet sofort fehlerfrei weiter.
- [ ] **Grafiktreiber-Reset**: Mit `Win + Ctrl + Shift + B` den Windows-Grafiktreiber neustarten -> Canvas stürzt nicht ab.
- [ ] **Theme-Wechsel**: Windows-Einstellung zwischen Hell- und Dunkelmodus wechseln -> Textbox passt Farben sofort an.
- [ ] **Tab-Schließen / Navigation**: Mehrere Tabs mit TextControlBox öffnen und schließen -> Speicherverbrauch im Task-Manager beobachten (kein Leak).
