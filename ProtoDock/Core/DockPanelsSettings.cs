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
				out var controllerr,
				v => { }
			);
			
			display.Combo(
				"Plugins",
				null,
				_dock.Plugins,
				out _,
				v => {});
		}

		public override string ToString() {
			return "Panels";
		}
	}
}