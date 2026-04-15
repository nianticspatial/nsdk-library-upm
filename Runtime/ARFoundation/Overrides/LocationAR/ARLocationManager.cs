// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Collections.Generic;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using NianticSpatial.NSDK.AR.Utilities;
using NianticSpatial.NSDK.AR.VPS2;
using NianticSpatial.NSDK.AR.XRSubsystems;

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace NianticSpatial.NSDK.AR.LocationAR
{
    /// <summary>
    /// The ARLocationManager is used to track ARLocations.  ARLocations tie digital content to the physical world.
    /// When you start tracking an ARLocation, and aim your phone's camera at the physical location,
    /// the digital content that you child to the ARLocation will appear in the physical world.
    /// </summary>
    [PublicAPI("apiref/Niantic/Lightship/AR/LocationAR/ARLocationManager/")]
    public class ARLocationManager : ARVps2Manager
    {
        [Header("AR Locations")]
        [Tooltip("Whether or not to auto-track the currently selected location.  Auto-tracked locations will be enabled, including their children, when the camera is aimed at the physical location.")]
        [SerializeField]
        private bool _autoTrack = false;

        /// <summary>
        /// Gets all of the ARLocations childed to the ARLocationManager.
        /// </summary>
        public ARLocation[] ARLocations => GetComponentsInChildren<ARLocation>(true);

        /// <summary>
        /// Whether or not to automatically start tracking the selected ARLocation.
        /// If true, the location that is currently enabled will be automatically tracked on Start.
        /// </summary>
        public bool AutoTrack => _autoTrack;

        /// <summary>
        /// Get all the active ARVps2Anchors created by ARLocationManager
        /// </summary>
        public Dictionary<ARVps2Anchor, ARLocation> AnchorToARLocationMap => _anchorToARLocationMap;

        // _targetARLocations hold the locations specified by SetARLocations().
        private readonly List<ARLocation> _targetARLocations = new();

        // _trackedARLocations holds the locations actively being tracked.
        // _trackedARLocations will be populated by HandleAnchorChanged()
        private readonly List<ARLocation> _trackedARLocations = new();

        // _anchorToARLocationMap holds the relationship between each anchor and its corresponding location
        // ARVps2Manager updates anchors and then we use this map to determine what location corresponds to what anchor update
        // ARLocationManager will only consume a subset of the anchor updates. What ar locations will be tracked is determined by _trackedARLocations
        // _anchorToARLocationMap will be populated by StartARLocationTracking()
        private readonly Dictionary<ARVps2Anchor, ARLocation> _anchorToARLocationMap = new();

        // _originalParents holds the transform that each location's anchor replaces
        private readonly Dictionary<ARVps2Anchor, Transform> _originalParents = new();

        protected override void OnEnable()
        {
            // This will invoke OnBeforeStart and then start the subsystem. Put any initialization logic
            //  dependent on the subsystem in OnBeforeStart.
            base.OnEnable();
            trackablesChanged.AddListener(HandleTrackablesChanged);
        }

        protected override void OnDisable()
        {
            StopARLocationTracking();
            base.OnDisable();
            trackablesChanged.RemoveListener(HandleTrackablesChanged);
        }

        protected virtual void Start()
        {
            var arLocations = new List<ARLocation>();
            foreach (var arLocation in ARLocations)
            {
                if (_autoTrack && arLocation.gameObject.activeSelf)
                {
                    arLocation.gameObject.SetActive(false);
                    arLocations.Add(arLocation);
                }
                else //TODO: Do this at build time, not in start here
                {
                    arLocation.gameObject.SetActive(false);
                }
            }

            if (arLocations.Count != 0)
            {
                SetARLocations(arLocations.ToArray());
                StartARLocationTracking();
            }
        }

        /// <summary>
        /// Selects the AR Locations to try to track when StartARLocationTracking() is called.
        /// </summary>
        /// <param name="arLocations">The locations to try to track.</param>
        public void SetARLocations(params ARLocation[] arLocations)
        {
            if (_anchorToARLocationMap.Count > 0)
            {
                Log.Error("Cannot set AR locations.  ARLocationManager must not be in tracking.");
                return;
            }
            _targetARLocations.Clear();
            _targetARLocations.AddRange(arLocations);
        }

        /// <summary>
        /// Starts tracking locations specified by SetARLocations() or set in Unity Editor
        ///
        /// Content authored as children of the ARLocation will be enabled once the ARLocation becomes tracked.
        /// This will create digital content in the physical world.
        ///
        /// If no locations were specified in SetARLocations(), requests will be made to attempt to track nearby
        /// locations. In this case, multiple nearby locations may be targeted and the first one to successfully
        /// track will be used.
        /// </summary>
        public void StartARLocationTracking()
        {
            if (_targetARLocations.Count == 0)
            {
                Log.Warning("ARLocationManager has no target ARLocations. Either set them using SetARLocations(locations), or enable auto-tracking.");
                return;
            }

            TryTrackLocations(_targetARLocations.ToArray());
        }

        /// <summary>
        /// Stops tracking all currently tracked locations.
        /// </summary>
        public void StopARLocationTracking()
        {
            if (_anchorToARLocationMap.Count == 0)
            {
                return;
            }

            ClearLocationManagerState();
        }

        private void HandleTrackablesChanged(ARTrackablesChangedEventArgs<ARVps2Anchor> args)
        {
            foreach (var anchor in args.added)
            {
                HandleAnchorChanged(anchor);
            }

            foreach (var anchor in args.updated)
            {
                HandleAnchorChanged(anchor);
            }

            foreach (var (_, anchor) in args.removed)
            {
                HandleAnchorChanged(anchor);
            }
        }

        private void HandleAnchorChanged(ARVps2Anchor anchor)
        {
            using (new ScopedProfiler("OnLocationTracked"))
            {
                if (!_anchorToARLocationMap.ContainsKey(anchor))
                {
                    // ARLocation/Anchor is not longer tracked
                    return;
                }

                var arLocation = _anchorToARLocationMap[anchor];

                // TrackingState.Limited is not acceptable here because this manager renders anchors in the AR world,
                // which requires the precision that comes with TrackingState.Tracking
                if (anchor.trackingState == TrackingState.Tracking)
                {
                    if (!_trackedARLocations.Contains(arLocation))
                    {
                        _trackedARLocations.Add(arLocation);
                    }

                    arLocation.gameObject.SetActive(true);
                }
                else
                {
                    // If the anchor is removed
                    if (anchor.trackingStateReason == Vps2AnchorTrackingStateReason.Removed)
                    {
                        ParentARLocationToOriginal(arLocation, anchor);
                        _anchorToARLocationMap.Remove(anchor);

                        // If the last tracked location is removed, clear state and be ready for the next StartARLocationTracking()
                        if (_anchorToARLocationMap.Count == 0)
                        {
                            ClearLocationManagerState();
                        }
                    }
                }
            }
        }

        private void TryTrackLocations(params ARLocation[] arLocations)
        {
            if (_anchorToARLocationMap.Count != 0)
            {
                Log.Error(
                    $"You are already tracking {_anchorToARLocationMap.Count} locations.  Call StopARLocationTracking() before attempting to track a new location." +
                    gameObject);
                return;
            }

            foreach (var arLocation in arLocations)
            {
                var payload = arLocation.Payload?.ToBase64();
                bool success = TryTrackAnchor(payload, out var anchor);
                if (success)
                {
                    _originalParents.Add(anchor, arLocation.transform.parent);
                    arLocation.transform.SetParent(anchor.transform, false);
                    _anchorToARLocationMap.Add(anchor, arLocation);
                }
                else
                {
#if UNITY_EDITOR
                    Log.Error($"Failed to track anchor." +
                        $"{Environment.NewLine}" +
                        $"In-Editor Playback uses Standalone XR Plug-in Management. Try enabling \"Niantic Nsdk\" for Standalone in XR Plug-in Management under Project Settings. And verify validity of Playback dataset" +
                        gameObject);
#else
                    Log.Error($"Failed to track anchor." + arLocation.gameObject);
#endif
                }
            }
        }

        // Move ARLocation's GameObject back to where originally in the scenegraph hierarchy
        private void ParentARLocationToOriginal(ARLocation arLocation, ARVps2Anchor anchor)
        {
            arLocation.gameObject.SetActive(false);
            if (_originalParents.TryGetValue(anchor, out Transform parent))
            {
                // Don't do anything if the parent is null
                if (parent == null)
                {
                    _originalParents.Remove(anchor);
                    return;
                }

                // If the anchor (current parent) is deactivated this frame, we can't move the arlocation
                //  from under it. However... the anchor is deactivated when it is about the be removed,
                //  so we will never be able to reparent the arlocation at scene close.
                // Instead, developers have to call StopARLocationTracking() before scene close.
                if (arLocation.transform.parent.gameObject.activeInHierarchy)
                {
                    arLocation.transform.SetParent(parent, false);
                }
                else
                {
                    Log.Warning("ARLocation cannot be reparented at scene close, because the anchor is deactivated. " +
                        "Call StopARLocationTracking() before scene close to avoid this problem.");
                }
            }

            _originalParents.Remove(anchor);
        }

        private void ClearLocationManagerState()
        {
            // Reparent ARLocations back to original parents before clearing state,
            // so they aren't destroyed when anchors are removed
            foreach (var (anchor, arLocation) in _anchorToARLocationMap)
            {
                ParentARLocationToOriginal(arLocation, anchor);
            }

            _trackedARLocations.Clear();
            _anchorToARLocationMap.Clear();
            _originalParents.Clear();
        }

        private new void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}
