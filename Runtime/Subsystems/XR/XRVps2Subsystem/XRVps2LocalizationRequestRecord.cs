// Copyright Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.XRSubsystems
{
    [PublicAPI]
    public enum Vps2LocalizationRequestType: byte
    {
        Unknown,
        VpsMapLocalize,
        GetGraph,
        GetReplacedNodes,
        RegisteredNodes,
        UniversalLocalize
    }

    [PublicAPI]
    public enum Vps2LocalizationRequestStatus : byte
    {
        /// <summary>Status is unknown or uninitialized.</summary>
        Unknown = 0,
        /// <summary>The request has been sent and is awaiting a response.</summary>
        Pending,
        /// <summary>The request completed and a response was received.</summary>
        /// <remarks>The localization itself may still have failed — check the localization error for details.</remarks>
        Completed,
        /// <summary>The request failed (e.g. network error, HTTP error, or unparseable response).</summary>
        Failed,
        /// <summary>The request was not sent because the frame was rejected (e.g. camera pointed downward).</summary>
        FrameRejected,
    }

    /// <summary>
    /// Possible errors from VPS2 localization operations.
    /// </summary>
    [PublicAPI]
    public enum Vps2LocalizationError : byte
    {
        /// <summary>Error is unknown or uninitialized.</summary>
        Unknown = 0,

        /// <summary>No error has occurred.</summary>
        None,

        /// <summary>The device cannot connect to the server.</summary>
        BadNetworkConnection,

        /// <summary>The API gateway rejected the request due to an authentication or authorization
        /// failure (e.g. missing, invalid, or revoked credentials).</summary>
        AuthFailure,

        /// <summary>The server denied permission for the request.</summary>
        PermissionDenied,

        /// <summary>The request rate limit has been exceeded.</summary>
        RequestsLimitExceeded,

        /// <summary>The server could not process the request due to an internal error,
        /// not due to malformed input.</summary>
        InternalServer,

        /// <summary>The server sent a response that could not be parsed by the client.</summary>
        InternalClient,

        /// <summary>The server could not find a map near the request GPS location.</summary>
        NoMapFound,

        /// <summary>The server processed the request but localization failed.</summary>
        LocalizationFailed,

        /// <summary>The API gateway rejected the request because the account or project quota
        /// has been exceeded (HTTP 429).</summary>
        QuotaExceeded,

        /// <summary>The frame was rejected because tracking state is not normal.</summary>
        BadTracking,

        /// <summary>The frame was rejected because the camera is pointing at the ground or sky
        /// (no horizon crossing).</summary>
        BadCameraAngle,

        /// <summary>The frame was rejected before dispatch because no valid geolocation prior was
        /// available — the GPS reading was NaN or a placeholder and no other valid prior (e.g. a
        /// WPS estimate) could be substituted.</summary>
        NoValidPrior,
    }

    /// <summary>
    /// Diagnostic information about a VPS2 network request.
    /// </summary>
    [PublicAPI]
    public struct XRVps2LocalizationRequestRecord
    {
        /// <summary>
        /// Unique request identifier
        /// </summary>
        public Guid RequestId;

        /// <summary>
        /// Type of request sent
        /// </summary>
        public Vps2LocalizationRequestType RequestType;

        /// <summary>
        /// Request status
        /// </summary>
        public Vps2LocalizationRequestStatus Status;

        /// <summary>
        /// Error code, if any
        /// </summary>
        public Vps2LocalizationError ErrorCode;

        /// <summary>
        /// Time that the request was sent, in milliseconds. It is only comparable
        /// to EndTimeMs.
        /// </summary>
        public ulong StartTimeMs;

        /// <summary>
        /// Time that the response was received, in milliseconds. It is only comparable
        /// to StartTimeMs.
        /// </summary>
        public ulong EndTimeMs;

        /// <summary>
        /// Id of the frame containing data sent in the request, if available.
        /// </summary>
        public ulong FrameId;

        public override string ToString()
        {
            var durationMs = EndTimeMs >= StartTimeMs ? $"{EndTimeMs - StartTimeMs}ms" : "N/A";
            return $"[{RequestType}] {Status} (Id={RequestId}, Error={ErrorCode}, " +
                $"Duration={durationMs}, FrameId={FrameId})";
        }
    }
}
