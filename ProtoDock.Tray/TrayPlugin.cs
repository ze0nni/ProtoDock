using ProtoDock.Api;
using PInvoke;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using static PInvoke.User32;

namespace ProtoDock.Tray
{
    public class TrayPlugin : IDockPlugin
    {
        public static readonly string Shell_TrayWnd = "Shell_TrayWnd";

        public string Name => "Tray";

        public string GUID => "{9A3F1A17-2F32-41B4-ABAA-0FC89EC75CDC}";

        public int Version => 1;

        public IDockApi _api;

        public void Init(IDockApi api)
        {
            _api = api;

            unsafe
            {
                var wndClass = new User32.WNDCLASS();
                wndClass.style = ClassStyles.CS_PARENTDC;
                wndClass.hbrBackground = (IntPtr)5;
                fixed (char* p = Shell_TrayWnd)
                    wndClass.lpszClassName = p;

                var result = User32.RegisterClass(ref wndClass);
                Debug.WriteLine(result);
            }

            unsafe
            {
                var wnd = User32.CreateWindow(Shell_TrayWnd, Shell_TrayWnd, WindowStyles.WS_CAPTION, 0, 0, 100, 100, IntPtr.Zero, IntPtr.Zero, api.HInstance, IntPtr.Zero);
                Debug.WriteLine(Kernel32.GetLastError());
            }

            var msg = User32.RegisterWindowMessage("TaskbarCreated");
            Win32.SendNotifyMessage(Win32.HWND_BROADCAST, msg, UIntPtr.Zero, IntPtr.Zero);

        }

        public void Destroy()
        {

        }

        public IDockPanelMediator Create()
        {
            return new TrayMediator(this);
        }
    }
}
