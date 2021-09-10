using ProtoDock.Api;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace ProtoDock.Tasks
{
    internal class TaskIcon : IDockIcon, IDisposable
    {
        public IDockPanelMediator Mediator { get; }

        private Bitmap _icon;

        public TaskIcon(IDockPanelMediator mediator, IntPtr hWnd)
        {
            Mediator = mediator;

            var hIcon = PInvoke.User32.SendMessage(hWnd, PInvoke.User32.WindowMessage.WM_GETICON, new IntPtr(1), IntPtr.Zero);
            if (hIcon != IntPtr.Zero) {
                _icon = Icon.FromHandle(hIcon).ToBitmap();
            }
            
            //TODO: Destroy icon
        }

        public void Dispose() {
            _icon?.Dispose();
        }

        public void Click()
        {

        }

        public bool ContextClick()
        {
            return false;
        }

        public void Render(Graphics graphics, float width, float height, bool isSelected)
        {
            if (_icon != null)
            {
                graphics.DrawImage(_icon, new Rectangle(0, 0, (int)width, (int)height));
            }
                
        }

        public bool Store(out string data)
        {
            data = default;
            return false;
        }

        public const int GCL_HICONSM = -34;
        public const int GCL_HICON = -14;

        [DllImport("user32.dll", EntryPoint = "GetClassLong")]
        public static extern uint GetClassLongPtr32(IntPtr hWnd, int nIndex);
    }
}
