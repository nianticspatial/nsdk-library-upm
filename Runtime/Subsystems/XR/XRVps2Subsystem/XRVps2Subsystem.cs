// Copyright Niantic Spatial.

using System;
using System.Collections.Generic;
using NianticSpatial.NSDK.AR.PersistentAnchors;
using NianticSpatial.NSDK.AR.Utilities;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SubsystemsImplementation;
using UnityEngine.XR.ARSubsystems;

namespace NianticSpatial.NSDK.AR.XRSubsystems
{
    [PublicAPI]
    public class XRVps2Subsystem : TrackingSubsystem<XRPersistentAnchor, XRVps2Subsystem, XRVps2SubsystemDescriptor, XRVps2Subsystem.Provider>
    {
        public XRVps2Subsystem()
        {

        }

        /// <summary>
        /// If true, universal localization is enabled. Universal localization provides geographic positioning that
        /// works anywhere without requiring pre-scanned maps.
        /// </summary>
        public bool UniversalLocalizationEnabled
        {
            set => provider.UniversalLocalizationEnabled = value;
        }

        /// <summary>
        /// Number of requests per second to send the cloud for universal localization.
        /// </summary>
        public float UniversalLocalizationRequestsPerSecond
        {
            set => provider.UniversalLocalizationRequestsPerSecond = value;
        }

        /// <summary>
        /// If true, localization on VPS maps is enabled. VPS maps only exist in pre-scanned areas. Your device must
        /// be localized on a VPS map in order for VPS anchors to be placed with high accuracy.
        /// </summary>
        public bool VpsMapLocalizationEnabled
        {
            set => provider.VpsMapLocalizationEnabled = value;
        }

        /// <summary>
        /// Number of VPS localization requests per second to send the server prior to the first successful
        /// localization on a VPS map.
        /// </summary>
        public float InitialVpsRequestsPerSecond
        {
            set => provider.InitialVpsRequestsPerSecond = value;
        }

        /// <summary>
        /// Number of VPS localization requests per second to send the server while successfully localized
        /// on a VPS map.
        /// </summary>
        public float ContinuousVpsRequestsPerSecond
        {
            set => provider.ContinuousVpsRequestsPerSecond = value;
        }

        /// <summary>
        /// If true, enables smooth interpolation between VPS2 transformer updates. As VPS2 updates its estimate
        /// from one transformer to another, having interpolation enabled will provide a more stable
        /// experience, especially when GPS or compass readings update abruptly.
        /// </summary>
        public bool GeolocationSmoothingEnabled
        {
            set => provider.GeolocationSmoothingEnabled = value;
        }

        /// <summary>
        /// Get the latest VPS2 transformer. The transformer contains the metadata required to perform bidirectional
        /// conversions between the application’s AR tracking space and the global coordinate system
        /// (lat/lng/alt/heading).
        /// </summary>
        /// <note>
        /// The transformer is a point-in-time snapshot. Updates to VPS2’s estimate are not reflected in
        /// previously retrieved transformer instances. Call this method again each frame to get
        /// the most up-to-date estimates.
        /// </note>
        /// <returns></returns>
        public XRVps2Transformer GetLatestTransformer()
        {
            return provider.GetLatestTransformer();
        }

        /// <summary>
        /// Convert a pose in the device's AR tracking space to a position and orientation in the global coordinate
        /// system.
        /// </summary>
        /// <param name="transformer"></param>
        /// <param name="pose"></param>
        /// <returns></returns>
        public XRVps2Geolocation GetGeolocation(XRVps2Transformer transformer, Pose pose)
        {
            return provider.GetGeolocation(transformer, pose);
        }

        /// <summary>
        /// Convert a position and orientation in the global coordinate system to a pose in the device's AR tracking space.
        /// </summary>
        /// <param name="transformer"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="altitude"></param>
        /// <param name="orientationEdn"></param>
        /// <returns></returns>
        public XRVps2Pose GetPose(XRVps2Transformer transformer, double latitude, double longitude, double altitude, Quaternion orientationEdn)
        {
            return provider.GetPose(transformer, latitude, longitude, altitude, orientationEdn);
        }

