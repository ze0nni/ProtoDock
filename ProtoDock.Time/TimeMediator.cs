using System.Diagnostics;
using System.Windows.Forms;
using ProtoDock.Api;

namespace ProtoDock.Time {
	internal class TimeMediator: IDockPanelMediator {
		public IDockPlugin Plugin => _plugin;
		private TimePlugin _plugin;

		public IDockPanelApi Api { get; private set; }

		private DigitClock _digit;
		private AnalogClock _analog;
		
		public TimeMediator(TimePlugin plugin) {
			_plugin = plugin;
		}

		public void Setup(IDockPanelApi api) {
			Api = api;
		}

		public void RestoreIcon(int version, string data) {
			
		}

		public void Awake() {
			Reload();
		}

		public void Destroy() {
			_plugin.removeMediator(this);
			_plugin = null;
			Clear();
		}

		public void Update()
		{

		}

		private void Clear() {
			
			if (_digit != null) {
				Api.Remove(_digit, false);
				_digit.Dispose();
			}

			if (_analog != null) {
				Api.Remove(_analog, false);
				_analog.Dispose();
			}
		}
		
		public void Reload() {
			Clear();

			switch (_plugin.ClockType) {
				case ClockType.Digit:
					_digit = new DigitClock(this);
					Api.Add(_digit, false);
					break;
				
				case ClockType.Analog:
					_analog = new AnalogClock(this);
					Api.Add(_analog, false);
					break;
			}
		}
		
		public bool DragCanAccept(IDataObject data) {
			return false;
		}

		public void DragAccept(int index, IDataObject data) {
			
		}
	}
}