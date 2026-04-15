// Copyright 2022-2025 Niantic.

using System;
using NianticSpatial.NSDK.AR.API;
using NianticSpatial.NSDK.AR.Auth.Api;
using NianticSpatial.NSDK.AR.Core;
using NianticSpatial.NSDK.AR.Utilities;
using NianticSpatial.NSDK.AR.Utilities.Logging;

namespace NianticSpatial.NSDK.AR.Auth
{
    /// <summary>
    /// Static client for interacting with the Auth Manager service.
    /// Provides methods to manage authentication tokens and check authorization status.
    /// </summary>
    [PublicAPI]
    public static class AuthClient
    {
        /// <summary>
        /// Sets the access token for authentication with NSDK services.
        /// This token will be used for API Gateway requests instead of the API key.
        /// </summary>
        /// <param name="accessToken">The access token string for authentication.</param>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the NSDK context is not initialized or the operation fails.
        /// </exception>
        public static void SetAccessToken(string accessToken)
        {
            var nsdkHandle = GetNSDKHandle();
            var status = NativeAuthApi.SetAccessToken(nsdkHandle, accessToken);
            if (status != NsdkStatus.Ok)
            {
                throw new InvalidOperationException($"Failed to set access token. Status: {status}");
            }
        }

        /// <summary>
        /// Gets access token authentication information.
        /// Returns authentication information containing information about the current access token.
        /// </summary>
        /// <returns>AuthInfo containing access token claims.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the NSDK context is not initialized or the operation fails.
        /// </exception>
        public static AuthInfo GetAccessAuthInfo()
        {
            var nsdkHandle = GetNSDKHandle();
            var status = NativeAuthApi.GetAccessAuthInfo(nsdkHandle, out var nativeRecord);
            if (status != NsdkStatus.Ok)
            {
                throw new InvalidOperationException($"Failed to get access auth info. Status: {status}");
            }

            try
            {
                return NativeAuthApi.ConvertAuthInfo(nativeRecord);
            }
            finally
            {
                if (nativeRecord.handle != IntPtr.Zero)
                {
                    NsdkExternUtils.ReleaseResource(nativeRecord.handle);
                }
            }
        }

        /// <summary>
        /// Gets refresh token authentication information.
        /// Returns authentication information containing information about the current refresh token.
        /// </summary>
        /// <returns>AuthInfo containing refresh token claims.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the NSDK context is not initialized or the operation fails.
        /// </exception>
        public static AuthInfo GetRefreshAuthInfo()
        {
            var nsdkHandle = GetNSDKHandle();
            var status = NativeAuthApi.GetRefreshAuthInfo(nsdkHandle, out var nativeRecord);
            if (status != NsdkStatus.Ok)
            {
                throw new InvalidOperationException($"Failed to get refresh auth info. Status: {status}");
            }

            try
            {
                return NativeAuthApi.ConvertAuthInfo(nativeRecord);
            }
            finally
            {
                if (nativeRecord.handle != IntPtr.Zero)
                {
                    NsdkExternUtils.ReleaseResource(nativeRecord.handle);
                }
            }
        }

        /// <summary>
        /// Checks if auth tokens are valid and ready for use.
        /// Returns true if a valid, non-expired access token is available.
        /// </summary>
        /// <returns>True if authorized, false otherwise.</returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown if the NSDK context is not initialized or the operation fails.
        /// </exception>
        public static bool IsAuthorized()
        {
            var nsdkHandle = GetNSDKHandle();
            var status = NativeAuthApi.IsAuthorized(nsdkHandle, out bool isAuthorized);
            if (status != NsdkStatus.Ok)
            {
                throw new InvalidOperationException($"Failed to check authorization status. Status: {status}");
            }
            return isAuthorized;
        }

        /// <summary>
        /// Clears cached auth tokens from persistent storage without requiring an NSDK context.
        /// This is the preferred logout path — safe to call before NSDK is initialized or after
        /// it has been destroyed. Any running session will pick up the cleared tokens on its next
        /// reconciliation cycle.
        /// </summary>
        public static void StaticLogout()
        {
            var status = NativeAuthApi.StaticLogout();
            if (status != NsdkStatus.Ok)
            {
                Log.Error($"[Auth] StaticLogout failed with status: {status}");
            }
        }

        private static IntPtr GetNSDKHandle()
        {
            var nsdkHandle = NsdkUnityContext.GetNSDKHandle(NsdkUnityContext.UnityContextHandle);
            if (nsdkHandle == IntPtr.Zero)
            {
                throw new InvalidOperationException("NSDK context is not initialized. Cannot perform auth operation.");
            }
            return nsdkHandle;
        }
    }
}
