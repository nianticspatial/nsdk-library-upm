// Copyright Niantic Spatial.

using System;
using System.Collections.Generic;
using Niantic.Lightship.AR.Protobuf;
using NianticSpatial.NSDK.AR.Utilities;
using Unity.Collections;
using UnityEngine;
using UnityEngine.SubsystemsImplementation;
using UnityEngine.XR.ARSubsystems;

namespace NianticSpatial.NSDK.AR.XRSubsystems
{
    [PublicAPI]
    public class XRVps2Subsystem : TrackingSubsystem<XRVps2Anchor, XRVps2Subsystem, XRVps2SubsystemDescriptor, XRVps2Subsystem.Provider>
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
        /// If true, enables smooth interpolation between VPS2 localization updates. As VPS2 updates its estimate
        /// from one localization to another, having interpolation enabled will provide a more stable
        /// experience, especially when GPS or compass readings update abruptly.
        /// </summary>
        public bool GeolocationSmoothingEnabled
        {
            set => provider.GeolocationSmoothingEnabled = value;
        }

        /// <summary>
        /// If true, localization on device maps is enabled. Device maps are created by the on-device mapping feature.
        /// They do not provide a georeference.
        /// </summary>
        public bool DeviceMapLocalizationEnabled
        {
            set => provider.DeviceMapLocalizationEnabled = value;
        }

        /// <summary>
        /// Number of times per second to attempt localization on available device maps.
        /// </summary>
        public int DeviceMapLocalizationFramerate
        {
            set => provider.DeviceMapLocalizationFramerate = value;
        }

        /// <summary>
        /// If true, enable the VPS Debugger, which will log VPS2 related internal events to diagnose VPS2 tracking
        /// behavior and issues.
        /// </summary>
        public bool VpsDebuggerEnabled
        {
            set => provider.VpsDebuggerEnabled = value;
        }

        /// <summary>
        /// Timeout in milliseconds for universal localization requests.
        /// Responses in uncached areas may take 60+ seconds; subsequent requests are typically under 1 second.
        /// Pass 0 to use the default value (90000 ms).
        /// </summary>
        public int UniversalLocalizationRequestTimeoutMs
        {
            set => provider.UniversalLocalizationRequestTimeoutMs = value;
        }

        /// <summary>
        /// Maximum number of concurrent localization requests per target.
        /// Limits how many in-flight cloud localization requests can be active for a single target
        /// simultaneously. Pass 0 to use the default value (2).
        /// </summary>
        public uint MaxRequestsInTransitPerTarget
        {
            set => provider.MaxRequestsInTransitPerTarget = value;
        }

        /// <summary>
        /// Distance gate in meters for anchor candidate filtering when geo-corrected.
        /// Nodes farther than this distance from the device are skipped during anchor localization.
        /// When only GPS is available (no geo-correction), the effective gate is 2x this value.
        /// Pass 0 to use the default value (50 meters geo-corrected, 100 meters GPS-only).
        /// Pass -1 to disable the distance check entirely.
        /// </summary>
        public float AnchorDistanceGateMeters
        {
            set => provider.AnchorDistanceGateMeters = value;
        }

        /// <summary>
        /// Get the latest VPS2 localization. The localization contains the metadata required to perform bidirectional
        /// conversions between the application’s AR tracking space and the global coordinate system
        /// (lat/lng/alt/heading).
        /// </summary>
        /// <note>
        /// The localization is a point-in-time snapshot. Updates to VPS2’s estimate are not reflected in
        /// previously retrieved localization instances. Call this method again each frame to get
        /// the most up-to-date estimates.
        /// </note>
        /// <returns></returns>
        public XRVps2Localization GetLatestLocalization()
        {
            return provider.GetLatestLocalization();
        }

        /// <summary>
        /// Get the geolocation of the device's last known camera pose.
        /// </summary>
        /// <param name="headingMode">Controls how the heading is derived.
        /// Use <see cref="HeadingMode.CameraDirection"/> when the device is upright,
        /// or <see cref="HeadingMode.DeviceTop"/> when flat or for a compass-style heading.</param>
        /// <returns>Geolocation data. Check tracking state for availability.</returns>
        public XRVps2Geolocation GetDeviceGeolocation(
            HeadingMode headingMode = HeadingMode.CameraDirection)
        {
            return provider.GetDeviceGeolocation(headingMode);
        }

        /// <summary>
        /// Convert a position and orientation in the global coordinate system to a pose in the device's AR tracking space.
        /// </summary>
        /// <param name="localization"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="altitude"></param>
        /// <param name="orientationEdn"></param>
        /// <returns></returns>
        public XRVps2Pose GetPose(XRVps2Localization localization, double latitude, double longitude, double altitude, Quaternion orientationEdn)
        {
            return provider.GetPose(localization, latitude, longitude, altitude, orientationEdn);
        }

