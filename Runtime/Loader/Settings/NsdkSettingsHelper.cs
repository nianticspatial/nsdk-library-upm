// Copyright 2022-2026 Niantic Spatial.

using System;
using UnityEngine;

namespace NianticSpatial.NSDK.AR.Loader
{
    public static class NsdkSettingsHelper
    {
        // The values in this settings instance can be different from the values
        // currently used by the NSDK system, if they have been altered since
        // components in the NSDK system have been initialized.
        private static RuntimeNsdkSettings _activeSettings;

        // This is guaranteed to stay the same object between when either of this class's
        // create methods are called, and when ClearRuntimeSettings is called. That means
        // that users can safely cache this object and always get the latest values.
        public static RuntimeNsdkSettings ActiveSettings
        {
            get
            {
                return _activeSettings;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void CreateRuntimeSettingsFromAsset()
        {
            var asset = NsdkSettings.Instance;
            _activeSettings = new RuntimeNsdkSettings(asset);
            OnRuntimeSettingsCreated?.Invoke();
        }

        public static event Action OnRuntimeSettingsCreated;

        public static void CreateDefaultRuntimeSettings()
        {
            _activeSettings = new RuntimeNsdkSettings();
        }

        // Used to make sure settings are cleared between test case runs.
        // In a non-test setting, this cannot be called because there's no
        // master exit point for Unity applications, and instead we rely on the
        // ActiveSettings to be re-initialized when the application is restarted.
        public static void ClearRuntimeSettings()
        {
            _activeSettings = null;
        }
    }
}
