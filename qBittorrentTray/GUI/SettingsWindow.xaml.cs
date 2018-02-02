using qBittorrentTray.Core;
using System;
using System.Diagnostics;
using System.Globalization;
using System.Windows;

namespace qBittorrentTray.GUI
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : ToolWindow
    {
        public SettingsWindow()
        {
            InitializeComponent();

            if (SettingsManager.GetHost() != null)
                HostTextBox.Text = SettingsManager.GetHost().ToString();

            if (SettingsManager.GetUsername() != null)
                UsernameTextBox.Text = SettingsManager.GetUsername();

            if (SettingsManager.GetPassword() != null)
                PasswordTextBox.Password = SettingsManager.GetPassword();

			deleteTorrentCheckbox.IsChecked = SettingsManager.GetDeleteTorrent();

			InitializeAutoStartCheckbox();

            InitializeCheckBox();
        }

        private void InitializeCheckBox()
        {
            ActionComboBox.Items.Add(Actions.Nothing);
            ActionComboBox.Items.Add(Actions.Delete);
            ActionComboBox.Items.Add(Actions.DeleteAll);

			ActionComboBox_Ratio.Items.Add(Actions.Nothing);
			ActionComboBox_Ratio.Items.Add(Actions.Delete);
			ActionComboBox_Ratio.Items.Add(Actions.DeleteAll);

			ActionComboBox.SelectedItem = SettingsManager.GetAction();
			ActionComboBox_Ratio.SelectedItem = SettingsManager.GetRatioAction();

			for (var i = 0; i < 100; i++)
                DaysComboBox.Items.Add(i);

			for (double i = 0; i < 9.9; i += 0.1)
				RatioComboBox.Items.Add(i.ToString("N1"));

			DaysComboBox.SelectedItem = SettingsManager.GetMaxSeedingTime();
			RatioComboBox.SelectedIndex = (int)(SettingsManager.GetMaxRatio() * 10);
		}

        /// <summary>
        /// Sets checkbox to value from settings.
        /// </summary>
        private void InitializeAutoStartCheckbox()
        {
            bool? autoStart = SettingsManager.GetAutoStart();

            if (autoStart == null)
                autostartCheckbox.Visibility = Visibility.Hidden;
            else
                autostartCheckbox.IsChecked = autoStart;
        }

        /// <summary>
        /// Occurs when 'Save' button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            bool? autoStart = null;

            if (autostartCheckbox.Visibility == Visibility.Visible)
                autoStart = autostartCheckbox.IsChecked;

			Uri newHost;
            if (!Uri.TryCreate(HostTextBox.Text, UriKind.Absolute, out newHost))
                InvalidHost.Visibility = Visibility.Visible;

            else
            {
				var password = "";
				if (PasswordTextBox.Password != SettingsManager.GetPassword())
					password = Security.EncryptString(Security.ToSecureString(PasswordTextBox.Password));

				SettingsManager.SaveSettings(newHost, UsernameTextBox.Text, password,
                    (int) DaysComboBox.SelectedItem, (string) ActionComboBox.SelectedItem, (string)ActionComboBox_Ratio.SelectedItem,
					deleteTorrentCheckbox.IsChecked == true, float.Parse(RatioComboBox.SelectedItem.ToString()), autoStart);
                Close();
            }
        }

        /// <summary>
        /// Occurs when 'Cancel' button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
