using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoDock
{
    [Serializable]
    class DockSettings
    {
        public List<DockPanelSettings> Panels;
    }

    [Serializable]
    class DockPanelSettings
    {
        public List<DockPanelPluginSettings> Plugins;
        public List<DockPanelIconSettings> Icons;
    }

    [Serializable]
    class DockPanelPluginSettings
    {
        public string GUID { get; set; }
        public int Instance { get; set; }
        public string Data { get; set; }
    }

    [Serializable]
    class DockPanelIconSettings
    {        
        public int PluginInstance { get; set; }

        public string Data { get; set; }
    }

    [Serializable]
    class GraphicsSettings
    {
        public int IconSize { get; set; }
        public int IconSpace { get; set; }
        public float ActiveIconScale { get; set; }
        public int ActiveIconScaleDistance { get; set; }
        public int IconScaleSpeed { get; set; }
        public string SelectedSkin { get; set; }
    }
}
