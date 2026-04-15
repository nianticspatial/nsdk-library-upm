// Copyright Niantic Spatial.

namespace NianticSpatial.NSDK.AR
{
    /// <summary>
    /// Controls how the heading is computed from the device's orientation.
    /// Mirrors native <c>ARDK_HeadingMode</c>.
    /// </summary>
    public enum HeadingMode
    {
        /// <summary>
        /// Heading from the camera's forward axis (perpendicular to screen).
        /// Best when the device is held upright in portrait or landscape.
        /// </summary>
        CameraDirection = 0,

        /// <summary>
        /// Heading from the top edge of the screen, accounting for display orientation.
        /// Best when the device is held face-up or for compass widgets.
        /// </summary>
        DeviceTop = 1,
    }
}
