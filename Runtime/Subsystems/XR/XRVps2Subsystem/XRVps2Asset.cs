// Copyright 2026 Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.Utilities;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace NianticSpatial.NSDK.AR.XRSubsystems
{
    /// <summary>
    /// Describes session-relative data for a VPS asset's origin.
    /// </summary>
    /// <remarks>
    /// The trackable id is the asset's UUID (see <see cref="Vps2AssetId"/>), and
    /// <see cref="siteGuid"/> identifies the site the asset belongs to. The pose is the
    /// asset's origin in AR session space. It is valid while the tracking state is
    /// <c>Tracking</c>; while <c>Limited</c>, the last known pose is kept.
    /// </remarks>
    [PublicAPI]
    public struct XRVps2Asset : ITrackable, IEquatable<XRVps2Asset>
    {
        /// <summary>
        /// Gets a default-initialized <see cref="XRVps2Asset"/>. This may be different
        /// from the zero-initialized version (for example, the <see cref="pose"/> is
        /// <c>Pose.identity</c> instead of zero-initialized).
        /// </summary>
        public static XRVps2Asset defaultValue => s_Default;

        private static readonly XRVps2Asset s_Default = new XRVps2Asset
        {
            m_Id = TrackableId.invalidId,
            m_Pose = Pose.identity
        };

        /// <summary>
        /// Constructs the session-relative data for an asset whose origin is not tracked
        /// yet: identity pose and <c>TrackingState.None</c>.
        /// </summary>
        /// <param name="trackableId">The asset's UUID as a <see cref="TrackableId"/>.</param>
        public XRVps2Asset(TrackableId trackableId)
        {
            m_Id = trackableId;
            m_SiteGuid = Guid.Empty;
            m_Pose = Pose.identity;
            m_TrackingState = TrackingState.None;
            m_TrackingConfidence = 0f;
            m_TimestampMs = 0;
        }

        /// <summary>
        /// Constructs the session-relative data for an asset. This is typically provided
        /// by the <see cref="XRVps2AssetSubsystem"/> implementation and not invoked directly.
        /// </summary>
        /// <param name="trackableId">The asset's UUID as a <see cref="TrackableId"/>.</param>
        /// <param name="siteGuid">The UUID of the site the asset belongs to.</param>
        /// <param name="pose">The pose, in AR session space, of the asset's origin.</param>
        /// <param name="trackingState">The <see cref="TrackingState"/> of the asset.</param>
        /// <param name="trackingConfidence">Confidence in the pose, from 0 (none) to 1 (full).</param>
        /// <param name="timestampMs">Timestamp of the camera frame the pose was last updated
        /// from, in monotonic milliseconds.</param>
        public XRVps2Asset(
            TrackableId trackableId,
            Guid siteGuid,
            Pose pose,
            TrackingState trackingState,
            float trackingConfidence,
            UInt64 timestampMs)
        {
            m_Id = trackableId;
            m_SiteGuid = siteGuid;
            m_Pose = pose;
            m_TrackingState = trackingState;
            m_TrackingConfidence = trackingConfidence;
            m_TimestampMs = timestampMs;
        }

        /// <summary>
        /// Get the <see cref="TrackableId"/> associated with this asset: its UUID,
        /// convertible back with <see cref="Vps2AssetId.ToUuidString"/>.
        /// </summary>
        public readonly TrackableId trackableId => m_Id;

        /// <summary>
        /// Get the UUID of the site the asset belongs to. <c>Guid.Empty</c> until the
        /// first tracking data arrives.
        /// </summary>
        public readonly Guid siteGuid => m_SiteGuid;

        /// <summary>
        /// Get the <c>Pose</c>, in AR session space, of the asset's origin, with position in meters.
        /// </summary>
        public readonly Pose pose => m_Pose;

        /// <summary>
        /// Get the <see cref="TrackingState"/> of this asset.
        /// </summary>
        public readonly TrackingState trackingState => m_TrackingState;

        /// <summary>
        /// Get the confidence in the pose, from 0 (none) to 1 (full).
        /// </summary>
        public readonly float trackingConfidence => m_TrackingConfidence;

        /// <summary>
        /// Get the timestamp of the camera frame the pose was last updated from, in
        /// monotonic milliseconds. The timestamp has the same base as the frame.
        /// </summary>
        public readonly UInt64 timestampMs => m_TimestampMs;

        /// <summary>
        /// Not used by this subsystem; always <c>IntPtr.Zero</c>.
        /// </summary>
        public readonly IntPtr nativePtr => IntPtr.Zero;

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = m_Id.GetHashCode();
                hashCode = hashCode * 486187739 + m_SiteGuid.GetHashCode();
                hashCode = hashCode * 486187739 + m_Pose.GetHashCode();
                hashCode = hashCode * 486187739 + ((int)m_TrackingState).GetHashCode();
                hashCode = hashCode * 486187739 + m_TrackingConfidence.GetHashCode();
                hashCode = hashCode * 486187739 + m_TimestampMs.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="XRVps2Asset"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this
        /// <see cref="XRVps2Asset"/>, otherwise false.</returns>
        public bool Equals(XRVps2Asset other)
        {
            return
                m_Id.Equals(other.m_Id) &&
                m_SiteGuid.Equals(other.m_SiteGuid) &&
                m_Pose.Equals(other.m_Pose) &&
                m_TrackingState == other.m_TrackingState &&
                m_TrackingConfidence.Equals(other.m_TrackingConfidence) &&
                m_TimestampMs == other.m_TimestampMs;
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="XRVps2Asset"/> and
        /// <see cref="Equals(XRVps2Asset)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(object obj) => obj is XRVps2Asset other && Equals(other);

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XRVps2Asset)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(XRVps2Asset lhs, XRVps2Asset rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XRVps2Asset)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(XRVps2Asset lhs, XRVps2Asset rhs) => !lhs.Equals(rhs);

        private TrackableId m_Id;

        private Guid m_SiteGuid;

        private Pose m_Pose;

        private TrackingState m_TrackingState;

        private float m_TrackingConfidence;

        private UInt64 m_TimestampMs;
    }

    /// <summary>
    /// Conversions between VPS asset UUID strings and <see cref="TrackableId"/>s.
    /// </summary>
    /// <remarks>
    /// Asset and site identifiers are canonical 36-character UUID strings. An asset's
    /// UUID converts losslessly to and from the 128-bit <see cref="TrackableId"/> of its
    /// <see cref="XRVps2Asset"/>.
    /// </remarks>
    [PublicAPI]
    public static class Vps2AssetId
    {
        /// <summary>
        /// Parses a canonical UUID string into a <see cref="TrackableId"/>.
        /// </summary>
        /// <param name="uuid">The UUID string, for example an asset id.</param>
        /// <param name="trackableId">Receives the parsed id on success.</param>
        /// <returns>True if <paramref name="uuid"/> is a canonical UUID string.</returns>
        public static bool TryParse(string uuid, out TrackableId trackableId)
        {
            if (Guid.TryParseExact(uuid, "D", out var guid))
            {
                trackableId = FromGuid(guid);
                return true;
            }

            trackableId = TrackableId.invalidId;
            return false;
        }

        /// <summary>
        /// Converts a <see cref="Guid"/> to a <see cref="TrackableId"/>.
        /// </summary>
        /// <param name="guid">The UUID to convert.</param>
        /// <returns>The equivalent <see cref="TrackableId"/>.</returns>
        public static TrackableId FromGuid(Guid guid)
        {
            var bytes = guid.ToByteArray();
            return new TrackableId(BitConverter.ToUInt64(bytes, 0), BitConverter.ToUInt64(bytes, 8));
        }

        /// <summary>
        /// Converts a <see cref="TrackableId"/> back to the <see cref="Guid"/> it was
        /// created from.
        /// </summary>
        /// <param name="trackableId">The id to convert.</param>
        /// <returns>The equivalent <see cref="Guid"/>.</returns>
        public static Guid ToGuid(TrackableId trackableId)
        {
            var bytes = new byte[16];
            BitConverter.GetBytes(trackableId.subId1).CopyTo(bytes, 0);
            BitConverter.GetBytes(trackableId.subId2).CopyTo(bytes, 8);
            return new Guid(bytes);
        }

        /// <summary>
        /// Formats a <see cref="TrackableId"/> as the canonical UUID string it was
        /// created from.
        /// </summary>
        /// <param name="trackableId">The id to format.</param>
        /// <returns>The canonical 36-character UUID string.</returns>
        public static string ToUuidString(TrackableId trackableId) => ToGuid(trackableId).ToString("D");
    }
}
