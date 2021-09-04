using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace BBDock.Api
{
    public interface IDockApi
    {
        IntPtr HInstance { get; }

        ReadOnlyCollection<IDockPlugin> Plugins { get; }
    }
}
