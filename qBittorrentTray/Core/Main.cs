using Hardcodet.Wpf.TaskbarNotification;
using qBittorrentTray.API;
using qBittorrentTray.Properties;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;

namespace qBittorrentTray.Core
{
    public class Main
    {
        public TaskbarIcon notifyIcon;

        /// <summary>
        /// Initializes API and tray icon.
        /// </summary>
        public Main()
        {
            Settings.Default.SettingsSaving += SettingSaving;

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

            Timer initializationTimer = new Timer(60000);
            initializationTimer.Elapsed += CheckSeedTime;
            initializationTimer.Enabled = true;
            GC.KeepAlive(initializationTimer);
        }

        private async void CheckSeedTime(object sender, ElapsedEventArgs e)
        {
            await WebUiCommunicator.DeleteAfterMaxSeedingTime();
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
        private async void Init()
        {
            bool? loggedInn = await WebUiCommunicator.Login();
            await WebUiCommunicator.DeleteAfterMaxSeedingTime();

            if (loggedInn == null)
            {
                notifyIcon.ShowBalloonTip("Error", "qBittorrent is unreachable!", BalloonIcon.Error);
            }

            else if (loggedInn == false)
            {
                notifyIcon.ShowBalloonTip("Error", "Username and/or password is wrong!", BalloonIcon.Error);
            }
        }

        /// <summary>
        /// Pauses or resumes all torrents.
        /// </summary>
        /// <returns></returns>
        public static async Task PauseResume()
        {
            bool? isPaused = await WebUiCommunicator.IsPaused();

            if (isPaused == true)
                await WebUiCommunicator.ResumeAll();

            else if (isPaused == false)
                await WebUiCommunicator.PauseAll();
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
    }
}
