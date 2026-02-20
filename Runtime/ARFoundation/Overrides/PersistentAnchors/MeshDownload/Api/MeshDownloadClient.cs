// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using NianticSpatial.NSDK.AR.API;
using NianticSpatial.NSDK.AR.Core;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NianticSpatial.NSDK.AR.Subsystems
{
    internal class MeshDownloadClient
    {
        internal enum ClientRequestError
        {
            None,
            Canceled,
            Timeout
        }

        internal struct Result
        {
            public NsdkStatus Status;
            public ClientRequestError ClientRequestError;
            public NsdkMeshDownloaderResults? Results;

            public Result(NsdkStatus status, ClientRequestError error, NsdkMeshDownloaderResults? results)
            {
                this.Status = status;
                this.ClientRequestError = error;
                this.Results = results;
            }

            public bool IsSuccess()
            {
                return Status == NsdkStatus.Ok && ClientRequestError == ClientRequestError.None && Results.HasValue &&
                    Results.Value.error == NsdkMeshDownloaderError.None;
            }
        }

        private const int TIMEOUT_SECONDS = 10;
        private readonly IMeshDownloaderApi _api;
        private IntPtr _nsdkHandle;
        private readonly Material _defaultMaterial;

        public MeshDownloadClient(IntPtr nsdkHandle)
        {
            if (!nsdkHandle.IsValidHandle())
            {
                throw new ArgumentNullException(nameof(nsdkHandle));
            }

            _api = new MeshDownloaderApi();

            _nsdkHandle = nsdkHandle;
            var status = _api.NSDK_MeshDownloader_Create(_nsdkHandle);
            if (status != NsdkStatus.Ok && status != NsdkStatus.FeatureAlreadyExists)
            {
                throw new Exception("Failed to create NSDK Mesh Downloader");
            }

            _defaultMaterial = new Material(Shader.Find("Standard"));
            if (_defaultMaterial == null)
            {
                throw new Exception("Failed to find Standard material");
            }
        }

        // Test Constructor for mocked native api
        internal MeshDownloadClient(IMeshDownloaderApi mockApi)
        {
            _nsdkHandle =
                new IntPtr(1); // This will never be dereferenced, just there to pass the nullptr check in method calls
            _api = mockApi;
        }

        private void CleanupResources(List<Mesh> meshes, List<Texture> textures, List<Material> materials)
        {
            // Clean up meshes
            foreach (Mesh mesh in meshes)
            {
                if (mesh != null)
                {
                    Object.Destroy(mesh);
                }
            }

            meshes.Clear();

            // Clean up textures
            foreach (Texture texture in textures)
            {
                if (texture != null)
                {
                    Object.Destroy(texture);
                }
            }

            textures.Clear();

            // Clean up materials
            foreach (Material material in materials)
            {
                if (material != null)
                {
                    Object.Destroy(material);
                }
            }

            materials.Clear();
        }

        private GameObject BuildMeshFromNsdkMeshDataResult(
            NsdkMeshDownloaderResults meshDownloaderResults,
            string payload,
            bool addCollider = false,
            Material texturelessMaterial = null,
            Material texturedMaterial = null)
        {
            var newMeshGo = new GameObject($"Payload {payload} Mesh");
            var meshContainer = new GameObject("MeshContainer");
            meshContainer.transform.SetParent(newMeshGo.transform, false);

            // In the case that we run into invalid data anywhere in the conversion process
            // we need to clean up the new objects we create that ultimately will not be used
            var meshes = new List<Mesh>();
            var textures = new List<Texture>();
            var materials = new List<Material>();

            // Get the size of our struct for pointer arithmetic
            int structSize = Marshal.SizeOf<NsdkMeshDownloaderData>();
            for (var i = 0; i < meshDownloaderResults.num_results; i++)
            {
                // Calculate the pointer to the current struct
                var currentMeshDownloaderDataPtr = IntPtr.Add(meshDownloaderResults.results, i * structSize);

                // Marshal the data at the current pointer into our struct
                NsdkMeshDownloaderData currentMeshDownloaderData =
                    Marshal.PtrToStructure<NsdkMeshDownloaderData>(currentMeshDownloaderDataPtr);

                // Now work on the data
                NsdkMeshData meshData = currentMeshDownloaderData.mesh_data;
                NsdkBuffer imageData = currentMeshDownloaderData.image_data;
                NsdkMatrix4F transform = currentMeshDownloaderData.transform;

                // Starting with meshData
                // Check if the meshData values are valid
                if (!IsMeshDataValid(meshData))
                {
                    Object.Destroy(newMeshGo);
                    return null;
                }

                // Convert vertices (float* to Vector3[])
                // vertices_size is the total number of floats, so divide by 3 to get vertex count
                int vertexCount = (int)meshData.vertices_size / 3;
                var vertices = new Vector3[vertexCount];
                var vertexFloats = new float[meshData.vertices_size];
                Marshal.Copy(meshData.vertices, vertexFloats, 0, (int)meshData.vertices_size);

                for (var v = 0; v < vertexCount; v++)
                {
                    vertices[v] = new Vector3(
                        vertexFloats[v * 3],
                        vertexFloats[v * 3 + 1],
                        vertexFloats[v * 3 + 2]
                    );
                }

                // Convert UVs (float* to Vector2[])
                // uvs_size is the total number of floats, so divide by 2 to get UV count
                int uvCount = (int)meshData.uvs_size / 2;
                var uvs = new Vector2[uvCount];
                var uvFloats = new float[meshData.uvs_size];
                Marshal.Copy(meshData.uvs, uvFloats, 0, (int)meshData.uvs_size);

                for (var u = 0; u < uvCount; u++)
                {
                    uvs[u] = new Vector2(
                        uvFloats[u * 2],
                        uvFloats[u * 2 + 1]
                    );
                }

                // Convert indices (uint32* to int[])
                var indices = new int[meshData.indices_size];
                // Since Marshal.Copy doesn't directly support uint[], we'll copy to int[]
                Marshal.Copy(meshData.indices, indices, 0, (int)meshData.indices_size);

                // Convert colors (uint32* to Color[])
                var colorsNormalized = new Color[meshData.colors_size / 4]; // Divide by 4 since each color uses 4 values
                var colorsRaw = new int[meshData.colors_size];
                Marshal.Copy(meshData.colors, colorsRaw, 0, (int)meshData.colors_size);

                for (var c = 0; c < colorsNormalized.Length; c++)
                {
                    // Get the four consecutive values for this color
                    int baseIndex = c * 4;
                    uint r = (uint)colorsRaw[baseIndex];
                    uint g = (uint)colorsRaw[baseIndex + 1];
                    uint b = (uint)colorsRaw[baseIndex + 2];
                    uint a = (uint)colorsRaw[baseIndex + 3];

                    // Convert to Unity Color (0-1)
                    colorsNormalized[c] = new Color(r / 255f, g / 255f, b / 255f, a / 255f);
                }

                // Now imageData
                // Create a new Texture2D
                var texture = new Texture2D(2, 2); // Initial size doesn't matter as LoadImage will resize
                Material material;
                // If we have downloaded texture data create a texture and material to use it
                if (imageData.data_size > 0)
                {
                    if (imageData.data == IntPtr.Zero)
                    {
                        Log.Error("Image data is null while indicating it has size greater than zero");
                        Object.Destroy(newMeshGo);
                        Object.Destroy(texture);
                        CleanupResources(meshes, textures, materials);
                        return null;
                    }

                    // Convert imageData (uint8_t* to byte[])
                    var imageDataBytes = new byte[imageData.data_size];
                    Marshal.Copy(imageData.data, imageDataBytes, 0, (int)imageData.data_size);

                    // Load the JPEG data
                    if (!texture.LoadImage(imageDataBytes))
                    {
                        Log.Error("Failed to load image data into texture");
                    }

                    // Create a material with the texture
                    if (texturedMaterial != null)
                    {
                        material = new Material(texturedMaterial);
                        material.mainTexture = texture;
                    }
                    else
                    {
                        Log.Info("texturedMaterial provided is null, using standard material");
                        material = _defaultMaterial;
                    }
                }
                // Else create material without downloaded texture
                else
                {
                    if (texturelessMaterial != null)
                    {
                        material = new Material(texturelessMaterial);
                    }
                    else
                    {
                        Log.Info("texturedMaterial provided is null, using standard material");
                        material = _defaultMaterial;
                    }
                }

                textures.Add(texture);
                materials.Add(material);

                // Finally, do the transform
                // Create an array to hold the 16 float values
                if (transform.Values == IntPtr.Zero)
                {
                    Log.Error("Mesh generation failed, transform values are null");
                    Object.Destroy(newMeshGo);
                    CleanupResources(meshes, textures, materials);
                    return null;
                }

                var matrixValues = new float[16];
                Marshal.Copy(transform.Values, matrixValues, 0, 16);

                // Create Unity matrix
                // Unity's Matrix4x4 is column-major, assuming the input is also column-major
                var transformMatrix = new Matrix4x4(
                    new Vector4(matrixValues[0], matrixValues[1], matrixValues[2], matrixValues[3]),
                    new Vector4(matrixValues[4], matrixValues[5], matrixValues[6], matrixValues[7]),
                    new Vector4(matrixValues[8], matrixValues[9], matrixValues[10], matrixValues[11]),
                    new Vector4(matrixValues[12], matrixValues[13], matrixValues[14], matrixValues[15])
                );

                // Create the mesh with everything now
                var mesh = new Mesh();
                mesh.name = $"Submesh {i}";

                // Assign the vertex positions
                mesh.vertices = vertices;

                // Assign the UV coordinates
                mesh.uv = uvs;

                // Assign the triangles (indices)
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
                mesh.triangles = indices;

                // Apply the colors array to the mesh
                mesh.colors = colorsNormalized;

                // Recalculate the mesh normals and bounds
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                meshes.Add(mesh);

                // Create a GameObject to hold the mesh
                var meshObject = new GameObject($"Submesh {i}");
                MeshFilter meshFilter = meshObject.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = meshObject.AddComponent<MeshRenderer>();

                // Assign the mesh and material
                meshFilter.mesh = mesh;
                meshRenderer.material = material;

                if (addCollider)
                {
                    MeshCollider newCollider = meshObject.AddComponent<MeshCollider>();
                    newCollider.sharedMesh = mesh;
                }

                // Apply the transform
                // Extract position, rotation, and scale from matrix
                Vector3 position = MatrixUtils.PositionFromMatrix(transformMatrix);

                Quaternion rotation = MatrixUtils.RotationFromMatrix(transformMatrix);
                var scale = new Vector3(
                    transformMatrix.GetColumn(0).magnitude,
                    transformMatrix.GetColumn(1).magnitude,
                    transformMatrix.GetColumn(2).magnitude
                );

                //  Make it a child of the parent
                meshObject.transform.SetParent(meshContainer.transform, false);

                // Apply transform components
                meshObject.transform.localPosition = position;
                meshObject.transform.localRotation = rotation;
                meshObject.transform.localScale = scale;
            }

            // Mesh chunks are put together upside down, we gotta flip it
            meshContainer.transform.localScale = new Vector3(1, -1, 1);

            return newMeshGo;
        }

        private async Task<Result> DownloadLocationMeshDataFromPayload(string payload, bool getTexture,
            uint maxSizeKb, CancellationToken cancellationToken = default)
        {
            if (_api == null)
            {
                throw new InvalidOperationException("Meshdownloader api is null");
            }
            if (!_nsdkHandle.IsValidHandle())
            {
                throw new InvalidOperationException("Handle to NSDK's native library is null");
            }

            ulong request = 0;
            NsdkStatus nsdkStatus;
            using (ManagedNsdkString nsdkPayload = new ManagedNsdkString(payload))
            {
                nsdkStatus = _api.NSDK_MeshDownloader_RequestLocationMesh(
                    _nsdkHandle,
                   nsdkPayload.ToNsdkString(),
                   getTexture, out request, maxSizeKb);
            }

            if (nsdkStatus != NsdkStatus.Ok)
            {
                return new Result(nsdkStatus, ClientRequestError.None, null);
            }

            NsdkMeshDownloaderResults results;
            DateTime timeout = DateTime.Now.AddSeconds(TIMEOUT_SECONDS);

            while (DateTime.Now < timeout)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return new Result(NsdkStatus.Ok, ClientRequestError.Canceled, null);
                }

                nsdkStatus = _api.NSDK_MeshDownloader_GetLocationMeshResults(_nsdkHandle, request, out results);

                switch (nsdkStatus)
                {
                    case NsdkStatus.Ok:
                        // Still waiting for the download
                        if (results.status == NsdkRequestStatus.InProgress)
                        {
                            break;
                        }

                        // Successfully downloaded mesh
                        return new Result(NsdkStatus.Ok, ClientRequestError.None, results);
                    default:
                        // Some other error, all caused by us calling the api wrong, native dying, or the feature being
                        // destroyed.
                        return new Result(nsdkStatus, ClientRequestError.None, null);
                }

                await Task.Delay(100, cancellationToken);
            }

            Log.Error("Timed out waiting for mesh download");
            return new Result(NsdkStatus.Ok, ClientRequestError.Timeout, null);
        }

        public async Task<GameObject> GenerateLocationMeshAsyncFromPayload(
            string payload,
            bool getTexture = false,
            bool addCollider = false,
            uint maxSizeKb = 0,
            Material texturelessMaterial = null,
            Material texturedMaterial = null,
            CancellationToken cancellationToken = default)
        {
            Result result =
                await DownloadLocationMeshDataFromPayload(payload, getTexture, maxSizeKb, cancellationToken);
            if (result.IsSuccess())
            {
                var results = result.Results.Value;
                GameObject meshGo = BuildMeshFromNsdkMeshDataResult(results, payload, addCollider,
                    texturelessMaterial,
                    texturedMaterial);

                if (_api == null)
                {
                    throw new Exception("Meshdownloader api is null");
                }

                if (!results.handle.IsValidHandle())
                {
                    throw new Exception("Handle is null");
                }

                _api.NSDK_Release_Resource(results.handle);
                return meshGo;
            }

            if (result.Status != NsdkStatus.Ok)
            {
                Log.Error($"Failed to download location mesh: {result.Status}");
            }
            else if (result.ClientRequestError == ClientRequestError.Timeout)
            {
                Log.Error($"Failed to download location mesh: timeout");
            }
            else if (result.ClientRequestError == ClientRequestError.Canceled)
            {
                Log.Warning($"Stopped downloading location mesh (canceled)");
            }
            else if (result.Results.Value.status == NsdkRequestStatus.Failed)
            {
                Log.Error($"Failed to download location mesh: {result.Results.Value.error}");
            }

            return null;
        }

        private static bool IsMeshDataValid(NsdkMeshData meshData)
        {
            // Check if vertices pointer and size are valid
            if (meshData.vertices == IntPtr.Zero || meshData.vertices_size == 0)
            {
                Log.Error("Invalid vertices data: null pointer or zero size");
                return false;
            }

            // Check if uvs pointer and size are valid
            if (meshData.uvs == IntPtr.Zero && meshData.uvs_size > 0)
            {
                Log.Error("Invalid UVs data: null pointer");
                return false;
            }

            // Check if indices pointer and size are valid
            if (meshData.indices == IntPtr.Zero)
            {
                Log.Error("Invalid indices data: null pointer");
                return false;
            }

            // Verify that vertices size is a multiple of 3 (x,y,z coordinates)
            if (meshData.vertices_size % 3 != 0)
            {
                Log.Error("Vertices size is not a multiple of 3 floats");
                return false;
            }

            // Verify that UVs size is a multiple of 2 (u,v coordinates)
            if (meshData.uvs_size % 2 != 0)
            {
                Log.Error("UVs size is not a multiple of 2 floats");
                return false;
            }

            // Calculate number of vertices and indices
            var indexCount = (int)meshData.indices_size;

            // Verify that we have at least one triangle
            if (indexCount < 3)
            {
                Log.Error("Not enough indices for a single triangle");
                return false;
            }

            // Verify that index count is a multiple of 3 (triangles)
            if (indexCount % 3 != 0)
            {
                Log.Error("Index count is not a multiple of 3");
                return false;
            }

            // Verify that colors a multiple of 4 (rgba)
            if (meshData.colors_size % 4 != 0)
            {
                Log.Error("Colors size is not a multiple of 4");
                return false;
            }

            return true;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NsdkMeshData
        {
            public IntPtr vertices; // const float*
            public IntPtr uvs; // const float*
            public IntPtr indices; // const uint32_t*
            public IntPtr colors; // const uint32_t*
            public UInt32 vertices_size;
            public UInt32 uvs_size;
            public UInt32 indices_size;
            public UInt32 colors_size;
            public UInt32 _padding; // Keeping the padding for 8-byte alignment
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NsdkMeshDownloaderData
        {
            public NsdkMeshData mesh_data;
            public NsdkBuffer image_data;
            public NsdkMatrix4F transform;
        }

        internal enum NsdkRequestStatus // Using int as the underlying type
        {
            InProgress = 0,
            Success,
            Failed
        }

        internal enum NsdkMeshDownloaderError
        {
            None = 0,
            SizeExceedsLimit,
            CurlClientError,
            HttpResponseError,
            UnexpectedResponse,
            InternalError
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NsdkMeshDownloaderResults
        {
            public IntPtr handle;
            public NsdkRequestStatus status;
            public NsdkMeshDownloaderError error;
            public IntPtr results; // Pointer to NSDK_MeshDownloader_Data array
            public int num_results;
        }

        private class MeshDownloaderApi : IMeshDownloaderApi
        {
            public NsdkStatus NSDK_MeshDownloader_Create(IntPtr nsdkHandle) =>
                NativeApi.ARDK_MeshDownloader_Create(nsdkHandle);

            public NsdkStatus NSDK_MeshDownloader_Destroy(IntPtr nsdkHandle) =>
                NativeApi.ARDK_MeshDownloader_Destroy(nsdkHandle);

            public NsdkStatus NSDK_MeshDownloader_RequestLocationMesh(
                IntPtr nsdkHandle,
                NsdkString payload,
                bool getTexture,
                out ulong requestIdOut,
                uint maxSizeKb) =>
                NativeApi.ARDK_MeshDownloader_RequestLocationMesh(nsdkHandle, payload, getTexture,
                    out requestIdOut, maxSizeKb);

            public NsdkStatus NSDK_MeshDownloader_GetLocationMeshResults(
                IntPtr nsdkHandle,
                ulong requestId,
                out NsdkMeshDownloaderResults resultsOut) =>
                NativeApi.ARDK_MeshDownloader_GetLocationMeshResults(nsdkHandle, requestId, out resultsOut);

            public void NSDK_Release_Resource(IntPtr resource)
            {
                NsdkExternUtils.ReleaseResource(resource);
            }
        }

        private static class NativeApi
        {
            [DllImport(NsdkPlugin.Name)]
            public static extern NsdkStatus ARDK_MeshDownloader_Create(IntPtr nsdkHandle);

            // The Unity SDK (currently) does not explicitly destroy the MeshDownloader component because
            // we can't easily guarantee the component will be destroyed before NsdkUnityContext is. The native
            // NSDK holds a shared pointer to its components, so when it is destroyed, all its components will be
            // released too.
            [DllImport(NsdkPlugin.Name)]
            public static extern NsdkStatus ARDK_MeshDownloader_Destroy(IntPtr nsdkHandle);

            [DllImport(NsdkPlugin.Name)]
            public static extern NsdkStatus ARDK_MeshDownloader_RequestLocationMesh(
                IntPtr nsdkHandle,
                NsdkString payload,
                bool getTexture,
                out ulong requestIdOut,
                uint maxSizeKb);

            [DllImport(NsdkPlugin.Name)]
            public static extern NsdkStatus ARDK_MeshDownloader_GetLocationMeshResults(
                IntPtr nsdkHandle,
                ulong requestId,
                out NsdkMeshDownloaderResults resultsOut);
        }
    }
}
