namespace SteamIntegration
{
    public enum SteamClientInitializationFailure : byte
    {
        Unknown = 0,
        GetInstallPath,
        Load,
        CreateSteamClient,
        CreateSteamPipe,
        ConnectToGlobalUser,
        AppIdMismatch,
    }
}
