using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace ProtoDock.Settings
{
    class SettingsToggle : CheckBox, ISettingsLine
    {
        public Control Control => this;

        public SettingsToggle(bool value, out Func<bool> getValue, out Action<bool> setValue, Action<bool> onValueChanged)
        {
            this.Checked = value;
            getValue = () => this.Checked;
            setValue = v => this.Checked = v;
            this.CheckedChanged += (s, e) => onValueChanged?.Invoke(this.Checked);
        }

        public void Dispose()
        {
            base.Dispose();
        }        
    }
}
