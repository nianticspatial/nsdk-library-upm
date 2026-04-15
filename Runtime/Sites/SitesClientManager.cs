// Copyright Niantic Spatial.

using System;
using System.Threading;
using System.Threading.Tasks;
using NianticSpatial.NSDK.AR.Utilities;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using UnityEngine;

namespace NianticSpatial.NSDK.AR.Sites
{
    /// <summary>
    /// A MonoBehaviour component that provides access to the Sites API.
    /// Use this to query organizational hierarchy data including users, organizations, sites, and assets.
    /// </summary>
    /// <remarks>
    /// The SitesClientManager handles the lifecycle of the underlying SitesClient automatically.
    /// It creates the client on Awake and disposes it on OnDestroy.
    /// </remarks>
    /// <example>
    /// <code>
    /// var userResult = await sitesClientManager.GetSelfUserInfoAsync(this.destroyCancellationToken);
    /// if (userResult.Status == SitesRequestStatus.Success) {
    ///     Debug.Log($"User: {userResult.User?.FirstName} {userResult.User?.LastName}");
    ///
    ///     var orgsResult = await sitesClientManager.GetOrganizationsForUserAsync(
    ///         userResult.User.Value.Id, this.destroyCancellationToken);
    ///     foreach (var org in orgsResult.Organizations) {
    ///         Debug.Log($"Organization: {org.Name}");
    ///     }
    /// }
    /// </code>
    /// </example>
    [PublicAPI("apiref/Niantic/Lightship/AR/Sites/SitesClientManager/")]
    public class SitesClientManager : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("Timeout in seconds for API requests")]
        [Range(5, 120)]
        private int _requestTimeoutSeconds = 60;

        private SitesClient _sitesClient;

        /// <summary>
        /// The timeout for API requests in seconds.
        /// </summary>
        public int RequestTimeoutSeconds
        {
            get => _requestTimeoutSeconds;
            set => _requestTimeoutSeconds = Mathf.Clamp(value, 5, 120);
        }

        /// <summary>
        /// The underlying SitesClient instance.
        /// </summary>
        public SitesClient Client => _sitesClient;

        private void Awake()
        {
            try
            {
                _sitesClient = new SitesClient();
            }
            catch (Exception e)
            {
                Log.Error($"Failed to create SitesClient: {e.Message}");
            }
        }

        private void OnDestroy()
        {
            _sitesClient?.Dispose();
            _sitesClient = null;
        }

        // ============================================================================
        // User API
        // ============================================================================

        /// <summary>
        /// Gets information for the currently authenticated user.
        /// </summary>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The user result.</returns>
        public Task<UserResult> GetSelfUserInfoAsync(CancellationToken cancellationToken = default)
        {
            if (_sitesClient == null)
            {
                Log.Error("SitesClient is not initialized");
                return Task.FromResult(UserResult.Failure(SitesError.UnexpectedError));
            }

            return _sitesClient.RequestSelfUserInfoAsync(_requestTimeoutSeconds * 1000, cancellationToken);
        }

        /// <summary>
        /// Gets user information by user ID.
        /// </summary>
        /// <param name="userId">The user ID to query.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The user result.</returns>
        public Task<UserResult> GetUserInfoAsync(string userId, CancellationToken cancellationToken = default)
        {
            if (_sitesClient == null)
            {
                Log.Error("SitesClient is not initialized");
                return Task.FromResult(UserResult.Failure(SitesError.UnexpectedError));
            }

            return _sitesClient.RequestUserInfoAsync(userId, _requestTimeoutSeconds * 1000, cancellationToken);
        }

        // ============================================================================
        // Organization API
        // ============================================================================

        /// <summary>
        /// Gets all organizations for a user.
        /// </summary>
        /// <param name="userId">The user ID to query organizations for.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The organization result.</returns>
        public Task<OrganizationResult> GetOrganizationsForUserAsync(
            string userId, CancellationToken cancellationToken = default)
        {
            if (_sitesClient == null)
            {
                Log.Error("SitesClient is not initialized");
                return Task.FromResult(OrganizationResult.Failure(SitesError.UnexpectedError));
            }

            return _sitesClient.RequestOrganizationsForUserAsync(
                userId, _requestTimeoutSeconds * 1000, cancellationToken);
        }

        /// <summary>
        /// Gets organization information by organization ID.
        /// </summary>
        /// <param name="orgId">The organization ID to query.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The organization result.</returns>
        public Task<OrganizationResult> GetOrganizationInfoAsync(
            string orgId, CancellationToken cancellationToken = default)
        {
            if (_sitesClient == null)
            {
                Log.Error("SitesClient is not initialized");
                return Task.FromResult(OrganizationResult.Failure(SitesError.UnexpectedError));
            }

            return _sitesClient.RequestOrganizationInfoAsync(
                orgId, _requestTimeoutSeconds * 1000, cancellationToken);
        }

        // ============================================================================
        // Site API
        // ============================================================================

        /// <summary>
        /// Gets all sites for an organization.
        /// </summary>
        /// <param name="orgId">The organization ID to query sites for.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The site result.</returns>
        public Task<SiteResult> GetSitesForOrganizationAsync(
            string orgId, CancellationToken cancellationToken = default)
        {
            if (_sitesClient == null)
            {
                Log.Error("SitesClient is not initialized");
                return Task.FromResult(SiteResult.Failure(SitesError.UnexpectedError));
            }

            return _sitesClient.RequestSitesForOrganizationAsync(
                orgId, _requestTimeoutSeconds * 1000, cancellationToken);
        }

        /// <summary>
        /// Gets site information by site ID.
        /// </summary>
        /// <param name="siteId">The site ID to query.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The site result.</returns>
        public Task<SiteResult> GetSiteInfoAsync(string siteId, CancellationToken cancellationToken = default)
        {
            if (_sitesClient == null)
            {
                Log.Error("SitesClient is not initialized");
                return Task.FromResult(SiteResult.Failure(SitesError.UnexpectedError));
            }

            return _sitesClient.RequestSiteInfoAsync(siteId, _requestTimeoutSeconds * 1000, cancellationToken);
        }

        // ============================================================================
        // Asset API
        // ============================================================================

        /// <summary>
        /// Gets all assets for a site.
        /// </summary>
        /// <param name="siteId">The site ID to query assets for.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The asset result.</returns>
        public Task<AssetResult> GetAssetsForSiteAsync(
            string siteId, CancellationToken cancellationToken = default)
        {
            if (_sitesClient == null)
            {
                Log.Error("SitesClient is not initialized");
                return Task.FromResult(AssetResult.Failure(SitesError.UnexpectedError));
            }

            return _sitesClient.RequestAssetsForSiteAsync(
                siteId, _requestTimeoutSeconds * 1000, cancellationToken);
        }

        /// <summary>
        /// Gets asset information by asset ID.
        /// </summary>
        /// <param name="assetId">The asset ID to query.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The asset result.</returns>
        public Task<AssetResult> GetAssetInfoAsync(string assetId, CancellationToken cancellationToken = default)
        {
            if (_sitesClient == null)
            {
                Log.Error("SitesClient is not initialized");
                return Task.FromResult(AssetResult.Failure(SitesError.UnexpectedError));
            }

            return _sitesClient.RequestAssetInfoAsync(assetId, _requestTimeoutSeconds * 1000, cancellationToken);
        }

        // ============================================================================
        // Location API
        // ============================================================================

        /// <summary>
        /// Gets sites and their assets near a GPS coordinate.
        /// </summary>
        /// <param name="latitude">Latitude of the query coordinate, in degrees.</param>
        /// <param name="longitude">Longitude of the query coordinate, in degrees.</param>
        /// <param name="radiusMeters">Search radius around the coordinate, in meters.</param>
        /// <param name="assetType">The type of assets to filter results by.</param>
        /// <param name="cancellationToken">Token to cancel the operation.</param>
        /// <returns>The site-assets result ordered by distance.</returns>
        public Task<SiteAssetsResult> GetSiteAssetsByLocationAsync(
            double latitude,
            double longitude,
            double radiusMeters,
            AssetType assetType,
            CancellationToken cancellationToken = default)
        {
            if (_sitesClient == null)
            {
                Log.Error("SitesClient is not initialized");
                return Task.FromResult(SiteAssetsResult.Failure(SitesError.UnexpectedError));
            }

            return _sitesClient.RequestSiteAssetsByLocationAsync(
                latitude, longitude, radiusMeters, assetType, _requestTimeoutSeconds * 1000, cancellationToken);
        }
    }
}
