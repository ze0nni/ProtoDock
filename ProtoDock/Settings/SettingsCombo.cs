using ProtoDock.Api;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Forms;

namespace ProtoDock.Settings
{
    class SettingsCombo<T> : ComboBox, ISettingsLine, ICollectionController<T>
    {
        public Control Control => this;

        public SettingsCombo(
            T selected,
            IEnumerable<T> items,
            Action<T> onChangeValue
            )
        {
            this.DropDownStyle = ComboBoxStyle.DropDownList;

            update(items);
            SelectedItem = selected;

            this.SelectedIndexChanged += (s, e) => {
                onChangeValue?.Invoke((T)this.SelectedItem);
            };
        }

        public void Dispose()
        {
            base.Dispose();
        }

        T ICollectionController<T>.getValue()
        {
            return (T)this.SelectedItem;
        }

        void ICollectionController<T>.addItem(T i)
        {
            this.Items.Add(i);
        }

        void ICollectionController<T>.removeItem(T i)
        {
            this.Items.Remove(i);
        }

        void ICollectionController<T>.select(T i)
        {
            this.SelectedItem = i;
        }

        public void update(IEnumerable<T> items) {
            var selected = SelectedItem;
            Items.Clear();
            foreach (var i in items) {
                Items.Add(i);
            }
            SelectedItem = selected;
        }
    }
}
