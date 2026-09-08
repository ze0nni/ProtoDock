using System;
using System.Drawing;
using System.Windows.Forms;

namespace ProtoDock.Tray
{
    internal sealed class TrayNotifyIcon : IDisposable
    {
        private DateTime _lastLClick = DateTime.MinValue;
        private DateTime _lastMClick = DateTime.MinValue;
        private DateTime _lastRClick = DateTime.MinValue;

        public IntPtr HWnd { get; set; }
        public uint Uid { get; set; }
        public Guid Guid { get; set; }
        public uint CallbackMessage { get; set; }
        public uint Version { get; set; }
        public string Title { get; set; }
        public string ProcessPath { get; set; }
        public bool IsHidden { get; set; }
        public Bitmap Image { get; private set; }
        public Rectangle Placement { get; set; }

        public event Action Changed;

        public bool Matches(uint hWnd, uint uid, Guid guid)
        {
            if (guid != Guid.Empty && Guid != Guid.Empty && Guid == guid)
            {
                return true;
            }

            return HWnd == Win32.ToHandle(hWnd) && Uid == uid;
        }

        public bool Matches(TrayNotifyIcon other)
        {
            if (other == null)
            {
                return false;
            }

            if (other.Guid != Guid.Empty && Guid != Guid.Empty && Guid == other.Guid)
            {
                return true;
            }

            return HWnd == other.HWnd && Uid == other.Uid;
        }

        public void SetImage(IntPtr hIcon)
        {
            Image?.Dispose();
            Image = null;

            if (hIcon == IntPtr.Zero)
            {
                NotifyChanged();
                return;
            }

            Image = CreateBestBitmap(hIcon);
            NotifyChanged();
        }

        private static Bitmap CreateBestBitmap(IntPtr hIcon)
        {
            Bitmap best = null;
            var bestArea = 0;

            foreach (var size in new[] { 48, 32 })
            {
                var copy = Win32.CopyImage(hIcon, Win32.IMAGE_ICON, size, size, Win32.LR_COPYFROMRESOURCE);
                if (TryTakeBitmap(copy, ref best, ref bestArea))
                {
                    continue;
                }
            }

            var fallback = Win32.CopyIcon(hIcon);
            TryTakeBitmap(fallback, ref best, ref bestArea);
            return best;
        }

        private static bool TryTakeBitmap(IntPtr hIcon, ref Bitmap best, ref int bestArea)
        {
            if (hIcon == IntPtr.Zero)
            {
                return false;
            }

            try
            {
                var size = Win32.GetIconSize(hIcon);
                var area = size.IsEmpty ? 1 : Math.Max(0, size.Width * size.Height);
                if (best != null && area <= bestArea)
                {
                    return false;
                }

                using var icon = Icon.FromHandle(hIcon);
                var bitmap = icon.ToBitmap();
                best?.Dispose();
                best = bitmap;
                bestArea = area;
                return true;
            }
            finally
            {
                Win32.DestroyIcon(hIcon);
            }
        }

        public void NotifyChanged()
        {
            Changed?.Invoke();
        }

        public void MouseEnter()
        {
            if (!EnsureWindow())
            {
                return;
            }

            var mouse = CursorParam();
            SendCallback(Win32.WM_MOUSEHOVER, mouse);
            if (Version > 3)
            {
                SendCallback(Win32.NIN_POPUPOPEN, mouse);
            }
        }

        public void MouseLeave()
        {
            if (!EnsureWindow())
            {
                return;
            }

            var mouse = CursorParam();
            SendCallback(Win32.WM_MOUSELEAVE, mouse);
            if (Version > 3)
            {
                SendCallback(Win32.NIN_POPUPCLOSE, mouse);
            }
        }

        public void MouseMove()
        {
            if (!EnsureWindow())
            {
                return;
            }

            SendCallback(Win32.WM_MOUSEMOVE, CursorParam());
        }

        public void MouseDown(MouseButtons button)
        {
            if (!EnsureWindow())
            {
                return;
            }

            Win32.GetWindowThreadProcessId(HWnd, out var processId);
            Win32.AllowSetForegroundWindow(processId);

            var mouse = CursorParam();
            var doubleClickTime = SystemInformation.DoubleClickTime;

            switch (button)
            {
                case MouseButtons.Left:
                    SendClick(Win32.WM_LBUTTONDOWN, Win32.WM_LBUTTONDBLCLK, mouse, doubleClickTime, ref _lastLClick);
                    break;
                case MouseButtons.Middle:
                    SendClick(Win32.WM_MBUTTONDOWN, Win32.WM_MBUTTONDBLCLK, mouse, doubleClickTime, ref _lastMClick);
                    break;
                case MouseButtons.Right:
                    SendClick(Win32.WM_RBUTTONDOWN, Win32.WM_RBUTTONDBLCLK, mouse, doubleClickTime, ref _lastRClick);
                    break;
            }
        }

        public void MouseUp(MouseButtons button)
        {
            if (!EnsureWindow())
            {
                return;
            }

            var mouse = CursorParam();

            switch (button)
            {
                case MouseButtons.Left:
                    SendCallback(Win32.WM_LBUTTONUP, mouse);
                    if (Version >= 3)
                    {
                        SendCallback(Win32.NIN_SELECT, mouse);
                    }
                    _lastLClick = DateTime.Now;
                    break;
                case MouseButtons.Middle:
                    SendCallback(Win32.WM_MBUTTONUP, mouse);
                    _lastMClick = DateTime.Now;
                    break;
                case MouseButtons.Right:
                    SendCallback(Win32.WM_RBUTTONUP, mouse);
                    if (Version >= 3)
                    {
                        SendCallback(Win32.WM_CONTEXTMENU, mouse);
                    }
                    _lastRClick = DateTime.Now;
                    break;
            }
        }

        public void Dispose()
        {
            Image?.Dispose();
            Image = null;
            Changed = null;
        }

        private void SendClick(int down, int dblclk, uint mouse, int doubleClickTime, ref DateTime lastClick)
        {
            if (DateTime.Now.Subtract(lastClick).TotalMilliseconds <= doubleClickTime)
            {
                SendCallback(dblclk, mouse);
            }
            else
            {
                SendCallback(down, mouse);
            }

            lastClick = DateTime.Now;
        }

        private bool EnsureWindow()
        {
            return HWnd != IntPtr.Zero && Win32.IsWindow(HWnd);
        }

        private void SendCallback(int message, uint mouse)
        {
            var wParam = Version > 3 ? mouse : Uid;
            var lParam = (uint)message;
            if (Version > 3)
            {
                lParam |= Uid << 16;
            }

            Win32.SendNotifyMessage(HWnd, (int)CallbackMessage, (UIntPtr)wParam, (IntPtr)lParam);
        }

        private static uint CursorParam()
        {
            var position = Cursor.Position;
            return unchecked((uint)((position.Y << 16) | (position.X & 0xFFFF)));
        }
    }
}
