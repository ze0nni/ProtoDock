using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ProtoDock.Tray
{
    internal sealed class TrayHost : IDisposable
    {
        public const string Shell_TrayWnd = "Shell_TrayWnd";
        public const string TrayNotifyWnd = "TrayNotifyWnd";

        public static TrayHost Instance { get; } = new TrayHost();

        public event Action<TrayNotifyIcon> IconAdded;
        public event Action<TrayNotifyIcon> IconUpdated;
        public event Action<TrayNotifyIcon> IconRemoved;

        private readonly object _sync = new object();
        private readonly List<TrayNotifyIcon> _icons = new List<TrayNotifyIcon>();
        private readonly IntPtr _hInstance = Marshal.GetHINSTANCE(typeof(TrayHost).Module);

        private Win32.WndProc _wndProc;
        private Timer _monitor;
        private IntPtr _hwndTray;
        private IntPtr _hwndNotify;
        private IntPtr _hwndFwd;
        private int _refCount;
        private bool _started;

        private TrayHost()
        {
        }

        public TrayNotifyIcon[] Snapshot()
        {
            lock (_sync)
            {
                return _icons.ToArray();
            }
        }

        public void AddRef()
        {
            lock (_sync)
            {
                if (_refCount++ == 0)
                {
                    Start();
                }
            }
        }

        public void Release()
        {
            lock (_sync)
            {
                if (_refCount == 0)
                {
                    return;
                }

                if (--_refCount == 0)
                {
                    Stop();
                }
            }
        }

        public void SetBounds(Rectangle bounds)
        {
            if (_hwndTray != IntPtr.Zero)
            {
                Win32.SetWindowPos(
                    _hwndTray,
                    IntPtr.Zero,
                    bounds.Left,
                    bounds.Top,
                    bounds.Width,
                    bounds.Height,
                    Win32.SWP_NOACTIVATE | Win32.SWP_NOZORDER);
            }

            if (_hwndNotify != IntPtr.Zero)
            {
                Win32.SetWindowPos(
                    _hwndNotify,
                    IntPtr.Zero,
                    0,
                    0,
                    bounds.Width,
                    bounds.Height,
                    Win32.SWP_NOACTIVATE | Win32.SWP_NOZORDER);
            }
        }

        public void Dispose()
        {
            lock (_sync)
            {
                _refCount = 0;
                Stop();
            }
        }

        private void Start()
        {
            if (_started)
            {
                return;
            }

            _wndProc = WndProc;
            DestroyWindows();
            CreateTrayWindow();
            CreateNotifyWindow();

            if (_hwndTray == IntPtr.Zero)
            {
                return;
            }

            SetExplorerTrayBottommost();
            MakeTopmost();
            SendTaskbarCreated();

            _monitor = new Timer { Interval = 100 };
            _monitor.Tick += OnMonitorTick;
            _monitor.Start();
            _started = true;
        }

        private void Stop()
        {
            if (_monitor != null)
            {
                _monitor.Stop();
                _monitor.Tick -= OnMonitorTick;
                _monitor.Dispose();
                _monitor = null;
            }

            lock (_sync)
            {
                foreach (var icon in _icons)
                {
                    icon.Dispose();
                }
                _icons.Clear();
            }

            DestroyWindows();
            SendTaskbarCreated();
            _started = false;
            _wndProc = null;
        }

        private void CreateTrayWindow()
        {
            RegisterWindowClass(Shell_TrayWnd);

            var width = Win32.GetSystemMetrics(Win32.SM_CXSCREEN);
            _hwndTray = Win32.CreateWindowEx(
                Win32.WS_EX_TOPMOST | Win32.WS_EX_TOOLWINDOW,
                Shell_TrayWnd,
                string.Empty,
                Win32.WS_POPUP | Win32.WS_CLIPCHILDREN | Win32.WS_CLIPSIBLINGS,
                0,
                0,
                width,
                23,
                IntPtr.Zero,
                IntPtr.Zero,
                _hInstance,
                IntPtr.Zero);
        }

        private void CreateNotifyWindow()
        {
            if (_hwndTray == IntPtr.Zero)
            {
                return;
            }

            RegisterWindowClass(TrayNotifyWnd);

            var width = Win32.GetSystemMetrics(Win32.SM_CXSCREEN);
            _hwndNotify = Win32.CreateWindowEx(
                0,
                TrayNotifyWnd,
                null,
                Win32.WS_CHILD | Win32.WS_CLIPCHILDREN | Win32.WS_CLIPSIBLINGS,
                0,
                0,
                width,
                23,
                _hwndTray,
                IntPtr.Zero,
                _hInstance,
                IntPtr.Zero);
        }

        private void RegisterWindowClass(string className)
        {
            var wndClass = new Win32.WNDCLASS
            {
                lpszClassName = className,
                hInstance = _hInstance,
                style = Win32.CS_DBLCLKS,
                lpfnWndProc = _wndProc
            };

            if (Win32.RegisterClass(ref wndClass) == 0)
            {
                var error = Marshal.GetLastWin32Error();
                if (error != Win32.ERROR_CLASS_ALREADY_EXISTS)
                {
                    System.Diagnostics.Debug.WriteLine($"TrayHost: failed to register {className} ({error})");
                }
            }
        }

        private void DestroyWindows()
        {
            if (_hwndNotify != IntPtr.Zero)
            {
                Win32.DestroyWindow(_hwndNotify);
                Win32.UnregisterClass(TrayNotifyWnd, _hInstance);
                _hwndNotify = IntPtr.Zero;
            }

            if (_hwndTray != IntPtr.Zero)
            {
                Win32.DestroyWindow(_hwndTray);
                Win32.UnregisterClass(Shell_TrayWnd, _hInstance);
                _hwndTray = IntPtr.Zero;
            }

            _hwndFwd = IntPtr.Zero;
        }

        private IntPtr WndProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            switch (msg)
            {
                case Win32.WM_COPYDATA:
                    var copyResult = HandleCopyData(lParam);
                    if (copyResult != null)
                    {
                        return copyResult.Value;
                    }
                    break;

                case Win32.WM_WINDOWPOSCHANGED:
                    HideIfShown(lParam);
                    break;
            }

            if (msg == Win32.WM_COPYDATA ||
                msg == Win32.WM_ACTIVATEAPP ||
                msg == Win32.WM_COMMAND ||
                msg >= Win32.WM_USER)
            {
                return Forward(hWnd, msg, wParam, lParam);
            }

            return Win32.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        private IntPtr? HandleCopyData(IntPtr lParam)
        {
            if (lParam == IntPtr.Zero)
            {
                return null;
            }

            var copyData = Marshal.PtrToStructure<Win32.COPYDATASTRUCT>(lParam);
            switch (copyData.dwData.ToInt32())
            {
                case Win32.COPYDATA_TRAYICON:
                    HandleTrayIcon(copyData);
                    return null;

                case Win32.COPYDATA_ICONID:
                    return HandleIconIdentity(copyData);
            }

            return null;
        }

        private void HandleTrayIcon(Win32.COPYDATASTRUCT copyData)
        {
            if (copyData.lpData == IntPtr.Zero || copyData.cbData <= 0)
            {
                return;
            }

            var trayData = ReadStructure<Win32.SHELLTRAYDATA>(copyData.lpData, copyData.cbData);
            var nid = trayData.nid;

            if (nid.hWnd == 0 && (nid.guidItem == Guid.Empty || trayData.dwMessage == Win32.NIM_ADD))
            {
                return;
            }

            switch (trayData.dwMessage)
            {
                case Win32.NIM_ADD:
                case Win32.NIM_MODIFY:
                    UpsertIcon(nid);
                    break;
                case Win32.NIM_DELETE:
                    RemoveIcon(nid);
                    break;
                case Win32.NIM_SETVERSION:
                    SetVersion(nid);
                    break;
            }
        }

        private void UpsertIcon(Win32.NOTIFYICONDATA nid)
        {
            TrayNotifyIcon icon;
            var created = false;
            var updated = false;

            lock (_sync)
            {
                icon = FindIcon(nid.hWnd, nid.uID, nid.guidItem);
                if (icon == null)
                {
                    if (nid.hWnd == 0)
                    {
                        return;
                    }

                    icon = new TrayNotifyIcon
                    {
                        HWnd = Win32.ToHandle(nid.hWnd),
                        Uid = nid.uID,
                        Placement = DefaultPlacement()
                    };
                    _icons.Add(icon);
                    created = true;
                }

                ApplyNotifyData(icon, nid);
                updated = !created;
            }

            if (created)
            {
                IconAdded?.Invoke(icon);
            }
            else if (updated)
            {
                IconUpdated?.Invoke(icon);
            }
        }

        private void RemoveIcon(Win32.NOTIFYICONDATA nid)
        {
            TrayNotifyIcon removed = null;

            lock (_sync)
            {
                var icon = FindIcon(nid.hWnd, nid.uID, nid.guidItem);
                if (icon == null)
                {
                    return;
                }

                _icons.Remove(icon);
                removed = icon;
            }

            if (removed != null)
            {
                IconRemoved?.Invoke(removed);
                removed.Dispose();
            }
        }

        private void SetVersion(Win32.NOTIFYICONDATA nid)
        {
            if (nid.uVersion > 4)
            {
                return;
            }

            lock (_sync)
            {
                var icon = FindIcon(nid.hWnd, nid.uID, nid.guidItem);
                if (icon != null)
                {
                    icon.Version = nid.uVersion;
                }
            }
        }

        private void ApplyNotifyData(TrayNotifyIcon icon, Win32.NOTIFYICONDATA nid)
        {
            if ((nid.uFlags & Win32.NIF_STATE) != 0)
            {
                icon.IsHidden = nid.dwState == Win32.NIS_HIDDEN;
            }

            if ((nid.uFlags & Win32.NIF_TIP) != 0 && !string.IsNullOrEmpty(nid.szTip))
            {
                icon.Title = nid.szTip;
            }

            if ((nid.uFlags & Win32.NIF_ICON) != 0)
            {
                icon.SetImage(Win32.ToHandle(nid.hIcon));
            }

            if (nid.hWnd != 0)
            {
                icon.HWnd = Win32.ToHandle(nid.hWnd);
                icon.Uid = nid.uID;
                icon.ProcessPath ??= NotifyIconPinning.GetProcessPath(icon.HWnd);
            }

            if ((nid.uFlags & Win32.NIF_GUID) != 0)
            {
                icon.Guid = nid.guidItem;
            }

            if (nid.uVersion > 0 && nid.uVersion <= 4)
            {
                icon.Version = nid.uVersion;
            }

            if ((nid.uFlags & Win32.NIF_MESSAGE) != 0)
            {
                icon.CallbackMessage = nid.uCallbackMessage;
            }

            if (icon.Image == null)
            {
                icon.NotifyChanged();
            }
        }

        private IntPtr? HandleIconIdentity(Win32.COPYDATASTRUCT copyData)
        {
            if (copyData.lpData == IntPtr.Zero || copyData.cbData <= 0)
            {
                return null;
            }

            var identity = ReadStructure<Win32.WINNOTIFYICONIDENTIFIER>(copyData.lpData, copyData.cbData);
            TrayNotifyIcon icon;

            lock (_sync)
            {
                icon = FindIcon(identity.hWnd, identity.uID, identity.guidItem);
            }

            if (icon == null)
            {
                return null;
            }

            if (identity.dwMessage == 1)
            {
                return Win32.MakeLParam(icon.Placement.Left, icon.Placement.Top);
            }

            if (identity.dwMessage == 2)
            {
                return Win32.MakeLParam(icon.Placement.Right, icon.Placement.Bottom);
            }

            return null;
        }

        private TrayNotifyIcon FindIcon(uint hWnd, uint uid, Guid guid)
        {
            for (var i = 0; i < _icons.Count; i++)
            {
                if (_icons[i].Matches(hWnd, uid, guid))
                {
                    return _icons[i];
                }
            }

            return null;
        }

        private IntPtr Forward(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            if (_hwndFwd == IntPtr.Zero || !Win32.IsWindow(_hwndFwd))
            {
                _hwndFwd = FindExplorerTray();
            }

            if (_hwndFwd != IntPtr.Zero)
            {
                if (msg >= Win32.WM_USER && msg == Win32.TRAY_FORWARD_POST)
                {
                    Win32.PostMessage(_hwndFwd, (uint)msg, wParam, lParam);
                    return Win32.DefWindowProc(hWnd, msg, wParam, lParam);
                }

                return Win32.SendMessage(_hwndFwd, msg, wParam, lParam);
            }

            return Win32.DefWindowProc(hWnd, msg, wParam, lParam);
        }

        private IntPtr FindExplorerTray()
        {
            var hwnd = Win32.FindWindow(Shell_TrayWnd, null);
            while (hwnd != IntPtr.Zero)
            {
                if (hwnd != _hwndTray)
                {
                    return hwnd;
                }

                hwnd = Win32.FindWindowEx(IntPtr.Zero, hwnd, Shell_TrayWnd, null);
            }

            return IntPtr.Zero;
        }

        private void HideIfShown(IntPtr lParam)
        {
            if (lParam == IntPtr.Zero || _hwndTray == IntPtr.Zero)
            {
                return;
            }

            var pos = Marshal.PtrToStructure<Win32.WINDOWPOS>(lParam);
            if ((pos.flags & Win32.SWP_SHOWWINDOW) == 0)
            {
                return;
            }

            var style = Win32.GetWindowLongPtr(_hwndTray, Win32.GWL_STYLE).ToInt64();
            style &= ~Win32.WS_VISIBLE;
            Win32.SetWindowLongPtr(_hwndTray, Win32.GWL_STYLE, new IntPtr(style));
        }

        private void OnMonitorTick(object sender, EventArgs e)
        {
            if (_hwndTray == IntPtr.Zero)
            {
                return;
            }

            if (Win32.FindWindow(Shell_TrayWnd, string.Empty) != _hwndTray)
            {
                MakeTopmost();
            }
        }

        private void SetExplorerTrayBottommost()
        {
            var explorerTray = FindExplorerTray();
            if (explorerTray != IntPtr.Zero)
            {
                Win32.SetWindowPos(
                    explorerTray,
                    Win32.HWND_BOTTOM,
                    0,
                    0,
                    0,
                    0,
                    Win32.SWP_NOMOVE | Win32.SWP_NOSIZE | Win32.SWP_NOACTIVATE);
            }
        }

        private void MakeTopmost()
        {
            if (_hwndTray == IntPtr.Zero)
            {
                return;
            }

            Win32.SetWindowPos(
                _hwndTray,
                Win32.HWND_TOPMOST,
                0,
                0,
                0,
                0,
                Win32.SWP_NOMOVE | Win32.SWP_NOSIZE | Win32.SWP_NOACTIVATE);
        }

        private static void SendTaskbarCreated()
        {
            var msg = Win32.RegisterWindowMessage("TaskbarCreated");
            if (msg > 0)
            {
                Win32.SendNotifyMessage(Win32.HWND_BROADCAST, msg, UIntPtr.Zero, IntPtr.Zero);
            }
        }

        private static Rectangle DefaultPlacement()
        {
            var screenWidth = Win32.GetSystemMetrics(Win32.SM_CXSCREEN);
            return Rectangle.FromLTRB(screenWidth - 200, 0, screenWidth - 177, 23);
        }

        private static T ReadStructure<T>(IntPtr source, int cbData) where T : struct
        {
            var size = Marshal.SizeOf<T>();
            var buffer = Marshal.AllocHGlobal(size);
            try
            {
                for (var i = 0; i < size; i++)
                {
                    Marshal.WriteByte(buffer, i, 0);
                }

                var copySize = (uint)Math.Min(cbData, size);
                if (copySize > 0)
                {
                    Win32.CopyMemory(buffer, source, copySize);
                }

                return Marshal.PtrToStructure<T>(buffer);
            }
            finally
            {
                Marshal.FreeHGlobal(buffer);
            }
        }
    }
}
