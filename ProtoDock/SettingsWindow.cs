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
    public partial class SettingsWindow : Form, IDockSettingsContext, IDockSettingsDisplay
    {
        private readonly Dock _dock;

        internal SettingsWindow(Dock dock) : base()
        {
            InitializeComponent();
            _dock = dock;

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

        private readonly List<(Label label, ISettingsLine line)> _lines = new List<(Label, ISettingsLine)>();

        private void Clear()
        {
            foreach (var line in _lines)
            {
                if (line.label != null)
                {
                    Content.Controls.Remove(line.label);
                    line.label.Dispose();
                }
                Content.Controls.Remove(line.line.Control);
                line.line.Control.Dispose();
            }
            _lines.Clear();
        }

        private T Add<T>(string labelText, T line) where T : ISettingsLine
        {

            var left = 0;
            var top = 8;
            if (_lines.Count > 0)
            {
                var lastLine = _lines[_lines.Count - 1];
                top = lastLine.line.Control.Top + lastLine.line.Control.Height + 8;
            }


            Label label = null;
            if (!string.IsNullOrEmpty(labelText))
            {
                label = new Label();
                label.AutoSize = false;
                label.Width = 200;
                label.Text = labelText;                
                label.Top = top;
                label.TextAlign = System.Drawing.ContentAlignment.MiddleRight;

                Content.Controls.Add(label);
                
                left += 200 + 8;
            }

            line.Control.Left = left;
            line.Control.Top = top;
            line.Control.Width = Content.Width - left;
            line.Control.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

            if (_lines.Count > 0)
            {
                var lastLine = _lines[_lines.Count - 1];
                line.Control.Top = lastLine.line.Control.Top + lastLine.line.Control.Height + 8;
            }

            _lines.Add((label, line));
            Content.Controls.Add(line.Control);

            return line;
        }

        void IDockSettingsDisplay.Flush() {
            _dock.Flush();
        }

        void IDockSettingsDisplay.SetDirty() {
            _dock.Graphics.SetDirty();
        }

        void IDockSettingsDisplay.Header(string text)
        {
            Add(null, new SettingsHeader(text));
        }

        void IDockSettingsDisplay.Combo<T>(
            string label,
            T selected,
            IEnumerable<T> items,
            out ICollectionController<T> controller,
            Action<T> onValueChanged)
        {
            var c = new SettingsCombo<T>(selected, items, onValueChanged);
            controller = c;

            Add(label, c);
        }

        void IDockSettingsDisplay.Toggle(string label, bool value, out Func<bool> getValue, out Action<bool> setValue, Action<bool> onValueChanged)
        {
            getValue = default;
            setValue = default;
            Add(label, new SettingsToggle(value, out getValue, out setValue, onValueChanged));
        }
    }
}
