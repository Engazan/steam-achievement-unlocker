namespace SteamIntegration
{
    public sealed class InstalledApplicationCatalog
    {
        private readonly SteamInterfaces.InstalledApplicationCatalogInterface _nativeInterface;

        internal InstalledApplicationCatalog(SteamInterfaces.InstalledApplicationCatalogInterface nativeInterface)
        {
            this._nativeInterface = nativeInterface;
        }

        public bool IsSubscribedApp(uint appId) =>
            this._nativeInterface.IsSubscribedApp(appId);

        public string GetCurrentGameLanguage() =>
            this._nativeInterface.GetCurrentGameLanguage();
    }
}
