// Copyright Niantic Spatial.

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NianticSpatial.NSDK.AR.API;
using NianticSpatial.NSDK.AR.Core;
using NianticSpatial.NSDK.AR.Sites.Api;
using NianticSpatial.NSDK.AR.Utilities;
using NianticSpatial.NSDK.AR.Utilities.Logging;

namespace NianticSpatial.NSDK.AR.Sites
{
    /// <summary>
    /// Client for interacting with the Sites Manager service.
    /// Provides methods to query organizational hierarchy data including users,
    /// organizations, sites, and assets.
    /// </summary>
    [PublicAPI]
    public class SitesClient : IDisposable
    {
        private const int DefaultPollingIntervalMs = 100;
        private const int DefaultTimeoutMs = 60000;

        private IntPtr _nsdkHandle;
        private bool _isCreated;
        private bool _isDisposed;

        /// <summary>
        /// Creates a new SitesClient.
        /// </summary>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the NSDK context is not initialized.
        /// </exception>
        public SitesClient()
        {
            _nsdkHandle = NsdkUnityContext.GetNSDKHandle(NsdkUnityContext.UnityContextHandle);
            if (_nsdkHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("NSDK context is not initialized. Cannot create SitesClient.");
            }

            var status = NativeSitesApi.ARDK_SitesManager_Create(_nsdkHandle);
            if (status != NsdkStatus.Ok && status != NsdkStatus.FeatureAlreadyExists)
            {
                throw new InvalidOperationException($"Failed to create Sites Manager. Status: {status}");
            }

            _isCreated = true;
        }

        /// <summary>
        /// Releases native resources.
        /// </summary>
        /// <remarks>
        /// The Unity SDK does not explicitly destroy the SitesManager component because
        /// we can't easily guarantee the component will be destroyed before NsdkUnityContext is.
        /// The native NSDK holds a shared pointer to its components, so when it is destroyed,
        /// all its components will be released too.
        /// </remarks>
        public void Dispose()
        {
            if (_isDisposed)
            {
                return;
            }
            _isDisposed = true;

            // Note: We intentionally do NOT call ARDK_SitesManager_Destroy here.
            // The native NSDK manages component lifecycle - when the NSDK handle is destroyed,
            // all its components (including SitesManager) are automatically released.
            // Calling Destroy explicitly can crash if NsdkUnityContext is already shut down.

            _nsdkHandle = IntPtr.Zero;
            _isCreated = false;
        }

        // ============================================================================
        // Public async API
        // ============================================================================

        /// <summary>
        /// Requests information for the currently authenticated user.
        /// </summary>
        /// <param name="timeoutMs">Maximum time to wait for the request to complete.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The user result.</returns>
        public async Task<UserResult> RequestSelfUserInfoAsync(
            int timeoutMs = DefaultTimeoutMs,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            var status = NativeSitesApi.RequestSelfUserInfo(_nsdkHandle, out var requestId);
            if (status != NsdkStatus.Ok)
            {
                Log.Error($"Failed to request self user info. Status: {status}");
                return UserResult.Failure(SitesError.UnexpectedError);
            }

            return await PollForResultAsync(
                requestId, GetUserResult, UserResult.Failure, timeoutMs, cancellationToken);
        }

        /// <summary>
        /// Requests user information by user ID.
        /// </summary>
        public async Task<UserResult> RequestUserInfoAsync(
            string userId,
            int timeoutMs = DefaultTimeoutMs,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            var status = NativeSitesApi.RequestUserInfo(_nsdkHandle, userId, out var requestId);
            if (status != NsdkStatus.Ok)
            {
                Log.Error($"Failed to request user info. Status: {status}");
                return UserResult.Failure(SitesError.UnexpectedError);
            }

            return await PollForResultAsync(
                requestId, GetUserResult, UserResult.Failure, timeoutMs, cancellationToken);
        }

