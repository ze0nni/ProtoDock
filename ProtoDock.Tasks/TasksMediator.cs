using ProtoDock.Api;
using PInvoke;
using System;
using System.Collections.Generic;
using static PInvoke.User32;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;

namespace ProtoDock.Tasks
{
    internal class TasksMediator : Form, IDockPanelMediator
    {
        public IDockPlugin Plugin { get; private set;  }

        internal IDockPanelApi Api { get; private set; }
        
        private int _shellHookMsg;

        private readonly List<IntPtr> _windows = new List<IntPtr>();
        private readonly Dictionary<IntPtr, TaskIcon> _icons = new Dictionary<IntPtr, TaskIcon>();
        private readonly Dictionary<IntPtr, TaskIcon> _hiddenIcons = new Dictionary<IntPtr, TaskIcon>();

        private readonly Config _config;
        private readonly IDockApi _api;

        public TasksMediator(IDockApi api, IDockPlugin plugin, string data): base()
        {
            Plugin = plugin;
            _api = api;
            _config = Config.Read(data);
        }

        private Kernel32.SafeLibraryHandle _shellHookLib;

        public void Setup(IDockPanelApi api)
        {
            Api = api;
        }

        public bool RequestSettings => true;
        public void DisplaySettings(IDockSettingsDisplay display)
        {
            display.Toggle(
                "Only minimised",
                _config.OnlyMinimised,
                out _,
                out _,
                v =>
                {
                    _config.OnlyMinimised = v;
                    display.SetDirty();
                    UpdateWindows(true);
                });
        }

        public void RestoreIcon(int version, string data)
        {

        }
        
        public bool Store(out string data)
        {
            data = _config.Write();
            return true;
        }

        public void Awake()
        {
            SetTaskmanWindow(Handle);
            RegisterShellHookWindow(Handle);   

            _shellHookMsg = RegisterWindowMessage("SHELLHOOK");

            EnumWindows(delegate (IntPtr wnd, IntPtr param)
            {
                _windows.Add(wnd);
                return true;
            }, IntPtr.Zero);

            UpdateWindows(false);
            UpdateActiveWindow(User32.GetActiveWindow());
        }
        
        public void Destroy()
        {
            foreach (var icon in _icons)
            {
                Api.Remove(icon.Value, false);
                icon.Value.Dispose();
            }
            
            DeregisterShellHookWindow(Handle);
            Close();
            Dispose();
        }

        public void Update() {
            ;
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
            HSHELL_GETMINRECT = 5,
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

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == _shellHookMsg)
            {
                ShellWndProc(ref m);
            }
            else
                base.WndProc(ref m);
        }

        private void ShellWndProc(ref Message m)
        {
            var shellMsg = (HShellMsg)m.WParam;
            var wnd = m.LParam;
            switch (shellMsg) {
                case HShellMsg.HSHELL_WINDOWCREATED:
                {
                    _windows.Add(wnd);
                    CreateIcon(wnd, true);
                    UpdateWindows(true);
                    break;
                }

                case HShellMsg.HSHELL_WINDOWDESTROYED:
                {
                    _windows.Remove(wnd);
                    DestroyIcon(wnd, true);
                    UpdateWindows(false);

                    break;
                }

                case HShellMsg.HSHELL_REDRAW:
                {
                    if (_icons.TryGetValue(wnd, out var icon)) {
                        icon.Redraw();
                        Api.Dock.SetDirty();
                    }

                    break;
                }

                case HShellMsg.HSHELL_RUDEAPPACTIVATED:
                {
                    UpdateActiveWindow(wnd);
                    UpdateWindows(false);
                    break;
                }
                case HShellMsg.HSHELL_FLASH:
                {
                    var window = m.LParam;
                    
                    if (_icons.TryGetValue(window, out var icon)) {
                        icon.Flash();
                    }

                    break;
                }
                case HShellMsg.HSHELL_GETMINRECT:
                    if (!_api.GetPanelRect(this, out var panelRect))
                    {
                        return;
                    }
                    var rect = Marshal.PtrToStructure<MINRECT>(m.LParam);
                    rect.Left = (short)panelRect.X;
                    rect.Top = (short)panelRect.Y;
                    rect.Right = (short)(panelRect.X + panelRect.Height);
                    rect.Bottom = (short)(panelRect.Y + panelRect.Height);
                    Marshal.StructureToPtr<MINRECT>(rect, m.LParam, false);
                    m.Result = new IntPtr(1);
                    break;
            }
        }

