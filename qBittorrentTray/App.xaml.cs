using qBittorrentTray.Core;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows;

namespace qBittorrentTray
{
    /// <summary>
    ///     Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        //private TaskbarIcon notifyIcon;
        private Main main;

        /// <summary>
        /// Creates tray icon and starts listening for input.
        /// </summary>
        /// <param name="e"></param>
        [STAThread]
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            main = new Main();
        }

        /// <summary>
        /// Makes sure tray icon is removed when application exits.
        /// </summary>
        /// <param name="e"></param>
        protected override void OnExit(ExitEventArgs e)
        {
            main.notifyIcon.Dispose();
            base.OnExit(e);
        }

        /// 
        /// check if given exe alread running or not
        /// 
        /// returns true if already running
        private static bool IsAlreadyRunning()
        {
            string strLoc = Assembly.GetExecutingAssembly().Location;
            FileSystemInfo fileInfo = new FileInfo(strLoc);
            string sExeName = fileInfo.Name;
            bool bCreatedNew;

            Mutex mutex = new Mutex(true, "Global\\" + sExeName, out bCreatedNew);
            if (bCreatedNew)
                mutex.ReleaseMutex();

            return !bCreatedNew;
        }
    }
}