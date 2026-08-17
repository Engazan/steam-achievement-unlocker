namespace SteamAchievementUnlocker.Application.Ports
{
    internal interface IStatisticsRequestGateway
    {
        bool RequestUserStats();

        void RequestGlobalAchievementPercentages();

        float? GetGlobalAchievementPercentage(string achievementId);

        bool TryProcessGlobalAchievementPercentages();
    }
}
