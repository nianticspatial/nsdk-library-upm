// Copyright 2022-2026 Niantic Spatial.
using NianticSpatial.NSDK.AR.VPS2;
using NianticSpatial.NSDK.AR.Utilities;

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
        /// <remarks>
        /// Identifying a place by anchor payload is the legacy mechanism: an anchor is tied
        /// to the site asset it was created against, so it will not resolve against a
        /// different site or a different asset of the same site. New work should target a
        /// map by site ID with <c>ARVps2Manager.TryLocalize(string)</c>.
        /// </remarks>
        public ARVps2AnchorPayload Payload;

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
