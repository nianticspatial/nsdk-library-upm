// Copyright 2022-2026 Niantic Spatial.
using System;
using NianticSpatial.NSDK.AR.Utilities;
using UnityEngine.SubsystemsImplementation;
using UnityEngine.XR.ARSubsystems;

namespace NianticSpatial.NSDK.AR.XRSubsystems
{
    /// <summary>
    /// Constructor parameters for the <see cref="XRSceneSegmentationSubsystemDescriptor"/>.
    /// </summary>
    [PublicAPI]
    public struct XRSceneSegmentationSubsystemCinfo : IEquatable<XRSceneSegmentationSubsystemCinfo>
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
        /// Specifies the <c>XRAnchorSubsystem</c>-derived type that forwards casted calls to its provider.
        /// </summary>
        /// <value>
        /// The type of the subsystem to use for instantiation. If null, <c>XRAnchorSubsystem</c> will be instantiated.
        /// </value>
        public Type subsystemTypeOverride { get; set; }


        /// <summary>
        /// Specifies if the current subsystem supports semantics segmentation image.
        /// </summary>
        public Func<Supported> sceneSegmentationImageSupportedDelegate { get; set; }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="XRSceneSegmentationSubsystemCinfo"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="XRSceneSegmentationSubsystemCinfo"/>, otherwise false.</returns>
        public bool Equals(XRSceneSegmentationSubsystemCinfo other)
        {
            return
                ReferenceEquals(id, other.id)
                && ReferenceEquals(providerType, other.providerType)
                && ReferenceEquals(subsystemTypeOverride, other.subsystemTypeOverride)
                && sceneSegmentationImageSupportedDelegate == other.sceneSegmentationImageSupportedDelegate;
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="XRSceneSegmentationSubsystemCinfo"/> and
        /// <see cref="Equals(XRSceneSegmentationSubsystemCinfo)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(System.Object obj) => ((obj is XRSceneSegmentationSubsystemCinfo) && Equals((XRSceneSegmentationSubsystemCinfo)obj));

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XRSceneSegmentationSubsystemCinfo)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(XRSceneSegmentationSubsystemCinfo lhs, XRSceneSegmentationSubsystemCinfo rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XRSceneSegmentationSubsystemCinfo)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(XRSceneSegmentationSubsystemCinfo lhs, XRSceneSegmentationSubsystemCinfo rhs) => !lhs.Equals(rhs);

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
                hashCode = (hashCode * 486187739) + sceneSegmentationImageSupportedDelegate.GetHashCode();
            }
            return hashCode;
        }
    }

    /// <summary>
    /// Descriptor for the XRSceneSegmentationSubsystem.
    /// </summary>
    [PublicAPI]
    public class XRSceneSegmentationSubsystemDescriptor :
        SubsystemDescriptorWithProvider<XRSceneSegmentationSubsystem, XRSceneSegmentationSubsystem.Provider>
    {
        private XRSceneSegmentationSubsystemDescriptor(XRSceneSegmentationSubsystemCinfo sceneSegmentationSubsystemCinfo)
        {
            id = sceneSegmentationSubsystemCinfo.id;
            providerType = sceneSegmentationSubsystemCinfo.providerType;
            subsystemTypeOverride = sceneSegmentationSubsystemCinfo.subsystemTypeOverride;
            m_SceneSegmentationImageSupportedDelegate = sceneSegmentationSubsystemCinfo.sceneSegmentationImageSupportedDelegate;
        }

        /// <summary>
        /// Query for whether semantic segmentation is supported.
        /// </summary>
        private Func<Supported> m_SceneSegmentationImageSupportedDelegate;

        /// <summary>
        /// (Read Only) Whether the subsystem supports semantic segmentation image.
        /// </summary>
        /// <remarks>
        /// The supported status might take time to determine. If support is still being determined, the value will be <see cref="Supported.Unknown"/>.
        /// </remarks>
        public Supported sceneSegmentationImageSupported
        {
            get
            {
                if (m_SceneSegmentationImageSupportedDelegate != null)
                {
                    return m_SceneSegmentationImageSupportedDelegate();
                }

                return Supported.Unknown;
            }
        }


        /// <summary>
        /// Creates the semantics subsystem descriptor from the construction info.
        /// </summary>
        /// <param name="sceneSegmentationSubsystemCinfo">The semantics subsystem descriptor constructor information.</param>
        internal static XRSceneSegmentationSubsystemDescriptor Create(XRSceneSegmentationSubsystemCinfo sceneSegmentationSubsystemCinfo)
        {
            if (string.IsNullOrEmpty(sceneSegmentationSubsystemCinfo.id))
            {
                throw new ArgumentException("Cannot create semantics subsystem descriptor because id is invalid",
                                            nameof(sceneSegmentationSubsystemCinfo));
            }

            if (sceneSegmentationSubsystemCinfo.providerType == null
                || !sceneSegmentationSubsystemCinfo.providerType.IsSubclassOf(typeof(XRSceneSegmentationSubsystem.Provider)))
            {
                throw new ArgumentException("Cannot create semantics subsystem descriptor because providerType is invalid",
                                            nameof(sceneSegmentationSubsystemCinfo));
            }

            if (sceneSegmentationSubsystemCinfo.subsystemTypeOverride == null
                || !sceneSegmentationSubsystemCinfo.subsystemTypeOverride.IsSubclassOf(typeof(XRSceneSegmentationSubsystem)))
            {
                throw new ArgumentException("Cannot create semantics subsystem descriptor because subsystemTypeOverride is invalid",
                                            nameof(sceneSegmentationSubsystemCinfo));
            }

            return new XRSceneSegmentationSubsystemDescriptor(sceneSegmentationSubsystemCinfo);
        }
    }
}
