// Copyright 2022-2026 Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.Loader;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using UnityEngine.XR.Management;

namespace NianticSpatial.NSDK.AR.VpsCoverage
{
    /// Factory to create CoverageClient instances.
    public static class CoverageClientFactory
    {
        [Obsolete("Create a CoverageClient instance using the default constructor instead.")]
        public static CoverageClient Create()
        {
            return new CoverageClient();
        }
    }
}
