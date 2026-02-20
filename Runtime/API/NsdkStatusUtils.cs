// Copyright Niantic Spatial.

using System;

namespace NianticSpatial.NSDK.AR.API
{
    internal static class NsdkStatusUtils
    {
        public static void ThrowExceptionIfNeeded(this NsdkStatus nsdkStatus)
        {
            switch (nsdkStatus)
            {
                case NsdkStatus.Ok:
                    return;

                case NsdkStatus.NullArgument:
                    throw new ArgumentNullException();

                case NsdkStatus.InvalidArgument:
                    throw new ArgumentNullException();

                case NsdkStatus.InvalidOperation:
                    throw new InvalidOperationException();

                default:
                    // Should not occur because Unity setup does not allow them to:
                    // - NullArdkHandle
                    // - FeatureDoesNotExist
                    // - FeatureAlreadyExists

                    // Should not occur because deprecated: NoData, InternalError
                    throw new ArgumentOutOfRangeException
                    (
                        "nsdkStatus",
                        $"NSDK Status `{nsdkStatus}` returned from native NSDK when not expected"
                    );
            }
        }

        public static bool IsOk(this NsdkStatus nsdkStatus)
        {
            return nsdkStatus == NsdkStatus.Ok;
        }
    }
}
