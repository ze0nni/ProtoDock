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
using ProtoDock.Time;
using PInvoke;
using System.DirectoryServices.ActiveDirectory;

namespace ProtoDock.Core
{
    public class Dock: IDockApi, IDisposable
    {
        public IntPtr HInstance { get; }
        private bool _disposed;

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
            _plugins.Add(new TimePlugin());

            foreach (var p in _plugins) {
                if (p.ResolveHook<IDockPlugin.IDockSetupHook>(out var dockSetupHook)) {
                    dockSetupHook.OnDockSetup();
                }
            }
            
            Restore();
            
            foreach (var p in _plugins) {
                if (p.ResolveHook<IDockPlugin.IDockAwakeHook>(out var dockAwakeHook)) {
                    dockAwakeHook.OnDockAwake();
                }
            }
        }

        public void Dispose() {
            Flush();
            _disposed = true;
            
            foreach (var panel in _panels) {
                panel.Dispose();
            }
            _panels.Clear();
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

        public DockPanel AddPanel()
        {
            var panel = new DockPanel(this);
            _panels.Add(panel);
            Graphics.AddPanel(panel);
            panel.Awake();

            Flush();

            return panel;
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
            if (_disposed) {
                return;
            }

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

        public bool PanelScreenPos(DockPanel panel, out Point outPos)
        {
            var left = Graphics.SelectedSkin.Padding.Left;
            var top = Graphics.SelectedSkin.Padding.Top;

            for (var i = 0; i < _panels.Count; i++)
            {
                var p = _panels[i];
                if (panel == p)
                {
                    outPos = Graphics.DockWindow.PointToScreen(
                        new Point(
                            (int)(left - Graphics.OffsetX),
                            (int)(top - Graphics.OffsetY)));
                    return true;
                }
            }
            outPos = default;
            return false;
        }
    }

    public class DropMediator : IDropMediator
    {
        public IReadOnlyList<IDockPanelMediator> Mediators => mediators;

        public readonly List<IDockPanelMediator> mediators = new List<IDockPanelMediator>();
    }
}
