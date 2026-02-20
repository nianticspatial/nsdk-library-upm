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

        [StructLayout(LayoutKind.Sequential)]
        public struct NsdkVps2Config
        {
            [MarshalAs(UnmanagedType.U1)]
            public bool bevLocalizationEnabled;

            public float bevRequestsPerSecond;

            [MarshalAs(UnmanagedType.U1)]
            public bool vpsLocalizationEnabled;

            public float initialVpsRequestsPerSecond;
            public float continuousVpsRequestsPerSecond;

            [MarshalAs(UnmanagedType.U1)]
            public bool geolocationSmoothingEnabled;
        }

        // Defined in ardk_vps2_transformer.h
        [StructLayout(LayoutKind.Sequential)]
        public struct NsdkVps2Transformer
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
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] requestIdentifier;

            public int status;
            public int type;
            public int error;
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
            public byte updateType;             // uint8_t in C++
        }

        IntPtr Construct(IntPtr unityContext);

        void Destroy(IntPtr providerHandle);

        NsdkFeatureStatus GetFeatureStatus(IntPtr providerHandle);

        NsdkStatus Start(IntPtr providerHandle);

        NsdkStatus Stop(IntPtr providerHandle);

        NsdkStatus Configure(IntPtr providerHandle, NsdkVps2Config config);

        NsdkStatus GetLatestTransformer(IntPtr providerHandle, out NsdkVps2Transformer transformer);

        NsdkStatus GetGeolocation(NsdkVps2Transformer transformer, NsdkTransform pose, out NsdkVps2GeolocationData location);

        NsdkStatus GetPose(NsdkVps2Transformer transformer, NsdkGeolocationData location, out NsdkVps2Pose pose);

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
            out int anchorPayloadSize
        );

        NsdkStatus GetLatestNetworkRequestRecords(
            IntPtr providerHandle,
            out IntPtr networkRequestRecords,
            out int count,
            out IntPtr handle
        );

        TrackingState ConvertTrackingStateToUnity(int nativeTrackingState);

        TrackingStateReason ConvertTrackingStateReasonToUnity(int nativeReason);
    }
}
