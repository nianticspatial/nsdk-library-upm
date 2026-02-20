// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Linq;
using System.Threading;
using NianticSpatial.NSDK.AR.Auth;
using NianticSpatial.NSDK.AR.Editor.Auth;
using NianticSpatial.NSDK.AR.Loader;
using NianticSpatial.NSDK.AR.Utilities.Auth;
using UnityEditor;
using UnityEditor.XR.Management;
using UnityEngine;

namespace NianticSpatial.NSDK.AR.Editor
{
    /// <summary>
    /// This Editor renders to the XR Plug-in Management category of the Project Settings window.
    /// </summary>
    [CustomEditor(typeof(NsdkSettings))]
    internal class NsdkSettingsEditor : UnityEditor.Editor
    {
        internal const string ProjectValidationSettingsPath = "Project/XR Plug-in Management/Project Validation";
        internal const string XRPluginManagementPath = "Project/XR Plug-in Management";
        internal const string XREnvironmentViewPath = "Window/XR/AR Foundation/XR Environment";

        private enum Platform
        {
            Editor = 0,
            Device = 1
        }

        private static class Contents
        {
            public static readonly GUIContent[] _platforms =
            {
                new GUIContent
                (
                    Platform.Editor.ToString(),
                    "Playback settings for Play Mode in the Unity Editor"
                ),
                new GUIContent
                (
                    Platform.Device.ToString(),
                    "Playback settings for running on a physical device"
                )
            };

            public static readonly GUIContent apiKeyLabel = new GUIContent("API Key");
            public static readonly GUIContent enabledLabel = new GUIContent("Enabled");
            public static readonly GUIContent preferLidarLabel = new GUIContent("Prefer LiDAR if Available");
            public static readonly GUIContent environmentViewLabel = new GUIContent("Environment Prefab");
            public static readonly GUIContent environmentViewButton =
                new GUIContent
                (
                    "Open XR Environment Window",
                    "To set an environment prefab, open the scene view and use the XR Environment overlay."
                );

            private static readonly GUIContent helpIcon = EditorGUIUtility.IconContent("_Help");

            public static readonly GUIContent playbackLabel =
                new GUIContent
                    (
                        "",
                        helpIcon.image,
                        "Enable playback to use recorded camera and sensor data to drive your app's AR session." +
                        "Click for documentation."
                    );

            public static readonly GUIContent simulationLabel =
                new GUIContent
                (
                    "",
                    helpIcon.image,
                    "Enable the Niantic NSDK Simulation loader for the Standalone platform in " +
                    "the XR Plug-in Management menu to use NSDK with ARFoundation's simulation mode." +
                    "Click for documentation"
                );

            public static readonly GUIContent useZBufferDepthInSimulationLabel = new GUIContent("Use Z-Buffer Depth");
            public static readonly GUIContent useNsdkPersistentAnchorInSimulationLabel =
                new GUIContent("Use NSDK Persistent Anchors");

            private static GUIStyle _sdkEnabledStyle;

            public static GUIStyle sdkEnabledStyle
            {
                get
                {
                    return _sdkEnabledStyle ??= new GUIStyle(EditorStyles.boldLabel)
                    {
                        alignment = TextAnchor.MiddleCenter
                    };
                }
            }

            public static readonly GUILayoutOption[] sdkEnabledOptions =
            {
                GUILayout.MinWidth(0)
            };

            private static GUIStyle _sdkDisabledStyle;

            public static GUIStyle sdkDisabledStyle
            {
                get
                {
                    return _sdkDisabledStyle ??= new GUIStyle(EditorStyles.miniButton)
                    {
                        stretchWidth = true,
                        fontStyle = FontStyle.Bold
                    };
                }
            }

            public static readonly GUIContent LoginButton =
                new
                (
                    "Login",
                    "Login with your Niantic Spatial account to use developer authentication: " +
                    "quick authentication for development that supports one user (the developer)"
                );

            public static readonly GUIStyle LoggedInLabelStyle = new(EditorStyles.boldLabel)
            {
                alignment = TextAnchor.MiddleLeft
            };

