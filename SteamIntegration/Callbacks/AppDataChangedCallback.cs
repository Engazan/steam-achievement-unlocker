namespace SteamIntegration.Callbacks
{
    public class AppDataChangedCallback : SteamCallback<Models.AppDataChangedCallbackData>
    {
        public override int Id => 1001;
        public override bool IsServer => false;
    }
}
