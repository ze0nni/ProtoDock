using ProtoDock;
using ProtoDock.Api;
using ProtoDock.Config;
using ProtoDock.QuickLaunch;
using ProtoDock.Tasks;
using ProtoDock.Tray;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Text.Json.Serialization;

namespace ProtoDock.Core
{
    public class Dock: IDockApi
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

            _plugins.Add(new QuickLaunchPlugin());
            _plugins.Add(new TasksPlugin());
            _plugins.Add(new TrayPlugin());          

            Restore();
        }

        private readonly DropMediator _dropMediator = new DropMediator();
        public IDropMediator GetDropMediator()
        {
            _dropMediator.panels.Clear();
            for (var i = 0; i < _panels.Count; i++)
            {
                _dropMediator.panels.Add(_panels[i].Model);
            }

            return _dropMediator;
        }

        public void AddPanel(IDockPlugin plugin)
        {
            var panelModel = plugin.Create();
            var panel = new DockPanel(this, panelModel);
            _panels.Add(panel);

            Flush();
        }

        public void Restore()
        {

        }

        public void Flush()
        {
            var config = new DockConfig
            {
                Panels = new List<DockPanelConfig>()
            };
            
            foreach (var panel in _panels)
            {
                config.Panels.Add(panel.Store());
            }

            ConfigurationManager.AppSettings["panel"] = System.Text.Json.JsonSerializer.Serialize(config);
        }
    }

    public class DropMediator : IDropMediator
    {
        public IEnumerable<IDockPanelMediator> Mediators => panels;

        public readonly List<IDockPanelMediator> panels = new List<IDockPanelMediator>();
    }
}
