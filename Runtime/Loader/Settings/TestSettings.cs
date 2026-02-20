// Copyright 2022-2026 Niantic Spatial.

namespace NianticSpatial.NSDK.AR.Loader
{
    internal class TestSettings
    {
        public bool DisableTelemetry { get; set; } = true;
        public bool TickPamOnUpdate { get; set; } = true;

        public TestSettings() { }

        public TestSettings(TestSettings source)
        {
            DisableTelemetry = source.DisableTelemetry;
            TickPamOnUpdate = source.TickPamOnUpdate;
        }
    }
}
