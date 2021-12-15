﻿using ProtoDock.Api;
using ProtoDock.Config;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ProtoDock.Core
{
    public class DockPanel : IDockPanelApi, IDisposable {
        public IDockApi Dock => _dock;

        private readonly Dock _dock;
        public IReadOnlyList<IDockPanelMediator> Mediators => _mediators;
        private readonly List<IDockPanelMediator> _mediators = new List<IDockPanelMediator>();

        private readonly HashSet<IDockIcon> _icons = new HashSet<IDockIcon>();

        public DockPanel(Dock dock) {
            _dock = dock;
        }

        public DockPanel(Dock dock, Config.DockPanelConfig config) : this(dock) {
            var mediatorsList = new List<IDockPanelMediator>();
            foreach (var mediatorConfig in config.Mediators) {
                var plugin = _dock.PluginFromGUID(mediatorConfig.PluginGUID);
                if (!plugin.ResolveHook<IDockPlugin.IPanelHook>(out var panelHook)) {
                    System.Diagnostics.Debug.WriteLine($"Plugin {plugin} not resolve {nameof(IDockPlugin.IPanelHook)}");
                    continue;
                }

                var mediator = panelHook.Create();
                mediator.Setup(this);
                mediatorsList.Add(mediator);
                if (mediator != null) {
                    _mediators.Add(mediator);
                }
            }

            foreach (var iconConfig in config.Icons) {
                try {
                    //TODO: Version
                    mediatorsList[iconConfig.MediatorId].RestoreIcon(iconConfig.PluginVersion, iconConfig.Data);
                }
                catch {
                    //TODO:
                }
            }
        } 
        internal void UpdateScales() {
            foreach (var m in Mediators) {
                m.UpdateScales(_dock.Graphics.Scales);
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
            mediator.UpdateScales(_dock.Graphics.Scales);
            mediator.Awake();

            _mediators.Add(mediator);
            Dock.SetDirty();

            _dock.Flush();
        }

        public void Add(IDockIcon icon, bool playAppear)
        {
            if (_icons.Add(icon))
            {
                _dock.Graphics.AddIcon(this, icon, playAppear);
                _dock.Flush();
                Dock.SetDirty();
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
            Dock.SetDirty();
        }

        public void StartFlash(IDockIcon icon) {
            _dock.Graphics.SetFlashIcon(this, icon, true);
        }

        public void StopFlash(IDockIcon icon) {
            _dock.Graphics.SetFlashIcon(this, icon, false);
        }
        
        public bool ScreenRect(IDockIcon icon, out System.Drawing.Rectangle outRect)
        {
            if (_dock.PanelScreenPos(this, out var pos))
            {
                outRect = new System.Drawing.Rectangle(pos.X, pos.Y, _dock.Graphics.IconSize, _dock.Graphics.IconSize);
                return true;
            }
            outRect = default;
            return false;
        }

        internal Config.DockPanelConfig Store()
        {
            var config = new Config.DockPanelConfig
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

        public override string ToString() {
            if (_mediators.Count == 0) {
                return "";
            }
            return _mediators[0].Plugin.Name;
        }
    }
}
