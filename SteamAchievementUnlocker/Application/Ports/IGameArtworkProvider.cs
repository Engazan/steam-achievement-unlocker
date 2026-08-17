using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace SteamAchievementUnlocker.Application.Ports
{
    internal interface IGameArtworkProvider
    {
        string GetImageUrl(uint appId);

        Task<BitmapImage> LoadAsync(GameListEntry game);
    }
}
