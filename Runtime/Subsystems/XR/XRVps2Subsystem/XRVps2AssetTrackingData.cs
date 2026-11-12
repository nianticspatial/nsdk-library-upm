// Copyright Niantic Spatial.

using NianticSpatial.NSDK.AR.Utilities;
using UnityEngine;

namespace NianticSpatial.NSDK.AR.XRSubsystems
{
    /// <summary>
    /// The pose of a VPS asset's origin in the device's AR tracking space.
    /// </summary>
    /// <remarks>
    /// Place content authored in the asset's coordinate space with <see cref="Pose"/>. The asset's
    /// coordinate space is the same space downloaded mesh chunks of the asset are placed in.
    /// Only served while the asset is fully tracked; see
    /// <see cref="XRVps2Subsystem.TryGetAssetTrackingData"/>.
    /// </remarks>
    [PublicAPI]
    public struct XRVps2AssetTrackingData
    {
        /// <summary>
        /// The identifier of the site the asset belongs to.
        /// </summary>
        public string SiteId;

        /// <summary>
        /// The identifier of the VPS map asset.
        /// </summary>
        public string AssetId;

        /// <summary>
        /// The area of the asset whose anchor supplied this pose, or null when the asset has no
        /// subareas. Every area of a split asset shares the asset's origin, so the pose is the same
        /// whichever area supplies it. It can differ from <see cref="Vps2LocalizedAsset.AreaId"/>.
        /// </summary>
        public string AreaId;

        /// <summary>
        /// Pose of the asset's origin, in AR session space: not composed with the XR origin.
        /// Equivalently, the transform from the asset's coordinate space to AR session space, with position
        /// in meters. Both spaces are expressed in Unity axes. Independent of the current camera pose.
        /// </summary>
        public Pose Pose;

        /// <summary>
        /// Confidence in the pose, from 0 (none) to 1 (full).
        /// </summary>
        public float Confidence;

        /// <summary>
        /// Timestamp of the camera frame the pose was last updated from, in monotonic
        /// milliseconds.
        /// </summary>
        public ulong TimestampMs;

        /// <summary>
        /// Geolocation of the asset's origin, or null while the asset has no georeference.
        /// </summary>
        public XRGeolocation? Geolocation;
    }
}
