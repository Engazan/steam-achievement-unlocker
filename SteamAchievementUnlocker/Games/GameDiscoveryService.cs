using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using SteamAchievementUnlocker.Application.Ports;
using API = SteamIntegration;

namespace SteamAchievementUnlocker
{
    internal sealed class GameDiscoveryService : IGameCatalog
    {
        internal const uint MaximumAppIdToScan = 6_000_000;
        private const string CacheVersion = "game-catalog-v1";

        private readonly API.SteamClientSession _steamClient;

        public GameDiscoveryService(API.SteamClientSession steamClient)
        {
            this._steamClient = steamClient;
        }

        public List<GameListEntry> LoadOwnedGames(
            bool forceRefresh,
            IProgress<uint> scanProgress = null)
        {
            if (forceRefresh == false &&
                GameCacheService.TryReadGameList(true, out byte[] cachedBytes) &&
                TryParseCachedGames(cachedBytes, out List<GameListEntry> cachedGames))
            {
                scanProgress?.Report(MaximumAppIdToScan);
                return cachedGames;
            }

            List<GameListEntry> games = new();
            scanProgress?.Report(0);
            for (uint appId = 1; appId <= MaximumAppIdToScan; appId++)
            {
                if (appId % 1_000 == 0)
                {
                    scanProgress?.Report(appId);
                }

                if (this._steamClient.Apps.IsSubscribedApp(appId) == false)
                {
                    continue;
                }

                string gameType = this.GetSupportedGameType(appId);
                if (gameType == null)
                {
                    continue;
                }

                games.Add(new GameListEntry(appId, gameType)
                {
                    Name = this._steamClient.ApplicationData.GetAppData(appId, "name") ??
                           appId.ToString(CultureInfo.InvariantCulture),
                });
            }

            scanProgress?.Report(MaximumAppIdToScan);
            GameCacheService.WriteGameList(SerializeGames(games));
            return games;
        }

        private string GetSupportedGameType(uint appId)
        {
            string appType = this._steamClient.ApplicationData.GetAppData(appId, "type") ??
                              this._steamClient.ApplicationData.GetAppData(appId, "app_type");
            if (string.Equals(appType, "game", StringComparison.OrdinalIgnoreCase))
            {
                return "normal";
            }

            return string.Equals(appType, "demo", StringComparison.OrdinalIgnoreCase)
                ? "demo"
                : null;
        }

        private static byte[] SerializeGames(IEnumerable<GameListEntry> games)
        {
            StringBuilder builder = new();
            builder.AppendLine(CacheVersion);
            foreach (GameListEntry game in games)
            {
                string name = Convert.ToBase64String(Encoding.UTF8.GetBytes(game.Name ?? string.Empty));
                builder.Append(game.AppId.ToString(CultureInfo.InvariantCulture));
                builder.Append('|');
                builder.Append(game.GameType);
                builder.Append('|');
                builder.AppendLine(name);
            }
            return Encoding.UTF8.GetBytes(builder.ToString());
        }

        private static bool TryParseCachedGames(
            byte[] bytes,
            out List<GameListEntry> games)
        {
            games = new();
            string[] lines = Encoding.UTF8.GetString(bytes)
                .Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length == 0 || lines[0] != CacheVersion)
            {
                return false;
            }

            for (int index = 1; index < lines.Length; index++)
            {
                string[] parts = lines[index].Split('|');
                if (parts.Length != 3 ||
                    uint.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out uint appId) == false ||
                    IsSupportedGameType(parts[1]) == false ||
                    TryDecodeName(parts[2], out string name) == false)
                {
                    games.Clear();
                    return false;
                }

                games.Add(new GameListEntry(appId, parts[1]) { Name = name });
            }

            return true;
        }

        private static bool TryDecodeName(string value, out string name)
        {
            try
            {
                name = Encoding.UTF8.GetString(Convert.FromBase64String(value));
                return true;
            }
            catch (FormatException)
            {
                name = null;
                return false;
            }
        }

        private static bool IsSupportedGameType(string gameType)
        {
            return string.Equals(gameType, "normal", StringComparison.Ordinal) ||
                   string.Equals(gameType, "demo", StringComparison.Ordinal);
        }
    }
}
