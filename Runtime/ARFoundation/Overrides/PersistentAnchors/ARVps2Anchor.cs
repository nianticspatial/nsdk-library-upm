// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Collections.Generic;
using System.Linq;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using NianticSpatial.NSDK.AR.Common;
using NianticSpatial.NSDK.AR.Utilities;
using NianticSpatial.NSDK.AR.XRSubsystems;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

namespace NianticSpatial.NSDK.AR.VPS2
{
    /// <summary>
    /// Represents a Persistent Anchor tracked by an XR device.
    /// </summary>
    /// <remarks>
    /// Persistent anchors are persistent <c>Pose</c>s in the world that are generated
    /// by processed scans, and will be in the same real world location in future sessions.
    /// By placing virtual content relative to a Persistent Anchor, it can be restored to the same
    /// real world location in a future session.
    /// </remarks>
    [PublicAPI("apiref/Niantic/Lightship/AR/VPS2/ARVps2Anchor/")]
    [DefaultExecutionOrder(NsdkARUpdateOrder.Vps2Anchor)]
    [DisallowMultipleComponent]
    public sealed class ARVps2Anchor : ARTrackable<XRVps2Anchor, ARVps2Anchor>
    {
        /// <summary>
        /// The Persistent Anchor's current tracking state.
        /// A Persistent Anchor should only be used to display virtual content if its tracking state
        ///     is Tracking or Limited. Otherwise, the pose of the Persistent Anchor is not guaranteed
        ///     to be in the correct real world location
        /// </summary>
        public new TrackingState trackingState => trackingStateOverride ?? sessionRelativeData.trackingState;

        /// <summary>
        /// The reason for the Persistent Anchor's current tracking state
        /// </summary>
        public Vps2AnchorTrackingStateReason trackingStateReason => trackingStateReasonOverride ?? sessionRelativeData.trackingStateReason;

        /// <summary>
        /// Positive number representing confidence we have in latest tracking update.
        /// </summary>
        public float trackingConfidence => trackingConfidenceOverride ?? sessionRelativeData.trackingConfidence;

        /// <summary>
        /// The last predicted pose for this Persistent Anchor. This is used as a source of truth in
        ///     case the GameObject's pose is manipulated by interpolation or other features.
        /// </summary>
        public Pose PredictedPose { get; internal set; } = Pose.identity;

        /// <summary>
        /// The timestamp in miliseconds corresponding to the last predicted pose for this Persistent Anchor(PredictedPose).
        /// The timestamp is in the same base as the frame.
        /// </summary>
        public UInt64 TimestampMs => sessionRelativeData.timestampMs;

        /// <summary>
        /// The geolocation of this Persistent Anchor, if available.
        /// </summary>
        public XRGeolocation? geolocation => sessionRelativeData.geolocation;

        internal TrackingState? trackingStateOverride { get; set; }
        internal Vps2AnchorTrackingStateReason? trackingStateReasonOverride { get; set; }
        internal float? trackingConfidenceOverride { get; set; }

        internal bool _markedForDestruction = false;

        internal XRVps2Anchor SessionRelativeData => sessionRelativeData;

        private bool _wasntTrackingLastInterpolationApplication = false;

        /// <summary>
        /// The payload for this anchor as bytes[]
        /// This is an expensive call!
        /// </summary>
        public byte[] GetDataAsBytes()
        {
            return sessionRelativeData.anchorPayload.GetDataAsBytes();
        }

        private void Awake()
        {
            cachedTransform = transform;
        }

        private new void OnDestroy()
        {
            base.OnDestroy();
        }

        #region Temporal Fusion
        // Defines the number of poses to hold for temporal fusion
        internal static int FusionSlidingWindowSize = 5;
        private List<Pose> predictedPoses = new List<Pose>();
        private Transform cachedTransform;

        internal Pose FusedPose { get; private set; }

        internal void ClearTemporalFusion()
        {
            predictedPoses.Clear();
            // Just hold the current pose as the fused pose
            FusedPose = TransformToPose();
        }

        internal Pose UpdateAndApplyTemporalFusion(Pose pose)
        {
            while (predictedPoses.Count >= FusionSlidingWindowSize)
            {
                predictedPoses.RemoveAt(0);
            }

            predictedPoses.Add(pose);

            FusedPose = GetAveragePose(predictedPoses);
            ApplyPoseToTransform(FusedPose);
            return FusedPose;
        }

        // Currently this uses the latest known rotation until proper Quaternion averaging is implemented
        private Pose GetAveragePose(List<Pose> poses)
        {
            if (poses.Count == 0)
            {
                return Pose.identity;
            }

            if (poses.Count == 1)
            {
                return poses[0];
            }

            Vector3 pos = Vector3.zero;
            foreach (var pose in poses)
            {
                pos += pose.position;
            }

            pos /= poses.Count;
            // Currently this uses the latest known rotation until proper Quaternion averaging is implemented
            var rot = poses.Last().rotation;
            return new Pose(pos, rot);
        }

        internal void ApplyPoseToTransform(Pose pose)
        {
            cachedTransform.position = pose.position;
            cachedTransform.rotation = pose.rotation;
        }

        private Pose TransformToPose()
        {
            return new Pose(cachedTransform.position, cachedTransform.rotation);
        }
        #endregion
    }
}
