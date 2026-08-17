namespace SteamIntegration
{
    public sealed class ApplicationMetadata
    {
        private readonly SteamInterfaces.ApplicationMetadataInterface _nativeInterface;

        internal ApplicationMetadata(SteamInterfaces.ApplicationMetadataInterface nativeInterface)
        {
            this._nativeInterface = nativeInterface;
        }

        public string GetAppData(uint appId, string key) =>
            this._nativeInterface.GetAppData(appId, key);
    }
}
