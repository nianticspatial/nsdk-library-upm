// Copyright 2022-2026 Niantic Spatial.

using System.IO;
using Newtonsoft.Json;
using NianticSpatial.NSDK.AR.Auth;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using NianticSpatial.NSDK.Utilities.UnityAssets;
using UnityEngine;

namespace NianticSpatial.NSDK.AR.Loader
{
    internal class EndpointSettings
    {
        public string ScanningEndpoint { get; set; }
        public string VpsEndpoint { get; set; }
        public string SharedArEndpoint { get; set; }
        public string FastDepthSemanticsEndpoint { get; set; }
        public string DefaultDepthSemanticsEndpoint { get; set; }
        public string SmoothDepthSemanticsEndpoint { get; set; }
        public string ScanningSqcEndpoint { get; set; }
        public string TelemetryEndpoint { get; set; }
        public string IdentityEndpoint { get; set; }
        public string BevEndpoint { get; set; }
        public string PortalEndpoint { get; set; }
        public string GeographiclibGeoidEndpoint { get; set; }

        private const string ConfigFileName = "nsdkConfig.json";

        /// <summary>
        /// The production endpoint set, compiled in as the fallback for projects that carry no endpoint
        /// config in StreamingAssets. Every host here is public.
        /// </summary>
        public static EndpointSettings GetDefaultEnvironmentConfig()
        {
            EndpointSettings defaultSettings = new EndpointSettings()
            {
                ScanningEndpoint = "https://wayfarer-ugc-api.nianticspatial.com/api/proto/v1/",
                ScanningSqcEndpoint = "https://armodels.eng.nianticspatial.com/sqc/sqc3_enc.tar.gz",

                SharedArEndpoint = "marsh-prod.nianticspatial.com",

                VpsEndpoint = "https://vps-frontend.nianticspatial.com/web",

                DefaultDepthSemanticsEndpoint = "https://armodels.eng.nianticspatial.com/niantic_ca_v1.2.bin",
                FastDepthSemanticsEndpoint = "https://armodels.eng.nianticspatial.com/niantic_ca_v1.2_fast.bin",
                SmoothDepthSemanticsEndpoint = "https://armodels.eng.nianticspatial.com/niantic_ca_v1.2_antiflicker.bin",

                TelemetryEndpoint = "https://analytics.nianticspatial.com",
                IdentityEndpoint = AuthEnvironment.Instance.GetIdentityEndpoint(AuthEnvironmentType.Production),

                PortalEndpoint = "https://portal-backend-api.nianticspatial.com/api/v1/",

                BevEndpoint = "https://vps-frontend.nianticspatial.com/web",
                GeographiclibGeoidEndpoint = "https://armodels.eng.nianticspatial.com/geo/egm96-5.tar.gz",
            };

            return defaultSettings;
        }

        /// <summary>
        /// Path to the endpoint config for <paramref name="environment"/> under StreamingAssets.
        /// </summary>
        /// <remarks>
        /// Only internal projects carry this directory.
        /// </remarks>
        private static string GetEnvironmentConfigPath(AuthEnvironmentType environment)
        {
            // The directory names match the AuthEnvironmentType values: Dev, Staging, Production.
            return
                Path.Combine
                (
                    Application.streamingAssetsPath,
                    "Internal",
                    "EndpointConfigs",
                    environment.ToString(),
                    ConfigFileName
                );
        }

        public static bool TryGetConfigurationFromJson(string fileWithPath, out EndpointSettings parsedConfig)
        {
            parsedConfig = null;

            bool hasData = FileUtilities.TryReadAllText(fileWithPath, out string result);

            if (hasData && !string.IsNullOrWhiteSpace(result))
            {
                try
                {
                    parsedConfig = JsonConvert.DeserializeObject<EndpointSettings>(result);
                    Log.Debug($"Targeting the endpoints in {fileWithPath}: {result}");
                    return true;
                }
                catch
                {
                    Log.Warning($"Parsing the config at {fileWithPath} failed. Ignoring it.");
                }
            }

            return false;
        }

        /// <summary>
        /// Resolves the endpoint set for <paramref name="requestedEnvironment"/>, taking the first of:
        ///   1. nsdkConfig.json at the root of StreamingAssets - a project-wide override that wins for
        ///      every environment, identity endpoint included.
        ///   2. The environment's config under Internal/EndpointConfigs/ in StreamingAssets.
        ///   3. <see cref="GetDefaultEnvironmentConfig"/>, the compiled-in production set.
        /// </summary>
        /// <remarks>
        /// Only case 2 honours the argument, hence "requested": the override in case 1 replaces the
        /// endpoints whatever the environment, and case 3 is production by definition.
        /// Selecting dev or staging in a project that carries no config for it lands on production rather
        /// than a mix of production services and a non-production identity server, and warns.
        /// </remarks>
        public static EndpointSettings GetFromFileOrDefault(
            AuthEnvironmentType requestedEnvironment = AuthEnvironmentType.Production)
        {
            var overridePath = Path.Combine(Application.streamingAssetsPath, ConfigFileName);

            if (TryGetConfigurationFromJson(overridePath, out EndpointSettings overrideConfig))
            {
                Log.Info($"NSDK endpoints: overridden by {overridePath}");
                return overrideConfig;
            }

            var environmentConfigPath = GetEnvironmentConfigPath(requestedEnvironment);

            if (TryGetConfigurationFromJson(environmentConfigPath, out EndpointSettings environmentConfig))
            {
                // The auth gateway derives its identity server from the environment rather than from the
                // config file, so the two have to agree.
                environmentConfig.IdentityEndpoint = AuthEnvironment.Instance.GetIdentityEndpoint(requestedEnvironment);

                if (requestedEnvironment != AuthEnvironmentType.Production)
                {
                    Log.Info($"NSDK endpoints: {requestedEnvironment} environment");
                }

                return environmentConfig;
            }

            if (requestedEnvironment != AuthEnvironmentType.Production)
            {
                Log.Warning
                (
                    $"NSDK endpoints: no {requestedEnvironment} config at {environmentConfigPath}. " +
                    "Falling back to the production endpoints."
                );
            }

            return GetDefaultEnvironmentConfig();
        }

        public EndpointSettings() { }

        public EndpointSettings(EndpointSettings source)
        {
            CopyFrom(source);
        }

        internal void CopyFrom(EndpointSettings source)
        {
            ScanningEndpoint = source.ScanningEndpoint;
            VpsEndpoint = source.VpsEndpoint;
            SharedArEndpoint = source.SharedArEndpoint;
            FastDepthSemanticsEndpoint = source.FastDepthSemanticsEndpoint;
            DefaultDepthSemanticsEndpoint = source.DefaultDepthSemanticsEndpoint;
            SmoothDepthSemanticsEndpoint = source.SmoothDepthSemanticsEndpoint;
            ScanningSqcEndpoint = source.ScanningSqcEndpoint;
            TelemetryEndpoint = source.TelemetryEndpoint;
            IdentityEndpoint = source.IdentityEndpoint;
            BevEndpoint = source.BevEndpoint;
            PortalEndpoint = source.PortalEndpoint;
            GeographiclibGeoidEndpoint = source.GeographiclibGeoidEndpoint;
        }
    }
}
