// Copyright 2022-2026 Niantic Spatial.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace NianticSpatial.NSDK.AR.Subsystems.SceneSegmentation
{
    internal interface IApi
    {
        public IntPtr Construct(IntPtr unityContext);

        public void Start(IntPtr nativeProviderHandle);

        public void Stop(IntPtr nativeProviderHandle);

        public void Configure(IntPtr nativeProviderHandle, UInt32 framesPerSecond, UInt32 numThresholds, IntPtr thresholds, HashSet<SceneSegmentationChannel> suppressionMaskChannels);

        public void Destruct(IntPtr nativeProviderHandle);

        public bool TryGetSceneSegmentationChannel
        (
            IntPtr nativeProviderHandle,
            SceneSegmentationChannel channel,
            XRCameraParams? cameraParams,
            Matrix4x4? targetPose,
            out XRTextureDescriptor sceneSegmentationChannelDescriptor,
            out Matrix4x4 samplerMatrix
        );

        public bool TryAcquireSceneSegmentationChannelCpuImage
        (
            IntPtr nativeProviderHandle,
            SceneSegmentationChannel channel,
            XRCameraParams? cameraParams,
            Matrix4x4? targetPose,
            out XRCpuImage cpuImage,
            out Matrix4x4 samplerMatrix
        );

        public bool TryGetPackedSceneSegmentationChannels
        (
            IntPtr nativeProviderHandle,
            XRCameraParams? cameraParams,
            Matrix4x4? targetPose,
            out XRTextureDescriptor packedSceneSegmentationDescriptor,
            out Matrix4x4 samplerMatrix
        );

        public bool TryAcquirePackedSceneSegmentationChannelsCpuImage
        (
            IntPtr nativeProviderHandle,
            XRCameraParams? cameraParams,
            Matrix4x4? targetPose,
            out XRCpuImage cpuImage,
            out Matrix4x4 samplerMatrix
        );

        public bool TryGetSuppressionMaskTexture
        (
            IntPtr nativeProviderHandle,
            XRCameraParams? cameraParams,
            Matrix4x4? targetPose,
            out XRTextureDescriptor suppressionMaskDescriptor,
            out Matrix4x4 samplerMatrix
        );

        public bool TryAcquireSuppressionMaskCpuImage
        (
            IntPtr nativeProviderHandle,
            XRCameraParams? cameraParams,
            Matrix4x4? targetPose,
            out XRCpuImage cpuImage,
            out Matrix4x4 samplerMatrix
        );

        public List<SceneSegmentationChannel> GetChannels();

        public bool TryGetLatestFrameId(IntPtr nativeProviderHandle, out uint frameId);

        public bool TryGetLatestIntrinsicsMatrix(IntPtr nativeProviderHandle, out Matrix4x4 intrinsicsMatrix);

        public UInt32 GetFlags(IEnumerable<SceneSegmentationChannel> channels);
    }
}
