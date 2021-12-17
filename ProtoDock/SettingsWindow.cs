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
        private SettingsPanel _panel;

        internal SettingsWindow(Dock dock) : base()
        {
            InitializeComponent();
            _dock = dock;
            _panel = new SettingsPanel(dock, Content);

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

        private void Category_SelectedIndexChanged(object sender, System.EventArgs e) {
            _panel.Clear();

            var source = (IDockSettingsSource)Category.SelectedItem;            
            source.Display(_panel);
        }
    }
}
