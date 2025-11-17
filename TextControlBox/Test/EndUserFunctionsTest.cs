using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.ExceptionServices;
using TextControlBoxNS.Core;

namespace TextControlBoxNS.Test;

internal class EndUserFunctionsTest : TestCase
{
    private TextControlBox textbox;
    public CoreTextControlBox coreTextbox;

    public EndUserFunctionsTest(string name, CoreTextControlBox coreTextbox, TextControlBox textbox)
    {
        this.coreTextbox = coreTextbox;
        this.textbox = textbox;
        coreTextbox.LineEnding = LineEnding.LF;
        this.name = name;
    }

    public override string name { get; set; }

    public override Func<bool>[] GetAllTests()
    {
        return [
            this.Test_1,
            this.Test_2,
            this.Test_3,
            this.Test_4,
            this.Test_5,
            this.Test_6,
            this.Test_7,
            this.Test_8,
            this.Test_9,
            this.Test_10,
            this.Test_11,
            this.Test_12,
            this.Test_13,
            this.Test_14,
            this.Test_15,
            this.Test_16,
            this.Test_17,
            this.Test_18,
            this.Test_19,
            this.Test_20,
            this.Test_21,
            this.Test_22,
            this.Test_23,
            this.Test_24,
            this.Test_25,
            this.Test_26,
            this.Test_27,
            this.Test_28,
            this.Test_29,
            this.Test_30,
            this.Test_31,
            this.Test_32,
            this.Test_33,
            this.Test_34,
            this.Test_35,
            ];
    }

    public bool Test_1()
    {
        TestHelper.ResetContent(textbox, 20);

        Debug.WriteLine("End User Function AddLine with out of range line");

        //addline should return false, line 100 does not exist
        bool res = !coreTextbox.AddLine(100, "Test Text");

        return res;
    }

    public bool Test_2()
    {
        TestHelper.ResetContent(textbox, 10);
        Debug.WriteLine("Function GetLinesText with out of range line");

        try
        {
            coreTextbox.GetLinesText(100, 10);
        }
        catch (IndexOutOfRangeException)
        {
            return true;
        }
        return false;
    }

    public bool Test_3()
    {
        TestHelper.ResetContent(textbox, 30);
        Debug.WriteLine("Function GetLineText with out of range line");

        try
        {
            coreTextbox.GetLineText(100);
        }
        catch (IndexOutOfRangeException)
        {
            return true;
        }
        return false;
    }

    public bool Test_4()
    {
        TestHelper.ResetContent(textbox, 0);
        Debug.WriteLine("Function GetText without text");

        coreTextbox.SetText("");

        var lines = coreTextbox.textManager.totalLines;

        var text = coreTextbox.GetText();

        return text.Length == 0;
    }

    public bool Test_5()
    {
        TestHelper.ResetContent(textbox, 10);
        Debug.WriteLine("Function SetCursorPosition too high");

        coreTextbox.SetCursorPosition(500, 1000, true);

        return coreTextbox.CursorPosition.LineNumber != 500 && coreTextbox.CursorPosition.CharacterPosition != 1000;
    }

    public bool Test_6()
    {
        TestHelper.ResetContent(textbox, 10);
        Debug.WriteLine("Function SetCursorPosition negative");

        coreTextbox.SetCursorPosition(-100, -500, true);

        return coreTextbox.CursorPosition.LineNumber != -100 && coreTextbox.CursorPosition.CharacterPosition != -500;
    }

    public bool Test_7()
    {
        TestHelper.ResetContent(textbox, 10);
        Debug.WriteLine("Function SetSelection too high");

        try
        {
            coreTextbox.SetSelection(10000, 5000);
        }
        catch
        {
            return false;
        }
        //should be no selection
        return !coreTextbox.CurrentSelectionOrdered.HasValue && !coreTextbox.CurrentSelection.HasValue;
    }

    public bool Test_8()
    {
        TestHelper.ResetContent(textbox, 0);
        Debug.WriteLine("Function get Selected Text equals text in textbox");

        const string text = "Line1\nLine2\nLine3\n";
        coreTextbox.SetText(text);

        coreTextbox.SelectAll();
        //should be no selection
        return coreTextbox.SelectedText.Equals(coreTextbox.stringManager.CleanUpString(text));
    }

