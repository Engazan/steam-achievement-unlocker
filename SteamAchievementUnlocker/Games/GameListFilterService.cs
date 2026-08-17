using System;
using System.Collections.Generic;
using System.Linq;

namespace SteamAchievementUnlocker
{
    internal sealed class GameListFilterService
    {
        public IReadOnlyList<GameListEntry> Filter(
            IEnumerable<GameListEntry> games,
            string search,
            bool includeGames,
            bool includeDemos)
        {
            return games
                .Where(game => this.MatchesSearch(game, search))
                .Where(game => this.IsIncluded(game, includeGames, includeDemos))
                .OrderBy(game => game.Name)
                .ToList();
        }

        private bool MatchesSearch(GameListEntry game, string search) =>
            string.IsNullOrWhiteSpace(search) ||
            game.Name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0;

        private bool IsIncluded(
            GameListEntry game,
            bool includeGames,
            bool includeDemos)
        {
            return game.GameType switch
            {
                "normal" => includeGames,
                "demo" => includeDemos,
                _ => false,
            };
        }
    }
}
