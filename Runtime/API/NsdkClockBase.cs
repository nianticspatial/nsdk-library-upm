// Copyright Niantic Spatial.

namespace NianticSpatial.NSDK.AR.API
{
    // Defined in ardk_clock_base.h (ARDK_ClockBase)
    internal enum NsdkClockBase : uint
    {
        // Wall-clock time measured from the Unix epoch.
        Epoch = 0,

        // Monotonic time from a steadily-increasing clock (e.g. time since device boot).
        Monotonic = 1,
    }
}
