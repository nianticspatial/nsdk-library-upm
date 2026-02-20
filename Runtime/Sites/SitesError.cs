// Copyright Niantic Spatial.

using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.Sites
{
    /// <summary>
    /// Error codes for Sites API operations.
    /// </summary>
    [PublicAPI]
    public enum SitesError
    {
        /// <summary>
        /// No error occurred.
        /// </summary>
        None = 0,

        /// <summary>
        /// A network error occurred during the request.
        /// </summary>
        NetworkError = 1,

        /// <summary>
        /// The request was invalid.
        /// </summary>
        InvalidRequest = 2,

        /// <summary>
        /// HTTP 403 Forbidden - authentication failed or insufficient permissions.
        /// </summary>
        HttpForbidden = 3,

        /// <summary>
        /// HTTP 404 Not Found - the requested resource was not found.
        /// </summary>
        HttpNotFound = 4,

        /// <summary>
        /// HTTP 429 Too Many Requests - rate limit exceeded.
        /// </summary>
        HttpTooManyRequests = 5,

        /// <summary>
        /// HTTP 5xx Server Error - server-side error occurred.
        /// </summary>
        HttpServerError = 6,

        /// <summary>
        /// Failed to parse the response.
        /// </summary>
        ParseError = 7,

        /// <summary>
        /// An unexpected error occurred.
        /// </summary>
        UnexpectedError = 8
    }
}
