using System;
using System.Drawing;
using System.Windows.Forms;
using Google.Apis.Calendar.v3.Data;
using ProtoDock.Api;

namespace ProtoDock.GoogleCalendar {
	internal sealed class GoogleCalendarIcon : IDockIcon, IDisposable {

		public IDockPanelMediator Mediator => _mediator;
		private GoogleCalendarMediator _mediator;
		private IDockPanelApi _api;
		
		public string Title => "";
		public float Width => 3;
		public bool Hovered => false;

		private Event _data;
		private Font _font;

		private Color _backgroundColor = Color.Gray;
		private Color _foregroundColor = Color.White;
		private Brush _background;
		private Brush _foreground;

		public GoogleCalendarIcon(GoogleCalendarMediator mediator, IDockPanelApi api) {
			_mediator = mediator;
			_api = api;

			updateGraphics();
		}

		public void Dispose() {
			_font?.Dispose();
		}

		private void updateGraphics() {
			_background?.Dispose();
			_font?.Dispose();
			_foreground?.Dispose();

			_background = new SolidBrush(_backgroundColor);
			_foreground = new SolidBrush(_foregroundColor);
			_font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Regular, GraphicsUnit.Pixel);
			
			_api.Dock.SetDirty();
		}

		public bool Store(out string data) {
			data = default;
			return false;
		}
		
		public void SetData(Event data) {
			_data = data;
			_api.Dock.SetDirty();
		}

		public void Update() {
			
		}

		public void MouseEnter() {
			
		}

		public void MouseLeave() {
			
		}

		public void MouseDown(int x, int y, MouseButtons button) {
			
		}

		public bool MouseUp(int x, int y, MouseButtons button) {
			return true;
		}

		public void MouseMove(int x, int y, MouseButtons button) {
			
		}

		public void Render(Graphics graphics, float width, float height, bool isSelected) {
			if (_data == null) {
				return;
			}
			graphics.FillRectangle(_background, 0, 0, width, height);
			graphics.DrawString(_data.Summary, _font, _foreground, new RectangleF(0, 0, width, height));
			//graphics.DrawString(_data.Start.DateTime.ToString(), _font, _foreground, new PointF(0, 20));
		}
	}
}