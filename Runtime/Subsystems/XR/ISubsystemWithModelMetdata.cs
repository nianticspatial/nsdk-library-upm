// Copyright 2022-2026 Niantic Spatial.

using UnityEngine.SubsystemsImplementation;

namespace NianticSpatial.NSDK.AR.Subsystems.XR
{
    public interface ISubsystemWithModelMetadata
    {
        public bool IsMetadataAvailable { get; }

        public uint? LatestFrameId { get; }
    }
}
