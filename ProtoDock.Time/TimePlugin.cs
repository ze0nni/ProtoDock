using ProtoDock.Api;

namespace ProtoDock.Time {
	public class TimePlugin : IDockPlugin {
		public string Name => "Time";
		public string GUID => "{20FCE949-44CA-4F34-A0D4-30FED4972561}";
		public int Version => 1;
		public IDockPanelMediator Create() {
			return new TimeMediator(this);
		}
	}
}