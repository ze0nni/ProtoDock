using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using ProtoDock.Api;
using ProtoDock.Settings;

namespace ProtoDock.Core {
	public class DockPanelsSettings : IDockSettingsSource {
		
		private readonly Dock _dock;

		public DockPanelsSettings(Dock dock) {
			_dock = dock;
		}

		public void Display(IDockSettingsDisplay display) {
			Button settingsButton = null;
			display.List(
				"",
				null,
				_dock.Panels,
				out var panels,
				v => {
					if (v == null || v.Mediators.Count != 1)
					{
						settingsButton.Enabled = false;
					} else
                    {
						var m = v.Mediators[0];
						settingsButton.Enabled = m.RequestSettings;
                    }
				}
			);
			
			display.Combo(
				"Plugins",
				null,
				_dock.Plugins.Where(p => p.ResolveHook<IDockPlugin.IPanelHook>(out _)),
				out var plugins,
				v => {});

			display.Buttons()
				.Add("Insert", () =>
				{
					if (plugins.getValue() == null)
					{
						return;
					}

					_dock.AddPanel(plugins.getValue());
					panels.update(_dock.Panels);
				})
				.Add("Remove", () =>
				{
					if (panels.getValue() == null)
					{
						return;
					}

					_dock.RemovePanel(panels.getValue());
					panels.update(_dock.Panels);
				})
				.Add("Left", () =>
				{
					if (panels.getValue() == null)
					{
						return;
					}

					var index = IndexOf(_dock.Panels, panels.getValue());
					_dock.MovePanel(panels.getValue(), index - 1);
					panels.update(_dock.Panels);
				})
				.Add("Right", () =>
				{
					if (panels.getValue() == null)
					{
						return;
					}

					var index = IndexOf(_dock.Panels, panels.getValue());
					_dock.MovePanel(panels.getValue(), index + 1);
					panels.update(_dock.Panels);
				})
				.Add("Settings", out settingsButton, () =>
				{
					var panel = panels.getValue();
					var window = new PanelSettingsWindow(panel.Mediators[0], display.Flush, display.SetDirty, display.FlashWindow);
					window.ShowDialog();
				});
			settingsButton.Enabled = false;
		}

		public override string ToString() {
			return "Panels";
		}

		private static int IndexOf<T>(IReadOnlyList<T> list, T value) {
			for (var i = 0; i < list.Count; i++) {
				if (value.Equals(list[i])) {
					return i;
				}
			}
			return -1;
		}
	}
}