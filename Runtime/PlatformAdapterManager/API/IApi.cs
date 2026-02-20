// Copyright 2022-2026 Niantic Spatial.
using System;

namespace NianticSpatial.NSDK.AR.PAM
{
    internal interface IApi
    {
        public IntPtr ARDK_SAH_Create(IntPtr unityContext, bool isLidarDepthEnabled);
        public void ARDK_SAH_OnFrame(IntPtr handle, IntPtr frameData);

        public void ARDK_SAH_Release(IntPtr handle);

        public void ARDK_SAH_GetDataFormatsReadyForNewFrame
        (
            IntPtr handle,
            out uint dataFormatsReady
        );

        public void ARDK_SAH_GetDispatchedFormatsToModules
        (
            IntPtr handle,
            out uint dispatchedFrameId,
            out ulong dispatchedToModules,
            out uint dispatchedDataFormats
        );
    }
}
