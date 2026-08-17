using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Windows.Media.Imaging;
using static SteamAchievementUnlocker.InvariantFormatting;

namespace SteamAchievementUnlocker
{
    internal sealed class AchievementIconService : IDisposable
    {
        private readonly long _gameId;
        private readonly List<Stats.AchievementInfo> _queue = new();
        private readonly Dictionary<string, BitmapImage> _icons = new();
        private static readonly HttpClient HttpClient = new();

        public AchievementIconService(long gameId)
        {
            this._gameId = gameId;
        }

        public event Action<Stats.AchievementInfo, BitmapImage> IconLoaded;
        public event Action<int> QueueChanged;

        public void Queue(Stats.AchievementInfo info, bool startDownload)
        {
            string key = info.IsAchieved ? info.IconNormal : info.IconLocked;
            if (string.IsNullOrEmpty(key))
            {
                info.IsIconLoading = false;
                return;
            }

            if (this._icons.TryGetValue(key, out BitmapImage image))
            {
                info.IsIconLoading = false;
                info.Icon = image;
                return;
            }

            info.IsIconLoading = true;
            this._queue.Add(info);
            this.QueueChanged?.Invoke(this._queue.Count);

            if (startDownload)
            {
                this.StartNextDownload();
            }
        }

        public void StartQueuedDownloads() => this.StartNextDownload();

        public void Dispose()
        {
            this._queue.Clear();
        }

        private async void StartNextDownload()
        {
            if (this._queue.Count == 0)
            {
                this.QueueChanged?.Invoke(0);
                return;
            }

            Stats.AchievementInfo info = this._queue[0];
            this._queue.RemoveAt(0);
            this.QueueChanged?.Invoke(this._queue.Count);

            string iconName = info.IsAchieved ? info.IconNormal : info.IconLocked;
            byte[] data = null;
            try
            {
                data = await HttpClient.GetByteArrayAsync(new Uri(
                    Format($"https://cdn.steamstatic.com/steamcommunity/public/images/apps/{this._gameId}/{iconName}")));
            }
            catch (Exception)
            {
            }

            BitmapImage bitmap = null;
            if (data != null)
            {
                try
                {
                    using MemoryStream stream = new(data, writable: false);
                    bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.StreamSource = stream;
                    bitmap.EndInit();
                    bitmap.Freeze();
                }
                catch (Exception)
                {
                    bitmap = null;
                }
            }

            if (bitmap != null)
            {
                string key = info.IsAchieved ? info.IconNormal : info.IconLocked;
                if (string.IsNullOrEmpty(key) == false)
                {
                    this._icons[key] = bitmap;
                }
            }

            info.Icon = bitmap;
            info.IsIconLoading = false;
            this.IconLoaded?.Invoke(info, bitmap);
            this.StartNextDownload();
        }
    }
}
