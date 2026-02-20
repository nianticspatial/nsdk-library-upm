// Copyright 2022-2026 Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.Core;
using UnityEngine;

namespace NianticSpatial.NSDK.AR.Utilities.Preloading
{
    [PublicAPI]
    public class ModelPreloaderFactory
    {
        public static IModelPreloader Create()
        {
            return Create(false);
        }

        internal static IModelPreloader Create(bool useMock = false)
        {
#if NIANTICSPATIAL_NSDK_AR_LOADER_ENABLED
            var unityContextHandle = NsdkUnityContext.UnityContextHandle;

            if (unityContextHandle == IntPtr.Zero)
            {
                Debug.Assert(false, "NSDK must be initialized before the Model Preloader can be " +
                    "called. An application can use RegisterModel to complete the download before starting NSDK.");
                return null;
            }

            if (!useMock)
            {
                return new NativeModelPreloader(NsdkUnityContext.UnityContextHandle);
            }
            else
            {
                Debug.Assert(false, "ModelPreloader mock is not implemented");
                return null;
            }
#else
            return null;
#endif
        }
    }
}
