using System.Collections.Specialized;
using ProtoDock.Api;
using System.Windows.Forms;
using ManagedShell.WindowsTray;
using NotifyIcon = ManagedShell.WindowsTray.NotifyIcon;

namespace ProtoDock.Tray
{
    internal class TrayMediator : IDockPanelMediator
    {
        public IDockPlugin Plugin { get; private set; }

        private IDockPanelApi _api;
        private NotificationArea _notificationArea;

        public TrayMediator(IDockPlugin plugin)
        {
            Plugin = plugin;
        }

        public void Setup(IDockPanelApi api)
        {
            _api = api;
        }

        public void RestoreIcon(int version, string data)
        {

        }

        public void Awake() {
            var trayService = new TrayService();
            var explorerTraService = new ExplorerTrayService();
            _notificationArea = new NotificationArea(trayService, explorerTraService);
            
            _notificationArea.Initialize();
            
            _notificationArea.PinnedIcons.CollectionChanged += OnTrayCollectionChanged;
            _notificationArea.UnpinnedIcons.CollectionChanged += OnTrayCollectionChanged;

            foreach (var entry in _notificationArea.PinnedIcons) {
                var icon = entry as NotifyIcon;
                _api.Add(new TrayIcon(this, icon), true);
            }
            
            foreach (var entry in _notificationArea.UnpinnedIcons) {
                var icon = entry as NotifyIcon;
                _api.Add(new TrayIcon(this, icon), true);
            }
        }

        public void Destroy()
        {
            _notificationArea.PinnedIcons.CollectionChanged -= OnTrayCollectionChanged;
            _notificationArea.UnpinnedIcons.CollectionChanged -= OnTrayCollectionChanged;
            _notificationArea.Dispose();
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
                        _api.Add(new TrayIcon(this, icon), true);
                    }

                    break;
                
                case NotifyCollectionChangedAction.Remove:
                    break;
            }
        }
    }
}
