// Copyright 2022-2026 Niantic Spatial.

using UnityEngine.XR.ARSubsystems;

namespace NianticSpatial.NSDK.AR.MapStorageAccess
{
    public readonly struct MapNode
    {
        private readonly TrackableId _nodeId;
        private readonly byte[] _data;

        public MapNode(TrackableId nodeId, byte[] data)
        {
            _nodeId = nodeId;
            _data = data;
        }

        public TrackableId GetNodeId()
        {
            return _nodeId;
        }

        // For internal use. Do not modify this data
        public byte[] GetData()
        {
            return _data;
        }
    }
}
