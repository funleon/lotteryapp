using Microsoft.UI.Dispatching;
using Microsoft.Windows.AppLifecycle;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;

namespace lotteryapp
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            try
            {
                WinRT.ComWrappersSupport.InitializeComWrappers();

                Microsoft.UI.Xaml.Application.Start((p) =>
                {
                    var context = new DispatcherQueueSynchronizationContext(DispatcherQueue.GetForCurrentThread());
                    SynchronizationContext.SetSynchronizationContext(context);
                    new App();
                });
            }
            catch (Exception ex)
            {
                string logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "crash_log.txt");
                File.WriteAllText(logPath, "Crash at startup: " + ex.ToString());
                // Also try to show a message box if possible, but file log is safer
            }
        }
    }
}
