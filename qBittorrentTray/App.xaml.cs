using qBittorrentTray.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
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
		private static readonly string id = "{8F6F0AC4-B9A2-45fd-A8CF-72F03E7BDE5F}";
		static Mutex mutex = new Mutex(true, id);
		private Thread listen;


		/// <summary>
		/// Creates tray icon and starts listening for input.
		/// </summary>
		/// <param name="e"></param>
		[STAThread]
		protected override void OnStartup(StartupEventArgs e)
		{
			List<string> filePaths = new List<string>();

			if (e.Args != null)
				filePaths = new List<string>(e.Args);

			// Exit application if already running.
			if (!mutex.WaitOne(TimeSpan.Zero, true))
			{
				if (e.Args != null)
				{
					using (var client = new NamedPipeClientStream(id))
					using (var writer = new BinaryWriter(client))
					{
						if (filePaths.Count != 0)
						{
							client.Connect(3000);
							string filePathsString = "";
							foreach (string filePath in filePaths)
								filePathsString += filePath + "\n";

							filePathsString = filePathsString.Remove(filePathsString.Length - 1);
							writer.Write(filePathsString);
						}
					}
				}

				Current.Shutdown();
			}

			else
			{
				base.OnStartup(e);
				main = new Main(new List<string>(e.Args));

				listen = new Thread(() =>
				{
					while (true)
					{
						using (NamedPipeServerStream server = new NamedPipeServerStream(id))
						{
							server.WaitForConnection();

							using (var reader = new BinaryReader(server))
							{
								string arguments = reader.ReadString();
								filePaths = new List<string>(arguments.Split('\n'));
								main.AddTorrents(filePaths);
							}
						}
					}
				});

				listen.IsBackground = true;
				listen.Start();
			}
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