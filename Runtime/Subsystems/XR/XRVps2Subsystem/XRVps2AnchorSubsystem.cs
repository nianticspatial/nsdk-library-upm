// Copyright 2026 Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.Utilities;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SubsystemsImplementation;
using UnityEngine.XR.ARSubsystems;

namespace NianticSpatial.NSDK.AR.XRSubsystems
{
    /// <summary>
    /// A tracking subsystem serving VPS2 anchors as trackables.
    /// </summary>
    /// <remarks>
    /// The VPS2 feature's lifecycle is owned by <see cref="XRVps2Subsystem"/>; this
    /// subsystem only creates, tracks, and removes anchors against it. Poses are in AR
    /// session space.
    ///
    /// Anchors mark a place in the world that content should stay attached to; maps are
    /// targeted by site ID or coordinate through <see cref="XRVps2Subsystem"/>.
    /// </remarks>
    [PublicAPI]
    public class XRVps2AnchorSubsystem : TrackingSubsystem<XRVps2Anchor, XRVps2AnchorSubsystem,
        XRVps2AnchorSubsystemDescriptor, XRVps2AnchorSubsystem.Provider>
    {
        /// <summary>
        /// Create a new anchor located at <paramref name="pose"/> in the device's AR tracking space.
        /// </summary>
        /// <param name="pose">The pose, in AR session space, of the anchor.</param>
        /// <param name="anchor">The created anchor.</param>
        /// <returns>True if the anchor was created.</returns>
        public bool TryCreateAnchor(Pose pose, out XRVps2Anchor anchor)
        {
            return provider.TryCreateAnchor(pose, out anchor);
        }

        /// <summary>
        /// Start tracking the anchor encoded by <paramref name="anchorPayload"/>.
        /// </summary>
        /// <remarks>
        /// For restoring an anchor the app created itself.
        /// @note An anchor is tied to the site asset it was created against, so it will not
        /// resolve against a different site or a different asset of the same site; target a
        /// map through <see cref="XRVps2Subsystem"/> by site ID or coordinate instead.
        /// </remarks>
        /// <param name="anchorPayload">The payload which encodes the anchor.</param>
        /// <param name="anchor">The tracked anchor.</param>
        /// <returns>True if the anchor was accepted for tracking.</returns>
        public bool TryTrackAnchor(string anchorPayload, out XRVps2Anchor anchor)
        {
            return provider.TryTrackAnchor(anchorPayload, out anchor);
        }

        /// <summary>
        /// Stop tracking an anchor. The anchor surfaces one removed change through
        /// <see cref="GetChanges"/> before it is forgotten.
        /// </summary>
        /// <param name="anchorId">The id of the anchor to remove.</param>
        /// <returns>True if the anchor was removed.</returns>
        public bool TryRemoveAnchor(TrackableId anchorId)
        {
            return provider.TryRemoveAnchor(anchorId);
        }

        /// <summary>
        /// Get the payload which encodes the anchor. The payload can be used to re-track
        /// the anchor in a different VPS2 session.
        /// </summary>
        /// <remarks>
        /// @note An anchor is tied to the site asset it was created against: a later session
        /// must already be localizing to that site before the payload can be tracked.
        /// </remarks>
        /// <param name="trackableId">The id of the anchor.</param>
        /// <param name="payload">Receives the payload on success.</param>
        /// <returns>True if the payload is available.</returns>
        public bool GetAnchorPayload(TrackableId trackableId, out string payload)
        {
            return provider.GetAnchorPayload(trackableId, out payload);
        }

        /// <summary>
        /// Get the changes (added, updated, and removed) to anchors since the last call
        /// to this method.
        /// </summary>
        /// <param name="allocator">The allocator to use for the returned changes.</param>
        /// <returns>The changes since the last call.</returns>
        public override TrackableChanges<XRVps2Anchor> GetChanges(Allocator allocator)
        {
            return provider.GetChanges(allocator);
        }

        /// <summary>
        /// Stops tracking every anchor at once, without surfacing removed changes. Used
        /// by the managers when tearing all anchors down immediately.
        /// </summary>
        internal void RemoveAllAnchors()
        {
            provider.RemoveAllAnchors();
        }

        /// <summary>
        /// The provider API for <see cref="XRVps2AnchorSubsystem"/>-derived classes to implement.
        /// </summary>
        public abstract class Provider : SubsystemProvider<XRVps2AnchorSubsystem>
        {
            /// <summary>
            /// Create a new anchor located at <paramref name="pose"/> in the device's AR tracking space.
            /// </summary>
            /// <param name="pose">The pose, in AR session space, of the anchor.</param>
            /// <param name="anchor">The created anchor.</param>
            /// <returns>True if the anchor was created.</returns>
            public virtual bool TryCreateAnchor(Pose pose, out XRVps2Anchor anchor)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Start tracking the anchor encoded by <paramref name="anchorPayload"/>.
            /// </summary>
            /// <param name="anchorPayload">The payload which encodes the anchor.</param>
            /// <param name="anchor">The tracked anchor.</param>
            /// <returns>True if the anchor was accepted for tracking.</returns>
            public virtual bool TryTrackAnchor(string anchorPayload, out XRVps2Anchor anchor)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Stop tracking an anchor.
            /// </summary>
            /// <param name="trackableId">The id of the anchor to remove.</param>
            /// <returns>True if the anchor was removed.</returns>
            public virtual bool TryRemoveAnchor(TrackableId trackableId)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Get the payload which encodes the anchor.
            /// </summary>
            /// <param name="trackableId">The id of the anchor.</param>
            /// <param name="payload">Receives the payload on success.</param>
            /// <returns>True if the payload is available.</returns>
            public virtual bool GetAnchorPayload(TrackableId trackableId, out string payload)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Get the changes (added, updated, and removed) to anchors since the last
            /// call to this method.
            /// </summary>
            /// <param name="allocator">The allocator to use for the returned changes.</param>
            /// <returns>The changes since the last call.</returns>
            public virtual TrackableChanges<XRVps2Anchor> GetChanges(Allocator allocator)
            {
                throw new NotSupportedException();
            }

            /// <summary>
            /// Stops tracking every anchor at once, without surfacing removed changes.
            /// </summary>
            public virtual void RemoveAllAnchors()
            {
                throw new NotSupportedException();
            }
        }
    }
}
