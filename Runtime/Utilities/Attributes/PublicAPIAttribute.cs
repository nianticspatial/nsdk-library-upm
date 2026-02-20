// Copyright 2022-2026 Niantic Spatial.

using System;

namespace NianticSpatial.NSDK.AR.Utilities
{
    // Used for Doxygen generation
    [AttributeUsage(AttributeTargets.All, Inherited = false, AllowMultiple = false)]
    public class PublicAPIAttribute : UnityEngine.HelpURLAttribute
    {
        public PublicAPIAttribute(string helpUrl = "")
            : base($"https://nianticspatial.com/docs/ardk/{helpUrl}")
        { }
    }
}
