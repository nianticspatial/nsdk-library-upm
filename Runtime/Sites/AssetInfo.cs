// Copyright Niantic Spatial.

using System;
using System.Collections.Generic;
using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.Sites
{
    /// <summary>
    /// Information about an asset, and the areas it is split into.
    /// </summary>
    [PublicAPI]
    public readonly struct AssetInfo
    {
        // --- From assetKey ---

        /// <summary>
        /// The unique identifier for the asset.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The Unix timestamp (in seconds) from the asset key, or 0 if unset.
        /// </summary>
        public long AssetTimestamp { get; }

        // --- From the response root ---

        /// <summary>
        /// The site ID this asset belongs to.
        /// </summary>
        public string SiteId { get; }

        // --- From assetData ---

        /// <summary>
        /// The asset's display name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The asset's description, or null if not set.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The asset type - determines which fields of <see cref="Areas"/> are meaningful.
        /// </summary>
        public AssetType AssetType { get; }

        /// <summary>
        /// DEPRECATED - always <see cref="AssetStatusType.Unspecified"/>.
        /// </summary>
        public AssetStatusType AssetStatus { get; }

        /// <summary>
        /// The asset deployment type.
        /// </summary>
        public AssetDeploymentType Deployment { get; }

        /// <summary>
        /// DEPRECATED - read <see cref="AssetArea.JobStatus"/> on each entry of
        /// <see cref="Areas"/>. The portal leaves the asset-level status unset, since a job
        /// belongs to an area, so expect <see cref="AssetPipelineJobStatus.Unspecified"/>.
        /// </summary>
        public AssetPipelineJobStatus PipelineJobStatus { get; }

        // --- DEPRECATED: wholly replaced by Areas ---
        // Every field on these three also exists on AssetArea, read from Areas[0].
        //
        // All three are null unless the first area carries the field this type is read
        // through: the anchor for VpsInfo, the root node for Mesh and Splat.

        /// <summary>
        /// DEPRECATED - read <see cref="Areas"/>[0] instead. Only non-null when
        /// <see cref="AssetType"/> is <see cref="AssetType.Mesh"/> and <see cref="Areas"/> is
        /// non-empty, and it describes that first area only.
        /// </summary>
        public AssetMeshData? MeshData { get; }

        /// <summary>
        /// DEPRECATED - read <see cref="Areas"/>[0] instead. Only non-null when
        /// <see cref="AssetType"/> is <see cref="AssetType.Splat"/> and <see cref="Areas"/> is
        /// non-empty.
        /// </summary>
        public AssetSplatData? SplatData { get; }

        /// <summary>
        /// DEPRECATED - read <see cref="Areas"/>[0] instead. Only non-null when
        /// <see cref="AssetType"/> is <see cref="AssetType.VpsInfo"/> and <see cref="Areas"/> is
        /// non-empty.
        /// </summary>
        public AssetVpsData? VpsData { get; }

        /// <summary>
        /// DEPRECATED - always null. The portal has no source for it; retained so existing
        /// callers keep compiling.
        /// </summary>
        public string PipelineJobId { get; }

        /// <summary>
        /// DEPRECATED - always empty. The portal has no source for it; retained so existing
        /// callers keep compiling.
        /// </summary>
        public IReadOnlyList<string> SourceScanIds { get; }

        /// <summary>
        /// Every area of this asset.
        /// </summary>
        /// <remarks>
        /// An asset mapped in a single pass has exactly one. Empty while the asset's job has
        /// produced no output yet.
        /// </remarks>
        public IReadOnlyList<AssetArea> Areas { get; }

        public AssetInfo(
            string id,
            long assetTimestamp,
            string siteId,
            string name,
            string description,
            AssetType assetType,
            AssetStatusType assetStatus,
            AssetDeploymentType deployment,
            AssetMeshData? meshData,
            AssetSplatData? splatData,
            AssetVpsData? vpsData,
            string pipelineJobId,
            AssetPipelineJobStatus pipelineJobStatus,
            IReadOnlyList<string> sourceScanIds)
            : this(id, assetTimestamp, siteId, name, description, assetType, assetStatus,
                   deployment, meshData, splatData, vpsData, pipelineJobId, pipelineJobStatus,
                   sourceScanIds, Array.Empty<AssetArea>())
        {
        }

        public AssetInfo(
            string id,
            long assetTimestamp,
            string siteId,
            string name,
            string description,
            AssetType assetType,
            AssetStatusType assetStatus,
            AssetDeploymentType deployment,
            AssetMeshData? meshData,
            AssetSplatData? splatData,
            AssetVpsData? vpsData,
            string pipelineJobId,
            AssetPipelineJobStatus pipelineJobStatus,
            IReadOnlyList<string> sourceScanIds,
            IReadOnlyList<AssetArea> areas)
        {
            Id = id ?? string.Empty;
            AssetTimestamp = assetTimestamp;
            SiteId = siteId ?? string.Empty;
            Name = name ?? string.Empty;
            Description = description;
            AssetType = assetType;
            AssetStatus = assetStatus;
            Deployment = deployment;
            MeshData = meshData;
            SplatData = splatData;
            VpsData = vpsData;
            PipelineJobId = pipelineJobId;
            PipelineJobStatus = pipelineJobStatus;
            SourceScanIds = sourceScanIds ?? Array.Empty<string>();
            Areas = areas ?? Array.Empty<AssetArea>();
        }

        /// <inheritdoc />
        public override string ToString()
        {
            var result = $"Asset: Id={Id}, Name={Name}, Type={AssetType}, Status={AssetStatus}, SiteId={SiteId}";

            if (!string.IsNullOrEmpty(Description))
            {
                result += $", Description={Description}";
            }

            if (Deployment != AssetDeploymentType.Unspecified)
            {
                result += $", Deployment={Deployment}";
            }

            if (!string.IsNullOrEmpty(PipelineJobId))
            {
                result += $", PipelineJobId={PipelineJobId}";
            }

            if (PipelineJobStatus != AssetPipelineJobStatus.Unspecified)
            {
                result += $", PipelineJobStatus={PipelineJobStatus}";
            }

            if (SourceScanIds.Count > 0)
            {
                result += $", SourceScanIds=[{string.Join(", ", SourceScanIds)}]";
            }

            if (Areas.Count > 0)
            {
                result += $", Areas={Areas.Count}";
            }

            if (MeshData.HasValue)
            {
                result += $", {MeshData.Value}";
            }

            if (SplatData.HasValue)
            {
                result += $", {SplatData.Value}";
            }

            if (VpsData.HasValue)
            {
                result += $", {VpsData.Value}";
            }

            return result;
        }
    }
}
