using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using System;
using System.Diagnostics;
using System.Linq;
using TextControlBoxNS;
using TextControlBoxNS.Core.Text;

namespace TextControlBox.Tests;

[TestClass]
public class EndUserFunctionTests
{

    [UITestMethod]
    public void AddLineWithOutOfRangeLine()
    {
        var textbox = TestHelper.MakeTextbox(20);

        Assert.IsFalse(textbox.AddLine(100, "Test Text"));
        Assert.IsFalse(textbox.AddLine(-100, "Test Text"));
        
        Assert.IsTrue(textbox.AddLine(0, "Test Text"));
        Assert.IsTrue(textbox.AddLine(0, "Test Text"));
    }

    [UITestMethod]
    public void GetLinesTextWithOutOfRangeLine()
    {
        var textbox = TestHelper.MakeTextbox(10);

        try
        {
            textbox.GetLinesText(100, 10);
        }
        catch (IndexOutOfRangeException)
        {
            return;
        }   
        Debug.Assert(false);
    }

    [UITestMethod]
    public void GetLineTextWithOutOfRangeLine()
    {
        var textbox = TestHelper.MakeTextbox();

        Assert.ThrowsExactly<IndexOutOfRangeException>(() =>
        {
            textbox.GetLineText(100);
        });
    }

    [UITestMethod]
    public void GetTextNoTextInTB()
    {
        var textbox = TestHelper.MakeTextbox();
        Debug.WriteLine("Function GetText without text");

        textbox.SetText("");

        var text = textbox.GetText();

        Assert.AreEqual(0, text.Length);
    }

    [UITestMethod]
    public void SetCursorpositionTooHigh()
    {
        var textbox = TestHelper.MakeTextbox();

        textbox.SetCursorPosition(500, 1000, true);

        Assert.AreNotEqual(500, textbox.CursorPosition.LineNumber);
        Assert.AreNotEqual(10000, textbox.CursorPosition.CharacterPosition);
    }

    [UITestMethod]
    public void SetCursorpositionNegative()
    {
        var textbox = TestHelper.MakeTextbox(10);

        textbox.SetCursorPosition(-100, -500, true);

        Assert.AreNotEqual(-100, textbox.CursorPosition.LineNumber);
        Assert.AreNotEqual(-500, textbox.CursorPosition.CharacterPosition);
    }

    [UITestMethod]
    public void SetSelectionTooHigh()
    {
        var textbox = TestHelper.MakeTextbox(10);
        textbox.SetSelection(10000, 5000);

        Assert.IsFalse(textbox.CurrentSelectionOrdered.HasValue);
        Assert.IsFalse(textbox.CurrentSelection.HasValue);
    }

    [UITestMethod]
    public void GetSelectedTextEqualsTextboxText()
    {
        var textbox = TestHelper.MakeTextbox(10);

        const string text = "Line1\nLine2\nLine3\n";
        textbox.SetText(text);
        textbox.SelectAll();

        Assert.AreEqual(LineEndings.CleanLineEndings(text, textbox.LineEnding), textbox.SelectedText);
    }

    [UITestMethod]
    public void SetSelectedText_NoTextInTB()
    {
        var textbox = TestHelper.MakeTextbox(0);
        textbox.SetText("");

        var text = "Line1\nLine2\nLine3\n";

        textbox.SelectedText = text;

        //should be no selection
        Assert.AreEqual(LineEndings.CleanLineEndings(text, textbox.LineEnding), textbox.GetText());
    }

    [UITestMethod]
    public void SetText_EmptyString()
    {
        var textbox = TestHelper.MakeTextbox(10);

        textbox.SetText("");

        Debug.Assert(textbox.GetText().Length == 0 &&
            textbox.CursorPosition.LineNumber == 0 &&
            textbox.CursorPosition.CharacterPosition == 0);
    }

    public void LoadLines_EmptyArray()
    {
        var textbox = TestHelper.MakeTextbox(10);
        textbox.LoadLines([]);

        Debug.Assert(textbox.GetText().Length == 0 &&
            textbox.CursorPosition.LineNumber == 0 &&
            textbox.CursorPosition.CharacterPosition == 0);
    }

