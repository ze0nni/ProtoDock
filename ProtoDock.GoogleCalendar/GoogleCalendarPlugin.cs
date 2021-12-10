using Google.Apis.Auth.OAuth2;
using ProtoDock.Api;

namespace ProtoDock.GoogleCalendar {
	public sealed class GoogleCalendarPlugin :
		IDockPlugin,
		IDockPlugin.IPanelHook,
		IDockPlugin.ISettingsHook,
		IDockSettingsSource
	{
		public string Name => "GoogleCalendar";
		public string GUID => "{E1FF94BB-7751-495B-8119-6CE986379811}";
		public int Version => 1;
		public bool ResolveHook<T>(out T hook) where T : class {
			switch (typeof(T))
			{
				case var cls when cls == typeof(IDockPlugin.IPanelHook):
					hook = this as T;
					return true;
			}

			hook = default;
			return false;
		}

		public IDockPanelMediator Create() {
			return new GoogleCalendarMediator(this);
		}

		public override string ToString() {
			return "Google Calendar";
		}
		
		void IDockPlugin.ISettingsHook.OnSettings(IDockSettingsContext context) {
			context.Register(this);
		}

		void IDockPlugin.ISettingsHook.OnSettingsRestore(int version, string data) {
			
		}

		bool IDockPlugin.ISettingsHook.OnSettingsStore(out string data) {
			data = null;
			return false;
		}
		
		void IDockSettingsSource.Display(IDockSettingsDisplay display) {
			
		}
	}
}