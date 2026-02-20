// Copyright Niantic Spatial.

using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.Sites
{
    /// <summary>
    /// Information about a site.
    /// </summary>
    [PublicAPI]
    public readonly struct SiteInfo
    {
        /// <summary>
        /// The unique identifier for the site.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The site's name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The site's status.
        /// </summary>
        public string Status { get; }

        /// <summary>
        /// The organization ID this site belongs to.
        /// </summary>
        public string OrganizationId { get; }

        /// <summary>
        /// The latitude of the site's location.
        /// Only valid if <see cref="HasLocation"/> is true.
        /// </summary>
        public double Latitude { get; }

        /// <summary>
        /// The longitude of the site's location.
        /// Only valid if <see cref="HasLocation"/> is true.
        /// </summary>
        public double Longitude { get; }

        /// <summary>
        /// Whether the site has a valid location (latitude/longitude).
        /// </summary>
        public bool HasLocation { get; }

        /// <summary>
        /// The parent site ID, or null if this is a top-level site.
        /// </summary>
        public string ParentSiteId { get; }

        public SiteInfo(
            string id,
            string name,
            string status,
            string organizationId,
            double latitude,
            double longitude,
            bool hasLocation,
            string parentSiteId)
        {
            Id = id;
            Name = name;
            Status = status;
            OrganizationId = organizationId;
            Latitude = latitude;
            Longitude = longitude;
            HasLocation = hasLocation;
            ParentSiteId = parentSiteId;
        }
    }
}
