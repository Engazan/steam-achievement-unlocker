using System;
using System.Collections.Generic;
using SteamAchievementUnlocker.Application.Ports;
using SteamAchievementUnlocker.Stats;
using API = SteamIntegration;

namespace SteamAchievementUnlocker
{
    internal sealed class AchievementStateReader : IAchievementStateProvider
    {
        private readonly API.SteamClientSession _steamClient;

        public AchievementStateReader(API.SteamClientSession steamClient)
        {
            this._steamClient = steamClient;
        }

        public List<AchievementInfo> Read(
            IEnumerable<AchievementDefinition> definitions,
            string search,
            bool lockedOnly,
            bool unlockedOnly,
            Func<string, float?> globalPercentageProvider,
            Action<AchievementInfo, bool> queueIcon)
        {
            List<AchievementInfo> achievements = new();
            foreach (AchievementDefinition definition in definitions)
            {
                if (string.IsNullOrEmpty(definition.Id) ||
                    this._steamClient.UserStatistics.GetAchievementStateAndUnlockTime(
                        definition.Id,
                        out bool isAchieved,
                        out uint unlockTime) == false)
                {
                    continue;
                }

                bool included = (lockedOnly == false && unlockedOnly == false) ||
                    isAchieved switch
                    {
                        true => unlockedOnly,
                        false => lockedOnly,
                    };
                if (included == false ||
                    MatchesSearch(definition, search) == false)
                {
                    continue;
                }

                AchievementInfo achievement = new()
                {
                    Id = definition.Id,
                    OriginalIsAchieved = isAchieved,
                    IsAchieved = isAchieved,
                    UnlockTime = isAchieved && unlockTime > 0
                        ? DateTimeOffset.FromUnixTimeSeconds(unlockTime).LocalDateTime
                        : null,
                    IconNormal = string.IsNullOrEmpty(definition.IconNormal) ? null : definition.IconNormal,
                    IconLocked = string.IsNullOrEmpty(definition.IconLocked)
                        ? definition.IconNormal
                        : definition.IconLocked,
                    Permission = definition.Permission,
                    Name = definition.Name,
                    Description = definition.Description,
                    GlobalUnlockPercentage = globalPercentageProvider(definition.Id),
                };

                if (achievement.Name.StartsWith("#", StringComparison.InvariantCulture))
                {
                    achievement.Name = achievement.Id;
                    achievement.Description = string.Empty;
                }

                queueIcon(achievement, false);
                achievements.Add(achievement);
            }

            return achievements;
        }

        private static bool MatchesSearch(AchievementDefinition definition, string search)
        {
            return string.IsNullOrEmpty(search) ||
                definition.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                definition.Description.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;
        }
    }
}
