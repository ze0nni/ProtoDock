using ProtoDock.Api;
using ProtoDock.Tasks;
using ProtoDock.Tray;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace ProtoDock.Core
{
    class Dock: IDockApi
    {
        public IntPtr HInstance { get; }
        public ReadOnlyCollection<IDockPlugin> Plugins => _plugins.AsReadOnly();

        public readonly DockGraphics Graphics;

        private readonly List<IDockPlugin> _plugins = new List<IDockPlugin>();

        private readonly List<DockPanel> _panels = new List<DockPanel>();

        public Dock(IntPtr hInstance, DockGraphics graphics)
        {
            HInstance = hInstance;
            Graphics = graphics;

            _plugins.Add(new TasksPlugin());
            _plugins.Add(new TrayPlugin());            

            foreach (var p in _plugins)
            {
                AddPanel(p.Create());
            }
        }

        private void AddPanel(IDockPanel model)
        {
            var view = new DockPanel(this, model);
        }
    }
}
