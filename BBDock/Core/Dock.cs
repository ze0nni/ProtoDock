using BBDock.Api;
using BBDock.Tray;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BBDock.Core
{
    class Dock: IDockApi
    {
        public IntPtr HInstance => _window.HInstance;
        public ReadOnlyCollection<IDockPlugin> Plugins => _plugins.AsReadOnly();

        private DockWindow _window;
        private readonly List<IDockPlugin> _plugins = new List<IDockPlugin>();

        private readonly List<DockPanel> _panels = new List<DockPanel>();

        public Dock(DockWindow window)
        {
            _window = window;

            _plugins.Add(new TrayPlugin());

            foreach (var p in _plugins)
            {
                AddPanel(p.Create());
            }

            _window.SetIconsCount(3);
        }

        private void AddPanel(IDockPanel panel)
        {
            var view = new DockPanel(this, panel);
        }
    }
}
