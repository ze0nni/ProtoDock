using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using ProtoDock.Api;

namespace ProtoDock.Time {
	internal class AnalogClock: IDockIcon, IDisposable {

		public IDockPanelMediator Mediator => _mediator;
		public string Title => "";
		public float Width => 1;
		public bool Hovered => true;
		
		private TimeMediator _mediator;

		private Bitmap _clockBg;
		private Bitmap _clockFg;

		public AnalogClock(TimeMediator mediator) {
			_mediator = mediator;

			_clockBg = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("ProtoDock.Time.Embeded.Analog_bg.png"));
			_clockFg = new Bitmap(Assembly.GetExecutingAssembly().GetManifestResourceStream("ProtoDock.Time.Embeded.Analog_fg.png"));
		}
		
		public void Dispose() {
			_clockBg.Dispose();
			_clockFg.Dispose();
		}
		
		public void Update() {
			
		}
		
		public void Render(Graphics graphics, float width, float height, bool isSelected) {
			graphics.DrawImage(_clockBg, 0, 0, width, height);

			var now = DateTime.Now;

			DrawArrow(graphics, width, height, width * 0.4f, 10, now.Hour + (60 / now.Minute), 12);
			DrawArrow(graphics, width, height, width * 0.25f, 5, now.Minute, 60);
			
			graphics.DrawImage(_clockFg, 0, 0, width, height);
		}

		public void DrawArrow(Graphics g, float width, float height, float distance, float weight, float value, float total) {
			var angle = 2 * MathF.PI * value / total;
			
			var cx = width / 2;
			var cy = height / 2;
			var tx = cx + distance * MathF.Sin(angle);
			var ty = cy - distance * MathF.Cos(angle);

			using var pen = new Pen(Color.Black, weight * (width / 255));
			
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.DrawLine(pen, cx, cy, tx, ty);
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

		public bool Store(out string data) {
			data = default;
			return false;
		}
	}
}