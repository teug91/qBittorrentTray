using Newtonsoft.Json;

namespace qBittorrentTray.API
{
    public class Torrent
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("progress")]
        public int Progress { get; set; }

        [JsonProperty("eta")]
        public int Eta { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }
    }
}