            public static readonly GUILayoutOption[] LoggedInOptions =
            {
                GUILayout.MinWidth(0)
            };

            private static GUIStyle _boldFont18Style;

            public static GUIStyle boldFont18Style
            {
                get
                {
                    return _boldFont18Style ??= new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 18
                    };
                }
            }
        }

        private int _platformSelected = 0;

        private SerializedObject _nsdkSettings;
        private SerializedObject _authBuildSettings;

        private SerializedProperty _apiKeyProperty;
        private SerializedProperty _useNsdkDepthProperty;
        private SerializedProperty _useNsdkMeshingProperty;
        private SerializedProperty _preferLidarIfAvailableProperty;
        private SerializedProperty _useNsdkPersistentAnchorProperty;
        private SerializedProperty _useNsdkVps2Property;
        private SerializedProperty _useNsdkSemanticSegmentationProperty;
        private SerializedProperty _useNsdkScanningProperty;
        private SerializedProperty _useNsdkObjectDetectionProperty;
        private SerializedProperty _useNsdkWorldPositioningProperty;
        private SerializedProperty _locationAndCompassDataSourceProperty;
        private SerializedProperty _spoofLocationInfoProperty;
        private SerializedProperty _spoofCompassInfoProperty;
        private SerializedProperty _unityLogLevelProperty;
        private SerializedProperty _fileLogLevelProperty;
        private SerializedProperty _stdOutLogLevelProperty;
        private SerializedProperty _useZBufferDepthInSimulationProperty;
        private SerializedProperty _useSimulationPersistentAnchorInSimulationProperty;
        private SerializedProperty _nsdkPersistentAnchorParamsProperty;
        private SerializedProperty _useDeveloperAuthenticationProperty;
        private IPlaybackSettingsEditor[] _playbackSettingsEditors;
        private Texture _enabledIcon;
        private Texture _disabledIcon;

        private void OnEnable()
        {
            _nsdkSettings = new SerializedObject(NsdkSettings.Instance);
            _authBuildSettings = new SerializedObject(NsdkSettings.Instance.AuthBuildSettings);
            _apiKeyProperty = _nsdkSettings.FindProperty("_apiKey");

            _useNsdkDepthProperty = _nsdkSettings.FindProperty("_useNsdkDepth");
            _useNsdkMeshingProperty = _nsdkSettings.FindProperty("_useNsdkMeshing");
            _preferLidarIfAvailableProperty = _nsdkSettings.FindProperty("_preferLidarIfAvailable");
            _useNsdkPersistentAnchorProperty = _nsdkSettings.FindProperty("_useNsdkPersistentAnchor");
            _useNsdkVps2Property = _nsdkSettings.FindProperty("_useNsdkVps2");
            _useNsdkSemanticSegmentationProperty =
                _nsdkSettings.FindProperty("_useNsdkSemanticSegmentation");
            _useNsdkScanningProperty = _nsdkSettings.FindProperty("_useNsdkScanning");
            _useNsdkObjectDetectionProperty =
                _nsdkSettings.FindProperty("_useNsdkObjectDetection");
            _useNsdkWorldPositioningProperty = _nsdkSettings.FindProperty("_useNsdkWorldPositioning");

            _locationAndCompassDataSourceProperty = _nsdkSettings.FindProperty("_locationAndCompassDataSource");
            _spoofLocationInfoProperty = _nsdkSettings.FindProperty("_spoofLocationInfo");
            _spoofCompassInfoProperty = _nsdkSettings.FindProperty("_spoofCompassInfo");

            _unityLogLevelProperty = _nsdkSettings.FindProperty("_unityLogLevel");
            _fileLogLevelProperty = _nsdkSettings.FindProperty("_fileLogLevel");
            _stdOutLogLevelProperty = _nsdkSettings.FindProperty("_stdoutLogLevel");

            // Simulation sub-properties
            _useZBufferDepthInSimulationProperty = _nsdkSettings.FindProperty("_nsdkSimulationParams._useZBufferDepth");
            _useSimulationPersistentAnchorInSimulationProperty = _nsdkSettings.FindProperty("_nsdkSimulationParams._useSimulationPersistentAnchor");
            _nsdkPersistentAnchorParamsProperty = _nsdkSettings.FindProperty("_nsdkSimulationParams._simulationPersistentAnchorParams");

            _playbackSettingsEditors =
                new IPlaybackSettingsEditor[] { new EditorPlaybackSettingsEditor(), new DevicePlaybackSettingsEditor() };

            _enabledIcon = EditorGUIUtility.IconContent("TestPassed").image;
            _disabledIcon = EditorGUIUtility.IconContent("Warning").image;
            _useDeveloperAuthenticationProperty = _authBuildSettings.FindProperty("_useDeveloperAuthentication");
        }

