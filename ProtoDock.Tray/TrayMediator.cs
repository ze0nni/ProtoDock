using System.Collections.Generic;
using System.Collections.Specialized;
using ProtoDock.Api;
using System.Windows.Forms;
using ManagedShell.WindowsTray;
using NotifyIcon = ManagedShell.WindowsTray.NotifyIcon;
using ManagedShell;

namespace ProtoDock.Tray
{
    internal class TrayMediator : IDockPanelMediator
    {
        public IDockPlugin Plugin { get; private set; }

        public IDockPanelApi Api { get; private set; }
        
        private NotificationArea _notificationArea;
        private readonly Dictionary<NotifyIcon, TrayIcon> _icons = new Dictionary<NotifyIcon, TrayIcon>();

        public TrayMediator(IDockPlugin plugin)
        {
            Plugin = plugin;
        }

        public void Setup(IDockPanelApi api)
        {
            Api = api;
        }

        public void RestoreIcon(int version, string data)
        {

        }

        public void UpdateScales(PanelScales scales) {
			
        }
        
        public void Awake() {
            var trayService = new TrayService();
            var explorerTraService = new ExplorerTrayService();
            _notificationArea = new NotificationArea(NotificationArea.DEFAULT_PINNED, trayService, explorerTraService);
            
            _notificationArea.Initialize();
            
            _notificationArea.PinnedIcons.CollectionChanged += OnTrayCollectionChanged;
            _notificationArea.UnpinnedIcons.CollectionChanged += OnTrayCollectionChanged;

            foreach (var entry in _notificationArea.PinnedIcons)
            {
                var icon = entry as NotifyIcon;
                if (!_icons.ContainsKey(icon))
                {
                    var view = new TrayIcon(this, icon);
                    _icons[icon] = view;
                    Api.Add(view, true);
                }
            }

            foreach (var entry in _notificationArea.UnpinnedIcons)
            {
                var icon = entry as NotifyIcon;
                if (!_icons.ContainsKey(icon))
                {
                    var view = new TrayIcon(this, icon);
                    _icons[icon] = view;
                    Api.Add(view, true);
                }
            }
        }

        public void Destroy()
        {
            foreach (var icon in _icons) {
                Api.Remove(icon.Value, false);
                icon.Value.Dispose();
            }

            _notificationArea.PinnedIcons.CollectionChanged -= OnTrayCollectionChanged;
            _notificationArea.UnpinnedIcons.CollectionChanged -= OnTrayCollectionChanged;
            _notificationArea.Dispose();
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

        private void OnTrayCollectionChanged(object sender, NotifyCollectionChangedEventArgs e) {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    foreach (var entry in e.NewItems) {
                        var icon = (NotifyIcon) entry;
                        if (_icons.ContainsKey(icon)) {
                            continue;
                        }
                        
                        var view = new TrayIcon(this, icon);
                        _icons[icon] = view;
                        Api.Add(view, true);
                    }

                    break;
                
                case NotifyCollectionChangedAction.Remove:
                    foreach (var entry in e.OldItems) {
                        var icon = (NotifyIcon) entry;
                        if (_icons.Remove(icon, out var view)) {
                            Api.Remove(view, true);
                            view.Dispose();
                        }
                    }

                    break;
            }
        }
    }
}
