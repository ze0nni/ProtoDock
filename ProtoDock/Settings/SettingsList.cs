using System;
using System.Collections.Generic;
using System.Windows.Forms;
using ProtoDock.Api;

namespace ProtoDock.Settings {
	public class SettingsList<T>: ListBox, ISettingsLine, ICollectionController<T> {
		public Control Control => this;

		public SettingsList(
			T selected,
			IEnumerable<T> items,
			Action<T> onChangeValue)
		{
			foreach (var item in items) {
				this.Items.Add(item);
			}

			this.SelectedItem = selected;

			this.SelectedValueChanged += (s, e) => {
				onChangeValue.Invoke((T)this.SelectedItem);
			};
		}

		public void Dispose()
		{
			base.Dispose();
		}

		public T getValue() {
			throw new System.NotImplementedException();
		}

		public void addItem(T i) {
			throw new System.NotImplementedException();
		}

		public void removeItem(T i) {
			throw new System.NotImplementedException();
		}

		public void @select(T i) {
			throw new System.NotImplementedException();
		}
	}
}