        public override void OnInspectorGUI()
        {
            _nsdkSettings.Update();
            _authBuildSettings.Update();

            using (var change = new EditorGUI.ChangeCheckScope())
            {
                // Disable changes to the asset during runtime
                EditorGUI.BeginDisabledGroup(Application.isPlaying);

                // -- Put new NSDK settings here --
                DrawLNsdkSettings();

                EditorGUILayout.Space(20);
                DrawPlaybackSettings();

                EditorGUILayout.Space(20);
                // -- Put new simulation settings here --
                DrawNsdkSimulationSettings();

                // -- Put experimental settings here, when there are any --
#if NIANTICSPATIAL_NSDK_EXPERIMENTAL_FEATURES
                // DrawExperimentalSettings();
#endif

                EditorGUI.EndDisabledGroup();
                if (change.changed)
                {
                    _nsdkSettings.ApplyModifiedProperties();
                    _authBuildSettings.ApplyModifiedProperties();
                }
            }
        }

        private CancellationTokenSource _loginCts = new();

        private void DrawLNsdkSettings()
        {
            EditorGUIUtility.labelWidth = 220;

            EditorGUILayout.Space(10);
            EditorGUILayout.BeginHorizontal();

            var editorSDKEnabled =
                NsdkEditorUtilities.GetStandaloneIsNsdkPluginEnabled() ||
                IsNsdkSimulatorEnabled();

            var androidSDKEnabled = NsdkEditorUtilities.GetAndroidIsNsdkPluginEnabled();
            var iosSDKEnabled = NsdkEditorUtilities.GetIosIsNsdkPluginEnabled();

            LayOutSDKEnabled("Editor", editorSDKEnabled, BuildTargetGroup.Standalone);
            LayOutSDKEnabled("Android", androidSDKEnabled, BuildTargetGroup.Android);
            LayOutSDKEnabled("iOS", iosSDKEnabled, BuildTargetGroup.iOS);

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space(10);

            EditorGUILayout.LabelField("Credentials", EditorStyles.boldLabel);

            var loginInProgress = AuthEditorLoginCommand.Instance.InProgress;
            var isLoggedInOrInProgress = IsLoggedIn()|| loginInProgress;
#if NIANTICSPATIAL_NSDK_APIKEY_ENABLED
            var apiKeySet = !string.IsNullOrEmpty(NsdkSettings.Instance.ApiKey);
            using (new EditorGUI.DisabledScope(isLoggedInOrInProgress && !apiKeySet))
            {
                EditorGUILayout.PropertyField(_apiKeyProperty, Contents.apiKeyLabel);

                var navigateToNsdk = GUILayout.Button("Get API Key", GUILayout.Width(125));
                if (navigateToNsdk)
                {
                    Application.OpenURL("https://lightship.dev/account/projects");
                }
            }

            // Disable login UI and developer authentication option if API key has been set:
            using (new EditorGUI.DisabledScope(apiKeySet && !isLoggedInOrInProgress))
#endif
            {
                DrawLoginUI();
                EditorGUILayout.PropertyField(_useDeveloperAuthenticationProperty);
            }
#if NIANTICSPATIAL_NSDK_AUTH_DEBUG
            var editorSettings = AuthEditorSettings.Instance;
            var settings = AuthEditorBuildSettings.Instance;
            var authEnvironmentNames = Enum.GetNames(typeof(AuthEnvironmentType));
            var newEnvironment = (AuthEnvironmentType)GUILayout.Toolbar(
                (int)editorSettings.AuthEnvironment, authEnvironmentNames, EditorStyles.toolbarButton);

            // When the environment changes, we need to clear all access tokens (effectively "log out")
            if (editorSettings.AuthEnvironment != newEnvironment)
            {
                if (IsLoggedIn())
                {
                    if (EditorUtility.DisplayDialog("Change Environment", "This will log you out of the current environment. Continue?", "Yes", "No"))
                    {
                        AuthEditorLogoutCommand.Instance.Execute();
                        editorSettings.AuthEnvironment = newEnvironment;
                    }
                }
                else
                {
                    editorSettings.AuthEnvironment = newEnvironment;
                    editorSettings.UpdateEditorAccess(string.Empty, 0, string.Empty, 0);
                    settings.UpdateAccess(string.Empty, 0, string.Empty, 0);
                }
            }

            DrawToken("Editor Refresh", editorSettings.EditorRefreshToken, editorSettings.EditorRefreshExpiresAt);
            DrawToken("Editor Access", editorSettings.EditorAccessToken, editorSettings.EditorAccessExpiresAt);

            var refresh = GUILayout.Button("Refresh", GUILayout.Width(125));
            if (refresh)
            {
                _ = AuthEditorSettingsUpdater.Instance.RefreshAccessAsync(editorSettings);
            }

            if (!string.IsNullOrEmpty(editorSettings.EditorRefreshToken))
            {
                var requestRuntime = GUILayout.Button("Request Runtime", GUILayout.Width(125));
                if (requestRuntime)
                {
                    _ = AuthRuntimeSettingsUpdater.Instance.RequestRuntimeRefreshTokenAsync(
                        editorSettings.EditorRefreshToken, settings, isRuntimeLogin: false);
                }
            }

            DrawToken("Refresh", settings.RefreshToken, settings.RefreshExpiresAt);
            DrawToken("Access", settings.AccessToken, settings.AccessExpiresAt);

            if (!string.IsNullOrEmpty(settings.RefreshToken))
            {
                var refreshRuntime = GUILayout.Button("Refresh Runtime", GUILayout.Width(125));
                if (refreshRuntime)
                {
                    _ = AuthRuntimeSettingsUpdater.Instance.RefreshRuntimeAccessAsync(settings);
                }
            }

            if (AuthRuntimeSettingsStore.Instance.Exists)
            {
                var readPlaymode = GUILayout.Button("Read Play-Mode", GUILayout.Width(125));
                if (readPlaymode)
                {
                    _ = AuthRuntimeSettingsStore.Instance.LoadAsync(settings, CancellationToken.None);
                }

                var clearPlaymode = GUILayout.Button("Clear Play-Mode", GUILayout.Width(125));
                if (clearPlaymode)
                {
                    AuthRuntimeSettingsStore.Instance.Clear();
                }
            }
#endif

            // Depth settings

            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Depth", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_useNsdkDepthProperty, Contents.enabledLabel);
            EditorGUI.indentLevel++;
            // Put Depth sub-settings here
            if (_useNsdkDepthProperty.boolValue)
            {
                EditorGUILayout.PropertyField
                    (_preferLidarIfAvailableProperty, Contents.preferLidarLabel);
            }

            EditorGUI.indentLevel--;

            // Semantic Segmentation settings
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Semantic Segmentation", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField
                (_useNsdkSemanticSegmentationProperty, Contents.enabledLabel);


            // Meshing settings
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Meshing", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_useNsdkMeshingProperty, Contents.enabledLabel);
            EditorGUI.indentLevel++;
            // Put Meshing sub-settings here
            EditorGUI.indentLevel--;

            // Persistent Anchors settings
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Persistent Anchors", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField
                (_useNsdkPersistentAnchorProperty, Contents.enabledLabel);

            EditorGUI.indentLevel++;
            // Put Persistent Anchors sub-settings here
            EditorGUI.indentLevel--;

            // VPS2 settings
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("VPS2", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField
                (_useNsdkVps2Property, Contents.enabledLabel);

            EditorGUI.indentLevel++;
            // Put VPS2 sub-settings here
            EditorGUI.indentLevel--;

            // Scanning settings
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Scanning", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_useNsdkScanningProperty, Contents.enabledLabel);
            EditorGUI.indentLevel++;
            // Put Scanning sub-settings here
            EditorGUI.indentLevel--;

            // Object Detection settings
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Object Detection", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField
                (_useNsdkObjectDetectionProperty, Contents.enabledLabel);

            EditorGUI.indentLevel++;
            // Put Object Detection sub-settings here
            EditorGUI.indentLevel--;

            // World Positioning settings
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("World Positioning System", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_useNsdkWorldPositioningProperty, Contents.enabledLabel);

            EditorGUI.indentLevel++;
            // Put World Positioning sub-settings here
            EditorGUI.indentLevel--;

            EditorGUILayout.Space(10);
            DrawLocationSourceSettings();

            EditorGUILayout.Space(10);
            DrawLoggingSettings();
        }

        private void DrawLoginUI()
        {
            EditorGUILayout.BeginHorizontal();
            if (!IsLoggedIn())
            {
                if (!AuthEditorLoginCommand.Instance.InProgress)
                {
                    if (GUILayout.Button(Contents.LoginButton, GUILayout.Width(125)))
                    {
                        _ = AuthEditorLoginCommand.Instance.ExecuteAsync(_loginCts.Token);
                    }
                }
                else
                {
                    using (new EditorGUI.DisabledScope(true))
                    {
                        GUILayout.Button("Logging in ...", GUILayout.Width(125));
                    }
                    var cancelLogin = GUILayout.Button("Cancel", GUILayout.Width(125));
                    if (cancelLogin)
                    {
                        _loginCts.Cancel();
                        _loginCts = new();
                    }
                }
            }
            else
            {
                var logoutFromNsdk = GUILayout.Button("Logout", GUILayout.Width(125));
                if (logoutFromNsdk)
                {
                    AuthEditorLogoutCommand.Instance.Execute();
                }

                EditorGUILayout.LabelField
                (
                    new GUIContent(GetLoggedInString(), _enabledIcon),
                    Contents.LoggedInLabelStyle,
                    Contents.LoggedInOptions
                );
            }

            EditorGUILayout.EndHorizontal();
        }

        private string GetLoggedInString()
        {
            var body = AuthGatewayUtils.Instance.DecodeJwtTokenBody(AuthEditorSettings.Instance.EditorRefreshToken);
            if (!string.IsNullOrEmpty(body?.name) && !string.IsNullOrEmpty(body.email))
            {
                return $"Logged In: {body.name} ({body.email})";
            }

            var userName = !string.IsNullOrEmpty(body?.name) ? body.name : body?.email;
            return $"Logged In: {userName ?? "<unknown>"}";
        }

        private bool IsLoggedIn()
        {
            var editorSettings = AuthEditorSettings.Instance;
            return !string.IsNullOrEmpty(editorSettings.EditorRefreshToken) &&
                !AuthGatewayUtils.Instance.IsAccessExpired(editorSettings.EditorRefreshExpiresAt, DateTime.UtcNow);
        }

        private void DrawToken(string context, string token, int expiresAt)
        {
#if NIANTICSPATIAL_NSDK_AUTH_DEBUG
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"{context} Token:", GUILayout.Width(150));
            EditorGUILayout.LabelField(AuthGatewayUtils.GetTokenShortName(token), GUILayout.Width(50));
            EditorGUILayout.LabelField(GetTimeString(expiresAt), GUILayout.Width(150));
            EditorGUILayout.TextField(token);
            EditorGUILayout.EndHorizontal();
#endif
        }