    [UITestMethod]
    public void LoadText_EmptyString()
    {
        var textbox = TestHelper.MakeTextbox(0);
        textbox.LoadText("");

        bool res = textbox.GetText().Length == 0 &&
            textbox.CursorPosition.LineNumber == 0 &&
            textbox.CursorPosition.CharacterPosition == 0;
        Debug.Assert(res);
    }

    [UITestMethod]
    public void SelectAllSetSelectedText()
    {
        var textbox = TestHelper.MakeTextbox(0);

        var text = "Line1\nLine2\nLine3\n";

        textbox.SetText("Line100\nLine200\nLine300\n");

        textbox.SelectAll();

        textbox.SelectedText = text;

        Debug.Assert(textbox.GetText().Equals(LineEndings.CleanLineEndings(text, textbox.LineEnding)));
    }

    [UITestMethod]
    public void SetText()
    {
        var textbox = TestHelper.MakeTextbox(10);

        var text = "Line1\nLine2\nLine3\n";

        textbox.SetText(text);

        bool res = textbox.GetText().Equals(LineEndings.CleanLineEndings(text, textbox.LineEnding));
        Debug.Assert(res);
    }
    [UITestMethod]
    public void SetText3Lines()
    {
        var textbox = TestHelper.MakeTextbox(10);

        var text = "Line1\nLine2\nLine3";

        textbox.SetText(text);

        bool res = textbox.GetText().Equals(LineEndings.CleanLineEndings(text, textbox.LineEnding)) &&
            textbox.CursorPosition.LineNumber == 2 && textbox.CursorPosition.CharacterPosition == 5;
        Debug.Assert(res);
    }
    [UITestMethod]
    public void LoadText_3Lines()
    {
        var textbox = TestHelper.MakeTextbox(10);

        var text = "Line1\nLine2\nLine3";

        textbox.LoadText(text);

        bool res = textbox.GetText().Equals(LineEndings.CleanLineEndings(text, textbox.LineEnding)) &&
            textbox.CursorPosition.LineNumber == 2 && textbox.CursorPosition.CharacterPosition == 5;
        Debug.Assert(res);
    }
    [UITestMethod]
    public void LoadLines3Lines()
    {
        var textbox = TestHelper.MakeTextbox(10);

        var text = "Line1\nLine2\nLine3";

        textbox.LoadLines(["Line1", "Line2", "Line3"]);

        bool res = textbox.GetText().Equals(LineEndings.CleanLineEndings(text, textbox.LineEnding)) &&
            textbox.CursorPosition.LineNumber == 2 && textbox.CursorPosition.CharacterPosition == 5;
        Debug.Assert(res);
    }
    [UITestMethod]
    public void SelectSyntaxHighlightingById()
    {
        var textbox = TestHelper.MakeTextbox(10);

        textbox.LoadLines(["Line1", "Line2", "Line3"]);

        try
        {
            foreach (SyntaxHighlightID item in Enum.GetValues(typeof(SyntaxHighlightID)))
            {
                textbox.SelectSyntaxHighlightingById(item);
            }
        }
        catch
        {
            Debug.Assert(false);
        }
    }

    [UITestMethod]
    public void GetSyntaxHighlightingFromID()
    {
        var textbox = TestHelper.MakeTextbox(10);

        try
        {
            foreach (SyntaxHighlightID item in Enum.GetValues(typeof(SyntaxHighlightID)))
            {
                textbox.SyntaxHighlighting = TextControlBoxNS.TextControlBox.GetSyntaxHighlightingFromID(item);
            }

            textbox.SyntaxHighlighting = null;
        }
        catch
        {
            Debug.Assert(false);
            return;
        }
        Debug.Assert(true);
    }

