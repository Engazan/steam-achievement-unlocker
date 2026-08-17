namespace SteamIntegration
{
    public static class SteamInstallLocator
    {
        public static string GetInstallPath() => NativeSteamRuntime.GetInstallPath();
    }
}
