using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace SteamIntegration
{
    public class SteamClientSession : IDisposable
    {
        public UserAccount UserAccount { get; private set; }
        public UserStatistics UserStatistics { get; private set; }
        public CallbackUtilities CallbackUtilities { get; private set; }
        public ApplicationMetadata ApplicationData { get; private set; }
        public InstalledApplicationCatalog Apps { get; private set; }

        private SteamInterfaces.SteamClientInterface _nativeClient;
        private bool _IsDisposed = false;
        private bool _IsInitialized = false;
        private int _Pipe;
        private int _User;

        private readonly List<ISteamCallback> _Callbacks = new();

        public void Initialize(long appId)
        {
            ThrowIfDisposed();
            if (this._IsInitialized == true)
            {
                throw new InvalidOperationException("Steam API client is already initialized.");
            }

            if (string.IsNullOrEmpty(NativeSteamRuntime.GetInstallPath()) == true)
            {
                throw new SteamClientInitializationException(SteamClientInitializationFailure.GetInstallPath, "failed to get Steam install path");
            }

            if (appId != 0)
            {
                Environment.SetEnvironmentVariable("SteamAppId", appId.ToString(CultureInfo.InvariantCulture));
            }

            if (NativeSteamRuntime.Load() == false)
            {
                throw new SteamClientInitializationException(SteamClientInitializationFailure.Load, "failed to load SteamClient");
            }

            try
            {
                this._nativeClient = NativeSteamRuntime.CreateInterface<SteamInterfaces.SteamClientInterface>(NativeInterfaceVersions.Client);
                if (this._nativeClient == null)
                {
                    throw new SteamClientInitializationException(SteamClientInitializationFailure.CreateSteamClient, "failed to create Steam client interface");
                }

                this._Pipe = this._nativeClient.CreateSteamPipe();
            if (this._Pipe == 0)
            {
                throw new SteamClientInitializationException(SteamClientInitializationFailure.CreateSteamPipe, "failed to create pipe");
            }

                this._User = this._nativeClient.ConnectToGlobalUser(this._Pipe);
            if (this._User == 0)
            {
                throw new SteamClientInitializationException(SteamClientInitializationFailure.ConnectToGlobalUser, "failed to connect to global user");
            }

            var nativeUtils = this._nativeClient.GetCallbackUtilitiesInterface(this._Pipe);
            this.CallbackUtilities = nativeUtils == null ? null : new CallbackUtilities(nativeUtils);
            if (this.CallbackUtilities == null)
            {
                throw new SteamClientInitializationException(SteamClientInitializationFailure.CreateSteamClient, "failed to create Steam utils interface");
            }
            if (appId > 0 && this.CallbackUtilities.GetCurrentAppId() != (uint)appId)
            {
                throw new SteamClientInitializationException(SteamClientInitializationFailure.AppIdMismatch, "appID mismatch");
            }

            var nativeUser = this._nativeClient.GetUserAccountInterface(this._User, this._Pipe);
            var nativeUserStats = this._nativeClient.GetUserStatisticsInterface(this._User, this._Pipe);
            var nativeApplicationData = this._nativeClient.GetApplicationDataInterface(this._User, this._Pipe);
            var nativeApps = this._nativeClient.GetInstalledApplicationCatalogInterface(this._User, this._Pipe);
            this.UserAccount = nativeUser == null ? null : new UserAccount(nativeUser);
            this.UserStatistics = nativeUserStats == null ? null : new UserStatistics(nativeUserStats);
            this.ApplicationData = nativeApplicationData == null ? null : new ApplicationMetadata(nativeApplicationData);
            this.Apps = nativeApps == null ? null : new InstalledApplicationCatalog(nativeApps);

            if (this.UserAccount == null ||
                this.UserStatistics == null ||
                this.ApplicationData == null ||
                this.Apps == null)
            {
                throw new SteamClientInitializationException(SteamClientInitializationFailure.CreateSteamClient, "failed to create Steam interface");
            }

                this._IsInitialized = true;
            }
            catch
            {
                ReleaseNativeResources();
                throw;
            }
        }

        ~SteamClientSession()
        {
            this.Dispose(false);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this._IsDisposed == true)
            {
                return;
            }

            ReleaseNativeResources();
            this._Callbacks.Clear();
            this._IsInitialized = false;
            this._IsDisposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public TCallback CreateAndRegisterCallback<TCallback>()
            where TCallback : ISteamCallback, new()
        {
            ThrowIfDisposed();
            TCallback callback = new();
            this._Callbacks.Add(callback);
            return callback;
        }

        private bool _RunningCallbacks;

        public void RunCallbacks(bool server)
        {
            ThrowIfDisposed();
            EnsureInitialized();

            if (this._RunningCallbacks == true)
            {
                return;
            }

            this._RunningCallbacks = true;
            try
            {
                Models.SteamCallbackMessage message;
                while (NativeSteamRuntime.GetCallback(this._Pipe, out message, out _) == true)
                {
                    var callbackId = message.Id;
                    foreach (ISteamCallback callback in this._Callbacks.Where(
                        candidate => candidate.Id == callbackId &&
                                     candidate.IsServer == server))
                    {
                        callback.Run(message.ParamPointer);
                    }
                    NativeSteamRuntime.FreeLastCallback(this._Pipe);
                }
            }
            finally
            {
                this._RunningCallbacks = false;
            }
        }

        private void ThrowIfDisposed()
        {
            if (this._IsDisposed == true)
            {
                throw new ObjectDisposedException(nameof(SteamClientSession));
            }
        }

        private void EnsureInitialized()
        {
            if (this._IsInitialized == false)
            {
                throw new InvalidOperationException("Steam API client is not initialized.");
            }
        }

        private void ReleaseNativeResources()
        {
            if (this._nativeClient != null && this._Pipe > 0)
            {
                if (this._User > 0)
                {
                    this._nativeClient.ReleaseUser(this._Pipe, this._User);
                    this._User = 0;
                }

                this._nativeClient.ReleaseSteamPipe(this._Pipe);
                this._Pipe = 0;
            }

            this._nativeClient = null;
            this.UserAccount = null;
            this.UserStatistics = null;
            this.CallbackUtilities = null;
            this.ApplicationData = null;
            this.Apps = null;
        }
    }
}
