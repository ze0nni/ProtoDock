using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Windows.Forms;
using ProtoDock.Api;

namespace ProtoDock.Time {
	internal class DigitClock: IDockIcon, IDisposable {
		public IDockPanelMediator Mediator => _mediator;
		private TimeMediator _mediator;
		public string Title => DateTime.Now.ToShortDateString();
		public float Width { get; private set; }
		public bool Hovered => false;

		private Font _font;

		private float _width = -1;
		private float _height = -1;

		private int _minutes = -1;
		

		public DigitClock(TimeMediator mediator) {
			_mediator = mediator;
			Width = 0;
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

		private void UpdateFont(float height) {
			_font?.Dispose();

			_font = new Font(FontFamily.GenericMonospace, _height, GraphicsUnit.Pixel);

			var size = TextRenderer.MeasureText("00:00", _font);
			Width = size.Width / height;
			
			_mediator.Api.Dock.SetDirty();
		}
		
		public void Render(Graphics graphics, float width, float height, bool isSelected) {
			if (_width != width || _height != height) {
				_width = width;
				_height = height;
				UpdateFont(height);
			}

			graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
			graphics.DrawString(DateTime.Now.ToShortTimeString(), _font, Brushes.White,new PointF(0, 0));
		}

		public bool Store(out string data) {
			data = default;
			return false;
		}
	}
}