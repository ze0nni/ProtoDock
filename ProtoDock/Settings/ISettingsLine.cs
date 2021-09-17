using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoDock.Settings
{
    interface ISettingsLine: IDisposable
    {
        System.Windows.Forms.Control Control { get; }
    }
}
