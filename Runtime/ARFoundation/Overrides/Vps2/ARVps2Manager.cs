// Copyright 2026 Niantic Spatial.

using System;
using System.Linq;
using Niantic.Lightship.AR.Protobuf;
using NianticSpatial.NSDK.AR.Common;
using NianticSpatial.NSDK.AR.Utilities;
using NianticSpatial.NSDK.AR.XRSubsystems;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

namespace NianticSpatial.NSDK.AR.VPS2
{
    [PublicAPI]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(NsdkARUpdateOrder.Vps2Manager)]
    public class ARVps2Manager : ARTrackableManager<
        XRVps2Subsystem,
        XRVps2SubsystemDescriptor,
        XRVps2Subsystem.Provider,
        XRVps2Anchor,
        ARVps2Anchor>
    {
        [Tooltip("The GameObject to use when creating an anchor.  If null, a new GameObject will be created.")]
        [SerializeField]
        private GameObject _defaultAnchorGameObject;

        [Header("Configuration")]
        [SerializeField]
        private bool _universalLocalizationEnabled = true;

        [SerializeField]
        private float _universalLocalizationRequestsPerSecond = 1;

        [SerializeField]
        private bool _vpsMapLocalizationEnabled = true;

        [SerializeField]
        private float _initialVpsRequestsPerSecond = 1;

        [SerializeField]
        private float _continuousVpsRequestsPerSecond = 0.2f;

        [SerializeField]
        private bool _geolocationSmoothingEnabled = true;

        [SerializeField]
        private bool _deviceMapLocalizationEnabled = false;

        [SerializeField]
        private int _deviceMapLocalizationFramerate = 10;

        [SerializeField]
        private bool _vpsDebuggerEnabled = false;

        public bool UniversalLocalizationEnabled
        {
            get => _universalLocalizationEnabled;
            set
            {
                _universalLocalizationEnabled = value;
                if (subsystem != null)
                {
                    subsystem.UniversalLocalizationEnabled = value;
                }
            }
        }

        public float UniversalLocalizationRequestsPerSecond
        {
            get => _universalLocalizationRequestsPerSecond;
            set
            {
                _universalLocalizationRequestsPerSecond = value;
                if (subsystem != null)
                {
                    subsystem.UniversalLocalizationRequestsPerSecond = value;
                }
            }
        }

        public bool VpsMapLocalizationEnabled
        {
            get => _vpsMapLocalizationEnabled;
            set
            {
                _vpsMapLocalizationEnabled = value;
                if (subsystem != null)
                {
                    subsystem.VpsMapLocalizationEnabled = value;
                }
            }
        }

        public float InitialVpsRequestsPerSecond
        {
            get => _initialVpsRequestsPerSecond;
            set
            {
                _initialVpsRequestsPerSecond = value;
                if (subsystem != null)
                {
                    subsystem.InitialVpsRequestsPerSecond = value;
                }
            }
        }

        public float ContinuousVpsRequestsPerSecond
        {
            get => _continuousVpsRequestsPerSecond;
            set
            {
                _continuousVpsRequestsPerSecond = value;
                if (subsystem != null)
                {
                    subsystem.ContinuousVpsRequestsPerSecond = value;
                }
            }
        }

        public bool GeolocationSmoothingEnabled
        {
            get => _geolocationSmoothingEnabled;
            set
            {
                _geolocationSmoothingEnabled = value;
                if (subsystem != null)
                {
                    subsystem.GeolocationSmoothingEnabled = value;
                }
            }
        }

        public bool VpsDebuggerEnabled
        {
            get => _vpsDebuggerEnabled;
            set
            {
                _vpsDebuggerEnabled = value;
                if (subsystem != null)
                {
                    subsystem.VpsDebuggerEnabled = value;
                }
            }
        }

        public bool DeviceMapLocalizationEnabled
        {
            get => _deviceMapLocalizationEnabled;
            set
            {
                _deviceMapLocalizationEnabled = value;
                if (subsystem != null)
                {
                    subsystem.DeviceMapLocalizationEnabled = value;
                }
            }
        }

        public int DeviceMapLocalizationFramerate
        {
            get => _deviceMapLocalizationFramerate;
            set
            {
                _deviceMapLocalizationFramerate = value;
                if (subsystem != null)
                {
                    subsystem.DeviceMapLocalizationFramerate = value;
                }
            }
        }

        protected override GameObject GetPrefab() => _defaultAnchorGameObject;
        protected override string gameObjectName => "VPS2 Persistent Anchor";

        public event Action<XRVps2LocalizationRequestRecord> LocalizationRequestRecordAdded;
        public event Action<VpsDebuggerDataEvent> DebuggerDataReceived;

        public bool TryGetLatestLocalization(out XRVps2Localization localizationOut)
        {
            if (subsystem == null)
            {
                localizationOut = new XRVps2Localization();
                return false;
            }

            var localization = subsystem.GetLatestLocalization();
            if (localization.TrackingState == Vps2TrackingState.Unavailable)
            {
                localizationOut = new XRVps2Localization();
                return false;
            }

            localizationOut = localization;
            return true;
        }

        /// <summary>
        /// Attempts to get the geolocation of the device's last known camera pose.
        /// </summary>
        /// <param name="geolocationOut">Receives the geolocation data on success.</param>
        /// <param name="headingMode">Controls how the heading is derived.
        /// Use <see cref="HeadingMode.CameraDirection"/> when the device is upright,
        /// or <see cref="HeadingMode.DeviceTop"/> when flat or for a compass-style heading.</param>
        /// <returns><c>true</c> if the subsystem is available; otherwise <c>false</c>.</returns>
        public bool TryGetDeviceGeolocation(out XRVps2Geolocation geolocationOut,
            HeadingMode headingMode = HeadingMode.CameraDirection)
        {
            if (subsystem == null)
            {
                geolocationOut = new XRVps2Geolocation();
                return false;
            }

            geolocationOut = subsystem.GetDeviceGeolocation(headingMode);
            return true;
        }

        public bool TryGetPose
        (
            XRVps2Localization localization,
            double latitude,
            double longitude,
            double altitude,
            Quaternion orientationEdn,
            out XRVps2Pose poseOut
        )
        {
            if (subsystem == null)
            {
                poseOut = new XRVps2Pose();
                return false;
            }

            poseOut = subsystem.GetPose(localization, latitude, longitude, altitude, orientationEdn);
            return true;
        }

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

        public bool TryTrackAnchor(string anchorPayload, out ARVps2Anchor anchorOut)
        {
            if (subsystem != null)
            {
                if (subsystem.TryTrackAnchor(anchorPayload, out var anchor))
                {
                    anchorOut = CreateTrackableImmediate(anchor);
                    return true;
                }
            }

            anchorOut = null;
            return false;
        }

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

        public bool GetSessionId(out string sessionId)
        {
            if (subsystem == null)
            {
                sessionId = null;
                return false;
            }

            return subsystem.GetSessionId(out sessionId);
        }


        protected override void OnDisable()
        {
            RemoveAllAnchorsImmediate();
            base.OnDisable();
        }

        private void RemoveAllAnchorsImmediate()
        {
            var trackables = m_Trackables.Values.ToArray();
            foreach (var trackable in trackables)
            {
                m_PendingAdds.Remove(trackable.trackableId);
                m_Trackables.Remove(trackable.trackableId);
                subsystem?.TryRemoveAnchor(trackable.trackableId);
                Destroy(trackable.gameObject);
            }
        }

        // Callback before the subsystem is started (but after it is created).
        // Pushes the serialized configuration to the subsystem.
        protected override void OnBeforeStart()
        {
            subsystem.UniversalLocalizationEnabled = _universalLocalizationEnabled;
            subsystem.UniversalLocalizationRequestsPerSecond = _universalLocalizationRequestsPerSecond;
            subsystem.VpsMapLocalizationEnabled = _vpsMapLocalizationEnabled;
            subsystem.InitialVpsRequestsPerSecond = _initialVpsRequestsPerSecond;
            subsystem.ContinuousVpsRequestsPerSecond = _continuousVpsRequestsPerSecond;
            subsystem.GeolocationSmoothingEnabled = _geolocationSmoothingEnabled;
            subsystem.VpsDebuggerEnabled = _vpsDebuggerEnabled;
            subsystem.DeviceMapLocalizationEnabled = _deviceMapLocalizationEnabled;
            subsystem.DeviceMapLocalizationFramerate = _deviceMapLocalizationFramerate;
        }

        protected override void Update()
        {
            base.Update();

            if (subsystem == null) return;

            var newRecords = subsystem.GetLatestLocalizationRequestRecords();
            foreach (var record in newRecords)
            {
                LocalizationRequestRecordAdded?.Invoke(record);
            }

            var newData = subsystem.GetLatestDebuggerData();
            foreach (var data in newData)
            {
                DebuggerDataReceived?.Invoke(data);
            }
        }
    }
}
