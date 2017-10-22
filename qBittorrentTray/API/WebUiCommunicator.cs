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
using System.IO;

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
            client.Timeout = new TimeSpan(0, 0, 10);

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
                await Post("/command/pauseAll");
        }

        /// <summary>
        /// Resumes all torrents.
        /// </summary>
        /// <returns></returns>
        public static async Task ResumeAll()
        {
            if (IsLoggedIn())
                await Post("/command/resumeAll");
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
                if (torrent.State != "pausedUP" && torrent.State != "pausedDL")
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
            var result = await Post("/query/torrents");

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

        /// <summary>
        /// Post with file.
        /// </summary>
        /// <param name="requestUri"></param>
        /// <param name="form"></param>
        /// <returns></returns>
        private static async Task<string> Post(string requestUri, MultipartFormDataContent form)
        {
            try
            {
                HttpResponseMessage result;
                result = await client.PostAsync(requestUri, form);
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

        public static async Task AddTorrent(string filePath)
        {
            string filename = filePath.Substring(filePath.LastIndexOf('\\') + 1);
            var content = File.ReadAllBytes(filePath);

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            MultipartFormDataContent form = new MultipartFormDataContent();
            form.Add(new ByteArrayContent(content), "torrents", filename);

            await Post("/command/upload", form);

            bool torrentAdded = await ContainsTorrent(filename);
            if (torrentAdded)
                if (File.Exists(filePath))
                    File.Delete(filePath);
        }

        public static async Task DeleteAfterMaxSeedingTime()
        {
            var maxSeedingTime = SettingsManager.GetMaxSeedingTime();
            var torrents = await GetTorrents();
            var hashes = new List<string>();

            foreach (var torrent in torrents)
            {
                var seedingTimeSpan = await GetSeedingTimeSpan(torrent.Hash);
                if (seedingTimeSpan != null)
                {
                    if (seedingTimeSpan > maxSeedingTime)
                    {
                        hashes.Add(torrent.Hash);
                    }
                }
            }

            if (hashes.Count > 0)
                await DeleteTorrents(hashes);
        }

        private static async Task<TimeSpan?> GetSeedingTimeSpan(string hash)
        {
            var result = await Post("/query/propertiesGeneral/" + hash);

            if (result == "")
                return null;

            var torrentInfo = JsonConvert.DeserializeObject<Torrent>(result);

            return TimeSpan.FromSeconds(torrentInfo.SeedingTime);
        }

        private static async Task DeleteTorrents(List<string> hashes)
        {
            string allHashes = "";

            foreach (var hash in hashes)
            {
                allHashes += (hash + "|");
            }


            allHashes = allHashes.Remove(allHashes.Length - 1);

            var content = new[]
            {
                new KeyValuePair<string, string>("hashes", allHashes)
            };

            await Post("/command/deletePerm", content);
        }

        private static async Task<bool> ContainsTorrent(string torrentName)
        {
           List<Torrent> torrents = await GetTorrents();

            foreach (Torrent torrent in torrents)
            {
                if (torrent.Name == torrentName)
                    return true;
            }

            return false;
        }
    }
}
