// Copyright 2026 Niantic Spatial.

using System;

using UnityEngine.SubsystemsImplementation;

namespace NianticSpatial.NSDK.AR.XRSubsystems
{
    /// <summary>
    /// Constructor parameters for the <see cref="XRVps2AnchorSubsystemDescriptor"/>.
    /// </summary>
    public struct XRVps2AnchorSubsystemCinfo : IEquatable<XRVps2AnchorSubsystemCinfo>
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
        /// Specifies the <c>XRVps2AnchorSubsystem</c>-derived type that forwards casted calls to its provider.
        /// </summary>
        /// <value>
        /// The type of the subsystem to use for instantiation. If null, <c>XRVps2AnchorSubsystem</c> will be instantiated.
        /// </value>
        public Type subsystemTypeOverride { get; set; }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="XRVps2AnchorSubsystemCinfo"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="XRVps2AnchorSubsystemCinfo"/>, otherwise false.</returns>
        public bool Equals(XRVps2AnchorSubsystemCinfo other)
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
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="XRVps2AnchorSubsystemCinfo"/> and
        /// <see cref="Equals(XRVps2AnchorSubsystemCinfo)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(System.Object obj) =>
            ((obj is XRVps2AnchorSubsystemCinfo) &&
                Equals((XRVps2AnchorSubsystemCinfo)obj));

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XRVps2AnchorSubsystemCinfo)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==
            (XRVps2AnchorSubsystemCinfo lhs, XRVps2AnchorSubsystemCinfo rhs) =>
            lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XRVps2AnchorSubsystemCinfo)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=
            (XRVps2AnchorSubsystemCinfo lhs, XRVps2AnchorSubsystemCinfo rhs) =>
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
    /// Descriptor for the XRVps2AnchorSubsystem.
    /// </summary>
    public class XRVps2AnchorSubsystemDescriptor :
        SubsystemDescriptorWithProvider<XRVps2AnchorSubsystem,
            XRVps2AnchorSubsystem.Provider>
    {
        XRVps2AnchorSubsystemDescriptor(XRVps2AnchorSubsystemCinfo anchorSubsystemCinfo)
        {
            id = anchorSubsystemCinfo.id;
            providerType = anchorSubsystemCinfo.providerType;
            subsystemTypeOverride = anchorSubsystemCinfo.subsystemTypeOverride;
        }

        /// <summary>
        /// Creates the VPS2 anchor subsystem descriptor from the construction info.
        /// </summary>
        /// <param name="subsystemCinfo">The VPS2 anchor subsystem descriptor constructor information.</param>
        internal static XRVps2AnchorSubsystemDescriptor Create(XRVps2AnchorSubsystemCinfo subsystemCinfo)
        {
            if (string.IsNullOrEmpty(subsystemCinfo.id))
            {
                throw new ArgumentException
                (
                    "Cannot create VPS2 anchor subsystem descriptor because id is invalid",
                    nameof(subsystemCinfo)
                );
            }

            if (subsystemCinfo.providerType == null ||
                !subsystemCinfo.providerType.IsSubclassOf
                    (typeof(XRVps2AnchorSubsystem.Provider)))
            {
                throw new ArgumentException
                (
                    "Cannot create VPS2 anchor subsystem descriptor because providerType is invalid",
                    nameof(subsystemCinfo)
                );
            }

            if (subsystemCinfo.subsystemTypeOverride == null ||
                !subsystemCinfo.subsystemTypeOverride.IsSubclassOf
                    (typeof(XRVps2AnchorSubsystem)))
            {
                throw new ArgumentException
                (
                    "Cannot create VPS2 anchor subsystem descriptor because subsystemTypeOverride is invalid",
                    nameof(subsystemCinfo)
                );
            }

            return new XRVps2AnchorSubsystemDescriptor(subsystemCinfo);
        }
    }
}
