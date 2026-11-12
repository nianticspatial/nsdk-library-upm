// Copyright Niantic Spatial.

using System;
using System.Runtime.InteropServices;
using NianticSpatial.NSDK.AR.API;

namespace NianticSpatial.NSDK.AR.Subsystems.Vps2.Api
{
    internal interface IApi
    {
        public const int NSDK_VPS2_ANCHOR_ID_SIZE = 32;
        public const int NSDK_VPS2_SESSION_ID_SIZE = 32;
        public const int NSDK_VPS2_ASSET_ID_BUFFER_SIZE = 64;

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

            [MarshalAs(UnmanagedType.U1)]
            public bool aerialLocalizationEnabled;

            public int universalLocalizationRequestTimeoutMs;

            public uint maxRequestsInTransitPerTarget;

            public float anchorDistanceGateMeters;

            [MarshalAs(UnmanagedType.U1)]
            public bool blurDetectionEnabled;

            public float minSharpnessScore;

            [MarshalAs(UnmanagedType.U1)]
            public bool vpsDebuggerSavingFrameImageEnabled;

            public float areaTargetingEnterMarginMeters;

            public float areaTargetingExitMarginMeters;

            [MarshalAs(UnmanagedType.U1)]
            public bool vpsTemporalFusionEnabled;

            // Layout padding: keeps this struct in sync with ARDK_VPS2_Config's nested
            // ARDK_VPS2_InternalConfig sub-struct. Not exposed to Unity callers — those
            // fields are intentionally internal and only the Python and ROS2 SDKs mirror
            // them. Always zero-initialized from Unity.
            private NsdkVps2InternalConfig _internal;
        }

        // Layout mirror of ARDK_VPS2_InternalConfig.
        [StructLayout(LayoutKind.Sequential)]
        private struct NsdkVps2InternalConfig
        {
            private int _wpsPriorMode;
            private float _wpsPriorDistanceThreshold;
            private float _wpsGnssFramerate;
            private float _wpsPoseFramerate;
            private float _bevConfidenceThreshold;

            [MarshalAs(UnmanagedType.U1)]
            private bool _janusMultiviewEnabled;

            [MarshalAs(UnmanagedType.U1)]
            private bool _rejectUpsideDown;

            [MarshalAs(UnmanagedType.U1)]
            private bool _bevFullResolutionEnabled;

            private float _vpsGeoreferenceHoldConfidence;
        }

        // Defined in ardk_vps2_asset_identifier.h
        [StructLayout(LayoutKind.Sequential)]
        public struct NsdkVps2AssetIdentifier
        {
            // NUL-terminated id strings in fixed buffers.
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NSDK_VPS2_ASSET_ID_BUFFER_SIZE)]
            public byte[] siteId;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NSDK_VPS2_ASSET_ID_BUFFER_SIZE)]
            public byte[] assetId;

            // Identifier of subarea in the site localized, or an empty string when the asset has
            // no subareas.
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = NSDK_VPS2_ASSET_ID_BUFFER_SIZE)]
            public byte[] areaId;
        }

        // Defined in ardk_vps2_asset_tracking_data.h
        [StructLayout(LayoutKind.Sequential)]
        public struct NsdkVps2AssetTrackingData
        {
            public NsdkVps2AssetIdentifier asset;

            // Transform from the asset's coordinate space to the device's local AR
            // tracking space, as a column-major 4x4 matrix.
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
            public float[] assetToLocalTrackingTransform;

            public float trackingConfidence;

            public UInt64 timestampMs;

            public NsdkGeolocationData geolocationData;

            public byte hasGeolocation;
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

            // Ids are empty until a visual localization succeeds against a submitted asset.
            public NsdkVps2AssetIdentifier localizedAsset;
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
            public Int32 trackingState;
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

        NsdkStatus LocalizeToSite(IntPtr providerHandle, NsdkString siteId, NsdkString assetId);

        NsdkStatus LocalizeToNearbySites
        (
            IntPtr providerHandle,
            double latitude,
            double longitude,
            double radiusMeters
        );

        // An empty siteId stops localizing against every requested site.
        NsdkStatus StopLocalize(IntPtr providerHandle, NsdkString siteId);

        // assetId is null for the asset the device is currently localized to, or a
        // single-element array naming an asset (marshalled as an optional pointer).
        NsdkStatus GetAssetTrackingData
        (
            IntPtr providerHandle,
            NsdkString[] assetId,
            out NsdkVps2AssetTrackingData trackingData
        );

        NsdkStatus GetDeviceGeolocation(IntPtr providerHandle, HeadingMode headingMode, out NsdkVps2GeolocationData location);

        NsdkStatus GetPose(NsdkVps2Localization localization, NsdkGeolocationData location, out NsdkVps2Pose pose);

        NsdkStatus GetPoseFromLatestLocalization(IntPtr providerHandle, NsdkGeolocationData location, out NsdkVps2Pose pose);

        NsdkStatus GetGeolocationFromLatestLocalization(IntPtr providerHandle, NsdkTransform pose, out NsdkVps2GeolocationData geolocation);

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
    }
}
