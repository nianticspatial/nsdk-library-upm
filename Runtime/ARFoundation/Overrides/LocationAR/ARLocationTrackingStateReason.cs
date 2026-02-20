// Copyright 2022-2026 Niantic Spatial.

using System;

namespace NianticSpatial.NSDK.AR.LocationAR
{
    public enum ARLocationTrackingStateReason : UInt32
    {
        Unknown = 0,
        None = 1,
        Limited = 2,
        Removed = 3,
    }
}
