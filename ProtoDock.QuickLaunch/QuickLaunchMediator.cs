using ProtoDock.Api;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace ProtoDock.QuickLaunch
{
    class QuickLaunchMediator : IDockPanelMediator
    {
        public IDockPlugin Plugin { get; private set; }

        public QuickLaunchMediator(IDockPlugin plugin)
        {
            Plugin = plugin;
        }

        private IDockPanelApi _api;
        public void Setup(IDockPanelApi api)
        {
            _api = api;
        }

        private readonly List<QuickLaunchIcon> _icons = new List<QuickLaunchIcon>();

        public void RestoreIcon(int version, string data) {
            var icon = new QuickLaunchIcon(this, data);
            _api.Add(icon, false);
            _icons.Add(icon);
        }

        public void Awake()
        {
            
        }

        public void Destroy()
        {
            foreach (var icon in _icons)
            {
                _api.Remove(icon, false);
                icon.Dispose();
            }
        }

        public void Update()
        {

        }
        public bool DragCanAccept(IDataObject data)
        {
            var formats = data.GetFormats();
            for (var i = 0; i < formats.Length; i++)
            {
                switch (formats[i])
                {
                    case "FileDrop":
                    //case "FileNameW":
                        return true;
                }
            }
            return false;
        }

        public void DragAccept(int index, IDataObject data)
        {
            var formats = data.GetFormats();
            for (var i = 0; i < formats.Length; i++)
            {
                var format = formats[i];
                switch (format)
                {
                    case "FileDrop":
                        {
                            var files = (string[])data.GetData(format);
                            foreach (var filename in files) {
                                var icon = new QuickLaunchIcon(this, filename);
                                _api.Add(icon, true);
                                _icons.Add(icon);
                            }
                            return;
                        }

                    //case "FileNameW":
                    //    {
                    //        var f = data.GetData(format);
                    //        return;
                    //    }
                }
            }
        }
    }
}
