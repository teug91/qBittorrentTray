using Hardcodet.Wpf.TaskbarNotification;
using qBittorrentTray.Properties;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using qBittorrentSharp;
using System.Collections.Generic;
using System.IO;

namespace qBittorrentTray.Core
{
    public class Main
    {
        public TaskbarIcon notifyIcon;
        private Timer checkSeedTimeTimer;

        /// <summary>
        /// Initializes API and tray icon.
        /// </summary>
        public Main(List<string> filePaths)
        {
            Settings.Default.SettingsSaving += SettingSaving;

			if (SettingsManager.GetHost() == null)
            {
                Application.Current.MainWindow = new GUI.SettingsWindow();
                Application.Current.MainWindow.Show();
                Application.Current.MainWindow.Activate();
			}

            else
                Init(filePaths);

            notifyIcon = (TaskbarIcon) Application.Current.FindResource("NotifyIcon");
            notifyIcon.TrayBalloonTipClicked += TrayBalloonTipClicked;

            
        }

        private async void CheckSeedTime(object sender, ElapsedEventArgs e)
        {
			await DeleteTorrents();
        }

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
        private async void Init(List<string> filePaths = null)
        {
			if (checkSeedTimeTimer != null)
                if (checkSeedTimeTimer.Enabled)
                    checkSeedTimeTimer.Enabled = false;

			API.Initialize(SettingsManager.GetHost().ToString(), 10);

			try
			{
				bool? loggedInn = await API.Login(SettingsManager.GetUsername(), Security.ToInsecureString(Security.DecryptString(SettingsManager.GetPassword())));

				if (loggedInn == true)
				{
					await DeleteTorrents();	
					checkSeedTimeTimer = new Timer(60000);
					checkSeedTimeTimer.Elapsed += CheckSeedTime;
					checkSeedTimeTimer.Enabled = true;
					GC.KeepAlive(checkSeedTimeTimer);

					if (filePaths != null)
						 AddTorrents(filePaths);
				}

				else if (loggedInn == false)
				{
					notifyIcon.ShowBalloonTip("Error", "Authorization failure!", BalloonIcon.Error);
				}

				else
				{
					notifyIcon.ShowBalloonTip("Error", "qBittorrent is unreachable!", BalloonIcon.Error);
				}
			}

			catch (Exception)
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

		public async void AddTorrents(List<string> filePaths)
		{
			try
			{
				await API.DownloadFromDisk(filePaths);

				if (SettingsManager.GetDeleteTorrent())
					foreach (string filePath in filePaths)
						if (File.Exists(filePath))
							File.Delete(filePath);
			}

			catch (QBTException ex)
			{
				notifyIcon.ShowBalloonTip("Error", ex.Message, BalloonIcon.Error);
			}

			catch (Exception ex)
			{
				notifyIcon.ShowBalloonTip("Error", ex.Message, BalloonIcon.Error);
			}
		}

		private async Task DeleteTorrents()
		{
			try
			{
				if (SettingsManager.GetAction() != Actions.Nothing)
					await API.DeleteAfterMaxSeedTime(TimeSpan.FromDays(SettingsManager.GetMaxSeedingTime()),
													(SettingsManager.GetAction() == Actions.DeleteAll));

				if (SettingsManager.GetRatioAction() != Actions.Nothing)
					await API.DeleteAfterMaxRatio(SettingsManager.GetMaxRatio(), (SettingsManager.GetRatioAction() == Actions.DeleteAll));
			}

			catch (QBTException ex)
			{
				notifyIcon.ShowBalloonTip("Error" + ex.HttpStatusCode.ToString(), ex.Message, BalloonIcon.Error);
			}
		}
    }
}
