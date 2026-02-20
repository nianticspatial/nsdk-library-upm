// Copyright 2022-2026 Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using UnityEngine;

namespace NianticSpatial.NSDK.AR
{
    public static class NativeHandleValidation
    {
        public static bool IsValidHandle(this IntPtr handle)
        {
            if (handle != IntPtr.Zero)
                return true;

            // With AR-15672 upgrade, won't need to explicitly ifdef here
#if NIANTICSPATIAL_NSDK_DEVELOPMENT
            Log.Warning("Attempted to call native API with an invalid handle.");
#endif

            return false;
        }
    }
}
