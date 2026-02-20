// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using NianticSpatial.NSDK.AR.Common;
using NianticSpatial.NSDK.AR.Subsystems.Semantics;
using NianticSpatial.NSDK.AR.Utilities;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using NianticSpatial.NSDK.AR.Utilities.Textures;
using NianticSpatial.NSDK.AR.XRSubsystems;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace NianticSpatial.NSDK.AR.Semantics
{
    /// <summary>
    /// The <c>ARSemanticSegmentationManager</c> controls the <c>XRSemanticsSubsystem</c> and updates the semantics
    /// textures on each Update loop. Textures and XRCpuImages are available for confidence maps of individual semantic
    /// segmentation channels and a bit array indicating which semantic channels have surpassed the chosen confidence
    /// threshold per pixel. For cases where a semantic segmentation texture is overlaid on the screen, utilities are
    /// provided to read semantic properties at a given point on the screen.
    /// </summary>
    [PublicAPI("apiref/Niantic/Lightship/AR/Semantics/ARSemanticSegmentationManager/")]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(NsdkARUpdateOrder.SemanticSegmentationManager)]
    public class ARSemanticSegmentationManager :
        SubsystemLifecycleManager<XRSemanticsSubsystem, XRSemanticsSubsystemDescriptor, XRSemanticsSubsystem.Provider>
    {
        [SerializeField]
        [Tooltip("Frame rate that semantic segmentation inference will aim to run at")]
        [Range(1, 90)]
        private uint _targetFrameRate = NsdkSemanticsSubsystem.MaxRecommendedFrameRate;

        /// <summary>
        /// Frame rate that semantic segmentation inference will aim to run at.
        /// </summary>
        public uint TargetFrameRate
        {
            get => subsystem?.TargetFrameRate ?? _targetFrameRate;
            set
            {
                if (value <= 0)
                {
                    Log.Error("Target frame rate value must be greater than zero.");
                    return;
                }

                _targetFrameRate = value;
                if (subsystem != null)
                {
                    subsystem.TargetFrameRate = value;
                }
            }
        }

        /// <summary>
        /// The semantic channels that the current model is able to detect.
        /// </summary>
        public IReadOnlyList<SemanticsChannel> Channels => _readOnlyChannels;

        private IReadOnlyList<SemanticsChannel> _readOnlyChannels;


        /// <summary>
        /// The indices of the semantic channels that the current model is able to detect.
        /// </summary>
        public IReadOnlyDictionary<SemanticsChannel, int> ChannelIndices => _readOnlyChannelsToIndices;

        private IReadOnlyDictionary<SemanticsChannel, int> _readOnlyChannelsToIndices;
        private Dictionary<SemanticsChannel, int> _channelsToIndices = new();

        /// <summary>
        /// True if the underlying subsystem has finished initialization.
        /// </summary>
        public bool IsMetadataAvailable { get; private set; }

        /// <summary>
        /// An event which fires when the underlying subsystem has finished initializing.
        /// </summary>
        public event Action<ARSemanticSegmentationModelEventArgs> MetadataInitialized
        {
            add
            {
                _metadataInitialized += value;
                if (IsMetadataAvailable)
                {
                    var args =
                        new ARSemanticSegmentationModelEventArgs
                        {
                            Channels = Channels,
                            ChannelIndices = ChannelIndices
                        };

                    value.Invoke(args);
                }
            }
            remove
            {
                _metadataInitialized -= value;
            }
        }

        private Action<ARSemanticSegmentationModelEventArgs> _metadataInitialized;

        public event Action<ARSemanticSegmentationFrameEventArgs> FrameReceived;

        /// <summary>
        /// A type that holds a semantic segmentation texture along with its associated metadata for warping.
        /// </summary>
        private struct SemanticsTexture
        {
            // The external texture
            public NsdkExternalTexture ExternalTexture;

            // Metadata for warping the texture to viewport space
            public Matrix4x4 SamplerMatrix;
            public XRCameraParams CameraParams;

            /// <summary>
            /// True if the texture has not been updated this frame.
            /// </summary>
            public readonly bool IsOutOfDate => LastUpdatedFrameId != Time.frameCount;
            public int LastUpdatedFrameId;

            /// <summary>
            /// Disposes of the external texture and resets metadata.
            /// </summary>
            public void Reset()
            {
                ExternalTexture?.Dispose();
                SamplerMatrix = default;
                CameraParams = default;
                LastUpdatedFrameId = -1;
            }
        }

        /// <summary>
        /// A dictionary mapping semantic confidence textures (<c>ARTextureInfo</c>s) to their respective semantic
        /// segmentation channel names.
        /// </summary>
        /// <value>
        /// The semantic segmentation confidence texture infos.
        /// </value>
        private readonly Dictionary<SemanticsChannel, SemanticsTexture> _semanticChannelTextureInfos = new();

        /// <summary>
        /// The semantic segmentation packed thresholded bitmask.
        /// </summary>
        /// <value>
        /// The semantic segmentation packed thresholded bitmask.
        /// </value>
        private SemanticsTexture _packedBitmaskTextureInfo;

        /// <summary>
        /// The suppression mask texture info.
        /// </summary>
        /// <value>
        ///The suppression mask texture info.
        /// </value>
        private SemanticsTexture _suppressionMaskTextureInfo;

        /// <summary>
        /// Frequently updated information about the viewport.
        /// </summary>
        private XRCameraParams _viewport;

        /// <summary>
        /// The frame id of the last seen semantic segmentation output buffer.
        /// </summary>
        private uint? _lastKnownFrameId;

        /// <summary>
        /// Callback before the subsystem is started (but after it is created).
        /// </summary>
        protected override void OnBeforeStart()
        {
            TargetFrameRate = _targetFrameRate;
            ResetTextureInfos();
            ResetModelMetadata();
        }

        /// <summary>
        /// Callback when the manager is being disabled.
        /// </summary>
        protected override void OnDisable()
        {
            // Stop the subsystem
            base.OnDisable();

            // Reset textures and meta data
            ResetTextureInfos();
            ResetModelMetadata();
        }

        /// <summary>
        /// Callback as the manager is being updated.
        /// </summary>
        public void Update()
        {
            if (subsystem == null)
                return;

            TargetFrameRate = _targetFrameRate;

            if (!subsystem.running)
                return;

            if (!IsMetadataAvailable)
            {
                var channels = subsystem.GetChannels();
                SetModelMetadata(channels);

                var args =
                    new ARSemanticSegmentationModelEventArgs
                    {
                        Channels = Channels,
                        ChannelIndices = ChannelIndices
                    };

                _metadataInitialized?.Invoke(args);
            }

            // Method will have exited already if metadata (ie channel names) are not available,
            // so below code only executes when metadata is available.

            // Update viewport info
            _viewport.screenWidth = Screen.width;
            _viewport.screenHeight = Screen.height;
            _viewport.screenOrientation = XRDisplayContext.GetScreenOrientation();

            // Invoke event if new keyframe is available
            var currentFrameId = subsystem.LatestFrameId;
            if (currentFrameId != _lastKnownFrameId)
            {
                _lastKnownFrameId = currentFrameId;
                var eventArgs = new ARSemanticSegmentationFrameEventArgs();
                FrameReceived?.Invoke(eventArgs);
            }
        }

        /// <summary>
        /// Clears the references to the packed and confidence semantic segmentation textures
        /// </summary>
        private void ResetTextureInfos()
        {
            foreach (KeyValuePair<SemanticsChannel, SemanticsTexture> pair in _semanticChannelTextureInfos)
            {
                pair.Value.Reset();
            }

            _semanticChannelTextureInfos.Clear();
            _packedBitmaskTextureInfo.Reset();
            _suppressionMaskTextureInfo.Reset();
        }

        private void ResetModelMetadata()
        {
            IsMetadataAvailable = false;
            _readOnlyChannels = new List<SemanticsChannel>().AsReadOnly();

            _channelsToIndices = new Dictionary<SemanticsChannel, int>();
            _readOnlyChannelsToIndices = new ReadOnlyDictionary<SemanticsChannel, int>(_channelsToIndices);
        }

        private void SetModelMetadata(IReadOnlyList<SemanticsChannel> channels)
        {
            IsMetadataAvailable = true;
            _readOnlyChannels = channels;

            _channelsToIndices.Clear();
            for (int i = 0; i < channels.Count; i++)
            {
                _channelsToIndices.Add(channels[i], i);
            }

            _readOnlyChannelsToIndices = new ReadOnlyDictionary<SemanticsChannel, int>(_channelsToIndices);
        }

        /// <summary>
        /// Converts a SemanticsChannel enum value to its string representation for display purposes.
        /// </summary>
        /// <param name="channel">The semantic channel enum value.</param>
        /// <returns>The string representation of the channel (e.g., "sky", "ground", "natural_ground").</returns>
        public static string GetChannelNameFromEnum(SemanticsChannel channel)
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
        /// Converts a channel name string to its corresponding SemanticsChannel enum value.
        /// </summary>
        /// <param name="channelName">The channel name string (e.g., "sky", "ground", "natural_ground").</param>
        /// <returns>The SemanticsChannel enum value if found, otherwise null.</returns>
        public SemanticsChannel? GetChannelFromName(string channelName)
        {
            if (string.IsNullOrEmpty(channelName))
            {
                return null;
            }

            var channels = Channels;
            var channelNames = channels.Select(GetChannelNameFromEnum).ToList();
            var index = channelNames.IndexOf(channelName);
            
            if (index >= 0 && index < channels.Count)
            {
                return channels[index];
            }

            return null;
        }

        /// <summary>
        /// Returns semantic segmentation texture for the specified semantic channel.
        /// </summary>
        /// <param name="channel">The semantic channel to acquire.</param>
        /// <param name="samplerMatrix">A matrix that converts from viewport to image coordinates according to the latest pose.</param>
        /// <param name="cameraParams">Params of the viewport to sample with. Defaults to current screen dimensions if null.</param>
        /// <returns>The texture for the specified semantic channel, if any. Otherwise, <c>null</c>.</returns>
        public Texture2D GetSemanticChannelTexture(SemanticsChannel channel, out Matrix4x4 samplerMatrix,
            XRCameraParams? cameraParams = null)
        {
            // Default to current viewport
            cameraParams ??= _viewport;

            // If semantic segmentation is unavailable
            if (descriptor?.semanticSegmentationImageSupported != Supported.Supported)
            {
                samplerMatrix = default;
                return null;
            }

            // If we already have an up-to-date texture
            if (_semanticChannelTextureInfos.TryGetValue(channel, out var info))
            {
                if (!info.IsOutOfDate && info.CameraParams == cameraParams)
                {
                    samplerMatrix = info.SamplerMatrix;
                    return info.ExternalTexture.Texture as Texture2D;
                }
            }

            // Acquire the new texture descriptor
            if (!subsystem.TryGetSemanticChannel(channel, out var textureDescriptor, out samplerMatrix,
                    cameraParams))
            {
                samplerMatrix = default;
                return null;
            }

            // Format mismatch
            if (NsdkExternalTexture.GetTextureDimension(textureDescriptor) != TextureDimension.Tex2D)
            {
                Log.Error("Semantic confidence texture needs to be a Texture2D, but is " +
                    NsdkExternalTexture.GetTextureDimension(textureDescriptor) + ".");
                samplerMatrix = default;
                return null;
            }

            // Cache the texture
            if (!_semanticChannelTextureInfos.ContainsKey(channel))
            {
                var texture = NsdkExternalTexture.Create(textureDescriptor);
                if (texture == null)
                {
                    Log.Error("Failed to create semantic confidence texture for channel " + channel + ".");
                    return null;
                }

                _semanticChannelTextureInfos.Add
                (
                    channel,
                    new SemanticsTexture
                    {
                        ExternalTexture = texture,
                        SamplerMatrix = samplerMatrix,
                        CameraParams = cameraParams.Value,
                        LastUpdatedFrameId = Time.frameCount
                    }
                );
            }
            else
            {
                var texture = _semanticChannelTextureInfos[channel].ExternalTexture;
                if (texture.Update(textureDescriptor))
                {
                    _semanticChannelTextureInfos[channel] =
                        new SemanticsTexture
                        {
                            ExternalTexture = texture,
                            SamplerMatrix = samplerMatrix,
                            CameraParams = cameraParams.Value,
                            LastUpdatedFrameId = Time.frameCount
                        };
                }
            }

            return _semanticChannelTextureInfos[channel].ExternalTexture.Texture as Texture2D;
        }

        /// <summary>
        /// Retrieves the texture of semantic data where each pixel can be interpreted as a uint with bits
        /// corresponding to different classifications.
        /// </summary>
        /// <param name="samplerMatrix">A matrix that converts from viewport to image coordinates according to the latest pose.</param>
        /// <param name="cameraParams">Params of the viewport to sample with. Defaults to current screen dimensions if null.</param>
        /// <returns>The packed semantics texture, owned by the manager, if any. Otherwise, <c>null</c>.</returns>
        public Texture2D GetPackedSemanticsChannelsTexture(out Matrix4x4 samplerMatrix, XRCameraParams? cameraParams = null)
        {
            // Default to current viewport
            cameraParams ??= _viewport;

            // This will only update the external texture if  the subsystem returns a different textureDescriptor.
            if (subsystem.TryGetPackedSemanticChannels(out var textureDescriptor, out samplerMatrix, cameraParams))
            {
                var texture = _packedBitmaskTextureInfo.ExternalTexture;
                if (NsdkExternalTexture.CreateOrUpdate(ref texture, textureDescriptor))
                {
                    _packedBitmaskTextureInfo = new SemanticsTexture
                    {
                        ExternalTexture = texture,
                        SamplerMatrix = samplerMatrix,
                        CameraParams = cameraParams.Value,
                        LastUpdatedFrameId = Time.frameCount
                    };
                }

                return _packedBitmaskTextureInfo.ExternalTexture.Texture as Texture2D;
            }

            samplerMatrix = default;
            return null;
        }

        /// <summary>
        /// Retrieves the suppression mask texture, where each pixel contains a uint which can be used to interpolate
        /// between the predicted depth and the far field depth of the scene. This is useful for enabling smooth occlusion suppression.
        /// </summary>
        /// <param name="samplerMatrix">A matrix that converts from viewport to image coordinates according to the latest pose.</param>
        /// <param name="cameraParams">Params of the viewport to sample with. Defaults to current screen dimensions if null.</param>
        /// <returns>The suppression mask texture, owned by the manager, if any. Otherwise, <c>null</c>.</returns>
        public Texture2D GetSuppressionMaskTexture(out Matrix4x4 samplerMatrix, XRCameraParams? cameraParams = null)
        {
            // Default to current viewport
            cameraParams ??= _viewport;

            // This will only update the external texture if  the subsystem returns a different textureDescriptor.
            if (subsystem.TryGetSuppressionMaskTexture(out var textureDescriptor, out samplerMatrix, cameraParams))
            {
                var texture = _suppressionMaskTextureInfo.ExternalTexture;
                if (NsdkExternalTexture.CreateOrUpdate(ref texture, textureDescriptor))
                {
                    _suppressionMaskTextureInfo = new SemanticsTexture
                    {
                        ExternalTexture = texture,
                        SamplerMatrix = samplerMatrix,
                        CameraParams = cameraParams.Value,
                        LastUpdatedFrameId = Time.frameCount
                    };
                }

                return _suppressionMaskTextureInfo.ExternalTexture.Texture as Texture2D;
            }

            samplerMatrix = default;
            return null;
        }

        /// <summary>
        /// Attempt to acquire the latest semantic segmentation XRCpuImage for the specified semantic class. This
        /// provides direct access to the raw pixel data.
        /// </summary>
        /// <remarks>
        /// The <c>XRCpuImage</c> must be disposed to avoid resource leaks.
        /// </remarks>
        /// <param name="channel">The semantic channel to acquire.</param>
        /// <param name="cpuImage">If this method returns `true`, an acquired <see cref="XRCpuImage"/>. The XRCpuImage
        /// must be disposed by the caller.</param>
        /// <param name="samplerMatrix">A matrix that converts from viewport to image coordinates according to the latest pose.</param>
        /// <param name="cameraParams">Params of the viewport to sample with. Defaults to current screen dimensions if null.</param>
        /// <returns>True if the CPU image was acquired. Otherwise, false</returns>
        public bool TryAcquireSemanticChannelCpuImage(SemanticsChannel channel, out XRCpuImage cpuImage, out Matrix4x4 samplerMatrix, XRCameraParams? cameraParams = null)
        {
            cameraParams ??= _viewport;
            return subsystem.TryAcquireSemanticChannelCpuImage(channel, out cpuImage, out samplerMatrix, cameraParams);
        }

        /// <summary>
        /// Tries to acquire the latest packed semantic channels XRCpuImage. Each element of the XRCpuImage is a bit field
        /// indicating which semantic channels have surpassed their respective detection confidence thresholds for that
        /// pixel. (See <c>GetChannelIndex</c>)
        /// </summary>
        /// <remarks>The utility <c>GetChannelNamesAt</c> can be used for reading semantic channel names at a viewport
        /// location.</remarks>
        /// <param name="cpuImage">If this method returns `true`, an acquired <see cref="XRCpuImage"/>. The CPU image
        /// must be disposed by the caller.</param>
        /// <param name="samplerMatrix">A matrix that converts from viewport to image coordinates according to the latest pose.</param>
        /// <param name="cameraParams">Params of the viewport to sample with. Defaults to current screen dimensions if null.</param>
        /// <returns>True if the CPU image was acquired. Otherwise, false</returns>
        public bool TryAcquirePackedSemanticChannelsCpuImage(out XRCpuImage cpuImage, out Matrix4x4 samplerMatrix, XRCameraParams? cameraParams = null)
        {
            cameraParams ??= _viewport;
            return subsystem.TryAcquirePackedSemanticChannelsCpuImage(out cpuImage, out samplerMatrix, cameraParams);
        }

        /// <summary>
        /// Tries to acquire the latest suppression mask XRCpuImage. Each element of the XRCpuImage is a uint32 value
        /// which can be used to interpolate between instantaneous depth and far field depth.
        /// </summary>
        /// <param name="cpuImage">If this method returns `true`, an acquired <see cref="XRCpuImage"/>. The CPU image
        /// must be disposed by the caller.</param>
        /// <param name="samplerMatrix">A matrix that converts from viewport to image coordinates according to the latest pose.</param>
        /// <param name="cameraParams">Params of the viewport to sample with. Defaults to current screen dimensions if null.</param>
        /// <returns>True if the CPU image was acquired. Otherwise, false</returns>
        public bool TryAcquireSuppressionMaskCpuImage(out XRCpuImage cpuImage, out Matrix4x4 samplerMatrix, XRCameraParams? cameraParams = null)
        {
            cameraParams ??= _viewport;
            return subsystem.TryAcquireSuppressionMaskCpuImage(out cpuImage, out samplerMatrix, cameraParams);
        }

        /// <summary>
        /// Get the channel index of a specified semantic class. This corresponds to a bit position in the packed
        /// semantics buffer, with index 0 being the most-significant bit.
        /// </summary>
        /// <param name="channel">The semantic channel.</param>
        /// <returns>The index of the specified semantic class, or -1 if the channel does not exist.</returns>
        public int GetChannelIndex(SemanticsChannel channel)
        {
            if (_channelsToIndices.TryGetValue(channel, out int index))
            {
                return index;
            }

            return -1;
        }

        /// <summary>
        /// Returns the semantics at the specified pixel on screen.
        /// </summary>
        /// <param name="viewportX">Horizontal coordinate in viewport space.</param>
        /// <param name="viewportY">Vertical coordinate in viewport space.</param>
        /// <param name="cameraParams">Params of the viewport to sample with. Defaults to current screen dimensions if null.</param>
        /// <returns>A 32-bit packed unsigned integer where each
        /// bit is a binary indicator for a class, and the most-significant bit corresponds to the channel that
        /// is the 0th element of the ChannelNames list.</returns>
        public uint GetSemantics(int viewportX, int viewportY, XRCameraParams? cameraParams = null)
        {
            cameraParams ??= _viewport;

            // Acquire the CPU image
            if (!subsystem.TryAcquirePackedSemanticChannelsCpuImage(out var cpuImage, out var samplerMatrix, cameraParams))
            {
                return 0u;
            }

            // Get normalized image coordinates
            var x = viewportX + 0.5f;
            var y = viewportY + 0.5f;
            var uv = new Vector2(x / cameraParams.Value.screenWidth, y / cameraParams.Value.screenHeight);

            // Sample the image
            uint sample = cpuImage.Sample<uint>(uv, samplerMatrix);

            cpuImage.Dispose();
            return sample;
        }

        /// <summary>
        /// Returns an array of channel indices that are present at the specified pixel onscreen.
        /// </summary>
        /// <remarks>This query allocates garbage.</remarks>
        /// <param name="viewportX">Horizontal coordinate in viewport space.</param>
        /// <param name="viewportY">Vertical coordinate in viewport space.</param>
        /// <param name="cameraParams">Params of the viewport to sample with. Defaults to current screen dimensions if null.</param>
        /// <returns>An array of channel indices present for the pixel.</returns>
        public List<int> GetChannelIndicesAt(int viewportX, int viewportY, XRCameraParams? cameraParams = null)
        {
            uint sample = GetSemantics(viewportX, viewportY, cameraParams);

            var indices = new List<int>();
            for (int idx = 0; idx < Channels.Count; idx++)
            {
                // MSB = beginning of the channels list
                if ((sample & (1 << 31)) != 0)
                {
                    indices.Add(idx);
                }

                sample <<= 1;
            }

            return indices;
        }

        /// <summary>
        /// Returns an array of channels that are present for the specified pixel onscreen.
        /// </summary>
        /// <remarks>This query allocates garbage.</remarks>
        /// <param name="viewportX">Horizontal coordinate in viewport space.</param>
        /// <param name="viewportY">Vertical coordinate in viewport space.</param>
        /// <param name="cameraParams">Params of the viewport to sample with. Defaults to current screen dimensions if null.</param>
        /// <returns>An array of channels present for the pixel.</returns>
        public List<SemanticsChannel> GetChannelsAt(int viewportX, int viewportY, XRCameraParams? cameraParams = null)
        {
            var channels = new List<SemanticsChannel>();
            if (_readOnlyChannels.Count == 0)
            {
                return channels;
            }

            var indices = GetChannelIndicesAt(viewportX, viewportY, cameraParams);

            foreach (var idx in indices)
            {
                if (idx >= _readOnlyChannels.Count)
                {
                    Log.Error("Semantics channel index exceeded channels list");
                    return new List<SemanticsChannel>();
                }

                channels.Add(_readOnlyChannels[idx]);
            }

            return channels;
        }

        /// <summary>
        /// Returns an array of channel names that are present for the specified pixel onscreen (for display purposes).
        /// </summary>
        /// <remarks>This query allocates garbage.</remarks>
        /// <param name="viewportX">Horizontal coordinate in viewport space.</param>
        /// <param name="viewportY">Vertical coordinate in viewport space.</param>
        /// <param name="cameraParams">Params of the viewport to sample with. Defaults to current screen dimensions if null.</param>
        /// <returns>An array of channel names present for the pixel.</returns>
        public List<string> GetChannelNamesAt(int viewportX, int viewportY, XRCameraParams? cameraParams = null)
        {
            var channels = GetChannelsAt(viewportX, viewportY, cameraParams);
            return channels.Select(GetChannelNameFromEnum).ToList();
        }

        /// <summary>
        /// Check if a semantic class is detected at the specified location in screen space, based on the confidence
        /// threshold set for this channel. (See <c>TrySetChannelConfidenceThresholds</c>)
        /// </summary>
        /// <param name="viewportX">Horizontal coordinate in viewport space.</param>
        /// <param name="viewportY">Vertical coordinate in viewport space.</param>
        /// <param name="channel">The semantic channel to look for.</param>
        /// <param name="cameraParams">Params of the viewport to sample with. Defaults to current screen dimensions if null.</param>
        /// <returns>True if the semantic class exists at the given coordinates.</returns>
        public bool DoesChannelExistAt(int viewportX, int viewportY, SemanticsChannel channel, XRCameraParams? cameraParams = null)
        {
            if (ChannelIndices.TryGetValue(channel, out int index))
            {
                return DoesChannelExistAt(viewportX, viewportY, index, cameraParams);
            }

            return false;
        }

        /// <summary>
        /// Check if a semantic class is detected at the specified location in screen space, based on the confidence
        /// threshold set for this channel. (See <c>TrySetChannelConfidenceThresholds</c>)
        /// </summary>
        /// <param name="viewportX">Horizontal coordinate in viewport space.</param>
        /// <param name="viewportY">Vertical coordinate in viewport space.</param>
        /// <param name="channelIndex">Index of the semantic class to look for in the ChannelNames list.</param>
        /// <param name="cameraParams">Params of the viewport to sample with. Defaults to current screen dimensions if null.</param>
        /// <returns>True if the semantic class exists at the given coordinates.</returns>
        public bool DoesChannelExistAt(int viewportX, int viewportY, int channelIndex, XRCameraParams? cameraParams = null)
        {
            uint channelIndices = GetSemantics(viewportX, viewportY, cameraParams);
            return (channelIndices & (1 << (31 - channelIndex))) != 0;
        }

        /// <summary>
        /// Sets the confidence threshold for including the specified semantic channel in the packed semantic
        /// channel buffer.
        /// </summary>
        /// <remarks>
        /// Each semantic channel will use its default threshold value chosen by the model until a new value is set
        /// by this function during the AR session.
        /// Changes to the semantic segmentation thresholds are undone by either restarting the subsystem or by calling
        /// <see cref="TryResetChannelConfidenceThresholds"/>.
        /// </remarks>
        /// <param name="channelConfidenceThresholds">
        /// A dictionary consisting of keys specifying the semantics channel that is needed and values
        /// between 0 and 1, inclusive, that set the threshold above which the platform will include the specified
        /// channel in the packed semantics buffer. The key must be a semantic channel present in the list
        /// returned by <c>TryGetChannelNames</c>.
        /// </param>
        /// <exception cref="System.NotSupportedException">Thrown when setting confidence thresholds is not
        /// supported by the implementation.</exception>
        /// <returns>True if the thresholds were set. Otherwise, false.</returns>
        public bool TrySetChannelConfidenceThresholds(Dictionary<SemanticsChannel, float> channelConfidenceThresholds)
        {
            return subsystem.TrySetChannelConfidenceThresholds(channelConfidenceThresholds);
        }

        /// <summary>
        /// Resets the confidence thresholds for all semantic channels to the default values from the current model.
        /// </summary>
        /// <remarks>
        /// This reverts any changes made with <see cref="TrySetChannelConfidenceThresholds"/>.
        /// </remarks>
        /// <exception cref="System.NotSupportedException">Thrown when resetting confidence thresholds is not
        /// supported by the implementation.</exception>
        /// <returns>True if the thresholds were reset. Otherwise, false.</returns>
        public bool TryResetChannelConfidenceThresholds()
        {
            return subsystem.TryResetChannelConfidenceThresholds();
        }
    }
}
