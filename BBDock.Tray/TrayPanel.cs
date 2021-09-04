using BBDock.Api;
using System;
using System.Diagnostics;
using System.Drawing;

namespace BBDock.Tray
{
    internal class TrayPanel : IDockPanel
    {
        private IDockPanelApi _api;

        public void Setup(IDockPanelApi api)
        {
            _api = api;
        }

        public void Awake()
        {
            _api.Add(new TrayIcon(Icon.ExtractAssociatedIcon(@"D:\Projects\BBDock\Assets\Skins\PanelDark.png")));
            _api.Add(new TrayIcon(Icon.ExtractAssociatedIcon(@"D:\Projects\BBDock\Assets\Skins\PanelDark.xcf")));
            _api.Add(new TrayIcon(Icon.ExtractAssociatedIcon(@"c:\go\favicon.ico")));
        }

        public void Destroy()
        {
            
        }
    }
}
