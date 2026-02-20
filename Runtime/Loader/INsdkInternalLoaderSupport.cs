// Copyright 2022-2026 Niantic Spatial.

using System.Collections.Generic;
using UnityEngine;

namespace NianticSpatial.NSDK.AR.Loader
{
    internal interface INsdkInternalLoaderSupport : INsdkLoader
    {
        /// <summary>
        /// Initializes the loader with an injected NsdkLoaderHelper.
        /// This is a helper to initialize manually from tests.
        /// </summary>
        /// <returns>`True` if the session subsystems were successfully created, otherwise `false`.</returns>
        public bool InitializeWithNsdkHelper(NsdkLoaderHelper nsdkLoaderHelper);

        bool InitializePlatform();

        bool DeinitializePlatform();

        bool IsPlatformDepthAvailable();
    }
}
