using System;
using System.Windows.Forms;
using ProtoDock.Api;
using Control = System.Windows.Forms.Control;

namespace ProtoDock.Settings {
	internal sealed class SettingsButtonsPanel : FlowLayoutPanel, ISettingsLine, IButtonsPanel {
		
		public Control Control => this;

		public SettingsButtonsPanel(): base() {
			Height = 32;
			FlowDirection = FlowDirection.RightToLeft;
		}

		public IButtonsPanel Add(string text, Action onClick) {
			var button = new Button();
			button.Text = text;
			button.Click += (s, e) => onClick();
			Controls.Add(button);
			
			return this;
		}
	}
}