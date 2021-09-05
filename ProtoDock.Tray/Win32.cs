using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ProtoDock.Tray
{
    static class Win32
    {
        public static readonly IntPtr HWND_BROADCAST = (IntPtr)0xffff;


        [DllImport("user32.dll", EntryPoint = "RegisterWindowMessageW", SetLastError = true)]
        public static extern uint RegisterWindowMessage(string lpString);

        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool SendNotifyMessage(IntPtr hWnd, int Msg, UIntPtr wParam, IntPtr lParam);        
    }
}
