using PInvoke;
using ProtoDock.Api;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;

namespace ProtoDock.Tasks
{
    internal class TaskIcon : IDockIcon, IDisposable
    {
        public readonly StringBuilder _sb = new StringBuilder(2048);

        public IDockPanelMediator Mediator { get; }
        private readonly IntPtr _hWnd;
        private readonly IDockApi _api;

        private Bitmap _icon;
        private bool _isActive;

        public TaskIcon(IDockPanelMediator mediator, IDockApi api, IntPtr hWnd)
        {
            Mediator = mediator;
            _hWnd = hWnd;
            _api = api;
            Redraw();
        }

        public void Dispose()
        {
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
            if (_isActive)
            {
                _api.DrawSkin(SkinElement.SelectedBg, graphics, 0, 0, width, height);
            }

            if (_icon != null)
            {
                graphics.DrawImage(_icon, new Rectangle(0, 0, (int)width, (int)height));
            }

            if (_isActive)
            {
                _api.DrawSkin(SkinElement.SelectedFg, graphics, 0, 0, width, height);
            }
        }

        public bool Store(out string data)
        {
            data = default;
            return false;
        }

        internal void SetActiveWindow(IntPtr wnd)
        {
            _isActive = _hWnd == wnd;
            _api.SetDirty();
        }

        internal void Redraw()
        {

            _icon?.Dispose();
            _icon = null;

            var hIcon = PInvoke.User32.SendMessage(_hWnd, PInvoke.User32.WindowMessage.WM_GETICON, new IntPtr(1), IntPtr.Zero);
            if (hIcon != IntPtr.Zero)
            {
                _icon = Icon.FromHandle(hIcon).ToBitmap();
            }
            else
            {
                User32.GetWindowThreadProcessId(_hWnd, out var processId);
                uint size = (uint)_sb.Capacity;
                QueryFullProcessImageName((IntPtr)processId, 0, _sb, ref size);
                Debug.WriteLine(_sb.ToString());

            }
        }

        public const int GCL_HICONSM = -34;
        public const int GCL_HICON = -14;

        [DllImport("user32.dll", EntryPoint = "GetClassLong")]
        public static extern uint GetClassLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);
    }
}
