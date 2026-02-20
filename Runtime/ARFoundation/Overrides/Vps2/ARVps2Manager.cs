// Copyright 2026 Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.Common;
using NianticSpatial.NSDK.AR.Utilities;
using NianticSpatial.NSDK.AR.XRSubsystems;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using NianticSpatial.NSDK.AR.PersistentAnchors;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace NianticSpatial.NSDK.AR
{
    [PublicAPI]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(NsdkARUpdateOrder.Vps2Manager)]
    public class ARVps2Manager : ARTrackableManager<
        XRVps2Subsystem,
        XRVps2SubsystemDescriptor,
        XRVps2Subsystem.Provider,
        XRPersistentAnchor,
        ARPersistentAnchor>
    {
        [SerializeField]
        private bool _universalLocalizationEnabled = true;

        [SerializeField]
        private float _universalLocalizationRequestsPerSecond = 4;

        [SerializeField]
        private bool _vpsMapLocalizationEnabled = true;

        [SerializeField]
        private float _initialVpsRequestsPerSecond = 4;

        [SerializeField]
        private float _continuousVpsRequestsPerSecond = 1;

        [SerializeField]
        private bool _geolocationSmoothingEnabled = true;

        public bool UniversalLocalizationEnabled
        {
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
            set
            {
                _geolocationSmoothingEnabled = value;
                if (subsystem != null)
                {
                    subsystem.GeolocationSmoothingEnabled = value;
                }
            }
        }

        public bool TryGetLatestTransformer(out XRVps2Transformer transformerOut)
        {
            if (subsystem == null)
            {
                transformerOut = new XRVps2Transformer();
                return false;
            }

            var transformer = subsystem.GetLatestTransformer();
            if (transformer.TrackingState == Vps2TrackingState.Unavailable)
            {
                transformerOut = new XRVps2Transformer();
                return false;
            }

            transformerOut = transformer;
            return true;
        }

        public bool TryGetGeolocation(XRVps2Transformer transformer, Pose pose, out XRVps2Geolocation geolocationOut)
        {
            if (subsystem == null)
            {
                geolocationOut = new XRVps2Geolocation();
                return false;
            }

            geolocationOut = subsystem.GetGeolocation(transformer, pose);
            return true;
        }

        public bool TryGetPose
        (
            XRVps2Transformer transformer,
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

            poseOut = subsystem.GetPose(transformer, latitude, longitude, altitude, orientationEdn);
            return true;
        }

        public bool TryCreateAnchor(Pose localPose, out ARPersistentAnchor anchorOut)
        {
            if (subsystem != null && subsystem.TryCreateAnchor(localPose, out var anchor))
            {
                anchorOut = CreateTrackableImmediate(anchor);
                return true;
            }

            anchorOut = null;
            return false;
        }

        public bool TryTrackAnchor(string anchorPayload, out ARPersistentAnchor anchorOut)
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

        public bool TryRemoveAnchor(ARPersistentAnchor anchor)
        {
            if (anchor == null)
            {
                throw new ArgumentNullException(nameof(anchor));
            }

            if (subsystem == null)
            {
                return false;
            }

            // TODO
            return subsystem.TryRemoveAnchor(anchor.trackableId);
        }

        public bool TryGetAnchorPayload(ARPersistentAnchor anchor, out string payload)
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
        }

        private new ARPersistentAnchor CreateTrackableImmediate(XRPersistentAnchor xrPersistentAnchor)
        {
            var trackableId = xrPersistentAnchor.trackableId;
            if (base.m_Trackables.TryGetValue(trackableId, out var trackable))
            {
                return trackable;
            }

            return base.CreateTrackableImmediate(xrPersistentAnchor);
        }

        protected override string gameObjectName => "VPS2 Persistent Anchor";
    }
}
