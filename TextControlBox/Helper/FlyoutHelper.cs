using Microsoft.UI.Xaml.Controls;
using System;
using TextControlBoxNS.Text;

namespace TextControlBoxNS.Helper
{
    internal class FlyoutHelper
    {
        public MenuFlyout MenuFlyout;

        public void Init(CoreTextControlBox sender)
        {
            CreateFlyout(sender);
        }

        public void CreateFlyout(CoreTextControlBox sender)
        {
            MenuFlyout = new MenuFlyout();
            MenuFlyout.Items.Add(CreateItem(() => { sender.Copy(); }, "Copy", Symbol.Copy, "Ctrl + C"));
            MenuFlyout.Items.Add(CreateItem(() => { sender.Paste(); }, "Paste", Symbol.Paste, "Ctrl + V"));
            MenuFlyout.Items.Add(CreateItem(() => { sender.Cut(); }, "Cut", Symbol.Cut, "Ctrl + X"));
            MenuFlyout.Items.Add(new MenuFlyoutSeparator());
            MenuFlyout.Items.Add(CreateItem(() => { sender.Undo(); }, "Undo", Symbol.Undo, "Ctrl + Z"));
            MenuFlyout.Items.Add(CreateItem(() => { sender.Redo(); }, "Redo", Symbol.Redo, "Ctrl + Y"));
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
