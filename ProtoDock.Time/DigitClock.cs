using System;
using System.Drawing;
using ProtoDock.Api;

namespace ProtoDock.Time {
	internal class DigitClock: IDockIcon {
		public IDockPanelMediator Mediator { get; }
		public string Title => DateTime.Now.ToLongTimeString();
		public int Width => 2;
		public bool Hovered => false;

		public DigitClock(IDockPanelMediator mediator) {
			Mediator = mediator;
		}

		public void Update() {
			
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