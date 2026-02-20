using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NianticSpatial.NSDK.AR.Auth;
using NianticSpatial.NSDK.AR.Utilities.Auth;
using NianticSpatial.NSDK.AR.Utilities.Http;
using UnityEngine.Networking;

namespace NianticSpatial.NSDK.AR.Editor.Auth
{
    /// <summary>
    /// Interface to AuthGatewayAccess class for test mocking
    /// </summary>
    internal interface IAuthEditorGatewayAccess
    {
        /// <summary>
        /// Call to refresh the current access token.
        /// On success, the current refresh token is also updated.
        /// </summary>
        /// <param name="editorRefreshToken">the current refresh token</param>
        /// <returns>updated access and refresh tokens</returns>
        Task<AuthGatewayTokens> RefreshEditorAccessAsync(string editorRefreshToken);
    }

    internal class AuthEditorGatewayAccess : IAuthEditorGatewayAccess
    {
        // constants public for test mocking (should not otherwise be used outside of this class)
        public const string GrantTypeRefreshUserSession = "refresh_user_session_access_token";

        public const string RefreshEditorAccessContext = "Refresh of editor access";

        // Disable warnings about naming rules as the following serialized fields are named for server fields
        // ReSharper disable InconsistentNaming

        // All identity requests hit the same end-point (grantType indicates the type of request)
        [Serializable]
        public class Request
        {
            public string grantType;
        }

        // ReSharper restore InconsistentNaming

        // Dependencies:
        private readonly IAuthGatewayUtils _utils;
        private readonly IAuthEditorSettings _editorSettings;
        private readonly IAuthEnvironment _environment;

        /// <summary>
        /// Constructor is private as this is a singleton
        /// </summary>
        private AuthEditorGatewayAccess(
            IAuthGatewayUtils utils, IAuthEditorSettings editorSettings, IAuthEnvironment environment)
        {
            _utils = utils;
            _editorSettings = editorSettings;
            _environment = environment;
        }

        /// <summary>
        /// Create() function for testing (allows mocking of dependencies)
        /// </summary>
        public static AuthEditorGatewayAccess Create(
            IAuthGatewayUtils utils, IAuthEditorSettings editorSettings, IAuthEnvironment environment)
        {
            return new AuthEditorGatewayAccess(utils, editorSettings, environment);
        }

        /// <summary>
        /// Singleton instance of this class
        /// </summary>
        public static IAuthEditorGatewayAccess Instance { get; } = Create(
            AuthGatewayUtils.Instance, AuthEditorSettings.Instance, AuthEnvironment.Instance);

        public async Task<AuthGatewayTokens> RefreshEditorAccessAsync(string editorRefreshToken)
        {
            var headers = new Dictionary<string, string>
            {
                { "Cookie", $"{AuthConstants.RefreshTokenCookieName}={editorRefreshToken}" }
            };

            // Clear any cached cookies associated with this endpoint. Sometimes UnityWebRequest seems to get stuck and
            // return the cached cookie rather than the new one (we never want that).
            UnityWebRequest.ClearCookieCache(new Uri(GetUrl()));

            var request = new Request { grantType = GrantTypeRefreshUserSession };

            var result = await HttpClient.SendPostAsync<Request, AuthGatewayAccess.RefreshAccessResponse>(
                GetUrl(), request, headers, new[] { "set-cookie" });

            if (result.Status == ResponseStatus.Success)
            {
                result.Headers.TryGetValue("set-cookie", out var cookieHeader);
                var newRefreshToken = HttpUtility.GetHeaderValue(cookieHeader, AuthConstants.RefreshTokenCookieName);
                return new AuthGatewayTokens
                {
                    AccessToken = result.Data.token,
                    RefreshToken = newRefreshToken,
                    AccessExpiresAt = result.Data.expiresAt
                };
            }

            _utils.LogAnyError(result, RefreshEditorAccessContext, editorRefreshToken);

            // On failure, return an empty struct
            return new AuthGatewayTokens();
        }

        private string GetUrl()
        {
            return _environment.GetIdentityEndpoint(_editorSettings.AuthEnvironment);
        }
    }
}