    [UITestMethod]
    public void Test_SearchReplaceAll()
    {
        var textbox = TestHelper.MakeTextbox(100);

        textbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");

        string textBefore = textbox.GetText();
        textbox.ReplaceAll("line", "Test", false, false);

        Debug.Assert(textbox.GetText().Equals(textBefore.Replace("Line", "Test")));
    }
    [UITestMethod]
    public void SearchReplaceAll_MatchCase()
    {
        var textbox = TestHelper.MakeTextbox(100);

        textbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");

        string textBefore = textbox.GetText();

        //should replace nothing
        textbox.ReplaceAll("line", "Test", true, false);
        bool res1 = textbox.GetText().Equals(textBefore);

        textbox.ReplaceAll("Line", "Test", true, false);

        bool res2 = textbox.GetText().Equals(textBefore.Replace("Line", "Test"));
        Debug.Assert(res1 && res2);
    }

    [UITestMethod]
    public void SearchReplaceAllWholeWord()
    {
        var textbox = TestHelper.MakeTextbox(100);

        //nothign should change
        textbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");
        string textBefore = textbox.GetText();
        textbox.ReplaceAll("Line", "Test", false, true);

        bool res1 = textbox.GetText().Equals(textBefore);

        //replace them
        textbox.SetText("Line 1\nLine 2\nLine 3\nLine 4\nLine 5");

        textBefore = textbox.GetText();
        textbox.ReplaceAll("Line", "Test", false, true);

        bool res2 = textbox.GetText().Equals(textBefore.Replace("Line", "Test"));
        Debug.Assert(res1 && res2);
    }

    [UITestMethod]
    public void SearchReplaceAllReadonlyMode()
    {
        var textbox = TestHelper.MakeTextbox(10);
        textbox.IsReadOnly = true;
        string textBefore = textbox.GetText();

        //nothing should change
        var replaceRes = textbox.ReplaceAll("Line", "Test", false, false);

        bool res1 = replaceRes  == SearchResult.ReplaceNotAllowedInReadonly && textbox.GetText().Equals(textBefore);

        Debug.Assert(res1);
    }

    [UITestMethod]
    public void SearchReplaceAllForcedInReadonlyMode()
    {
        var textbox = TestHelper.MakeTextbox(10);
        textbox.IsReadOnly = true;
        var textBefore = textbox.GetText();

        //force replace, even with readonly
        var replaceRes = textbox.ReplaceAll("Line", "Test", false, false, ignoreIsReadOnly: true);

        bool res2 = replaceRes == SearchResult.Found && textbox.GetText().Equals(textBefore.Replace("Line", "Test"));

        Debug.Assert(res2);
    }


    [UITestMethod]
    public void SearchReplaceNextReadonlyMode()
    {
        var textbox = TestHelper.MakeTextbox(10);
        textbox.IsReadOnly = true;
        //nothing should change
        textbox.SetCursorPosition(0, 0);

        textbox.BeginSearch("Line", false, false);
        string textBefore = textbox.GetText();
        bool success = true;
        for (int i = 0; i < 10; i++)
        {
            if (textbox.ReplaceNext("Test") != SearchResult.ReplaceNotAllowedInReadonly)
                success = false;
        }

        bool res1 = success && textbox.GetText().Equals(textBefore) && !textbox.HasSelection;
        Debug.Assert(res1);
    }

    [UITestMethod]
    public void SearchReplaceNextForcedInReadonlyMode()
    {
        var textbox = TestHelper.MakeTextbox(10);
        textbox.IsReadOnly = true;
        string textBefore = textbox.GetText();

        //force replace, even with readonly
        textbox.SetCursorPosition(0, 0);

        var reslol = textbox.BeginSearch("Line", false, false);
        bool success = true;
        for (int i = 0; i < 10; i++)
        {
            if (textbox.ReplaceNext("Test", ignoreIsReadOnly: true) != SearchResult.Found)
                success = false;
        }

        bool textMatch = textbox.GetText().Equals(textBefore.Replace("Line", "Test"));
        bool res1 = success && textMatch && !textbox.HasSelection;
        Debug.Assert(res1);
    }

    [UITestMethod]
    public void SearchReplaceNext()
    {
        var textbox = TestHelper.MakeTextbox(100);

        textbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");
        textbox.SetCursorPosition(0, 0);

        textbox.BeginSearch("Line", false, false);
        string textBefore = textbox.GetText();
        for (int i = 0; i < 5; i++)
        {
            textbox.ReplaceNext("Test");
        }

        Debug.Assert(textbox.GetText().Equals(textBefore.Replace("Line", "Test")) && !textbox.HasSelection);
    }

