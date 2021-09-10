﻿using ProtoDock;
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
using System.IO;
using System.Linq;

namespace ProtoDock.Core
{
    public class Dock: IDockApi
    {
        public IntPtr HInstance { get; }
        public IReadOnlyList<IDockPlugin> Plugins => _plugins.AsReadOnly();
        public IDockPlugin PluginFromGUID(string guid) => _plugins.FirstOrDefault(p => p.GUID == guid);

        public readonly DockGraphics Graphics;

        private readonly List<IDockPlugin> _plugins = new List<IDockPlugin>();        

        public IReadOnlyList<DockPanel> Panels => _panels;
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
                var mediators = _panels[i].Mediators;
                for (var j = 0; j < mediators.Count; j++)
                {
                    _dropMediator.panels.Add(mediators[j]);
                }
            }

            return _dropMediator;
        }

        public void AddPanel()
        {
            var panel = new DockPanel(this);
            _panels.Add(panel);

            Flush();
        }

        public void AddPanel(DockPanelConfig config)
        {
            var panel = new DockPanel(this, config);
            _panels.Add(panel);
        }

        public void Restore()
        {            
            try
            {
                var json = File.ReadAllText(ConfigPath());
                var config = System.Text.Json.JsonSerializer.Deserialize<DockConfig>(json);
                foreach (var panelConfig in config.Panels)
                {
                    AddPanel(panelConfig);
                }
            }
            catch (Exception e)
            {
                //
            }
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

            var json = System.Text.Json.JsonSerializer.Serialize(config);            
            System.IO.File.WriteAllText(ConfigPath(), json);
        }

        private string ConfigPath()
        {
            var root = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ProtoDock");
            Directory.CreateDirectory(root);
            return Path.Join(root, "config.json");
        }
    }

    public class DropMediator : IDropMediator
    {
        public IEnumerable<IDockPanelMediator> Mediators => panels;

        public readonly List<IDockPanelMediator> panels = new List<IDockPanelMediator>();
    }
}