        private static string GetTimeString(int unixTimeSeconds)
        {
            if (unixTimeSeconds > 0)
            {
                var localTime = DateTimeOffset.FromUnixTimeSeconds(unixTimeSeconds).ToLocalTime();
                return localTime.ToString("yyyy-MM-dd HH:mm:ss");
            }

            return "n/a";
        }


        private void DrawLoggingSettings()
        {
            EditorGUILayout.LabelField("Logging", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField
            (
                _unityLogLevelProperty,
                new GUIContent("Unity Log Level", tooltip: "Log level for Unity's built-in logging system")
            );

            EditorGUILayout.PropertyField
            (
                _stdOutLogLevelProperty,
                new GUIContent
                (
                    "Stdout Log Level",
                    tooltip: "Log level for stdout logging system. Recommended to be set to 'off'"
                )
            );

            EditorGUILayout.PropertyField
            (
                _fileLogLevelProperty,
                new GUIContent
                (
                    "File Log Level",
                    tooltip: "Log level for logging things into a file. " +
                    "Recommended to be set to 'off' unless its a niantic support case. File Location: {Project-Root}/data/log.txt"
                )
            );
        }

        private void DrawPlaybackSettings()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Playback", Contents.boldFont18Style, GUILayout.Width(80));
            if (EditorGUILayout.LinkButton(Contents.playbackLabel))
            {
                Application.OpenURL("https://nianticspatial.com/docs/ardk/how-to/unity/setting_up_playback/");
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            var currPlatform = GUILayout.Toolbar(_platformSelected, Contents._platforms);
            GUILayout.EndHorizontal();

            // If the playback dataset text field is focused when the Playback platform is changed,
            // the text field content will not switch to the new platform's dataset path, causing confusion.
            // To prevent this, we clear the focus when the platform is changed.
            if (currPlatform != _platformSelected)
            {
                GUI.FocusControl(null);
                _platformSelected = currPlatform;
            }

            _playbackSettingsEditors[_platformSelected].DrawGUI();
        }

        private void DrawNsdkSimulationSettings()
        {
            GUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Simulation", Contents.boldFont18Style, GUILayout.Width(95));
            if (EditorGUILayout.LinkButton(Contents.simulationLabel))
            {
                Application.OpenURL("https://nianticspatial.com/docs/ardk/how-to/unity/simulation_mocking/");
            }
            GUILayout.EndHorizontal();

            // Simulation status label
            LayOutSimulationEnabled();

            EditorGUI.BeginDisabledGroup(!IsNsdkSimulatorEnabled());

            // Environment prefab
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Simulation Environment", EditorStyles.boldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(Contents.environmentViewLabel, GUILayout.Width(EditorGUIUtility.labelWidth));
            bool clicked = GUILayout.Button(Contents.environmentViewButton, GUILayout.ExpandWidth(true));
            EditorGUILayout.EndHorizontal();
            if (clicked)
            {
                EditorApplication.delayCall += () =>
                {
                    EditorApplication.ExecuteMenuItem(XREnvironmentViewPath);
                };
            }

            // Depth
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Depth", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField
            (
                _useZBufferDepthInSimulationProperty,
                Contents.useZBufferDepthInSimulationLabel
            );

            // Persistent anchors (currently forcing the use of the simulation mock system)
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Persistent Anchors", EditorStyles.boldLabel);
            // EditorGUILayout.PropertyField
            // (
            //     _useSimulationPersistentAnchorInSimulationProperty,
            //     Contents.useNsdkPersistentAnchorInSimulationLabel
            // );
            //
            // EditorGUI.indentLevel++;
            if (_useSimulationPersistentAnchorInSimulationProperty.boolValue) // always true
            {
                // Persistent anchor sub-settings
                EditorGUIUtility.labelWidth = 285;
                EditorGUILayout.PropertyField
                    (_nsdkPersistentAnchorParamsProperty, GUILayout.ExpandWidth(true));
            }
            // EditorGUI.indentLevel--;

            EditorGUILayout.Space(10);
            EditorGUI.EndDisabledGroup();
        }

        private void DrawLocationSourceSettings()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField("Location & Compass", EditorStyles.boldLabel);
            EditorGUILayout.PropertyField(_locationAndCompassDataSourceProperty, new GUIContent("Data Source"));

            EditorGUILayout.PropertyField(_spoofLocationInfoProperty);
            EditorGUILayout.PropertyField(_spoofCompassInfoProperty);
        }

