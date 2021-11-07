using Microsoft.Win32;
using ProtoDock.Api;
using System;
using System.IO;
using System.Windows.Forms;

namespace ProtoDock.Autostart
{
    public class AutostartPlugin : IDockPlugin, IDockSettingsSource, IDockPlugin.ISettingsHook
    {
        public string Name => "Autostart";

        public string GUID => "{3B4A2B68-8BD9-43C3-9727-241922ED2C32}";

        public int Version => 1;


        public bool ResolveHook<T>(out T hook) where T : class
        {
            switch (typeof(T))
            {
                case var cls when cls == typeof(IDockPlugin.ISettingsHook):
                    hook = this as T;
                    return true;
            }

            hook = default;
            return false;
        }


        const string AppName = "ProtoDock";
        private string AppPath => Path.ChangeExtension(Application.ExecutablePath, ".exe");

        private bool IsAutostarted()
        {
            using (RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                return (rk.GetValue(AppName) as String) == AppPath;
            }
        }

        private void SetAutostarted(bool value)
        {
            using (RegistryKey rk = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true))
            {
                if (value)
                {
                    rk.SetValue(AppName, AppPath);
                }
                else
                {
                    rk.DeleteValue(AppName);
                }
            }
        }

        public void Display(IDockSettingsDisplay display)
        {
            display.Toggle("Start with windows",
                IsAutostarted(),
                out _,
                out _,
                SetAutostarted);
        }

        public void OnSettings(IDockSettingsContext context)
        {
            context.Register(this);
        }

        public void OnSettingsRestore(int version, string data)
        {
            
        }

        public bool OnSettingsStore(out string data)
        {
            data = default;
            return false;
        }

        public override string ToString()
        {
            return "Autostart";
        }
    }
}