    [UITestMethod]
    public void SearchFindNext()
    {
        var textbox = TestHelper.MakeTextbox(100);

        textbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");
        textbox.SetCursorPosition(0, 0);

        textbox.BeginSearch("Line", false, false);
        string textBefore = textbox.GetText();

        bool invalidRes = false;
        for (int i = 0; i < 5; i++)
        {
            if (textbox.FindNext() != SearchResult.Found)
                invalidRes = true;
        }
        Debug.Assert(!invalidRes);
    }

    [UITestMethod]
    public void SearchFindPrevious()
    {
        var textbox = TestHelper.MakeTextbox(100);

        textbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");
        textbox.SetCursorPosition(5, 5);

        textbox.BeginSearch("Line", false, false);
        string textBefore = textbox.GetText();

        bool invalidRes = false;
        for (int i = 0; i < 5; i++)
        {
            if (textbox.FindPrevious() != SearchResult.Found)
                invalidRes = true;
        }
        Debug.Assert(!invalidRes);
    }

    [UITestMethod]
    public void SearchReplaceAllWithNothing()
    {
        var textbox = TestHelper.MakeTextbox(100);

        textbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");

        string textBefore = textbox.GetText();
        textbox.ReplaceAll("line", "", false, false);

        Assert.AreEqual(textBefore.Replace("Line", ""), textbox.GetText());
    }

    [UITestMethod]
    public void TextLoadedEvent()
    {
        var textbox = TestHelper.MakeTextbox(100);

        const string textToLoad = "Hello World";
        bool eventTriggered = false;

        void Textbox_TextLoaded(TextControlBoxNS.TextControlBox sender)
        {
            eventTriggered = true;
        }

        textbox.TextLoaded += Textbox_TextLoaded;
        textbox.LoadText(textToLoad);

        bool res = eventTriggered && textbox.GetText() == textToLoad;
        textbox.TextLoaded -= Textbox_TextLoaded;

        Debug.Assert(res);
    }
    [UITestMethod]
    public void TabsSpacesChangedEvent()
    {
        var textbox = TestHelper.MakeTextbox(100);

        bool[] success = new bool[4];
        int testCase = 0;
        void Textbox_TabsSpacesChanged(TextControlBoxNS.TextControlBox sender, bool spacesInsteadTabs, int spaces)
        {
            switch (testCase)
            {
                case 0:
                    success[0] = spacesInsteadTabs == false;
                    break;
                case 1:
                    success[1] = spacesInsteadTabs == true;
                    break;
                case 2:
                    success[2] = spacesInsteadTabs == true && spaces == 8;
                    break;
                case 3:
                    success[3] = spacesInsteadTabs == true && spaces == 16;
                    break;
            }
            testCase++;
        }

        textbox.TabsSpacesChanged += Textbox_TabsSpacesChanged;

        textbox.UseSpacesInsteadTabs = false;
        textbox.UseSpacesInsteadTabs = true;
        textbox.NumberOfSpacesForTab = 8;
        textbox.NumberOfSpacesForTab = 16;

        int exceptionThrownCount = 0;
        try
        {
            textbox.NumberOfSpacesForTab = 0;
        }
        catch (ArgumentOutOfRangeException)
        {
            exceptionThrownCount++;
        }
        try
        {
            textbox.NumberOfSpacesForTab = -1;
        }
        catch (ArgumentOutOfRangeException)
        {
            exceptionThrownCount++;
        }

        textbox.TabsSpacesChanged -= Textbox_TabsSpacesChanged;
        Debug.Assert(success.Any(item => item == true) && testCase == 4 && exceptionThrownCount == 2);
    }