        private void DrawExperimentalSettings()
        {
            EditorGUILayout.Space(10);
            EditorGUILayout.LabelField
            (
                "Experimental",
                Contents.boldFont18Style
            );
        }

        private void LayOutSDKEnabled(string platform, bool enabled, BuildTargetGroup group = BuildTargetGroup.Unknown)
        {
            if (enabled)
            {
                EditorGUILayout.LabelField
                (
                    new GUIContent
                    (
                        $"{platform} : SDK Enabled",
                        _enabledIcon,
                        $"Niantic Spatial Development Kit is selected as the plug-in provider for {platform} XR. " +
                        "The SDK is enabled for this platform."
                    ),
                    Contents.sdkEnabledStyle,
                    Contents.sdkEnabledOptions
                );
            }
            else
            {
                bool clicked = GUILayout.Button
                (
                    new GUIContent
                    (
                        $"{platform} : SDK Disabled",
                        _disabledIcon,
                        $"Niantic Spatial Development Kit is not selected as the plug-in provider for {platform} XR." +
                        "The SDK will not be used. Click to open Project Validation for more info on changing" +
                        " plug-in providers to enable NSDK."
                    ),
                    Contents.sdkDisabledStyle
                );
                if (clicked)
                {
                    // From OpenXRProjectValidationRulesSetup.cs,
                    // Delay opening the window since sometimes other settings in the player settings provider redirect to the
                    // project validation window causing serialized objects to be nullified
                    EditorApplication.delayCall += () =>
                    {
                        if (group is BuildTargetGroup.Standalone or BuildTargetGroup.Android or BuildTargetGroup.iOS)
                        {
                            EditorUserBuildSettings.selectedBuildTargetGroup = group;
                        }

                        SettingsService.OpenProjectSettings(ProjectValidationSettingsPath);
                    };
                }
            }
        }

