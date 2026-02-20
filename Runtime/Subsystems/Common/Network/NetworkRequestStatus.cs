// Copyright Niantic Spatial.

using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR
{
    /// <summary>
    /// Reports the status of a localization network request (single client -> server request).
    /// </summary>
    [PublicAPI]
    public enum NetworkRequestStatus : byte
    {
        // Defined in ardk_network_request_status.h
        Unknown = 0,
        Pending,
        Successful,
        Failed,
    }
}
