// Copyright Niantic Spatial.

using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.Sites
{
    /// <summary>
    /// Asset type - determines which fields of <see cref="AssetArea"/> are meaningful.
    /// </summary>
    [PublicAPI]
    public enum AssetType
    {
        /// <summary>No type reported; no typed data present.</summary>
        Unspecified = 0,
        /// <summary>A 3D mesh of the site. RootNodeId, NodeIds and MeshCoverage are meaningful.</summary>
        Mesh = 1,
        /// <summary>A Gaussian splat of the site. RootNodeId is meaningful.</summary>
        Splat = 2,
        /// <summary>The site's localization map. AnchorPayload and the bounds are meaningful.</summary>
        VpsInfo = 3
    }

    /// <summary>
    /// Asset status - where the asset is in its lifecycle.
    ///
    /// DEPRECATED - the asset status is no longer reported, so
    /// <see cref="AssetInfo.AssetStatus"/> is always <see cref="Unspecified"/>.
    /// </summary>
    [PublicAPI]
    public enum AssetStatusType
    {
        /// <summary>No status reported.</summary>
        Unspecified = 0,
        /// <summary>The asset is in use.</summary>
        Active = 1,
        /// <summary>The asset is not in use.</summary>
        Inactive = 2,
        /// <summary>The asset is awaiting activation.</summary>
        Pending = 3
    }

    /// <summary>
    /// Asset deployment type - whether an asset is live.
    /// </summary>
    [PublicAPI]
    public enum AssetDeploymentType
    {
        /// <summary>No deployment state.</summary>
        Unspecified = 0,
        /// <summary>Deployed and live. The only kind VPS localizes against unless it is given an asset ID.</summary>
        Production = 1,
        /// <summary>Deployed but not live.</summary>
        Published = 2,
        /// <summary>Authored but not deployed.</summary>
        Draft = 3
    }

    /// <summary>
    /// Whether a location query returns production assets only, or every asset.
    /// </summary>
    [PublicAPI]
    public enum AssetScope
    {
        /// <summary>Production assets only: the ones nearby localization uses.</summary>
        Production = 0,
        /// <summary>Every asset, production or not.</summary>
        All = 1
    }

    /// <summary>
    /// Asset pipeline job status - how far the mapping pipeline got in producing this asset.
    /// </summary>
    [PublicAPI]
    public enum AssetPipelineJobStatus
    {
        /// <summary>No status reported.</summary>
        Unspecified = 0,
        /// <summary>Queued, not started.</summary>
        Pending = 1,
        /// <summary>Currently being built.</summary>
        Running = 2,
        /// <summary>Finished; the asset is usable.</summary>
        Succeeded = 3,
        /// <summary>Finished with an error; no asset will arrive.</summary>
        Failed = 4,
        /// <summary>The pipeline reported a state NSDK does not recognize.</summary>
        Unknown = 5,
        /// <summary>The pipeline has no record of the job.</summary>
        NotFound = 6,
        /// <summary>DEPRECATED on the v2 endpoints - never reported.</summary>
        Ready = 7,
        /// <summary>Stopped before finishing; no asset will arrive.</summary>
        Canceled = 8
    }
}
