// Copyright 2026 Niantic Spatial.

namespace NianticSpatial.NSDK.AR.Subsystems
{
    /// <summary>
    /// Whether a mesh download carries a usable georeference.
    /// </summary>
    public enum MeshGeoReferenceMode
    {
        /// <summary>
        /// The asset has no georeference, or its chunk transforms do not share a single frame for
        /// one to apply to. The mesh can only be placed in a local (non-geographic) frame; every
        /// other property of <see cref="MeshGeoReference"/> is zero.
        /// </summary>
        Unavailable = 0,

        /// <summary>
        /// The georeference is valid. Anchor the mesh group's world origin at the reference
        /// position and apply TrackingToReference to the whole group, then place each chunk by its
        /// own transform.
        /// </summary>
        Georeferenced = 1
    }
}
