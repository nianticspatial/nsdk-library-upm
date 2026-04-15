// Copyright 2022-2026 Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.Utilities;
using UnityEngine.SubsystemsImplementation;

namespace NianticSpatial.NSDK.AR.XRSubsystems
{
    [Experimental]
    [PublicAPI]
    public class XRDeviceMappingSubsystem
        : SubsystemWithProvider<XRDeviceMappingSubsystem, XRDeviceMappingSubsystemDescriptor, XRDeviceMappingSubsystem.Provider>
    {
        public XRDeviceMappingSubsystem()
        {
        }

        /// <summary>
        /// Target framerate to run mappers. A value of 0 indicates maximum framerate.
        /// </summary>
        public uint TargetFrameRate
        {
            set => provider.TargetFrameRate = value;
        }

        /// <summary>
        /// Node splitter config for max distance travelled before creating a new map node.
        /// </summary>
        public float SplitterMaxDistanceMeters
        {
            set => provider.SplitterMaxDistanceMeters = value;
        }

        /// <summary>
        /// Node splitter config for max duration exceeded before creating a new map node.
        /// </summary>
        public float SplitterMaxDurationSeconds
        {
            set => provider.SplitterMaxDurationSeconds = value;
        }

        /// <summary>
        /// Whether mapping is currently in progress.
        /// </summary>
        public bool IsMapping => provider.IsMapping;

        /// <summary>
        /// Start map generation.
        /// </summary>
        public void StartMapping()
        {
            provider.StartMapping();
        }

        /// <summary>
        /// Stop map generation.
        /// </summary>
        public void StopMapping()
        {
            provider.StopMapping();
        }

        public abstract class Provider : SubsystemProvider<XRDeviceMappingSubsystem>
        {
            public virtual uint TargetFrameRate
            {
                set => throw new NotSupportedException();
            }

            public virtual float SplitterMaxDistanceMeters
            {
                set => throw new NotSupportedException();
            }

            public virtual float SplitterMaxDurationSeconds
            {
                set => throw new NotSupportedException();
            }

            public virtual bool IsMapping => throw new NotSupportedException();

            public virtual void StartMapping()
            {
                throw new NotSupportedException();
            }

            public virtual void StopMapping()
            {
                throw new NotSupportedException();
            }
        }
    }
}
