// Copyright 2026 Niantic Spatial.

using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.Sites
{
    /// <summary>
    /// One scan of a site, as the portal lists it.
    /// </summary>
    [PublicAPI]
    public readonly struct ScanInfo
    {
        /// <summary>
        /// The unique identifier for the scan.
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// The scan's display name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The scan type.
        /// </summary>
        public ScanType ScanType { get; }

        /// <summary>
        /// Where the scan is in upload and processing.
        /// </summary>
        public ScanStatus ScanStatus { get; }

        /// <summary>
        /// Total capture length in seconds.
        /// For a chunked scan this sums every chunk, while opening the scan for playback plays
        /// only the first.
        /// </summary>
        public double DurationSeconds { get; }

        /// <summary>
        /// Earliest capture time in Unix seconds, or 0 if unknown.
        /// </summary>
        public long CaptureTimestamp { get; }

        /// <summary>
        /// Record creation time in Unix seconds, or 0 if unknown.
        /// </summary>
        public long CreatedTimestamp { get; }

        /// <summary>
        /// The latitude of the capture location.
        /// Only valid if <see cref="HasLocation"/> is true.
        /// </summary>
        public double Latitude { get; }

        /// <summary>
        /// The longitude of the capture location.
        /// Only valid if <see cref="HasLocation"/> is true.
        /// </summary>
        public double Longitude { get; }

        /// <summary>
        /// Whether the scan has a capture location.
        /// </summary>
        public bool HasLocation { get; }

        /// <summary>
        /// Total size of the scan's capture files in bytes.
        /// </summary>
        public long TotalSize { get; }

        /// <summary>
        /// Whether the scan has a thumbnail image.
        /// </summary>
        public bool HasThumbnail { get; }

        /// <summary>
        /// Whether the scan has a preview video.
        /// </summary>
        public bool HasPreview { get; }

        /// <summary>
        /// ID of the user who uploaded the scan; empty for a community scan.
        /// </summary>
        public string UserId { get; }

        public ScanInfo(
            string id,
            string name,
            ScanType scanType,
            ScanStatus scanStatus,
            double durationSeconds,
            long captureTimestamp,
            long createdTimestamp,
            double latitude,
            double longitude,
            bool hasLocation,
            long totalSize,
            bool hasThumbnail,
            bool hasPreview,
            string userId)
        {
            Id = id;
            Name = name;
            ScanType = scanType;
            ScanStatus = scanStatus;
            DurationSeconds = durationSeconds;
            CaptureTimestamp = captureTimestamp;
            CreatedTimestamp = createdTimestamp;
            Latitude = latitude;
            Longitude = longitude;
            HasLocation = hasLocation;
            TotalSize = totalSize;
            HasThumbnail = hasThumbnail;
            HasPreview = hasPreview;
            UserId = userId;
        }
    }
}
