using ProtoDock.Api;

namespace ProtoDock.Time {
	public class TimePlugin : IDockPlugin, IDockPlugin.ISettingsHook, IDockSettingsSource
	{
		public string Name => "Time";
		public string GUID => "{20FCE949-44CA-4F34-A0D4-30FED4972561}";
		public int Version => 1;
		public IDockPanelMediator Create() {
			return new TimeMediator(this);
		}

		public bool ResolveHook<T>(out T hook) where T : class {
			switch (typeof(T))
			{
				case var cls when cls == typeof(IDockPlugin.ISettingsHook):
					hook = this as T;
					return true;
			}

			hook = default;
			return false;
		}

		public override string ToString()
		{
			return "Time";
		}

		void IDockPlugin.ISettingsHook.OnSettings(IDockSettingsContext context)
		{
			context.Register(this);
		}

		void IDockSettingsSource.Display(IDockSettingsDisplay display)
		{
			
		}
	}
}