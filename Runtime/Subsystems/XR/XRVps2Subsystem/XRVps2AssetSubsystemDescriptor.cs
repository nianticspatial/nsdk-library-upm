// Copyright 2026 Niantic Spatial.

using System;

using UnityEngine.SubsystemsImplementation;

namespace NianticSpatial.NSDK.AR.XRSubsystems
{
    /// <summary>
    /// Constructor parameters for the <see cref="XRVps2AssetSubsystemDescriptor"/>.
    /// </summary>
    public struct XRVps2AssetSubsystemCinfo : IEquatable<XRVps2AssetSubsystemCinfo>
    {
        /// <summary>
        /// Specifies an identifier for the provider implementation of the subsystem.
        /// </summary>
        /// <value>
        /// The identifier for the provider implementation of the subsystem.
        /// </value>
        public string id { get; set; }

        /// <summary>
        /// Specifies the provider implementation type to use for instantiation.
        /// </summary>
        /// <value>
        /// The provider implementation type to use for instantiation.
        /// </value>
        public Type providerType { get; set; }

        /// <summary>
        /// Specifies the <c>XRVps2AssetSubsystem</c>-derived type that forwards casted calls to its provider.
        /// </summary>
        /// <value>
        /// The type of the subsystem to use for instantiation. If null, <c>XRVps2AssetSubsystem</c> will be instantiated.
        /// </value>
        public Type subsystemTypeOverride { get; set; }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="XRVps2AssetSubsystemCinfo"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="XRVps2AssetSubsystemCinfo"/>, otherwise false.</returns>
        public bool Equals(XRVps2AssetSubsystemCinfo other)
        {
            return
                ReferenceEquals(id, other.id) &&
                ReferenceEquals(providerType, other.providerType) &&
                ReferenceEquals(subsystemTypeOverride, other.subsystemTypeOverride);
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="XRVps2AssetSubsystemCinfo"/> and
        /// <see cref="Equals(XRVps2AssetSubsystemCinfo)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(System.Object obj) =>
            ((obj is XRVps2AssetSubsystemCinfo) &&
                Equals((XRVps2AssetSubsystemCinfo)obj));

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XRVps2AssetSubsystemCinfo)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==
            (XRVps2AssetSubsystemCinfo lhs, XRVps2AssetSubsystemCinfo rhs) =>
            lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XRVps2AssetSubsystemCinfo)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=
            (XRVps2AssetSubsystemCinfo lhs, XRVps2AssetSubsystemCinfo rhs) =>
            !lhs.Equals(rhs);

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        public override int GetHashCode()
        {
            int hashCode = 486187739;
            unchecked
            {
                hashCode = (hashCode * 486187739) + id.GetHashCode();
                hashCode = (hashCode * 486187739) + providerType.GetHashCode();
                hashCode = (hashCode * 486187739) + subsystemTypeOverride.GetHashCode();
            }

            return hashCode;
        }
    }

    /// <summary>
    /// Descriptor for the XRVps2AssetSubsystem.
    /// </summary>
    public class XRVps2AssetSubsystemDescriptor :
        SubsystemDescriptorWithProvider<XRVps2AssetSubsystem,
            XRVps2AssetSubsystem.Provider>
    {
        XRVps2AssetSubsystemDescriptor(XRVps2AssetSubsystemCinfo assetSubsystemCinfo)
        {
            id = assetSubsystemCinfo.id;
            providerType = assetSubsystemCinfo.providerType;
            subsystemTypeOverride = assetSubsystemCinfo.subsystemTypeOverride;
        }

        /// <summary>
        /// Creates the VPS2 asset subsystem descriptor from the construction info.
        /// </summary>
        /// <param name="subsystemCinfo">The VPS2 asset subsystem descriptor constructor information.</param>
        internal static XRVps2AssetSubsystemDescriptor Create(XRVps2AssetSubsystemCinfo subsystemCinfo)
        {
            if (string.IsNullOrEmpty(subsystemCinfo.id))
            {
                throw new ArgumentException
                (
                    "Cannot create VPS2 asset subsystem descriptor because id is invalid",
                    nameof(subsystemCinfo)
                );
            }

            if (subsystemCinfo.providerType == null ||
                !subsystemCinfo.providerType.IsSubclassOf
                    (typeof(XRVps2AssetSubsystem.Provider)))
            {
                throw new ArgumentException
                (
                    "Cannot create VPS2 asset subsystem descriptor because providerType is invalid",
                    nameof(subsystemCinfo)
                );
            }

            if (subsystemCinfo.subsystemTypeOverride == null ||
                !subsystemCinfo.subsystemTypeOverride.IsSubclassOf
                    (typeof(XRVps2AssetSubsystem)))
            {
                throw new ArgumentException
                (
                    "Cannot create VPS2 asset subsystem descriptor because subsystemTypeOverride is invalid",
                    nameof(subsystemCinfo)
                );
            }

            return new XRVps2AssetSubsystemDescriptor(subsystemCinfo);
        }
    }
}
