using Microsoft.UI.Xaml.Controls;
using System;

namespace TextControlBox.Helper
{
    internal class FlyoutHelper
    {
        public MenuFlyout MenuFlyout;

        public FlyoutHelper(TextControlBox sender)
        {
            CreateFlyout(sender);
        }

        public void CreateFlyout(TextControlBox sender)
        {
            //MenuFlyout = new MenuFlyout();
            //MenuFlyout.Items.Add(CreateItem(() => { sender.Copy(); }, "Copy", Symbol.Copy, "Ctrl + C"));
            //MenuFlyout.Items.Add(CreateItem(() => { sender.Paste(); }, "Paste", Symbol.Paste, "Ctrl + V"));
            //MenuFlyout.Items.Add(CreateItem(() => { sender.Cut(); }, "Cut", Symbol.Cut, "Ctrl + X"));
            //MenuFlyout.Items.Add(new MenuFlyoutSeparator());
            //MenuFlyout.Items.Add(CreateItem(() => { sender.Undo(); }, "Undo", Symbol.Undo, "Ctrl + Z"));
            //MenuFlyout.Items.Add(CreateItem(() => { sender.Redo(); }, "Redo", Symbol.Redo, "Ctrl + Y"));
        }

        public MenuFlyoutItem CreateItem(Action action, string Text, Symbol Icon, string Key)
        {
            var Item = new MenuFlyoutItem
            {
                Text = Text,
                KeyboardAcceleratorTextOverride = Key,
                Icon = new SymbolIcon { Symbol = Icon }
            };
            Item.Click += delegate
            {
                action();
            };
            return Item;
        }
    }
}
