using System;
using System.Globalization;
using System.IO;

namespace SteamAchievementUnlocker
{
    internal static class GameCacheService
    {
        private const string CacheDirectoryName = "cache-v2";
        private static readonly TimeSpan GameListCacheDuration = TimeSpan.FromDays(30);
        private static readonly TimeSpan GameLogoCacheDuration = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan AchievementProgressCacheDuration = TimeSpan.FromMinutes(5);

        public static bool TryReadGameList(bool requireFresh, out byte[] bytes) =>
            TryRead(GetGameListPath(), GameListCacheDuration, requireFresh, out bytes);

        public static void WriteGameList(byte[] bytes) => Write(GetGameListPath(), bytes);

        public static bool TryReadGameLogo(uint appId, bool requireFresh, out byte[] bytes) =>
            TryRead(GetGameLogoPath(appId), GameLogoCacheDuration, requireFresh, out bytes);

        public static void WriteGameLogo(uint appId, byte[] bytes) => Write(GetGameLogoPath(appId), bytes);

        public static bool TryReadAchievementProgress(
            ulong steamId,
            uint appId,
            bool requireFresh,
            out byte[] bytes) =>
            TryRead(
                GetAchievementProgressPath(steamId, appId),
                AchievementProgressCacheDuration,
                requireFresh,
                out bytes);

        public static void WriteAchievementProgress(ulong steamId, uint appId, byte[] bytes) =>
            Write(GetAchievementProgressPath(steamId, appId), bytes);

        private static string GetGameListPath() => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SteamAchievementUnlocker", CacheDirectoryName, "games.catalog");

        private static string GetGameLogoPath(uint appId) => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SteamAchievementUnlocker", CacheDirectoryName, "artwork",
            appId.ToString(CultureInfo.InvariantCulture) + ".img");

        private static string GetAchievementProgressPath(ulong steamId, uint appId) => Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "SteamAchievementUnlocker",
            CacheDirectoryName,
            "achievement-progress",
            steamId.ToString(CultureInfo.InvariantCulture),
            appId.ToString(CultureInfo.InvariantCulture) + ".txt");

        private static bool TryRead(string path, TimeSpan duration, bool requireFresh, out byte[] bytes)
        {
            bytes = null;
            try
            {
                if (File.Exists(path) == false ||
                    (requireFresh && File.GetLastWriteTimeUtc(path) < DateTime.UtcNow.Subtract(duration)))
                {
                    return false;
                }

                bytes = File.ReadAllBytes(path);
                return bytes.Length > 0;
            }
            catch (IOException)
            {
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        private static void Write(string path, byte[] bytes)
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllBytes(path, bytes);
            }
            catch (IOException)
            {
            }
            catch (UnauthorizedAccessException)
            {
            }
        }
    }
}
