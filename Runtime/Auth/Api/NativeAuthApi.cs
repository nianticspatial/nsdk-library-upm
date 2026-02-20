// Copyright 2022-2025 Niantic.

using System;
using System.Runtime.InteropServices;
using System.Text;
using NianticSpatial.NSDK.AR.API;
using NianticSpatial.NSDK.AR.Auth;
using NianticSpatial.NSDK.AR.Core;

namespace NianticSpatial.NSDK.AR.Auth.Api
{
    /// <summary>
    /// Native API bindings for Auth Manager.
    /// </summary>
    internal static class NativeAuthApi
    {
        // ============================================================================
        // Marshaled structs matching C API types
        // ============================================================================

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeAuthInfo
        {
            public NsdkString token;
            public int expiration_time;
            public int issued_at_time;
            public NsdkString user_id;
            public NsdkString subject;
            public NsdkString name;
            public NsdkString email;
            public NsdkString issuer;
            public NsdkString audience;
            public IntPtr handle;  // ARDK_ResourceHandle
        }

        // ============================================================================
        // Raw P/Invoke declarations
        // ============================================================================

        [DllImport(NsdkPlugin.Name)]
        private static extern NsdkStatus ARDK_AuthManager_SetAccessToken(
            IntPtr nsdk_handle,
            NsdkString access_token);

        [DllImport(NsdkPlugin.Name)]
        private static extern NsdkStatus ARDK_AuthManager_SetRefreshToken(
            IntPtr nsdk_handle,
            NsdkString refresh_token);

        [DllImport(NsdkPlugin.Name)]
        private static extern NsdkStatus ARDK_AuthManager_GetAccessAuthInfo(
            IntPtr nsdk_handle,
            out NativeAuthInfo auth_info_out);

        [DllImport(NsdkPlugin.Name)]
        private static extern NsdkStatus ARDK_AuthManager_GetRefreshAuthInfo(
            IntPtr nsdk_handle,
            out NativeAuthInfo auth_info_out);


        [DllImport(NsdkPlugin.Name)]
        private static extern NsdkStatus ARDK_AuthManager_IsAuthorized(
            IntPtr nsdk_handle,
            out bool is_authorized_out);

        // ============================================================================
        // Wrapper methods that handle string marshaling
        // ============================================================================

        /// <summary>
        /// Sets the access token for authentication with Lightship services.
        /// </summary>
        /// <param name="nsdkHandle">Handle to the NSDK object.</param>
        /// <param name="accessToken">The access token string for authentication.</param>
        /// <returns>Status indicating success or failure.</returns>
        internal static NsdkStatus SetAccessToken(IntPtr nsdkHandle, string accessToken)
        {
            using (var managedString = new ManagedNsdkString(accessToken))
            {
                return ARDK_AuthManager_SetAccessToken(nsdkHandle, managedString.ToNsdkString());
            }
        }

        /// <summary>
        /// Sets the refresh token so native can refresh access tokens as needed.
        /// </summary>
        /// <param name="nsdkHandle">Handle to the NSDK object.</param>
        /// <param name="refreshToken">The refresh token string.</param>
        /// <returns>Status indicating success or failure.</returns>
        internal static NsdkStatus SetRefreshToken(IntPtr nsdkHandle, string refreshToken)
        {
            using (var managedString = new ManagedNsdkString(refreshToken))
            {
                return ARDK_AuthManager_SetRefreshToken(nsdkHandle, managedString.ToNsdkString());
            }
        }

        /// <summary>
        /// Gets access token authentication information.
        /// </summary>
        /// <param name="nsdkHandle">Handle to the NSDK object.</param>
        /// <param name="authInfo">Output parameter for the access token claims.</param>
        /// <returns>Status indicating success or failure.</returns>
        internal static NsdkStatus GetAccessAuthInfo(IntPtr nsdkHandle, out NativeAuthInfo authInfo)
        {
            var status = ARDK_AuthManager_GetAccessAuthInfo(nsdkHandle, out authInfo);
            return status;
        }

        /// <summary>
        /// Gets refresh token authentication information.
        /// </summary>
        /// <param name="nsdkHandle">Handle to the NSDK object.</param>
        /// <param name="authInfo">Output parameter for the refresh token claims.</param>
        /// <returns>Status indicating success or failure.</returns>
        internal static NsdkStatus GetRefreshAuthInfo(IntPtr nsdkHandle, out NativeAuthInfo authInfo)
        {
            var status = ARDK_AuthManager_GetRefreshAuthInfo(nsdkHandle, out authInfo);
            return status;
        }


        /// <summary>
        /// Checks if auth tokens are valid and ready for use.
        /// </summary>
        /// <param name="nsdkHandle">Handle to the NSDK object.</param>
        /// <param name="isAuthorized">Output parameter set to true if authorized, false otherwise.</param>
        /// <returns>Status indicating success or failure.</returns>
        internal static NsdkStatus IsAuthorized(IntPtr nsdkHandle, out bool isAuthorized)
        {
            return ARDK_AuthManager_IsAuthorized(nsdkHandle, out isAuthorized);
        }

        // ============================================================================
        // Conversion helpers
        // ============================================================================

        /// <summary>
        /// Converts a NativeAuthInfo to a managed AuthInfo.
        /// </summary>
        internal static AuthInfo ConvertAuthInfo(NativeAuthInfo native)
        {
            return new AuthInfo(
                token: NsdkStringToString(native.token),
                expirationTime: native.expiration_time,
                issuedAtTime: native.issued_at_time,
                userId: NsdkStringToString(native.user_id),
                subject: NsdkStringToString(native.subject),
                name: NsdkStringToString(native.name),
                email: NsdkStringToString(native.email),
                issuer: NsdkStringToString(native.issuer),
                audience: NsdkStringToString(native.audience)
            );
        }

        /// <summary>
        /// Converts an NsdkString to a C# string.
        /// </summary>
        private static string NsdkStringToString(NsdkString nsdkStr)
        {
            if (nsdkStr.data == IntPtr.Zero || nsdkStr.length == 0)
            {
                return string.Empty;
            }

            byte[] bytes = new byte[nsdkStr.length];
            Marshal.Copy(nsdkStr.data, bytes, 0, (int)nsdkStr.length);
            return Encoding.UTF8.GetString(bytes);
        }
    }
}