    public bool Test_9()
    {
        TestHelper.ResetContent(textbox, 0);
        Debug.WriteLine("Function set Selected Text no text in textbox equals text in textbox");
        coreTextbox.SetText("");

        var text = "Line1\nLine2\nLine3\n";

        coreTextbox.SelectedText = text;

        //should be no selection
        return coreTextbox.GetText().Equals(coreTextbox.stringManager.CleanUpString(text));
    }

    public bool Test_10()
    {
        Debug.WriteLine("Function SetText empty");
        coreTextbox.SetText("");

        return coreTextbox.GetText().Length == 0 &&
            coreTextbox.CursorPosition.LineNumber == 0 &&
            coreTextbox.CursorPosition.CharacterPosition == 0;
    }

    public bool Test_11()
    {
        Debug.WriteLine("Function LoadLines empty");
        coreTextbox.LoadLines([""]);

        return coreTextbox.GetText().Length == 0 &&
            coreTextbox.CursorPosition.LineNumber == 0 &&
            coreTextbox.CursorPosition.CharacterPosition == 0;
    }

    public bool Test_12()
    {
        TestHelper.ResetContent(textbox, 0);
        Debug.WriteLine("Function LoadText empty");
        coreTextbox.LoadText("");

        return coreTextbox.GetText().Length == 0 &&
            coreTextbox.CursorPosition.LineNumber == 0 &&
            coreTextbox.CursorPosition.CharacterPosition == 0;
    }

    public bool Test_13()
    {
        TestHelper.ResetContent(textbox, 0);

        Debug.WriteLine("Function select all, set SelectedText equals text in textbox");
        var text = "Line1\nLine2\nLine3\n";

        coreTextbox.SetText("Line100\nLine200\nLine300\n");

        coreTextbox.SelectAll();

        coreTextbox.SelectedText = text;

        return coreTextbox.GetText().Equals(coreTextbox.stringManager.CleanUpString(text));
    }

    public bool Test_14()
    {
        Debug.WriteLine("Function SetText");
        var text = "Line1\nLine2\nLine3\n";

        coreTextbox.SetText(text);

        return coreTextbox.GetText().Equals(coreTextbox.stringManager.CleanUpString(text));
    }
    public bool Test_15()
    {
        Debug.WriteLine("Function SetText 3 lines");
        var text = "Line1\nLine2\nLine3";

        coreTextbox.SetText(text);

        return coreTextbox.GetText().Equals(coreTextbox.stringManager.CleanUpString(text)) &&
            coreTextbox.cursorManager.LineNumber == 2 && coreTextbox.cursorManager.CharacterPosition == 5;
    }
    public bool Test_16()
    {
        Debug.WriteLine("Function LoadText 3 lines");
        var text = "Line1\nLine2\nLine3";

        coreTextbox.LoadText(text);

        return coreTextbox.GetText().Equals(coreTextbox.stringManager.CleanUpString(text)) &&
            coreTextbox.cursorManager.LineNumber == 2 && coreTextbox.cursorManager.CharacterPosition == 5;
    }
    public bool Test_17()
    {
        Debug.WriteLine("Function LoadLines 3 lines");
        var text = "Line1\nLine2\nLine3";

        coreTextbox.LoadLines(["Line1", "Line2", "Line3"]);

        return coreTextbox.GetText().Equals(coreTextbox.stringManager.CleanUpString(text)) &&
            coreTextbox.cursorManager.LineNumber == 2 && coreTextbox.cursorManager.CharacterPosition == 5;
    }
    public bool Test_18()
    {
        Debug.WriteLine("Function SelectSyntaxHighlightingById");

        coreTextbox.LoadLines(["Line1", "Line2", "Line3"]);

        try
        {
            foreach (SyntaxHighlightID item in Enum.GetValues(typeof(SyntaxHighlightID)))
            {
                coreTextbox.SelectSyntaxHighlightingById(item);
            }
        }
        catch
        {
            return false;
        }

        return true;
    }

