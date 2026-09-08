using System;
using System.IO;
using System.Text;
using Microsoft.Win32;

namespace ProtoDock.Tray
{
    static class NotifyIconPinning
    {
        private const string SettingsKey = @"Control Panel\NotifyIconSettings";
        private const string ExplorerKey = @"Software\Microsoft\Windows\CurrentVersion\Explorer";

        public static bool IsPromoted(TrayNotifyIcon icon)
        {
            try
            {
                using var settings = Registry.CurrentUser.OpenSubKey(SettingsKey);
                if (settings != null)
                {
                    return IsPromotedInSettings(settings, icon);
                }

                using var explorer = Registry.CurrentUser.OpenSubKey(ExplorerKey);
                var autoTray = explorer?.GetValue("EnableAutoTray");
                if (autoTray is int enabled && enabled == 0)
                {
                    return true;
                }
            }
            catch
            {
                // Fall through to the message-level hidden flag.
            }

            return !icon.IsHidden;
        }

        public static string GetProcessPath(IntPtr hWnd)
        {
            if (hWnd == IntPtr.Zero)
            {
                return null;
            }

            Win32.GetWindowThreadProcessId(hWnd, out var processId);
            if (processId == 0)
            {
                return null;
            }

            var process = Win32.OpenProcess(Win32.PROCESS_QUERY_LIMITED_INFORMATION, false, processId);
            if (process == IntPtr.Zero)
            {
                return null;
            }

            try
            {
                var path = new StringBuilder(1024);
                var size = path.Capacity;
                if (!Win32.QueryFullProcessImageName(process, 0, path, ref size))
                {
                    return null;
                }

                return path.ToString();
            }
            finally
            {
                Win32.CloseHandle(process);
            }
        }

        private static bool IsPromotedInSettings(RegistryKey settings, TrayNotifyIcon icon)
        {
            foreach (var name in settings.GetSubKeyNames())
            {
                using var sub = settings.OpenSubKey(name);
                if (sub == null || !Matches(icon, sub))
                {
                    continue;
                }

                return Convert.ToInt32(sub.GetValue("IsPromoted", 0)) == 1;
            }

            return false;
        }

        private static bool Matches(TrayNotifyIcon icon, RegistryKey sub)
        {
            var uidValue = sub.GetValue("UID");
            if (uidValue != null && unchecked((uint)Convert.ToInt32(uidValue)) != icon.Uid)
            {
                return false;
            }

            var executablePath = sub.GetValue("ExecutablePath") as string;
            if (!string.IsNullOrEmpty(executablePath) && !string.IsNullOrEmpty(icon.ProcessPath))
            {
                var registryFile = Path.GetFileName(executablePath.Replace('/', '\\'));
                var processFile = Path.GetFileName(icon.ProcessPath);
                return !string.IsNullOrEmpty(registryFile) &&
                       !string.IsNullOrEmpty(processFile) &&
                       string.Equals(registryFile, processFile, StringComparison.OrdinalIgnoreCase);
            }

            var tooltip = sub.GetValue("InitialTooltip") as string;
            if (!string.IsNullOrEmpty(tooltip) &&
                !string.IsNullOrEmpty(icon.Title) &&
                string.Equals(tooltip, icon.Title, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            return uidValue != null && string.IsNullOrEmpty(executablePath);
        }
    }
}
