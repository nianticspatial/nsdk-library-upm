// Copyright Niantic Spatial.

using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.Sites
{
    /// <summary>
    /// Status of a Sites API request.
    /// </summary>
    [PublicAPI]
    public enum SitesRequestStatus
    {
        /// <summary>
        /// The request is still in progress.
        /// </summary>
        InProgress = 0,

        /// <summary>
        /// The request completed successfully.
        /// </summary>
        Success = 1,

        /// <summary>
        /// The request failed.
        /// </summary>
        Failed = 2
    }
}
