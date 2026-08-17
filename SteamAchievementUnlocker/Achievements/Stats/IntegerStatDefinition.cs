namespace SteamAchievementUnlocker.Stats
{
    internal class IntegerStatDefinition : StatDefinition
    {
        public int MinValue;
        public int MaxValue;
        public int MaxChange;
        public bool IncrementOnly;
        public bool SetByTrustedGameServer;
        public int DefaultValue;
    }
}
