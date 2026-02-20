using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NianticSpatial.NSDK.AR.Loader;
using NianticSpatial.NSDK.AR.Utilities.Auth;
using NianticSpatial.NSDK.AR.Utilities.Http;
using UnityEngine;

namespace NianticSpatial.NSDK.AR.Auth
{
    internal struct AuthGatewayTokens
    {
        public string AccessToken;
        public string RefreshToken;
        public int AccessExpiresAt;
    }

    internal struct AuthRuntimeRefreshToken
    {
        public string RefreshToken;
        public int ExpiresAt;
    }

    /// <summary>
    /// Interface to AuthGatewayAccess class for test mocking
    /// </summary>
    internal interface IAuthGatewayAccess
    {
        /// <summary>
        /// Call to refresh the current runtime access token.
        /// This will normally be called at runtime, but can be called from the editor (say for first-time request).
        /// </summary>
        /// <param name="runtimeRefreshToken">the current runtime refresh token</param>
        /// <returns>updated runtime access and refresh tokens</returns>
        Task<AuthGatewayTokens> RefreshRuntimeAccessAsync(string runtimeRefreshToken);

        /// <summary>
        /// Call to request a runtime refresh token.
        /// Its purpose is to provide a unique runtime refresh token; it can be used by the build to "bake" a token,
        /// Unity play-mode, or at runtime when logging in dynamically.
        /// Note: This is a first-time request, creating a new refresh token (not refreshing an existing one).
        /// A user session refresh token is required to make this request (it is used as an access token here).
        /// </summary>
        /// <param name="userSessionRefreshToken">a valid editor refresh token</param>
        /// <returns>a value runtime refresh token</returns>
        Task<AuthRuntimeRefreshToken> RequestRuntimeRefreshTokenAsync(string userSessionRefreshToken);
    }

    /// <summary>
    /// Class that handles communication with all Auth (Identity and access) communications with the Auth Gateway in
    /// Unity NSDK.
    /// </summary>
    internal class AuthGatewayAccess : IAuthGatewayAccess
    {
        // constants public for test mocking (should not otherwise be used outside of this class)
        public const string GrantTypeRefreshUserSession = "refresh_user_session_access_token";
        public const string GrantTypeExchangeRefresh = "exchange_build_refresh_token";
        public const string GrantTypeRefreshBuildAccess = "refresh_build_access_token";

        public const string RefreshEditorAccessContext = "Refresh of editor access";
        public const string RequestRuntimeRefreshTokenContext = "Request of runtime refresh token";
        public const string RefreshRuntimeAccessContext = "Refresh of runtime access";

        // Disable warnings about naming rules as the following serialized fields are named for server fields
        // ReSharper disable InconsistentNaming

        // All identity requests hit the same end-point (grantType indicates the type of request)
        [Serializable]
        public class Request
        {
            public string grantType;
        }

        [Serializable]
        public class RuntimeRefreshRequest : Request
        {
            public string buildRefreshToken;
        }

        // Response from the refresh endpoint (public for test mocking)
        [Serializable]
        public class RefreshAccessResponse
        {
            public string token;
            public int expiresAt;
        }

        // Response from the refresh endpoint when there is an error
        [Serializable]
        internal class ErrorResponse
        {
            public string error;
        }

        [Serializable]
        public class RequestRuntimeRefreshTokenResponse
        {
            public string buildRefreshToken;
            public int expiresAt;
        }

        // Response from our call to refresh a runtime access token (note that the refresh token is updated too).
        [Serializable]
        public class RuntimeRefreshAccessResponse
        {
            public string buildRefreshToken;
            public string buildAccessToken;
            public int expiresAt;
        }
        // ReSharper restore InconsistentNaming

