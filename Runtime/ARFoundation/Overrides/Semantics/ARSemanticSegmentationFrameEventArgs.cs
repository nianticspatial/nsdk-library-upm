// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Collections.Generic;
using System.Text;
using NianticSpatial.NSDK.AR.Utilities;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace NianticSpatial.NSDK.AR.Semantics
{
    /// <summary>
    /// A structure for camera-related information pertaining to a particular frame.
    /// This is used to communicate information in the <see cref="ARSemanticSegmentationManager.frameReceived" /> event.
    /// </summary>
    [PublicAPI]
    public readonly struct ARSemanticSegmentationFrameEventArgs
    {
    }
}
