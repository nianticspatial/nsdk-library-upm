// Copyright Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.Sites
{
    /// <summary>
    /// Result of an asset information request.
    /// </summary>
    [PublicAPI]
    public readonly struct AssetResult
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
        /// The assets returned by the request.
        /// Empty array if the request failed or is in progress.
        /// </summary>
        public AssetInfo[] Assets { get; }

        internal AssetResult(SitesRequestStatus status, SitesError error, AssetInfo[] assets)
        {
            Status = status;
            Error = error;
            Assets = assets ?? Array.Empty<AssetInfo>();
        }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        internal static AssetResult Success(AssetInfo[] assets) =>
            new AssetResult(SitesRequestStatus.Success, SitesError.None, assets);

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        internal static AssetResult Failure(SitesError error) =>
            new AssetResult(SitesRequestStatus.Failed, error, Array.Empty<AssetInfo>());

        /// <summary>
        /// Creates an in-progress result.
        /// </summary>
        internal static AssetResult InProgress() =>
            new AssetResult(SitesRequestStatus.InProgress, SitesError.None, Array.Empty<AssetInfo>());
    }
}
