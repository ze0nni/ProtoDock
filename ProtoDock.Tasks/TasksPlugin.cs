using ProtoDock.Api;

namespace ProtoDock.Tasks
{
    public class TasksPlugin : IDockPlugin, IDockPlugin.IPanelHook
    {
        public string Name => "Tasks";

        public string GUID => "{8F5FC966-7791-4F6F-B878-8BA4B335BCBE}";

        public int Version => 1;

        public IDockPanelMediator Create(IDockApi api, string data)
        {
            return new TasksMediator(api, this, data);
        }
        
        public bool ResolveHook<T>(out T hook) where T : class
        {
            switch (typeof(T))
            {
                case var cls when cls == typeof(IDockPlugin.IPanelHook):
                    hook = this as T;
                    return true;
            }

            hook = default;
            return false;
        }

        public override string ToString()
        {
            return "Tasks bar";
        }
    }
}
