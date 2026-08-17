using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using SteamAchievementUnlocker.Application.Ports;
using static SteamAchievementUnlocker.InvariantFormatting;
using API = SteamIntegration;

namespace SteamAchievementUnlocker
{
    internal sealed class GameArtworkService : IGameArtworkProvider
    {
        private readonly API.SteamClientSession _steamClient;
        private static readonly HttpClient HttpClient = new();

        public GameArtworkService(API.SteamClientSession steamClient)
        {
            this._steamClient = steamClient;
        }

        public string GetImageUrl(uint appId)
        {
            string currentLanguage = this._steamClient.Apps.GetCurrentGameLanguage();
            string candidate = this._steamClient.ApplicationData.GetAppData(
                appId,
                Format($"small_capsule/{currentLanguage}"));
            if (string.IsNullOrEmpty(candidate) == false)
            {
                return Format($"https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/{appId}/{candidate}");
            }

            if (currentLanguage != "english")
            {
                candidate = this._steamClient.ApplicationData.GetAppData(appId, "small_capsule/english");
                if (string.IsNullOrEmpty(candidate) == false)
                {
                    return Format($"https://shared.cloudflare.steamstatic.com/store_item_assets/steam/apps/{appId}/{candidate}");
                }
            }

            candidate = this._steamClient.ApplicationData.GetAppData(appId, "logo");
            return string.IsNullOrEmpty(candidate)
                ? null
                : Format($"https://cdn.steamstatic.com/steamcommunity/public/images/apps/{appId}/{candidate}.jpg");
        }

        public Task<BitmapImage> LoadAsync(GameListEntry game)
        {
            return Task.Run(() =>
            {
                byte[] data = LoadBytes(game.AppId, game.ImageUrl);
                return data != null && TryCreateBitmap(data, out BitmapImage image) ? image : null;
            });
        }

        private static byte[] LoadBytes(uint appId, string imageUrl)
        {
            if (GameCacheService.TryReadGameLogo(appId, true, out byte[] cachedData))
            {
                return cachedData;
            }

            try
            {
                byte[] data = HttpClient.GetByteArrayAsync(new Uri(imageUrl)).GetAwaiter().GetResult();
                GameCacheService.WriteGameLogo(appId, data);
                return data;
            }
            catch (Exception)
            {
                return GameCacheService.TryReadGameLogo(appId, false, out byte[] staleData)
                    ? staleData
                    : null;
            }
        }

        private static bool TryCreateBitmap(byte[] data, out BitmapImage bitmap)
        {
            bitmap = null;
            try
            {
                using (MemoryStream stream = new(data, false))
                {
                    BitmapImage image = new();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    image.StreamSource = stream;
                    image.EndInit();
                    image.Freeze();
                    bitmap = image;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
