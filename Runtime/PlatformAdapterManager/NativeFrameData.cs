// Copyright 2022-2026 Niantic Spatial.
using System;
using System.Runtime.InteropServices;

namespace NianticSpatial.NSDK.AR.PAM
{
    // IMPORTANT
    // If you change this file, please add multiple Unity and NativeC reviewers

    // C# to C struct. Must match frame_data.h
    public enum ImageFormatCEnum : uint
    {
        Unknown = 0,

        // Bi-planar Y'CbCr 4:2:0 format
        // NV12, UV are interleaved into one plane.
        Yuv420_NV12 = 1,

        // Bi-planar Y'CbCr 4:2:0 format
        // NV21, VU are interleaved into one plane.
        Yuv420_NV21 = 2,

        // Tri-planar YUV 8-bit 4:2:0 separate planes
        Yuv420_888 = 3,

        // Single channel with 8 bits per pixel
        OneComponent8 = 4,

        // IEEE754-2008 binary32 float, describing the depth (distance to an object)
        DepthFloat32 = 5,

        // 16-bit unsigned integer, describing the depth (distance to an object) in
        DepthUint16 = 6,

        // Single channel image format with 32 bits per pixel
        OneComponent32 = 7,

        // 4 channel image with 8 bits per channel, describing the color in ARGB order
        ARGB32 = 8,

        // 4 channel image with 8 bits per channel, describing the color in RGBA order
        RGBA32 = 9,

        // 4 channel image with 8 bits per channel, describing the color in BGRA order
        BGRA32 = 10,

        // 3 8-bit unsigned integer channels, describing the color in RGB order
        RGB24 = 11,
    }

    // IMPORTANT – This struct has explicit matching C# and pure-C alignment/padding requirements
    // C# to C struct. Must match ardk_string.h
    [StructLayout(LayoutKind.Sequential)]
    internal struct ArdkStringCStruct
    {
        public IntPtr Data;
        public uint DataSize;
    }

    // IMPORTANT – This struct has explicit matching C# and pure-C alignment/padding requirements
    // C# to C struct. Must match compass.h
    [StructLayout(LayoutKind.Sequential)]
    internal struct CompassDataCStruct
    {
        // Compass data timestamp in milliseconds since epoch
        // Zero, if this data was not captured
        public ulong TimestampMs;

        // Accuracy of heading reading in degrees.
        public float HeadingAccuracy;

        // The heading in degrees relative to the magnetic North Pole.
        public float MagneticHeading;

        // The raw geomagnetic data measured in microteslas.
        public float RawDataX;
        public float RawDataY;
        public float RawDataZ;

        // The heading in degrees relative to the geographic North Pole.
        public float TrueHeading;
    }

    // IMPORTANT – This struct has explicit matching C# and pure-C alignment/padding requirements
    // C# to C struct. Must match ardk_gps_data.h
    [StructLayout(LayoutKind.Sequential)]
    internal struct GpsLocationCStruct
    {
        // GPS data timestamp in milliseconds since epoch
        // Zero, if this data was not captured
        public ulong TimestampMs;

        // GPS location latitude
        public double Latitude;
        // GPS location longitude
        public double Longitude;
        // GPS location altitude
        public double Altitude;

        // GPS vertical accuracy
        public float VerticalAccuracy;
        // GPS horizontal accuracy
        public float HorizontalAccuracy;
    }

    // IMPORTANT – This struct has explicit matching C# and pure-C alignment/padding requirements
    // C# to C struct. Must match frame_data.h
    [StructLayout(LayoutKind.Sequential)]
    internal struct CameraIntrinsicsCStruct
    {
        public float FocalLengthX;
        public float FocalLengthY;
        public float PrincipalPointX;
        public float PrincipalPointY;
        public uint ResolutionX;
        public uint ResolutionY;

        public CameraIntrinsicsCStruct
        (
            UnityEngine.Vector2 focalLength,
            UnityEngine.Vector2 principalPoint,
            UnityEngine.Vector2Int resolution
        )
        {
            FocalLengthX = focalLength.x;
            FocalLengthY = focalLength.y;
            PrincipalPointX = principalPoint.x;
            PrincipalPointY = principalPoint.y;
            ResolutionX = (uint)resolution.x;
            ResolutionY = (uint)resolution.y;
        }

        public void SetIntrinsics
        (
            UnityEngine.Vector2 focalLength,
            UnityEngine.Vector2 principalPoint,
            UnityEngine.Vector2Int resolution
        )
        {
            FocalLengthX = focalLength.x;
            FocalLengthY = focalLength.y;
            PrincipalPointX = principalPoint.x;
            PrincipalPointY = principalPoint.y;
            ResolutionX = (uint)resolution.x;
            ResolutionY = (uint)resolution.y;
        }
    }

    // IMPORTANT – This struct has explicit matching C# and pure-C alignment/padding requirements
    // C# to C struct. Must match frame_data.h
    [StructLayout(LayoutKind.Sequential)]
    internal struct TransformCStruct
    {
        public float TranslationX;
        public float TranslationY;
        public float TranslationZ;
        public float ScaleXYZ;
        public float OrientationX;
        public float OrientationY;
        public float OrientationZ;
        public float OrientationW;

