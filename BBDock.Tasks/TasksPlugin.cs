using BBDock.Api;
using System;
using System.Collections.Generic;
using System.Text;

namespace BBDock.Tasks
{
    public class TasksPlugin : IDockPlugin
    {
        public string Name => "Tasks";

        public string GUID => "{8F5FC966-7791-4F6F-B878-8BA4B335BCBE}";

        public IDockPanel Create()
        {
            return new TasksPanel();
        }
    }
}
