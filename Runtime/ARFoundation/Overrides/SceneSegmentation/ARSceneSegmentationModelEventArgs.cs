// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Collections.Generic;
using NianticSpatial.NSDK.AR.Subsystems.SceneSegmentation;
using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.SceneSegmentation
{
    /// <summary>
    /// A structure for information about the semantic segmentation model that's become ready. This is used to
    /// communicate information in the <see cref="ARSceneSegmentationManager.MetadataInitialized" /> event.
    /// </summary>
    [PublicAPI]
    public struct ARSceneSegmentationModelEventArgs : IEquatable<ARSceneSegmentationModelEventArgs>
    {
        /// <summary>
        /// The semantic channels detected by the semantic segmentation model.
        /// </summary>
        public IReadOnlyList<SceneSegmentationChannel> Channels { get; internal set; }


        /// <summary>
        /// The indices of the semantic channels detected by the semantic segmentation model.
        /// </summary>
        public IReadOnlyDictionary<SceneSegmentationChannel, int> ChannelIndices { get; internal set; }

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        public override int GetHashCode() =>
            HashCode.Combine(Channels.GetHashCode(), ChannelIndices.GetHashCode());

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="ARSceneSegmentationModelEventArgs"/> and
        /// <see cref="Equals(ARSceneSegmentationModelEventArgs)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(object obj)
            => obj is ARSceneSegmentationModelEventArgs && Equals((ARSceneSegmentationModelEventArgs)obj);

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="ARSceneSegmentationModelEventArgs"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="ARSceneSegmentationModelEventArgs"/>, otherwise false.</returns>
        public bool Equals(ARSceneSegmentationModelEventArgs other)
            => (Channels == null ? other.Channels == null : Channels.Equals(other.Channels))
                && (ChannelIndices == null ? other.ChannelIndices == null : ChannelIndices.Equals(other.ChannelIndices));

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(ARSceneSegmentationModelEventArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(ARSceneSegmentationModelEventArgs lhs, ARSceneSegmentationModelEventArgs rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(ARSceneSegmentationModelEventArgs)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(ARSceneSegmentationModelEventArgs lhs, ARSceneSegmentationModelEventArgs rhs) => !lhs.Equals(rhs);
    }
}
