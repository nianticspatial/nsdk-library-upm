// Copyright Niantic Spatial.

namespace NianticSpatial.NSDK.AR.XRSubsystems
{
    /// <summary>
    /// Quality of the device's VPS localization.
    ///
    /// Note: <c>Precise</c> can persist while a device is disturbed, such as a covered camera
    /// or a dark room, and persists indefinitely while the device stays still. It changes once
    /// confidence drops low enough.
    /// </summary>
    public enum Vps2TrackingState
    {
        Unavailable,
        Coarse,
        Precise
    }
}
