using System;
using System.Drawing;
using ProtoDock.Api;

namespace ProtoDock.Time {
	internal class DigitClock: IDockIcon, IDisposable {
		public IDockPanelMediator Mediator => _mediator;
		private TimeMediator _mediator;
		public string Title => DateTime.Now.ToShortDateString();
		public int Width => 2;
		public bool Hovered => false;

		private int _minutes;

		public DigitClock(TimeMediator mediator) {
			_mediator = mediator;
		}

		public void Dispose() {
			
		}

		public void Update() {
			var minutes = DateTime.Now.Minute;
			if (_minutes != minutes) {
				_minutes = minutes;
				_mediator.Api.Dock.SetDirty();
			}
		}

		public void Click() {
			
		}

		public bool ContextClick() {
			return false;
		}

		public void Render(Graphics graphics, float width, float height, bool isSelected) {
		
		}

		public bool Store(out string data) {
			data = default;
			return false;
		}
	}
}