    public bool Test_19()
    {
        Debug.WriteLine("Static Function GetSyntaxHighlightingFromID");

        TestHelper.ResetContent(textbox);

        try
        {
            foreach (SyntaxHighlightID item in Enum.GetValues(typeof(SyntaxHighlightID)))
            {
                coreTextbox.SyntaxHighlighting = TextControlBox.GetSyntaxHighlightingFromID(item);
            }

            coreTextbox.SyntaxHighlighting = null;
        }
        catch
        {
            return false;
        }

        return true;
    }

    public bool Test_20()
    {
        Debug.WriteLine("Search Replace All");

        coreTextbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");

        string textBefore = coreTextbox.GetText();
        coreTextbox.ReplaceAll("line", "Test", false, false);

        bool res = coreTextbox.GetText().Equals(textBefore.Replace("Line", "Test"));
        return res;
    }

    public bool Test_21()
    {
        Debug.WriteLine("Search Replace All (Match Case)");

        coreTextbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");

        string textBefore = coreTextbox.GetText();

        //should replace nothing
        coreTextbox.ReplaceAll("line", "Test", true, false);
        bool res1 = coreTextbox.GetText().Equals(textBefore);

        coreTextbox.ReplaceAll("Line", "Test", true, false);

        bool res2 = coreTextbox.GetText().Equals(textBefore.Replace("Line", "Test"));
        return res1 && res2;
    }

    public bool Test_22()
    {
        Debug.WriteLine("Search Replace All (Whole word)");

        //nothign should change
        coreTextbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");
        string textBefore = coreTextbox.GetText();
        coreTextbox.ReplaceAll("Line", "Test", false, true);

        bool res1 = coreTextbox.GetText().Equals(textBefore);

        //replace them
        coreTextbox.SetText("Line 1\nLine 2\nLine 3\nLine 4\nLine 5");

        textBefore = coreTextbox.GetText();
        coreTextbox.ReplaceAll("Line", "Test", false, true);

        bool res2 = coreTextbox.GetText().Equals(textBefore.Replace("Line", "Test"));
        return res1 && res2;
    }

    public bool Test_23()
    {
        Debug.WriteLine("Search Replace next");

        coreTextbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");
        coreTextbox.SetCursorPosition(0, 0);

        coreTextbox.BeginSearch("Line", false, false);
        string textBefore = coreTextbox.GetText();
        for (int i = 0; i < 5; i++)
        {
            coreTextbox.ReplaceNext("Test");
        }

        return coreTextbox.GetText().Equals(textBefore.Replace("Line", "Test")) && !coreTextbox.selectionManager.HasSelection && !coreTextbox.HasSelection;
    }

    public bool Test_24()
    {
        Debug.WriteLine("Search Find next");

        coreTextbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");
        coreTextbox.SetCursorPosition(0, 0);

        coreTextbox.BeginSearch("Line", false, false);
        string textBefore = coreTextbox.GetText();

        bool invalidRes = false;
        for (int i = 0; i < 5; i++)
        {
            if (coreTextbox.FindNext() != SearchResult.Found)
                invalidRes = true;
        }
        return !invalidRes;
    }

    public bool Test_25()
    {
        Debug.WriteLine("Search Find previous");

        coreTextbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");
        coreTextbox.SetCursorPosition(5, 5);

        coreTextbox.BeginSearch("Line", false, false);
        string textBefore = coreTextbox.GetText();

        bool invalidRes = false;
        for (int i = 0; i < 5; i++)
        {
            if (coreTextbox.FindPrevious() != SearchResult.Found)
                invalidRes = true;
        }
        return !invalidRes;
    }

    public bool Test_26()
    {
        Debug.WriteLine("Search Replace All with nothing");

        coreTextbox.SetText("Line1\nLine2\nLine3\nLine4\nLine5");

        string textBefore = coreTextbox.GetText();
        coreTextbox.ReplaceAll("line", "", false, false);

        bool res = coreTextbox.GetText().Equals(textBefore.Replace("Line", ""));
        return res;
    }

