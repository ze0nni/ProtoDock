using ProtoDock.Api;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace ProtoDock.Settings
{
    class SettingsDisplay : IDockSettingsDisplay
    {
        private readonly Panel _content;
        private readonly Action _onFlush;
        private readonly Action _setDirty;
        private readonly Action _flashWindow;

        private readonly List<(Label label, ISettingsLine line)> _lines = new List<(Label, ISettingsLine)>();

        public SettingsDisplay(
            Panel content,
            Action onFlush,
            Action setDirty,
            Action flashWindow
        ) {
            _content = content;
            _onFlush = onFlush;
            _setDirty = setDirty;
            _flashWindow = flashWindow;
        }

        public void Clear()
        {
            foreach (var line in _lines)
            {
                if (line.label != null)
                {
                    _content.Controls.Remove(line.label);
                    line.label.Dispose();
                }
                _content.Controls.Remove(line.line.Control);
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

                _content.Controls.Add(label);

                left += 200 + 8;
            }

            line.Control.Left = left;
            line.Control.Top = top;
            line.Control.Width = _content.Width - left;
            line.Control.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

            if (_lines.Count > 0)
            {
                var lastLine = _lines[_lines.Count - 1];
                line.Control.Top = lastLine.line.Control.Top + lastLine.line.Control.Height + 8;
            }

            _lines.Add((label, line));
            _content.Controls.Add(line.Control);

            return line;
        }

        public void Flush()
        {
            _onFlush();
        }

        public void SetDirty()
        {
            _setDirty();
        }

        public void FlashWindow()
        {
            _flashWindow();
        }

        public void Header(string text)
        {
            Add(null, new SettingsHeader(text));
        }

        public void Combo<T>(string label, T selected, IEnumerable<T> items, out ICollectionController<T> controller, Action<T> onValueChanged)
        {
            var c = new SettingsCombo<T>(selected, items, onValueChanged);
            controller = c;

            Add(label, c);
        }

        public void List<T>(string label, T selected, IEnumerable<T> items, out ICollectionController<T> controller, Action<T> onValueChanged)
        {
            var c = new SettingsList<T>(selected, items, onValueChanged);
            controller = c;

            Add(label, c);
        }

        public void Toggle(string label, bool value, out Func<bool> getValue, out Action<bool> setValue, Action<bool> onValueChanged)
        {
            getValue = default;
            setValue = default;
            Add(label, new SettingsToggle(value, out getValue, out setValue, onValueChanged));
        }

        public void Number(string label, int value, int min, int max, out Action<int> setValue, Action<int> onValueChange)
        {
            Add(label, new SettingsNumber(value, min, max, out setValue, onValueChange));
        }

        public IButtonsPanel Buttons()
        {
            return Add(null, new SettingsButtonsPanel());
        }
    }
}
