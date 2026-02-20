// Copyright Niantic Spatial.

using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.Sites
{
    /// <summary>
    /// Asset type - determines which typed asset data is present.
    /// Maps to proto enum AssetType.
    /// </summary>
    [PublicAPI]
    public enum AssetType
    {
        /// <summary>Asset type not specified.</summary>
        Unspecified = 0,
        /// <summary>Mesh asset - has AssetMeshData.</summary>
        Mesh = 1,
        /// <summary>Splat asset - has AssetSplatData.</summary>
        Splat = 2,
        /// <summary>VPS Info asset - has AssetVpsData.</summary>
        VpsInfo = 3
    }

    /// <summary>
    /// Asset status.
    /// Maps to proto enum AssetStatusType.
    /// </summary>
    [PublicAPI]
    public enum AssetStatusType
    {
        /// <summary>Status not specified.</summary>
        Unspecified = 0,
        /// <summary>Asset is active.</summary>
        Active = 1,
        /// <summary>Asset is inactive.</summary>
        Inactive = 2,
        /// <summary>Asset is pending.</summary>
        Pending = 3
    }

    /// <summary>
    /// Asset deployment type.
    /// Maps to proto enum AssetDeploymentType.
    /// </summary>
    [PublicAPI]
    public enum AssetDeploymentType
    {
        /// <summary>Deployment type not specified.</summary>
        Unspecified = 0,
        /// <summary>Asset is deployed to production.</summary>
        Production = 1
    }

    /// <summary>
    /// Asset pipeline job status.
    /// Maps to proto enum AssetPipelineJobStatus.
    /// </summary>
    [PublicAPI]
    public enum AssetPipelineJobStatus
    {
        /// <summary>Status not specified.</summary>
        Unspecified = 0,
        /// <summary>Job is pending.</summary>
        Pending = 1,
        /// <summary>Job is running.</summary>
        Running = 2,
        /// <summary>Job succeeded.</summary>
        Succeeded = 3,
        /// <summary>Job failed.</summary>
        Failed = 4,
        /// <summary>Job status is unknown.</summary>
        Unknown = 5,
        /// <summary>Job not found.</summary>
        NotFound = 6,
        /// <summary>Job is ready.</summary>
        Ready = 7
    }
}
