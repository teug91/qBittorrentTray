using Hardcodet.Wpf.TaskbarNotification;
using qBittorrentTray.API;
using qBittorrentTray.Properties;
using System.ComponentModel;
using System.Threading.Tasks;
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
        }

        /// <summary>
        /// Initializes API.
        /// </summary>
        private async void Init()
        {
            bool? loggedInn = await WebUiCommunicator.Login();

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
        public static async Task<string> PauseResume()
        {
            bool? isPaused = await WebUiCommunicator.IsPaused();

            if (isPaused == true)
            {
                await WebUiCommunicator.ResumeAll();
                return "/qBittorrentTray;component/Resources/qb.ico";
            }

            else if (isPaused == false)
            {
                await WebUiCommunicator.PauseAll();
                return "/qBittorrentTray;component/Resources/pause.ico";
            }

            return "";
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
