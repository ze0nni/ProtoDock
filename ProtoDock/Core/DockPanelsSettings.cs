using System;
using System.Collections.Generic;
using System.Linq;
using ProtoDock.Api;

namespace ProtoDock.Core {
	public class DockPanelsSettings : IDockSettingsSource {
		
		private readonly Dock _dock;

		public DockPanelsSettings(Dock dock) {
			_dock = dock;
		}

		public void Display(IDockSettingsDisplay display) {
			display.List(
				"",
				null,
				_dock.Panels,
				out var panels,
				v => { }
			);
			
			display.Combo(
				"Plugins",
				null,
				_dock.Plugins.Where(p => p.ResolveHook<IDockPlugin.IPanelHook>(out _)),
				out var plugins,
				v => {});

			display.Buttons()
				.Add("Insert", () => {
					if (plugins.getValue() == null) {
						return;
					}

					_dock.AddPanel(plugins.getValue());
					panels.update(_dock.Panels);
				})
				.Add("Remove", () => {
					if (panels.getValue() == null) {
						return;
					}

					_dock.RemovePanel(panels.getValue());
					panels.update(_dock.Panels);
				})
				.Add("Left", () => {
					if (panels.getValue() == null)
					{
						return;
					}

					var index = IndexOf(_dock.Panels, panels.getValue());
					_dock.MovePanel(panels.getValue(), index - 1);
					panels.update(_dock.Panels);
				})
				.Add("Right", () => {
					if (panels.getValue() == null) {
						return;
					}

					var index = IndexOf(_dock.Panels, panels.getValue());
					_dock.MovePanel(panels.getValue(), index + 1);
					panels.update(_dock.Panels);
				});
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