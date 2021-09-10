using ProtoDock.Api;
using PInvoke;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static PInvoke.User32;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace ProtoDock.Tasks
{
    internal class TasksMediator : Form, IDockPanelMediator
    {
        public IDockPlugin Plugin { get; private set;  }

        private IDockPanelApi _api;
        
        private int _shellHookMsg;

        private readonly List<IntPtr> _windows = new List<IntPtr>();
        private readonly Dictionary<IntPtr, TaskIcon> _icons = new Dictionary<IntPtr, TaskIcon>();

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
            
            EnumWindows(delegate(IntPtr wnd, IntPtr param) {
                _windows.Add(wnd);
                return true;
            }, IntPtr.Zero);
            
            UpdateWindows();

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

        private void ShellWndProc(ref Message m) {
            var shellMsg = (HShellMsg)m.WParam;
            var win = m.LParam;
            switch (shellMsg) {
                case HShellMsg.HSHELL_WINDOWCREATED:
                {
                    _windows.Add(win);
                    UpdateWindows();
                    break;
                }

                case HShellMsg.HSHELL_WINDOWDESTROYED:
                {
                    _windows.Remove(win);
                    DestroyIcon(win);
                    UpdateWindows();

                    break;
                }
            }
        }

        private void UpdateWindows() {
            var sb = new StringBuilder(255);
            
            for (var i = 0; i < _windows.Count; i++) {
                var wnd = _windows[i];

                var visible = true;
                if (!IsWindowVisible(wnd)) {
                    visible = false;
                }

                if (visible)
                {
                    if (GetParent(wnd) != IntPtr.Zero)
                    {
                        visible = false;
                    }
                }
                
                if (visible) {
                    var style = GetWindowLong(wnd, WindowLongIndexFlags.GWL_STYLE);
                    if ((style & (int) WindowStyles.WS_CHILD) != 0)
                    {
                        visible = false;
                    }
                    
                    if ((style & (uint) WindowStyles.WS_POPUP) != 0)
                    {
                        visible = false;
                    }

                    if ((style & (int) WindowStyles.WS_VISIBLE) == 0)
                    {
                        visible = false;
                    }
                }

                if (visible)
                {
                    var styleEx = GetWindowLong(wnd, WindowLongIndexFlags.GWL_EXSTYLE);
                    if ((styleEx & (int) WindowStylesEx.WS_EX_TOOLWINDOW) != 0)
                    {
                        visible = false;
                    }
                }

                if (visible)
                {
                    var style = GetWindowLong(wnd, WindowLongIndexFlags.GWL_STYLE);
                    var styleEx = GetWindowLong(wnd, WindowLongIndexFlags.GWL_EXSTYLE);
                    GetWindowText(wnd, sb, 255);
                    Debug.WriteLine($"{sb.ToString()}\n    {Convert.ToString(style, 2)}\n    {Convert.ToString(styleEx, 2)}");
                }

                if (visible)
                {
                    CreateIcon(wnd);
                }
                else
                {
                    DestroyIcon(wnd);
                }
            }

            Debug.WriteLine("----");
        }

        private void CreateIcon(IntPtr wnd) {
            if (_icons.ContainsKey(wnd))
                return;
            
            var icon = new TaskIcon(this, wnd);
            _icons.Add(wnd, icon);
            _api.Add(icon);
        }
        
        private void DestroyIcon(IntPtr wnd) {
            if (_icons.Remove(wnd, out var icon)) {
                _api.Remove(icon);
                icon.Dispose();
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
        
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
        
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("user32.dll")]
        private static extern IntPtr GetWindowText(IntPtr hWnd, System.Text.StringBuilder text, int count);
    }
}
