using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ProtoDock.WinApi
{
    public static class Kernal32
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern IntPtr OpenProcess(
            uint processAccess,
            bool bInheritHandle,
            uint processId
        );
        public static IntPtr OpenProcess(Process proc, ProcessAccessFlags flags)
        {
            return OpenProcess(flags, false, (uint)proc.Id);
        }
    }
}
