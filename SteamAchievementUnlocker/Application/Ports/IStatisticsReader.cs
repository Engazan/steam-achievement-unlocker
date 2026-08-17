using System.Collections.Generic;
using SteamAchievementUnlocker.Stats;

namespace SteamAchievementUnlocker.Application.Ports
{
    internal interface IStatisticsReader
    {
        List<StatInfo> Read(IEnumerable<StatDefinition> definitions);
    }
}