        /// <summary>
        /// Create a new anchor located at <paramref name="pose"/> in the device's AR tracking space.
        /// </summary>
        /// <param name="pose"></param>
        /// <returns></returns>
        public bool TryCreateAnchor(Pose pose, out XRPersistentAnchor anchor)
        {
            return provider.TryCreateAnchor(pose, out anchor);
        }

        /// <summary>
        /// Start tracking the anchor encoded by <paramref name="payload"/>.
        /// </summary>
        /// <param name="anchorPayload"></param>
        /// <returns></returns>
        public bool TryTrackAnchor(string anchorPayload, out XRPersistentAnchor anchor)
        {
            return provider.TryTrackAnchor(anchorPayload, out anchor);
        }

        /// <summary>
        /// Stop tracking an anchor.
        /// </summary>
        /// <param name="anchorId"></param>
        public bool TryRemoveAnchor(TrackableId anchorId)
        {
            return provider.TryRemoveAnchor(anchorId);
        }

        /// <summary>
        /// Get the payload which encodes the anchor. The payload can be used to re-track the anchor in a different
        /// VPS2 session.
        /// </summary>
        /// <param name="trackableId"></param>
        /// <returns></returns>
        public bool GetAnchorPayload(TrackableId trackableId, out string payload)
        {
            return provider.GetAnchorPayload(trackableId, out payload);
        }

        public override TrackableChanges<XRPersistentAnchor> GetChanges(Allocator allocator)
        {
            return provider.GetChanges(allocator);
        }

        /// <summary>
        /// Get all the network request records since the last time this method was called.
        /// </summary>
        /// <returns></returns>
        public List<XRVps2NetworkRequestRecord> GetLatestNetworkRequestRecords()
        {
            return provider.GetLatestNetworkRequestRecords();
        }

        public abstract class Provider : SubsystemProvider<XRVps2Subsystem>
        {
            public virtual bool UniversalLocalizationEnabled
            {
                set => throw new NotSupportedException();
            }

            public virtual float UniversalLocalizationRequestsPerSecond
            {
                set => throw new NotSupportedException();
            }

            public virtual bool VpsMapLocalizationEnabled
            {
                set => throw new NotSupportedException();
            }

            public virtual float InitialVpsRequestsPerSecond
            {
                set => throw new NotSupportedException();
            }

            public virtual float ContinuousVpsRequestsPerSecond
            {
                set => throw new NotSupportedException();
            }

            public virtual bool GeolocationSmoothingEnabled
            {
                set => throw new NotSupportedException();
            }

            public virtual XRVps2Transformer GetLatestTransformer()
            {
                throw new NotSupportedException();
            }

            public virtual XRVps2Geolocation GetGeolocation(XRVps2Transformer transformer, Pose pose)
            {
                throw new NotSupportedException();
            }

            public virtual XRVps2Pose GetPose(XRVps2Transformer transformer, double latitude, double longitude, double altitude, Quaternion orientationEdn)
            {
                throw new NotSupportedException();
            }

            public virtual bool TryCreateAnchor(Pose pose, out XRPersistentAnchor anchor)
            {
                throw new NotSupportedException();
            }

            public virtual bool TryTrackAnchor(string anchorPayload, out XRPersistentAnchor anchor)
            {
                throw new NotSupportedException();
            }

            public virtual bool TryRemoveAnchor(TrackableId trackableId)
            {
                throw new NotSupportedException();
            }

            public virtual bool GetAnchorPayload(TrackableId trackableId, out string payload)
            {
                throw new NotSupportedException();
            }

            public virtual TrackableChanges<XRPersistentAnchor> GetChanges(Allocator allocator)
            {
                throw new NotSupportedException();
            }

            public virtual List<XRVps2NetworkRequestRecord> GetLatestNetworkRequestRecords()
            {
                throw new NotSupportedException();
            }
        }


    }
}