        /// <summary>
        /// Requests all organizations for a user.
        /// </summary>
        public async Task<OrganizationResult> RequestOrganizationsForUserAsync(
            string userId,
            int timeoutMs = DefaultTimeoutMs,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            var status = NativeSitesApi.RequestOrganizationsForUser(_nsdkHandle, userId, out var requestId);
            if (status != NsdkStatus.Ok)
            {
                Log.Error($"Failed to request organizations for user. Status: {status}");
                return OrganizationResult.Failure(SitesError.UnexpectedError);
            }

            return await PollForResultAsync(
                requestId, GetOrganizationResult, OrganizationResult.Failure, timeoutMs, cancellationToken);
        }

        /// <summary>
        /// Requests organization information by organization ID.
        /// </summary>
        public async Task<OrganizationResult> RequestOrganizationInfoAsync(
            string orgId,
            int timeoutMs = DefaultTimeoutMs,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            var status = NativeSitesApi.RequestOrganizationInfo(_nsdkHandle, orgId, out var requestId);
            if (status != NsdkStatus.Ok)
            {
                Log.Error($"Failed to request organization info. Status: {status}");
                return OrganizationResult.Failure(SitesError.UnexpectedError);
            }

            return await PollForResultAsync(
                requestId, GetOrganizationResult, OrganizationResult.Failure, timeoutMs, cancellationToken);
        }

        /// <summary>
        /// Requests all sites for an organization.
        /// </summary>
        public async Task<SiteResult> RequestSitesForOrganizationAsync(
            string orgId,
            int timeoutMs = DefaultTimeoutMs,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            var status = NativeSitesApi.RequestSitesForOrganization(_nsdkHandle, orgId, out var requestId);
            if (status != NsdkStatus.Ok)
            {
                Log.Error($"Failed to request sites for organization. Status: {status}");
                return SiteResult.Failure(SitesError.UnexpectedError);
            }

            return await PollForResultAsync(
                requestId, GetSiteResult, SiteResult.Failure, timeoutMs, cancellationToken);
        }

        /// <summary>
        /// Requests site information by site ID.
        /// </summary>
        public async Task<SiteResult> RequestSiteInfoAsync(
            string siteId,
            int timeoutMs = DefaultTimeoutMs,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            var status = NativeSitesApi.RequestSiteInfo(_nsdkHandle, siteId, out var requestId);
            if (status != NsdkStatus.Ok)
            {
                Log.Error($"Failed to request site info. Status: {status}");
                return SiteResult.Failure(SitesError.UnexpectedError);
            }

            return await PollForResultAsync(
                requestId, GetSiteResult, SiteResult.Failure, timeoutMs, cancellationToken);
        }

        /// <summary>
        /// Requests all assets for a site.
        /// </summary>
        public async Task<AssetResult> RequestAssetsForSiteAsync(
            string siteId,
            int timeoutMs = DefaultTimeoutMs,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            var status = NativeSitesApi.RequestAssetsForSite(_nsdkHandle, siteId, out var requestId);
            if (status != NsdkStatus.Ok)
            {
                Log.Error($"Failed to request assets for site. Status: {status}");
                return AssetResult.Failure(SitesError.UnexpectedError);
            }

            return await PollForResultAsync(
                requestId, GetAssetResult, AssetResult.Failure, timeoutMs, cancellationToken);
        }

        /// <summary>
        /// Requests asset information by asset ID.
        /// </summary>
        public async Task<AssetResult> RequestAssetInfoAsync(
            string assetId,
            int timeoutMs = DefaultTimeoutMs,
            CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            var status = NativeSitesApi.RequestAssetInfo(_nsdkHandle, assetId, out var requestId);
            if (status != NsdkStatus.Ok)
            {
                Log.Error($"Failed to request asset info. Status: {status}");
                return AssetResult.Failure(SitesError.UnexpectedError);
            }

            return await PollForResultAsync(
                requestId, GetAssetResult, AssetResult.Failure, timeoutMs, cancellationToken);
        }

