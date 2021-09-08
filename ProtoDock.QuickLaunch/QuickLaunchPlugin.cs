using ProtoDock.Api;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoDock.QuickLaunch
{
    public class QuickLaunchPlugin : IDockPlugin
    {
        public string Name => "Quick Launch";

        public string GUID => "{2AE5179C-3552-4782-A091-F05B6A312EAE}";

        public int Version => 1;

        public IDockPanelMediator Create()
        {
            return new QuickLaunchMediator(this);
        }
    }
}
