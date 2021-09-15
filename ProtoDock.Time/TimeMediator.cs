using System.Windows.Forms;
using ProtoDock.Api;

namespace ProtoDock.Time {
	internal class TimeMediator: IDockPanelMediator {
		public IDockPlugin Plugin { get; }

		private IDockPanelApi _api;
		
		public TimeMediator(IDockPlugin plugin) {
			Plugin = plugin;
		}

		public void Setup(IDockPanelApi api) {
			_api = api;
		}

		public void RestoreIcon(int version, string data) {
			
		}

		public void Awake() {
			_api.Add(new DigitClock(this));
		}

		public void Destroy() {
			
		}

		public bool DragCanAccept(IDataObject data) {
			return false;
		}

		public void DragAccept(int index, IDataObject data) {
			
		}
	}
}