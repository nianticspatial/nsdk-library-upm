// Copyright 2022-2026 Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.API;

namespace NianticSpatial.NSDK.AR.Subsystems.Dashcam
{
    /// <summary>
    /// Wraps the native dashcam C API. Implementations:
    /// <see cref="NativeApi"/> (real P/Invoke) and <c>MockApi</c> (test double).
    /// </summary>
    internal interface IApi
    {
        NsdkStatus Create(IntPtr nsdkHandle);
        NsdkStatus Destroy(IntPtr nsdkHandle);
        NsdkStatus Configure(IntPtr nsdkHandle, ref NsdkDashcamConfig config);
        NsdkStatus Start(IntPtr nsdkHandle);
        NsdkStatus Stop(IntPtr nsdkHandle);
        NsdkStatus GetBufferInfo(IntPtr nsdkHandle, out NsdkDashcamBufferInfo info);
        NsdkStatus TriggerSave(IntPtr nsdkHandle);
        NsdkStatus GetSaveResult(IntPtr nsdkHandle, out NsdkDashcamSaveResult result);
        int GetFeatureStatus(IntPtr nsdkHandle);
    }
}
