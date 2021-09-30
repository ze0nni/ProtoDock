using System;
using System.Windows.Forms;

namespace ProtoDock.Settings
{
    class SettingsNumber: System.Windows.Forms.NumericUpDown, ISettingsLine
    {        

        public SettingsNumber(int value, int min, int max, out Action<int> setValue, Action<int> onValueChange)
        {
            this.Maximum = max;
            this.Minimum = min;
            this.Value = value;            
            setValue = v =>
            {
                this.Value = v;
            };
            this.ValueChanged += (s, e) => {
                onValueChange?.Invoke((int)this.Value);
            };
        }

        public Control Control => this;

        public void Dispose()
        {
            base.Dispose();
        }
    }
}
