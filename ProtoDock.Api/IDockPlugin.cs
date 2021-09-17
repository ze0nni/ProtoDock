using System;

namespace ProtoDock.Api
{
    public interface IDockPlugin
    {
        string Name { get; }
        string GUID { get; }
        int Version { get; }

        IDockPanelMediator Create();
        
        bool ResolveHook<T>(out T hook);

        public interface IDockSetupHook {
            void OnDockSetup();
        }
        
        public interface IDockAwakeHook {
            void OnDockAwake();
        }

        public interface ISettingsHook
        {
            public void OnSettings(IDockSettingsContext context);
        }
    }
}
