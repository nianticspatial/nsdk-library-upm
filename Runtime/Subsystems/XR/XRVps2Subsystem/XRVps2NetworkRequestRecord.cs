// Copyright Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.XRSubsystems
{
    [PublicAPI]
    public enum Vps2NetworkRequestType
    {
        Unknown,
        VpsMapLocalize,
        GetGraph,
        GetReplacedNodes,
        RegisteredNodes,
        UniversalLocalize
    }

    /// <summary>
    /// Diagnostic information about a VPS2 network request.
    /// </summary>
    [PublicAPI]
    public struct XRVps2NetworkRequestRecord
    {
        /// <summary>
        /// Unique request identifier
        /// </summary>
        public Guid RequestId;

        /// <summary>
        /// Type of request sent
        /// </summary>
        public Vps2NetworkRequestType RequestType;

        /// <summary>
        /// Request status
        /// </summary>
        public NetworkRequestStatus Status;

        /// <summary>
        /// Error code, if any
        /// </summary>
        public NetworkError ErrorCode;

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
    }
}
