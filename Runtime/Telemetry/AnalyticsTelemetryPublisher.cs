// Copyright 2022-2026 Niantic Spatial.
using System;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using Niantic.Lightship.AR.Protobuf;
using Niantic.Platform.Analytics.Telemetry;
using Niantic.Platform.Analytics.Telemetry.Logging;
using ILogHandler = Niantic.Platform.Analytics.Telemetry.Logging.ILogHandler;
using LogLevel = Niantic.Platform.Analytics.Telemetry.Logging.LogLevel;

namespace NianticSpatial.NSDK.AR.Telemetry
{
    public class AnalyticsTelemetryPublisher : ITelemetryPublisher
    {
        private readonly ARDKTelemetryService<ArdkNextTelemetryOmniProto> _nsdkPublisher;

        public AnalyticsTelemetryPublisher(string endpoint, string key, string directoryPath, bool registerLogger)
        {
            var builder = new ARDKTelemetryService<ArdkNextTelemetryOmniProto>.Builder
            (
                rpcEndpointUrl: endpoint,
                directoryPath,
                key
            );

            if (registerLogger)
            {
                var debugOptions = new StartupDebugOptions();
                debugOptions.LogHandler = new TelemetryLogger();
                debugOptions.LogOptions = LogOptions.All;

                builder.SetDebugOptions(debugOptions);
                Log.Debug("Registering logger for telemetry.");
            }

            _nsdkPublisher = builder.Build();
            Log.Debug("Successfully created the nsdk publisher.");
        }

        public void RecordEvent(ArdkNextTelemetryOmniProto telemetryEvent)
        {
            try
            {

                    _nsdkPublisher.RecordEvent(telemetryEvent);

            }
            catch (Exception)
            {
                // fail silently
                // enable for debugging
                // Log.Warning($"Posting telemetry failed with the following exception: {ex}");
            }
        }

        private class TelemetryLogger : ILogHandler
        {
            public void LogMessage(LogLevel logLevel, string message)
            {
                switch (logLevel)
                {
                    case LogLevel.Verbose:
                    case LogLevel.Info:

                        Log.Info(message);
                        break;

                    case LogLevel.Warning:
                        Log.Warning(message);
                        break;

                    case LogLevel.Error:
                    case LogLevel.Fatal:

                        Log.Error(message);
                        break;

                    default:
                        Log.Info(message);
                        break;
                }
            }
        }
    }
}
