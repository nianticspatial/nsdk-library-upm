// Copyright 2022-2026 Niantic Spatial.
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using NianticSpatial.NSDK.AR.Core;
using NianticSpatial.NSDK.AR.Occlusion;
using NianticSpatial.NSDK.AR.Subsystems.Common;
using NianticSpatial.NSDK.AR.Utilities;
using NianticSpatial.NSDK.AR.Utilities.Textures;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARSubsystems;

namespace NianticSpatial.NSDK.AR.Subsystems.Semantics
{
    /// <summary>
    /// Represents the different semantic segmentation channels that can be detected.
    /// This mirrors the native ARDK_Semantics_Channel enum.
    /// </summary>
    public enum SemanticsChannel : byte
    {
        Sky = 0,
        Ground = 1,
        NaturalGround = 2,
        ArtificialGround = 3,
        Water = 4,
        Person = 5,
        Building = 6,
        Foliage = 7,
        Grass = 8,

        // Experimental channels start from 100
        FlowerExperimental = 100,
        TreeTrunkExperimental = 101,
        PetExperimental = 102,
        SandExperimental = 103,
        TvExperimental = 104,
        DirtExperimental = 105,
        VehicleExperimental = 106,
        FoodExperimental = 107,
        LoungeableExperimental = 108,
        SnowExperimental = 109
    }

    internal class NativeApi : IApi
    {
        private const int FramesInMemory = 2;
        private const string TextureSemanticChannelPropertyPrefix = "_SemanticChannel_";
        private const string TexturePackedSemanticChannelPropertyName = "_PackedSemanticChannels";

        private static readonly List<SemanticsChannel> AllChannels = new List<SemanticsChannel>
        {
            SemanticsChannel.Sky,
            SemanticsChannel.Ground,
            SemanticsChannel.NaturalGround,
            SemanticsChannel.ArtificialGround,
            SemanticsChannel.Water,
            SemanticsChannel.Person,
            SemanticsChannel.Building,
            SemanticsChannel.Foliage,
            SemanticsChannel.Grass,
            SemanticsChannel.FlowerExperimental,
            SemanticsChannel.TreeTrunkExperimental,
            SemanticsChannel.PetExperimental,
            SemanticsChannel.SandExperimental,
            SemanticsChannel.TvExperimental,
            SemanticsChannel.DirtExperimental,
            SemanticsChannel.VehicleExperimental,
            SemanticsChannel.FoodExperimental,
            SemanticsChannel.LoungeableExperimental,
            SemanticsChannel.SnowExperimental
        };

        private readonly Dictionary<SemanticsChannel, int> _semanticsChannelToShaderPropertyIdMap = new ();
        private readonly Dictionary<SemanticsChannel, BufferedTextureCache> _semanticsBufferedTextureCaches = new ();

        private readonly int _packedSemanticsPropertyNameID = Shader.PropertyToID(TexturePackedSemanticChannelPropertyName);
        private readonly int _suppressionMaskPropertyNameID = Shader.PropertyToID("_SuppressionMask");
        private BufferedTextureCache _packedSemanticsBufferedTextureCache = new BufferedTextureCache(FramesInMemory);
        private BufferedTextureCache _suppressionMaskBufferedTextureCache = new BufferedTextureCache(FramesInMemory);
        private ulong _latestTimestampMs;

        /// <summary>
        /// The CPU image API for interacting with the environment depth image.
        /// </summary>
        private NsdkCpuImageApi _cpuImageApi => NsdkCpuImageApi.Instance;

        public IntPtr Construct(IntPtr unityContext)
        {
            // Initialize metadata dependencies immediately since enum values are always available
            SetMetadataDependencies();
            return Native.Construct(unityContext);
        }

        public void Start(IntPtr nativeProviderHandle)
        {
            Native.Start(nativeProviderHandle);
        }

        public void Stop(IntPtr nativeProviderHandle)
        {
            Native.Stop(nativeProviderHandle);

            ResetMetadataDependencies();
        }

