// Copyright 2022-2026 Niantic Spatial.

using System;
using System.IO;
using NianticSpatial.NSDK.AR.Auth;
using NianticSpatial.NSDK.AR.Core;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using UnityEngine;
using NianticSpatial.NSDK.AR.Settings;
using NianticSpatial.NSDK.AR.Utilities.Auth;

namespace NianticSpatial.NSDK.AR.Loader
{
    public class RuntimeNsdkSettings : IAuthSettings
    {
        private string _apiKey;
        private AuthEnvironmentType _authEnvironment;
        private string _accessToken;
        private int _accessExpiresAt;
        private string _refreshToken;
        private int _refreshExpiresAt;
        private bool _useNsdkDepth;
        private bool _preferLidarIfAvailable;
        private bool _useNsdkMeshing;
        private bool _useNsdkSemanticSegmentation;
        private bool _useNsdkScanning;
        private bool _useNsdkPersistentAnchor;
        private bool _useNsdkObjectDetection;
        private bool _useNsdkWorldPositioning;
        private bool _useNsdkVps2;
        private LocationDataSource _locationAndCompassDataSource;
        private SpoofLocationInfo _spoofLocationInfo;
        private SpoofCompassInfo _spoofCompassInfo;

        private LogLevel _unityLogLevel;
        private LogLevel _fileLogLevel;
        private LogLevel _stdoutLogLevel;

        private NsdkSimulationParams _nsdkSimulationParams;
        private EndpointSettings _endpointSettings;
        private TestSettings _testSettings;
        private INsdkPlaybackSettings _playbackSettings;

        /// <summary>
        /// Get the NSDK API key.
        /// </summary>
        public string ApiKey
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(_apiKey))
                {
                    // ensure that the config provider's key is overridden in case the user has provided their own
                    return _apiKey;
                }

