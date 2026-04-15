// Copyright 2022-2026 Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.Utilities;
using UnityEngine.SubsystemsImplementation;

namespace NianticSpatial.NSDK.AR.XRSubsystems
{
    [PublicAPI]
    public class XRDeviceMappingSubsystemDescriptor : SubsystemDescriptorWithProvider<XRDeviceMappingSubsystem,
            XRDeviceMappingSubsystem.Provider>
    {
        public struct Cinfo : IEquatable<Cinfo>
        {
            /// <summary>
            /// The string identifier for this subsystem.
            /// </summary>
            public string id { get; set; }

            /// <summary>
            /// Specifies the provider implementation type to use for instantiation.
            /// </summary>
            public Type providerType { get; set; }

            /// <summary>
            /// Specifies the <c>XRDeviceMappingSubsystem</c>-derived type that forwards casted calls to its provider.
            /// </summary>
            public Type subsystemTypeOverride { get; set; }

            public override int GetHashCode()
            {
                return HashCode.Combine(id, providerType, subsystemTypeOverride);
            }

            public override bool Equals(object obj) => (obj is Cinfo other) && Equals(other);

            public bool Equals(Cinfo other)
            {
                return
                    String.Equals(id, other.id) &&
                    ReferenceEquals(providerType, other.providerType) &&
                    ReferenceEquals(subsystemTypeOverride, other.subsystemTypeOverride);
            }

            public static bool operator ==(Cinfo lhs, Cinfo rhs) => lhs.Equals(rhs);

            public static bool operator !=(Cinfo lhs, Cinfo rhs) => !lhs.Equals(rhs);
        }

        /// <summary>
        /// Creates a new subsystem descriptor and registers it with the <c>SubsystemManager</c>.
        /// </summary>
        /// <param name="cinfo">Constructor info describing the descriptor to create.</param>
        public static void Create(Cinfo cinfo)
        {
            SubsystemDescriptorStore.RegisterDescriptor(new XRDeviceMappingSubsystemDescriptor(cinfo));
        }

        private XRDeviceMappingSubsystemDescriptor(Cinfo cinfo)
        {
            id = cinfo.id;
            providerType = cinfo.providerType;
            subsystemTypeOverride = cinfo.subsystemTypeOverride;
        }
    }
}
