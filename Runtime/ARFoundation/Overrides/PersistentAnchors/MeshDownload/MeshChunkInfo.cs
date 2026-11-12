// Copyright 2026 Niantic Spatial.

using UnityEngine;

namespace NianticSpatial.NSDK.AR.Subsystems
{
    /// <summary>
    /// Identifies the asset and area a downloaded mesh chunk belongs to. Attached to the chunk
    /// GameObject returned by
    /// <see cref="LocationMeshManager.GetLocationMeshChunksByVpsMapAssetIdAsync"/>.
    /// </summary>
    public class MeshChunkInfo : MonoBehaviour
    {
        /// <summary>
        /// The VPS map asset ID the download was requested with. Not the ID of the mesh asset the
        /// geometry came from, so it matches the asset VPS2 reports as localized.
        /// </summary>
        public string VpsMapAssetId { get; private set; } = string.Empty;

        /// <summary>
        /// Identifier of subarea in the site this chunk belongs to, or an empty string when the
        /// asset has no subareas. This identifier can be used to optimize which mesh chunk to
        /// render depending on localization result.
        /// </summary>
        public string AreaId { get; private set; } = string.Empty;

        internal void Initialize(string vpsMapAssetId, string areaId)
        {
            VpsMapAssetId = vpsMapAssetId;
            AreaId = areaId;
        }
    }
}
