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

		public void MouseEnter() {
		}

		public void MouseLeave() {
		}

		public void MouseDown(int x, int y, MouseButtons button) {
		}

		public bool MouseUp(int x, int y, MouseButtons button) {
			return false;
		}
        
		public void MouseMove(int x, int y, MouseButtons button) {
		}

		private void UpdateFont(float height) {
			if (height == 0) {
				Width = 1;
				_mediator.Api.Dock.SetDirty();
				return;
			}
			
			_font?.Dispose();

			_font = new Font(FontFamily.GenericMonospace, _height, GraphicsUnit.Pixel);

			var size = TextRenderer.MeasureText("00:00", _font);
			Width = size.Width / height;
			
			_mediator.Api.Dock.SetDirty();
		}

		public void Render(Graphics graphics, float width, float height, Rectangle content) {
			if (_width != width || _height != height) {
				_width = width;
				_height = height;
				UpdateFont(height);
			}

			if (_font == null) {
				return;
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