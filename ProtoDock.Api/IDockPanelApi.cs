using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoDock.Api
{
    public interface IDockPanelApi
    {
        IDockApi Dock { get; }

        void Add(IDockIcon icon);
        void Remove(IDockIcon icon);
    }
}