                return EndpointSettings.ApiKey ?? string.Empty;
            }

            internal set => _apiKey = value;
        }

        public AuthEnvironmentType AuthEnvironment => _authEnvironment;

        public string AccessToken
        {
            get => _accessToken;
            set => SetAccess(value);
        }

        public int AccessExpiresAt => _accessExpiresAt;

        public string RefreshToken
        {
            get => _refreshToken;
            set => SetRefresh(value);
        }

        public int RefreshExpiresAt => _refreshExpiresAt;

        public bool IsRuntimeLogin { get; set; }

        /// <summary>
        /// When enabled, NSDK's depth and occlusion features can be used via ARFoundation. Additional occlusion
        /// features unique to NSDK can be configured in the NsdkOcclusionExtension component.
        /// </summary>
        public bool UseNsdkDepth
        {
            get => _useNsdkDepth;
            set => _useNsdkDepth = value;
        }

        /// <summary>
        /// When enabled, LiDAR depth will be used instead of NSDK depth on devices where LiDAR is available.
        /// Features unique to the NsdkOcclusionExtension cannot be used.
        /// </summary>
        /// <remarks>
        /// When enabled in experiences with meshing, the XROcclusionSubsystem must also be running in order to
        /// generate meshes.
        /// </remarks>
        public bool PreferLidarIfAvailable
        {
            get => _preferLidarIfAvailable;
            set => _preferLidarIfAvailable = value;
        }

        /// <summary>
        /// When enabled, NSDK's meshing features can be used via ARFoundation. Additional mesh features unique
        /// to NSDK can be configured in the LightshipMeshingExtension component.
        /// </summary>
        public bool UseNsdkMeshing
        {
            get => _useNsdkMeshing;
            set => _useNsdkMeshing = value;
        }

        /// <summary>
        /// When enabled, NSDK's semantic segmentation features can be used.
        /// </summary>
        public bool UseNsdkSemanticSegmentation
        {
            get => _useNsdkSemanticSegmentation;
            set => _useNsdkSemanticSegmentation = value;
        }

        /// <summary>
        /// When enabled, NSDK's scanning features can be used.
        /// </summary>
        public bool UseNsdkScanning
        {
            get => _useNsdkScanning;
            set => _useNsdkScanning = value;
        }

        /// <summary>
        /// When enabled, NSDK VPS can be used.
        /// </summary>
        public bool UseNsdkPersistentAnchor
        {
            get => _useNsdkPersistentAnchor;
            set => _useNsdkPersistentAnchor = value;
        }

        /// <summary>
        /// When enabled, NSDK's VPS2 feature can be used.
        /// </summary>
        public bool UseNsdkVps2
        {
            get => _useNsdkVps2;
            set => _useNsdkVps2 = value;
        }

        /// <summary>
        /// When true, NSDK's object detection features can be used.
        /// </summary>
        public bool UseNsdkObjectDetection
        {
            get => _useNsdkObjectDetection;
            set => _useNsdkObjectDetection = value;
        }

        /// <summary>
        /// When true, NSDK's World Positioning System (WPS) feature can be used.
        /// </summary>
        public bool UseNsdkWorldPositioning
        {
            get => _useNsdkWorldPositioning;
            set => _useNsdkWorldPositioning = value;
        }

        /// <summary>
        /// Source of location and compass data fetched from the NianticSpatial.NSDK.AR.Input APIs
        /// </summary>
        public LocationDataSource LocationAndCompassDataSource
        {
            get => _locationAndCompassDataSource;
            set { _locationAndCompassDataSource = value; }
        }

        /// <summary>
        /// Values returned by location service when in Spoof mode
        /// </summary>
        public SpoofLocationInfo SpoofLocationInfo
        {
            get => _spoofLocationInfo;
        }

        /// <summary>
        /// Values returned by compass service when in Spoof mode
        /// </summary>
        public SpoofCompassInfo SpoofCompassInfo
        {
            get => _spoofCompassInfo;
        }

        /// <summary>
        /// The highest log level to print for Unity logger
        /// </summary>
        public LogLevel UnityNsdkLogLevel
        {
            get => _unityLogLevel;
            set => _unityLogLevel = value;
        }

        /// <summary>
        /// The highest log level to print for a file logger
        /// </summary>
        public LogLevel FileNsdkLogLevel
        {
            get => _fileLogLevel;
            set => _fileLogLevel = value;
        }

        /// <summary>
        /// The highest log level to print for the stdout logger - typically for internal testing. Keep this off unless
        /// you know what you are looking for
        /// </summary>
        public LogLevel StdOutNsdkLogLevel
        {
            get => _stdoutLogLevel;
            set => _stdoutLogLevel = value;
        }

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

        public bool UsePlayback
        {
            get => _playbackSettings.UsePlayback;
            set => _playbackSettings.UsePlayback = value;
        }

        public string PlaybackDatasetPath
        {
            get => _playbackSettings.PlaybackDatasetPath;
            set => _playbackSettings.PlaybackDatasetPath = value;
        }

        public bool RunPlaybackManually
        {
            get => _playbackSettings.RunManually;
            set => _playbackSettings.RunManually = value;
        }

        public bool LoopPlaybackInfinitely
        {
            get => _playbackSettings.LoopInfinitely;
            set => _playbackSettings.LoopInfinitely = value;
        }

        public int StartFrame
        {
            get => _playbackSettings.StartFrame;
            set => _playbackSettings.StartFrame = value;
        }

        public int EndFrame
        {
            get => _playbackSettings.EndFrame;
            set => _playbackSettings.EndFrame = value;
        }

        internal EndpointSettings EndpointSettings
        {
            get => _endpointSettings;
        }

        internal TestSettings TestSettings
        {
            get => _testSettings;
        }

        internal INsdkPlaybackSettings PlaybackSettings
        {
            get => _playbackSettings;
        }

        internal RuntimeNsdkSettings()
        {
            _playbackSettings = new OverloadPlaybackSettings();
            _nsdkSimulationParams = new NsdkSimulationParams();
            _testSettings = new TestSettings();
            _endpointSettings = EndpointSettings.GetDefaultEnvironmentConfig();

            _spoofLocationInfo = SpoofLocationInfo.Default;
            _spoofCompassInfo = SpoofCompassInfo.Default;
        }

        internal RuntimeNsdkSettings(NsdkSettings source)
        {
            CopyFrom(source);
        }

        internal void CopyFrom(NsdkSettings source)
        {
            ApiKey = source.ApiKey;
            UseNsdkDepth = source.UseNsdkDepth;
            PreferLidarIfAvailable = source.PreferLidarIfAvailable;
            UseNsdkMeshing = source.UseNsdkMeshing;
            UseNsdkPersistentAnchor = source.UseNsdkPersistentAnchor;
            UseNsdkSemanticSegmentation = source.UseNsdkSemanticSegmentation;
            UseNsdkScanning = source.UseNsdkScanning;
            UseNsdkObjectDetection = source.UseNsdkObjectDetection;
            UseNsdkWorldPositioning = source.UseNsdkWorldPositioning;
            UseNsdkVps2 = source.UseNsdkVps2;
            LocationAndCompassDataSource = source.LocationAndCompassDataSource;

            _spoofLocationInfo = new SpoofLocationInfo(source.SpoofLocationInfo);
            _spoofCompassInfo = new SpoofCompassInfo(source.SpoofCompassInfo);

            _authEnvironment = source.AuthEnvironment;

            // Only copy the developer authentication settings if the user has left developer authentication enabled
            // and is not using an API key
            if (source.UseDeveloperAuthentication && string.IsNullOrEmpty(source.ApiKey))
            {
                _accessToken = source.AccessToken;
                _accessExpiresAt = source.AccessExpiresAt;
                _refreshToken = source.RefreshToken;
                _refreshExpiresAt = source.RefreshExpiresAt;
            }

            UnityNsdkLogLevel = source.UnityNsdkLogLevel;
            FileNsdkLogLevel = source.FileNsdkLogLevel;
            StdOutNsdkLogLevel = source.StdOutNsdkLogLevel;

            var activePlaybackSettings =
                Application.isEditor
                    ? source.EditorPlaybackSettings
                    : source.DevicePlaybackSettings;

            _playbackSettings = new OverloadPlaybackSettings(activePlaybackSettings);
            _testSettings = new TestSettings(source.TestSettings);
            _endpointSettings = new EndpointSettings(source.EndpointSettings);

            _nsdkSimulationParams = new NsdkSimulationParams(source.NsdkSimulationParams);
        }

        [Obsolete("Use the parameter-less constructor with object initializers instead")]
        internal static RuntimeNsdkSettings _CreateRuntimeInstance
        (
            bool enableDepth = false,
            bool enableMeshing = false,
            bool enablePersistentAnchors = false,
            bool usePlayback = false,
            string playbackDataset = "",
            bool runPlaybackManually = false,
            bool loopPlaybackInfinitely = false,
            string apiKey = "",
            bool enableSemanticSegmentation = false,
            bool preferLidarIfAvailable = false,
            bool enableScanning = false,
            bool enableObjectDetection = false,
            bool enableWorldPositioning = false,
            LogLevel unityLogLevel = LogLevel.Debug,
            EndpointSettings endpointSettings = null,
            LogLevel stdoutLogLevel = LogLevel.Off,
            LogLevel fileLogLevel = LogLevel.Off,
            bool disableTelemetry = true,
            bool tickPamOnUpdate = true,
            NsdkSimulationParams simulationParams = null,
            int startFrame = 0,
            int endFrame = -1
        )
        {
            var settings =
                new RuntimeNsdkSettings
                {
                    ApiKey = apiKey,
                    UseNsdkDepth = enableDepth,
                    PreferLidarIfAvailable = preferLidarIfAvailable,
                    UseNsdkMeshing = enableMeshing,
                    UseNsdkPersistentAnchor = enablePersistentAnchors,
                    UseNsdkSemanticSegmentation = enableSemanticSegmentation,
                    UseNsdkScanning = enableScanning,
                    UseNsdkObjectDetection = enableObjectDetection,
                    UseNsdkWorldPositioning = enableWorldPositioning,
                    UnityNsdkLogLevel = unityLogLevel,
                    FileNsdkLogLevel = fileLogLevel,
                    StdOutNsdkLogLevel = stdoutLogLevel,
                    UsePlayback = usePlayback,
                    PlaybackDatasetPath = playbackDataset,
                    RunPlaybackManually = runPlaybackManually,
                    LoopPlaybackInfinitely = loopPlaybackInfinitely,
                    _playbackSettings =
                        new OverloadPlaybackSettings
                        {
                            UsePlayback = usePlayback,
                            PlaybackDatasetPath = playbackDataset,
                            RunManually = runPlaybackManually,
                            LoopInfinitely = loopPlaybackInfinitely,
                            StartFrame = startFrame,
                            EndFrame = endFrame
                        },
                    _endpointSettings = endpointSettings ?? EndpointSettings.GetDefaultEnvironmentConfig(),
                    _testSettings =
                        new TestSettings
                        {
                            DisableTelemetry = disableTelemetry,
                            TickPamOnUpdate = tickPamOnUpdate
                        }
                };

            simulationParams ??= new NsdkSimulationParams();
            settings._nsdkSimulationParams = simulationParams;

            return settings;
        }

        public void UpdateAccess(string accessToken, int accessExpiresAt, string refreshToken, int refreshExpiresAt)
        {
            _refreshToken = refreshToken;
            _accessToken = accessToken;
            _accessExpiresAt = accessExpiresAt;
            _refreshExpiresAt = refreshExpiresAt;

            AuthGatewayUtils.Instance.LogSettings(this, "Updated runtime");

            if (NsdkUnityContext.UnityContextHandle != IntPtr.Zero)
            {
                // Pass the access token to native NSDK code.
                // Note: NSDK native can also receive the refresh token, but we don't want to set it here
                // (in Unity, we run the refresh loop in C#).
                Metadata.SetAccessToken(_accessToken);
            }
        }

        public override string ToString()
        {
            return
                $"{GetType()}: \n" +
                "\t ApiKey: " + _apiKey + "\n" +
                "\t UseNsdkDepth: " + UseNsdkDepth + "\n" +
                "\t PreferLidarIfAvailable: " + PreferLidarIfAvailable + "\n" +
                "\t UseNsdkMeshing: " + UseNsdkMeshing + "\n" +
                "\t UseNsdkPersistentAnchor: " + UseNsdkPersistentAnchor + "\n" +
                "\t UseNsdkSemanticSegmentation: " + UseNsdkSemanticSegmentation + "\n" +
                "\t UseNsdkScanning: " + UseNsdkScanning + "\n" +
                "\t UseNsdkObjectDetection: " + UseNsdkObjectDetection + "\n" +
                "\t UseNsdkWorldPositioning: " + UseNsdkWorldPositioning + "\n" +
                "\t UnityNsdkLogLevel: " + UnityNsdkLogLevel;
        }

        private void SetAccess(string accessToken)
        {
            if (_accessToken != accessToken)
            {
                _accessToken = accessToken;
                _accessExpiresAt = AuthGatewayUtils.Instance.DecodeJwtTokenBody(accessToken)?.exp ?? 0;
                AuthGatewayUtils.Instance.LogToken("Updated runtime access", _accessToken);

                if (NsdkUnityContext.UnityContextHandle != IntPtr.Zero)
                {
                    // Pass the access token to native NSDK code.
                    // Note: NSDK native can also receive the refresh token, but we don't want to set it here
                    // (in Unity, we run the refresh loop in C#).
                    Metadata.SetAccessToken(_accessToken);
                }
            }
        }

        private void SetRefresh(string refreshToken)
        {
            if (_refreshToken != refreshToken)
            {
                _refreshToken = refreshToken;
                _refreshExpiresAt = AuthGatewayUtils.Instance.DecodeJwtTokenBody(refreshToken)?.exp ?? 0;
                AuthGatewayUtils.Instance.LogToken("Updated runtime refresh", _refreshToken);
                AuthRuntimeRefreshManager.RestartRefreshLoop();
            }
        }
    }
}
