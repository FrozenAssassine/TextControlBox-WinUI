using System;
using System.Diagnostics;
using TextControlBoxNS.Core;

namespace TextControlBoxNS.Test;

internal class EndUserFunctionsTest : TestCase
{
    public CoreTextControlBox coreTextbox;

    public EndUserFunctionsTest(string name, CoreTextControlBox coreTextbox)
    {
        this.coreTextbox = coreTextbox;
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
            ];
    }

    public bool Test_1()
    {
        Debug.WriteLine("End User Function AddLine with out of range line");

        //addline should return false, line 100 does not exist
        bool res = !coreTextbox.AddLine(100, "Test Text");

        return res;
    }

    public bool Test_2()
    {
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
        Debug.WriteLine("Function GetText without text");

        coreTextbox.SetText("");

        var lines = coreTextbox.textManager.totalLines;

        var text = coreTextbox.GetText();

        return text.Length == 0;
    }

    public bool Test_5()
    {
        Debug.WriteLine("Function SetCursorPosition too high");

        coreTextbox.SetCursorPosition(500, 1000, true);

        return coreTextbox.CursorPosition.LineNumber != 500 && coreTextbox.CursorPosition.CharacterPosition != 1000;
    }

    public bool Test_6()
    {
        Debug.WriteLine("Function SetCursorPosition negative");

        coreTextbox.SetCursorPosition(-100, -500, true);

        return coreTextbox.CursorPosition.LineNumber != -100 && coreTextbox.CursorPosition.CharacterPosition != -500;
    }

    public bool Test_7()
    {
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
        Debug.WriteLine("Function get Selected Text equals text in textbox");

        var text = "Line1\nLine2\nLine3\n";

        coreTextbox.SetText(text);

        coreTextbox.SelectAll();
        //should be no selection
        return coreTextbox.SelectedText.Equals(coreTextbox.stringManager.CleanUpString(text));
    }

    public bool Test_9()
    {
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
        Debug.WriteLine("Function LoadText empty");
        coreTextbox.LoadText("");

        return coreTextbox.GetText().Length == 0 &&
            coreTextbox.CursorPosition.LineNumber == 0 &&
            coreTextbox.CursorPosition.CharacterPosition == 0;
    }

    public bool Test_13()
    {
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
}
