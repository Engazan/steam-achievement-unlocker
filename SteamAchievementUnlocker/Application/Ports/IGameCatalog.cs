using System;
using System.Collections.Generic;

namespace SteamAchievementUnlocker.Application.Ports
{
    internal interface IGameCatalog
    {
        List<GameListEntry> LoadOwnedGames(
            bool forceRefresh,
            IProgress<uint> scanProgress = null);
    }
}
