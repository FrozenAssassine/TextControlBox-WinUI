using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using TextControlBoxNS.Core;

namespace TextControlBoxNS.Helper
{
    internal class FlyoutHelper
    {
        public MenuFlyout menuFlyout;

        public void Init(CoreTextControlBox sender)
        {
            CreateFlyout(sender);
        }

        public void CreateFlyout(CoreTextControlBox sender)
        {
            menuFlyout = new MenuFlyout();
            menuFlyout.Items.Add(CreateItem(() => { sender.Copy(); }, "Copy", Symbol.Copy, "Ctrl + C"));
            menuFlyout.Items.Add(CreateItem(() => { sender.Paste(); }, "Paste", Symbol.Paste, "Ctrl + V"));
            menuFlyout.Items.Add(CreateItem(() => { sender.Cut(); }, "Cut", Symbol.Cut, "Ctrl + X"));
            menuFlyout.Items.Add(new MenuFlyoutSeparator());
            menuFlyout.Items.Add(CreateItem(() => { sender.Undo(); }, "Undo", Symbol.Undo, "Ctrl + Z"));
            menuFlyout.Items.Add(CreateItem(() => { sender.Redo(); }, "Redo", Symbol.Redo, "Ctrl + Y"));

            menuFlyout.Closed += (_, _) => { sender.Focus(FocusState.Programmatic); };

        }

        public MenuFlyoutItem CreateItem(Action action, string text, Symbol icon, string key)
        {
            var item = new MenuFlyoutItem
            {
                Text = text,
                KeyboardAcceleratorTextOverride = key,
                Icon = new SymbolIcon { Symbol = icon }
            };
            item.Click += delegate
            {
                action();
            };
            return item;
        }
    }
}
