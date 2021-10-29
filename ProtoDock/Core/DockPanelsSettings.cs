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
				_dock.Plugins,
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
				.Add("Up", () => { })
				.Add("Down", () => { });
		}

		public override string ToString() {
			return "Panels";
		}
	}
}