        /// <summary>
        /// Convert a Unity LocationInfo to a pose in the device's AR tracking space using the provided localization.
        /// </summary>
        /// <param name="localization">The localization snapshot to use for conversion.</param>
        /// <param name="location">The LocationInfo containing latitude, longitude, and altitude.</param>
        /// <returns>The AR pose corresponding to the given location.</returns>
        public XRVps2Pose GetPose(XRVps2Localization localization, LocationInfo location)
        {
            return provider.GetPose(localization, location.latitude, location.longitude, location.altitude, Quaternion.identity);
        }

        /// <summary>
        /// Create a new anchor located at <paramref name="pose"/> in the device's AR tracking space.
        /// </summary>
        /// <param name="pose"></param>
        /// <returns></returns>
        public bool TryCreateAnchor(Pose pose, out XRVps2Anchor anchor)
        {
            return provider.TryCreateAnchor(pose, out anchor);
        }

        /// <summary>
        /// Start tracking the anchor encoded by <paramref name="payload"/>.
        /// </summary>
        /// <param name="anchorPayload"></param>
        /// <returns></returns>
        public bool TryTrackAnchor(string anchorPayload, out XRVps2Anchor anchor)
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

        public override TrackableChanges<XRVps2Anchor> GetChanges(Allocator allocator)
        {
            return provider.GetChanges(allocator);
        }

        /// <summary>
        /// Get all the network request records since the last time this method was called.
        /// </summary>
        /// <returns></returns>
        public List<XRVps2LocalizationRequestRecord> GetLatestLocalizationRequestRecords()
        {
            return provider.GetLatestLocalizationRequestRecords();
        }

        /// <summary>
        /// Get all the debugger data since the last time this method was called.
        /// </summary>
        /// <returns></returns>
        public List<VpsDebuggerDataEvent> GetLatestDebuggerData()
        {
            return provider.GetLatestDebuggerData();
        }

        /// <summary>
        /// Get the session id, if one exists.
        /// </summary>
        /// <param name="sessionId">The session id as 32 character hexidecimal upper-case string.</param>
        /// <returns>True if a session id is present, false otherwise</returns>
        public bool GetSessionId(out string sessionId)
        {
            var success = provider.GetSessionId(out sessionId);
            return success;
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

            public virtual bool DeviceMapLocalizationEnabled
            {
                set => throw new NotSupportedException();
            }

            public virtual int DeviceMapLocalizationFramerate
            {
                set => throw new NotSupportedException();
            }

            public virtual bool VpsDebuggerEnabled
            {
                set => throw new NotSupportedException();
            }

            public virtual int UniversalLocalizationRequestTimeoutMs
            {
                set => throw new NotSupportedException();
            }

            public virtual uint MaxRequestsInTransitPerTarget
            {
                set => throw new NotSupportedException();
            }

            public virtual float AnchorDistanceGateMeters
            {
                set => throw new NotSupportedException();
            }

            public virtual XRVps2Localization GetLatestLocalization()
            {
                throw new NotSupportedException();
            }

            public virtual XRVps2Geolocation GetDeviceGeolocation(
                HeadingMode headingMode = HeadingMode.CameraDirection)
            {
                throw new NotSupportedException();
            }

            public virtual XRVps2Pose GetPose(XRVps2Localization localization, double latitude, double longitude, double altitude, Quaternion orientationEdn)
            {
                throw new NotSupportedException();
            }

            public virtual XRVps2Pose GetPose(XRVps2Localization localization, LocationInfo location)
            {
                return GetPose(localization, location.latitude, location.longitude, location.altitude, Quaternion.identity);
            }

            public virtual bool TryCreateAnchor(Pose pose, out XRVps2Anchor anchor)
            {
                throw new NotSupportedException();
            }

            public virtual bool TryTrackAnchor(string anchorPayload, out XRVps2Anchor anchor)
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

            public virtual TrackableChanges<XRVps2Anchor> GetChanges(Allocator allocator)
            {
                throw new NotSupportedException();
            }

            public virtual List<XRVps2LocalizationRequestRecord> GetLatestLocalizationRequestRecords()
            {
                throw new NotSupportedException();
            }

            public virtual List<VpsDebuggerDataEvent> GetLatestDebuggerData()
            {
                throw new NotSupportedException();
            }

            public virtual bool GetSessionId(out string sessionId)
            {
                throw new NotSupportedException();
            }
        }
    }
}
