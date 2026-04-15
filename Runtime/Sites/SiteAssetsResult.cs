// Copyright Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.Sites
{
    /// <summary>
    /// Result of a site-assets location query.
    /// </summary>
    [PublicAPI]
    public readonly struct SiteAssetsResult
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
        /// The site-assets entries ordered by distance from the query coordinate.
        /// Empty array if the request failed or is in progress.
        /// </summary>
        public SiteAssetsInfo[] Entries { get; }

        internal SiteAssetsResult(SitesRequestStatus status, SitesError error, SiteAssetsInfo[] entries)
        {
            Status = status;
            Error = error;
            Entries = entries ?? Array.Empty<SiteAssetsInfo>();
        }

        internal static SiteAssetsResult Success(SiteAssetsInfo[] entries) =>
            new SiteAssetsResult(SitesRequestStatus.Success, SitesError.None, entries);

        internal static SiteAssetsResult Failure(SitesError error) =>
            new SiteAssetsResult(SitesRequestStatus.Failed, error, Array.Empty<SiteAssetsInfo>());
    }
}
