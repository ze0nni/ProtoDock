using ProtoDock.Core;
using System.Windows.Forms;
using System.Linq;
using ProtoDock.Api;
using System.Windows.Documents;
using ProtoDock.Settings;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System;

namespace ProtoDock
{
    public partial class SettingsWindow : Form, IDockSettingsContext, IDockSettingsDisplay
    {
        private readonly Dock _dock;
        private readonly DockGraphics _graphics;

        internal SettingsWindow(Dock dock, DockGraphics graphics) : base()
        {

            InitializeComponent();

            Register(dock);

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
            Clear();

            var source = (IDockSettingsSource)Category.SelectedItem;            
            source.Display(this);
        }

        private readonly List<ISettingsLine> _lines = new List<ISettingsLine>();

        private void Clear()
        {
            foreach (var line in _lines)
            {
                Content.Controls.Remove(line.Control);
                line.Dispose();
            }
            _lines.Clear();
        }

        private T Add<T>(T line) where T : ISettingsLine
        {


            line.Control.Width = Content.Width;
            line.Control.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

            if (_lines.Count > 0)
            {
                var lastLine = _lines[_lines.Count - 1];
                line.Control.Top = lastLine.Control.Top + lastLine.Control.Height + 8;
            }

            _lines.Add(line);
            Content.Controls.Add(line.Control);

            return line;
        }

        void IDockSettingsDisplay.Header(string text)
        {
            Add(new SettingsHeader(text));
        }

        void IDockSettingsDisplay.Combo<T>(
            T selected,
            IEnumerable<T> items,
            out Func<T> getValue,
            out Action<T> addItem,
            out Action<T> removeItem,
            Action<T> onValueChanged)
        {
            getValue = default;
            addItem = default;
            removeItem = default;
            Add(new SettingsCombo<T>(selected, items, out getValue, out addItem, out removeItem, onValueChanged));
        }
    }
}