    [UITestMethod]
    public void LineEndingChangedEvent()
    {
        var textbox = TestHelper.MakeTextbox(100);

        bool[] success = new bool[3];
        int testCase = 0;
        void Textbox_LineEndingChanged(TextControlBoxNS.TextControlBox sender, LineEnding lineEnding)
        {
            switch (testCase)
            {
                case 0:
                    success[0] = lineEnding == LineEnding.LF;
                    break;
                case 1:
                    success[1] = lineEnding == LineEnding.CRLF;
                    break;
                case 2:
                    success[2] = lineEnding == LineEnding.CR;
                    break;
            }
            testCase++;
        }
        textbox.LineEndingChanged += Textbox_LineEndingChanged;
        textbox.LineEnding = LineEnding.LF;
        textbox.LineEnding = LineEnding.CR;
        textbox.LineEnding = LineEnding.CRLF;
        textbox.LineEndingChanged -= Textbox_LineEndingChanged;

        Debug.Assert(success.Any(item => item == true));
    }

    [UITestMethod]
    public void LoadLinesWithTab_DetectTabsSpaces_Tabs()
    {
        var textbox = TestHelper.MakeTextbox(0);

        textbox.LoadLines(["Line1", "\tLine2", "\t\tLine3"]);

        //4 is the default value
        Debug.Assert(textbox.UseSpacesInsteadTabs == false && textbox.NumberOfSpacesForTab == 4);
    }
    [UITestMethod]
    public void LoadLinesWithTab_DetectTabsSpaces_4Spaces()
    {
        var textbox = TestHelper.MakeTextbox(0);
        Debug.WriteLine("Loadlines with tab, detect tabsspaces (4 spaces)");

        textbox.LoadLines(["Line1", "    Line2", "        Line3"]);

        //4 is the default value
        Debug.Assert(textbox.UseSpacesInsteadTabs == true && textbox.NumberOfSpacesForTab == 4);
    }
    [UITestMethod]
    public void LoadLinesWithTab_DetectTabsSpaces_8Spaces()
    {
        var textbox = TestHelper.MakeTextbox(0);
        Debug.WriteLine("Loadlines with tab, detect tabsspaces (8 spaces)");

        textbox.LoadLines(["Line1", "        Line2", "                Line3"]);

        //4 is the default value
        Debug.Assert(textbox.UseSpacesInsteadTabs == true && textbox.NumberOfSpacesForTab == 8);
    }
    [UITestMethod]
    public void LoadTextWithTab_DetectTabsSpaces_4Spaces()
    {
        var textbox = TestHelper.MakeTextbox(0);
        Debug.WriteLine("Loadtext with tab, detect tabsspaces (4 spaces)");

        textbox.LoadText("Line1\n        Line2\n                Line3\n");

        //4 is the default value
        Debug.Assert(textbox.UseSpacesInsteadTabs == true && textbox.NumberOfSpacesForTab == 8);
    }
    [UITestMethod]
    public void LoadTextWithTab_DetectTabsSpaces_Tabs()
    {
        var textbox = TestHelper.MakeTextbox(0);
        Debug.WriteLine("Loadtext with tab, detect tabsspaces (tabs)");

        textbox.LoadText("Line1\n\tLine2\n\t\tLine3\n");

        //4 is the default value
        Debug.Assert(textbox.UseSpacesInsteadTabs == false && textbox.NumberOfSpacesForTab == 4);
    }
    [UITestMethod]
    public void LoadTextWithTab_DetectTabsSpaces_2Spaces()
    {
        var textbox = TestHelper.MakeTextbox(0);
        Debug.WriteLine("Loadtext with tab, detect tabsspaces (2 spaces)");

        textbox.LoadText("Line1\n    Line2\n        Line3\n");

        //4 is the default value
        Debug.Assert(textbox.UseSpacesInsteadTabs == true && textbox.NumberOfSpacesForTab == 4);
    }
    [UITestMethod]
    public void Test_RewriteTabsSpaces_SpacesToTabs()
    {
        var textbox = TestHelper.MakeTextbox(100);

        Debug.WriteLine("Test: Spaces -> Tabs");

        textbox.LoadText("    Line1\n    Line2\n    Line3"); // 4 spaces
        string textBefore = textbox.GetText();

        // Rewrite using tabs
        textbox.RewriteTabsSpaces(4, false);

        string expected = "\tLine1\n\tLine2\n\tLine3";
        string textAfter = textbox.GetText();

        Debug.Assert(expected.Equals(textAfter));
    }

