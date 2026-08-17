using System;
using SteamAchievementUnlocker.Application.Ports;
using API = SteamIntegration;
using APITypes = SteamIntegration.Models;

namespace SteamAchievementUnlocker
{
    internal sealed class StatisticsRequestService : IStatisticsRequestGateway
    {
        private readonly API.SteamClientSession _steamClient;
        private API.SteamCallHandle _globalAchievementPercentagesCall = API.SteamCallHandle.Invalid;
        private bool _globalAchievementPercentagesAvailable;

        public StatisticsRequestService(API.SteamClientSession steamClient)
        {
            this._steamClient = steamClient;
        }

        public bool RequestUserStats()
        {
            ulong steamId = this._steamClient.UserAccount.GetSteamId();
            return this._steamClient.UserStatistics.RequestUserStats(steamId) !=
                   API.SteamCallHandle.Invalid;
        }

        public void RequestGlobalAchievementPercentages()
        {
            if (this._globalAchievementPercentagesCall == API.SteamCallHandle.Invalid)
            {
                this._globalAchievementPercentagesCall =
                    this._steamClient.UserStatistics.RequestGlobalAchievementPercentages();
            }
        }

        public float? GetGlobalAchievementPercentage(string achievementId)
        {
            if (this._globalAchievementPercentagesAvailable == false ||
                this._steamClient.UserStatistics.GetGlobalAchievementUnlockPercentage(
                    achievementId,
                    out float percentage) == false ||
                float.IsNaN(percentage) ||
                float.IsInfinity(percentage))
            {
                return null;
            }

            return percentage;
        }

        public bool TryProcessGlobalAchievementPercentages()
        {
            API.SteamCallHandle callHandle = this._globalAchievementPercentagesCall;
            if (callHandle == API.SteamCallHandle.Invalid ||
                this._steamClient.CallbackUtilities.IsApiCallCompleted(callHandle, out bool failed) == false)
            {
                return false;
            }

            this._globalAchievementPercentagesCall = API.SteamCallHandle.Invalid;
            if (failed ||
                this._steamClient.CallbackUtilities.GetApiCallResult(
                    callHandle,
                    APITypes.GlobalAchievementPercentagesReadyCallbackData.CallbackId,
                    out APITypes.GlobalAchievementPercentagesReadyCallbackData result,
                    out bool callbackFailed) == false ||
                callbackFailed ||
                result.Result != 1)
            {
                return false;
            }

            this._globalAchievementPercentagesAvailable = true;
            return true;
        }
    }
}
