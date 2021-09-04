using System;

namespace BBDock.Api
{
    public interface IDockPlugin
    {
        string Name { get; }
        string GUID { get; }

        IDockPanel Create();
    }
}
