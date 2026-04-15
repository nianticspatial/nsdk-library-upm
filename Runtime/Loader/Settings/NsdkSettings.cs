// Copyright 2022-2026 Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.Auth;
using NianticSpatial.NSDK.AR.Utilities;
using UnityEditor;
using UnityEngine;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using UnityEngine.Serialization;
using UnityEngine.XR.Management;

namespace NianticSpatial.NSDK.AR.Loader
{
    /// <summary>
    /// Build time settings for NSDK AR. These are serialized to an asset file and can only be altered
    /// via the Unity Inspector window.
    /// </summary>
    [Serializable]
    [XRConfigurationData("Niantic Spatial Development Kit", SettingsKey)]
    public partial class NsdkSettings : ScriptableObject
    {
        public const string SettingsKey = "NianticSpatial.NSDK.AR.NsdkSettings";
        public const string BuildSettingsKey = "NianticSpatial.NSDK.AR.AuthBuildSettings";

        [FormerlySerializedAs("_useLightshipDepth")]
        [SerializeField]
        [Tooltip
            (
                "When enabled, NSDK's depth and occlusion features can be used via ARFoundation. " +
                "Additional occlusion features unique to NSDK can be configured in the " +
                "NsdkOcclusionExtension component."
            )
        ]
        private bool _useNsdkDepth = true;

        [SerializeField]
        [Tooltip
            (
                "When enabled, LiDAR depth will be used instead of NSDK depth on devices where LiDAR is " +
                "available. Features unique to the NsdkOcclusionExtension cannot be used."
            )
        ]
        private bool _preferLidarIfAvailable = true;

        [FormerlySerializedAs("_useLightshipMeshing")]
        [SerializeField]
        [Tooltip
            (
                "When enabled, NSDK's meshing features can be used via ARFoundation. Additional mesh features " +
                "unique to NSDK can be configured in the LightshipMeshingExtension component."
            )
        ]
        private bool _useNsdkMeshing = true;

        [FormerlySerializedAs("_useLightshipSemanticSegmentation")]
        [FormerlySerializedAs("_useNsdkSemanticSegmentation")]
        [SerializeField, Tooltip("When enabled, NSDK's scene segmentation features can be used.")]
        private bool _useNsdkSceneSegmentation = true;

        [FormerlySerializedAs("_useLightshipScanning")]
        [SerializeField, Tooltip("When enabled, NSDK's scanning features can be used.")]
        private bool _useNsdkScanning = true;

        [SerializeField, Tooltip("When enabled, NSDK's VPS2 feature can be used.")]
        private bool _useNsdkVps2 = true;

        [SerializeField, Tooltip("When enabled, NSDK's device mapping features can be used.")]
        private bool _useNsdkDeviceMapping = true;

        [SerializeField, Tooltip("Source of location and compass data")]
        private LocationDataSource _locationAndCompassDataSource = LocationDataSource.Sensors;

        // Default to the Ferry Building location, so that non-zero location info is
        // surfaced even if the dev has not specified spoof location values.
        [SerializeField, Tooltip("Values returned by location service when in Spoof mode")]
        private SpoofLocationInfo _spoofLocationInfo = SpoofLocationInfo.Default;

        [SerializeField, Tooltip("Values returned by compass service when in Spoof mode")]
        private SpoofCompassInfo _spoofCompassInfo = SpoofCompassInfo.Default;

        [SerializeField, Tooltip("The lowest log level to print")]
        private LogLevel _unityLogLevel = LogLevel.Warn;

        [SerializeField, Tooltip("The lowest log level for file logger")]
        private LogLevel _fileLogLevel = LogLevel.Off;

        [SerializeField, Tooltip("The lowest log level to print")]
        private LogLevel _stdoutLogLevel = LogLevel.Off;

        [FormerlySerializedAs("_lightshipSimulationParams")]
        [SerializeField]
        private NsdkSimulationParams _nsdkSimulationParams;

        [SerializeField]
        private DevicePlaybackSettings _devicePlaybackSettings;

        [SerializeField]
        private AuthBuildSettings _authBuildSettings;

        private EditorPlaybackSettings _editorPlaybackSettings;
        private EndpointSettings _endpointSettings;
        private TestSettings _testSettings;

