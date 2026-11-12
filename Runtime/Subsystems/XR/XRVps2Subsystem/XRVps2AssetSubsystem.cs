// Copyright 2026 Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.Utilities;
using Unity.Collections;
using UnityEngine.SubsystemsImplementation;
using UnityEngine.XR.ARSubsystems;

namespace NianticSpatial.NSDK.AR.XRSubsystems
{
    /// <summary>
    /// A tracking subsystem serving the origins of VPS assets as trackables.
    /// </summary>
    /// <remarks>
    /// The asset the device is localized to (the localized asset of
    /// <see cref="XRVps2Subsystem.GetLatestLocalization"/>) is tracked automatically;
    /// other assets known to the session can be requested with
    /// <see cref="TryTrackAsset"/>. An asset appears in the changes once its origin is
    /// first tracked, and afterwards degrades to <c>TrackingState.Limited</c> with its
    /// last known pose rather than being removed. Poses are in AR session space.
    /// </remarks>
    [PublicAPI]
    public class XRVps2AssetSubsystem : TrackingSubsystem<XRVps2Asset, XRVps2AssetSubsystem,
        XRVps2AssetSubsystemDescriptor, XRVps2AssetSubsystem.Provider>
    {
        /// <summary>
        /// Requests tracking of a VPS map asset's origin, identified by its asset ID.
        /// </summary>
        /// <remarks>
        /// The asset must be known to the session: assets become known as a localize
        /// request fetches them, so this can be called as soon as the asset's site has
        /// been submitted with <see cref="XRVps2Subsystem.TryLocalize(string)"/>. The
        /// asset surfaces through <see cref="GetChanges"/> once its origin is first
        /// tracked.
        /// </remarks>
        /// <param name="assetId">The identifier of the VPS map asset.</param>
        /// <param name="asset">Receives the asset's current session-relative data.</param>
        /// <returns>True if the request was accepted; false if the id is not a valid
        /// asset id.</returns>
        public bool TryTrackAsset(string assetId, out XRVps2Asset asset)
        {
            return provider.TryTrackAsset(assetId, out asset);
        }

        /// <summary>
        /// Get the changes (added, updated, and removed) to tracked assets since the last
        /// call to this method.
        /// </summary>
        /// <param name="allocator">The allocator to use for the returned changes.</param>
        /// <returns>The changes since the last call.</returns>
        public override TrackableChanges<XRVps2Asset> GetChanges(Allocator allocator)
        {
            return provider.GetChanges(allocator);
        }

        /// <summary>
        /// The provider API for <see cref="XRVps2AssetSubsystem"/>-derived classes to implement.
        /// </summary>
        public abstract class Provider : SubsystemProvider<XRVps2AssetSubsystem>
        {
            /// <summary>
            /// Requests tracking of a VPS map asset's origin, identified by its asset ID.
            /// </summary>
            /// <param name="assetId">The identifier of the VPS map asset.</param>
            /// <param name="asset">Receives the asset's current session-relative data.</param>
            /// <returns>True if the request was accepted.</returns>
            public virtual bool TryTrackAsset(string assetId, out XRVps2Asset asset)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Get the changes (added, updated, and removed) to tracked assets since the
            /// last call to this method.
            /// </summary>
            /// <param name="allocator">The allocator to use for the returned changes.</param>
            /// <returns>The changes since the last call.</returns>
            public virtual TrackableChanges<XRVps2Asset> GetChanges(Allocator allocator)
            {
                throw new NotSupportedException();
            }
        }
    }
}
