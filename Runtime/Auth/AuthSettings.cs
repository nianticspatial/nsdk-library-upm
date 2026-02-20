using UnityEngine;

namespace NianticSpatial.NSDK.AR.Auth
{
    /// <summary>
    /// Interface to all settings related to Auth.
    /// This interface is used by both NsdkSettings and RuntimeNsdkSettings; its purpose is to support auth
    /// code that can be called from both editor and runtime.
    /// </summary>
    internal interface IAuthSettings
    {
        /// <summary>
        /// Get the NSDK API key.
        /// </summary>
        string ApiKey { get; }

        /// <summary>
        /// Get the current auth environment.
        /// </summary>
        AuthEnvironmentType AuthEnvironment { get; }

        /// <summary>
        /// The current valid access token
        /// </summary>
        string AccessToken { get; }

        /// <summary>
        /// Time when the access token expires
        /// </summary>
        int AccessExpiresAt { get; }

        /// <summary>
        /// Token for refreshing the access token (if available)
        /// </summary>
        string RefreshToken { get; }

        /// <summary>
        /// Time when the refresh token expires
        /// </summary>
        int RefreshExpiresAt { get; }

        /// <summary>
        /// If we have a refresh token, the session may have been created outside the runtime and "baked" into the build
        /// as an asset. Or it may have been created at runtime (by logging in while the app is running).
        /// This flag indicates if it is the latter.
        /// </summary>
        bool IsRuntimeLogin { set; get; }

        /// <summary>
        /// Set runtime access token information in one block (which matches how it's generally received)
        /// </summary>
        /// <param name="accessToken">the current valid access token</param>
        /// <param name="accessExpiresAt">time when the access token expires</param>
        /// <param name="refreshToken">token for refreshing the access token (if available)</param>
        /// <param name="refreshExpiresAt">time when the refresh token expires</param>
        void UpdateAccess(string accessToken, int accessExpiresAt, string refreshToken, int refreshExpiresAt);
    }
}
