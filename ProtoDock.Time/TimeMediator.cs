using System.Windows.Forms;
using ProtoDock.Api;

namespace ProtoDock.Time {
	internal class TimeMediator: IDockPanelMediator {
		public IDockPlugin Plugin { get; }

		public IDockPanelApi Api { get; private set; }

		private DigitClock _clock;
		
		public TimeMediator(IDockPlugin plugin) {
			Plugin = plugin;
		}

		public void Setup(IDockPanelApi api) {
			Api = api;
		}

		public void RestoreIcon(int version, string data) {
			
		}

		public void Awake() {
			_clock = new DigitClock(this);
			Api.Add(_clock, false);
		}

		public void Destroy() {
			if (_clock != null) {
				Api.Remove(_clock, false);
				_clock.Dispose();
			}			
		}

		public bool DragCanAccept(IDataObject data) {
			return false;
		}

		public void DragAccept(int index, IDataObject data) {
			
		}
	}
}