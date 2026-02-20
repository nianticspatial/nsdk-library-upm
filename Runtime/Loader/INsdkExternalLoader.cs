// Copyright 2022-2026 Niantic Spatial.

using UnityEngine.XR.Management;

namespace NianticSpatial.NSDK.AR.Loader
{
    /// <summary>
    /// [Experimental] An interface to be implemented by packages to provide similar functionality to a Loader.
    /// Classes implementing this interface can be registered through the INsdkLoader interface
    /// so that their Initialize and Deinitialize methods get called at the appropriate times for
    /// creating and destroying subsystems.
    ///
    /// This Interface is experimental so may change or be removed from future versions without warning.
    /// </summary>
    public interface INsdkExternalLoader
    {
        public bool Initialize(INsdkLoader loader);
        public void Deinitialize(INsdkLoader loader);
    }
}