        /// <summary>
        /// The current runtime refresh token
        /// </summary>
        public string RefreshToken => _authBuildSettings?.RefreshToken;

        /// <summary>
        /// Time when the current runtime refresh token expires
        /// </summary>
        public int RefreshExpiresAt => _authBuildSettings?.RefreshExpiresAt ?? 0;

        /// <summary>
        /// The effective access token. Returns the access token override if set,
        /// otherwise the developer auth access token.
        /// </summary>
        public string AccessToken =>
            !string.IsNullOrEmpty(_authBuildSettings?.AccessTokenOverride)
                ? _authBuildSettings.AccessTokenOverride
                : _authBuildSettings?.AccessToken;

        /// <summary>
        /// Time when the current runtime access token expires
        /// </summary>
        public int AccessExpiresAt => _authBuildSettings?.AccessExpiresAt ?? 0;

        public void UpdateAccess(string accessToken, int accessExpiresAt, string refreshToken, int refreshExpiresAt)
        {
            _authBuildSettings?.UpdateAccess(accessToken, accessExpiresAt, refreshToken, refreshExpiresAt);

            // Changes to auth tokens need to be saved immediately (any prior tokens on disk are now invalid)
            SettingsUtils.SaveImmediatelyInEditor(_authBuildSettings);
        }

        /// <summary>
        /// An access token that takes priority over developer authentication when set.
        /// </summary>
        public string AccessTokenOverride => _authBuildSettings?.AccessTokenOverride;

        public bool UseDeveloperAuthentication => _authBuildSettings?.UseDeveloperAuthentication ?? true;

        public AuthEnvironmentType AuthEnvironment
        {
            get => _authBuildSettings?.AuthEnvironment ?? AuthEnvironmentType.Production;
            set
            {
                if (_authBuildSettings != null && _authBuildSettings.AuthEnvironment != value)
                {
                    _endpointSettings = EndpointSettings.GetFromFileOrDefault(value);
                    _authBuildSettings.AuthEnvironment = value;
                }
            }
        }

        /// <summary>
        /// When enabled, NSDK's depth and occlusion features can be used via ARFoundation. Additional occlusion
        /// features unique to NSDK can be configured in the NsdkOcclusionExtension component.
        /// </summary>
        public bool UseNsdkDepth => _useNsdkDepth;

        /// <summary>
        /// When enabled, NSDK's meshing features can be used via ARFoundation. Additional mesh features unique
        /// to NSDK can be configured in the LightshipMeshingExtension component.
        /// </summary>
        public bool UseNsdkMeshing => _useNsdkMeshing;

        /// <summary>
        /// When enabled, LiDAR depth will be used instead of NSDK depth on devices where LiDAR is available.
        /// Features unique to the NsdkOcclusionExtension cannot be used.
        /// </summary>
        /// <remarks>
        /// When enabled in experiences with meshing, the XROcclusionSubsystem must also be running in order to
        /// generate meshes.
        /// </remarks>
        public bool PreferLidarIfAvailable => _preferLidarIfAvailable;

        /// <summary>
        /// When enabled, NSDK's VPS2 feature can be used.
        /// </summary>
        public bool UseNsdkVps2 => _useNsdkVps2;

        /// <summary>
        /// When enabled, NSDK's device mapping features can be used.
        /// </summary>
        public bool UseNsdkDeviceMapping => _useNsdkDeviceMapping;

        /// <summary>
        /// When enabled, NSDK's scene segmentation features can be used.
        /// </summary>
        public bool UseNsdkSceneSegmentation => _useNsdkSceneSegmentation;

        /// <summary>
        /// When enabled, NSDK's scanning features can be used.
        /// </summary>
        public bool UseNsdkScanning => _useNsdkScanning;

        /// <summary>
        /// Source of location and compass data fetched from the NianticSpatial.NSDK.AR.Input APIs
        /// </summary>
        public LocationDataSource LocationAndCompassDataSource
        {
            get => _locationAndCompassDataSource;
            set => _locationAndCompassDataSource = value;
        }

