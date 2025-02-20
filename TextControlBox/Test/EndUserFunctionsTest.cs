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

        Debug.WriteLine("TEXT:" + text + ":END");

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
        return !coreTextbox.CurrentSelectionOrdered.HasValue;
    }
}
