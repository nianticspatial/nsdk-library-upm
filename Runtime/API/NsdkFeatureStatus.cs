// Copyright Niantic Spatial.

using System;

namespace NianticSpatial.NSDK.AR.API
{
    /// <summary>
    /// Status of an NSDK feature. More than one flag can be set at once.
    /// </summary>
    /// <remarks>
    /// Values are assigned explicitly and must not be renumbered or reordered.
    /// </remarks>
    [Flags]
    internal enum NsdkFeatureStatus
    {
        None = 0,
        NullArdkHandle = 1 << 0,
        DoesNotExist = 1 << 1,
        ConfigurationFailed = 1 << 2,

        /// <summary>
        /// The authentication credentials used to initialize NSDK are invalid for this feature.
        /// </summary>
        [Obsolete("Read the localization request records instead. VPS localization and the " +
                  "portal are authorized separately, so a single flag cannot say which one " +
                  "rejected the app; each record names its request type and error.")]
        BadAuth = 1 << 3
    }
}
