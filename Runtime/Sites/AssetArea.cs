// Copyright Niantic Spatial.

using System;
using System.Collections.Generic;
using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.Sites
{
    /// <summary>
    /// One independently-localizable piece of an asset.
    /// </summary>
    /// <remarks>
    /// A scan too large for a single mapping run is split, and each piece gets its own job,
    /// anchor, node graph and extent. An asset mapped in one pass has exactly one area.
    ///
    /// One shape serves every asset type. <see cref="AssetInfo.AssetType"/> selects which
    /// properties carry anything:
    /// <list type="bullet">
    ///   <item>Mesh: RootNodeId, NodeIds, MeshCoverage</item>
    ///   <item>Splat: RootNodeId</item>
    ///   <item>VpsInfo: AnchorPayload, and the bounds</item>
    /// </list>
    /// </remarks>
    [PublicAPI]
    public readonly struct AssetArea
    {
        /// <summary>
        /// This area's own identifier, unique within the asset.
        /// </summary>
        public string AreaId { get; }

        /// <summary>
        /// Status of this area's own mapping job.
        /// </summary>
        /// <remarks>
        /// A job belongs to an area, so this is the only status the portal reports;
        /// <see cref="AssetInfo.PipelineJobStatus"/> is deprecated and left unset.
        /// </remarks>
        public AssetPipelineJobStatus JobStatus { get; }

        /// <summary>
        /// The space this area was mapped into.
        /// </summary>
        public string SpaceId { get; }

        /// <summary>
        /// Root node ID of this area's space.
        /// </summary>
        public string RootNodeId { get; }

        /// <summary>
        /// All node IDs from this area's space.
        /// </summary>
        public IReadOnlyList<string> NodeIds { get; }

        /// <summary>
        /// This area's anchor payload, base64-encoded ManagedPoseData.
        /// </summary>
        /// <remarks>
        /// @note A legacy map identifier. An anchor payload is no longer the preferred method
        /// to identify the map to localize against: pass a site ID to
        /// <c>ARVps2Manager.TryLocalize(string)</c> or a coordinate to its lat/long
        /// overload.
        /// </remarks>
        public string AnchorPayload { get; }

        /// <summary>
        /// This area's mesh coverage in square meters.
        /// </summary>
        public double MeshCoverage { get; }

        /// <summary>
        /// Southern edge of this area's extent. Only meaningful when <see cref="HasBounds"/>.
        /// </summary>
        public double MinLat { get; }

        /// <summary>
        /// Western edge of this area's extent. Only meaningful when <see cref="HasBounds"/>.
        /// </summary>
        public double MinLng { get; }

        /// <summary>
        /// Northern edge of this area's extent. Only meaningful when <see cref="HasBounds"/>.
        /// </summary>
        public double MaxLat { get; }

        /// <summary>
        /// Eastern edge of this area's extent. Only meaningful when <see cref="HasBounds"/>.
        /// </summary>
        public double MaxLng { get; }

        /// <summary>
        /// False when the portal published no extent for this area, or one describing no usable
        /// area. The four bounds are then zero.
        /// </summary>
        public bool HasBounds { get; }

        internal AssetArea(
            string areaId,
            AssetPipelineJobStatus jobStatus,
            string spaceId,
            string rootNodeId,
            IReadOnlyList<string> nodeIds,
            string anchorPayload,
            double meshCoverage,
            double minLat,
            double minLng,
            double maxLat,
            double maxLng,
            bool hasBounds)
        {
            AreaId = areaId ?? string.Empty;
            JobStatus = jobStatus;
            SpaceId = spaceId ?? string.Empty;
            RootNodeId = rootNodeId ?? string.Empty;
            NodeIds = nodeIds ?? Array.Empty<string>();
            AnchorPayload = anchorPayload ?? string.Empty;
            MeshCoverage = meshCoverage;
            MinLat = minLat;
            MinLng = minLng;
            MaxLat = maxLat;
            MaxLng = maxLng;
            HasBounds = hasBounds;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var bounds = HasBounds
                ? $"({MinLat}, {MinLng})-({MaxLat}, {MaxLng})"
                : "none";
            return $"Area: AreaId={AreaId}, JobStatus={JobStatus}, SpaceId={SpaceId}, " +
                   $"RootNodeId={RootNodeId}, NodeIds({NodeIds.Count}), " +
                   $"MeshCoverage={MeshCoverage}m², Bounds={bounds}";
        }
    }
}
