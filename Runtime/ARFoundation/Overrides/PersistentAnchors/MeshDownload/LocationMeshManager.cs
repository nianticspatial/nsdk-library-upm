// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using NianticSpatial.NSDK.AR.Core;

using UnityEngine;

namespace NianticSpatial.NSDK.AR.Subsystems
{
    /// <summary>
    /// MonoBehaviour that manages the downloading and rendering of location meshes
    /// </summary>
    public class LocationMeshManager : MonoBehaviour
    {
        // The material to use for vertex colored meshes
        // Use the provided vertex shader to render the meshes with colored vertices,
        //   or use a custom shader to render the meshes with a custom material
        [SerializeField]
        private Material _vertexColorMaterial;

        // The material to use for textured meshes
        [SerializeField]
        [Tooltip("Specify a material to use for textured meshes, or leave empty to use the `Standard` material")]
        private Material _texturedMeshMaterial;

        [SerializeField]
        [Tooltip("Cache downloaded mesh data on disk. Downloading the same asset again then reads from disk instead of the network, and an interrupted download resumes where it stopped.")]
        private bool _cacheEnabled;

        [SerializeField]
        [Tooltip("Maximum disk space the cache may use, in megabytes. Least recently used chunks are removed once it is exceeded. Must be greater than zero when the cache is enabled. Size it above the total download size of the assets you expect to hold.")]
        private uint _cacheMaxMb = 5000;

        private CancellationTokenSource _cancellationTokenSource;
        private MeshDownloadClient _meshDownloadClient;

        /// <summary>
        /// Default decoded-size budget of the asset mesh downloads, sized for phones.
        /// </summary>
        public const uint DefaultMaxDecodedSizeKb = MeshDownloadClient.DefaultMaxDecodedSizeKb;

        private void Start()
        {
            if (_texturedMeshMaterial == null)
            {
                // If we are using URP, use the default material from the render pipeline.
                if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null)
                {
                    _texturedMeshMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                }
                else
                {
                    _texturedMeshMaterial = new Material(Shader.Find("Standard"));
                }

                // If the material is not found, log an error
                if (_texturedMeshMaterial == null)
                {
                    // This class uses Debug instead of ARLog to support editor logging without NSDK Native loaded
                    Debug.LogError
                    (
                        "LocationMeshManager: Failed to load the standard shader, please provide a textured mesh material",
                        this
                    );
                }
            }

            if (_vertexColorMaterial == null)
            {
                var tryGetShader = Shader.Find("Custom/VertexColor");
                if (tryGetShader != null)
                {
                    _vertexColorMaterial = new Material(tryGetShader);
                }

                // If the material is not found, log an error
                if (_vertexColorMaterial == null)
                {
                    // This class uses Debug instead of ARLog to support editor logging without NSDK Native loaded
                    Debug.LogError
                    (
                        "LocationMeshManager: Failed to find the vertex color shader, please provide a vertex color material",
                        this
                    );
                }
            }

            var unityContextHandle = NsdkUnityContext.UnityContextHandle;
            var nsdkHandle = NsdkUnityContext.GetNSDKHandle(unityContextHandle);
            _meshDownloadClient = new MeshDownloadClient(nsdkHandle);
            if (_cacheEnabled)
            {
                ApplyCacheConfiguration();
            }
        }

        private void OnEnable()
        {
            _cancellationTokenSource = new CancellationTokenSource();
        }

