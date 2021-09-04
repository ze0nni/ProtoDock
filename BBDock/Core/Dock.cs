using BBDock.Api;
using BBDock.Tray;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BBDock.Core
{
    class Dock: IDockApi
    {
        public IntPtr HInstance { get; }
        public ReadOnlyCollection<IDockPlugin> Plugins => _plugins.AsReadOnly();

        private readonly DockGraphics _graphics;
        private readonly List<IDockPlugin> _plugins = new List<IDockPlugin>();

        private readonly List<DockPanel> _panels = new List<DockPanel>();

        public Dock(IntPtr hInstance, DockGraphics graphics)
        {
            HInstance = hInstance;
            _graphics = graphics;

            _plugins.Add(new TrayPlugin());

            foreach (var p in _plugins)
            {
                AddPanel(p.Create());
            }
        }

        private void AddPanel(IDockPanel panel)
        {
            var view = new DockPanel(this, panel);
        }
    }
}
