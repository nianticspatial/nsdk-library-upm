// Copyright 2022-2026 Niantic Spatial.
using NianticSpatial.NSDK.AR.PersistentAnchors;
using NianticSpatial.NSDK.AR.Utilities;
using NianticSpatial.NSDK.AR.VpsCoverage;

using UnityEngine;

namespace NianticSpatial.NSDK.AR.LocationAR
{
    /// <summary>
    /// The ARLocation is the digital twin of the physical location
    /// </summary>
    [PublicAPI("apiref/Niantic/Lightship/AR/LocationAR/ARLocation")]
    public class ARLocation : MonoBehaviour
    {
        /// <summary>
        /// The payload associated with the ARLocation
        /// </summary>
        public ARPersistentAnchorPayload Payload;

        [SerializeField]
        internal GameObject MeshContainer;

        [SerializeField]
        internal bool IncludeMeshInBuild;

        [SerializeField]
        internal LatLng GpsLocation;

#if UNITY_EDITOR
        [SerializeField]
        internal ARLocationManifest ARLocationManifest;

        [SerializeField]
        internal string AssetGuid;
#endif
    }
}
