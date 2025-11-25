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
    private TextControlBoxNS.TextControlBox MakeTextbox(int addNewLines = 100)
    {
        TextControlBoxNS.TextControlBox textbox = new TextControlBoxNS.TextControlBox();

        if (addNewLines == 0)
        {
            textbox.LoadLines([]);
        }
        textbox.LoadLines(Enumerable.Range(0, addNewLines).Select(x => "Line " + x + " is cool right?"));
        return textbox;
    }

    [UITestMethod]
    public void Test_1()
    {
        var textbox = MakeTextbox(20);

        Debug.WriteLine("End User Function AddLine with out of range line");

        //addline should return false, line 100 does not exist
        bool res = !textbox.AddLine(100, "Test Text");

        Debug.Assert(res);
    }

    [UITestMethod]
    public void Test_2()
    {
        var textbox = MakeTextbox(10);
        Debug.WriteLine("Function GetLinesText with out of range line");

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
    public void Test_3()
    {
        var textbox = MakeTextbox();
        Debug.WriteLine("Function GetLineText with out of range line");

        try
        {
            textbox.GetLineText(100);
        }
        catch (IndexOutOfRangeException)
        {
            return;
        }
        Debug.Assert(false);
    }

    [UITestMethod]
    public void Test_4()
    {
        var textbox = MakeTextbox();
        Debug.WriteLine("Function GetText without text");

        textbox.SetText("");

        var text = textbox.GetText();

        Debug.Assert(text.Length == 0);
    }

    [UITestMethod]
    public void Test_5()
    {
        var textbox = MakeTextbox();

        Debug.WriteLine("Function SetCursorPosition too high");

        textbox.SetCursorPosition(500, 1000, true);

        Debug.Assert(textbox.CursorPosition.LineNumber != 500 && textbox.CursorPosition.CharacterPosition != 1000);
    }

    [UITestMethod]
    public void Test_6()
    {
        var textbox = MakeTextbox(10);

        Debug.WriteLine("Function SetCursorPosition negative");

        textbox.SetCursorPosition(-100, -500, true);

        Debug.Assert(textbox.CursorPosition.LineNumber != -100 && textbox.CursorPosition.CharacterPosition != -500);
    }

    [UITestMethod]
    public void Test_7()
    {
        var textbox = MakeTextbox(10);
        Debug.WriteLine("Function SetSelection too high");

        try
        {
            textbox.SetSelection(10000, 5000);
        }
        catch
        {
            Debug.Assert(false);
        }
        //should be no selection
        Debug.Assert(!textbox.CurrentSelectionOrdered.HasValue && !textbox.CurrentSelection.HasValue);
    }

    [UITestMethod]
    public void Test_8()
    {
        var textbox = MakeTextbox(10);
        Debug.WriteLine("Function get Selected Text equals text in textbox");

        const string text = "Line1\nLine2\nLine3\n";
        textbox.SetText(text);

        textbox.SelectAll();

        Debug.Assert(textbox.SelectedText.Equals(LineEndings.CleanLineEndings(text, textbox.LineEnding)));
    }

    [UITestMethod]
    public void Test_9()
    {
        var textbox = MakeTextbox(0);
        Debug.WriteLine("Function set Selected Text no text in textbox equals text in textbox");
        textbox.SetText("");

        var text = "Line1\nLine2\nLine3\n";

        textbox.SelectedText = text;

        //should be no selection
        Debug.Assert(textbox.GetText().Equals(LineEndings.CleanLineEndings(text, textbox.LineEnding)));
    }
    [UITestMethod]

    public void Test_10()
    {
        var textbox = MakeTextbox(10);

        Debug.WriteLine("Function SetText empty");
        textbox.SetText("");

        Debug.Assert(textbox.GetText().Length == 0 &&
            textbox.CursorPosition.LineNumber == 0 &&
            textbox.CursorPosition.CharacterPosition == 0);
    }

    public void Test_11()
    {
        var textbox = MakeTextbox(10);
        Debug.WriteLine("Function LoadLines empty");
        textbox.LoadLines([""]);

        Debug.Assert(textbox.GetText().Length == 0 &&
            textbox.CursorPosition.LineNumber == 0 &&
            textbox.CursorPosition.CharacterPosition == 0);
    }

    [UITestMethod]
    public void Test_12()
    {
        var textbox = MakeTextbox(0);
        Debug.WriteLine("Function LoadText empty");
        textbox.LoadText("");

        bool res = textbox.GetText().Length == 0 &&
            textbox.CursorPosition.LineNumber == 0 &&
            textbox.CursorPosition.CharacterPosition == 0;
        Debug.Assert(res);
    }

    [UITestMethod]
    public void Test_13()
    {
        var textbox = MakeTextbox(0);

        Debug.WriteLine("Function select all, set SelectedText equals text in textbox");
        var text = "Line1\nLine2\nLine3\n";

        textbox.SetText("Line100\nLine200\nLine300\n");

        textbox.SelectAll();

        textbox.SelectedText = text;

        Debug.Assert(textbox.GetText().Equals(LineEndings.CleanLineEndings(text, textbox.LineEnding)));
    }

    [UITestMethod]
    public void Test_14()
    {
        var textbox = MakeTextbox(10);

        Debug.WriteLine("Function SetText");
        var text = "Line1\nLine2\nLine3\n";

        textbox.SetText(text);

        bool res = textbox.GetText().Equals(LineEndings.CleanLineEndings(text, textbox.LineEnding));
        Debug.Assert(res);
    }
    [UITestMethod]
    public void Test_15()
    {
        var textbox = MakeTextbox(10);

        Debug.WriteLine("Function SetText 3 lines");
        var text = "Line1\nLine2\nLine3";

        textbox.SetText(text);

        bool res = textbox.GetText().Equals(LineEndings.CleanLineEndings(text, textbox.LineEnding)) &&
            textbox.CursorPosition.LineNumber == 2 && textbox.CursorPosition.CharacterPosition == 5;
        Debug.Assert(res);
    }
    [UITestMethod]
    public void Test_16()
    {
        var textbox = MakeTextbox(10);

        Debug.WriteLine("Function LoadText 3 lines");
        var text = "Line1\nLine2\nLine3";

        textbox.LoadText(text);

        bool res = textbox.GetText().Equals(LineEndings.CleanLineEndings(text, textbox.LineEnding)) &&
            textbox.CursorPosition.LineNumber == 2 && textbox.CursorPosition.CharacterPosition == 5;
        Debug.Assert(res);
    }
    [UITestMethod]
    public void Test_17()
    {
        var textbox = MakeTextbox(10);

        Debug.WriteLine("Function LoadLines 3 lines");
        var text = "Line1\nLine2\nLine3";

        textbox.LoadLines(["Line1", "Line2", "Line3"]);

        bool res = textbox.GetText().Equals(LineEndings.CleanLineEndings(text, textbox.LineEnding)) &&
            textbox.CursorPosition.LineNumber == 2 && textbox.CursorPosition.CharacterPosition == 5;
        Debug.Assert(res);
    }
    [UITestMethod]
    public void Test_18()
    {
        var textbox = MakeTextbox(10);

        Debug.WriteLine("Function SelectSyntaxHighlightingById");

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
    public void Test_19()
    {
        var textbox = MakeTextbox(10);

        Debug.WriteLine("Static Function GetSyntaxHighlightingFromID");

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
        var textbox = MakeTextbox(100);
        Debug.WriteLine("");

        textbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");

        string textBefore = textbox.GetText();
        textbox.ReplaceAll("line", "Test", false, false);

        Debug.Assert(textbox.GetText().Equals(textBefore.Replace("Line", "Test")));
    }
    [UITestMethod]
    public void Test_21()
    {
        var textbox = MakeTextbox(100);
        Debug.WriteLine("Search Replace All (Match Case)");

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
    public void Test_22()
    {
        var textbox = MakeTextbox(100);

        Debug.WriteLine("Search Replace All (Whole word)");

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
    public void Test_23()
    {
        var textbox = MakeTextbox(100);

        Debug.WriteLine("Search Replace next");

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
    public void Test_24()
    {
        var textbox = MakeTextbox(100);

        Debug.WriteLine("Search Find next");

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
    public void Test_25()
    {
        var textbox = MakeTextbox(100);

        Debug.WriteLine("Search Find previous");

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
    public void Test_26()
    {
        var textbox = MakeTextbox(100);

        Debug.WriteLine("Search Replace All with nothing");

        textbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");

        string textBefore = textbox.GetText();
        textbox.ReplaceAll("line", "", false, false);

        bool res = textbox.GetText().Equals(textBefore.Replace("Line", ""));
        Debug.Assert(res);
    }

    [UITestMethod]
    public void Test_27()
    {
        var textbox = MakeTextbox(100);

        Debug.WriteLine("TextLoadedEvent");

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
    public void Test_28()
    {
        var textbox = MakeTextbox(100);

        Debug.WriteLine("Test TabsSpacesChangedEvent");
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
    public void Test_29()
    {
        var textbox = MakeTextbox(100);

        Debug.WriteLine("Test LineEndingChangedEvent");
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
    public void Test_30()
    {
        var textbox = MakeTextbox(0);
        Debug.WriteLine("Loadlines with tab, detect tabsspaces (tabs)");

        textbox.LoadLines(["Line1", "\tLine2", "\t\tLine3"]);

        //4 is the default value
        Debug.Assert(textbox.UseSpacesInsteadTabs == false && textbox.NumberOfSpacesForTab == 4);
    }
    [UITestMethod]
    public void Test_31()
    {
        var textbox = MakeTextbox(0);
        Debug.WriteLine("Loadlines with tab, detect tabsspaces (4 spaces)");

        textbox.LoadLines(["Line1", "    Line2", "        Line3"]);

        //4 is the default value
        Debug.Assert(textbox.UseSpacesInsteadTabs == true && textbox.NumberOfSpacesForTab == 4);
    }
    [UITestMethod]
    public void Test_32()
    {
        var textbox = MakeTextbox(0);
        Debug.WriteLine("Loadlines with tab, detect tabsspaces (8 spaces)");

        textbox.LoadLines(["Line1", "        Line2", "                Line3"]);

        //4 is the default value
        Debug.Assert(textbox.UseSpacesInsteadTabs == true && textbox.NumberOfSpacesForTab == 8);
    }
    [UITestMethod]
    public void Test_33()
    {
        var textbox = MakeTextbox(0);
        Debug.WriteLine("Loadtext with tab, detect tabsspaces (4 spaces)");

        textbox.LoadText("Line1\n        Line2\n                Line3\n");

        //4 is the default value
        Debug.Assert(textbox.UseSpacesInsteadTabs == true && textbox.NumberOfSpacesForTab == 8);
    }
    [UITestMethod]
    public void Test_34()
    {
        var textbox = MakeTextbox(0);
        Debug.WriteLine("Loadtext with tab, detect tabsspaces (tabs)");

        textbox.LoadText("Line1\n\tLine2\n\t\tLine3\n");

        //4 is the default value
        Debug.Assert(textbox.UseSpacesInsteadTabs == false && textbox.NumberOfSpacesForTab == 4);
    }
    [UITestMethod]
    public void Test_35()
    {
        var textbox = MakeTextbox(0);
        Debug.WriteLine("Loadtext with tab, detect tabsspaces (2 spaces)");

        textbox.LoadText("Line1\n    Line2\n        Line3\n");

        //4 is the default value
        Debug.Assert(textbox.UseSpacesInsteadTabs == true && textbox.NumberOfSpacesForTab == 4);
    }
    [UITestMethod]
    public void Test_RewriteTabsSpaces_SpacesToTabs()
    {
        var textbox = MakeTextbox(100);

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
        var textbox = MakeTextbox(100);

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
        var textbox = MakeTextbox(100);

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
        var textbox = MakeTextbox(100);

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
}
