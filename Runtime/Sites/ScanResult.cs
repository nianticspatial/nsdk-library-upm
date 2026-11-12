// Copyright 2026 Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.Sites
{
    /// <summary>
    /// Result of a scan list or scan information request.
    /// </summary>
    [PublicAPI]
    public readonly struct ScanResult
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
        /// The site ID the scans were listed for, or null for a scan information request.
        /// </summary>
        public string SiteId { get; }

        /// <summary>
        /// The site's scans, or the single scan requested.
        /// Empty array if the request failed or is in progress.
        /// </summary>
        public ScanInfo[] Scans { get; }

        internal ScanResult(SitesRequestStatus status, SitesError error, string siteId, ScanInfo[] scans)
        {
            Status = status;
            Error = error;
            SiteId = siteId;
            Scans = scans ?? Array.Empty<ScanInfo>();
        }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        internal static ScanResult Success(string siteId, ScanInfo[] scans) =>
            new ScanResult(SitesRequestStatus.Success, SitesError.None, siteId, scans);

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        internal static ScanResult Failure(string siteId, SitesError error) =>
            new ScanResult(SitesRequestStatus.Failed, error, siteId, Array.Empty<ScanInfo>());
    }
}
