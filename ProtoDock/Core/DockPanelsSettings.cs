using ProtoDock.Api;

namespace ProtoDock.Core {
	public class DockPanelsSettings : IDockSettingsSource {
		
		private readonly Dock _dock;

		public DockPanelsSettings(Dock dock) {
			_dock = dock;
		}

		public void Display(IDockSettingsDisplay display) {
			
		}

		public override string ToString() {
			return "Panels";
		}
	}
}