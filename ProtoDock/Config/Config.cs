using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace ProtoDock.Config
{

    [Serializable]
    public class DockConfig
    {

        [DefaultValue(null)]
        public string Skin { get; set; }
        public List<DockPanelConfig> Panels { get; set; }
    }

    [Serializable]
    public class DockPanelConfig
    {
        public List<DockPluginMediatorConfig> Mediators { get; set; }
        public List<DockIconConfig> Icons { get; set; }
    }

    [Serializable]
    public class DockPluginMediatorConfig
    {
        public string PluginGUID { get; set; }
    }

    [Serializable]
    public class DockIconConfig
    {
        public int MediatorId { get; set; }
        public int PluginVersion { get; set; }
        public string Data { get; set; }
    }
}
