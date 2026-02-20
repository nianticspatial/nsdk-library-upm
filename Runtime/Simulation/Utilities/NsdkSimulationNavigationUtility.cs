// Copyright 2022-2026 Niantic Spatial.

using System.Reflection;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using UnityEngine.XR.Simulation;

namespace NianticSpatial.NSDK.AR.Simulation
{
    public static class NsdkSimulationNavigationUtility
    {
        // Disable the XRSimulationPreference's navigation field at runtime. This is necessary to prevent
        //  a NullReferenceException when Keyboard.current is null (ie CI runners).
        // Putting this utility in the loader to work with the XRSimulationBridge
        public static void SetArfSimulationNavigation(bool enabled)
        {
            var preferences = XRSimulationPreferences.Instance;
            var field = preferences.GetType()
                .GetField
                (
                    "m_EnableNavigation",
                    BindingFlags.Instance | BindingFlags.NonPublic
                );

            if (field != null)
            {
                field.SetValue(preferences, enabled);
            }
            else
            {
                Log.Warning("Could not disable navigation in XRSimulationPreferences. Field not found.");
            }
        }
    }
}
