using BBDock.Api;
using PInvoke;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using static PInvoke.User32;

namespace BBDock.Tasks
{
    internal class TasksPanel : IDockPanel
    {
        private IDockPanelApi _api;

        private SafeHookHandle _hook;

        public void Setup(IDockPanelApi api)
        {
            _api = api;
        }

        public void Awake()
        {
            using var process = Process.GetCurrentProcess();
            using var module = process.MainModule;

            _hook = User32.SetWindowsHookEx(
                WindowsHookType.WH_SHELL,
                WindowsShellHook,
                Kernel32.GetModuleHandle(module.ModuleName),
                0
            );

            Debug.WriteLine(Kernel32.GetLastError());
        }

        public void Destroy()
        {
            _hook.Dispose();
        }

        private int WindowsShellHook(int nCode, IntPtr wParam, IntPtr lParam)
        {
            Debug.WriteLine(nCode);

            return User32.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }
    }
}
