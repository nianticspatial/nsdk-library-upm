// Copyright 2022-2026 Niantic Spatial.

namespace NianticSpatial.NSDK.AR.Core
{
        /// <summary>
        /// [Experimental] <c>NsdkPlugin</c> holds general properties of the NSDK Unity plugin.
        ///
        /// This Interface is experimental so may change or be removed from future versions without warning.
        /// </summary>
        /// <value><c>Name</c> is the name of the shared library from which native symbols can be imported.</value>
        public static class NsdkPlugin
        {
#if UNITY_IOS && !UNITY_EDITOR
        public const string Name = "__Internal";
#elif (UNITY_EDITOR && !IN_ROSETTA) || UNITY_STANDALONE_OSX || UNITY_ANDROID
        public const string Name = "LightshipARDK";
#else
                public const string Name = "PLATFORM_NOT_SUPPORTED";
#endif
        }
}
