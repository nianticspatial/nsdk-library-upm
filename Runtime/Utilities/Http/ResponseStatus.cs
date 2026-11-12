// Copyright 2022-2026 Niantic Spatial.

namespace NianticSpatial.NSDK.AR.Utilities.Http
{
    /// Status of a response from a NS server.
    internal enum ResponseStatus
    {
        // From UnityWebRequest.Result
        /// Could not reach the server
        ConnectionError,
        ProtocolError, // all 4xx and 5xx => see Gateway

        // From API Gateway
        /// No authorization credentials specified
        AuthMissing = 400,

        /// Authorization credentials are not valid
        Forbidden = 403,

        /// Too many requests in a short time triggered Rate Limiting
        TooManyRequests = 429,
        InternalGatewayError = 500,

        // From VPS Coverage backend API
        Unset,
        Success,
        InvalidRequest,
        InternalError,

        /// Over 100 localization targets requested in single request
        TooManyEntitiesRequested
    }
}
