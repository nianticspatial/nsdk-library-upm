// Copyright 2026 Niantic Spatial.

using NianticSpatial.NSDK.AR.Common;
using NianticSpatial.NSDK.AR.Utilities;
using NianticSpatial.NSDK.AR.XRSubsystems;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace NianticSpatial.NSDK.AR.VPS2
{
    /// <summary>
    /// A trackable manager surfacing the origins of VPS assets as
    /// <see cref="ARVps2Asset"/> trackables.
    /// </summary>
    /// <remarks>
    /// The localized asset of <see cref="ARVps2Manager.TryGetLatestLocalization"/> is
    /// tracked automatically; other assets known to the session can be requested with
    /// <see cref="TryTrackAsset"/>. Trackables live under the XR origin, so their
    /// transforms are in Unity world space and follow the asset origin as VPS refines the
    /// localization. An asset whose tracking lapses degrades to
    /// <c>TrackingState.Limited</c> with its last known pose rather than being removed.
    /// </remarks>
    [PublicAPI]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(NsdkARUpdateOrder.Vps2AssetManager)]
    public class ARVps2AssetManager : ARTrackableManager<
        XRVps2AssetSubsystem,
        XRVps2AssetSubsystemDescriptor,
        XRVps2AssetSubsystem.Provider,
        XRVps2Asset,
        ARVps2Asset>
    {
        [Tooltip("The GameObject to instantiate at a tracked asset's origin. If null, an empty GameObject is created.")]
        [SerializeField]
        private GameObject _defaultAssetGameObject;

        /// <summary>
        /// Gets the prefab to instantiate for each <see cref="ARVps2Asset"/>.
        /// </summary>
        /// <returns>The prefab, or null to create an empty GameObject.</returns>
        protected override GameObject GetPrefab() => _defaultAssetGameObject;

        /// <summary>
        /// The name assigned to each instantiated <see cref="ARVps2Asset"/> GameObject.
        /// </summary>
        protected override string gameObjectName => "VPS2 Asset";

        /// <summary>
        /// Requests tracking of a VPS map asset's origin, identified by its asset ID.
        /// </summary>
        /// <remarks>
        /// The asset must be known to the session: assets become known as a localize
        /// request fetches them, so this can be called as soon as the asset's site has
        /// been submitted with <see cref="ARVps2Manager.TryLocalize(string)"/>. The
        /// returned trackable starts with <c>TrackingState.None</c> at the scene origin
        /// and snaps to the asset's origin when tracking data first arrives. Requesting
        /// an asset that is already tracked returns its existing trackable.
        /// </remarks>
        /// <param name="assetId">The identifier of the VPS map asset.</param>
        /// <param name="assetOut">The asset's trackable.</param>
        /// <returns><c>true</c> if the request was accepted; <c>false</c> if the
        /// subsystem is unavailable or the id is not a valid asset id.</returns>
        public bool TryTrackAsset(string assetId, out ARVps2Asset assetOut)
        {
            if (subsystem == null || !subsystem.TryTrackAsset(assetId, out var asset))
            {
                assetOut = null;
                return false;
            }

            if (m_Trackables.TryGetValue(asset.trackableId, out assetOut))
            {
                return true;
            }

            assetOut = CreateTrackableImmediate(asset);
            return true;
        }
    }
}
