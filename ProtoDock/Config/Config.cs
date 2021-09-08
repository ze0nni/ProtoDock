using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoDock.Config
{

    [Serializable]
    public class DockConfig
    {
        public List<DockPanelConfig> Panels { get; set; }
    }

    [Serializable]
    public class DockPanelConfig
    {
        public string PluginGUID { get; set; }
        public List<DockIconConfig> Icons { get; set; }
    }

    [Serializable]
    public class DockIconConfig
    {
        public string Data { get; set; }
    }
}