    [UITestMethod]
    public void Test_RewriteTabsSpaces_TabsToSpaces()
    {
        var textbox = TestHelper.MakeTextbox(100);

        Debug.WriteLine("Test: Tabs -> Spaces");

        textbox.LoadText("\tLine1\n\tLine2\n\tLine3"); // tabs
        string textBefore = textbox.GetText();

        // Rewrite using 2 spaces
        textbox.RewriteTabsSpaces(2, true);

        string expected = "  Line1\n  Line2\n  Line3";
        string textAfter = textbox.GetText();

        Debug.Assert(expected.Equals(textAfter));
    }

    [UITestMethod]
    public void Test_RewriteTabsSpaces_ChangeSpacesWidth()
    {
        var textbox = TestHelper.MakeTextbox(100);

        Debug.WriteLine("Test: Change spaces width");

        textbox.LoadText("  Line1\n  Line2\n  Line3"); // 2 spaces
        string textBefore = textbox.GetText();

        // Rewrite to 4 spaces
        textbox.RewriteTabsSpaces(4, true);

        string expected = "    Line1\n    Line2\n    Line3";
        string textAfter = textbox.GetText();

        Debug.Assert(expected.Equals(textAfter));
    }

    [UITestMethod]
    public void Test_RewriteTabsSpaces_MixedLines()
    {
        var textbox = TestHelper.MakeTextbox(100);

        Debug.WriteLine("Test: Mixed spaces/tabs");

        textbox.LoadText("\tLine1\n    Line2\n\tLine3\n  Line4"); // mixed
        string textBefore = textbox.GetText();

        // Rewrite all to 4 spaces
        textbox.RewriteTabsSpaces(4, true);

        bool res = textbox.UseSpacesInsteadTabs;
        int count = textbox.NumberOfSpacesForTab;

        string expected = "    Line1\n        Line2\n    Line3\n    Line4";
        string textAfter = textbox.GetText();

        Debug.Assert(expected.Equals(textAfter));
    }

    [UITestMethod]
    public void Test_SelectLines()
    {
        string[] lines = TestHelper.TestLines;

        var textbox = TestHelper.MakeTextbox(0);
        textbox.LoadLines(lines, true, LineEnding.LF);

        Assert.IsTrue(textbox.SelectLines(0, 3));
        Assert.AreEqual(string.Join("\n", lines.Take(3)), textbox.SelectedText);
        Assert.AreEqual(2, textbox.CursorPosition.LineNumber);
        Assert.AreEqual(lines[2].Length, textbox.CursorPosition.CharacterPosition);

        Assert.IsTrue(textbox.SelectLines(2, 3));
        Assert.AreEqual(string.Join("\n", lines.Skip(2).Take(3)), textbox.SelectedText);
        Assert.AreEqual(4, textbox.CursorPosition.LineNumber);
        Assert.AreEqual(lines[4].Length, textbox.CursorPosition.CharacterPosition);

        Assert.IsTrue(textbox.SelectLines(0, 5));
        Assert.AreEqual(string.Join("\n", lines), textbox.SelectedText);
        Assert.AreEqual(4, textbox.CursorPosition.LineNumber);
        Assert.AreEqual(lines[4].Length, textbox.CursorPosition.CharacterPosition);

        Assert.IsFalse(textbox.SelectLines(5, 10));
        Assert.IsFalse(textbox.SelectLines(-5, -10));
        Assert.IsFalse(textbox.SelectLines(0, 20));
        Assert.IsFalse(textbox.SelectLines(0, -20));
    }


    [UITestMethod]
    public void Test_SelectLine()
    {
        string[] lines = TestHelper.TestLines;
        var textbox = TestHelper.MakeTextbox(0);
        textbox.LoadLines(lines, true, LineEnding.LF);

        Assert.IsTrue(textbox.SelectLine(0));
        Assert.AreEqual(lines[0] + "\n", textbox.SelectedText);
        Assert.AreEqual(0, textbox.CursorPosition.LineNumber);
        Assert.AreEqual(0, textbox.CursorPosition.CharacterPosition);
        Assert.IsTrue(textbox.HasSelection);

        Assert.IsTrue(textbox.SelectLine(2));
        Assert.AreEqual(lines[2] + "\n", textbox.SelectedText);
        Assert.AreEqual(2, textbox.CursorPosition.LineNumber);
        Assert.AreEqual(0, textbox.CursorPosition.CharacterPosition);
        Assert.IsTrue(textbox.HasSelection);

        Assert.IsTrue(textbox.SelectLine(4));
        Assert.AreEqual(lines[4] + "\n", textbox.SelectedText);
        Assert.AreEqual(4, textbox.CursorPosition.LineNumber);
        Assert.AreEqual(0, textbox.CursorPosition.CharacterPosition);
        Assert.IsTrue(textbox.HasSelection);

        Assert.IsFalse(textbox.SelectLine(10));
        Assert.IsFalse(textbox.SelectLine(-10));
    }

