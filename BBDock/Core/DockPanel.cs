using BBDock.Api;
using System;
using System.Runtime.InteropServices;

namespace BBDock.Core
{
    class DockPanel : IDockPanelApi
    {
        public IDockApi Dock => _dock;

        private Dock _dock;
        private IDockPanel _panel;

        public DockPanel(Dock dock, IDockPanel panel)
        {
            _dock = dock;
            _panel = panel;
        }
    }
}
