using qBittorrentSharp;
using qBittorrentTray.Core;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Timers;
using System.Windows;
using System.Windows.Input;

namespace qBittorrentTray.GUI
{
    internal class TrayIcon : INotifyPropertyChanged
    {
		//public TaskbarIcon notifyIcon;
		//public static event EventHandler Disconnected;
		string icon = "/qBittorrentTray;component/Resources/qbdark.ico";

        public TrayIcon()
        {
			//notifyIcon = (TaskbarIcon)Application.Current.FindResource("NotifyIcon");

			Timer initializationTimer = new Timer(1000);
            initializationTimer.Elapsed += SetIcon;
            initializationTimer.Enabled = true;
            GC.KeepAlive(initializationTimer);
        }

        /// <summary>
        /// Sets icon.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public async void SetIcon(object sender, ElapsedEventArgs e)
        {
			try
			{
				bool? isPaused = await API.AreAllTorrentsPaused();

				if (isPaused == false)
				{
					Icon = "/qBittorrentTray;component/Resources/qbdark.ico";
					OnPropertyChanged("Icon");
				}

				else if (isPaused == true)
				{
					Icon = "/qBittorrentTray;component/Resources/pause.ico";
					OnPropertyChanged("Icon");
				}

				else
				{
					Icon = "/qBittorrentTray;component/Resources/dc.ico";
					OnPropertyChanged("Icon");
				}
			}

			catch (Exception ex)
			{
				Debug.WriteLine(ex.Message);
				if (Icon != "/qBittorrentTray;component/Resources/dc.ico")
				{
					Icon = "/qBittorrentTray;component/Resources/dc.ico";
					OnPropertyChanged("Icon");
					//Disconnected?.Invoke(ex, null);
				}
			}
		}

		/// <summary>
		///     Shows a window, if none is already open.
		/// </summary>
		public ICommand ShowWindowCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () =>
                    {
                        if (Application.Current.MainWindow != null)
                        {
                            Application.Current.MainWindow.Activate();
                        }

                        else
                        {
                            Application.Current.MainWindow = new SettingsWindow();
                            Application.Current.MainWindow.Show();
                            Application.Current.MainWindow.Activate();
                        }
                    }
                };
            }
        }

        /// <summary>
        ///     Pause/resume all torrents.
        /// </summary>
        public ICommand PauseResumeCommand
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = async () =>
                    {
                        await Core.Main.PauseResume();
                    }
                };
            }
        }

        /// <summary>
        /// Open qBittorrent WebUI in standard browser.
        /// </summary>
        public ICommand OpenWebUi
        {
            get
            {
                return new DelegateCommand
                {
                    CommandAction = () =>
                    {
						System.Diagnostics.Process.Start(SettingsManager.GetHost().ToString());
					}
                };
            }
        }

        /// <summary>
        ///     Shuts down the application.
        /// </summary>
        public ICommand ExitApplicationCommand
        {
            get { return new DelegateCommand {CommandAction = () => Application.Current.Shutdown()}; }
        }

        /// <summary>
        /// Gets icon.
        /// </summary>
        public string Icon
        {
            get
            {
                return icon;
            }

            set
            {
                icon = value;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }

    /// <summary>
    ///     Delegate command
    /// </summary>
    public class DelegateCommand : ICommand
    {
        public Action CommandAction { get; set; }
        public Func<bool> CanExecuteFunc { get; set; }

        public void Execute(object parameter)
        {
            CommandAction();
        }

        public bool CanExecute(object parameter)
        {
            return CanExecuteFunc == null || CanExecuteFunc();
        }

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}