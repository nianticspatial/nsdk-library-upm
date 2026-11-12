// Copyright Niantic Spatial.

using System;
using System.Collections.Generic;
using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.Sites
{
    /// <summary>
    /// Mesh-specific asset data.
    /// Maps to proto message AssetMeshData.
    /// </summary>
    [PublicAPI]
    public readonly struct AssetMeshData
    {
        /// <summary>
        /// Root node ID of the first valid space.
        /// </summary>
        public string RootNodeId { get; }

        /// <summary>
        /// All node IDs from the space.
        /// </summary>
        public IReadOnlyList<string> NodeIds { get; }

        /// <summary>
        /// Mesh coverage in square meters.
        /// </summary>
        public double MeshCoverage { get; }

        internal AssetMeshData(string rootNodeId, IReadOnlyList<string> nodeIds, double meshCoverage)
        {
            RootNodeId = rootNodeId ?? string.Empty;
            NodeIds = nodeIds ?? Array.Empty<string>();
            MeshCoverage = meshCoverage;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var nodePreview = NodeIds.Count > 3
                ? string.Join(", ", new[] { NodeIds[0], NodeIds[1], NodeIds[2] }) + "..."
                : string.Join(", ", NodeIds);
            return $"MeshData: RootNodeId={RootNodeId}, MeshCoverage={MeshCoverage}m², NodeIds({NodeIds.Count})=[{nodePreview}]";
        }
    }

    /// <summary>
    /// Splat-specific asset data.
    /// Maps to proto message AssetSplatData.
    /// </summary>
    [PublicAPI]
    public readonly struct AssetSplatData
    {
        /// <summary>
        /// Root node ID of the first valid space.
        /// </summary>
        public string RootNodeId { get; }

        internal AssetSplatData(string rootNodeId)
        {
            RootNodeId = rootNodeId ?? string.Empty;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"SplatData: RootNodeId={RootNodeId}";
        }
    }

    /// <summary>
    /// VPS-specific asset data.
    /// Maps to proto message AssetVpsData.
    /// </summary>
    [PublicAPI]
    public readonly struct AssetVpsData
    {
        /// <summary>
        /// Default anchor payload used by VPS Service (base64 encoded).
        /// </summary>
        /// <remarks>
        /// @note A legacy map identifier. An anchor payload is no longer the preferred method
        /// to identify the map to localize against: pass a site ID to
        /// <c>ARVps2Manager.TryLocalize(string)</c> or a coordinate to its lat/long
        /// overload.
        /// </remarks>
        public string AnchorPayload { get; }

        public AssetVpsData(string anchorPayload)
        {
            AnchorPayload = anchorPayload ?? string.Empty;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var truncated = AnchorPayload.Length > 50
                ? AnchorPayload.Substring(0, 50) + "..."
                : AnchorPayload;
            return $"VpsData: AnchorPayload={truncated}";
        }
    }
}
