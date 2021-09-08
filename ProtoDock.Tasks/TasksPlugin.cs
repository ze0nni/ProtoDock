using ProtoDock.Api;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProtoDock.Tasks
{
    public class TasksPlugin : IDockPlugin
    {
        public string Name => "Tasks";

        public string GUID => "{8F5FC966-7791-4F6F-B878-8BA4B335BCBE}";

        public int Version => 1;

        public IDockPanel Create()
        {
            return new TasksPanel(this);
        }
    }
}
