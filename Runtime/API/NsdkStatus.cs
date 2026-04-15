// Copyright Niantic Spatial.

namespace NianticSpatial.NSDK.AR.API
{
    // Defined in ardk_status.h
    internal enum NsdkStatus
    {
        Ok = 0,
        NullArgument,
        InvalidArgument,
        InvalidOperation,
        NullNsdkHandle,
        FeatureDoesNotExist,
        FeatureAlreadyExists,
    }
}
