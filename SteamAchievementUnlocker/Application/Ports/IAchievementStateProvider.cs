using System;
using System.Collections.Generic;
using SteamAchievementUnlocker.Stats;

namespace SteamAchievementUnlocker.Application.Ports
{
    internal interface IAchievementStateProvider
    {
        List<AchievementInfo> Read(
            IEnumerable<AchievementDefinition> definitions,
            string search,
            bool lockedOnly,
            bool unlockedOnly,
            Func<string, float?> globalPercentageProvider,
            Action<AchievementInfo, bool> queueIcon);
    }
}
