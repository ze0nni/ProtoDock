using ProtoDock.Api;
using PInvoke;
using System;
using System.Diagnostics;
using static PInvoke.User32;
using System.Runtime.InteropServices;

namespace ProtoDock.Tasks
{
    internal class TasksPanel : IDockPanel
    {
        private IDockPanelApi _api;

        private Kernel32.SafeLibraryHandle _shellHookLib;
        private delegate IntPtr SetListener();
        private delegate void RemoveListener();

        public void Setup(IDockPanelApi api)
        {
            _api = api;
        }

        public void Awake()
        {
            //_shellHookLib = Kernel32.LoadLibrary("ShellHook.dll");

            //var setListener = Marshal.GetDelegateForFunctionPointer<SetListener>(Kernel32.GetProcAddress(_shellHookLib, "SetHook"));
            //setListener.Invoke();
        }

        public void Destroy()
        {
            //var removeListener = Kernel32.GetProcAddress(_shellHookLib, "RemoveHook");
            //FreeLibrary(_shellHookLib);
        }

        private int WindowsShellHook(int nCode, IntPtr wParam, IntPtr lParam)
        {
            Debug.WriteLine(nCode);

            return User32.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }

        [DllImport("kernel32.dll", EntryPoint = "FreeLibrary")]
        static extern bool FreeLibrary(Kernel32.SafeLibraryHandle hModule);
    }
}