        public void SetTransform(UnityEngine.Matrix4x4 nsdkMatrix)
        {
            UnityEngine.Vector3 translation = nsdkMatrix.GetPosition();
            TranslationX = translation.x;
            TranslationY = translation.y;
            TranslationZ = translation.z;
            // We use the uniform scale from any of the components
            ScaleXYZ = nsdkMatrix.lossyScale.x;

            var orientation = nsdkMatrix.rotation;
            OrientationX = orientation.x;
            OrientationY = orientation.y;
            OrientationZ = orientation.z;
            OrientationW = orientation.w;
        }
    }

    // IMPORTANT – This struct has explicit matching C# and pure-C alignment/padding requirements
    // C# to C struct. Must match frame_data.h
    [StructLayout(LayoutKind.Sequential)]
    internal struct CameraPlaneCStruct
    {
        public void SetImagePlane(NsdkCpuImagePlane plane)
        {
            DataPtr = plane.DataPtr;
            DataSize = plane.DataSize;
            PixelStride = plane.PixelStride;
            RowStride = plane.RowStride;
        }

        public IntPtr DataPtr;
        public uint DataSize;
        public uint PixelStride;
        public uint RowStride;
        private uint Padding;
    }

    // IMPORTANT – This struct has explicit matching C# and pure-C alignment/padding requirements
    // C# to C struct. Must match nsdk_camera_frame.h (NSDK_CameraFrame)
    [StructLayout(LayoutKind.Sequential)]
    internal struct NsdkCameraFrameCStruct
    {
        // Camera pose and image timestamp in milliseconds since epoch
        // Note, this is not strictly the exact timestamp for all devices as of May 2024 (e.g. Magic Leap)
        public ulong CameraTimestampMs;

        // Camera image plane data
        public CameraPlaneCStruct CameraImagePlane0;
        public CameraPlaneCStruct CameraImagePlane1;
        public CameraPlaneCStruct CameraImagePlane2;
        // Camera pose of the current frame as a 4x4 float matrix.
        public TransformCStruct CameraPose;
        // Camera intrinsics
        public CameraIntrinsicsCStruct CameraIntrinsics;
        // The width and height of the raw camera image
        public uint CameraImageWidth;
        public uint CameraImageHeight;

        // Camera image format
        public ImageFormatCEnum CameraImageFormat;

        // A descriptive name for the camera sensor, e.g. "camera_rear"
        public ArdkStringCStruct SensorName;

        // True if this camera sensor should be used as the default for SDK features
        public byte DefaultCameraSensor; // C bool is 1 byte

        // Orientation of this camera image within its buffer. See ardk_orientation.h for definitions.
        // Use XREnumConversions.FromUnityToNsdk() to calculate the correct value
        public uint CameraOrientation;
    }

    // IMPORTANT – This struct has explicit matching C# and pure-C alignment/padding requirements
    // C# to C struct. Must match nsdk_depth_frame.h (NSDK_DepthFrame)
    [StructLayout(LayoutKind.Sequential)]
    internal struct NsdkDepthFrameCStruct
    {
        // Depth frame timestamp in milliseconds since epoch
        public ulong DepthTimestampMs;

        // Platform depth data (float buffer)
        public IntPtr DepthImageData;

        // Platform depth confidence data.
        public IntPtr DepthConfidenceData;

        // Platform depth image intrinsics with image resolution.
        public CameraIntrinsicsCStruct DepthImageIntrinsics;

        // Pose of the depth camera/sensor
        // If depth is requested but unavailable, set this to identity
        public TransformCStruct DepthCameraPose;

        // Length of the depth float buffer (same value as depth confidence buffer as well)
        public uint DepthAndConfidenceDataLength;

        // Width and height of the depth float buffer
        public uint DepthImageDataWidth;
        public uint DepthImageDataHeight;

        // A descriptive name for the camera sensor, e.g. "lidar_rear"
        public ArdkStringCStruct SensorName;

        // Name of the camera sensor this depth is aligned to (if applicable)
        public ArdkStringCStruct AlignedToCameraSensorName;

        // True if this is the default depth sensor
        public byte DefaultDepthSensor; // C bool is 1 byte

        // Orientation of this depth image within its buffer. See ardk_orientation.h for definitions.
        // Use XREnumConversions.FromUnityToNsdk() to calculate the correct value
        public uint CameraOrientation;
    }

    // IMPORTANT – This struct has explicit matching C# and pure-C alignment/padding requirements
    // C# to C struct. Must match ardk_frame_data.h (ARDK_FrameData)
    [StructLayout(LayoutKind.Sequential)]
    internal struct NsdkFrameData
    {
        // Most recent compass data from the device
        // 64b-aligned struct
        public CompassDataCStruct CompassData;

        // Most recent GPS data from the device
        // 64b-aligned struct
        public GpsLocationCStruct GpsLocation;

        // A unique ID to identify the current frame across frames within the same run
        public uint FrameId;

        // Tracking state of current frame. See tracking_state.h for definitions.
        // Use XREnumConversions.FromUnityToNsdk() to calculate the correct value
        public uint TrackingState;

        // Pointer to array of camera frames (NsdkCameraFrameCStruct buffer)
        public IntPtr CameraFrames;
        // The number of frames in the CameraFrames array
        public uint CameraFramesCount;

        // Pointer to array of depth frames (NsdkDepthFrameCStruct buffer)
        public IntPtr DepthFrames;
        // The number of frames in the DepthFrames array
        public uint DepthFramesCount;
    }
}