        // ============================================================================
        // Polling helpers
        // ============================================================================

        /// <summary>
        /// Result state from a single poll attempt.
        /// </summary>
        private readonly struct PollState<TResult>
        {
            public SitesRequestStatus Status { get; }
            public SitesError Error { get; }
            public TResult Result { get; }

            private PollState(SitesRequestStatus status, SitesError error, TResult result)
            {
                Status = status;
                Error = error;
                Result = result;
            }

            public static PollState<TResult> InProgress() =>
                new PollState<TResult>(SitesRequestStatus.InProgress, SitesError.None, default);

            public static PollState<TResult> Success(TResult result) =>
                new PollState<TResult>(SitesRequestStatus.Success, SitesError.None, result);

            public static PollState<TResult> Failed(SitesError error) =>
                new PollState<TResult>(SitesRequestStatus.Failed, error, default);
        }

        /// <summary>
        /// Generic polling helper that waits for a request to complete.
        /// This mirrors the Swift pattern in NsdkSitesSession.pollForResult().
        /// </summary>
        /// <param name="requestId">The request ID to poll for.</param>
        /// <param name="getResult">Function to poll for the result state.</param>
        /// <param name="createFailure">Function to create a failure result.</param>
        /// <param name="timeoutMs">Maximum time to wait for completion.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The result when the request completes.</returns>
        private async Task<TResult> PollForResultAsync<TResult>(
            ulong requestId,
            Func<ulong, PollState<TResult>> getResult,
            Func<SitesError, TResult> createFailure,
            int timeoutMs,
            CancellationToken cancellationToken)
        {
            var startTime = DateTime.UtcNow;

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                if ((DateTime.UtcNow - startTime).TotalMilliseconds > timeoutMs)
                {
                    return createFailure(SitesError.UnexpectedError);
                }

                var state = getResult(requestId);

                switch (state.Status)
                {
                    case SitesRequestStatus.Success:
                        return state.Result;
                    case SitesRequestStatus.Failed:
                        return createFailure(state.Error);
                    case SitesRequestStatus.InProgress:
                        await Task.Delay(DefaultPollingIntervalMs, cancellationToken);
                        break;
                }
            }
        }

        // Individual result getters that poll native and convert to managed types

        private PollState<UserResult> GetUserResult(ulong requestId)
        {
            var status = NativeSitesApi.ARDK_SitesManager_GetUserResult(
                _nsdkHandle, requestId, out var nativeResult);

            if (status != NsdkStatus.Ok)
            {
                return PollState<UserResult>.Failed(SitesError.UnexpectedError);
            }

            var requestStatus = (SitesRequestStatus)nativeResult.status;

            if (requestStatus == SitesRequestStatus.Success)
            {
                // Marshal data BEFORE releasing the handle (handle owns the memory)
                UserInfo? user = null;
                if (nativeResult.user != IntPtr.Zero)
                {
                    var nativeUser = Marshal.PtrToStructure<NativeSitesApi.NativeUserInfo>(nativeResult.user);
                    user = NativeSitesApi.ConvertUser(nativeUser);
                }

                // Now safe to release
                if (nativeResult.handle != IntPtr.Zero)
                {
                    NsdkExternUtils.ReleaseResource(nativeResult.handle);
                }

                if (user.HasValue)
                {
                    return PollState<UserResult>.Success(UserResult.Success(user.Value));
                }

                // Success but no user data - treat as unexpected error
                return PollState<UserResult>.Failed(SitesError.UnexpectedError);
            }

            if (requestStatus == SitesRequestStatus.Failed)
            {
                if (nativeResult.handle != IntPtr.Zero)
                {
                    NsdkExternUtils.ReleaseResource(nativeResult.handle);
                }
                return PollState<UserResult>.Failed((SitesError)nativeResult.error);
            }

            return PollState<UserResult>.InProgress();
        }

