// Copyright 2026 Niantic Spatial.

using System;
using Niantic.Lightship.AR.Protobuf;
using NianticSpatial.NSDK.AR.Common;
using NianticSpatial.NSDK.AR.Utilities;
using NianticSpatial.NSDK.AR.XRSubsystems;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.ARFoundation;

namespace NianticSpatial.NSDK.AR.VPS2
{
    /// <summary>
    /// The entry point to VPS2: configuration, localization, geolocation, anchors, and
    /// asset tracking.
    /// </summary>
    /// <remarks>
    /// This manager owns the VPS2 feature's lifecycle. The trackables are served by the
    /// <see cref="ARVps2AnchorManager"/> and <see cref="ARVps2AssetManager"/> components
    /// required next to it; the anchor and asset methods here forward to them.
    /// </remarks>
    [PublicAPI]
    [DisallowMultipleComponent]
    [DefaultExecutionOrder(NsdkARUpdateOrder.Vps2Manager)]
    [RequireComponent(typeof(ARVps2AnchorManager))]
    [RequireComponent(typeof(ARVps2AssetManager))]
    public class ARVps2Manager : SubsystemLifecycleManager<
        XRVps2Subsystem,
        XRVps2SubsystemDescriptor,
        XRVps2Subsystem.Provider>
    {
        [Header("Configuration")]
        [SerializeField]
        private bool _universalLocalizationEnabled = false;

        [SerializeField]
        private float _universalLocalizationRequestsPerSecond = 1;

        [Tooltip("Localize in areas mapped by a drone or other aerial capture. Requires Universal " +
            "Localization. Coverage cannot be queried yet; contact Niantic Spatial. Off by default.")]
        [SerializeField]
        private bool _aerialLocalizationEnabled = false;

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

        [Tooltip("If true (and VPS Debugger is enabled), attach 180x135 JPEG thumbnails of " +
            "the camera frames submitted with VPS map localization requests to the VPS " +
            "debugger data (roughly 3-5 KB per request).")]
        [SerializeField]
        private bool _vpsDebuggerSavingFrameImageEnabled = false;

        [Tooltip("Maximum concurrent localization requests per target. 0 = default (2).")]
        [SerializeField]
        private uint _maxRequestsInTransitPerTarget = 0;

        [Tooltip("Distance gate in meters for anchor candidate filtering when geo-corrected. " +
            "GPS-only gate is 2x this value. 0 = default (100m), -1 = disabled.")]
        [SerializeField]
        private float _anchorDistanceGateMeters = 0;

        [Tooltip("How far outside a VPS area's geographic bounds the device can be and still " +
            "start localizing against it, in meters. 0 = default (25m), negative = no margin.")]
        [SerializeField]
        private float _areaTargetingEnterMarginMeters = 0;

        [Tooltip("How far outside its bounds an area already being localized against is kept, " +
            "in meters. Values below the enter margin are raised to it. 0 = default (50m), " +
            "negative = no margin.")]
        [SerializeField]
        private float _areaTargetingExitMarginMeters = 0;

        [Tooltip("Enable the advisory image blur check on the cloud path. Blurry frames are " +
            "flagged (FrameFlagged / ImageTooBlurry) so the app can warn the user, but are " +
            "still sent for localization. Off by default.")]
        [SerializeField]
        private bool _blurDetectionEnabled = false;

        [Tooltip("Sharpness threshold below which a frame is flagged as too blurry. " +
            "Higher scores mean sharper images; 0 flags nothing. Only used when " +
            "Blur Detection Enabled is on.")]
        [SerializeField]
        private float _minSharpnessScore = 0;

        [Tooltip("Temporally fuse successive VPS localizations. Fusion smooths the localization " +
            "stream, at the cost of latency reacting to a genuine pose change. On by default.")]
        [SerializeField]
        private bool _temporalFusionEnabled = true;

        /// <summary>
        /// If true, enables cloud-based geopositioning: VPS2 sends camera imagery to the
        /// cloud to compute an improved geoposition and heading, beyond what local
        /// sensor fusion provides. Requires a network connection but no pre-scanned VPS
        /// maps. If false, geopositioning relies on local sensor fusion alone.
        /// </summary>
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

        /// <summary>
        /// Number of cloud geopositioning requests to send per second while
        /// <see cref="UniversalLocalizationEnabled"/> is true.
        /// </summary>
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

        /// <summary>
        /// If true, universal localization can localize in areas that have been mapped by a
        /// drone or other aerial capture. Requires <see cref="UniversalLocalizationEnabled"/>.
        /// There is currently no way to query aerial coverage; contact Niantic Spatial for
        /// details. Disabled by default.
        /// </summary>
        public bool AerialLocalizationEnabled
        {
            get => _aerialLocalizationEnabled;
            set
            {
                _aerialLocalizationEnabled = value;
                if (subsystem != null)
                {
                    subsystem.AerialLocalizationEnabled = value;
                }
            }
        }

        /// <summary>
        /// If true, localization on VPS maps is enabled. VPS maps only exist in
        /// pre-scanned areas. Your device must be localized on a VPS map in order for
        /// VPS anchors to be placed with high accuracy.
        /// </summary>
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

        /// <summary>
        /// Number of VPS localization requests per second to send the server prior to
        /// the first successful localization on a VPS map.
        /// </summary>
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

        /// <summary>
        /// Number of VPS localization requests per second to send the server after the
        /// first successful localization on a VPS map.
        /// </summary>
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

        /// <summary>
        /// If true, enables smooth interpolation between VPS2 localization updates. As
        /// VPS2 updates its estimate from one localization to another, having
        /// interpolation enabled will provide a more stable experience, especially when
        /// GPS or compass readings update abruptly.
        /// </summary>
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

        /// <summary>
        /// If true, enable the VPS Debugger, which will log VPS2 related internal events
        /// to diagnose VPS2 tracking behavior and issues.
        /// </summary>
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

        /// <summary>
        /// If true (and <see cref="VpsDebuggerEnabled"/>), attach a 180x135 JPEG
        /// thumbnail of each camera frame submitted with a VPS map localization request
        /// to the VPS debugger data, adding roughly 3-5 KB per request.
        /// </summary>
        public bool VpsDebuggerSavingFrameImageEnabled
        {
            get => _vpsDebuggerSavingFrameImageEnabled;
            set
            {
                _vpsDebuggerSavingFrameImageEnabled = value;
                if (subsystem != null)
                {
                    subsystem.VpsDebuggerSavingFrameImageEnabled = value;
                }
            }
        }

        /// <summary>
        /// If true, localization on device maps is enabled. Device maps are created by
        /// the on-device mapping feature. They do not provide a georeference.
        /// </summary>
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

        /// <summary>
        /// Number of times per second to attempt localization on available device maps.
        /// </summary>
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

        /// <summary>
        /// Maximum number of concurrent localization requests per target.
        /// Pass 0 to use the default value (2).
        /// </summary>
        public uint MaxRequestsInTransitPerTarget
        {
            get => _maxRequestsInTransitPerTarget;
            set
            {
                _maxRequestsInTransitPerTarget = value;
                if (subsystem != null)
                {
                    subsystem.MaxRequestsInTransitPerTarget = value;
                }
            }
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
            get => _anchorDistanceGateMeters;
            set
            {
                _anchorDistanceGateMeters = value;
                if (subsystem != null)
                {
                    subsystem.AnchorDistanceGateMeters = value;
                }
            }
        }

        /// <summary>
        /// If true, enable the image blur check on the cloud localization path.
        /// Advisory only: a frame whose sharpness score is below <see cref="MinSharpnessScore"/>
        /// produces a FrameFlagged localization request record (error ImageTooBlurry) so the app
        /// can warn the user, but is still sent for localization, never rejected.
        /// Default false (disabled).
        /// </summary>
        public bool BlurDetectionEnabled
        {
            get => _blurDetectionEnabled;
            set
            {
                _blurDetectionEnabled = value;
                if (subsystem != null)
                {
                    subsystem.BlurDetectionEnabled = value;
                }
            }
        }

        /// <summary>
        /// Sharpness threshold below which a frame is flagged as too blurry.
        /// Higher scores mean sharper images. The default 0 flags nothing, since the score is
        /// always non-negative. Only used when <see cref="BlurDetectionEnabled"/> is true.
        /// </summary>
        public float MinSharpnessScore
        {
            get => _minSharpnessScore;
            set
            {
                _minSharpnessScore = value;
                if (subsystem != null)
                {
                    subsystem.MinSharpnessScore = value;
                }
            }
        }

        /// <summary>
        /// How far outside a VPS area's geographic bounds the device can be and still start
        /// localizing against that area, in meters. Covers ordinary GPS error, so a device
        /// standing just outside an area still picks it up. An area the server published no
        /// bounds for is always a candidate.
        /// 0 uses the default (25 meters); a negative value means no margin, judging the bounds
        /// exactly.
        /// </summary>
        public float AreaTargetingEnterMarginMeters
        {
            get => _areaTargetingEnterMarginMeters;
            set
            {
                _areaTargetingEnterMarginMeters = value;
                if (subsystem != null)
                {
                    subsystem.AreaTargetingEnterMarginMeters = value;
                }
            }
        }

        /// <summary>
        /// How far outside its bounds an area already being localized against is kept, in meters.
        /// Larger than <see cref="AreaTargetingEnterMarginMeters"/> on purpose: the gap between
        /// the two is what stops a device sitting on the boundary between two areas swapping
        /// between them on every position update. A value below the enter margin is raised to it.
        /// 0 uses the default (50 meters); a negative value means no margin.
        /// </summary>
        public float AreaTargetingExitMarginMeters
        {
            get => _areaTargetingExitMarginMeters;
            set
            {
                _areaTargetingExitMarginMeters = value;
                if (subsystem != null)
                {
                    subsystem.AreaTargetingExitMarginMeters = value;
                }
            }
        }

        /// <summary>
        /// If true, successive VPS localizations are temporally fused. Fusion smooths the
        /// localization stream, at the cost of latency reacting to a genuine pose change.
        /// Enabled by default.
        /// </summary>
        public bool TemporalFusionEnabled
        {
            get => _temporalFusionEnabled;
            set
            {
                _temporalFusionEnabled = value;
                if (subsystem != null)
                {
                    subsystem.TemporalFusionEnabled = value;
                }
            }
        }

        /// <summary>
        /// Invoked for each new localization request record: the outcome of a network
        /// request or frame submission made by VPS2.
        /// </summary>
        public event Action<XRVps2LocalizationRequestRecord> LocalizationRequestRecordAdded;

        /// <summary>
        /// Invoked for each VPS Debugger event while <see cref="VpsDebuggerEnabled"/> is true.
        /// </summary>
        public event Action<VpsDebuggerDataEvent> DebuggerDataReceived;

        private ARVps2AnchorManager _anchorManager;
        private ARVps2AssetManager _assetManager;

        /// <summary>
        /// The trackable manager serving <see cref="ARVps2Anchor"/>s.
        /// </summary>
        /// <remarks>
        /// Required on the same GameObject. Added at runtime when missing, because
        /// <c>RequireComponent</c> only applies when this component is added in the
        /// editor, not when a scene saved without the manager loads.
        /// </remarks>
        public ARVps2AnchorManager AnchorManager =>
            _anchorManager != null ? _anchorManager : _anchorManager = EnsureComponent<ARVps2AnchorManager>();

        /// <summary>
        /// The trackable manager serving <see cref="ARVps2Asset"/>s.
        /// </summary>
        /// <remarks>
        /// Required on the same GameObject. Added at runtime when missing, because
        /// <c>RequireComponent</c> only applies when this component is added in the
        /// editor, not when a scene saved without the manager loads.
        /// </remarks>
        public ARVps2AssetManager AssetManager =>
            _assetManager != null ? _assetManager : _assetManager = EnsureComponent<ARVps2AssetManager>();

        /// <summary>
        /// The anchors served by <see cref="AnchorManager"/>.
        /// </summary>
        public TrackableCollection<ARVps2Anchor> trackables => AnchorManager.trackables;

        /// <summary>
        /// Invoked when anchors have changed: added, updated, or removed. Forwarded from
        /// <see cref="AnchorManager"/>.
        /// </summary>
        public UnityEvent<ARTrackablesChangedEventArgs<ARVps2Anchor>> trackablesChanged =>
            AnchorManager.trackablesChanged;

        /// <summary>
        /// Attempts to get the latest VPS2 localization, which carries the data required to
        /// convert between the AR tracking space and geolocation.
        /// </summary>
        /// <remarks>
        /// The localization relates the AR session space origin to a geographic reference point;
        /// it is not relative to the current camera pose. The geolocation fields are only valid while
        /// <see cref="XRVps2Localization.TrackingState"/> is not
        /// <see cref="Vps2TrackingState.Unavailable"/>. The localized asset is independent of
        /// the tracking state: an asset without a georeference is reported while the state
        /// stays unavailable.
        /// </remarks>
        /// <param name="localizationOut">Receives the localization on success.</param>
        /// <returns><c>true</c> if the subsystem exists; otherwise <c>false</c>.</returns>
        public bool TryGetLatestLocalization(out XRVps2Localization localizationOut)
        {
            if (subsystem == null)
            {
                localizationOut = new XRVps2Localization();
                return false;
            }

            localizationOut = subsystem.GetLatestLocalization();
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

        /// <summary>
        /// Attempts to convert a position and orientation in the global coordinate system to a
        /// pose in AR session space, using the supplied localization snapshot.
        /// </summary>
        /// <remarks>
        /// The pose is in AR session space, not Unity world space. To place a world-space
        /// transform at it, compose it with the XR origin first via
        /// <c>origin.TrackablesParent.TransformPose</c>. In most cases you want the overload
        /// that uses the latest localization automatically; use this one only when a batch of
        /// geolocations must share a single snapshot. Throws if
        /// <paramref name="localization"/> is unavailable. The geolocation can be any location,
        /// not just the device's; the result does not depend on the current camera pose.
        /// </remarks>
        /// <param name="localization">The localization snapshot to use for conversion.</param>
        /// <param name="latitude">WGS84 latitude, in degrees.</param>
        /// <param name="longitude">WGS84 longitude, in degrees.</param>
        /// <param name="altitude">Altitude in meters above the WGS84 ellipsoid (not mean sea level).</param>
        /// <param name="orientationEdn">Orientation relative to the local East-Down-North frame, passed to
        /// NSDK as is: it is not in Unity axes. <c>Quaternion.identity</c> aligns the pose with that frame.</param>
        /// <param name="poseOut">Receives the pose of the given geolocation in AR session space, with
        /// position in meters.</param>
        /// <returns><c>true</c> if the subsystem exists; otherwise <c>false</c>.</returns>
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

        /// <summary>
        /// Attempts to convert a position and orientation in the global coordinate system to a
        /// pose in AR session space, using the latest VPS2 localization.
        /// </summary>
        /// <remarks>
        /// The geolocation can be any location, not just the device's; the result does not depend
        /// on the current camera pose.
        /// </remarks>
        /// <param name="latitude">WGS84 latitude, in degrees.</param>
        /// <param name="longitude">WGS84 longitude, in degrees.</param>
        /// <param name="altitude">Altitude in meters above the WGS84 ellipsoid (not mean sea level).</param>
        /// <param name="orientationEdn">Orientation relative to the local East-Down-North frame, passed to
        /// NSDK as is: it is not in Unity axes. <c>Quaternion.identity</c> aligns the pose with that frame.</param>
        /// <param name="poseOut">Receives the pose of the given geolocation in AR session space, with
        /// position in meters.</param>
        /// <returns><c>false</c> if the subsystem is unavailable or VPS2 has not localized yet.</returns>
        public bool TryGetPose
        (
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

            return subsystem.TryGetPose(latitude, longitude, altitude, orientationEdn, out poseOut);
        }

        /// <summary>
        /// Attempts to convert a Unity LocationInfo to a pose in AR session space, using the
        /// latest VPS2 localization.
        /// </summary>
        /// <remarks>
        /// <c>LocationInfo.altitude</c> is passed through unconverted, but NSDK expects altitude above
        /// the WGS84 ellipsoid. On iOS, Unity reports mean sea level altitude, so the resulting pose can
        /// be off vertically by tens of meters; use the overload taking an explicit ellipsoidal altitude
        /// instead.
        /// </remarks>
        /// <param name="location">The LocationInfo containing latitude, longitude, and altitude.</param>
        /// <param name="poseOut">Receives the AR pose on success.</param>
        /// <returns><c>false</c> if the subsystem is unavailable or VPS2 has not localized yet.</returns>
        public bool TryGetPose(LocationInfo location, out XRVps2Pose poseOut)
        {
            if (subsystem == null)
            {
                poseOut = new XRVps2Pose();
                return false;
            }

            return subsystem.TryGetPose(location, out poseOut);
        }

        /// <summary>
        /// Attempts to convert a pose in AR session space to a position and orientation in the
        /// global coordinate system, using the latest VPS2 localization. The inverse of
        /// <see cref="TryGetPose(double, double, double, Quaternion, out XRVps2Pose)"/>.
        /// </summary>
        /// <remarks>
        /// The pose is in AR session space, not Unity world space. To convert a world-space
        /// transform, bring it into session space first via
        /// <c>origin.TrackablesParent.InverseTransformPose</c>. The heading is the direction of the
        /// pose's forward axis projected onto the horizontal plane, which matches
        /// <see cref="HeadingMode.CameraDirection"/> in <see cref="TryGetDeviceGeolocation"/>.
        /// <see cref="HeadingMode.DeviceTop"/> is not offered because it depends on the display
        /// orientation, which an arbitrary pose does not have. The heading is not meaningful when
        /// the forward axis points straight up or down, or close to it. The pose can be any pose,
        /// not just the camera's.
        /// </remarks>
        /// <param name="pose">The pose in AR session space, with position in meters.</param>
        /// <param name="geolocationOut">Receives the geolocation on success.</param>
        /// <returns><c>false</c> if the subsystem is unavailable or VPS2 has not localized yet.</returns>
        public bool TryGetGeolocation(Pose pose, out XRVps2Geolocation geolocationOut)
        {
            if (subsystem == null)
            {
                geolocationOut = new XRVps2Geolocation();
                return false;
            }

            return subsystem.TryGetGeolocation(pose, out geolocationOut);
        }

        /// <summary>
        /// Requests localization to a site, identified by its site ID.
        /// </summary>
        /// <remarks>
        /// Localizes against the site's production-deployed VPS assets only. Asynchronous:
        /// observe the outcome via <see cref="TryGetLatestLocalization"/>'s localized asset
        /// and tracking state. Requests are additive, and submitted assets stay in the
        /// localization set until <see cref="TryStopLocalize"/> removes them; stopping and
        /// restarting the subsystem does not remove them. This manager must be enabled;
        /// a request made while it is disabled is rejected.
        /// </remarks>
        /// <param name="siteId">The identifier of the site to localize to.</param>
        /// <returns><c>true</c> if the request was started; otherwise <c>false</c>.</returns>
        public bool TryLocalize(string siteId)
        {
            if (subsystem == null)
            {
                return false;
            }

            return subsystem.TryLocalize(siteId);
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
        /// stops it. This manager must be enabled.
        /// </remarks>
        /// <param name="siteId">The identifier of the site the asset belongs to.</param>
        /// <param name="assetId">The VPS map asset's ID, or empty for the site's production
        /// assets.</param>
        /// <returns><c>true</c> if the request was started; otherwise <c>false</c>.</returns>
        public bool TryLocalize(string siteId, string assetId)
        {
            if (subsystem == null)
            {
                return false;
            }

            return subsystem.TryLocalize(siteId, assetId);
        }

        /// <summary>
        /// Requests localization near a geographic coordinate.
        /// </summary>
        /// <remarks>
        /// Looks up every site holding VPS assets within <paramref name="radiusMeters"/> of the
        /// coordinate and localizes against their production-deployed assets only.
        /// Asynchronous: observe the
        /// outcome via <see cref="TryGetLatestLocalization"/>'s localized asset and tracking
        /// state. Requests are additive, and submitted assets stay in the localization set until
        /// <see cref="TryStopLocalize"/> removes them; stopping and restarting the subsystem
        /// does not remove them. This manager must be enabled; a request made while it
        /// is disabled is rejected.
        /// </remarks>
        /// <param name="latitude">Latitude of the search center, in the range [-90, 90].</param>
        /// <param name="longitude">Longitude of the search center, in the range [-180, 180].</param>
        /// <param name="radiusMeters">Search radius in meters. Zero uses the default value
        /// (100 m). Must not be negative.</param>
        /// <returns><c>true</c> if the request was started; otherwise <c>false</c>.</returns>
        public bool TryLocalize(double latitude, double longitude, double radiusMeters = 0)
        {
            if (subsystem == null)
            {
                return false;
            }

            return subsystem.TryLocalize(latitude, longitude, radiusMeters);
        }

        /// <summary>
        /// Stops localizing against previously requested sites.
        /// </summary>
        /// <remarks>
        /// Unregisters the site's assets, removes their anchors, and removes the map data
        /// they seeded. Call it before this manager is disabled so the site's maps are
        /// not localization targets when the session starts again.
        /// Requests for the site still in progress are cancelled, and their localization
        /// request records end as failed.
        /// </remarks>
        /// <param name="siteId">The site to stop localizing to, or <c>null</c> for every
        /// site requested in this session.</param>
        /// <returns><c>true</c> if the request was dispatched; otherwise <c>false</c>.</returns>
        public bool TryStopLocalize(string siteId = null)
        {
            if (subsystem == null)
            {
                return false;
            }

            return subsystem.TryStopLocalize(siteId);
        }

        /// <summary>
        /// Attempts to get the pose of a VPS asset's origin, in AR session space.
        /// </summary>
        /// <remarks>
        /// Polling drives tracking: the first call for an asset starts tracking its origin,
        /// so keep polling while this returns <c>false</c>. Data is only served while the
        /// asset is fully tracked; <c>false</c> also covers lapsed tracking and an asset id
        /// not yet known to the session (e.g. a <see cref="TryLocalize(string)"/> for its
        /// site is still in flight). The pose relates the asset's origin to the AR session space
        /// origin and is not relative to the current camera pose. The pose is in AR session space, like
        /// <see cref="TryCreateAnchor"/> and <see cref="TryGetPose"/>; for Unity world-space
        /// placement, prefer the <see cref="ARVps2AssetManager"/> trackables, which follow
        /// the asset origin automatically.
        /// </remarks>
        /// <param name="trackingData">Receives the asset's tracking data on success.</param>
        /// <param name="assetId">The identifier of the VPS map asset, or null for the asset the
        /// device is currently localized to (see <see cref="XRVps2Localization.LocalizedAsset"/>).
        /// Only null selects the localized asset; an empty id returns false.</param>
        /// <returns><c>true</c> if tracking data is available; otherwise <c>false</c>.</returns>
        public bool TryGetAssetTrackingData(out XRVps2AssetTrackingData trackingData, string assetId = null)
        {
            if (subsystem == null)
            {
                trackingData = default;
                return false;
            }

            return subsystem.TryGetAssetTrackingData(out trackingData, assetId);
        }

        /// <summary>
        /// Attempts to create a new anchor at <paramref name="localPose"/>, in AR session
        /// space. Forwards to <see cref="ARVps2AnchorManager.TryCreateAnchor"/>.
        /// </summary>
        /// <remarks>
        /// Anchors mark a place in the world that content should stay attached to. VPS2 must
        /// localize to a map before anchors can be created for it — use
        /// <see cref="TryLocalize(string)"/> or its lat/long overload — and an anchor only
        /// tracks with the asset of the site that was active when it was created.
        /// </remarks>
        /// <param name="localPose">The pose, in AR session space, of the anchor.</param>
        /// <param name="anchorOut">The created anchor.</param>
        /// <returns><c>true</c> if the anchor was created; otherwise <c>false</c>.</returns>
        public bool TryCreateAnchor(Pose localPose, out ARVps2Anchor anchorOut)
        {
            return AnchorManager.TryCreateAnchor(localPose, out anchorOut);
        }

        /// <summary>
        /// Starts tracking the anchor encoded by <paramref name="anchorPayload"/>.
        /// Forwards to <see cref="ARVps2AnchorManager.TryTrackAnchor"/>.
        /// </summary>
        /// <remarks>
        /// Use this only to restore an AR anchor the app created itself.
        /// @note An anchor payload is no longer the preferred method to identify the map to
        /// localize against: use <see cref="TryLocalize(string)"/> with a site ID, or its
        /// lat/long overload
        /// @note An anchor will not resolve against a different site or a different
        /// asset of the same site.
        /// </remarks>
        /// <param name="anchorPayload">The payload which encodes the anchor.</param>
        /// <param name="anchorOut">The tracked anchor.</param>
        /// <returns><c>true</c> if the anchor was accepted for tracking; otherwise <c>false</c>.</returns>
        public bool TryTrackAnchor(string anchorPayload, out ARVps2Anchor anchorOut)
        {
            return AnchorManager.TryTrackAnchor(anchorPayload, out anchorOut);
        }

        /// <summary>
        /// Stops tracking an anchor and destroys its trackable.
        /// Forwards to <see cref="ARVps2AnchorManager.RemoveAnchor"/>.
        /// </summary>
        /// <remarks>
        /// This removes one of the app's own anchors. To stop localizing to a map, use
        /// <see cref="TryStopLocalize"/> instead.
        /// </remarks>
        /// <param name="anchor">The anchor to remove.</param>
        public void RemoveAnchor(ARVps2Anchor anchor)
        {
            AnchorManager.RemoveAnchor(anchor);
        }

        /// <summary>
        /// Gets the payload which encodes the anchor.
        /// Forwards to <see cref="ARVps2AnchorManager.TryGetAnchorPayload"/>.
        /// </summary>
        /// <remarks>
        /// @note An anchor is tied to the site asset it was created against. To start
        /// tracking it in a different session, first localize to its site with
        /// <see cref="TryLocalize(string)"/>, then call <see cref="TryTrackAnchor"/>.
        /// </remarks>
        /// <param name="anchor">The anchor whose payload to get.</param>
        /// <param name="payload">Receives the payload on success.</param>
        /// <returns><c>true</c> if the payload is available; otherwise <c>false</c>.</returns>
        public bool TryGetAnchorPayload(ARVps2Anchor anchor, out string payload)
        {
            return AnchorManager.TryGetAnchorPayload(anchor, out payload);
        }

        /// <summary>
        /// Requests tracking of a VPS map asset's origin, identified by its asset ID.
        /// Forwards to <see cref="ARVps2AssetManager.TryTrackAsset"/>.
        /// </summary>
        /// <param name="assetId">The identifier of the VPS map asset.</param>
        /// <param name="assetOut">The asset's trackable.</param>
        /// <returns><c>true</c> if the request was accepted; otherwise <c>false</c>.</returns>
        public bool TryTrackAsset(string assetId, out ARVps2Asset assetOut)
        {
            return AssetManager.TryTrackAsset(assetId, out assetOut);
        }

        /// <summary>
        /// Gets the session id, if one exists.
        /// </summary>
        /// <param name="sessionId">The session id as a 32 character hexadecimal
        /// upper-case string.</param>
        /// <returns><c>true</c> if a session id is present; otherwise <c>false</c>.</returns>
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
            // Anchors cannot outlive the VPS2 session that created them: tear them down
            // before the feature stops. TryGetComponent instead of the AnchorManager
            // property, which would add the component during teardown.
            if (TryGetComponent<ARVps2AnchorManager>(out var anchorManager))
            {
                anchorManager.RemoveAllAnchorsImmediate();
            }

            base.OnDisable();
        }

        private T EnsureComponent<T>() where T : Component
        {
            return TryGetComponent<T>(out var component) ? component : gameObject.AddComponent<T>();
        }

        // Callback before the subsystem is started (but after it is created).
        // Pushes the serialized configuration to the subsystem.
        protected override void OnBeforeStart()
        {
            subsystem.UniversalLocalizationEnabled = _universalLocalizationEnabled;
            subsystem.UniversalLocalizationRequestsPerSecond = _universalLocalizationRequestsPerSecond;
            subsystem.AerialLocalizationEnabled = _aerialLocalizationEnabled;
            subsystem.VpsMapLocalizationEnabled = _vpsMapLocalizationEnabled;
            subsystem.InitialVpsRequestsPerSecond = _initialVpsRequestsPerSecond;
            subsystem.ContinuousVpsRequestsPerSecond = _continuousVpsRequestsPerSecond;
            subsystem.GeolocationSmoothingEnabled = _geolocationSmoothingEnabled;
            subsystem.VpsDebuggerEnabled = _vpsDebuggerEnabled;
            subsystem.VpsDebuggerSavingFrameImageEnabled = _vpsDebuggerSavingFrameImageEnabled;
            subsystem.DeviceMapLocalizationEnabled = _deviceMapLocalizationEnabled;
            subsystem.DeviceMapLocalizationFramerate = _deviceMapLocalizationFramerate;
            subsystem.MaxRequestsInTransitPerTarget = _maxRequestsInTransitPerTarget;
            subsystem.AnchorDistanceGateMeters = _anchorDistanceGateMeters;
            subsystem.BlurDetectionEnabled = _blurDetectionEnabled;
            subsystem.MinSharpnessScore = _minSharpnessScore;
            subsystem.AreaTargetingEnterMarginMeters = _areaTargetingEnterMarginMeters;
            subsystem.AreaTargetingExitMarginMeters = _areaTargetingExitMarginMeters;
            subsystem.TemporalFusionEnabled = _temporalFusionEnabled;
        }

        private void Update()
        {
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
