using System.Collections.Generic;
using System.Linq;
using SteamAchievementUnlocker.Application.Ports;
using SteamAchievementUnlocker.Stats;
using API = SteamIntegration;

namespace SteamAchievementUnlocker
{
    internal sealed class StatisticsStoreService : IStatisticsStore
    {
        private readonly API.SteamClientSession _steamClient;

        public StatisticsStoreService(API.SteamClientSession steamClient)
        {
            this._steamClient = steamClient;
        }

        public int StoreAchievements(
            IEnumerable<AchievementInfo> achievements,
            out string failedAchievementId)
        {
            failedAchievementId = null;
            List<AchievementInfo> modifiedAchievements = achievements
                .Where(info => info.OriginalIsAchieved != info.IsAchieved)
                .ToList();

            foreach (AchievementInfo achievement in modifiedAchievements)
            {
                if (this._steamClient.UserStatistics.SetAchievementState(
                    achievement.Id,
                    achievement.IsAchieved) == false)
                {
                    failedAchievementId = achievement.Id;
                    return -1;
                }

                achievement.OriginalIsAchieved = achievement.IsAchieved;
            }

            return modifiedAchievements.Count;
        }

        public int StoreStatistics(
            IEnumerable<StatInfo> statistics,
            out string failedStatisticId)
        {
            failedStatisticId = null;
            List<StatInfo> modifiedStatistics = statistics
                .Where(statistic => statistic.IsModified)
                .ToList();

            foreach (StatInfo statistic in modifiedStatistics)
            {
                bool stored = statistic switch
                {
                    IntegerStatInfo integerStatistic => this._steamClient.UserStatistics.SetIntegerStat(
                        integerStatistic.Id,
                        integerStatistic.IntValue),
                    FloatStatInfo floatStatistic => this._steamClient.UserStatistics.SetFloatStat(
                        floatStatistic.Id,
                        floatStatistic.FloatValue),
                    _ => throw new System.InvalidOperationException("unsupported stat type"),
                };

                if (stored == false)
                {
                    failedStatisticId = statistic.Id;
                    return -1;
                }
            }

            return modifiedStatistics.Count;
        }
    }
}
