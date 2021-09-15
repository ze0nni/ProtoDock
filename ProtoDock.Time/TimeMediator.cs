using System.Windows.Forms;
using ProtoDock.Api;

namespace ProtoDock.Time {
	internal class TimeMediator: IDockPanelMediator {
		public IDockPlugin Plugin { get; }

		public IDockPanelApi Api { get; private set; }
		
		public TimeMediator(IDockPlugin plugin) {
			Plugin = plugin;
		}

		public void Setup(IDockPanelApi api) {
			Api = api;
		}

		public void RestoreIcon(int version, string data) {
			
		}

		public void Awake() {
			Api.Add(new DigitClock(this), false);
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