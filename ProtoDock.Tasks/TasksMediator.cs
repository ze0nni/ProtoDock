using ProtoDock.Api;
using PInvoke;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static PInvoke.User32;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ProtoDock.Tasks
{
    internal class TasksMediator : Form, IDockPanelMediator
    {
        public IDockPlugin Plugin { get; private set;  }

        private IDockPanelApi _api;
        
        private int _shellHookMsg;

        public TasksMediator(IDockPlugin plugin): base()
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
            SetTaskmanWindow(Handle);
            RegisterShellHookWindow(Handle);

            _shellHookMsg = RegisterWindowMessage("SHELLHOOK");

            //int msg = RegisterWindowMessage("TaskbarCreated");
            // SendMessage(new IntPtr(0xffff), msg, IntPtr.Zero, IntPtr.Zero);
            // SendMessage(GetDesktopWindow(), 0x0400, IntPtr.Zero, IntPtr.Zero);
        }

        public void Destroy()
        {
            DeregisterShellHookWindow(Handle);
            Close();
            Dispose();
        }

        public bool DragCanAccept(IDataObject data)
        {
            return false;
        }

        public void DragAccept(int index, IDataObject data)
        {

        }
        
        private enum HShellMsg: int
        {   
            HSHELL_WINDOWCREATED = 1,
            HSHELL_WINDOWDESTROYED = 2,
            HSHELL_ACTIVATESHELLWINDOW = 3,
            //Windows N,
            HSHELL_WINDOWACTIVATED = 4,
            HSHELL_GETM_NRECT = 5,
            HSHELL_REDRAW = 6,
            HSHELL_TASKMAN = 7,
            HSHELL_LANGUAGE = 8,
            HSHELL_SYSMENU = 9,
            HSHELL_ENDTASK = 10,
            //Windows 200,
            HSHELL_ACCESSIBILITYSTATE = 11,
            HSHELL_APPCOMMAND = 12,
            //Windows X,
            HSHELL_WINDOWREPLACED = 13,
            HSHELL_WINDOWREPLACING = 14,
            HSHELL_HIGHBIT = 0x8000,
            HSHELL_FLASH = (HSHELL_REDRAW | HSHELL_HIGHBIT),
            HSHELL_RUDEAPPACTIVATED = (HSHELL_WINDOWACTIVATED | HSHELL_HIGHBIT),
        }
        
        protected override void WndProc(ref Message m) {
            if (m.Msg == _shellHookMsg) {
                ShellWndProc(ref m);
            }
            else
                base.WndProc(ref m);
        }

        private readonly Dictionary<IntPtr, TaskIcon> _icons = new Dictionary<IntPtr, TaskIcon>();

        private void ShellWndProc(ref Message m) {
            var shellMsg = (HShellMsg)m.WParam;
            var win = m.LParam;
            switch (shellMsg) {
                case HShellMsg.HSHELL_WINDOWCREATED:
                {
                    var icon = new TaskIcon(this, win);
                    _icons.Add(win, icon);
                    _api.Add(icon);
                    break;
                }

                case HShellMsg.HSHELL_WINDOWDESTROYED:
                {
                    if (_icons.Remove(win, out var icon)) {
                        _api.Remove(icon);
                        icon.Dispose();
                    }

                    break;
                }
            }
        }
        
        [DllImport("user32.dll")]
        private static extern bool SetTaskmanWindow(IntPtr hWnd);
        
        
        [DllImport("user32.dll")]
        private static extern bool RegisterShellHookWindow(IntPtr hWnd);
        
        [DllImport("user32.dll")]
        private static extern bool DeregisterShellHookWindow(IntPtr hWnd);
        
        [DllImport("Shell32.dll")]
        private static extern bool RegisterShellHook(IntPtr hWnd, uint flags);

        
        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hwnd, int message, IntPtr wparam, IntPtr lparam);
        
        [DllImport("user32.dll")]
        private static extern IntPtr GetDesktopWindow();
    }
}
