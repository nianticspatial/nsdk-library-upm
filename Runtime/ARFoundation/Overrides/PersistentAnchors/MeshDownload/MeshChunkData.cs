// Copyright 2026 Niantic Spatial.

using UnityEngine;

namespace NianticSpatial.NSDK.AR.Subsystems
{
    /// <summary>
    /// The geometry of one downloaded mesh chunk, copied out of NSDK into managed memory and not
    /// yet turned into Unity objects. Yielded by
    /// <see cref="LocationMeshManager.GetLocationMeshChunkDataByVpsMapAssetIdAsync"/>.
    /// </summary>
    /// <remarks>
    /// Holds no Unity Mesh, Texture or Material, so keeping it costs only managed memory. Pass it
    /// to <see cref="LocationMeshManager.CreateMeshChunk"/> to build a chunk GameObject. Destroying
    /// that GameObject frees its Unity assets and leaves this data intact, so the chunk can be
    /// built again later without downloading it again.
    /// </remarks>
    public sealed class MeshChunkData
    {
        /// <summary>
        /// The VPS map asset ID the download was requested with. Not the ID of the mesh asset the
        /// geometry came from, so it matches the asset VPS2 reports as localized.
        /// </summary>
        public string VpsMapAssetId { get; }

        /// <summary>
        /// Identifier of subarea in the site this chunk belongs to, or an empty string when the
        /// asset has no subareas.
        /// </summary>
        public string AreaId { get; }

        internal Vector3[] Vertices { get; }
        internal Vector2[] Uvs { get; }
        internal int[] Indices { get; }
        internal Color[] Colors { get; }

        // Encoded image file bytes; null when the chunk has no texture.
        internal byte[] ImageData { get; }

        internal Matrix4x4 Transform { get; }
        internal MeshDownloadClient.NsdkMeshGeoReference GeoReference { get; }

        internal MeshChunkData(
            string vpsMapAssetId,
            string areaId,
            Vector3[] vertices,
            Vector2[] uvs,
            int[] indices,
            Color[] colors,
            byte[] imageData,
            Matrix4x4 transform,
            MeshDownloadClient.NsdkMeshGeoReference geoReference)
        {
            VpsMapAssetId = vpsMapAssetId;
            AreaId = areaId;
            Vertices = vertices;
            Uvs = uvs;
            Indices = indices;
            Colors = colors;
            ImageData = imageData;
            Transform = transform;
            GeoReference = geoReference;
        }
    }
}
