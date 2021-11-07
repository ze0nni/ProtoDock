using System;
using System.Collections.Generic;
using ProtoDock.Api;

namespace ProtoDock.Time {

	public enum ClockType
	{
		Digit,
		Analog
	}

	public class TimePlugin :
			IDockPlugin,
			IDockPlugin.IPanelHook,
			IDockPlugin.ISettingsHook,
			IDockSettingsSource 
	{
		private List<TimeMediator> _mediators = new List<TimeMediator>();
		
		public string Name => "Time";
		public string GUID => "{20FCE949-44CA-4F34-A0D4-30FED4972561}";
		public int Version => 1;

		public ClockType ClockType { get; private set; }

		public IDockPanelMediator Create() {
			var mediator = new TimeMediator(this);
			_mediators.Add(mediator);
			return mediator;
		}

		internal void removeMediator(TimeMediator mediator) {
			_mediators.Remove(mediator);
		}

		public bool ResolveHook<T>(out T hook) where T : class {
			switch (typeof(T))
			{
				case var cls when cls == typeof(IDockPlugin.IPanelHook):
					hook = this as T;
					return true;

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

		void IDockPlugin.ISettingsHook.OnSettingsRestore(int vertsion, string data) {
			Enum.TryParse<ClockType>(data, out var clockType);
			ClockType = clockType;
		}

		bool IDockPlugin.ISettingsHook.OnSettingsStore(out string data)
		{
			data = ClockType.ToString();
			return true;
		}

		void IDockSettingsSource.Display(IDockSettingsDisplay display)
		{
			display.Combo<ClockType>(
				"Clock type",
				ClockType,
				out _,				
				t => {
					ClockType = t;

					foreach (var m in _mediators) {
						m.Reload();
					}
					
					display.Flush();
				});
		}
	}
}