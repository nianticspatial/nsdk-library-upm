// Copyright Niantic Spatial.

using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.Sites
{
    /// <summary>
    /// A single entry in a site-assets location query result.
    /// </summary>
    [PublicAPI]
    public readonly struct SiteAssetsInfo
    {
        /// <summary>
        /// Site information.
        /// </summary>
        public SiteInfo Site { get; }

        /// <summary>
        /// Assets belonging to this site.
        /// </summary>
        public AssetInfo[] Assets { get; }

        /// <summary>
        /// Distance from the query coordinate to the site, in meters.
        /// </summary>
        public double Distance { get; }

        internal SiteAssetsInfo(SiteInfo site, AssetInfo[] assets, double distance)
        {
            Site = site;
            Assets = assets;
            Distance = distance;
        }
    }
}
