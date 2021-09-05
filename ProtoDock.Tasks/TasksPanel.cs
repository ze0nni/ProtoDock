using ProtoDock.Api;
using PInvoke;
using System;
using System.Diagnostics;
using static PInvoke.User32;

namespace ProtoDock.Tasks
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

            var bbHook = Kernel32.LoadLibrary("ShellHook.dll");
            var setListener = Kernel32.GetProcAddress(bbHook, "setListener");
            var removeListener = Kernel32.GetProcAddress(bbHook, "removeListener");
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
