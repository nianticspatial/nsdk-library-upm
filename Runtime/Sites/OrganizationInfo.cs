// Copyright Niantic Spatial.

using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.Sites
{
    /// <summary>
    /// Information about an organization.
    /// </summary>
    [PublicAPI]
    public readonly struct OrganizationInfo
    {
        /// <summary>
        /// The unique identifier for the organization.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The organization's name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The organization's status.
        /// </summary>
        public string Status { get; }

        /// <summary>
        /// The Unix timestamp (in seconds) when the organization was created.
        /// </summary>
        public long CreatedTimestamp { get; }

        public OrganizationInfo(string id, string name, string status, long createdTimestamp)
        {
            Id = id;
            Name = name;
            Status = status;
            CreatedTimestamp = createdTimestamp;
        }
    }
}
