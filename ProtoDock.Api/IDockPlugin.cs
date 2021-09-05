using System;

namespace ProtoDock.Api
{
    public interface IDockPlugin
    {
        string Name { get; }
        string GUID { get; }

        IDockPanel Create();
    }
}
