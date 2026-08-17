namespace SteamIntegration.Callbacks
{
    public class UserStatsReceivedCallback : SteamCallback<Models.UserStatsReceivedCallbackData>
    {
        public override int Id => 1101;
        public override bool IsServer => false;
    }
}
