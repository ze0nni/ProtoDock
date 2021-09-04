using BBDock.Core;
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BBDock
{
    public partial class DockWindow : Form
    {
        public IntPtr HInstance => Marshal.GetHINSTANCE(GetType().Module);

        private Dock _dock;

        private int _iconsCount;

        public DockWindow(): base()
        {
            this.TopMost = true;
            this.ShowInTaskbar = false;
            this.ShowIcon = false;
            this.FormBorderStyle = FormBorderStyle.None;

            _dock = new Dock(this);
        }

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == (int)PInvoke.User32.WindowMessage.WM_PAINT)
            {
                UpdatePosition();
                return;
            }

            base.WndProc(ref m);
        }

        protected override CreateParams CreateParams
        {
            get
            {
                var p = base.CreateParams;
                p.ExStyle |= (int)PInvoke.User32.WindowStylesEx.WS_EX_TOOLWINDOW;
                return p;
            }
        }

        public const int IconSize = 64;

        public void SetIconsCount(int count)
        {
            _iconsCount = count;
            UpdatePosition();
        }

        public void UpdatePosition()
        {
            var screen = Screen.AllScreens[0];
            var bounds = screen.Bounds;

            this.Width = IconSize * _iconsCount;
            this.Height = IconSize;
            this.Left = (bounds.Width - this.Width) / 2;
            this.Top = (bounds.Height - this.Height);
        }
    }
}
