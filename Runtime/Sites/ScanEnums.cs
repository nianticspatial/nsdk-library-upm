// Copyright 2026 Niantic Spatial.

using NianticSpatial.NSDK.AR.Utilities;

namespace NianticSpatial.NSDK.AR.Sites
{
    /// <summary>
    /// Scan type, as the portal reports it.
    /// </summary>
    [PublicAPI]
    public enum ScanType
    {
        /// <summary>No type reported, or one this SDK does not know.</summary>
        Unspecified = 0,
        /// <summary>A phone capture, single or chunked.</summary>
        V2 = 1,
        /// <summary>A 360-degree video capture.</summary>
        Video360 = 2,
        /// <summary>A series of drone photos.</summary>
        DronePhoto = 3,
        /// <summary>An E57 point cloud capture.</summary>
        E57 = 4,
        /// <summary>A playback (validation) capture.</summary>
        Validation = 10
    }

    /// <summary>
    /// Scan status, as the portal reports it.
    /// </summary>
    [PublicAPI]
    public enum ScanStatus
    {
        /// <summary>No status reported, or one this SDK does not know.</summary>
        Unspecified = 0,
        /// <summary>The scan record exists; nothing is uploaded yet.</summary>
        Initialized = 1,
        /// <summary>Files are uploading.</summary>
        Uploading = 2,
        /// <summary>Submitted to the mapping pipeline.</summary>
        Submitted = 3,
        /// <summary>Accepted by the mapping pipeline.</summary>
        Accepted = 4,
        /// <summary>Rejected by the mapping pipeline.</summary>
        Rejected = 5,
        /// <summary>The client has marked every file uploaded.</summary>
        UploadedClient = 6,
        /// <summary>The server is copying files from source URLs.</summary>
        Transferring = 7,
        /// <summary>Copying files from source URLs failed.</summary>
        TransferFailed = 8
    }
}
