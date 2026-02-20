// Copyright 2022-2026 Niantic Spatial.

namespace NianticSpatial.NSDK.AR.Mapping
{
    public readonly struct XRDeviceMapGraph
    {
        private readonly byte[] _data;

        public XRDeviceMapGraph(byte[] data)
        {
            _data = data;
        }

        public byte[] CopyData()
        {
            return _data;
        }
    }
}
