using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SteamAchievementUnlocker.Tests
{
    [TestClass]
    public sealed class GameListFilterServiceTests
    {
        [TestMethod]
        public void FilterIncludesOnlySelectedSupportedTypes()
        {
            GameListFilterService filter = new();
            List<GameListEntry> games = new()
            {
                new GameListEntry(1, "normal") { Name = "Game" },
                new GameListEntry(2, "demo") { Name = "Demo" },
                new GameListEntry(3, "mod") { Name = "Unsupported" },
            };

            IReadOnlyList<GameListEntry> result = filter.Filter(
                games,
                null,
                includeGames: true,
                includeDemos: false);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(1U, result[0].AppId);
        }
    }
}
