// Copyright Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.Sites
{
    /// <summary>
    /// Result of a site information request.
    /// </summary>
    [PublicAPI]
    public readonly struct SiteResult
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
        /// The sites returned by the request.
        /// Empty array if the request failed or is in progress.
        /// </summary>
        public SiteInfo[] Sites { get; }

        internal SiteResult(SitesRequestStatus status, SitesError error, SiteInfo[] sites)
        {
            Status = status;
            Error = error;
            Sites = sites ?? Array.Empty<SiteInfo>();
        }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        internal static SiteResult Success(SiteInfo[] sites) =>
            new SiteResult(SitesRequestStatus.Success, SitesError.None, sites);

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        internal static SiteResult Failure(SitesError error) =>
            new SiteResult(SitesRequestStatus.Failed, error, Array.Empty<SiteInfo>());

        /// <summary>
        /// Creates an in-progress result.
        /// </summary>
        internal static SiteResult InProgress() =>
            new SiteResult(SitesRequestStatus.InProgress, SitesError.None, Array.Empty<SiteInfo>());
    }
}
