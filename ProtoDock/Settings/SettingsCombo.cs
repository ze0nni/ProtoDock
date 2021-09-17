using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;

namespace ProtoDock.Settings
{
    class SettingsCombo<T> : ComboBox, ISettingsLine
    {
        public Control Control => this;

        public SettingsCombo(
            T selected,
            IEnumerable<T> items,
            out Func<T> getValue,
            out Action<T> addItem,
            out Action<T> removeItem,
            Action<T> onChangeValue
            )
        {
            this.DropDownStyle = ComboBoxStyle.DropDownList;

            foreach (var i in items)
            {
                this.Items.Add(i);
            }

            SelectedItem = selected;

            getValue = () => { return (T)this.SelectedItem; };
            addItem = i => { this.Items.Add(i); };
            removeItem = i => { this.Items.Remove(i); };
            this.SelectedIndexChanged += (s, e) => {
                onChangeValue?.Invoke((T)this.SelectedItem);
            };
        }

        public void Dispose()
        {
            base.Dispose();
        }
    }
}
