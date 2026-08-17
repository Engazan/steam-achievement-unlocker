namespace SteamIntegration
{
    public sealed class UserStatistics
    {
        private readonly SteamInterfaces.UserStatisticsInterface _nativeInterface;

        internal UserStatistics(SteamInterfaces.UserStatisticsInterface nativeInterface)
        {
            this._nativeInterface = nativeInterface;
        }

        public bool GetIntegerStat(string name, out int value) =>
            this._nativeInterface.GetIntegerStat(name, out value);

        public bool GetFloatStat(string name, out float value) =>
            this._nativeInterface.GetFloatStat(name, out value);

        public bool SetIntegerStat(string name, int value) =>
            this._nativeInterface.SetIntegerStat(name, value);

        public bool SetFloatStat(string name, float value) =>
            this._nativeInterface.SetFloatStat(name, value);

        public bool GetAchievementState(string name, out bool isAchieved) =>
            this._nativeInterface.GetAchievementState(name, out isAchieved);

        public bool SetAchievementState(string name, bool state) =>
            this._nativeInterface.SetAchievementState(name, state);

        public bool GetAchievementStateAndUnlockTime(
            string name,
            out bool isAchieved,
            out uint unlockTime) =>
            this._nativeInterface.GetAchievementStateAndUnlockTime(
                name,
                out isAchieved,
                out unlockTime);

        public bool StoreStats() => this._nativeInterface.StoreStats();

        public int GetAchievementIconHandle(string name) =>
            this._nativeInterface.GetAchievementIconHandle(name);

        public string GetAchievementDisplayAttribute(string name, string key) =>
            this._nativeInterface.GetAchievementDisplayAttribute(name, key);

        public SteamCallHandle RequestUserStats(ulong steamIdUser) =>
            this._nativeInterface.RequestUserStats(steamIdUser);

        public bool ResetAllStats(bool includeAchievements) =>
            this._nativeInterface.ResetAllStats(includeAchievements);

        public SteamCallHandle RequestGlobalAchievementPercentages() =>
            this._nativeInterface.RequestGlobalAchievementPercentages();

        public bool GetGlobalAchievementUnlockPercentage(string name, out float percent) =>
            this._nativeInterface.GetGlobalAchievementUnlockPercentage(name, out percent);
    }
}
