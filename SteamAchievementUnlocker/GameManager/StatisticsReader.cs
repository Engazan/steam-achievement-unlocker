using System.Collections.Generic;
using SteamAchievementUnlocker.Application.Ports;
using SteamAchievementUnlocker.Stats;
using API = SteamIntegration;

namespace SteamAchievementUnlocker
{
    internal sealed class StatisticsReader : IStatisticsReader
    {
        private readonly API.SteamClientSession _steamClient;

        public StatisticsReader(API.SteamClientSession steamClient)
        {
            this._steamClient = steamClient;
        }

        public List<StatInfo> Read(IEnumerable<StatDefinition> definitions)
        {
            List<StatInfo> statistics = new();
            foreach (StatDefinition definition in definitions)
            {
                if (string.IsNullOrEmpty(definition.Id))
                {
                    continue;
                }

                if (definition is IntegerStatDefinition integerDefinition)
                {
                    if (this._steamClient.UserStatistics.GetIntegerStat(
                        integerDefinition.Id,
                        out int value) == false)
                    {
                        continue;
                    }

                    statistics.Add(new IntegerStatInfo
                    {
                        Id = integerDefinition.Id,
                        DisplayName = integerDefinition.DisplayName,
                        IntValue = value,
                        OriginalValue = value,
                        IsIncrementOnly = integerDefinition.IncrementOnly,
                        Permission = integerDefinition.Permission,
                    });
                }
                else if (definition is FloatStatDefinition floatDefinition)
                {
                    if (this._steamClient.UserStatistics.GetFloatStat(
                        floatDefinition.Id,
                        out float value) == false)
                    {
                        continue;
                    }

                    statistics.Add(new FloatStatInfo
                    {
                        Id = floatDefinition.Id,
                        DisplayName = floatDefinition.DisplayName,
                        FloatValue = value,
                        OriginalValue = value,
                        IsIncrementOnly = floatDefinition.IncrementOnly,
                        Permission = floatDefinition.Permission,
                    });
                }
            }

            return statistics;
        }
    }
}
