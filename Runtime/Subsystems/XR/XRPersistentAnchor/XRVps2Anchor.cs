// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Runtime.InteropServices;
using NianticSpatial.NSDK.AR.Utilities;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace NianticSpatial.NSDK.AR.XRSubsystems
{
    /// <summary>
    /// Describes session-relative data for an anchor.
    /// </summary>
    /// <seealso cref="XRVps2Anchor"/>
    [PublicAPI]
    [StructLayout(LayoutKind.Sequential)]
    public struct XRVps2Anchor : ITrackable, IEquatable<XRVps2Anchor>
    {
        /// <summary>
        /// Gets a default-initialized <see cref="XRVps2Anchor"/>. This may be
        /// different from the zero-initialized version (for example, the <see cref="pose"/>
        /// is <c>Pose.identity</c> instead of zero-initialized).
        /// </summary>
        public static XRVps2Anchor defaultValue => s_Default;

        private static readonly XRVps2Anchor s_Default = new XRVps2Anchor
        {
            m_Id = TrackableId.invalidId,
            m_Pose = Pose.identity,
            m_TimestampMs = 0
        };

        /// <summary>
        /// Constructs the session-relative data for an anchor.
        /// This is typically provided by an implementation of the <see cref="XRVps2Anchor"/>
        /// and not invoked directly.
        /// </summary>
        /// <param name="trackableId">The <see cref="TrackableId"/> associated with this anchor.</param>
        /// <param name="pose">The <c>Pose</c>, in session space, of the anchor.</param>
        /// <param name="trackingState">The <see cref="TrackingState"/> of the anchor.</param>
        /// <param name="trackingStateReason">The reason for the current tracking state.</param>
        /// <param name="trackingConfidence">Positive number representing confidence we have in latest tracking update.</param>
        /// <param name="xrVps2AnchorPayload">The payload associated with the anchor.</param>
        public XRVps2Anchor(
            TrackableId trackableId,
            Pose pose,
            TrackingState trackingState,
            Vps2AnchorTrackingStateReason trackingStateReason,
            XRVps2AnchorPayload xrVps2AnchorPayload,
            UInt64 timestampMs,
            float trackingConfidence = 0.0f,
            XRGeolocation? geolocation = null)
        {
            m_Id = trackableId;
            m_Pose = pose;
            m_TrackingState = trackingState;
            m_TrackingStateReason = trackingStateReason;
            m_TrackingConfidence = trackingConfidence;
            m_XRVps2AnchorPayload = xrVps2AnchorPayload;
            m_TimestampMs = timestampMs;
            m_Geolocation = geolocation;
        }

        public XRVps2Anchor(
            TrackableId trackableId)
        {
            m_Id = trackableId;
            m_Pose = Pose.identity;
            m_TrackingState = TrackingState.None;
            m_TrackingStateReason = Vps2AnchorTrackingStateReason.None;
            m_TrackingConfidence = 0.0f;
            m_XRVps2AnchorPayload = new XRVps2AnchorPayload(new IntPtr(0), 0);
            m_TimestampMs = 0;
            m_Geolocation = null;
        }

        public XRVps2Anchor(TrackableId trackableId, bool initializing)
        {
            m_Id = trackableId;
            m_Pose = Pose.identity;
            m_TrackingState = TrackingState.None;
            m_TrackingStateReason = initializing ? Vps2AnchorTrackingStateReason.Initializing : Vps2AnchorTrackingStateReason.None;
            m_TrackingConfidence = 0.0f;
            m_XRVps2AnchorPayload = new XRVps2AnchorPayload(new IntPtr(0), 0);
            m_TimestampMs = 0;
            m_Geolocation = null;
        }

        /// <summary>
        /// Get the <see cref="TrackableId"/> associated with this anchor.
        /// </summary>
        public readonly TrackableId trackableId => m_Id;

        /// <summary>
        /// Get the <c>Pose</c>, in session space, for this anchor.
        /// </summary>
        public readonly Pose pose => m_Pose;

        /// <summary>
        /// Get the <see cref="TrackingState"/> of this anchor.
        /// </summary>
        public readonly TrackingState trackingState => m_TrackingState;

        /// <summary>
        /// Get the <see cref="trackingStateReason"/> of this anchor.
        /// </summary>
        public readonly Vps2AnchorTrackingStateReason trackingStateReason => m_TrackingStateReason;

        /// <summary>
        /// Get the <see cref="trackingConfidence"/> of this anchor.
        /// </summary>
        public readonly float trackingConfidence => m_TrackingConfidence;

        public readonly XRGeolocation? geolocation => m_Geolocation;

        internal XRVps2AnchorPayload anchorPayload => m_XRVps2AnchorPayload;

        /// <summary>
        /// Get the timestamp in miliseconds of the latest update for this anchor.
        /// The timestamp has the same base as the frame.
        /// </summary>
        public readonly UInt64 timestampMs => m_TimestampMs;

        /// <summary>
        /// A native pointer associated with the anchor.
        /// The data pointed to by this pointer is implementation-specific.
        /// </summary>
        public IntPtr nativePtr => m_XRVps2AnchorPayload.nativePtr;

        /// <summary>
        /// Generates a hash suitable for use with containers like `HashSet` and `Dictionary`.
        /// </summary>
        /// <returns>A hash code generated from this object's fields.</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = m_Id.GetHashCode();
                hashCode = hashCode * 486187739 + m_Pose.GetHashCode();
                hashCode = hashCode * 486187739 + ((int)m_TrackingState).GetHashCode();
                hashCode = hashCode * 486187739 + m_TimestampMs.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="other">The other <see cref="XRVps2Anchor"/> to compare against.</param>
        /// <returns>`True` if every field in <paramref name="other"/> is equal to this <see cref="XRVps2Anchor"/>, otherwise false.</returns>
        public bool Equals(XRVps2Anchor other)
        {
            return
                m_Id.Equals(other.m_Id) &&
                m_Pose.Equals(other.m_Pose) &&
                m_TrackingState == other.m_TrackingState &&
                m_XRVps2AnchorPayload == other.m_XRVps2AnchorPayload &&
                m_TimestampMs == other.m_TimestampMs;
        }

        /// <summary>
        /// Tests for equality.
        /// </summary>
        /// <param name="obj">The `object` to compare against.</param>
        /// <returns>`True` if <paramref name="obj"/> is of type <see cref="XRVps2Anchor"/> and
        /// <see cref="Equals(XRVps2Anchor)"/> also returns `true`; otherwise `false`.</returns>
        public override bool Equals(object obj) => obj is XRVps2Anchor && Equals((XRVps2Anchor)obj);

        /// <summary>
        /// Tests for equality. Same as <see cref="Equals(XRVps2Anchor)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator ==(XRVps2Anchor lhs, XRVps2Anchor rhs) => lhs.Equals(rhs);

        /// <summary>
        /// Tests for inequality. Same as `!`<see cref="Equals(XRVps2Anchor)"/>.
        /// </summary>
        /// <param name="lhs">The left-hand side of the comparison.</param>
        /// <param name="rhs">The right-hand side of the comparison.</param>
        /// <returns>`True` if <paramref name="lhs"/> is not equal to <paramref name="rhs"/>, otherwise `false`.</returns>
        public static bool operator !=(XRVps2Anchor lhs, XRVps2Anchor rhs) => !lhs.Equals(rhs);

        private TrackableId m_Id;

        private Pose m_Pose;

        private TrackingState m_TrackingState;

        private Vps2AnchorTrackingStateReason m_TrackingStateReason;

        private float m_TrackingConfidence;

        private XRVps2AnchorPayload m_XRVps2AnchorPayload;

        private UInt64 m_TimestampMs;

        private XRGeolocation? m_Geolocation;
    }
}
