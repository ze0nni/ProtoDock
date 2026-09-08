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
        private bool _alive;

        public TrayMediator(IDockPlugin plugin)
        {
            Plugin = plugin;
        }

        public void Setup(IDockPanelApi api)
        {
            Api = api;
        }

        public bool RequestSettings => false;

        public void DisplaySettings(IDockSettingsDisplay display)
        {
            throw new NotImplementedException();
        }

        public bool Store(out string data)
        {
            data = default;
            return false;
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
            RunOnDock(() =>
            {
                if (icon.IsHidden)
                {
                    RemoveView(icon);
                    return;
                }

                if (!_icons.ContainsKey(icon))
                {
                    AddView(icon);
                }
            });
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

        private void AddView(TrayNotifyIcon icon)
        {
            if (!_alive || icon.IsHidden || _icons.ContainsKey(icon))
            {
                return;
            }

            var view = new TrayIcon(this, icon);
            _icons[icon] = view;
            Api.Add(view, true);
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
