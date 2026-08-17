using System;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.XPath;
using static SteamAchievementUnlocker.InvariantFormatting;
using API = SteamIntegration;

namespace SteamAchievementUnlocker
{
    internal sealed class AchievementProgressService
    {
        private const string ProgressCacheVersion = "achievement-progress-v1";
        private readonly ulong _steamId;
        private readonly string _steamLibraryCachePath;
        private readonly API.SteamClientSession _steamClient;
        private readonly SemaphoreSlim _downloadSemaphore = new(4);
        private static readonly HttpClient HttpClient = new();

        public AchievementProgressService(
            ulong steamId,
            string steamLibraryCachePath,
            API.SteamClientSession steamClient)
        {
            this._steamId = steamId;
            this._steamLibraryCachePath = steamLibraryCachePath;
            this._steamClient = steamClient;
        }

        public async Task<(int Achieved, int Total)?> LoadAsync(uint appId)
        {
            string localPath = Path.Combine(
                this._steamLibraryCachePath,
                appId.ToString(CultureInfo.InvariantCulture) + ".json");

            int? schemaTotal = await Task.Run(() => this.TryReadSchemaAchievementCount(appId));
            var progress = await Task.Run(() => TryReadLocalProgress(localPath));
            if (progress.HasValue == false)
            {
                await this._downloadSemaphore.WaitAsync();
                try
                {
                    progress = await Task.Run(() => this.LoadRemoteProgress(appId));
                }
                finally
                {
                    this._downloadSemaphore.Release();
                }
            }

            if (progress.HasValue)
            {
                return (progress.Value.Achieved, schemaTotal ?? progress.Value.Total);
            }

            return schemaTotal.HasValue ? (0, schemaTotal.Value) : null;
        }

        private int? TryReadSchemaAchievementCount(uint appId)
        {
            try
            {
                GameStatisticsSchemaLoader loader = new(appId, this._steamClient);
                return loader.TryGetAchievementCount(out int count) ? count : null;
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static (int Achieved, int Total)? TryReadLocalProgress(string path)
        {
            try
            {
                if (File.Exists(path) == false)
                {
                    return null;
                }

                using JsonDocument document = JsonDocument.Parse(File.ReadAllText(path));
                if (TryFindProgress(document.RootElement, out int achieved, out int total))
                {
                    return (achieved, total);
                }
            }
            catch (Exception)
            {
            }

            return null;
        }

        private static bool TryFindProgress(JsonElement value, out int achieved, out int total)
        {
            achieved = 0;
            total = 0;
            if (value.ValueKind == JsonValueKind.Object)
            {
                if (value.TryGetProperty("nAchieved", out JsonElement achievedValue) &&
                    value.TryGetProperty("nTotal", out JsonElement totalValue) &&
                    TryConvertCount(achievedValue, out achieved) &&
                    TryConvertCount(totalValue, out total))
                {
                    return total > 0;
                }

                foreach (JsonProperty property in value.EnumerateObject())
                {
                    if (TryFindProgress(property.Value, out achieved, out total))
                    {
                        return true;
                    }
                }
            }
            else if (value.ValueKind == JsonValueKind.Array)
            {
                foreach (JsonElement child in value.EnumerateArray())
                {
                    if (TryFindProgress(child, out achieved, out total))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private static bool TryConvertCount(JsonElement value, out int count)
        {
            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out count))
            {
                return count >= 0;
            }

            if (value.ValueKind == JsonValueKind.String &&
                int.TryParse(value.GetString(), NumberStyles.None, CultureInfo.InvariantCulture, out count))
            {
                return count >= 0;
            }

            count = 0;
            return false;
        }

        private (int Achieved, int Total)? LoadRemoteProgress(uint appId)
        {
            if (GameCacheService.TryReadAchievementProgress(this._steamId, appId, true, out byte[] cachedBytes) &&
                TryParseProgress(cachedBytes, out var cachedProgress))
            {
                return cachedProgress;
            }

            try
            {
                string uri = Format($"https://steamcommunity.com/profiles/{this._steamId}/stats/{appId}/?xml=1");
                byte[] bytes;
                bytes = HttpClient.GetByteArrayAsync(new Uri(uri)).GetAwaiter().GetResult();

                using MemoryStream stream = new(bytes, writable: false);
                XPathDocument document = new(stream);
                XPathNodeIterator achievements = document.CreateNavigator()
                    .Select("/playerstats/achievements/achievement");
                int achieved = 0;
                int total = 0;
                while (achievements.MoveNext())
                {
                    total++;
                    if (achievements.Current.GetAttribute("closed", "") == "1")
                    {
                        achieved++;
                    }
                }

                if (total == 0)
                {
                    return null;
                }

                byte[] cacheBytes = Encoding.UTF8.GetBytes(
                    ProgressCacheVersion + "|" + achieved.ToString(CultureInfo.InvariantCulture) + "|" +
                    total.ToString(CultureInfo.InvariantCulture));
                GameCacheService.WriteAchievementProgress(this._steamId, appId, cacheBytes);
                return (achieved, total);
            }
            catch (Exception)
            {
                if (GameCacheService.TryReadAchievementProgress(this._steamId, appId, false, out byte[] staleBytes) &&
                    TryParseProgress(staleBytes, out var staleProgress))
                {
                    return staleProgress;
                }
                return null;
            }
        }

        private static bool TryParseProgress(
            byte[] bytes,
            out (int Achieved, int Total) progress)
        {
            progress = default;
            string[] parts = Encoding.UTF8.GetString(bytes).Split('|');
            if (parts.Length != 3 || parts[0] != ProgressCacheVersion ||
                int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out int achieved) == false ||
                int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out int total) == false ||
                total <= 0)
            {
                return false;
            }

            progress = (achieved, total);
            return true;
        }
    }
}
