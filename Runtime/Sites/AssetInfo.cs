// Copyright Niantic Spatial.

using System;
using System.Collections.Generic;
using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.Sites
{
    /// <summary>
    /// Information about an asset.
    /// Maps to proto messages AssetRecord, AssetData, and AssetComputedValues.
    /// </summary>
    [PublicAPI]
    public readonly struct AssetInfo
    {
        // --- From AssetKey ---

        /// <summary>
        /// The unique identifier for the asset.
        /// </summary>
        public string Id { get; }

        // --- From AssetRecord ---

        /// <summary>
        /// The site ID this asset belongs to.
        /// </summary>
        public string SiteId { get; }

        // --- From AssetData ---

        /// <summary>
        /// The asset's display name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The asset's description, or null if not set.
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The asset type - determines which typed asset data is present.
        /// </summary>
        public AssetType AssetType { get; }

        /// <summary>
        /// The asset status.
        /// </summary>
        public AssetStatusType AssetStatus { get; }

        /// <summary>
        /// The asset deployment type.
        /// </summary>
        public AssetDeploymentType Deployment { get; }

        /// <summary>
        /// Mesh-specific data. Only valid when <see cref="AssetType"/> is <see cref="AssetType.Mesh"/>.
        /// </summary>
        public AssetMeshData? MeshData { get; }

        /// <summary>
        /// Splat-specific data. Only valid when <see cref="AssetType"/> is <see cref="AssetType.Splat"/>.
        /// </summary>
        public AssetSplatData? SplatData { get; }

        /// <summary>
        /// VPS-specific data. Only valid when <see cref="AssetType"/> is <see cref="AssetType.VpsInfo"/>.
        /// </summary>
        public AssetVpsData? VpsData { get; }

        // --- From AssetComputedValues ---

        /// <summary>
        /// The pipeline job ID associated with this asset, or null if not applicable.
        /// </summary>
        public string PipelineJobId { get; }

        /// <summary>
        /// The pipeline job status.
        /// </summary>
        public AssetPipelineJobStatus PipelineJobStatus { get; }

        /// <summary>
        /// The source scan IDs used to create this asset.
        /// </summary>
        public IReadOnlyList<string> SourceScanIds { get; }

        public AssetInfo(
            string id,
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
        {
            Id = id ?? string.Empty;
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
