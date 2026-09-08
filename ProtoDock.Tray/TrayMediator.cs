using System;
using System.Collections.Generic;
using ProtoDock.Api;
using System.Windows.Forms;

namespace ProtoDock.Tray
{
    internal class TrayMediator : IDockPanelMediator
    {
        public IDockPlugin Plugin { get; private set; }

        public IDockPanelApi Api { get; private set; }

        private readonly Dictionary<TrayNotifyIcon, TrayIcon> _icons = new Dictionary<TrayNotifyIcon, TrayIcon>();
        private readonly Config _config;
        private bool _alive;
        private long _lastPinCheck;

        public TrayMediator(IDockPlugin plugin, string data)
        {
            Plugin = plugin;
            _config = Config.Read(data);
        }

        public void Setup(IDockPanelApi api)
        {
            Api = api;
        }

        public bool RequestSettings => true;

        public void DisplaySettings(IDockSettingsDisplay display)
        {
            display.Toggle(
                "Only pinned",
                _config.OnlyPinned,
                out _,
                out _,
                v =>
                {
                    _config.OnlyPinned = v;
                    display.SetDirty();
                    ApplyFilter();
                });
        }

        public bool Store(out string data)
        {
            data = _config.Write();
            return true;
        }

        public void RestoreIcon(int version, string data)
        {
        }

        public void Awake()
        {
            _alive = true;
            TrayHost.Instance.IconAdded += OnIconAdded;
            TrayHost.Instance.IconUpdated += OnIconUpdated;
            TrayHost.Instance.IconRemoved += OnIconRemoved;
            TrayHost.Instance.AddRef();

            foreach (var icon in TrayHost.Instance.Snapshot())
            {
                AddView(icon);
            }
        }

        public void Destroy()
        {
            _alive = false;
            TrayHost.Instance.IconAdded -= OnIconAdded;
            TrayHost.Instance.IconUpdated -= OnIconUpdated;
            TrayHost.Instance.IconRemoved -= OnIconRemoved;

            foreach (var icon in _icons)
            {
                Api.Remove(icon.Value, false);
                icon.Value.Dispose();
            }
            _icons.Clear();

            TrayHost.Instance.Release();
        }

        public void Update()
        {
            if (!_alive || !_config.OnlyPinned)
            {
                return;
            }

            var now = DateTime.UtcNow.Ticks;
            if (now - _lastPinCheck < TimeSpan.TicksPerSecond * 2)
            {
                return;
            }

            _lastPinCheck = now;
            ApplyFilter();
        }

        public bool DragCanAccept(IDataObject data)
        {
            return false;
        }

        public void DragAccept(int index, IDataObject data)
        {
        }

        private void OnIconAdded(TrayNotifyIcon icon)
        {
            RunOnDock(() => AddView(icon));
        }

        private void OnIconUpdated(TrayNotifyIcon icon)
        {
            RunOnDock(() => ApplyVisibility(icon));
        }

        private void OnIconRemoved(TrayNotifyIcon icon)
        {
            RunOnDock(() => RemoveView(icon));
        }

        private void RunOnDock(Action action)
        {
            Api.Dock.Invoke(() =>
            {
                if (_alive)
                {
                    action();
                }
            });
        }

        private void ApplyFilter()
        {
            foreach (var icon in TrayHost.Instance.Snapshot())
            {
                ApplyVisibility(icon, playAppear: false);
            }
        }

        private void ApplyVisibility(TrayNotifyIcon icon, bool playAppear = true)
        {
            if (ShouldShow(icon))
            {
                AddView(icon, playAppear);
            }
            else
            {
                RemoveView(icon);
            }
        }

        private bool ShouldShow(TrayNotifyIcon icon)
        {
            if (icon.IsHidden)
            {
                return false;
            }

            if (!_config.OnlyPinned)
            {
                return true;
            }

            if (string.IsNullOrEmpty(icon.ProcessPath))
            {
                icon.ProcessPath = NotifyIconPinning.GetProcessPath(icon.HWnd);
            }

            return NotifyIconPinning.IsPromoted(icon);
        }

        private void AddView(TrayNotifyIcon icon, bool playAppear = true)
        {
            if (!_alive || !ShouldShow(icon) || _icons.ContainsKey(icon))
            {
                return;
            }

            var view = new TrayIcon(this, icon);
            _icons[icon] = view;
            Api.Add(view, playAppear);
        }

        private void RemoveView(TrayNotifyIcon icon)
        {
            if (!_icons.Remove(icon, out var view))
            {
                return;
            }

            Api.Remove(view, true);
            view.Dispose();
        }
    }
}
