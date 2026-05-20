// Copyright Niantic Spatial.

using System;
using System.Runtime.InteropServices;
using NianticSpatial.NSDK.AR.API;
using NianticSpatial.NSDK.AR.XRSubsystems;
using UnityEngine.XR.ARSubsystems;

namespace NianticSpatial.NSDK.AR.Subsystems.Vps2.Api
{
    internal interface IApi
    {
        public const int NSDK_VPS2_ANCHOR_ID_SIZE = 32;
        public const int NSDK_VPS2_SESSION_ID_SIZE = 32;

        [StructLayout(LayoutKind.Sequential)]
        public struct NsdkVps2Config
        {
            [MarshalAs(UnmanagedType.U1)]
            public bool bevLocalizationEnabled;

            public float bevRequestsPerSecond;

            [MarshalAs(UnmanagedType.U1)]
            public bool bevMulticameraEnabled;

            [MarshalAs(UnmanagedType.U1)]
            public bool vpsLocalizationEnabled;

            public float initialVpsRequestsPerSecond;
            public float continuousVpsRequestsPerSecond;

            [MarshalAs(UnmanagedType.U1)]
            public bool geolocationSmoothingEnabled;

            [MarshalAs(UnmanagedType.U1)]
            public bool vpsDebuggerEnabled;

            [MarshalAs(UnmanagedType.U1)]
            public bool deviceMapLocalizationEnabled;

            public int deviceMapLocalizationFramerate;

            // Layout padding: keeps this struct in sync with ARDK_VPS2_Config which has
            // aerial_localization_enabled at this position. Always false from Unity.
            [MarshalAs(UnmanagedType.U1)]
            private bool aerialLocalizationEnabled;

            public int universalLocalizationRequestTimeoutMs;

            public uint maxRequestsInTransitPerTarget;

            public float anchorDistanceGateMeters;
        }

        // Defined in ardk_vps2_localization.h
        [StructLayout(LayoutKind.Sequential)]
        public struct NsdkVps2Localization
        {
            public Int32 trackingState;
            public double referenceLatitudeDegrees;
            public double referenceLongitudeDegrees;
            public double referenceAltitudeMeters;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public double[] trackingToRelativeLonNegAltLat;

            public float horizontalAccuracyMeters;
            public float verticalAccuracyMeters;
            public float rotationAccuracyDegrees;
        }

        // Defined in ardk_vps2_geolocation.h
        [StructLayout(LayoutKind.Sequential)]
        public struct NsdkVps2GeolocationData
        {
            public Int32 trackingState;
            public NsdkGeolocationData geolocationData;
            public float horizontalAccuracyMeters;
            public float verticalAccuracyMeters;
            public float rotationAccuracyDegrees;
        }

        // Defined in ardk_vps2_pose.h
        [StructLayout(LayoutKind.Sequential)]
        public struct NsdkVps2Pose
        {
            public NsdkTransform pose;
            public float horizontalAccuracyMeters;
            public float verticalAccuracyMeters;
            public float rotationAccuracyDegrees;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NsdkVps2NetworkResponseRecord
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NSDK_VPS2_ANCHOR_ID_SIZE)]
            public byte[] requestIdentifier; // char[32] hex-encoded UUID

            public byte status;  // uint8_t enum
            public byte type;    // uint8_t enum
            public byte error;   // uint8_t enum

            public UInt64 startTimeMs;
            public UInt64 endTimeMs;
            public UInt64 frameId;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct NsdkVpsAnchorUpdate
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NSDK_VPS2_ANCHOR_ID_SIZE)]
            public byte[] anchorId;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public float[] pose;

            public byte trackingState;          // uint8_t in C++
            public byte trackingStateReason;    // uint8_t in C++
            private byte _padding0;             // alignment padding for float
            private byte _padding1;             // alignment padding for float
            public float confidence;
            public ulong timestamp;
            public NsdkGeolocationData geolocationData;
            public byte hasGeolocation;
            public byte updateType;             // uint8_t in C++
        }

        IntPtr Construct(IntPtr unityContext);

        void Destroy(IntPtr providerHandle);

        NsdkFeatureStatus GetFeatureStatus(IntPtr providerHandle);

        NsdkStatus Start(IntPtr providerHandle);

        NsdkStatus Stop(IntPtr providerHandle);

        NsdkStatus Configure(IntPtr providerHandle, NsdkVps2Config config);

        NsdkStatus GetLatestLocalization(IntPtr providerHandle, out NsdkVps2Localization localization);

        NsdkStatus GetDeviceGeolocation(IntPtr providerHandle, HeadingMode headingMode, out NsdkVps2GeolocationData location);

        NsdkStatus GetPose(NsdkVps2Localization localization, NsdkGeolocationData location, out NsdkVps2Pose pose);

        NsdkStatus CreateAnchor(IntPtr providerHandle, NsdkTransform pose, ref byte[] anchorId);

        NsdkStatus TrackAnchor(IntPtr providerHandle, NsdkString anchorPayload, ref byte[] anchorId);

        NsdkStatus RemoveAnchor(IntPtr providerHandle, byte[] anchorId);

        NsdkStatus GetAnchorUpdate
        (
            IntPtr providerHandle,
            byte[] anchorId,
            out NsdkVpsAnchorUpdate updateOut
        );

        NsdkStatus GetAnchorPayload
        (
            IntPtr providerHandle,
            byte[] anchorId,
            out IntPtr anchorPayloadPtr,
            out int anchorPayloadSize,
            out IntPtr resourceHandle
        );

        NsdkStatus GetLatestLocalizationRequestRecords(
            IntPtr providerHandle,
            out IntPtr networkRequestRecords,
            out int count,
            out IntPtr handle
        );

        NsdkStatus GetLatestDebuggerLogs(
            IntPtr providerHandle,
            out IntPtr logs,
            out int count,
            out IntPtr handle
        );

        NsdkStatus GetSessionId(IntPtr providerHandle, ref byte[] sessionId);

        TrackingState ConvertTrackingStateToUnity(int nativeTrackingState);

        Vps2AnchorTrackingStateReason ConvertTrackingStateReasonToUnity(int nativeReason);
    }
}