        /// <summary>
        /// Values returned by location service when in Spoof mode
        /// </summary>
        public SpoofLocationInfo SpoofLocationInfo
        {
            get
            {
                if (_spoofLocationInfo == null)
                {
                    // Default to the Ferry Building location, so that non-zero location info is
                    // surfaced even if the dev has not specified spoof location values.
                    _spoofLocationInfo =
                        new SpoofLocationInfo
                        {
                            Latitude = 37.795322f,
                            Longitude = -122.39243f,
                            Timestamp = 123456,
                            Altitude = 16,
                            HorizontalAccuracy = 10,
                            VerticalAccuracy = 10
                        };
                }

                return _spoofLocationInfo;
            }

            set => _spoofLocationInfo = value;
        }

        /// <summary>
        /// Values returned by compass service when in Spoof mode
        /// </summary>
        public SpoofCompassInfo SpoofCompassInfo
        {
            get
            {
                if (_spoofCompassInfo == null)
                {
                    _spoofCompassInfo = new SpoofCompassInfo();
                }

                return _spoofCompassInfo;
            }
            set => _spoofCompassInfo = value;
        }

        /// <summary>
        /// The highest log level to print for Unity logger
        /// </summary>
        public LogLevel UnityNsdkLogLevel => _unityLogLevel;

        /// <summary>
        /// The highest log level to print for a file logger
        /// </summary>
        public LogLevel FileNsdkLogLevel => _fileLogLevel;

        /// <summary>
        /// The highest log level to print for the stdout logger - typically for internal testing. Keep this off unless
        /// you know what you are looking for
        /// </summary>
        public LogLevel StdOutNsdkLogLevel => _stdoutLogLevel;

        public NsdkSimulationParams NsdkSimulationParams
        {
            get
            {
                if (_nsdkSimulationParams == null)
                {
                    _nsdkSimulationParams = new NsdkSimulationParams();
                }

                return _nsdkSimulationParams;
            }
        }

        /// <summary>
        /// All Settings for Playback on the active platform
        /// </summary>
        internal INsdkPlaybackSettings PlaybackSettings
        {
            get => Application.isEditor ? EditorPlaybackSettings : DevicePlaybackSettings;
        }

        public bool UsePlayback => PlaybackSettings.UsePlayback;
        public string PlaybackDatasetPath => PlaybackSettings.PlaybackDatasetPath;
        public bool RunManually => PlaybackSettings.RunManually;
        public bool LoopInfinitely => PlaybackSettings.LoopInfinitely;

        internal AuthBuildSettings AuthBuildSettings => _authBuildSettings;

        public INsdkPlaybackSettings DevicePlaybackSettings
        {
            get
            {
                return _devicePlaybackSettings;
            }
        }

        public INsdkPlaybackSettings EditorPlaybackSettings
        {
            get
            {
                if (_editorPlaybackSettings == null)
                {
                    _editorPlaybackSettings = new EditorPlaybackSettings();
                }

                return _editorPlaybackSettings;

            }
        }

        internal EndpointSettings EndpointSettings
        {
            get
            {
                if (_endpointSettings == null)
                {
                    _endpointSettings = EndpointSettings.GetFromFileOrDefault(AuthEnvironment);
                }

                return _endpointSettings;
            }
        }

        internal TestSettings TestSettings
        {
            get
            {
                if (_testSettings == null)
                {
                    _testSettings = new TestSettings { DisableTelemetry = false, TickPamOnUpdate = true };
                }

                return _testSettings;
            }
        }

#if !UNITY_EDITOR
        /// <summary>
        /// Static instance that will hold the runtime asset instance we created in our build process.
        /// </summary>
        private static NsdkSettings s_RuntimeInstance;
#endif

        /// <summary>
        /// On devices, this will be called when the application is loaded.
        /// </summary>
        private void Awake()
        {
#if !UNITY_EDITOR
            s_RuntimeInstance = this;
#endif
        }

        /// <summary>
        /// Accessor to NSDK settings asset instance.
        /// THIS SHOULD ONLY BE USED IN SPECIFIC SITUATIONS:
        ///     1) Editor classes that need to read/write values to the asset
        ///     2) By NsdkLoaderHelper to initialize the runtime settings on application load
        ///
        /// All other code should use NsdkLoaderHelper.ActiveSettings to get the settings,
        /// otherwise it will get the asset instance's values instead of the runtime instance's, which
        /// may be different in tests or if the dev has modified settings at runtime.
        /// </summary>
        public static NsdkSettings Instance => GetOrCreateAssetInstance();

