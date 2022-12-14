using ProtoDock.Api;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace ProtoDock.Tray
{
    public class TrayPlugin : IDockPlugin, IDockPlugin.IPanelHook
    {
        public static readonly string Shell_TrayWnd = "Shell_TrayWnd";

        public string Name => "Tray";

        public string GUID => "{9A3F1A17-2F32-41B4-ABAA-0FC89EC75CDC}";

        public int Version => 1;

        public IDockApi _api;

        public void Init(IDockApi api)
        {
            _api = api;
        }

        public void Destroy()
        {

        }

        public IDockPanelMediator Create(IDockApi api, string data)
        {
            return new TrayMediator(this);
        }

        public bool ResolveHook<T>(out T hook) where T : class
        {
            switch (typeof(T))
            {
                case var cls when cls == typeof(IDockPlugin.IPanelHook):
                    hook = this as T;
                    return true;
            }

            hook = default;
            return false;
        }

        public override string ToString()
        {
            return "Tray panel";
        }
    }
}
