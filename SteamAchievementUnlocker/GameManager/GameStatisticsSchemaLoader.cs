using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using SteamAchievementUnlocker.Application.Ports;
using API = SteamIntegration;
using APITypes = SteamIntegration.Models;
using static SteamAchievementUnlocker.InvariantFormatting;

namespace SteamAchievementUnlocker
{
    public sealed class GameStatisticsSchemaLoader : IGameStatisticsSchemaSource
    {
        private readonly long _gameId;
        private readonly API.SteamClientSession _steamClient;

        public GameStatisticsSchemaLoader(long gameId, API.SteamClientSession steamClient)
        {
            this._gameId = gameId;
            this._steamClient = steamClient;
        }

        internal bool TryLoad(
            out List<Stats.AchievementDefinition> achievementDefinitions,
            out List<Stats.StatDefinition> statDefinitions)
        {
            achievementDefinitions = new();
            statDefinitions = new();

            string path;
            try
            {
                string fileName = Format($"UserGameStatsSchema_{this._gameId}.bin");
                path = Path.Combine(API.SteamInstallLocator.GetInstallPath(), "appcache", "stats", fileName);
                if (File.Exists(path) == false)
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
            }

            KeyValueNode data = KeyValueNode.LoadAsBinary(path);
            if (data == null)
            {
                return false;
            }

            string currentLanguage = this._steamClient.Apps.GetCurrentGameLanguage();
            KeyValueNode stats = data[this._gameId.ToString(CultureInfo.InvariantCulture)]["stats"];
            if (stats.Valid == false || stats.Children == null)
            {
                return false;
            }

            foreach (KeyValueNode stat in stats.Children)
            {
                if (stat.Valid == false)
                {
                    continue;
                }

                APITypes.UserAccountStatType type = GetStatType(stat);
                switch (type)
                {
                    case APITypes.UserAccountStatType.Integer:
                        statDefinitions.Add(CreateIntegerDefinition(stat, currentLanguage));
                        break;

                    case APITypes.UserAccountStatType.Float:
                    case APITypes.UserAccountStatType.AverageRate:
                        statDefinitions.Add(CreateFloatDefinition(stat, currentLanguage));
                        break;

                    case APITypes.UserAccountStatType.Achievements:
                    case APITypes.UserAccountStatType.GroupAchievements:
                        AddAchievementDefinitions(achievementDefinitions, stat, currentLanguage);
                        break;

                    case APITypes.UserAccountStatType.Invalid:
                        break;

                    default:
                        throw new InvalidOperationException("invalid stat type");
                }
            }

            return true;
        }

        bool IGameStatisticsSchemaSource.TryLoad(
            out List<Stats.AchievementDefinition> achievementDefinitions,
            out List<Stats.StatDefinition> statDefinitions) =>
            this.TryLoad(out achievementDefinitions, out statDefinitions);

        public bool TryGetAchievementCount(out int count)
        {
            count = 0;
            if (this.TryLoad(out List<Stats.AchievementDefinition> achievements, out _))
            {
                count = achievements.Count;
            }

            return count > 0;
        }

        private static APITypes.UserAccountStatType GetStatType(KeyValueNode stat)
        {
            KeyValueNode typeNode = stat["type"];
            if (typeNode.Valid && typeNode.Type == KeyValueNodeType.String &&
                Enum.TryParse((string)typeNode.Value, true, out APITypes.UserAccountStatType type))
            {
                return type;
            }

            KeyValueNode typeIntNode = stat["type_int"];
            int rawType = typeIntNode.Valid ? typeIntNode.AsInteger(0) : typeNode.AsInteger(0);
            return (APITypes.UserAccountStatType)rawType;
        }

        private static Stats.IntegerStatDefinition CreateIntegerDefinition(
            KeyValueNode stat,
            string language)
        {
            string id = stat["name"].AsString("");
            return new Stats.IntegerStatDefinition
            {
                Id = id,
                DisplayName = GetLocalizedString(stat["display"]["name"], language, id),
                MinValue = stat["min"].AsInteger(int.MinValue),
                MaxValue = stat["max"].AsInteger(int.MaxValue),
                MaxChange = stat["maxchange"].AsInteger(0),
                IncrementOnly = stat["incrementonly"].AsBoolean(false),
                SetByTrustedGameServer = stat["bSetByTrustedGS"].AsBoolean(false),
                DefaultValue = stat["default"].AsInteger(0),
                Permission = stat["permission"].AsInteger(0),
            };
        }

        private static Stats.FloatStatDefinition CreateFloatDefinition(
            KeyValueNode stat,
            string language)
        {
            string id = stat["name"].AsString("");
            return new Stats.FloatStatDefinition
            {
                Id = id,
                DisplayName = GetLocalizedString(stat["display"]["name"], language, id),
                MinValue = stat["min"].AsFloat(float.MinValue),
                MaxValue = stat["max"].AsFloat(float.MaxValue),
                MaxChange = stat["maxchange"].AsFloat(0.0f),
                IncrementOnly = stat["incrementonly"].AsBoolean(false),
                DefaultValue = stat["default"].AsFloat(0.0f),
                Permission = stat["permission"].AsInteger(0),
            };
        }

        private static void AddAchievementDefinitions(
            List<Stats.AchievementDefinition> definitions,
            KeyValueNode stat,
            string language)
        {
            if (stat.Children == null)
            {
                return;
            }

            foreach (KeyValueNode bits in stat.Children.Where(
                node => string.Equals(node.Name, "bits", StringComparison.InvariantCultureIgnoreCase)))
            {
                if (bits.Valid == false || bits.Children == null)
                {
                    continue;
                }

                foreach (KeyValueNode bit in bits.Children)
                {
                    string id = bit["name"].AsString("");
                    definitions.Add(new Stats.AchievementDefinition
                    {
                        Id = id,
                        Name = GetLocalizedString(bit["display"]["name"], language, id),
                        Description = GetLocalizedString(bit["display"]["desc"], language, ""),
                        IconNormal = bit["display"]["icon"].AsString(""),
                        IconLocked = bit["display"]["icon_gray"].AsString(""),
                        IsHidden = bit["display"]["hidden"].AsBoolean(false),
                        Permission = bit["permission"].AsInteger(0),
                    });
                }
            }
        }

        private static string GetLocalizedString(
            KeyValueNode value,
            string language,
            string defaultValue)
        {
            string name = value[language].AsString("");
            if (string.IsNullOrEmpty(name) == false)
            {
                return name;
            }

            if (language != "english")
            {
                name = value["english"].AsString("");
                if (string.IsNullOrEmpty(name) == false)
                {
                    return name;
                }
            }

            name = value.AsString("");
            return string.IsNullOrEmpty(name) ? defaultValue : name;
        }
    }
}
