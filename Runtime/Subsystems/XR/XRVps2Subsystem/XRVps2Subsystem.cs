// Copyright Niantic Spatial.

using System;
using System.Collections.Generic;
using Niantic.Lightship.AR.Protobuf;
using NianticSpatial.NSDK.AR.Utilities;
using UnityEngine;
using UnityEngine.SubsystemsImplementation;

namespace NianticSpatial.NSDK.AR.XRSubsystems
{
    /// <summary>
    /// The subsystem owning the VPS2 feature's lifecycle: configuration, localization,
    /// and geolocation. Anchors and asset origins are served by the
    /// <see cref="XRVps2AnchorSubsystem"/> and <see cref="XRVps2AssetSubsystem"/>
    /// trackable subsystems, which run against the feature this subsystem manages.
    /// </summary>
    [PublicAPI]
    public class XRVps2Subsystem : SubsystemWithProvider<XRVps2Subsystem, XRVps2SubsystemDescriptor, XRVps2Subsystem.Provider>
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
        /// If true (and <see cref="VpsDebuggerEnabled"/>), attach a 180x135 JPEG thumbnail of each
        /// camera frame submitted with a VPS map localization request to the VPS debugger data.
        /// </summary>
        public bool VpsDebuggerSavingFrameImageEnabled
        {
            set => provider.VpsDebuggerSavingFrameImageEnabled = value;
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
        /// Pass 0 to use the default value (100 meters geo-corrected, 200 meters GPS-only).
        /// Pass -1 to disable the distance check entirely.
        /// </summary>
        public float AnchorDistanceGateMeters
        {
            set => provider.AnchorDistanceGateMeters = value;
        }

        /// <summary>
        /// If true, enable the image blur check on the cloud localization path.
        /// Advisory only: a frame whose sharpness score is below MinSharpnessScore produces
        /// a FrameFlagged localization request record (error ImageTooBlurry) so the app can
        /// warn the user, but is still sent for localization, never rejected.
        /// Default false (disabled).
        /// </summary>
        public bool BlurDetectionEnabled
        {
            set => provider.BlurDetectionEnabled = value;
        }

        /// <summary>
        /// Sharpness threshold below which a frame is flagged as too blurry.
        /// Higher scores mean sharper images. The default 0 flags nothing, since the score is
        /// always non-negative. Only used when BlurDetectionEnabled is true.
        /// </summary>
        public float MinSharpnessScore
        {
            set => provider.MinSharpnessScore = value;
        }

        /// <summary>
        /// How far outside a VPS area's geographic bounds the device can be and still start
        /// localizing against that area, in meters. Covers ordinary GPS error, so a device
        /// standing just outside an area still picks it up. An area the server published no
        /// bounds for is always a candidate.
        /// Pass 0 to use the default value (25 meters). Pass a negative value for no margin,
        /// judging the bounds exactly.
        /// </summary>
        public float AreaTargetingEnterMarginMeters
        {
            set => provider.AreaTargetingEnterMarginMeters = value;
        }

        /// <summary>
        /// How far outside its bounds an area already being localized against is kept, in meters.
        /// Larger than AreaTargetingEnterMarginMeters on purpose: the gap between the two is what
        /// stops a device sitting on the boundary between two areas swapping between them on every
        /// position update. A value below the enter margin is raised to it.
        /// Pass 0 to use the default value (50 meters). Pass a negative value for no margin.
        /// </summary>
        public float AreaTargetingExitMarginMeters
        {
            set => provider.AreaTargetingExitMarginMeters = value;
        }

        /// <summary>
        /// If true, successive VPS localizations are temporally fused. Fusion smooths the
        /// localization stream, at the cost of latency reacting to a genuine pose change.
        /// Enabled by default.
        /// </summary>
        public bool TemporalFusionEnabled
        {
            set => provider.TemporalFusionEnabled = value;
        }

        /// <summary>
        /// If true, universal localization can localize in areas that have been mapped by a
        /// drone or other aerial capture. Requires <see cref="UniversalLocalizationEnabled"/>.
        /// There is currently no way to query aerial coverage; contact Niantic Spatial for
        /// details. Disabled by default.
        /// </summary>
        public bool AerialLocalizationEnabled
        {
            set => provider.AerialLocalizationEnabled = value;
        }

        /// <summary>
        /// Get the latest VPS2 localization. The localization contains the metadata required to perform bidirectional
        /// conversions between the application’s AR tracking space and the global coordinate system
        /// (lat/lng/alt/heading).
        /// </summary>
        /// <remarks>
        /// The localization relates the AR session space origin to a geographic reference point; it is not
        /// relative to the current camera pose. Convert specific poses and geolocations with
        /// <see cref="GetPose(XRVps2Localization, double, double, double, Quaternion)"/> and
        /// <see cref="TryGetGeolocation"/>.
        /// </remarks>
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
        /// <remarks>
        /// Equivalent to converting the last camera pose with <see cref="TryGetGeolocation"/>, with the
        /// heading derived according to <paramref name="headingMode"/>.
        /// </remarks>
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
        /// <remarks>
        /// The geolocation can be any location, not just the device's; the result is where it lies in AR
        /// session space, and does not depend on the current camera pose.
        /// In most cases you want the overload that uses the latest localization automatically; use
        /// this one only when a batch of geolocations must share a single snapshot.
        /// </remarks>
        /// <param name="localization">The localization snapshot to use for conversion.</param>
        /// <param name="latitude">WGS84 latitude, in degrees.</param>
        /// <param name="longitude">WGS84 longitude, in degrees.</param>
        /// <param name="altitude">Altitude in meters above the WGS84 ellipsoid (not mean sea level).</param>
        /// <param name="orientationEdn">Orientation relative to the local East-Down-North frame, passed to
        /// NSDK as is: it is not in Unity axes. <c>Quaternion.identity</c> aligns the pose with that frame.</param>
        /// <returns>The pose of the given geolocation in AR session space, with position in meters.</returns>
        public XRVps2Pose GetPose(XRVps2Localization localization, double latitude, double longitude, double altitude, Quaternion orientationEdn)
        {
            return provider.GetPose(localization, latitude, longitude, altitude, orientationEdn);
        }

        /// <summary>
        /// Convert a Unity LocationInfo to a pose in the device's AR tracking space using the provided localization.
        /// </summary>
        /// <remarks>
        /// <c>LocationInfo.altitude</c> is passed through unconverted, but NSDK expects altitude above the
        /// WGS84 ellipsoid. On iOS, Unity reports mean sea level altitude, so the resulting pose can be off
        /// vertically by tens of meters; use the overload taking an explicit ellipsoidal altitude instead.
        /// </remarks>
        /// <param name="localization">The localization snapshot to use for conversion.</param>
        /// <param name="location">The LocationInfo containing latitude, longitude, and altitude.</param>
        /// <returns>The AR pose corresponding to the given location.</returns>
        public XRVps2Pose GetPose(XRVps2Localization localization, LocationInfo location)
        {
            return provider.GetPose(localization, location.latitude, location.longitude, location.altitude, Quaternion.identity);
        }

        /// <summary>
        /// Convert a position and orientation in the global coordinate system to a pose in the device's AR tracking
        /// space, using the latest VPS2 localization.
        /// </summary>
        /// <remarks>
        /// The geolocation can be any location, not just the device's; the result does not depend on the
        /// current camera pose.
        /// </remarks>
        /// <param name="latitude">WGS84 latitude, in degrees.</param>
        /// <param name="longitude">WGS84 longitude, in degrees.</param>
        /// <param name="altitude">Altitude in meters above the WGS84 ellipsoid (not mean sea level).</param>
        /// <param name="orientationEdn">Orientation relative to the local East-Down-North frame, passed to
        /// NSDK as is: it is not in Unity axes. <c>Quaternion.identity</c> aligns the pose with that frame.</param>
        /// <param name="poseOut">The pose of the given geolocation in AR session space, with position in meters.</param>
        /// <returns>False if VPS2 has not localized yet, in which case <paramref name="poseOut"/> is default.</returns>
        public bool TryGetPose(double latitude, double longitude, double altitude, Quaternion orientationEdn, out XRVps2Pose poseOut)
        {
            return provider.TryGetPose(latitude, longitude, altitude, orientationEdn, out poseOut);
        }

        /// <summary>
        /// Convert a Unity LocationInfo to a pose in the device's AR tracking space, using the latest VPS2
        /// localization.
        /// </summary>
        /// <remarks>
        /// <c>LocationInfo.altitude</c> is passed through unconverted, but NSDK expects altitude above the
        /// WGS84 ellipsoid. On iOS, Unity reports mean sea level altitude, so the resulting pose can be off
        /// vertically by tens of meters; use the overload taking an explicit ellipsoidal altitude instead.
        /// </remarks>
        /// <param name="location">The LocationInfo containing latitude, longitude, and altitude.</param>
        /// <param name="poseOut">The AR pose corresponding to the given location.</param>
        /// <returns>False if VPS2 has not localized yet, in which case <paramref name="poseOut"/> is default.</returns>
        public bool TryGetPose(LocationInfo location, out XRVps2Pose poseOut)
        {
            return provider.TryGetPose(location.latitude, location.longitude, location.altitude, Quaternion.identity, out poseOut);
        }

        /// <summary>
        /// Convert a pose in the device's AR tracking space to a position and orientation in the global
        /// coordinate system, using the latest VPS2 localization. The inverse of
        /// <see cref="TryGetPose(double, double, double, Quaternion, out XRVps2Pose)"/>.
        /// </summary>
        /// <remarks>
        /// The heading is the direction of the pose's forward axis projected onto the horizontal plane,
        /// which matches <see cref="HeadingMode.CameraDirection"/> in <see cref="GetDeviceGeolocation"/>.
        /// <see cref="HeadingMode.DeviceTop"/> is not offered because it depends on the display
        /// orientation, which an arbitrary pose does not have. The heading is not meaningful when
        /// the forward axis points straight up or down, or close to it.
        /// The pose can be any pose, not just the camera's.
        /// </remarks>
        /// <param name="pose">The pose in AR session space (not composed with the XR origin), with position
        /// in meters.</param>
        /// <param name="geolocationOut">The geolocation of <paramref name="pose"/>.</param>
        /// <returns>False if VPS2 has not localized yet, in which case <paramref name="geolocationOut"/> is default.</returns>
        public bool TryGetGeolocation(Pose pose, out XRVps2Geolocation geolocationOut)
        {
            return provider.TryGetGeolocation(pose, out geolocationOut);
        }

        /// <summary>
        /// Requests localization to a site, identified by its site ID.
        /// </summary>
        /// <remarks>
        /// Localizes against the site's production-deployed VPS assets only. Asynchronous:
        /// observe the outcome via <see cref="GetLatestLocalization"/>'s localized asset and
        /// tracking state. Requests are additive, and submitted assets stay in the
        /// localization set until <see cref="TryStopLocalize"/> removes them; stopping and
        /// restarting the subsystem does not remove them. The subsystem must be running;
        /// a request made while it is stopped is rejected.
        /// </remarks>
        /// <param name="siteId">The identifier of the site to localize to.</param>
        /// <returns>True if the request was started.</returns>
        public bool TryLocalize(string siteId)
        {
            return provider.TryLocalize(siteId);
        }

        /// <summary>
        /// Requests localization to one VPS map asset of a site.
        /// </summary>
        /// <remarks>
        /// Like <see cref="TryLocalize(string)"/>, but <paramref name="assetId"/>
        /// narrows it to that single asset of any deployment, including non-production ones.
        /// <paramref name="assetId"/> is the VPS map asset's ID, as a UUID (32 hex digits,
        /// optionally <c>0x</c>-prefixed, or the canonical 8-4-4-4-12 form), not the ID of the
        /// asset group's mesh or splat. An empty string localizes against the site's
        /// production-deployed assets, exactly as <see cref="TryLocalize(string)"/> does. Any
        /// other value is rejected. When the site has no localizable asset with that ID, the
        /// request fails asynchronously. <see cref="TryStopLocalize"/> with the site's ID
        /// stops it. The subsystem must be running.
        /// </remarks>
        /// <param name="siteId">The identifier of the site the asset belongs to.</param>
        /// <param name="assetId">The VPS map asset's ID, or empty for the site's production
        /// assets.</param>
        /// <returns>True if the request was started.</returns>
        public bool TryLocalize(string siteId, string assetId)
        {
            return provider.TryLocalize(siteId, assetId);
        }

        /// <summary>
        /// Requests localization near a geographic coordinate.
        /// </summary>
        /// <remarks>
        /// Looks up every site holding VPS assets within <paramref name="radiusMeters"/> of the
        /// coordinate and localizes against their production-deployed assets only.
        /// Asynchronous: observe the
        /// outcome via <see cref="GetLatestLocalization"/>'s tracking state and localized asset.
        /// Requests are additive, and submitted assets stay in the localization set until
        /// <see cref="TryStopLocalize"/> removes them; stopping and restarting the subsystem
        /// does not remove them. The subsystem must be running; a request made while it is
        /// stopped is rejected.
        /// </remarks>
        /// <param name="latitude">Latitude of the search center, in the range [-90, 90].</param>
        /// <param name="longitude">Longitude of the search center, in the range [-180, 180].</param>
        /// <param name="radiusMeters">Search radius in meters. Zero uses the default value
        /// (100 m). Must not be negative.</param>
        /// <returns>True if the request was started.</returns>
        public bool TryLocalize(double latitude, double longitude, double radiusMeters = 0)
        {
            return provider.TryLocalize(latitude, longitude, radiusMeters);
        }

        /// <summary>
        /// Stops localizing against previously requested sites.
        /// </summary>
        /// <remarks>
        /// Unregisters the site's assets, removes their anchors, and removes the map data
        /// they seeded. Call it before the subsystem stops so the site's maps are not
        /// localization targets across a later start.
        /// Requests for the site still in progress are cancelled, and their localization
        /// request records end as failed.
        /// </remarks>
        /// <param name="siteId">The site to stop localizing to, or <c>null</c> for every
        /// site requested in this session.</param>
        /// <returns>True if the request was dispatched.</returns>
        public bool TryStopLocalize(string siteId = null)
        {
            return provider.TryStopLocalize(siteId);
        }

        /// <summary>
        /// Gets the pose of a VPS asset's origin in the device's AR tracking space.
        /// </summary>
        /// <remarks>
        /// The pose relates the asset's origin to the AR session space origin; it is not relative to the
        /// current camera pose.
        /// Polling drives tracking: the first call for an asset starts tracking its origin,
        /// so keep polling while this returns false. Data is only served while the asset is
        /// fully tracked; false also covers lapsed tracking and an asset id not yet known to
        /// the session (e.g. a <see cref="TryLocalize(string)"/> for its site is still in
        /// flight).
        /// </remarks>
        /// <param name="trackingData">The asset's tracking data.</param>
        /// <param name="assetId">The identifier of the VPS map asset, or null for the asset the
        /// device is currently localized to (see <see cref="XRVps2Localization.LocalizedAsset"/>).
        /// Only null selects the localized asset; an empty id returns false.</param>
        /// <returns>True if tracking data is available.</returns>
        public bool TryGetAssetTrackingData(out XRVps2AssetTrackingData trackingData, string assetId = null)
        {
            return provider.TryGetAssetTrackingData(out trackingData, assetId);
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

            public virtual bool VpsDebuggerSavingFrameImageEnabled
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

            public virtual bool BlurDetectionEnabled
            {
                set => throw new NotSupportedException();
            }

            public virtual float MinSharpnessScore
            {
                set => throw new NotSupportedException();
            }

            public virtual float AreaTargetingEnterMarginMeters
            {
                set => throw new NotSupportedException();
            }

            public virtual float AreaTargetingExitMarginMeters
            {
                set => throw new NotSupportedException();
            }

            public virtual bool TemporalFusionEnabled
            {
                set => throw new NotSupportedException();
            }

            public virtual bool AerialLocalizationEnabled
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

            public virtual bool TryGetPose(double latitude, double longitude, double altitude, Quaternion orientationEdn, out XRVps2Pose poseOut)
            {
                throw new NotSupportedException();
            }

            public virtual bool TryGetPose(LocationInfo location, out XRVps2Pose poseOut)
            {
                return TryGetPose(location.latitude, location.longitude, location.altitude, Quaternion.identity, out poseOut);
            }

            public virtual bool TryGetGeolocation(Pose pose, out XRVps2Geolocation geolocationOut)
            {
                throw new NotSupportedException();
            }

            public virtual bool TryLocalize(string siteId)
            {
                throw new NotSupportedException();
            }

            public virtual bool TryLocalize(string siteId, string assetId)
            {
                throw new NotSupportedException();
            }

            public virtual bool TryLocalize(double latitude, double longitude, double radiusMeters = 0)
            {
                throw new NotSupportedException();
            }

            public virtual bool TryStopLocalize(string siteId = null)
            {
                throw new NotSupportedException();
            }

            public virtual bool TryGetAssetTrackingData(out XRVps2AssetTrackingData trackingData, string assetId = null)
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
