// Copyright 2026 Niantic Spatial.

using UnityEngine;

namespace NianticSpatial.NSDK.AR.Subsystems
{
    /// <summary>
    /// Carries the georeference of the asset a downloaded mesh chunk belongs to: the geodetic
    /// origin the chunk transforms are expressed relative to, plus the transform into that
    /// origin's frame. Attached to the chunk GameObject returned by
    /// <see cref="LocationMeshManager.GetLocationMeshChunksByVpsMapAssetIdAsync"/>.
    /// </summary>
    /// <remarks>
    /// Identical on every chunk of one download, because it describes the space rather than any
    /// single node. A chunk's own geographic position is obtained by composing this with the chunk's
    /// transform within the asset's origin space.
    ///
    /// Both the origin and the transform are required: depending on the asset, the altitude offset
    /// may live entirely in ReferenceAltitude (with TrackingToReference carrying no vertical
    /// translation) or entirely in TrackingToReference (with ReferenceAltitude zero). Applying the
    /// origin without the transform is wrong in the general case.
    /// </remarks>
    public class MeshGeoReference : MonoBehaviour
    {
        /// <summary>Whether the properties below are valid. Inspect this first.</summary>
        public MeshGeoReferenceMode Mode { get; private set; }

        /// <summary>
        /// WGS84 latitude of the origin TrackingToReference maps into, in decimal degrees.
        /// </summary>
        public double ReferenceLatitude { get; private set; }

        /// <summary>WGS84 longitude of that origin, in decimal degrees.</summary>
        public double ReferenceLongitude { get; private set; }

        /// <summary>Altitude of that origin, in meters above the WGS84 ellipsoid.</summary>
        public double ReferenceAltitude { get; private set; }

        /// <summary>
        /// Column-major 4x4 matrix (16 elements) mapping the asset's origin space (the space the
        /// chunk transforms are expressed in, not the device's AR session space) into meters relative
        /// to the reference origin, exactly as delivered by Portal — this implementation does not
        /// transpose it. Both sides use the native NSDK convention (x right, y down, z forward in;
        /// East-Down-North out), not Unity axes, so +Y is downward.
        /// </summary>
        public float[] TrackingToReference { get; private set; }

        internal void Initialize(MeshDownloadClient.NsdkMeshGeoReference native)
        {
            Mode = native.mode;
            ReferenceLatitude = native.reference_latitude;
            ReferenceLongitude = native.reference_longitude;
            ReferenceAltitude = native.reference_altitude;
            TrackingToReference = native.tracking_to_reference ?? new float[16];
        }
    }
}