        private void LayOutSimulationEnabled()
        {
            if (!IsNsdkSimulatorEnabled())
            {
                bool clicked = GUILayout.Button
                (
                    new GUIContent
                    (
                        $" NSDK Simulation Disabled",
                        _disabledIcon,
                        $"Niantic Spatial Development Kit Simulation is not enabled.\n\nTo enable NSDK simulation, " +
                        "navigate to the XR Plug-in Management settings and select NSDK Simulation " +
                        "as the plug-in provider for Standalone XR."
                    ),
                    Contents.sdkDisabledStyle
                );
                if (clicked)
                {
                    EditorApplication.delayCall += () =>
                    {
                        SettingsService.OpenProjectSettings(XRPluginManagementPath);
                    };
                }
            }
            else
            {
                EditorGUILayout.LabelField
                (
                    new GUIContent
                    (
                        $" NSDK Simulation Enabled",
                        _enabledIcon,
                        $"Niantic Spatial Development Kit Simulation is selected as the plug-in provider for Standalone XR."
                    ),
                    Contents.sdkEnabledStyle
                );
            }
        }

        private static bool IsNsdkSimulatorEnabled()
        {
            var generalSettings = XRGeneralSettingsPerBuildTarget.XRGeneralSettingsForBuildTarget(BuildTargetGroup.Standalone);
            if (null == generalSettings)
                return false;

            var managerSettings = generalSettings.AssignedSettings;
            if (null == managerSettings)
                return false;

            var simulationLoaderIsActive = managerSettings.activeLoaders.Any(loader => loader is NsdkSimulationLoader);
            return simulationLoaderIsActive;
        }
    }
}
