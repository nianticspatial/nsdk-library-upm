// Copyright Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.XRSubsystems
{
    /// <summary>
    /// The VPS asset the device is localized to.
    /// </summary>
    /// <remarks>
    /// Identifies the asset behind the most recent successful visual localization.
    /// The device is anchored to one asset at a time; this changes when it localizes
    /// against a different asset of the site, or a different area of the same asset.
    /// <see cref="SiteId"/> and <see cref="AssetId"/> name the site's VPS map asset:
    /// <see cref="AssetId"/> is never the id of a mesh, splat, or other non-VPS asset
    /// of the site.
    /// </remarks>
    [PublicAPI]
    public struct Vps2LocalizedAsset : IEquatable<Vps2LocalizedAsset>
    {
        /// <summary>
        /// The identifier of the site the asset belongs to.
        /// </summary>
        public string SiteId;

        /// <summary>
        /// The identifier of the VPS map asset.
        /// </summary>
        public string AssetId;

        /// <summary>
        /// Identifier of subarea in the site localized, or null when the asset has no subareas.
        /// </summary>
        public string AreaId;

        public bool Equals(Vps2LocalizedAsset other)
        {
            return SiteId == other.SiteId && AssetId == other.AssetId && AreaId == other.AreaId;
        }

        public override bool Equals(object obj) => obj is Vps2LocalizedAsset other && Equals(other);

        public override int GetHashCode() => HashCode.Combine(SiteId, AssetId, AreaId);

        public override string ToString() =>
            $"Vps2LocalizedAsset(SiteId={SiteId}, AssetId={AssetId}, AreaId={AreaId ?? "null"})";
    }
}
