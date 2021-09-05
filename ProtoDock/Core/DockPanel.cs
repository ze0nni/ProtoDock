using ProtoDock.Api;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ProtoDock.Core
{
    class DockPanel : IDockPanelApi, IDisposable
    {
        public IDockApi Dock => _dock;

        private readonly Dock _dock;
        private readonly IDockPanel _model;

        private readonly HashSet<IDockIcon> _icons = new HashSet<IDockIcon>();

        public DockPanel(Dock dock, IDockPanel model)
        {
            _dock = dock;
            _model = model;

            _model.Setup(this);
            _model.Awake();
        }

        public void Dispose()
        {
            _model.Destroy();
        }

        public void Add(IDockIcon icon)
        {
            if (_icons.Add(icon))
            {
                _dock.Graphics.AddIcon(icon);
            }
        }

        public void Remove(IDockIcon icon)
        {
            
        }
    }
}
