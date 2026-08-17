using System.Collections.Generic;
using SteamAchievementUnlocker.Stats;

namespace SteamAchievementUnlocker.Application.Ports
{
    internal interface IGameStatisticsSchemaSource
    {
        bool TryLoad(
            out List<AchievementDefinition> achievementDefinitions,
            out List<StatDefinition> statDefinitions);

        bool TryGetAchievementCount(out int count);
    }
}
