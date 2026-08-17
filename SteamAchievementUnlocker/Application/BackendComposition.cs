using SteamAchievementUnlocker.Application.Ports;
using API = SteamIntegration;

namespace SteamAchievementUnlocker.Composition
{
    internal static class BackendComposition
    {
        public static IGameCatalog CreateGameCatalog(API.SteamClientSession steamClient) =>
            new GameDiscoveryService(steamClient);

        public static IGameArtworkProvider CreateGameArtworkProvider(API.SteamClientSession steamClient) =>
            new GameArtworkService(steamClient);

        public static IGameStatisticsSchemaSource CreateStatisticsSchemaSource(
            long gameId,
            API.SteamClientSession steamClient) =>
            new GameStatisticsSchemaLoader(gameId, steamClient);

        public static IStatisticsRequestGateway CreateStatisticsRequestGateway(
            API.SteamClientSession steamClient) =>
            new StatisticsRequestService(steamClient);

        public static IStatisticsReader CreateStatisticsReader(API.SteamClientSession steamClient) =>
            new StatisticsReader(steamClient);

        public static IAchievementStateProvider CreateAchievementStateProvider(
            API.SteamClientSession steamClient) =>
            new AchievementStateReader(steamClient);

        public static IStatisticsStore CreateStatisticsStore(API.SteamClientSession steamClient) =>
            new StatisticsStoreService(steamClient);
    }
}
