using System;
using System.Threading;
using System.Threading.Tasks;
using NianticSpatial.NSDK.AR.Loader;
using UnityEngine;

namespace NianticSpatial.NSDK.AR.Auth
{
    /// <summary>
    /// Manager to ensure that the access token is refreshed in the background (assuming we have a valid refresh token).
    /// </summary>
    public static class AuthRuntimeRefreshManager
    {
        // Interval in seconds between checks for token expiration. Arbitrarily set to 10 seconds.
        private const double UpdateInterval = 10;

        private static CancellationTokenSource s_settingsUpdatedCts;

        public static async Task StartRefreshLoop(string userSessionRefreshToken)
        {
            await s_settingsUpdater.RequestRuntimeRefreshTokenAsync(
                userSessionRefreshToken, NsdkSettingsHelper.ActiveSettings, isRuntimeLogin: true);
            await s_settingsUpdater.RefreshRuntimeAccessAsync(NsdkSettingsHelper.ActiveSettings);
        }

        public static void CancelRefreshLoop()
        {
#if NIANTICSPATIAL_NSDK_AUTH_DEBUG
            Debug.Log("[Auth] Cancelling any existing refresh loop.");
#endif
            s_settingsUpdatedCts?.Cancel();
        }

        public static void RestartRefreshLoop()
        {
            CancelRefreshLoop();
            if (!string.IsNullOrEmpty(NsdkSettingsHelper.ActiveSettings?.RefreshToken))
            {
                _ = RefreshAccessAsync();
            }
        }

        public static void LogoutAndPropagateTokens()
        {
            NsdkSettingsHelper.ActiveSettings?.UpdateAccess(string.Empty, 0, string.Empty, 0);
            try
            {
                AuthRuntimeSettingsStore.Instance.Clear();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Auth] Failed to clear runtime settings store: {e.Message}");
            }
            AuthClient.StaticLogout();
        }

        private static readonly IAuthRuntimeSettingsUpdater s_settingsUpdater = AuthRuntimeSettingsUpdater.Instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void SetupRuntimeRefresh()
        {
            // Start the refresh task as soon as settings are initialized
            // (if the access token is expired, we want to refresh immediately)
            if (NsdkSettingsHelper.ActiveSettings != null)
            {
                StartRuntimeRefresh();
            }
            else
            {
                NsdkSettingsHelper.OnRuntimeSettingsCreated += StartRuntimeRefresh;
            }
        }

        private static void StartRuntimeRefresh()
        {
            // Remove the event handler if we're subscribed
            NsdkSettingsHelper.OnRuntimeSettingsCreated -= StartRuntimeRefresh;

            // Start the refresh task.
            _ = RefreshAccessAsync();
        }

        private static async Task RefreshAccessAsync()
        {
            // Don't start the refresh loop if an access token override is being used (no refresh token to rotate).
            // Check this before loading persisted settings, which could overwrite the token.
            if (NsdkSettingsHelper.ActiveSettings.UsingAccessTokenOverride)
            {
#if NIANTICSPATIAL_NSDK_AUTH_DEBUG
                Debug.Log("[Auth] Refresh loop not started — using access token override.");
#endif
                return;
            }

            // Grab a copy of the application's cancellation token, so we can cancel the task if on exit
            // (the token is replaced on exit, so we need the current one)
            s_settingsUpdatedCts = new CancellationTokenSource();
            var cancellationToken = CancellationTokenSource.CreateLinkedTokenSource(
                s_settingsUpdatedCts.Token, Application.exitCancellationToken).Token;
            
            // Load the current runtime settings from disk, if available
            // (otherwise we use the default settings)
            await AuthRuntimeSettingsStore.Instance.LoadAsync(NsdkSettingsHelper.ActiveSettings, cancellationToken);

            // Don't start the refresh loop if we don't have a refresh token.
            if (string.IsNullOrEmpty(NsdkSettingsHelper.ActiveSettings.RefreshToken))
            {
#if NIANTICSPATIAL_NSDK_AUTH_DEBUG
                Debug.Log("[Auth] Refresh loop not started as we don't have a refresh token.");
#endif
                return;
            }

#if NIANTICSPATIAL_NSDK_AUTH_DEBUG
            Debug.Log("[Auth] Refresh loop starting ...");
#endif

            // Loop that runs forever during runtime, periodically refreshing the access token
            // (if we have a valid refresh token)
            try
            {
                while (true)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    // the active runtime settings may change during the lifecycle of the app
                    // update the latest active settings instance
                    await s_settingsUpdater.RefreshRuntimeAccessIfExpiringAsync(
                        NsdkSettingsHelper.ActiveSettings, DateTime.UtcNow);
                    await Task.Delay(TimeSpan.FromSeconds(UpdateInterval), cancellationToken);
                }
            }
            catch (OperationCanceledException)
            {
                // Exiting the application, so we don't need to do anything here.
            }
            finally
            {
#if NIANTICSPATIAL_NSDK_AUTH_DEBUG
                Debug.Log("[Auth] Refresh loop stopped.");
#endif
            }
        }
    }
}
