// Copyright 2022-2026 Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using NianticSpatial.NSDK.AR.Utilities;
using UnityEngine;

namespace NianticSpatial.NSDK.AR.VPS2
{
    /// <summary>
    /// The ARVps2AnchorPayload is data used to save and restore persistent anchors.
    /// </summary>
    [PublicAPI]
    [Serializable]
    public class ARVps2AnchorPayload
    {
        /// <summary>
        /// The data associated with the payload, decoded into bytes.
        /// </summary>
        public byte[] Data;

        /// <summary>
        /// Creates a new ARVps2AnchorPayload
        /// </summary>
        /// <param name="data">The data associated with the payload</param>
        public ARVps2AnchorPayload(byte[] data)
        {
            Data = data;
        }

        /// <summary>
        /// Creates a new ARVps2AnchorPayload
        /// </summary>
        /// <param name="data">The base 64 string to create the payload from</param>
        public ARVps2AnchorPayload(string data)
        {
            var bytes = new Span<byte>(new byte[data.Length]);
            bool valid = Convert.TryFromBase64String(data, bytes, out int bytesWritten);
            if (valid)
            {
                Data = bytes[..bytesWritten].ToArray();
            }
            else
            {
                Log.Error($"Failed to create ARVps2AnchorPayload due to invalid payload data: {data}");
            }
        }

        /// <summary>
        /// Converts a payload to a base 64 string.
        /// </summary>
        /// <returns>The string representation of the payload.  Returns null if no data exists in the payload.</returns>
        public string ToBase64()
        {
            if (Data != null)
            {
                return Convert.ToBase64String(Data);
            }
            else
            {
                return null;
            }
        }
    }
}
