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

        private TaskManForm _form;

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

            
            _form = new TaskManForm();
            RegisterShellHookWindow(_form.Handle);
        }

        public void Destroy()
        {
            //var removeListener = Kernel32.GetProcAddress(_shellHookLib, "RemoveHook");
            //FreeLibrary(_shellHookLib);
            RegisterShellHookWindow(IntPtr.Zero);
            _form.Close();
        }

        public bool DragCanAccept(IDataObject data)
        {
            return false;
        }

        public void DragAccept(int index, IDataObject data)
        {

        }
        
            
        [DllImport("user32.dll")]
        public static extern bool SetTaskmanWindow(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        public static extern bool RegisterShellHookWindow(IntPtr hWnd);
    }
    
    internal class TaskManForm : Form {
        private int _shellHookMsg;
        
        public TaskManForm() {
            _shellHookMsg = User32.RegisterWindowMessage("SHELLHOOK");
        }

        protected override void WndProc(ref Message m) {
            if (m.Msg == _shellHookMsg) {
                Debug.WriteLine($"{m.LParam} {m.WParam}");
            }
            else
                base.WndProc(ref m);
        }
    }

}
