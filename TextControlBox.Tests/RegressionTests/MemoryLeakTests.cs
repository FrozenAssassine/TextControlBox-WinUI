using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.AppContainer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using TextControlBoxNS.Core;

namespace TextControlBox.Tests.RegressionTests;

/// <summary>
/// A textbox that is not referenced anymore has to be collectable by the garbage collector.
/// Two things kept every instance alive forever: handlers attached to the Win2D CanvasControls (Draw and
/// the pointer events) and the Tick handlers of DispatcherQueueTimers. Both are reference cycles through
/// native code that the garbage collector can not see.
/// </summary>
[TestClass]
public class MemoryLeakTests
{
    private const int Instances = 20;

    private static int CountAlive(List<WeakReference> references)
    {
        for (int i = 0; i < 4; i++)
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        GC.Collect();
        return references.Count(x => x.IsAlive);
    }

    //the instances are created in their own method, so no local variable keeps them alive
    [MethodImpl(MethodImplOptions.NoInlining)]
    private static List<WeakReference> Create(Func<object> create)
    {
        var references = new List<WeakReference>();
        for (int i = 0; i < Instances; i++)
            references.Add(new WeakReference(create()));
        return references;
    }

    [UITestMethod]
    public void CoreTextControlBox_ConstructedOnly_IsCollected()
    {
        Assert.AreEqual(0, CountAlive(Create(() => new CoreTextControlBox())));
    }

    [UITestMethod]
    public void CoreTextControlBox_UsedWithoutDispose_IsCollected()
    {
        Assert.AreEqual(0, CountAlive(Create(() =>
        {
            var core = TestHelper.MakeCoreTextbox(50);
            core.textActionManager.AddCharacter("x");
            core.SelectLine(1);
            core.canvasUpdateManager.UpdateAll();

            //what happens when the control gets removed from the window: it loses the focus, which stops
            //the caret blink timer. A running timer keeps its textbox alive, that is intended.
            core.focusManager.RemoveFocus();
            return core;
        })));
    }

    [UITestMethod]
    public void CoreTextControlBox_Disposed_IsCollected()
    {
        Assert.AreEqual(0, CountAlive(Create(() =>
        {
            var core = TestHelper.MakeCoreTextbox(50);
            core.Dispose();
            return core;
        })));
    }

    [UITestMethod]
    public void TextControlBox_WithoutDispose_IsCollectedTogetherWithItsCore()
    {
        var cores = new List<WeakReference>();
        var textboxes = Create(() =>
        {
            var textbox = TestHelper.MakeTextbox(50);
            textbox.TextChanged += sender => { };
            cores.Add(new WeakReference(textbox.CoreTextBox));
            return textbox;
        });

        Assert.AreEqual(0, CountAlive(textboxes), "TextControlBox");
        Assert.AreEqual(0, CountAlive(cores), "CoreTextControlBox");
    }

    [UITestMethod]
    public void TextControlBox_Disposed_IsCollectedTogetherWithItsCore()
    {
        var cores = new List<WeakReference>();
        var textboxes = Create(() =>
        {
            var textbox = TestHelper.MakeTextbox(50);
            cores.Add(new WeakReference(textbox.CoreTextBox));
            textbox.Dispose();
            return textbox;
        });

        Assert.AreEqual(0, CountAlive(textboxes), "TextControlBox");
        Assert.AreEqual(0, CountAlive(cores), "CoreTextControlBox");
    }
}
