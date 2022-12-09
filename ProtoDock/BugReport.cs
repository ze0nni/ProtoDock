using System;
using System.Diagnostics;
using System.Text;
using System.Windows;

namespace ProtoDock
{
    internal static class BugReport
    {
        public static void SetHooks()
        {
            AppDomain.CurrentDomain.UnhandledException += OnException;
        }

        private static void OnException(object sender, UnhandledExceptionEventArgs arg)
        {
            var e = (Exception)arg.ExceptionObject;

            Debug.WriteLine(e.ToString());

            var sb = new StringBuilder();
            sb.Append(@"https://github.com/ze0nni/ProtoDock/issues/new");
            sb.Append($"?title={Uri.EscapeDataString(e.Message)}");
            sb.Append($"&label=bug");
            sb.Append($"&body=```{Uri.EscapeDataString('\n' + e.StackTrace + '\n')}```");
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(sb.ToString())
                {
                    UseShellExecute = true
                });
        }
    }
}
