using Hardcodet.Wpf.TaskbarNotification;
using qBittorrentTray.Properties;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using qBittorrentSharp;

namespace qBittorrentTray.Core
{
    public class Main
    {
        public TaskbarIcon notifyIcon;
        //private Timer checkSeedTimeTimer;
		//

        /// <summary>
        /// Initializes API and tray icon.
        /// </summary>
        public Main()
        {
            Settings.Default.SettingsSaving += SettingSaving;
			//GUI.TrayIcon.Disconnected += ShowBalloon;


			if (SettingsManager.GetHost() == null)
            {
                Application.Current.MainWindow = new GUI.SettingsWindow();
                Application.Current.MainWindow.Show();
                Application.Current.MainWindow.Activate();
			}

            else
                Init();

            notifyIcon = (TaskbarIcon) Application.Current.FindResource("NotifyIcon");
            notifyIcon.TrayBalloonTipClicked += TrayBalloonTipClicked;

            
        }

        /*private async void CheckSeedTime(object sender, ElapsedEventArgs e)
        {
			try
			{
				await API.DeleteAfterMaxSeedTime(TimeSpan.FromDays(SettingsManager.GetMaxSeedingTime()),
												 (SettingsManager.GetAction() == Actions.DeleteAll));
			}

			catch (QBTException ex)
			{
				notifyIcon.ShowBalloonTip("Error" + ex.HttpStatusCode.ToString(), ex.Message, BalloonIcon.Error);
			}
        }*/

        private void TrayBalloonTipClicked(object sender, RoutedEventArgs e)
        {
            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.Activate();

            else
            {
                Application.Current.MainWindow = new GUI.SettingsWindow();
                Application.Current.MainWindow.Show();
                Application.Current.MainWindow.Activate();
            }
        }

        /// <summary>
        /// Initializes API.
        /// </summary>
        private async void Init()
        {
			/*if (checkSeedTimeTimer != null)
                if (checkSeedTimeTimer.Enabled)
                    checkSeedTimeTimer.Enabled = false;*/

			API.Initialize(SettingsManager.GetHost().ToString(), 10);

            bool? loggedInn = await API.Login(SettingsManager.GetUsername(), SettingsManager.GetPassword());

            if (loggedInn == true)
            {
                /*if (SettingsManager.GetAction() != Actions.Nothing)
                {
					await API.DeleteAfterMaxSeedTime(TimeSpan.FromDays(SettingsManager.GetMaxSeedingTime()), 
													 (SettingsManager.GetAction() == Actions.DeleteAll));
                    checkSeedTimeTimer = new Timer(3600000);
                    checkSeedTimeTimer.Elapsed += CheckSeedTime;
                    checkSeedTimeTimer.Enabled = true;
                    GC.KeepAlive(checkSeedTimeTimer);
                }*/
            }

            else if (loggedInn == false)
            {
                notifyIcon.ShowBalloonTip("Error", "Username and/or password is wrong!", BalloonIcon.Error);
            }

            else
            {
                notifyIcon.ShowBalloonTip("Error", "qBittorrent is unreachable!", BalloonIcon.Error);
            }
        }

        /// <summary>
        /// Pauses or resumes all torrents.
        /// </summary>
        /// <returns></returns>
        public static async Task PauseResume()
        {
			bool? isPaused = await API.AreAllTorrentsPaused();

            if (isPaused == true)
                await API.ResumeAll();

            else if (isPaused == false)
                await API.PauseAll();
        }

        /// <summary>
        /// New settings, reinitializes.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SettingSaving(object sender, CancelEventArgs e)
        {
            Init();
        }

		/*private void ShowBalloon(object sender, EventArgs e)
		{
			notifyIcon.ShowBalloonTip("Error", "Some error!", BalloonIcon.Error);
		}*/
    }
}