        private void UpdateWindows(bool playAnimation) {
            for (var i = 0; i < _windows.Count; i++) {
                var wnd = _windows[i];

                var visible = true;
                if (!IsWindowVisible(wnd))
                {
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
                    

                    if ((style & (int) WindowStyles.WS_VISIBLE) == 0)
                    {
                        visible = false;
                    }

                    if (_config.OnlyMinimised)
                    {
                        if ((style & (int)WindowStyles.WS_MINIMIZE) == 0)
                        {
                            visible = false;
                        }
                    }
                }
                if (visible)
                {
                    var styleEx = GetWindowLong(wnd, WindowLongIndexFlags.GWL_EXSTYLE);
                    if ((styleEx & (int) WindowStylesEx.WS_EX_TOOLWINDOW) != 0 &&
                        (styleEx & (int)WindowStylesEx.WS_EX_APPWINDOW) == 0 )
                    {
                        visible = false;
                    }

                }

                if (visible)
                {
                    IntPtr cloakedVal;
                    var hRes = DwmGetWindowAttribute(wnd, (int)DWMWINDOWATTRIBUTE.DWMWA_CLOAKED, out cloakedVal, sizeof(int));
                    if (cloakedVal != IntPtr.Zero)
                    {
                        visible = false;
                    }
                }

                if (visible)
                {
                    if (!_hiddenIcons.ContainsKey(wnd)) {
                        CreateIcon(wnd, playAnimation);
                    }
                    else {
                        ShowIcon(wnd, playAnimation);
                    }
                }
                else
                {
                    HideIcon(wnd, playAnimation);
                }
            }
        }

        private void UpdateActiveWindow(IntPtr wnd)
        {
            foreach (var icon in _icons)
            {
                icon.Value.UpdateActiveWindow(wnd);
            }
        }

        private void CreateIcon(IntPtr wnd, bool playAppear) {
            if (_icons.ContainsKey(wnd))
                return;
            
            var icon = new TaskIcon(this, Api.Dock, wnd);
            _icons.Add(wnd, icon);
            Api.Add(icon, playAppear);
        }
        
        private void DestroyIcon(IntPtr wnd, bool playDisappear) {
            if (_icons.Remove(wnd, out var icon)) {
                Api.Remove(icon, playDisappear);
                icon.Dispose();
            }

            if (_hiddenIcons.Remove(wnd, out icon)) {
                icon.Dispose();
            }
        }

        private void ShowIcon(IntPtr wnd, bool playAppear) {
            if (_hiddenIcons.Remove(wnd, out var icon)) {
                _icons.Add(wnd, icon);
                Api.Add(icon, playAppear);
            }
        }

        private void HideIcon(IntPtr wnd, bool playDisappear) {
            if (_icons.Remove(wnd, out var icon)) {
                _hiddenIcons.Add(wnd, icon);
                Api.Remove(icon, playDisappear);
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
        private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
        
        private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

        [DllImport("dwmapi.dll")]
        private static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out IntPtr pvAttribute, int cbAttribute);

        enum DWMWINDOWATTRIBUTE
        {
            DWMWA_NCRENDERING_ENABLED = 1,
            DWMWA_NCRENDERING_POLICY,
            DWMWA_TRANSITIONS_FORCEDISABLED,
            DWMWA_ALLOW_NCPAINT,
            DWMWA_CAPTION_BUTTON_BOUNDS,
            DWMWA_NONCLIENT_RTL_LAYOUT,
            DWMWA_FORCE_ICONIC_REPRESENTATION,
            DWMWA_FLIP3D_POLICY,
            DWMWA_EXTENDED_FRAME_BOUNDS,
            DWMWA_HAS_ICONIC_BITMAP,
            DWMWA_DISALLOW_PEEK,
            DWMWA_EXCLUDED_FROM_PEEK,
            DWMWA_CLOAK,
            DWMWA_CLOAKED,
            DWMWA_FREEZE_REPRESENTATION,
            DWMWA_LAST
        }

        struct MINRECT
        {
            public IntPtr HWND;
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }

        private void InitializeComponent()
        {
            this.SuspendLayout();
            // 
            // TasksMediator
            // 
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Name = "TasksMediator";
            this.ResumeLayout(false);

        }
    }
}
