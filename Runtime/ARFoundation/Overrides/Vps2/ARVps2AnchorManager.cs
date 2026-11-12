// Copyright 2026 Niantic Spatial.

using System;
using System.Linq;
using NianticSpatial.NSDK.AR.Common;
using NianticSpatial.NSDK.AR.Utilities;
using NianticSpatial.NSDK.AR.XRSubsystems;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace NianticSpatial.NSDK.AR.VPS2
{
    /// <summary>
    /// A trackable manager surfacing VPS2 anchors as <see cref="ARVps2Anchor"/> trackables.
    /// </summary>
    /// <remarks>
    /// <see cref="ARVps2Manager"/> requires this component next to it and forwards its
    /// anchor methods here, so it is the usual entry point. Trackables live under the XR
    /// origin, so their transforms are in Unity world space. Disabling this component
    /// removes all anchors.
    ///
    /// Anchors mark a place in the world that content should stay attached to. VPS2 must
    /// localize to a map before anchors can be created for it: use
    /// <see cref="ARVps2Manager.TryLocalize(string)"/> with a site ID, or its lat/long
    /// overload.
    /// </remarks>
    [PublicAPI]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(NsdkARUpdateOrder.Vps2AnchorManager)]
    public class ARVps2AnchorManager : ARTrackableManager<
        XRVps2AnchorSubsystem,
        XRVps2AnchorSubsystemDescriptor,
        XRVps2AnchorSubsystem.Provider,
        XRVps2Anchor,
        ARVps2Anchor>
    {
        [Tooltip("The GameObject to use when creating an anchor. If null, a new GameObject will be created.")]
        [SerializeField]
        private GameObject _defaultAnchorGameObject;

        /// <summary>
        /// Gets the prefab to instantiate for each <see cref="ARVps2Anchor"/>.
        /// </summary>
        /// <returns>The prefab, or null to create an empty GameObject.</returns>
        protected override GameObject GetPrefab() => _defaultAnchorGameObject;

        /// <summary>
        /// The name assigned to each instantiated <see cref="ARVps2Anchor"/> GameObject.
        /// </summary>
        protected override string gameObjectName => "VPS2 Persistent Anchor";

        /// <summary>
        /// Attempts to create a new anchor at <paramref name="localPose"/>.
        /// </summary>
        /// <remarks>
        /// The pose is in AR session space, not Unity world space: it is forwarded to the
        /// subsystem unmodified, unlike <c>ARAnchorManager</c>, which takes a world-space
        /// pose. The pose of <see cref="ARVps2Manager.TryGetAssetTrackingData"/> is already
        /// in AR session space and can be passed as is. To anchor at a world-space pose,
        /// such as the transform of an <see cref="ARVps2Asset"/> or any other trackable,
        /// convert it first with <c>origin.TrackablesParent.InverseTransformPose</c>.
        ///
        /// An anchor created here will only track with the asset of the site that was active
        /// at the time of the anchor's creation.
        /// </remarks>
        /// <param name="localPose">The pose, in AR session space, of the anchor.</param>
        /// <param name="anchorOut">The created anchor.</param>
        /// <returns><c>true</c> if the anchor was created; otherwise <c>false</c>.</returns>
        public bool TryCreateAnchor(Pose localPose, out ARVps2Anchor anchorOut)
        {
            if (subsystem != null && subsystem.TryCreateAnchor(localPose, out var anchor))
            {
                anchorOut = CreateTrackableImmediate(anchor);
                return true;
            }

            anchorOut = null;
            return false;
        }

        /// <summary>
        /// Starts tracking the anchor encoded by <paramref name="anchorPayload"/>.
        /// </summary>
        /// <remarks>
        /// Use this only to restore an AR anchor the app created itself with
        /// <see cref="TryCreateAnchor"/> and saved with <see cref="TryGetAnchorPayload"/>.
        /// @note An anchor payload is no longer the preferred method to identify the map to
        /// localize against: target a map by site ID with
        /// <see cref="ARVps2Manager.TryLocalize(string)"/> or by coordinate with its
        /// lat/long overload. An anchor is tied to the site asset it was created against, so
        /// it will not resolve against a different site or a different asset of the same
        /// site.
        /// </remarks>
        /// <param name="anchorPayload">The payload which encodes the anchor.</param>
        /// <param name="anchorOut">The tracked anchor.</param>
        /// <returns><c>true</c> if the anchor was accepted for tracking; otherwise <c>false</c>.</returns>
        public bool TryTrackAnchor(string anchorPayload, out ARVps2Anchor anchorOut)
        {
            if (subsystem != null && subsystem.TryTrackAnchor(anchorPayload, out var anchor))
            {
                anchorOut = CreateTrackableImmediate(anchor);
                return true;
            }

            anchorOut = null;
            return false;
        }

        /// <summary>
        /// Stops tracking an anchor and destroys its trackable.
        /// </summary>
        /// <remarks>
        /// This removes one of the app's own anchors. It is not how localization to a map
        /// is stopped: use <see cref="ARVps2Manager.TryStopLocalize"/> to drop a site's
        /// assets from the localization targets.
        /// </remarks>
        /// <param name="anchor">The anchor to remove.</param>
        public void RemoveAnchor(ARVps2Anchor anchor)
        {
            if (anchor == null)
            {
                throw new ArgumentNullException(nameof(anchor));
            }

            if (subsystem == null)
            {
                return;
            }

            // Function surfaces log for failure
            subsystem.TryRemoveAnchor(anchor.trackableId);

            // Safe to call because the function no-ops if anchor is not pending addition
            DestroyPendingTrackable(anchor.trackableId);
        }

        /// <summary>
        /// Gets the payload which encodes the anchor. The payload can be used to re-track
        /// the anchor in a different VPS2 session.
        /// </summary>
        /// <remarks>
        /// @note An anchor is tied to the site asset it was created against, so it will not
        /// resolve against a different site or a different asset of the same site. To start
        /// tracking an anchor in a different session, first localize to its site with
        /// <see cref="ARVps2Manager.TryLocalize(string)"/>, then call
        /// <see cref="TryTrackAnchor"/>.
        /// </remarks>
        /// <param name="anchor">The anchor whose payload to get.</param>
        /// <param name="payload">Receives the payload on success.</param>
        /// <returns><c>true</c> if the payload is available; otherwise <c>false</c>.</returns>
        public bool TryGetAnchorPayload(ARVps2Anchor anchor, out string payload)
        {
            if (anchor == null)
            {
                throw new ArgumentNullException(nameof(anchor));
            }

            if (subsystem == null)
            {
                payload = null;
                return false;
            }

            return subsystem.GetAnchorPayload(anchor.trackableId, out payload);
        }

        protected override void OnDisable()
        {
            RemoveAllAnchorsImmediate();
            base.OnDisable();
        }

        /// <summary>
        /// Removes every anchor and destroys its trackable in place, without waiting for
        /// removed changes: those never arrive once this manager or the VPS2 session
        /// stops updating.
        /// </summary>
        internal void RemoveAllAnchorsImmediate()
        {
            var trackables = m_Trackables.Values.ToArray();
            foreach (var trackable in trackables)
            {
                m_PendingAdds.Remove(trackable.trackableId);
                m_Trackables.Remove(trackable.trackableId);

                // The trackable is null when its GameObject was already destroyed
                // directly, without going through RemoveAnchor.
                if (trackable != null)
                {
                    Destroy(trackable.gameObject);
                }
            }

            subsystem?.RemoveAllAnchors();
        }
    }
}
