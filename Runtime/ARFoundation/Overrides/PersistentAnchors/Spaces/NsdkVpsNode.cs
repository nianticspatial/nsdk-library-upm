// Copyright 2022-2026 Niantic Spatial.

using NianticSpatial.NSDK.AR.Utilities;

using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace NianticSpatial.NSDK.AR.PersistentAnchors.Spaces
{
    /// <summary>
    /// Represents a single node in a NSDK VPS space. Each node has a unique identifier and a
    ///  pose that represents the node's position and orientation in the space.
    ///
    /// @note This is an experimental feature. Experimental features should not be used in
    /// production products as they are subject to breaking changes, not officially supported, and
    /// may be deprecated without notice
    /// </summary>
    [Experimental]
    public struct NsdkVpsNode
    {
        /// <summary>
        /// Whether this node is the origin node of the space it belongs to.
        ///
        /// @note This is an experimental feature. Experimental features should not be used in
        /// production products as they are subject to breaking changes, not officially supported, and
        /// may be deprecated without notice
        /// </summary>
        [Experimental]
        public bool IsOrigin { get; internal set; }

        /// <summary>
        /// Id of the node
        ///
        /// @note This is an experimental feature. Experimental features should not be used in
        /// production products as they are subject to breaking changes, not officially supported, and
        /// may be deprecated without notice
        /// </summary>
        [Experimental]
        public string NodeId { get; internal set; }

        /// <summary>
        /// Local pose of the node in the space it belongs to. The pose is relative to the origin node of the
        /// space
        ///
        /// @note This is an experimental feature. Experimental features should not be used in
        /// production products as they are subject to breaking changes, not officially supported, and
        /// may be deprecated without notice
        /// </summary>
        [Experimental]
        public Pose NodeToSpaceOriginPose { get; internal set; }
    }
}
