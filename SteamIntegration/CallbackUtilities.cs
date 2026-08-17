namespace SteamIntegration
{
    public sealed class CallbackUtilities
    {
        private readonly SteamInterfaces.CallbackUtilitiesInterface _nativeInterface;

        internal CallbackUtilities(SteamInterfaces.CallbackUtilitiesInterface nativeInterface)
        {
            this._nativeInterface = nativeInterface;
        }

        public bool IsApiCallCompleted(SteamCallHandle callHandle, out bool failed) =>
            this._nativeInterface.IsApiCallCompleted(callHandle, out failed);

        public bool GetApiCallResult<T>(
            SteamCallHandle callHandle,
            int expectedCallbackId,
            out T result,
            out bool failed)
            where T : struct =>
            this._nativeInterface.GetApiCallResult(
                callHandle,
                expectedCallbackId,
                out result,
                out failed);

        public int GetConnectedUniverse() => this._nativeInterface.GetConnectedUniverse();

        public string GetIpCountry() => this._nativeInterface.GetIPCountry();

        internal uint GetCurrentAppId() => this._nativeInterface.GetCurrentAppId();
    }
}
