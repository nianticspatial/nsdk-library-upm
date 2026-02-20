// Copyright 2026 Niantic Spatial.

using System;

using UnityEngine.SubsystemsImplementation;

namespace NianticSpatial.NSDK.AR.XRSubsystems
{
    /// <summary>
    /// Constructor parameters for the <see cref="XRVps2SubsystemDescriptor"/>.
    /// </summary>
    public struct XRVps2SubsystemCinfo : IEquatable<XRVps2SubsystemCinfo>
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
        /// Specifies the <c>XRVps2Subsystem</c>-derived type that forwards casted calls to its provider.
        /// </summary>
        /// <value>
        /// The type of the subsystem to use for instantiation. If null, <c>XRVps2Subsystem</c> will be instantiated.
        /// </value>
        public Type subsystemTypeOverride { get; set; }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="XRVps2SubsystemCinfo"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="XRVps2SubsystemCinfo"/>, otherwise false.</returns>
        public bool Equals(XRVps2SubsystemCinfo other)
        {
            return
                ReferenceEquals
                    (id, other.id) &&
                ReferenceEquals(providerType, other.providerType) &&
                ReferenceEquals(subsystemTypeOverride, other.subsystemTypeOverride);
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="XRVps2SubsystemCinfo"/> and
        /// <see cref="Equals(XRVps2SubsystemCinfo)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(System.Object obj) =>
            ((obj is XRVps2SubsystemCinfo) &&
                Equals((XRVps2SubsystemCinfo)obj));

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XRVps2SubsystemCinfo)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==
            (XRVps2SubsystemCinfo lhs, XRVps2SubsystemCinfo rhs) =>
            lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XRVps2SubsystemCinfo)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=
            (XRVps2SubsystemCinfo lhs, XRVps2SubsystemCinfo rhs) =>
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
    /// Descriptor for the XRVps2Subsystem.
    /// </summary>
    public class XRVps2SubsystemDescriptor :
        SubsystemDescriptorWithProvider<XRVps2Subsystem,
            XRVps2Subsystem.Provider>
    {
        XRVps2SubsystemDescriptor(XRVps2SubsystemCinfo wpsSubsystemCinfo)
        {
            id = wpsSubsystemCinfo.id;
            providerType = wpsSubsystemCinfo.providerType;
            subsystemTypeOverride = wpsSubsystemCinfo.subsystemTypeOverride;
        }

        /// <summary>
        /// Creates the World Space subsystem descriptor from the construction info.
        /// </summary>
        /// <param name="wpsSubsystemCinfo">The World Space subsystem descriptor constructor information.</param>
        internal static XRVps2SubsystemDescriptor Create
            (XRVps2SubsystemCinfo wpsSubsystemCinfo)
        {
            if (string.IsNullOrEmpty(wpsSubsystemCinfo.id))
            {
                throw new ArgumentException
                (
                    "Cannot create World Space subsystem descriptor because id is invalid",
                    nameof(wpsSubsystemCinfo)
                );
            }

            if (wpsSubsystemCinfo.providerType == null ||
                !wpsSubsystemCinfo.providerType.IsSubclassOf
                    (typeof(XRVps2Subsystem.Provider)))
            {
                throw new ArgumentException
                (
                    "Cannot create World Space subsystem descriptor because providerType is invalid",
                    nameof(wpsSubsystemCinfo)
                );
            }

            if (wpsSubsystemCinfo.subsystemTypeOverride == null ||
                !wpsSubsystemCinfo.subsystemTypeOverride.IsSubclassOf
                    (typeof(XRVps2Subsystem)))
            {
                throw new ArgumentException
                (
                    "Cannot create World Space subsystem descriptor because subsystemTypeOverride is invalid",
                    nameof(wpsSubsystemCinfo)
                );
            }

            return new XRVps2SubsystemDescriptor(wpsSubsystemCinfo);
        }
    }
}
