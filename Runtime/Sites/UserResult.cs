// Copyright Niantic Spatial.

using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.Sites
{
    /// <summary>
    /// Result of a user information request.
    /// </summary>
    [PublicAPI]
    public readonly struct UserResult
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
        /// The user information if the request succeeded, or null if failed.
        /// </summary>
        public UserInfo? User { get; }

        internal UserResult(SitesRequestStatus status, SitesError error, UserInfo? user)
        {
            Status = status;
            Error = error;
            User = user;
        }

        /// <summary>
        /// Creates a successful result.
        /// </summary>
        internal static UserResult Success(UserInfo user) =>
            new UserResult(SitesRequestStatus.Success, SitesError.None, user);

        /// <summary>
        /// Creates a failed result.
        /// </summary>
        internal static UserResult Failure(SitesError error) =>
            new UserResult(SitesRequestStatus.Failed, error, null);

        /// <summary>
        /// Creates an in-progress result.
        /// </summary>
        internal static UserResult InProgress() =>
            new UserResult(SitesRequestStatus.InProgress, SitesError.None, null);
    }
}
