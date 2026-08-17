using System;

namespace SteamIntegration
{
    public class SteamClientInitializationException : Exception
    {
        public readonly SteamClientInitializationFailure Failure;

        public SteamClientInitializationException(SteamClientInitializationFailure failure)
        {
            this.Failure = failure;
        }

        public SteamClientInitializationException(SteamClientInitializationFailure failure, string message)
            : base(message)
        {
            this.Failure = failure;
        }

        public SteamClientInitializationException(SteamClientInitializationFailure failure, string message, Exception innerException)
            : base(message, innerException)
        {
            this.Failure = failure;
        }
    }
}