        private void OnDisable()
        {
            if (_cancellationTokenSource != null)
            {
                _cancellationTokenSource.Cancel();
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        }

        /// <summary>
        /// Get the mesh for an anchor payload
        /// </summary>
        [Obsolete("Use GetLocationMeshChunksByVpsMapAssetIdAsync instead. Site IDs and asset IDs are the preferred mechanisms for identifying maps in NSDK. This function is deprecated and does not support maps created from 360 camera captures.")]
        public async Task<GameObject> GetLocationMeshForPayloadAsync
        (
            string payload,
            ulong maxDownloadSizeKb = 0,
            bool addCollider = false,
            bool getTexture = false,
            bool cancelOnDisable = true
        )
        {
            // Create a cancellation token to cancel the download if the manager is disabled
            CancellationToken cancellationToken = default;
            if (cancelOnDisable)
            {
                if (_cancellationTokenSource == null)
                {
                    // This class uses Debug instead of ARLog to support editor logging without NSDK Native loaded
                    Debug.LogWarning
                    (
                        "Trying to cancel on disable without a valid cancellation token " +
                        "source, mesh download will not be canceled on disable"
                    );
                }
                else
                {
                    cancellationToken = _cancellationTokenSource.Token;
                }
            }
            return await _meshDownloadClient.GenerateLocationMeshAsyncFromPayload(payload, getTexture, addCollider, (uint)maxDownloadSizeKb, _vertexColorMaterial, _texturedMeshMaterial, cancellationToken);
        }

        /// <summary>
        /// Stream mesh chunks for a VPS map asset, yielding each chunk as a GameObject as it completes.
        /// Resolves the VPS map asset to its Mesh sibling in the same asset group. Chunks arrive
        /// one at a time, closest-first based on the provided GPS position.
        /// Iterate with <c>await foreach</c> to process each chunk as it arrives.
        /// The download stops once the chunks delivered so far occupy
        /// <paramref name="maxDecodedSizeKb"/> kilobytes decoded: vertices, UVs, indices and
        /// per-vertex colours at 4 bytes each, plus texture bytes as received. Pass 0 for no
        /// budget.
        /// </summary>
        /// <remarks>
        /// Each chunk GameObject's local space is the asset's origin space, in Unity axes and meters:
        /// the chunk's own transform (chunk-local vertices to the asset's origin) and the conversion
        /// from NSDK axes are applied on its children. Parent the chunk under the
        /// <see cref="NianticSpatial.NSDK.AR.VPS2.ARVps2Asset"/> of the same asset (or place it at
        /// <see cref="NianticSpatial.NSDK.AR.XRSubsystems.XRVps2AssetTrackingData.Pose"/>) to align it
        /// with the AR scene. The mesh carries no geographic data; see <see cref="MeshGeoReference"/>
        /// for its georeference.
        /// </remarks>
        public IAsyncEnumerable<GameObject> GetLocationMeshChunksByVpsMapAssetIdAsync(
            string vpsMapAssetId,
            double latitude,
            double longitude,
            bool getTexture = false,
            uint maxChunks = 0,
            uint maxSizeKb = 0,
            uint maxDecodedSizeKb = DefaultMaxDecodedSizeKb,
            bool addCollider = false,
            bool cancelOnDisable = true)
        {
            return _meshDownloadClient.DownloadLocationMeshChunksByVpsMapAssetId(
                vpsMapAssetId, latitude, longitude, getTexture, maxChunks, maxSizeKb,
                maxDecodedSizeKb, addCollider, _vertexColorMaterial, _texturedMeshMaterial,
                GetChunkStreamCancellationToken(cancelOnDisable));
        }

        /// <summary>
        /// Stream mesh chunks for a VPS map asset like
        /// <see cref="GetLocationMeshChunksByVpsMapAssetIdAsync"/>, but yield each chunk's geometry
        /// as <see cref="MeshChunkData"/> instead of building Unity objects for it.
        /// </summary>
        /// <remarks>
        /// Build a chunk GameObject from the yielded data with <see cref="CreateMeshChunk"/>.
        /// </remarks>
        public IAsyncEnumerable<MeshChunkData> GetLocationMeshChunkDataByVpsMapAssetIdAsync(
            string vpsMapAssetId,
            double latitude,
            double longitude,
            bool getTexture = false,
            uint maxChunks = 0,
            uint maxSizeKb = 0,
            uint maxDecodedSizeKb = DefaultMaxDecodedSizeKb,
            bool cancelOnDisable = true)
        {
            return _meshDownloadClient.DownloadLocationMeshChunkDataByVpsMapAssetId(
                vpsMapAssetId, latitude, longitude, getTexture, maxChunks, maxSizeKb,
                maxDecodedSizeKb, GetChunkStreamCancellationToken(cancelOnDisable));
        }

        /// <summary>
        /// Build a chunk GameObject from geometry yielded by
        /// <see cref="GetLocationMeshChunkDataByVpsMapAssetIdAsync"/>, rendered with this manager's
        /// materials. The chunk is the same as one yielded by
        /// <see cref="GetLocationMeshChunksByVpsMapAssetIdAsync"/>.
        /// </summary>
        /// <remarks>
        /// The chunk is returned inactive; activate it once it is parented. Each call creates new
        /// Unity Mesh, Texture and Material assets owned by the chunk, and destroying the chunk
        /// frees them. <paramref name="data"/> is left intact.
        /// </remarks>
        public GameObject CreateMeshChunk(MeshChunkData data, bool addCollider = false)
        {
            return _meshDownloadClient.BuildMeshFromChunkData(
                data, addCollider, _vertexColorMaterial, _texturedMeshMaterial);
        }

        /// <summary>
        /// Enable or disable the on-disk cache of downloaded mesh data, or change its budget. With
        /// the cache enabled, downloaded geometry and textures are kept on disk per chunk.
        /// Downloading the same asset again reads the chunks already on disk instead of the
        /// network, and an interrupted download resumes where it stopped. The cache is off by
        /// default. The <c>Cache Enabled</c> and <c>Cache Max Mb</c> fields in the Inspector apply
        /// the same settings when this manager starts.
        /// </summary>
        /// <remarks>
        /// The setting applies to the native mesh downloader, which every LocationMeshManager
        /// shares, so the last manager to start or to call this method wins. The change takes
        /// effect at once, or when this manager starts if it has not started yet. A changed budget
        /// also applies to downloads already in progress. Disabling only affects downloads started
        /// afterwards and does not delete cached data: while the cache is disabled, nothing in the
        /// cache directory is read, written or deleted, so the data is intact when the cache is
        /// enabled again. Cached data never goes stale, because a re-run of the mapping pipeline
        /// publishes new chunk IDs and an ID always names the same geometry.
        /// Once the cache exceeds <paramref name="maxMb"/>, the least recently used chunks are
        /// removed until it fits. Size the budget above the total download size of the assets you
        /// expect to hold. A smaller budget evicts chunks that are about to be needed again and
        /// downloads them again indefinitely.
        /// </remarks>
        /// <param name="enabled">Whether to cache downloaded mesh data on disk.</param>
        /// <param name="maxMb">
        /// Maximum disk space the cache may use, in megabytes. Must be greater than zero when
        /// <paramref name="enabled"/> is true.
        /// </param>
        /// <exception cref="ArgumentOutOfRangeException">
        /// <paramref name="enabled"/> is true and <paramref name="maxMb"/> is zero.
        /// </exception>
        public void ConfigureCache(bool enabled, uint maxMb)
        {
            ThrowIfCacheBudgetMissing(enabled, maxMb);
            _cacheEnabled = enabled;
            _cacheMaxMb = maxMb;
            if (_meshDownloadClient != null)
            {
                ApplyCacheConfiguration();
            }
        }

        private void ApplyCacheConfiguration()
        {
            ThrowIfCacheBudgetMissing(_cacheEnabled, _cacheMaxMb);
            _meshDownloadClient.Configure(new MeshDownloadClient.NsdkMeshDownloaderConfiguration
            {
                CacheEnabled = _cacheEnabled,
                CacheMaxMb = _cacheMaxMb,
            });
        }

        private static void ThrowIfCacheBudgetMissing(bool enabled, uint maxMb)
        {
            if (enabled && maxMb == 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxMb),
                    "The mesh cache budget must be greater than zero when the cache is enabled. " +
                    "Set Cache Max Mb on the LocationMeshManager or pass a non-zero maxMb to ConfigureCache.");
            }
        }

        private CancellationToken GetChunkStreamCancellationToken(bool cancelOnDisable)
        {
            if (!cancelOnDisable)
            {
                return default;
            }

            if (_cancellationTokenSource == null)
            {
                Debug.LogWarning(
                    "Trying to cancel on disable without a valid cancellation token source, " +
                    "mesh chunk stream will not be canceled on disable"
                );
                return default;
            }

            return _cancellationTokenSource.Token;
        }

        /// <summary>
        /// Get or set the material to use for textured meshes
        /// A custom material can be provided to render the meshes with a custom shader
        /// </summary>
        public Material TexturedMeshMaterial
        {
            get => _texturedMeshMaterial;
            set
            {
                _texturedMeshMaterial = value;
            }
        }

        /// <summary>
        /// Get or set the material to use for vertex colored meshes
        /// A custom material can be provided to render the meshes with a custom shader
        /// </summary>
        public Material VertexColorMaterial
        {
            get => _vertexColorMaterial;
            set
            {
                _vertexColorMaterial = value;
            }
        }
    }
}
