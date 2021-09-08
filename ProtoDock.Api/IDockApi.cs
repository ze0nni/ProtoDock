using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace ProtoDock.Api
{
    public interface IDockApi
    {
        IntPtr HInstance { get; }

        IReadOnlyList<IDockPlugin> Plugins { get; }

        void Flush();
    }
}
