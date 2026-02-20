// Copyright Niantic Spatial.

using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.Sites
{
    /// <summary>
    /// Information about a user.
    /// </summary>
    [PublicAPI]
    public readonly struct UserInfo
    {
        /// <summary>
        /// The unique identifier for the user.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The user's first name.
        /// </summary>
        public string FirstName { get; }

        /// <summary>
        /// The user's last name.
        /// </summary>
        public string LastName { get; }

        /// <summary>
        /// The user's email address.
        /// </summary>
        public string Email { get; }

        /// <summary>
        /// The user's status.
        /// </summary>
        public string Status { get; }

        /// <summary>
        /// The Unix timestamp (in seconds) when the user was created.
        /// </summary>
        public long CreatedTimestamp { get; }

        /// <summary>
        /// The organization ID associated with the user, or null if not associated.
        /// </summary>
        public string OrganizationId { get; }

        public UserInfo(
            string id,
            string firstName,
            string lastName,
            string email,
            string status,
            long createdTimestamp,
            string organizationId)
        {
            Id = id;
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Status = status;
            CreatedTimestamp = createdTimestamp;
            OrganizationId = organizationId;
        }
    }
}
