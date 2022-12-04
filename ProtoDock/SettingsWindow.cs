using ProtoDock.Core;
using System.Windows.Forms;
using System.Linq;
using ProtoDock.Api;
using System.Windows.Documents;
using ProtoDock.Settings;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;
using System.Diagnostics;

namespace ProtoDock
{
    public partial class SettingsWindow : Form, IDockSettingsContext
    {
        private readonly Dock _dock;
        private readonly SettingsDisplay _display;

        internal SettingsWindow(Dock dock) : base()
        {
            InitializeComponent();
            _dock = dock;
            _display = new SettingsDisplay(Content, Flush, SetDirty, FlashWindow);

            Register(new DockViewSettings(_dock.Graphics));
            Register(new DockPanelsSettings(_dock));

            foreach (var p in dock.Plugins)
            {
                if (p.ResolveHook<IDockPlugin.ISettingsHook>(out var settingsHook))
                {
                    settingsHook.OnSettings(this);
                }
            }

            if (Category.Items.Count > 0)
            {
                Category.SelectedItem = Category.Items[0];
            }
        }

        public void Register(IDockSettingsSource source)
        {
            Category.Items.Add(source);
        }

        private void Category_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            _display.Clear();

            var source = (IDockSettingsSource)Category.SelectedItem;
            source.Display(_display);
        }
        
        private void Flush() {
            _dock.Flush();
        }

        private void SetDirty() {
            _dock.Graphics.SetDirty();
        }

        private void FlashWindow() {
            var info = new PInvoke.User32.FLASHWINFO();
            info.cbSize = Marshal.SizeOf(info);
            info.hwnd = Handle;
            info.dwFlags = PInvoke.User32.FlashWindowFlags.FLASHW_ALL | PInvoke.User32.FlashWindowFlags.FLASHW_TIMER;
            info.uCount = 3;
            PInvoke.User32.FlashWindowEx(ref info);
        }
    }
}
