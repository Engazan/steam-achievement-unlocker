namespace SteamIntegration
{
    public sealed class UserAccount
    {
        private readonly SteamInterfaces.UserAccountInterface _nativeInterface;

        internal UserAccount(SteamInterfaces.UserAccountInterface nativeInterface)
        {
            this._nativeInterface = nativeInterface;
        }

        public bool IsLoggedIn() => this._nativeInterface.IsLoggedIn();

        public ulong GetSteamId() => this._nativeInterface.GetSteamId();
    }
}
