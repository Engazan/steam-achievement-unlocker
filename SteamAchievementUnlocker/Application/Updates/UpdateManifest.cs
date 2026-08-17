using System;
using System.Text.Json.Serialization;

namespace SteamAchievementUnlocker.Updates
{
    internal sealed class UpdateManifest
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("downloadUrl")]
        public string DownloadUrl { get; set; }

        [JsonPropertyName("releaseUrl")]
        public string ReleaseUrl { get; set; }

        [JsonPropertyName("sha256")]
        public string Sha256 { get; set; }

        [JsonPropertyName("publishedAt")]
        public DateTimeOffset PublishedAt { get; set; }
    }
}
