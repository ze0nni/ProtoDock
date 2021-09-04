using System;
using System.Collections.Generic;
using System.Text;

namespace BBDock.Api
{
    public interface IDockPanel
    {
        public void Setup(IDockPanelApi api);
        public void Awake();
        public void Destroy();
    }
}
