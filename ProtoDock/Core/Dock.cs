using ProtoDock.Api;
using ProtoDock.Config;
using ProtoDock.QuickLaunch;
using ProtoDock.Tasks;
using ProtoDock.Tray;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;

namespace ProtoDock.Core
{
    public class Dock: IDockApi
    {
        public IntPtr HInstance { get; }

        public IntPtr HWnd { get; }

        public Position Position => Graphics.Position;

        public IReadOnlyList<IDockPlugin> Plugins => _plugins.AsReadOnly();
        public IDockPlugin PluginFromGUID(string guid) => _plugins.FirstOrDefault(p => p.GUID == guid);

        public readonly DockGraphics Graphics;

        private readonly List<IDockPlugin> _plugins = new List<IDockPlugin>();        

        public IReadOnlyList<DockPanel> Panels => _panels;
        private readonly List<DockPanel> _panels = new List<DockPanel>();

        public Dock(IntPtr hInstance, IntPtr hWnd, DockGraphics graphics)
        {
            HInstance = hInstance;
            HWnd = HWnd;
            Graphics = graphics;

            _plugins.Add(new QuickLaunchPlugin());
            _plugins.Add(new TasksPlugin());
            _plugins.Add(new TrayPlugin());          

            Restore();
        }

        private readonly DropMediator _dropMediator = new DropMediator();
        public IDropMediator GetDropMediator(DockPanel forPanel)
        {
            _dropMediator.mediators.Clear();
            for (var i = 0; i < _panels.Count; i++)
            {
                var panel = _panels[i];
                if (panel != forPanel)
                    continue;

                var mediators = panel.Mediators;
                for (var j = 0; j < mediators.Count; j++)
                {
                    _dropMediator.mediators.Add(mediators[j]);
                }
            }

            return _dropMediator;
        }

        public void AddPanel()
        {
            var panel = new DockPanel(this);
            _panels.Add(panel);
            Graphics.AddPanel(panel);
            panel.Awake();

            Flush();
        }

        public void AddPanel(DockPanelConfig config)
        {
            var panel = new DockPanel(this, config);
            _panels.Add(panel);
            Graphics.AddPanel(panel);
            panel.Awake();
        }

        public void SetDirty()
        {
            Graphics.SetDirty();
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

        public void DrawSkin(SkinElement element, Graphics g, float x, float y, float width, float height)
        {
            Graphics.SelectedSkin.Draw(element, g, x, y, width, height);
        }
    }

    public class DropMediator : IDropMediator
    {
        public IReadOnlyList<IDockPanelMediator> Mediators => mediators;

        public readonly List<IDockPanelMediator> mediators = new List<IDockPanelMediator>();
    }
}