        /// <summary>
        /// Set the configuration for the currently-loaded Lightship semantic segmentation model
        /// </summary>
        /// <param name="nativeProviderHandle"> The handle to the semantics native API. </param>
        /// <param name="framesPerSecond"> The frame rate to run the model at in frames per second. </param>
        /// <param name="numThresholds"> The number of elements in thresholds. This is expected to be the same as the
        /// number of channels in the <c>SemanticsChannel</c> enum. </param>
        /// <param name="thresholds"> An array of float values. Each index in the array corresponds to a channel in the
        /// <c>SemanticsChannel</c> enum. A negative value will have no effect and will
        /// leave the threshold at the default or previously set value. A new threshold setting must be between 0 and
        /// 1, inclusive.</param>
        public void Configure(IntPtr nativeProviderHandle, UInt32 framesPerSecond, UInt32 numThresholds, IntPtr thresholds, HashSet<SemanticsChannel> suppressionMaskChannels)
        {
            // TODO(rbarnes): expose semantics mode in Lightship settings UI
            Native.Configure(nativeProviderHandle, framesPerSecond, numThresholds, thresholds, 0, GetFlags(suppressionMaskChannels));
        }

        public void Destruct(IntPtr nativeProviderHandle)
        {
            Native.Destruct(nativeProviderHandle);
        }

        /// <summary>
        /// Gets the texture descriptor for the named semantic channel.
        /// </summary>
        /// <param name="nativeProviderHandle"> The handle to the semantics native API </param>
        /// <param name="channelName"> The name of the semantic channel whose texture we want to get </param>
        /// <param name="cameraParams">Describes the viewport. If this is null, the samplerMatrix will result in an identity matrix.</param>
        /// <param name="targetPose">
        /// Any image acquired has been captured in the past. The target pose argument defines the pose the image
        /// needs to synchronize with. If the image can be synchronized, the <see cref="samplerMatrix"/> will be
        /// calibrated to warp the image as if it was taken from the target pose. If this argument is null, the
        /// image will not be warped.
        /// </param>
        /// <param name="semanticsChannelDescriptor"> The semantic channel texture descriptor to be populated, if available. </param>
        /// <param name="samplerMatrix">The matrix that converts from normalized viewport coordinates to normalized image coordinates.</param>
        /// <returns>
        /// <c>true</c> if the semantic channel texture descriptor is available and is returned. Otherwise,
        /// <c>false</c>.
        /// </returns>
        public bool TryGetSemanticChannel
        (
            IntPtr nativeProviderHandle,
            SemanticsChannel channel,
            XRCameraParams? cameraParams,
            Matrix4x4? targetPose,
            out XRTextureDescriptor semanticsChannelDescriptor,
            out Matrix4x4 samplerMatrix
        )
        {
            semanticsChannelDescriptor = default;
            samplerMatrix = Matrix4x4.identity;
            var resourceHandle =
                Native.GetSemanticsChannel
                (
                    nativeProviderHandle,
                    channel,
                    out IntPtr memoryBuffer,
                    out int size,
                    out int width,
                    out int height,
                    out TextureFormat format,
                    out uint frameId,
                    out _latestTimestampMs
                );

            if (resourceHandle == IntPtr.Zero || memoryBuffer == IntPtr.Zero)
                return false;

            var texture =
                _semanticsBufferedTextureCaches[channel].GetUpdatedTextureFromBuffer
                (
                    memoryBuffer,
                    size,
                    width,
                    height,
                    format,
                    frameId
                );

            samplerMatrix = GetSamplerMatrix(nativeProviderHandle, resourceHandle, cameraParams, targetPose, width, height);

            Native.DisposeResource(nativeProviderHandle, resourceHandle);

            // Package results
            semanticsChannelDescriptor = new XRTextureDescriptor
            (
                texture.GetNativeTexturePtr(),
                width,
                height,
                0,
                format,
                propertyNameId: _semanticsChannelToShaderPropertyIdMap[channel],
                depth: 0,
                dimension: TextureDimension.Tex2D
            );

            return true;
        }

