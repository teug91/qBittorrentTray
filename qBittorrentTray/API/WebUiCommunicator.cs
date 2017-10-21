using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using qBittorrentTray.Core;
using System.Diagnostics;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace qBittorrentTray.API
{
    public static class WebUiCommunicator
    {
        private static HttpClient client = new HttpClient();
        private static string cookie = "";

        /// <summary>
        /// Loging in to Web UI.
        /// </summary>
        /// <returns></returns>
        public static async Task<bool?> Login()
        {
            client = new HttpClient();
            client.BaseAddress = SettingsManager.GetHost();
            client.Timeout = new TimeSpan(0, 0, 5);

            var content = new[]
            {
                new KeyValuePair<string, string>("username", SettingsManager.GetUsername()),
                new KeyValuePair<string, string>("password", SettingsManager.GetPassword())
            };

            string resultContent = await Post("/login", content);

            if (resultContent == "Ok.")
                return true;
            else if (resultContent == "Fails.")
                return false;
            else
                return null;
        }

        /// <summary>
        /// Pauses all torrents.
        /// </summary>
        /// <returns></returns>
        public static async Task PauseAll()
        {
            if (IsLoggedIn())
                await Post("/command/pauseAll", null);
        }

        /// <summary>
        /// Resumes all torrents.
        /// </summary>
        /// <returns></returns>
        public static async Task ResumeAll()
        {
            if (IsLoggedIn())
                await Post("/command/resumeAll", null);
        }

        /// <summary>
        /// Checks if all torrents are paused.
        /// </summary>
        /// <returns>True if all torrents are paused.</returns>
        public static async Task<bool?> IsPaused()
        {
            var torrents = await GetTorrents();

            if (torrents == null)
                return null;

            foreach (var torrent in torrents)
            {
                if (torrent.State != "pausedUP" && torrent.State != "pausedUP")
                    return false;
            }

            return true;
        }


        /// <summary>
        /// Gets list of all torrents.
        /// </summary>
        /// <returns>List of all torrents.</returns>
        public static async Task<List<Torrent>> GetTorrents()
        {
            var result = await Post("/query/torrents", null);

            if (result == "")
                return null;

            return JsonConvert.DeserializeObject<List<Torrent>>(result);
        }

        /// <summary>
        /// Checks if logged in.
        /// </summary>
        /// <returns>True if logged in.</returns>
        public static bool IsLoggedIn()
        {
            if (cookie != "")
                return true;

            return false;
        }

        /// <summary>
        /// Opens Web UI in standard browser.
        /// </summary>
        public static void OpenWebUi()
        {
            Process.Start(client.BaseAddress.ToString());
        }

        private static async Task<string> Post(string requestUri, IEnumerable<KeyValuePair<string, string>> nameValueCollection = null)
        {
            try
            {
                HttpResponseMessage result;

                if (nameValueCollection != null)
                {
                    var content = new FormUrlEncodedContent(nameValueCollection);
                    result = await client.PostAsync(requestUri, content);
                }

                else
                    result = await client.PostAsync(requestUri, null);

                if (requestUri == "/login")
                {
                    HttpHeaders headers = result.Headers;
                    IEnumerable<string> values;

                    if (headers.TryGetValues("Set-Cookie", out values))
                    {
                        string session = values.First();
                        Match cleanedup = Regex.Match(session, @"^.*?(?=;)");

                        cookie = cleanedup.ToString();
                    }
                }

                return await result.Content.ReadAsStringAsync();
            }

            catch (HttpRequestException)
            {
                Debug.WriteLine("Request " + requestUri + " failed!");
                return "";
            }
        }

        private static async Task<string> Get(string requestUri, IEnumerable<KeyValuePair<string, string>> nameValueCollection = null)
        {
            try
            {
                var result = await client.GetAsync(requestUri);
                return await result.Content.ReadAsStringAsync();
            }

            catch (HttpRequestException)
            {
                Debug.WriteLine("Request " + requestUri + " failed!");
                return "";
            }
        }
    }
}
