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

        public static EndpointSettings GetDefaultEnvironmentConfig(
            AuthEnvironmentType defaultAuthEnvironment = AuthEnvironmentType.Production)
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
                IdentityEndpoint = AuthEnvironment.Instance.GetIdentityEndpoint(defaultAuthEnvironment),

                PortalEndpoint = "https://portal-backend-api.nianticspatial.com/api/v1/",

                BevEndpoint = "https://vps-frontend.nianticspatial.com/web",
                GeographiclibGeoidEndpoint = "https://armodels.eng.nianticspatial.com/geo/egm96-5.tar.gz",
            };

            return defaultSettings;
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
                    Log.Warning($"Targeting the following endpoints: {result}");
                    return true;
                }
                catch
                {
                    Log.Warning("Parsing the config failed. Defaulting to the prod config.");
                }
            }

            return false;
        }

        public static EndpointSettings GetFromFileOrDefault(
            AuthEnvironmentType defaultAuthEnvironment = AuthEnvironmentType.Production)
        {
            const string ConfigFileName = "nsdkConfig.json";

            var gotConfigurationFromJson =
                TryGetConfigurationFromJson
                (
                    Path.Combine(Application.streamingAssetsPath, ConfigFileName),
                    out EndpointSettings parsedConfig
                );

            return
                gotConfigurationFromJson ? parsedConfig : GetDefaultEnvironmentConfig(defaultAuthEnvironment);
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
