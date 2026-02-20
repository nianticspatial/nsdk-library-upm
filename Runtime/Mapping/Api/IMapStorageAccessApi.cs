// Copyright 2022-2026 Niantic Spatial.

using System;
using UnityEngine;

namespace NianticSpatial.NSDK.AR.Mapping.Api
{
    internal interface IMapStorageAccessApi : IDisposable
    {
        /// <summary>
        /// Initialize the map storage feature. Call this before calling any other map storage functions.
        /// </summary>
        /// <param name="unityContextHandle">Handle to the Unity context.</param>
        /// <returns>True if successful, false otherwise.</returns>
        bool Create(IntPtr unityContextHandle);

        /// <summary>
        /// Deinitialize the map storage feature, releasing all its resources.
        /// </summary>
        /// <returns>True if successful, false otherwise.</returns>
        bool Destroy();

        /// <summary>
        /// Get the current map data.
        /// </summary>
        /// <param name="mapData">The serialized map data will be written here.</param>
        /// <returns>True if successful, false otherwise.</returns>
        bool GetMapData(out byte[] mapData);

        /// <summary>
        /// Get the latest map update data, which consists of the new nodes and edges
        /// that have been added since the last call to this function.
        /// </summary>
        /// <param name="mapUpdateData">The serialized map update data will be written here.</param>
        /// <returns>True if successful, false otherwise.</returns>
        bool GetMapUpdate(out byte[] mapUpdateData);

        /// <summary>
        /// Add serialized map data to the map storage.
        /// </summary>
        /// <param name="mapData">The serialized map data to add.</param>
        /// <returns>True if successful, false otherwise.</returns>
        bool AddMap(byte[] mapData);

        /// <summary>
        /// Clear the map storage.
        /// </summary>
        /// <returns>True if successful, false otherwise.</returns>
        bool Clear();

        /// <summary>
        /// Merge a map update into an existing map.
        /// </summary>
        /// <param name="existingMapData">The existing map to merge the update into.</param>
        /// <param name="mapUpdateData">The map update to merge into the existing map.</param>
        /// <param name="mergedMapData">The merged map will be written here.</param>
        /// <returns>True if successful, false otherwise.</returns>
        bool MergeMapUpdate(byte[] existingMapData, byte[] mapUpdateData, out byte[] mergedMapData);

        /// <summary>
        /// Creates an anchor on the existing map located at the origin of the current
        /// AR session, if possible.
        /// </summary>
        /// <param name="anchorPayload">The payload of the anchor, encoded as a base64 string.</param>
        /// <returns>True if successful, false otherwise.</returns>
        bool CreateRootAnchor(out byte[] anchorPayload);

        /// <summary>
        /// Extract the metadata from a map relative to an anchor on the map.
        /// </summary>
        /// <param name="anchorPayload">The anchor payload as a byte array. The returned map metadata
        /// will be relative to this anchor.</param>
        /// <param name="mapData">The map to extract the metadata from.</param>
        /// <param name="points">The positions of the feature points in the map.</param>
        /// <param name="errors">The error metric for each of the points in the map.</param>
        /// <param name="usesLearnedFeatures">Whether the map uses learned features.</param>
        /// <returns>True if successful, false otherwise.</returns>
        bool ExtractMapMetadataFromAnchor(byte[] anchorPayload, byte[] mapData, out Vector3[] points, out float[] errors, out bool usesLearnedFeatures);
    }
}
