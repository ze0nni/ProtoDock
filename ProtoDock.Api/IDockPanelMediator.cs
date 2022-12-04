using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace ProtoDock.Api
{
    public interface IDockPanelMediator
    {
        IDockPlugin Plugin { get; }
        void Setup(IDockPanelApi api);
        bool RequestSettings { get; }
        void DisplaySettings(IDockSettingsDisplay display);
        void RestoreIcon(int version, string data);
        bool Store(out string data);
        void Awake();
        void Destroy();
        void Update();
        bool DragCanAccept(IDataObject data);
        void DragAccept(int index, IDataObject data);
    }
}