        // Dependencies:
        private readonly IAuthGatewayUtils _utils;
        private readonly IAuthEnvironment _environment;
        // NsdkSettings objects have complex lifetimes, so safer to get them dynamically:
        private readonly Func<RuntimeNsdkSettings> _getRuntimeNsdkSettings;
        private readonly Func<NsdkSettings> _getNsdkSettings;

        /// <summary>
        /// Constructor is private as this is a singleton
        /// </summary>
        private AuthGatewayAccess(
            IAuthGatewayUtils utils, IAuthEnvironment environment,
            Func<RuntimeNsdkSettings> getRuntimeNsdkSettings,
            Func<NsdkSettings> getNsdkSettings
            )
        {
            _utils = utils;
            _environment = environment;
            _getRuntimeNsdkSettings = getRuntimeNsdkSettings;
            _getNsdkSettings = getNsdkSettings;
        }

        /// <summary>
        /// Create() function for testing (allows mocking of dependencies)
        /// </summary>
        public static AuthGatewayAccess Create(
            IAuthGatewayUtils utils, IAuthEnvironment environment,
            Func<RuntimeNsdkSettings> getRuntimeLightshipSettings,
            Func<NsdkSettings> getLightshipSettings
            )
        {
            return new AuthGatewayAccess(utils, environment, getRuntimeLightshipSettings, getLightshipSettings);
        }

        /// <summary>
        /// Singleton instance of this class
        /// </summary>
        public static IAuthGatewayAccess Instance { get; } = Create(
            AuthGatewayUtils.Instance,
            AuthEnvironment.Instance,
            // ActiveSettings *should* only be set at runtime, but this is unreliable so safer to check:
            () => Application.isPlaying ? NsdkSettingsHelper.ActiveSettings : null,
            () => NsdkSettings.Instance
            );

        public async Task<AuthGatewayTokens> RefreshRuntimeAccessAsync(string runtimeRefreshToken)
        {
            var request = new RuntimeRefreshRequest
            {
                grantType = GrantTypeRefreshBuildAccess,
                buildRefreshToken = runtimeRefreshToken
            };

            var result = await HttpClient.SendPostAsync<Request, RuntimeRefreshAccessResponse>(GetUrl(), request);

            if (result.Status == ResponseStatus.Success)
            {
                return new AuthGatewayTokens
                {
                    AccessToken = result.Data.buildAccessToken,
                    RefreshToken = result.Data.buildRefreshToken,
                    AccessExpiresAt = result.Data.expiresAt
                };
            }

            _utils.LogAnyError(result, RefreshRuntimeAccessContext, runtimeRefreshToken);

            // On failure, return an empty struct
            return new AuthGatewayTokens();
        }

        public async Task<AuthRuntimeRefreshToken> RequestRuntimeRefreshTokenAsync(string userSessionRefreshToken)
        {
            var headers = new Dictionary<string, string>
            {
                { "Cookie", $"{AuthConstants.RefreshTokenCookieName}={userSessionRefreshToken}" }
            };

            var request = new Request { grantType = GrantTypeExchangeRefresh };

            var result = await HttpClient.SendPostAsync<Request, RequestRuntimeRefreshTokenResponse>(
                GetUrl(), request, headers);

            if (result.Status == ResponseStatus.Success)
            {
                return new AuthRuntimeRefreshToken
                {
                    RefreshToken = result.Data.buildRefreshToken,
                    ExpiresAt = result.Data.expiresAt
                };
            }

            _utils.LogAnyError(result, RequestRuntimeRefreshTokenContext, userSessionRefreshToken);

            return new AuthRuntimeRefreshToken();
        }

        // Public for unit testing only
        public string GetUrl()
        {
            var runtimeSettings = _getRuntimeNsdkSettings();

            // If runtime settings unavailable, fall back to a hardcoded endpoint
            return runtimeSettings != null
                ? runtimeSettings.EndpointSettings.IdentityEndpoint
                : _environment.GetIdentityEndpoint(_getNsdkSettings().AuthEnvironment);
        }
    }
}
