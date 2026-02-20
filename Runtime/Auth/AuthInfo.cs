// Copyright 2022-2025 Niantic.

using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.Auth
{
    /// <summary>
    /// Authentication information containing token claims.
    /// Contains parsed JWT claims including token string, expiration, user information, and other standard JWT fields.
    /// </summary>
    [PublicAPI]
    public readonly struct AuthInfo
    {
        /// <summary>
        /// Raw JWT token string; may be empty.
        /// </summary>
        public string Token { get; }

        /// <summary>
        /// Expiration time (seconds since epoch).
        /// </summary>
        public int ExpirationTime { get; }

        /// <summary>
        /// Issued at time (seconds since epoch).
        /// </summary>
        public int IssuedAtTime { get; }

        /// <summary>
        /// User ID claim.
        /// </summary>
        public string UserId { get; }

        /// <summary>
        /// Subject claim.
        /// </summary>
        public string Subject { get; }

        /// <summary>
        /// Name claim.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Email claim.
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// Issuer claim.
        /// </summary>
        public string Issuer { get; }

        /// <summary>
        /// Audience claim.
        /// </summary>
        public string Audience { get; }

        internal AuthInfo(
            string token,
            int expirationTime,
            int issuedAtTime,
            string userId,
            string subject,
            string name,
            string email,
            string issuer,
            string audience)
        {
            Token = token ?? string.Empty;
            ExpirationTime = expirationTime;
            IssuedAtTime = issuedAtTime;
            UserId = userId ?? string.Empty;
            Subject = subject ?? string.Empty;
            Name = name ?? string.Empty;
            Email = email ?? string.Empty;
            Issuer = issuer ?? string.Empty;
            Audience = audience ?? string.Empty;
        }
    }
}
