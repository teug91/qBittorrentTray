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
		static Mutex mutex = new Mutex(true, "{8F6F0AC4-B9A1-45fd-A8CF-72F04E6BDE8F}");

		/// <summary>
		/// Creates tray icon and starts listening for input.
		/// </summary>
		/// <param name="e"></param>
		[STAThread]
        protected override void OnStartup(StartupEventArgs e)
        {
			// Exit application if already running.
			if (!mutex.WaitOne(TimeSpan.Zero, true))
				Current.Shutdown();

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
	}
}