        /// <summary>
        /// Get the XRCpuImage for the specified semantic channel.
        /// </summary>
        /// <param name="nativeProviderHandle"> The handle to the semantics native API </param>
        /// <param name="channelName"> The name of the semantic channel whose texture we want to get </param>
        /// <param name="cameraParams">Describes the viewport the image is to be displayed on.</param>
        /// <param name="targetPose">
        /// Any image acquired has been captured in the past. The target pose argument defines the pose the image
        /// needs to synchronize with. If the image can be synchronized, the <see cref="samplerMatrix"/> will be
        /// calibrated to warp the image as if it was taken from the target pose. If this argument is null, the
        /// image will not be warped.
        /// </param>
        /// <param name="cpuImage">If this method returns `true`, an acquired <see cref="XRCpuImage"/>. The XRCpuImage
        /// must be disposed by the caller.</param>
        /// <param name="samplerMatrix">
        /// A matrix that converts from normalized viewport coordinates to normalized texture coordinates.
        /// </param>
        /// <returns>Whether acquiring the XRCpuImage was successful.</returns>
        public bool TryAcquireSemanticChannelCpuImage
        (
            IntPtr nativeProviderHandle,
            SemanticsChannel channel,
            XRCameraParams? cameraParams,
            Matrix4x4? targetPose,
            out XRCpuImage cpuImage,
            out Matrix4x4 samplerMatrix
        )
        {
            cpuImage = default;
            samplerMatrix = Matrix4x4.identity;
            var resourceHandle =
                Native.GetSemanticsChannel
                (
                    nativeProviderHandle,
                    channel,
                    out var memoryBuffer,
                    out var size,
                    out var width,
                    out var height,
                    out var format,
                    out var frameId,
                    out _latestTimestampMs
                );

            if (resourceHandle == IntPtr.Zero || memoryBuffer == IntPtr.Zero)
                return false;

            var gotCpuImage =
                _cpuImageApi.TryAddManagedXRCpuImage
                (
                    memoryBuffer,
                    size,
                    width,
                    height,
                    format,
                    _latestTimestampMs,
                    out var cinfo
                );

            samplerMatrix = GetSamplerMatrix(nativeProviderHandle, resourceHandle, cameraParams, targetPose, width, height);

            Native.DisposeResource(nativeProviderHandle, resourceHandle);

            if (gotCpuImage)
            {
                cpuImage = new XRCpuImage(_cpuImageApi, cinfo);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get the texture descriptor for the packed semantics.
        /// </summary>
        /// <param name="nativeProviderHandle"> The handle to the semantics native API </param>
        /// <param name="cameraParams">Describes the viewport the texture is to be displayed on.</param>
        /// <param name="targetPose">
        /// Any image acquired has been captured in the past. The target pose argument defines the pose the image
        /// needs to synchronize with. If the image can be synchronized, the <see cref="samplerMatrix"/> will be
        /// calibrated to warp the image as if it was taken from the target pose. If this argument is null, the
        /// image will not be warped.
        /// </param>
        /// <param name="packedSemanticsDescriptor"> The packed semantics texture descriptor to be populated, if available. </param>
        /// <param name="samplerMatrix">
        /// A matrix that converts from normalized viewport coordinates to normalized texture coordinates.
        /// </param>
        /// <returns>
        /// <c>true</c> if the packed semantics texture descriptor is available and is returned. Otherwise,
        /// <c>false</c>.
        /// </returns>
        public bool TryGetPackedSemanticChannels
        (
            IntPtr nativeProviderHandle,
            XRCameraParams? cameraParams,
            Matrix4x4? targetPose,
            out XRTextureDescriptor packedSemanticsDescriptor,
            out Matrix4x4 samplerMatrix
        )
        {
            packedSemanticsDescriptor = default;
            samplerMatrix = Matrix4x4.identity;

            var resourceHandle =
                Native.GetPackedSemanticsChannels
                (
                    nativeProviderHandle,
                    out var memoryBuffer,
                    out var size,
                    out var width,
                    out var height,
                    out var format,
                    out var frameId,
                    out _latestTimestampMs
                );

            if (resourceHandle == IntPtr.Zero || memoryBuffer == IntPtr.Zero)
                return false;

            var texture = _packedSemanticsBufferedTextureCache.GetUpdatedTextureFromBuffer
            (
                memoryBuffer,
                size,
                width,
                height,
                format,
                frameId
            );

            samplerMatrix = GetSamplerMatrix(nativeProviderHandle, resourceHandle, cameraParams, targetPose, width, height);

            Native.DisposeResource(nativeProviderHandle, resourceHandle);

            packedSemanticsDescriptor = new XRTextureDescriptor
            (
                texture.GetNativeTexturePtr(),
                width,
                height,
                0,
                format,
                _packedSemanticsPropertyNameID,
                0,
                TextureDimension.Tex2D
            );

            return true;
        }

        /// <summary>
        /// Acquire the XRCpuImage for the packed semantic channels.
        /// </summary>
        /// <param name="nativeProviderHandle"> The handle to the semantics native API </param>
        /// <param name="cameraParams">Describes the viewport the image is to be displayed on.</param>
        /// <param name="targetPose">
        /// Any image acquired has been captured in the past. The target pose argument defines the pose the image
        /// needs to synchronize with. If the image can be synchronized, the <see cref="samplerMatrix"/> will be
        /// calibrated to warp the image as if it was taken from the target pose. If this argument is null, the
        /// image will not be warped.
        /// </param>
        /// <param name="cpuImage">The resulting <see cref="XRCpuImage"/>. The XRCpuImage must be disposed by the caller.</param>
        /// <param name="samplerMatrix">
        /// A matrix that converts from normalized viewport coordinates to normalized texture coordinates.
        /// </param>
        /// <returns>Whether the XRCpuImage was successfully acquired.</returns>
        public bool TryAcquirePackedSemanticChannelsCpuImage
        (
            IntPtr nativeProviderHandle,
            XRCameraParams? cameraParams,
            Matrix4x4? targetPose,
            out XRCpuImage cpuImage,
            out Matrix4x4 samplerMatrix
        )
        {
            cpuImage = default;
            samplerMatrix = Matrix4x4.identity;

            var resourceHandle =
                Native.GetPackedSemanticsChannels
                (
                    nativeProviderHandle,
                    out var memoryBuffer,
                    out var size,
                    out var width,
                    out var height,
                    out var format,
                    out var frameId,
                    out _latestTimestampMs
                );

            if (resourceHandle == IntPtr.Zero || memoryBuffer == IntPtr.Zero)
                return false;

            var gotCpuImage =
                _cpuImageApi.TryAddManagedXRCpuImage
                (
                    memoryBuffer,
                    size,
                    width,
                    height,
                    format,
                    _latestTimestampMs,
                    out var cinfo
                );

            samplerMatrix = GetSamplerMatrix(nativeProviderHandle, resourceHandle, cameraParams, targetPose, width, height);

            Native.DisposeResource(nativeProviderHandle, resourceHandle);

            if (gotCpuImage)
            {
                cpuImage = new XRCpuImage(_cpuImageApi, cinfo);
                return true;
            }

            return true;
        }

        public bool TryGetSuppressionMaskTexture
        (
            IntPtr nativeProviderHandle,
            XRCameraParams? cameraParams,
            Matrix4x4? targetPose,
            out XRTextureDescriptor suppressionMaskDescriptor,
            out Matrix4x4 samplerMatrix
        )
        {
            suppressionMaskDescriptor = default;
            samplerMatrix = Matrix4x4.identity;

            var resourceHandle =
                Native.GetSuppressionMask
                (
                    nativeProviderHandle,
                    out var memoryBuffer,
                    out var size,
                    out var width,
                    out var height,
                    out var format,
                    out var frameId,
                    out _latestTimestampMs
                );

            if (resourceHandle == IntPtr.Zero || memoryBuffer == IntPtr.Zero)
            {
                return false;
            }

            var texture = _suppressionMaskBufferedTextureCache.GetUpdatedTextureFromBuffer
            (
                memoryBuffer,
                size,
                width,
                height,
                format,
                frameId
            );

            // Calculate the appropriate viewport mapping
            samplerMatrix = GetSamplerMatrix(nativeProviderHandle, resourceHandle, cameraParams, targetPose, width, height);

            // Release the native data
            Native.DisposeResource(nativeProviderHandle, resourceHandle);

            suppressionMaskDescriptor = new XRTextureDescriptor
            (
                texture.GetNativeTexturePtr(),
                width,
                height,
                0,
                format,
                _suppressionMaskPropertyNameID,
                depth: 0,
                dimension: TextureDimension.Tex2D
            );

            return true;
        }

        /// <summary>
        /// Acquire the XRCpuImage for the semantic suppression mask.
        /// </summary>
        /// <param name="nativeProviderHandle"> The handle to the semantics native API </param>
        /// <param name="cameraParams">Describes the viewport the image is to be displayed on.</param>
        /// <param name="targetPose">
        /// Any image acquired has been captured in the past. The target pose argument defines the pose the image
        /// needs to synchronize with. If the image can be synchronized, the <see cref="samplerMatrix"/> will be
        /// calibrated to warp the image as if it was taken from the target pose. If this argument is null, the
        /// image will not be warped.
        /// </param>
        /// <param name="cpuImage">If this method returns `true`, an acquired <see cref="XRCpuImage"/>. The XRCpuImage
        /// must be disposed by the caller.</param>
        /// <param name="samplerMatrix">
        /// A matrix that transforms from normalized viewport coordinates to normalized texture coordinates.
        /// </param>
        /// <returns>Whether the XRCpuImage was successfully acquired.</returns>
        public bool TryAcquireSuppressionMaskCpuImage
        (
            IntPtr nativeProviderHandle,
            XRCameraParams? cameraParams,
            Matrix4x4? targetPose,
            out XRCpuImage cpuImage,
            out Matrix4x4 samplerMatrix
        )
        {
            cpuImage = default;
            samplerMatrix = Matrix4x4.identity;

            var resourceHandle =
                Native.GetSuppressionMask
                (
                    nativeProviderHandle,
                    out var memoryBuffer,
                    out var size,
                    out var width,
                    out var height,
                    out var format,
                    out uint _,
                    out _latestTimestampMs
                );

            if (resourceHandle == IntPtr.Zero || memoryBuffer == IntPtr.Zero)
            {
                return false;
            }

            var gotCpuImage =
                _cpuImageApi.TryAddManagedXRCpuImage
                (
                    memoryBuffer,
                    size,
                    width,
                    height,
                    format,
                    _latestTimestampMs,
                    out var cinfo
                );

            samplerMatrix = GetSamplerMatrix(nativeProviderHandle, resourceHandle, cameraParams, targetPose, width, height);

            Native.DisposeResource(nativeProviderHandle, resourceHandle);

            if (gotCpuImage)
            {
                cpuImage = new XRCpuImage(_cpuImageApi, cinfo);
                return true;
            }

            return false;
        }

        public List<SemanticsChannel> GetChannels()
        {
            // Return all enum values without querying the native API
            return new List<SemanticsChannel>(AllChannels);
        }

        public bool TryGetLatestFrameId(IntPtr nativeProviderHandle, out uint frameId)
        {
            return Native.TryGetLatestFrameId(nativeProviderHandle, out frameId);
        }

        public bool TryGetLatestIntrinsicsMatrix(IntPtr nativeProviderHandle, out Matrix4x4 intrinsicsMatrix)
        {
            float[] intrinsics = new float[9];
            bool gotIntrinsics = Native.TryGetLatestIntrinsics(nativeProviderHandle, intrinsics);

            if (!gotIntrinsics)
            {
                intrinsicsMatrix = default;
                return false;
            }

            intrinsicsMatrix = new Matrix4x4
            (
                new Vector4(intrinsics[0], intrinsics[1], intrinsics[2], 0),
                new Vector4(intrinsics[3], intrinsics[4], intrinsics[5], 0),
                new Vector4(intrinsics[6], intrinsics[7], intrinsics[8], 0),
                new Vector4(0, 0, 0, 1)
            );
            return true;
        }

        public uint GetFlags(IEnumerable<SemanticsChannel> channels)
        {
            if (channels == null)
            {
                return 0u;
            }

            uint flags = 0u;
            const int bitsPerPixel = sizeof(UInt32) * 8;

            UInt32 GetChannelTextureMask(int channelIndex)
            {
                if (channelIndex is < 0 or >= bitsPerPixel)
                    return 0u;

                return 1u << (bitsPerPixel - 1 - channelIndex);
            }

            foreach (var channel in channels)
            {
                int cIdx = (int)channel;
                flags |= GetChannelTextureMask(cIdx);
            }

            return flags;
        }

        private void SetMetadataDependencies()
        {
            // Use all enum values to initialize metadata dependencies
            foreach (SemanticsChannel channel in Enum.GetValues(typeof(SemanticsChannel)))
            {
                // Convert enum to string representation (e.g., Sky -> "sky", NaturalGround -> "natural_ground")
                // for shader property names
                string channelName = GetChannelNameFromEnum(channel);

                _semanticsChannelToShaderPropertyIdMap[channel] =
                    Shader.PropertyToID(TextureSemanticChannelPropertyPrefix + channelName);

                _semanticsBufferedTextureCaches[channel] =
                    new BufferedTextureCache(FramesInMemory);
            }
        }

        /// <summary>
        /// Converts a SemanticsChannel enum value to its string representation.
        /// </summary>
        /// <param name="channel">The SemanticsChannel enum value.</param>
        /// <returns>The string representation of the channel (e.g., "sky", "ground", "natural_ground").</returns>
        private static string GetChannelNameFromEnum(SemanticsChannel channel)
        {
            switch (channel)
            {
                case SemanticsChannel.Sky:
                    return "sky";
                case SemanticsChannel.Ground:
                    return "ground";
                case SemanticsChannel.NaturalGround:
                    return "natural_ground";
                case SemanticsChannel.ArtificialGround:
                    return "artificial_ground";
                case SemanticsChannel.Water:
                    return "water";
                case SemanticsChannel.Person:
                    return "person";
                case SemanticsChannel.Building:
                    return "building";
                case SemanticsChannel.Foliage:
                    return "foliage";
                case SemanticsChannel.Grass:
                    return "grass";
                case SemanticsChannel.FlowerExperimental:
                    return "flower_experimental";
                case SemanticsChannel.TreeTrunkExperimental:
                    return "tree_trunk_experimental";
                case SemanticsChannel.PetExperimental:
                    return "pet_experimental";
                case SemanticsChannel.SandExperimental:
                    return "sand_experimental";
                case SemanticsChannel.TvExperimental:
                    return "tv_experimental";
                case SemanticsChannel.DirtExperimental:
                    return "dirt_experimental";
                case SemanticsChannel.VehicleExperimental:
                    return "vehicle_experimental";
                case SemanticsChannel.FoodExperimental:
                    return "food_experimental";
                case SemanticsChannel.LoungeableExperimental:
                    return "loungeable_experimental";
                case SemanticsChannel.SnowExperimental:
                    return "snow_experimental";
                default:
                    return channel.ToString().ToLowerInvariant();
            }
        }

        /// <summary>
        /// Converts a channel name string to a SemanticsChannel enum value.
        /// </summary>
        /// <param name="channelName">The channel name string (e.g., "sky", "ground", "natural_ground").</param>
        /// <param name="channel">The corresponding SemanticsChannel enum value if found.</param>
        /// <returns>True if the channel name was successfully parsed, false otherwise.</returns>
        private static bool TryParseChannelName(string channelName, out SemanticsChannel channel)
        {
            // Normalize the channel name for comparison (handle both snake_case and PascalCase)
            string normalized = channelName.ToLowerInvariant().Replace("_", "");

            // Map normalized names to enum values
            switch (normalized)
            {
                case "sky":
                    channel = SemanticsChannel.Sky;
                    return true;
                case "ground":
                    channel = SemanticsChannel.Ground;
                    return true;
                case "naturalground":
                    channel = SemanticsChannel.NaturalGround;
                    return true;
                case "artificialground":
                    channel = SemanticsChannel.ArtificialGround;
                    return true;
                case "water":
                    channel = SemanticsChannel.Water;
                    return true;
                case "person":
                    channel = SemanticsChannel.Person;
                    return true;
                case "building":
                    channel = SemanticsChannel.Building;
                    return true;
                case "foliage":
                    channel = SemanticsChannel.Foliage;
                    return true;
                case "grass":
                    channel = SemanticsChannel.Grass;
                    return true;
                case "flowerexperimental":
                    channel = SemanticsChannel.FlowerExperimental;
                    return true;
                case "treetrunkexperimental":
                    channel = SemanticsChannel.TreeTrunkExperimental;
                    return true;
                case "petexperimental":
                    channel = SemanticsChannel.PetExperimental;
                    return true;
                case "sandexperimental":
                    channel = SemanticsChannel.SandExperimental;
                    return true;
                case "tvexperimental":
                    channel = SemanticsChannel.TvExperimental;
                    return true;
                case "dirtexperimental":
                    channel = SemanticsChannel.DirtExperimental;
                    return true;
                case "vehicleexperimental":
                    channel = SemanticsChannel.VehicleExperimental;
                    return true;
                case "foodexperimental":
                    channel = SemanticsChannel.FoodExperimental;
                    return true;
                case "loungeableexperimental":
                    channel = SemanticsChannel.LoungeableExperimental;
                    return true;
                case "snowexperimental":
                    channel = SemanticsChannel.SnowExperimental;
                    return true;
                default:
                    channel = SemanticsChannel.Sky; // Default fallback
                    return false;
            }
        }

        private void ResetMetadataDependencies()
        {
            _packedSemanticsBufferedTextureCache?.Dispose();
            _suppressionMaskBufferedTextureCache?.Dispose();

            foreach (var bufferedTextureCache in _semanticsBufferedTextureCaches.Values)
                bufferedTextureCache.Dispose();


            _semanticsBufferedTextureCaches.Clear();
            _semanticsChannelToShaderPropertyIdMap.Clear();

            // Reinitialize metadata dependencies since enum values are always available
            SetMetadataDependencies();
        }

        /// <summary>
        /// Calculates a matrix that transforms from normalized coordinates of the
        /// specified viewport to normalized coordinates of the semantics image.
        /// </summary>
        /// <param name="nativeProviderHandle">The handle to the native semantics provider instance.</param>
        /// <param name="resourceHandle">The handle to the native image resource.</param>
        /// <param name="cameraParams">The viewport to calculate the mapping for.</param>
        /// <param name="targetPose">
        /// When provided, the resulting matrix will contain a warping transformation to make it seem like
        /// the image was taken from this camera pose.
        /// </param>
        /// <param name="imageWidth">The width of the image.</param>
        /// <param name="imageHeight">The height of the image.</param>
        /// <returns>The matrix that can be used to fit the image to the viewport.</returns>
        private static Matrix4x4 GetSamplerMatrix
        (
            IntPtr nativeProviderHandle,
            IntPtr resourceHandle,
            XRCameraParams? cameraParams,
            Matrix4x4? targetPose,
            int imageWidth,
            int imageHeight
        )
        {
            Matrix4x4 result;

            // If the camera params are not provided, default to the image container
            var viewport = cameraParams ?? new XRCameraParams
            {
                screenWidth = imageWidth,
                screenHeight = imageHeight,
                screenOrientation = ScreenOrientation.LandscapeLeft
            };

            if (targetPose.HasValue)
            {
                // Display + interpolation
                TryCalculateSamplerMatrix
                (
                    nativeProviderHandle,
                    resourceHandle,
                    viewport,
                    targetPose.Value,
                    XRDisplayContext.OccludeeEyeDepth,
                    out result
                );
            }
            else
            {
                // Display
                result =
                    CameraMath.CalculateDisplayMatrix
                    (
                        imageWidth,
                        imageHeight,
                        (int)viewport.screenWidth,
                        (int)viewport.screenHeight,
                        viewport.screenOrientation,
                        invertVertically: true
                    );
            }

            return result;
        }

        /// <summary>
        /// Calculates a 3x3 transformation matrix that when applied to the image,
        /// aligns its pixels such that the image was taken from the specified pose.
        /// </summary>
        /// <param name="nativeProviderHandle">The handle to the semantics native API </param>
        /// <param name="resourceHandle">The handle to the semantics buffer resource.</param>
        /// <param name="cameraParams">Describes the viewport.</param>
        /// <param name="pose">The camera pose the image needs to align with.</param>
        /// <param name="backProjectionPlane">The distance from the camera to the plane that
        /// the image should be projected onto (in meters).</param>
        /// <param name="result"></param>
        /// <returns>True, if the matrix could be calculated, otherwise false (in case the </returns>
        private static bool TryCalculateSamplerMatrix
        (
            IntPtr nativeProviderHandle,
            IntPtr resourceHandle,
            XRCameraParams cameraParams,
            Matrix4x4 pose,
            float backProjectionPlane,
            out Matrix4x4 result
        )
        {
            var outMatrix = new float[9];
            var poseArray = MatrixConversionHelper.Matrix4x4ToInternalArray(pose.FromUnityToNsdk());

            var gotInterpolationMatrix =
                Native.CalculateInterpolationMatrix
                (
                    nativeProviderHandle,
                    resourceHandle,
                    (int)cameraParams.screenWidth,
                    (int)cameraParams.screenHeight,
                    cameraParams.screenOrientation.FromUnityToNsdk(),
                    poseArray,
                    backProjectionPlane,
                    outMatrix
                );

            if (gotInterpolationMatrix)
            {
                result = new Matrix4x4
                (
                    new Vector4(outMatrix[0], outMatrix[1], outMatrix[2], 0),
                    new Vector4(outMatrix[3], outMatrix[4], outMatrix[5], 0),
                    new Vector4(outMatrix[6], outMatrix[7], outMatrix[8], 0),
                    new Vector4(0, 0, 0, 1)
                );

                return true;
            }

            Log.Warning("Interpolation matrix for semantic prediction could not be calculated.");
            result = Matrix4x4.identity;
            return false;
        }

        private static class Native
        {
            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_SemanticsProvider_Construct")]
            public static extern IntPtr Construct(IntPtr unityContext);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_SemanticsProvider_Start")]
            public static extern void Start(IntPtr nativeProviderHandle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_SemanticsProvider_Stop")]
            public static extern void Stop(IntPtr nativeProviderHandle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_SemanticsProvider_Configure")]
            public static extern void Configure(IntPtr nativeProviderHandle, UInt32 framesPerSecond, UInt32 numThresholds, IntPtr thresholds, byte mode, UInt32 suppressionMaskChannels);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_SemanticsProvider_Destruct")]
            public static extern void Destruct(IntPtr nativeProviderHandle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_SemanticsProvider_GetSemanticsChannel")]
            public static extern IntPtr GetSemanticsChannel
            (
                IntPtr nativeProviderHandle,
                SemanticsChannel channel,
                out IntPtr memoryBuffer,
                out int size,
                out int width,
                out int height,
                out TextureFormat format,
                out uint frameId,
                out ulong timestamp
            );

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_SemanticsProvider_GetPackedSemanticsChannel")]
            public static extern IntPtr GetPackedSemanticsChannels
            (
                IntPtr nativeProviderHandle,
                out IntPtr memoryBuffer,
                out int size,
                out int width,
                out int height,
                out TextureFormat format,
                out uint frameId,
                out ulong timestamp
            );

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_SemanticsProvider_GetSuppressionMask")]
            public static extern IntPtr GetSuppressionMask
            (
                IntPtr nativeProviderHandle,
                out IntPtr memoryBuffer,
                out int size,
                out int width,
                out int height,
                out TextureFormat format,
                out uint frameId,
                out ulong timestamp
            );

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_SemanticsProvider_CalculateSamplerMatrix")]
            public static extern bool CalculateInterpolationMatrix
            (
                IntPtr nativeProviderHandle,
                IntPtr nativeResourceHandle,
                int viewportWidth,
                int viewportHeight,
                uint orientation,
                float[] poseMatrix,
                float backProjectionPlane,
                float[] outMatrix3X3
            );

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_SemanticsProvider_ReleaseResource")]
            public static extern IntPtr DisposeResource(IntPtr nativeProviderHandle, IntPtr resourceHandle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_SemanticsProvider_TryGetLatestFrameId")]
            public static extern bool TryGetLatestFrameId(IntPtr nativeProviderHandle, out uint frameId);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_SemanticsProvider_TryGetLatestIntrinsics")]
            public static extern bool TryGetLatestIntrinsics(IntPtr nativeProviderHandle, float[] intrinsics);
        }
    }
}
