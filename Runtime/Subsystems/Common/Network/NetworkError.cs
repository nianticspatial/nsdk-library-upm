// Copyright Niantic Spatial.

using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR
{
    /// <summary>
    /// Reports the error code of a network request, if any
    /// </summary>
    [PublicAPI]
    public enum NetworkError : uint
    {
        // Defined in ardk_network_error.h
        Unknown = 0,
        None,
        BadNetworkConnection,
        BadApiKey,
        PermissionDenied,
        RequestsLimitExceeded,
        InternalServer,
        InternalClient,
    }
}