        private PollState<OrganizationResult> GetOrganizationResult(ulong requestId)
        {
            var status = NativeSitesApi.ARDK_SitesManager_GetOrganizationResult(
                _nsdkHandle, requestId, out var nativeResult);

            if (status != NsdkStatus.Ok)
            {
                return PollState<OrganizationResult>.Failed(SitesError.UnexpectedError);
            }

            var requestStatus = (SitesRequestStatus)nativeResult.status;

            if (requestStatus == SitesRequestStatus.Success)
            {
                var organizations = NativeSitesApi.ConvertOrganizations(
                    nativeResult.organizations, nativeResult.organizations_size);

                if (nativeResult.handle != IntPtr.Zero)
                {
                    NsdkExternUtils.ReleaseResource(nativeResult.handle);
                }

                return PollState<OrganizationResult>.Success(OrganizationResult.Success(organizations));
            }

            if (requestStatus == SitesRequestStatus.Failed)
            {
                if (nativeResult.handle != IntPtr.Zero)
                {
                    NsdkExternUtils.ReleaseResource(nativeResult.handle);
                }
                return PollState<OrganizationResult>.Failed((SitesError)nativeResult.error);
            }

            return PollState<OrganizationResult>.InProgress();
        }

        private PollState<SiteResult> GetSiteResult(ulong requestId)
        {
            var status = NativeSitesApi.ARDK_SitesManager_GetSiteResult(
                _nsdkHandle, requestId, out var nativeResult);

            if (status != NsdkStatus.Ok)
            {
                return PollState<SiteResult>.Failed(SitesError.UnexpectedError);
            }

            var requestStatus = (SitesRequestStatus)nativeResult.status;

            if (requestStatus == SitesRequestStatus.Success)
            {
                var sites = NativeSitesApi.ConvertSites(nativeResult.sites, nativeResult.sites_size);

                if (nativeResult.handle != IntPtr.Zero)
                {
                    NsdkExternUtils.ReleaseResource(nativeResult.handle);
                }

                return PollState<SiteResult>.Success(SiteResult.Success(sites));
            }

            if (requestStatus == SitesRequestStatus.Failed)
            {
                if (nativeResult.handle != IntPtr.Zero)
                {
                    NsdkExternUtils.ReleaseResource(nativeResult.handle);
                }
                return PollState<SiteResult>.Failed((SitesError)nativeResult.error);
            }

            return PollState<SiteResult>.InProgress();
        }

        private PollState<AssetResult> GetAssetResult(ulong requestId)
        {
            var status = NativeSitesApi.ARDK_SitesManager_GetAssetResult(
                _nsdkHandle, requestId, out var nativeResult);

            if (status != NsdkStatus.Ok)
            {
                return PollState<AssetResult>.Failed(SitesError.UnexpectedError);
            }

            var requestStatus = (SitesRequestStatus)nativeResult.status;

            if (requestStatus == SitesRequestStatus.Success)
            {
                var assets = NativeSitesApi.ConvertAssets(nativeResult.assets, nativeResult.assets_size);

                if (nativeResult.handle != IntPtr.Zero)
                {
                    NsdkExternUtils.ReleaseResource(nativeResult.handle);
                }

                return PollState<AssetResult>.Success(AssetResult.Success(assets));
            }

            if (requestStatus == SitesRequestStatus.Failed)
            {
                if (nativeResult.handle != IntPtr.Zero)
                {
                    NsdkExternUtils.ReleaseResource(nativeResult.handle);
                }
                return PollState<AssetResult>.Failed((SitesError)nativeResult.error);
            }

            return PollState<AssetResult>.InProgress();
        }

        private void ThrowIfDisposed()
        {
            if (_isDisposed)
            {
                throw new ObjectDisposedException(nameof(SitesClient));
            }
        }
    }
}
