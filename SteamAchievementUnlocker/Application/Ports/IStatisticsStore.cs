using System.Collections.Generic;
using SteamAchievementUnlocker.Stats;

namespace SteamAchievementUnlocker.Application.Ports
{
    internal interface IStatisticsStore
    {
        int StoreAchievements(
            IEnumerable<AchievementInfo> achievements,
            out string failedAchievementId);

        int StoreStatistics(
            IEnumerable<StatInfo> statistics,
            out string failedStatisticId);
    }
}
