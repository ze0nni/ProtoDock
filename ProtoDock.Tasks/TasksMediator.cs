using ProtoDock.Api;
using PInvoke;
using System;
using System.Diagnostics;
using static PInvoke.User32;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ProtoDock.Tasks
{
    internal class TasksMediator : IDockPanelMediator
    {
        public IDockPlugin Plugin { get; private set;  }

        private IDockPanelApi _api;

        public TasksMediator(IDockPlugin plugin)
        {
            Plugin = plugin;
        }

        private Kernel32.SafeLibraryHandle _shellHookLib;
        private delegate IntPtr SetListener();
        private delegate void RemoveListener();

        public void Setup(IDockPanelApi api)
        {
            _api = api;
        }

        public void RestoreIcon(int version, string data)
        {

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


        public bool DragCanAccept(IDataObject data)
        {
            return false;
        }

        public void DragAccept(int index, IDataObject data)
        {

        }
    }
}
