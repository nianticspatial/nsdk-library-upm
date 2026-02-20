// Copyright 2022-2026 Niantic Spatial.

using NianticSpatial.NSDK.AR.PAM;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace NianticSpatial.NSDK.AR.Utilities
{
    internal static class XREnumConversions
    {
        // Translates ARFoundation's TrackingState enum into the corresponding NSDK tracking state values
        // defined in tracking_state.h
        // Note: ARFoundation has no "Failed" state corresponding to NSDK's
        public static byte FromUnityToNsdk(this TrackingState state)
        {
            switch (state)
            {
                case TrackingState.None:
                    return 0; // Unknown
                case TrackingState.Limited:
                    return 2; // Poor
                case TrackingState.Tracking:
                    return 3; // Normal
                default:
                    return 0;
            }
        }

        // Translates Unity's ScreenOrientation enum into the corresponding NSDK values
        // defined in orientation.h
        public static byte FromUnityToNsdk(this ScreenOrientation orientation)
        {
            switch (orientation)
            {
                case ScreenOrientation.Portrait:
                    return 1; // Portrait
                case ScreenOrientation.PortraitUpsideDown:
                    return 2; // PortraitUpsideDown
                case ScreenOrientation.LandscapeLeft:
                    return 4; // LandscapeLeft
                case ScreenOrientation.LandscapeRight:
                    return 3; // LandscapeRight
                default:
                    return 0;
            }
        }
    }
}
