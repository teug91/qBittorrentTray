using Newtonsoft.Json;

namespace qBittorrentTray.API
{
    public class Torrent
    {
        [JsonProperty("hash")]
        public string Hash { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("seeding_time")]
        public int SeedingTime { get; set; }
    }
}
