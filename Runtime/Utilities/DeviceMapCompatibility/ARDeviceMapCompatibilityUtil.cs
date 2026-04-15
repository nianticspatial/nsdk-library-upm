// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Niantic.Lightship.AR.Protobuf;
using Niantic.Protobuf;
using UnityEngine;

namespace NianticSpatial.NSDK.AR.Utilities
{
    /// <summary>
    /// ARDeviceMapCompatibilityUtil is a utility to use ARDK 3.x Device Map in NSDK
    /// </summary>
    [PublicAPI]
    public class ARDeviceMapCompatibilityUtil
    {
        /// <summary>
        /// Compatible struct as in ARDK 3.x representing a single device map node
        /// </summary>
        [Serializable]
        private struct SerializeableDeviceMapNode
        {
            public ulong _subId1;
            public ulong _subId2;
            public byte[] _mapData;
            public byte[] _anchorPayload;
            public string _mapType;
        }

        /// <summary>
        /// Compatible struct as in ARDK 3.x to serialize/desrialize graph blobs
        /// </summary>
        [Serializable]
        private struct SerializeableDeviceMapGraph
        {
            public byte[] _graphData;
        }

        /// <summary>
        /// Compatible struct as in ARDK 3.x to serialize/desrialize entire device map
        /// </summary>
        [Serializable]
        private struct SerializableDeviceMap
        {
            public SerializeableDeviceMapNode[] _serializeableSingleDeviceMaps;

            public SerializeableDeviceMapGraph _graphData;
        }

        private List<SerializeableDeviceMapNode> DeviceMapNodes = new();
        private SerializeableDeviceMapGraph DeviceMapGraph = new();

        /// <summary>
        /// Try to convert Device Map blob as ARDK 3.x Device Map
        /// </summary>
        /// <param name="ardk3DeviceMap">Byte array data of serialized ARDK 3.x Device Map</param>
        /// <param name="nsdkDeviceMap">NSDK Device Map, or null if ardk3DeviceMap is not ARDK 3.x Device Map</param>
        /// <returns>true if successfully converted into NSDK Device Map. false if failed to convert Device Map</returns>
        public static bool TryConvertArdk3ToNsdk(byte[] ardk3DeviceMap, out byte[] nsdkDeviceMap)
        {
            nsdkDeviceMap = null;
            try
            {
                var map = RestoreArdk3DeviceMapBlob(ardk3DeviceMap);
                if (map == null)
                {
                    return false;
                }
                nsdkDeviceMap = SerializeToNsdkDeviceMapBlob(map);
            }

            // BinaryFormatter throws different exceptions depending on runtime version and platform
            // when given non-ARDK3 data (e.g. SerializationException, DecoderFallbackException, NullReferenceException)
            catch (Exception e)
            {
                Debug.Log(
                    $"Could not convert to NSDK Device Map {e.Message}. This is expected if this is map created with NSDK 4.0 and above");
                return false;
            }

            return true;
        }

        //
        private static ARDeviceMapCompatibilityUtil RestoreArdk3DeviceMapBlob(byte[] serializedArdk3DeviceMap)
        {
            SerializableDeviceMap serialiableMapNode;
            BinaryFormatter formatter = new BinaryFormatter();
            formatter.Binder = new ARDeviceMapRenameBinder();
            using (MemoryStream stream = new MemoryStream(serializedArdk3DeviceMap))
            {
                serialiableMapNode = (SerializableDeviceMap)formatter.Deserialize(stream);
            }

            var deviceMap = new ARDeviceMapCompatibilityUtil();
            for (var i = 0; i < serialiableMapNode._serializeableSingleDeviceMaps.Length; i++)
            {
                deviceMap.DeviceMapNodes.Add(serialiableMapNode._serializeableSingleDeviceMaps[i]);
            }

            deviceMap.DeviceMapGraph = serialiableMapNode._graphData;

            return deviceMap;
        }

        //
        private static byte[] SerializeToNsdkDeviceMapBlob(ARDeviceMapCompatibilityUtil deviceMapCompatibilityUtil)
        {
            var deviceMapProto = new DeviceMap();

            foreach (var deviceMapNode in deviceMapCompatibilityUtil.DeviceMapNodes)
            {
                var mapNode = new DeviceMapNode();
                mapNode.SubId1 = deviceMapNode._subId1;
                mapNode.SubId2 = deviceMapNode._subId2;
                mapNode.MapNodeDataType = MapTypeStringToEnum(deviceMapNode._mapType);
                mapNode.MapData = ByteString.CopyFrom(deviceMapNode._mapData);
                mapNode.MapDataTypeVersion = 1;
                mapNode.MapAnchorPayload = ByteString.CopyFrom(deviceMapNode._anchorPayload);
                mapNode.Algorithm = DeviceMappingAlgorithm.Slick;
                mapNode.ConfigsJson = "{}";
                deviceMapProto.DeviceMapNodes.Add(mapNode);
            }

            if (deviceMapCompatibilityUtil.DeviceMapGraph._graphData != null)
            {
                var graphs = new Graphs();
                graphs.GraphData = ByteString.CopyFrom(deviceMapCompatibilityUtil.DeviceMapGraph._graphData);
                graphs.GraphDataType = GraphDataType.Ardk;
                graphs.GraphDataTypeVersion = 1;
                deviceMapProto.Graphs = graphs;
            }

            if (deviceMapCompatibilityUtil.DeviceMapNodes.Count > 0)
            {
                deviceMapProto.AnchorPayload = ByteString.CopyFrom(deviceMapCompatibilityUtil.DeviceMapNodes[0]._anchorPayload);
            }

            return deviceMapProto.ToByteArray();
        }

        private const string MapTypeLearnedFeatures = "KeyNet-BinHyNet";
        private static MapNodeDataType MapTypeStringToEnum(string mapType)
        {
            if (mapType == MapTypeLearnedFeatures)
            {
                return MapNodeDataType.LearnedFeatures;
            }
            return MapNodeDataType.Orb;
        }

        /// <summary>
        /// Binds types serialized under the old name ARDeviceMap to ARDeviceMapCompatibilityUtil so that
        /// existing serialized data (e.g. slick map .bin files) can still be deserialized after the rename.
        /// </summary>
        private sealed class ARDeviceMapRenameBinder : SerializationBinder
        {
            public override Type BindToType(string assemblyName, string typeName)
            {
                // Redirect types that were serialized as ARDeviceMap+... to ARDeviceMapCompatibilityUtil+...
                const string oldPrefix = "Niantic.Lightship.AR.Mapping.ARDeviceMap+";
                if (typeName != null && typeName.StartsWith(oldPrefix, StringComparison.Ordinal))
                {
                    var suffix = typeName.Substring(oldPrefix.Length);
                    var compatTypeName = "NianticSpatial.NSDK.AR.Utilities.ARDeviceMapCompatibilityUtil+" + suffix;
                    return Type.GetType(compatTypeName + ", " + typeof(ARDeviceMapCompatibilityUtil).Assembly.FullName);
                }

                return Type.GetType(typeName + ", " + assemblyName);
            }
        }
    }
}
