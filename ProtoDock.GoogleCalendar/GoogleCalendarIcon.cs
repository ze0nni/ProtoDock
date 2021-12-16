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

		private float _fontSize;
		private Font _font;
		private readonly StringFormat _titleFontFormat  = new StringFormat();
		private readonly StringFormat _timeStampFontFormat  = new StringFormat();

		private Color _backgroundColor = Color.Gray;
		private Color _foregroundColor = Color.White;
		private Brush _background;
		private Brush _foreground;

		public GoogleCalendarIcon(GoogleCalendarMediator mediator, IDockPanelApi api, PanelScales scales) {
			_mediator = mediator;
			_api = api;

			updateGraphics(scales);
		}

		public void Dispose() {
			_font?.Dispose();
		}

		public void updateGraphics(PanelScales scales) {
			_background?.Dispose();
			_font?.Dispose();
			_foreground?.Dispose();

			if (scales.IconSize == 0) {
				return;
			}
			
			_fontSize = scales.IconSize / 3.2f;
			
			_titleFontFormat.Trimming = StringTrimming.EllipsisCharacter;
			_timeStampFontFormat.Alignment = StringAlignment.Far;

			_background = new SolidBrush(_backgroundColor);
			_foreground = new SolidBrush(_foregroundColor);
			_font = new Font(FontFamily.GenericSansSerif, _fontSize, FontStyle.Regular, GraphicsUnit.Pixel);
			
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
			if (button != MouseButtons.Right) {
				return false;
			}

			var menu = new ContextMenuStrip();
			menu.Items.Add(new ToolStripMenuItem("Link", null, onLinkClick));
			if (_data.HangoutLink != null) {
				menu.Items.Add(new ToolStripMenuItem("Hangout", null, onHangoutClick));
			}
			menu.Show(Cursor.Position);
			
			return true;
		}

		private void onLinkClick(object sender, EventArgs e) {
			var psi = new System.Diagnostics.ProcessStartInfo();
			psi.UseShellExecute = true;
			psi.FileName = _data.HtmlLink;
			System.Diagnostics.Process.Start(psi);
		}
		
		private void onHangoutClick(object sender, EventArgs e) {
			var psi = new System.Diagnostics.ProcessStartInfo();
			psi.UseShellExecute = true;
			psi.FileName = _data.HangoutLink;
			System.Diagnostics.Process.Start(psi);
		}

		public void MouseMove(int x, int y, MouseButtons button) {
			
		}

		public void Render(Graphics graphics, float width, float height, bool isSelected) {
			if (_data == null) {
				return;
			}
			graphics.FillRectangle(_background, 0, 0, width, height);

			if (_data.Summary != null) {
				var size = graphics.MeasureString("@", _font);
				
				graphics.DrawString(
					_data.Summary,
					_font,
					_foreground,
					new RectangleF(0, 0, width, size.Height * 2),
					_titleFontFormat);
			}

			if (_data.Start.DateTime != null) {
				var text = _data.Start.DateTime.Value.ToString("HH:mm");

				var size = graphics.MeasureString(text, _font);
				
				graphics.DrawString(
					text,
					_font,
					_foreground,
					new RectangleF(0, height - size.Height - 1, width, _fontSize + 1),
					_timeStampFontFormat);
			}
		}
	}
}