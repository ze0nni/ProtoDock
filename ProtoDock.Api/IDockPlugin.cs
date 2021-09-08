using System;

namespace ProtoDock.Api
{
    public interface IDockPlugin
    {
        string Name { get; }
        string GUID { get; }
        int Version { get; }

        IDockPanelMediator Create();
    }
}
