using BBDock.Core;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace BBDock
{
    public partial class DockWindow : Form
    {

        public IntPtr HInstance => User32.HInstance;

        private readonly Timer _timer;

        private readonly DockGraphics _graphics;
        private readonly Dock _dock;        

        public DockWindow(): base()
        {
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.ShowIcon = false;
            this.FormBorderStyle = FormBorderStyle.None;

            this.AllowDrop = true;

            _timer = new Timer();
            _timer.Interval = 1000 / 60;
            _timer.Start();
            _timer.Tick += OnTick;

            _graphics = new DockGraphics(
                64,
                8,
                //new Padding(16, 16, 16, 26),
                //24
                //new Bitmap(@"D:\Projects\BBDock\Assets\Panel.png"),
                //new Padding(32, 32, 32, 32)

                new Padding(0, 0, 0, 8),
                16,
                new Bitmap(@"D:\Projects\BBDock\Assets\Panel2.png"),
                new Padding(64, 16, 64, 31)
            );
            _dock = new Dock(HInstance, _graphics);
            Render();

            this.FormClosing += (s, e) =>
            {
                e.Cancel = true;
            };

            this.FormClosed += (s, e) =>
            {
                _timer.Dispose();
                _graphics.Dispose();
            };

            this.MouseLeave += OnMouseLeave;
            this.MouseMove += OnMouseMove;

            this.DragOver += (s, e) =>
            {
                _graphics.MouseMove(e.X, e.Y);

                e.Effect = DragDropEffects.Link;
            };
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var p = base.CreateParams;
                p.ExStyle |= (int)PInvoke.User32.WindowStylesEx.WS_EX_TOOLWINDOW;
                p.ExStyle |= (int)PInvoke.User32.WindowStylesEx.WS_EX_LAYERED;
                return p;
            }
        }

        public const int IconSize = 64;

        public void Render()
        {
            var screen = Screen.AllScreens[0];
            var bounds = screen.Bounds;

            _graphics.Render();
            SetImage(_graphics.Bitmap);

            this.Left = (bounds.Width - _graphics.Bitmap.Width) / 2;
            switch (_graphics.Position)
            {
                case Position.Top:
                    this.Top = -_graphics.VOffset;
                    break;

                case Position.Bottom:
                    this.Top = (bounds.Height - _graphics.Bitmap.Height) + _graphics.VOffset;
                    break;

                default:
                    throw new ArgumentException(_graphics.Position.ToString());
            }            
        }

        public void SetImage(Bitmap bitmap)
        {
             var screenDC = User32.GetDC(IntPtr.Zero);
             var memDC = User32.CreateCompatibleDC(screenDC);

            IntPtr bitmapHandle = IntPtr.Zero;
            IntPtr oldBitmapHandle = IntPtr.Zero;
            try
            {
                bitmapHandle = bitmap.GetHbitmap(Color.FromArgb(0));
                oldBitmapHandle = User32.SelectObject(memDC, bitmapHandle);

                var size = new User32.Size { cx = bitmap.Width, cy = bitmap.Height };
                var poinSource = new User32.Point { x = 0, y = 0 };
                var topPos = new User32.Point { x = this.Left, y = this.Top };
                var blend = new User32.BLENDFUNCTION
                {
                    blendOp = User32.AC_SRC_ALPHA,
                    blendFlags = 0,
                    sourceConstantAlpha = 255,
                    alphaFormat = User32.AC_SRC_ALPHA
                };
                User32.UpdateLayeredWindow(this.Handle, screenDC, ref topPos, ref size, memDC, ref poinSource, 0, ref blend, User32.ULW_ALPHA);
            }
            finally
            {
                User32.ReleaseDC(IntPtr.Zero, screenDC);
                if (bitmapHandle != IntPtr.Zero)
                {
                    User32.SelectObject(memDC, oldBitmapHandle);
                    User32.DeleteObject(bitmapHandle);
                }
                User32.DeleteDC(memDC);
            }
        }

        public void OnTick(Object sender, EventArgs e)
        {
            _graphics.Update(1 / 60f);

            if (_graphics.IsDirty)
                Render();
        }

        private void OnMouseMove(object sender, MouseEventArgs e)
        {
            _graphics.MouseMove(e.X, e.Y);
        }

        private void OnMouseLeave(object sender, EventArgs e)
        {
            _graphics.MouseLeave();
        }
    }
}
