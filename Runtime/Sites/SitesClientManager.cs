// Copyright Niantic Spatial.

using System;
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
    /// var userResult = await sitesClientManager.GetSelfUserInfoAsync();
    /// if (userResult.Status == SitesRequestStatus.Success) {
    ///     Debug.Log($"User: {userResult.User?.FirstName} {userResult.User?.LastName}");
    ///
    ///     var orgsResult = await sitesClientManager.GetOrganizationsForUserAsync(userResult.User.Value.Id);
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
        /// <returns>The user result.</returns>
        public Task<UserResult> GetSelfUserInfoAsync()
        {
            if (_sitesClient == null)
            {
                Log.Error("SitesClient is not initialized");
                return Task.FromResult(UserResult.Failure(SitesError.UnexpectedError));
            }

            return _sitesClient.RequestSelfUserInfoAsync(_requestTimeoutSeconds * 1000);
        }

        /// <summary>
        /// Gets user information by user ID.
        /// </summary>
        /// <param name="userId">The user ID to query.</param>
        /// <returns>The user result.</returns>
        public Task<UserResult> GetUserInfoAsync(string userId)
        {
            if (_sitesClient == null)
            {
                Log.Error("SitesClient is not initialized");
                return Task.FromResult(UserResult.Failure(SitesError.UnexpectedError));
            }

            return _sitesClient.RequestUserInfoAsync(userId, _requestTimeoutSeconds * 1000);
        }

        // ============================================================================
        // Organization API
        // ============================================================================

        /// <summary>
        /// Gets all organizations for a user.
        /// </summary>
        /// <param name="userId">The user ID to query organizations for.</param>
        /// <returns>The organization result.</returns>
        public Task<OrganizationResult> GetOrganizationsForUserAsync(string userId)
        {
            if (_sitesClient == null)
            {
                Log.Error("SitesClient is not initialized");
                return Task.FromResult(OrganizationResult.Failure(SitesError.UnexpectedError));
            }

            return _sitesClient.RequestOrganizationsForUserAsync(userId, _requestTimeoutSeconds * 1000);
        }

        /// <summary>
        /// Gets organization information by organization ID.
        /// </summary>
        /// <param name="orgId">The organization ID to query.</param>
        /// <returns>The organization result.</returns>
        public Task<OrganizationResult> GetOrganizationInfoAsync(string orgId)
        {
            if (_sitesClient == null)
            {
                Log.Error("SitesClient is not initialized");
                return Task.FromResult(OrganizationResult.Failure(SitesError.UnexpectedError));
            }

            return _sitesClient.RequestOrganizationInfoAsync(orgId, _requestTimeoutSeconds * 1000);
        }

        // ============================================================================
        // Site API
        // ============================================================================

        /// <summary>
        /// Gets all sites for an organization.
        /// </summary>
        /// <param name="orgId">The organization ID to query sites for.</param>
        /// <returns>The site result.</returns>
        public Task<SiteResult> GetSitesForOrganizationAsync(string orgId)
        {
            if (_sitesClient == null)
            {
                Log.Error("SitesClient is not initialized");
                return Task.FromResult(SiteResult.Failure(SitesError.UnexpectedError));
            }

            return _sitesClient.RequestSitesForOrganizationAsync(orgId, _requestTimeoutSeconds * 1000);
        }

        /// <summary>
        /// Gets site information by site ID.
        /// </summary>
        /// <param name="siteId">The site ID to query.</param>
        /// <returns>The site result.</returns>
        public Task<SiteResult> GetSiteInfoAsync(string siteId)
        {
            if (_sitesClient == null)
            {
                Log.Error("SitesClient is not initialized");
                return Task.FromResult(SiteResult.Failure(SitesError.UnexpectedError));
            }

            return _sitesClient.RequestSiteInfoAsync(siteId, _requestTimeoutSeconds * 1000);
        }

        // ============================================================================
        // Asset API
        // ============================================================================

        /// <summary>
        /// Gets all assets for a site.
        /// </summary>
        /// <param name="siteId">The site ID to query assets for.</param>
        /// <returns>The asset result.</returns>
        public Task<AssetResult> GetAssetsForSiteAsync(string siteId)
        {
            if (_sitesClient == null)
            {
                Log.Error("SitesClient is not initialized");
                return Task.FromResult(AssetResult.Failure(SitesError.UnexpectedError));
            }

            return _sitesClient.RequestAssetsForSiteAsync(siteId, _requestTimeoutSeconds * 1000);
        }

        /// <summary>
        /// Gets asset information by asset ID.
        /// </summary>
        /// <param name="assetId">The asset ID to query.</param>
        /// <returns>The asset result.</returns>
        public Task<AssetResult> GetAssetInfoAsync(string assetId)
        {
            if (_sitesClient == null)
            {
                Log.Error("SitesClient is not initialized");
                return Task.FromResult(AssetResult.Failure(SitesError.UnexpectedError));
            }

            return _sitesClient.RequestAssetInfoAsync(assetId, _requestTimeoutSeconds * 1000);
        }
    }
}