    public bool Test_27()
    {
        Debug.WriteLine("TextLoadedEvent");

        const string textToLoad = "Hello World";
        bool eventTriggered = false;

        void Textbox_TextLoaded(TextControlBox sender)
        {
            eventTriggered = true;
        }

        textbox.TextLoaded += Textbox_TextLoaded;
        coreTextbox.LoadText(textToLoad);

        bool res = eventTriggered && coreTextbox.GetText() == textToLoad;
        textbox.TextLoaded -= Textbox_TextLoaded;

        return res;
    }
    public bool Test_28()
    {
        Debug.WriteLine("Test TabsSpacesChangedEvent");
        bool[] success = new  bool[4];
        int testCase = 0;
        void Textbox_TabsSpacesChanged(TextControlBox sender, bool spacesInsteadTabs, int spaces)
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
        catch (ArgumentOutOfRangeException _)
        {
            exceptionThrownCount++;
        }
        try
        {
            textbox.NumberOfSpacesForTab = -1;
        }
        catch (ArgumentOutOfRangeException _)
        {
            exceptionThrownCount++;
        }

        textbox.TabsSpacesChanged -= Textbox_TabsSpacesChanged;
        return success.Any(item => item == true) && testCase == 4 && exceptionThrownCount == 2;
    }

    public bool Test_29()
    {
        Debug.WriteLine("Test LineEndingChangedEvent");
        bool[] success = new bool[3];
        int testCase = 0;
        void Textbox_LineEndingChanged(TextControlBox sender, LineEnding lineEnding)
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

        return success.Any(item => item == true);
    }
    
    public bool Test_30()
    {
        TestHelper.ResetContent(textbox, 0);
        Debug.WriteLine("Loadlines with tab, detect tabsspaces (tabs)");

        coreTextbox.LoadLines(["Line1", "\tLine2", "\t\tLine3"]);

        //4 is the default value
        return coreTextbox.UseSpacesInsteadTabs == false && coreTextbox.NumberOfSpacesForTab == 4;
    }
    public bool Test_31()
    {
        TestHelper.ResetContent(textbox, 0);
        Debug.WriteLine("Loadlines with tab, detect tabsspaces (4 spaces)");

        coreTextbox.LoadLines(["Line1", "    Line2", "        Line3"]);

        //4 is the default value
        return coreTextbox.UseSpacesInsteadTabs == true && coreTextbox.NumberOfSpacesForTab == 4;
    }
    public bool Test_32()
    {
        TestHelper.ResetContent(textbox, 0);
        Debug.WriteLine("Loadlines with tab, detect tabsspaces (8 spaces)");

        coreTextbox.LoadLines(["Line1", "        Line2", "                Line3"]);

        //4 is the default value
        return coreTextbox.UseSpacesInsteadTabs == true && coreTextbox.NumberOfSpacesForTab == 8;
    }
    public bool Test_33()
    {
        TestHelper.ResetContent(textbox, 0);
        Debug.WriteLine("Loadtext with tab, detect tabsspaces (4 spaces)");

        coreTextbox.LoadText("Line1\n        Line2\n                Line3\n");

        //4 is the default value
        return coreTextbox.UseSpacesInsteadTabs == true && coreTextbox.NumberOfSpacesForTab == 8;
    }
    public bool Test_34()
    {
        TestHelper.ResetContent(textbox, 0);
        Debug.WriteLine("Loadtext with tab, detect tabsspaces (tabs)");

        coreTextbox.LoadText("Line1\n\tLine2\n\t\tLine3\n");

        //4 is the default value
        return coreTextbox.UseSpacesInsteadTabs == false && coreTextbox.NumberOfSpacesForTab == 4;
    }
    public bool Test_35()
    {
        TestHelper.ResetContent(textbox, 0);
        Debug.WriteLine("Loadtext with tab, detect tabsspaces (2 spaces)");

        coreTextbox.LoadText("Line1\n  Line2\n    Line3\n");

        //4 is the default value
        return coreTextbox.UseSpacesInsteadTabs == true && coreTextbox.NumberOfSpacesForTab == 2;
    }
}
