using ProtoDock.Api;
using ProtoDock.Config;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ProtoDock.Core
{
    class DockPanel : IDockPanelApi, IDisposable
    {
        public IDockApi Dock => _dock;

        private readonly Dock _dock;
        public readonly IDockPanel Model;

        private readonly HashSet<IDockIcon> _icons = new HashSet<IDockIcon>();

        public DockPanel(Dock dock, DockPanelConfig config)
        {
            _dock = dock;
        }

        public DockPanel(Dock dock, IDockPanel model)
        {
            _dock = dock;
            Model = model;
            model.Setup(this);
            model.Awake();
        }

        public void Dispose()
        {
            Model.Destroy();
        }

        public void Add(IDockIcon icon)
        {
            if (_icons.Add(icon))
            {
                _dock.Graphics.AddIcon(icon);
                _dock.Flush();
            }
        }

        public void Remove(IDockIcon icon)
        {
            
        }

        internal DockPanelConfig Store()
        {
            var config = new DockPanelConfig
            {
                PluginGUID = Model.Plugin.GUID,
                Icons = new List<DockIconConfig>()
            };
            
            foreach (var icon in _icons)
            {
                if (icon.Store(out var data))
                {
                    config.Icons.Add(new DockIconConfig()
                    {
                        Data = data
                    });
                }
            }

            return config;
        }
    }
}