    [UITestMethod]
    public void Test_GoToLine()
    {
        var (textbox, text) = TestHelper.MakeCoreTextboxWithText();
        textbox.LineEnding = LineEnding.LF;
        var lines = text.Split("\n");

        Assert.IsTrue(textbox.GoToLine(0));
        Assert.AreEqual(0, textbox.CursorPosition.LineNumber);
        Assert.AreEqual(0, textbox.CursorPosition.CharacterPosition);

        Assert.IsTrue(textbox.GoToLine(99));
        Assert.AreEqual(99, textbox.CursorPosition.LineNumber);
        Assert.AreEqual(0, textbox.CursorPosition.CharacterPosition);

        Assert.IsFalse(textbox.GoToLine(100));
        Assert.IsFalse(textbox.GoToLine(-50));
    }

    [UITestMethod]
    public void Test_LoadText()
    {
        var textbox = TestHelper.MakeTextbox(0);
        textbox.LoadText("");
        Assert.AreEqual("", textbox.GetText());

        textbox.LoadText(null);
        Assert.AreEqual("", textbox.GetText());

        textbox.LoadText("    Hello World\nTree1", autodetectTabsSpaces: true);
        Assert.AreEqual("    Hello World\nTree1", textbox.GetText());
        Assert.AreEqual(4, textbox.NumberOfSpacesForTab);
        Assert.IsTrue(textbox.UseSpacesInsteadTabs);

        textbox.UseSpacesInsteadTabs = true;
        textbox.NumberOfSpacesForTab = 2;
        textbox.LoadText("\tHello World\nTree1", autodetectTabsSpaces: false);
        Assert.AreEqual("\tHello World\nTree1", textbox.GetText());
        Assert.AreEqual(2, textbox.NumberOfSpacesForTab);
        Assert.IsTrue(textbox.UseSpacesInsteadTabs);

        textbox.SetText("Hello Test 123");
        textbox.LoadText("Hello Test");

        Assert.IsFalse(textbox.CanUndo);
        Assert.IsFalse(textbox.CanRedo);

        textbox.Undo();

        Assert.AreEqual("Hello Test", textbox.GetText());

        textbox.LoadText("Test Text\nTestText2\nEND");
        Assert.AreEqual(2, textbox.CursorPosition.LineNumber);
        Assert.AreEqual(3, textbox.CursorPosition.CharacterPosition);
    }

    [UITestMethod]
    public void Test_ClearSelection()
    {
        var textbox = TestHelper.MakeTextbox(100);
        textbox.SelectAll();

        //validate selection exists
        Assert.IsTrue(textbox.CurrentSelectionOrdered.HasValue);
        Assert.AreEqual(0, textbox.CurrentSelectionOrdered.Value.StartLinePos);
        Assert.AreEqual(0, textbox.CurrentSelectionOrdered.Value.StartCharacterPos);
        Assert.AreEqual(99, textbox.CursorPosition.LineNumber);
        Assert.AreEqual(99, textbox.CurrentSelectionOrdered.Value.EndLinePos);

        textbox.ClearSelection();

        //validate no selection exists anymore
        Assert.IsFalse(textbox.CurrentSelectionOrdered.HasValue);
        Assert.AreEqual(99, textbox.CursorPosition.LineNumber);
    }

