// Copyright 2022-2026 Niantic Spatial.

using JetBrains.Annotations;

namespace NianticSpatial.NSDK.AR.Settings
{
    /// <summary>
    /// This class contains all the data required for data management requests.
    /// </summary>
    [PublicAPI]
    public static partial class PrivacyData
    {
        /// <summary>
        /// This is the device Id used to identify any device. In case there is no userId, the clientId can be provided
        /// for your GDPR data requests.
        /// If you are an NSDK developer, clientId is Unity's SystemInfo.deviceUniqueIdentifier.
        ///
        /// For your game users, it is a random Guid. In case of no userId, you have to record it.
        /// It changes if the ios/android app is uninstalled and reinstalled. It remains the same over app upgrades
        /// </summary>
        [PublicAPI]
        public static string ClientId
        {
            get => Metadata.ClientId;
        }
    }
}
