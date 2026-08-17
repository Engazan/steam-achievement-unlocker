namespace SteamAchievementUnlocker.Stats
{
    internal class AchievementDefinition
    {
        public string Id;
        public string Name;
        public string Description;
        public string IconNormal;
        public string IconLocked;
        public bool IsHidden;
        public int Permission;

        public override string ToString()
        {
            return $"{this.Name ?? this.Id ?? base.ToString()}: {this.Permission}";
        }
    }
}