    [UITestMethod]
    public void Test_GetLinesText()
    {
        var (textbox, text) = TestHelper.MakeCoreTextboxWithText(LineEnding.LF);
        var lines = text.Split("\n");
        int totalCount = lines.Length; 

        // 1. Standard Case: Get a middle chunk of text
        string middleLines = textbox.GetLinesText(5, 3);
        string expectedMiddle = string.Join(textbox.textManager.NewLineCharacter, lines.Skip(5).Take(3));
        Assert.AreEqual(expectedMiddle, middleLines);

        // 2. Edge Case: Get the very first line
        string firstLine = textbox.GetLinesText(0, 1);
        Assert.AreEqual(lines[0], firstLine);

        // 3. Edge Case: Get all lines (Start 0, Count = Total)
        string allLines = textbox.GetLinesText(0, totalCount);
        Assert.AreEqual(textbox.textManager.GetLinesAsString(), allLines);

        // 4. Edge Case: Count is 0 (Start + Count == 0)
        // The code has a specific check: if (start + count == 0) return "";
        Assert.AreEqual("", textbox.GetLinesText(0, 0));

        // 5. Boundary Case: Get the last line only
        string lastLine = textbox.GetLinesText(totalCount - 1, 1);
        Assert.AreEqual(lines[totalCount - 1], lastLine);

        //out of range
        Assert.ThrowsExactly<IndexOutOfRangeException>(() => {
            textbox.GetLinesText(-5, -10);
        });
        
        Assert.ThrowsExactly<IndexOutOfRangeException>(() => {
            textbox.GetLinesText(5, -10);
        });

        Assert.ThrowsExactly<IndexOutOfRangeException>(() => {
            textbox.GetLinesText(-5, 10);
        });

        // 7. Error Case: Out of Range (Starting too high)
        Assert.ThrowsExactly<IndexOutOfRangeException>(() => {
            textbox.GetLinesText(totalCount, 1);
        });
    }
    [UITestMethod]
    public void Test_SetLineText()
    {
        var textbox = TestHelper.MakeTextbox(5);

        var text = "Hello World";
        Assert.IsTrue(textbox.SetLineText(3, text));
        Assert.AreEqual(text, textbox.GetLineText(3));

        Assert.IsFalse(textbox.SetLineText(100, text));
        Assert.IsFalse(textbox.SetLineText(-100, text));

        //do not allow strings with newline characters
        Assert.ThrowsExactly<ArgumentException>(() => {
            textbox.SetLineText(3, "Hello\nBye");
        });
        Assert.ThrowsExactly<ArgumentException>(() => {
            textbox.SetLineText(3, "Hello\r\nBye");
        });
        Assert.ThrowsExactly<ArgumentException>(() => {
            textbox.SetLineText(3, "Hello\rBye");
        });
    }

    [UITestMethod]
    public void Test_SurroundSelection()
    {
        string[] lines = TestHelper.TestLines;
        var textbox = TestHelper.MakeTextbox(0);
        textbox.LoadLines(lines, true, LineEnding.LF);

        textbox.SelectLines(1,3);

        string surStr1 = "<div>";
        string surStr2 = "</div>";

        textbox.SurroundSelectionWith(surStr1, surStr2);

        Assert.AreEqual(string.Join("\n", [lines[0], surStr1 + lines[1], lines[2], lines[3] + surStr2, lines[4]]), textbox.GetText());
        Assert.AreEqual(3, textbox.CursorPosition.LineNumber);
        Assert.AreEqual(lines[3].Length + surStr2.Length, textbox.CursorPosition.CharacterPosition);
    }

    [UITestMethod]
    public void Test_SurroundSelectionSingleParam()
    {
        string[] lines = TestHelper.TestLines;
        var textbox = TestHelper.MakeTextbox(0);
        textbox.LoadLines(lines, true, LineEnding.LF);

        textbox.SelectLines(1, 3);

        string surStr = "<div>";

        textbox.SurroundSelectionWith(surStr);

        Assert.AreEqual(string.Join("\n", [lines[0], surStr + lines[1], lines[2], lines[3] + surStr, lines[4]]), textbox.GetText());
        Assert.AreEqual(3, textbox.CursorPosition.LineNumber);
        Assert.AreEqual(lines[3].Length + surStr.Length, textbox.CursorPosition.CharacterPosition);
    }

}
