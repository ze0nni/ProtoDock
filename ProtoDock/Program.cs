using System;
using System.Windows.Forms;

namespace ProtoDock
{
    static class Program
    {
        public static object Marshall { get; private set; }

        [STAThread]
        static void Main()
        {
            BugReport.SetHooks();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new DockWindow());
        }
    }
}
