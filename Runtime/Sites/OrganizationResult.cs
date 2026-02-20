// Copyright Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.Sites
{
    /// <summary>
    /// Result of an organization information request.
    /// </summary>
    [PublicAPI]
    public readonly struct OrganizationResult
    {
        /// <summary>
        /// The status of the request.
        /// </summary>
        public SitesRequestStatus Status { get; }

        /// <summary>
        /// The error code if the request failed.
        /// </summary>
        public SitesError Error { get; }

        /// <summary>
        /// The organizations returned by the request.
        /// Empty array if the request failed or is in progress.
        /// </summary>
        public OrganizationInfo[] Organizations { get; }

        internal OrganizationResult(SitesRequestStatus status, SitesError error, OrganizationInfo[] organizations)
        {
            Status = status;
            Error = error;
            Organizations = organizations ?? Array.Empty<OrganizationInfo>();
        }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        internal static OrganizationResult Success(OrganizationInfo[] organizations) =>
            new OrganizationResult(SitesRequestStatus.Success, SitesError.None, organizations);

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        internal static OrganizationResult Failure(SitesError error) =>
            new OrganizationResult(SitesRequestStatus.Failed, error, Array.Empty<OrganizationInfo>());

        /// <summary>
        /// Creates an in-progress result.
        /// </summary>
        internal static OrganizationResult InProgress() =>
            new OrganizationResult(SitesRequestStatus.InProgress, SitesError.None, Array.Empty<OrganizationInfo>());
    }
}
