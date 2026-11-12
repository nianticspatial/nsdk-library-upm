// Copyright 2026 Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.Common;
using NianticSpatial.NSDK.AR.Utilities;
using NianticSpatial.NSDK.AR.XRSubsystems;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace NianticSpatial.NSDK.AR.VPS2
{
    /// <summary>
    /// Represents the origin of a VPS asset tracked by the device.
    /// </summary>
    /// <remarks>
    /// The trackable's transform follows the asset's origin as VPS refines the
    /// localization, so content authored in the asset's coordinate space is placed by
    /// parenting it under this trackable. While the asset is not fully tracked, the
    /// tracking state degrades to <c>Limited</c> and the transform keeps the last known
    /// pose.
    /// </remarks>
    [PublicAPI]
    [DefaultExecutionOrder(NsdkARUpdateOrder.Vps2Asset)]
    [DisallowMultipleComponent]
    public sealed class ARVps2Asset : ARTrackable<XRVps2Asset, ARVps2Asset>
    {
        /// <summary>
        /// The identifier of the site the asset belongs to, or null until the first
        /// tracking data arrives.
        /// </summary>
        public string SiteId => sessionRelativeData.siteGuid == Guid.Empty
            ? null
            : sessionRelativeData.siteGuid.ToString("D");

        /// <summary>
        /// The identifier of the VPS map asset.
        /// </summary>
        public string AssetId => Vps2AssetId.ToUuidString(sessionRelativeData.trackableId);

        /// <summary>
        /// Confidence in the current pose, from 0 (none) to 1 (full).
        /// </summary>
        public float Confidence => sessionRelativeData.trackingConfidence;

        /// <summary>
        /// Timestamp of the camera frame the pose was last updated from, in monotonic
        /// milliseconds.
        /// </summary>
        public UInt64 TimestampMs => sessionRelativeData.timestampMs;
    }
}