        private static NsdkSettings GetOrCreateAssetInstance()
        {
            NsdkSettings settings = null;

#if UNITY_EDITOR
            settings = SettingsUtils.GetOrCreateSettingsAsset<NsdkSettings>(SettingsKey, "NSDK Settings");
            var prevEnvironment = settings.AuthEnvironment;
            if (settings._authBuildSettings == null)
            {
                settings._authBuildSettings =
                    SettingsUtils.GetOrCreateSettingsAsset<AuthBuildSettings>(BuildSettingsKey, "AuthBuildSettings");
                EditorUtility.SetDirty(settings);
            }

            // Regenerate endpointSettings if AuthEnvironment has changed
            if (settings._endpointSettings == null || prevEnvironment != settings.AuthEnvironment)
            {
                settings._endpointSettings = EndpointSettings.GetFromFileOrDefault(settings.AuthEnvironment);
            }
#else
            settings = s_RuntimeInstance;
            if (settings == null)
            {
                settings = CreateInstance<NsdkSettings>();
            }
#endif

            return settings;
        }

#if UNITY_EDITOR
        [MenuItem("NSDK/Settings", false, 1)]
        private static void FocusOnAsset()
        {
            SettingsService.OpenProjectSettings("Project/XR Plug-in Management/Niantic Spatial Development Kit");
        }

        [MenuItem("NSDK/XR Plug-in Management", false, 0)]
        private static void OpenXRPluginManagement()
        {
            SettingsService.OpenProjectSettings("Project/XR Plug-in Management");
        }

        [MenuItem("NSDK/Project Validation", false, 2)]
        private static void OpenProjectValidation()
        {
            SettingsService.OpenProjectSettings("Project/XR Plug-in Management/Project Validation");
        }
#endif

        internal static NsdkSettings CreateTestOnlyInstance
        (
            bool enableDepth = false,
            bool enableMeshing = false,
            bool usePlayback = false,
            string playbackDataset = "",
            bool runPlaybackManually = false,
            bool loopPlaybackInfinitely = false,
            bool enableSceneSegmentation = false,
            bool preferLidarIfAvailable = false,
            bool enableScanning = false,
            LogLevel unityLogLevel = LogLevel.Debug,
            EndpointSettings endpointSettings = null,
            LogLevel stdoutLogLevel = LogLevel.Off,
            LogLevel fileLogLevel = LogLevel.Off,
            bool disableTelemetry = true,
            bool tickPamOnUpdate = true,
            NsdkSimulationParams simulationParams = null,
            LocationDataSource locationAndCompassDataSource = LocationDataSource.Sensors,
            bool enableVps2 = false,
            bool enableDeviceMapping = false
        )
        {
            var settings = CreateInstance<NsdkSettings>();

            settings._useNsdkDepth = enableDepth;
            settings._preferLidarIfAvailable = preferLidarIfAvailable;
            settings._useNsdkMeshing = enableMeshing;
            settings._useNsdkVps2 = enableVps2;
            settings._useNsdkDeviceMapping = enableDeviceMapping;
            settings._useNsdkSceneSegmentation = enableSceneSegmentation;
            settings._useNsdkScanning = enableScanning;
            settings._locationAndCompassDataSource = locationAndCompassDataSource;
            settings._unityLogLevel = unityLogLevel;
            settings._fileLogLevel = fileLogLevel;
            settings._stdoutLogLevel = stdoutLogLevel;

            settings._devicePlaybackSettings =
                new DevicePlaybackSettings()
                {
                    UsePlayback = usePlayback,
                    PlaybackDatasetPath = playbackDataset,
                    RunManually = runPlaybackManually,
                    LoopInfinitely = loopPlaybackInfinitely
                };

            if (endpointSettings == null)
            {
                settings._endpointSettings = EndpointSettings.GetDefaultEnvironmentConfig();
            }
            else
            {
                settings._endpointSettings = endpointSettings;
            }

            settings._testSettings =
                new TestSettings { DisableTelemetry = disableTelemetry, TickPamOnUpdate = tickPamOnUpdate };

            simulationParams ??= new NsdkSimulationParams();
            settings._nsdkSimulationParams = simulationParams;
            settings._authBuildSettings = CreateInstance<AuthBuildSettings>();

            return settings;
        }

    }
}
