using PInvoke;
using ProtoDock.Api;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ProtoDock.Tasks
{
    internal class TaskIcon : IDockIcon, IDisposable
    {

        private readonly StringBuilder _sb = new StringBuilder(2048);

        public float Width => 1;
        public bool Hovered => true;

        public IDockPanelMediator Mediator => _mediator;
        private readonly TasksMediator _mediator;
        
        private readonly IntPtr _hWnd;
        private readonly IDockApi _api;

        private Bitmap _icon;

        private IntPtr _activeWindow;
        private bool _isActive;
        
        private bool _flashed;
        private readonly Stopwatch _flashTime = new Stopwatch();
        
        public TaskIcon(TasksMediator mediator, IDockApi api, IntPtr hWnd)
        {
            _mediator = mediator;
            _hWnd = hWnd;
            _api = api;
            Redraw();
        }

        public void Dispose()
        {
            _icon?.Dispose();
        }

        public void Update()
        {
            if (_flashed) {
                if (_flashTime.ElapsedMilliseconds > 2000) {
                    _flashed = false;
                    _flashTime.Stop();
                    _mediator.Api.StopFlash(this);
                }
            }
        }

        public void MouseEnter() {
        }

        public void MouseLeave() {
            
        }

        public void MouseDown(int x, int y, MouseButtons button) {
        }

        public bool MouseUp(int x, int y, MouseButtons button) {
            if (button == MouseButtons.Left) {
                Click();
            }
            return true;
        }

        public void MouseMove(int x, int y, MouseButtons button) {
        }
        
        private void Click()
        {
            var style = (User32.WindowStyles)User32.GetWindowLong(_hWnd, User32.WindowLongIndexFlags.GWL_STYLE);
            var minimized = style.HasFlag(User32.WindowStyles.WS_MINIMIZE);
            if (minimized)
            {
                User32.ShowWindow(_hWnd, User32.WindowShowStyle.SW_RESTORE);
                PInvoke.User32.SetForegroundWindow(_hWnd);
            }
            else if (_activeWindow != _hWnd)
            {
                PInvoke.User32.SetForegroundWindow(_hWnd);
            }
            else
            {
                User32.ShowWindow(_hWnd, User32.WindowShowStyle.SW_MINIMIZE);
            }
        }

        public string Title
        {
            get
            {
                _sb.Clear();
                GetWindowText(_hWnd, _sb, _sb.Capacity);
                return _sb.ToString();
            }
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

        internal void Flash() {
            _flashed = true;
            _flashTime.Reset();
            _flashTime.Start();
            _mediator.Api.StartFlash(this);
        }
        
        internal void UpdateActiveWindow(IntPtr wnd)
        {
            if (wnd == _api.HWnd)
            {
                return;
            }

            _activeWindow = wnd;
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
                var hProcess = Kernel32.OpenProcess(0x0400, false, processId);
                uint size = (uint)_sb.Capacity;
                try
                {
                    var result = QueryFullProcessImageName(hProcess, 0, _sb, ref size);

                    var icon = Icon.ExtractAssociatedIcon(_sb.ToString());
                    _icon = icon.ToBitmap();
                    icon.Dispose();
                }
                catch
                {
                    //
                }
                finally
                {
                    hProcess.Dispose();
                }

            }
        }

        public const int GCL_HICONSM = -34;
        public const int GCL_HICON = -14;

        [DllImport("user32.dll", EntryPoint = "GetClassLong")]
        public static extern uint GetClassLongPtr32(IntPtr hWnd, int nIndex);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryFullProcessImageName([In] Kernel32.SafeObjectHandle hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetActiveWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    }
}
