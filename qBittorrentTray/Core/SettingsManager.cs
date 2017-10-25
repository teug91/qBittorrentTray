using System;
using qBittorrentTray.Properties;
using System.Diagnostics;
using System.Security.Permissions;

namespace qBittorrentTray.Core
{
    internal static class SettingsManager
    {
        internal static Uri GetHost()
        {
            return Settings.Default.Host;
        }

        internal static string GetUsername()
        {
            return Settings.Default.Username;
        }

        internal static string GetPassword()
        {
            return Settings.Default.Password;
        }

        internal static int GetMaxSeedingTime()
        {
            return Settings.Default.MaxSeedingDays;
        }

        internal static string GetAction()
        {
            return Settings.Default.Action;
        }

        /// <summary>
        /// Gets autostart setting. Fixes path, If path in registry key is wrong.
        /// </summary>
        /// <returns>True if activated, null if no access to registry.</returns>
        internal static bool? GetAutoStart()
        {
            try
            {
                RegistryPermission perm1 = new RegistryPermission(RegistryPermissionAccess.Write, @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run");
                perm1.Demand();
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                if (key.GetValue(Process.GetCurrentProcess().ProcessName) != null)
                {
                    if ((string)key.GetValue(Process.GetCurrentProcess().ProcessName) != Process.GetCurrentProcess().MainModule.FileName)
                        SetAutoStart(true);
                    return true;
                }
                else
                    return false;
            }

            // No registry access.
            catch (System.Security.SecurityException)
            {
                return null;
            }
        }

        /// <summary>
        /// Saves settings.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="autoStart"></param>
        internal static void SaveSettings(Uri host, string username, string password, int MaxSeedDays, string action, bool? autoStart = null)
        {
            var hostString = host.ToString();
            if (hostString[hostString.Length - 1] != '/')
            {
                hostString += "/";
                Settings.Default.Host = new Uri(hostString);
            }
            else
                Settings.Default.Host = host;

            Settings.Default.Username = username;
            Settings.Default.Password = password;
            Settings.Default.MaxSeedingDays = MaxSeedDays;
            Settings.Default.Action = action;


            if (autoStart != null)
                SetAutoStart(autoStart);

            Settings.Default.Save();
        }

        /// <summary>
        /// Sets autostart setting.
        /// </summary>
        /// <param name="autoStart">Activate or deactivate.</param>
        private static void SetAutoStart(bool? autoStart)
        {
            try
            {
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Run", true);

                if (autoStart == true)
                    key.SetValue(Process.GetCurrentProcess().ProcessName, Process.GetCurrentProcess().MainModule.FileName);
                else
                    key.DeleteValue(Process.GetCurrentProcess().ProcessName, false);
            }

            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
