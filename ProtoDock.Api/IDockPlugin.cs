using System;

namespace ProtoDock.Api
{
    public interface IDockPlugin
    {
        string Name { get; }
        string GUID { get; }
        int Version { get; }

        bool ResolveHook<T>(out T hook) where T : class;

        public interface IPanelHook
        {
            IDockPanelMediator Create(string data);
        }

        public interface IDockSetupHook {
            void OnDockSetup();
        }
        
        public interface IDockAwakeHook {
            void OnDockAwake();
        }

        public interface ISettingsHook
        {
            public void OnSettings(IDockSettingsContext context);
            public void OnSettingsRestore(int version, string data);
            public bool OnSettingsStore(out string data);
        }
    }
}
