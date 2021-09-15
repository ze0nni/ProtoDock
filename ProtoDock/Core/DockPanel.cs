using ProtoDock.Api;
using ProtoDock.Config;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ProtoDock.Core
{
    public class DockPanel : IDockPanelApi, IDisposable
    {
        public IDockApi Dock => _dock;

        private readonly Dock _dock;
        public IReadOnlyList<IDockPanelMediator> Mediators => _mediators;
        private readonly List<IDockPanelMediator> _mediators = new List<IDockPanelMediator>();

        private readonly HashSet<IDockIcon> _icons = new HashSet<IDockIcon>();

        public DockPanel(Dock dock)
        {
            _dock = dock;
        }

        public DockPanel(Dock dock, DockPanelConfig config): this(dock)
        {
            var mediatorsList = new List<IDockPanelMediator>();
            foreach (var mediatorConfig in config.Mediators)
            {
                var mediator = _dock.PluginFromGUID(mediatorConfig.PluginGUID)?.Create() ?? null;
                mediator?.Setup(this);
                mediatorsList.Add(mediator);
                if (mediator != null)
                {
                    _mediators.Add(mediator);
                }
            }

            foreach (var iconConfig in config.Icons)
            {
                try
                {
                    //TODO: Version
                    mediatorsList[iconConfig.MediatorId].RestoreIcon(iconConfig.PluginVersion, iconConfig.Data);
                }
                catch
                {
                    //TODO:
                }
            }
        }

        internal void Awake() {
            foreach (var m in Mediators) {
                m.Awake();
            }
        }
        
        public void Dispose()
        {
            foreach (var m in _mediators)
            {
                m.Destroy();
            }
            _mediators.Clear();
        }

        public void AddMediator(IDockPanelMediator mediator)
        {
            mediator.Setup(this);
            mediator.Awake();

            _mediators.Add(mediator);

            _dock.Flush();
        }

        public void Add(IDockIcon icon, bool playAppear)
        {
            if (_icons.Add(icon))
            {
                _dock.Graphics.AddIcon(this, icon, playAppear);
                _dock.Flush();
            }
        }

        public void Remove(IDockIcon icon, bool playDisappear)
        {
            if (!_icons.Remove(icon))
            {
                return;
            }

            _dock.Graphics.RemoveIcon(this, icon, playDisappear);
            _dock.Flush();
        }

        internal DockPanelConfig Store()
        {
            var config = new DockPanelConfig
            {
                Mediators = new List<DockPluginMediatorConfig>(),
                Icons = new List<DockIconConfig>()
            };
            
            foreach (var m in _mediators)
            {
                config.Mediators.Add(new DockPluginMediatorConfig
                {
                    PluginGUID = m.Plugin.GUID,         
                });
            }

            foreach (var icon in _icons)
            {
                if (icon.Store(out var data))
                {
                    config.Icons.Add(new DockIconConfig()
                    {
                        MediatorId = _mediators.IndexOf(icon.Mediator),
                        PluginVersion = icon.Mediator.Plugin.Version,
                        Data = data
                    });
                }
            }

            return config;
        }
    }
}
