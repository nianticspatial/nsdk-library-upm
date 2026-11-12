// Copyright 2026 Niantic Spatial.

using System;
using System.Runtime.InteropServices;
using NianticSpatial.NSDK.AR.API;
using NianticSpatial.NSDK.AR.Core;
using NianticSpatial.NSDK.AR.XRSubsystems;
using UnityEngine.XR.ARSubsystems;

namespace NianticSpatial.NSDK.AR.Subsystems.Vps2.Api
{
    internal class AnchorNativeApi : IAnchorApi
    {
        public IntPtr Construct(IntPtr unityContext)
        {
            return Native.Construct(unityContext);
        }

        public void Destroy(IntPtr providerHandle)
        {
            Native.Destroy(providerHandle);
        }

        public NsdkStatus CreateAnchor(IntPtr providerHandle, NsdkTransform pose, ref byte[] anchorId)
        {
            return Native.CreateAnchor(providerHandle, pose, anchorId);
        }

        public NsdkStatus TrackAnchor(IntPtr providerHandle, NsdkString anchorPayload, ref byte[] anchorId)
        {
            return Native.TrackAnchor(providerHandle, anchorPayload, anchorId);
        }

        public NsdkStatus RemoveAnchor(IntPtr providerHandle, byte[] anchorId)
        {
            return Native.RemoveAnchor(providerHandle, anchorId);
        }

        public NsdkStatus GetAnchorUpdate
        (
            IntPtr providerHandle,
            byte[] anchorId,
            out IApi.NsdkVpsAnchorUpdate updateOut
        )
        {
            return Native.GetAnchorUpdate(providerHandle, anchorId, out updateOut);
        }

        public NsdkStatus GetAnchorPayload
        (
            IntPtr providerHandle,
            byte[] anchorId,
            out IntPtr anchorPayloadPtr,
            out int anchorPayloadSize,
            out IntPtr resourceHandle
        )
        {
            return Native.GetAnchorPayload(
                providerHandle, anchorId, out anchorPayloadPtr, out anchorPayloadSize, out resourceHandle);
        }

        // Defined in ardk_vps_tracking_state.h
        /**
        typedef enum ARDK_VPS_AnchorTrackingState : uint8_t {
            /// @brief    The anchor is not being tracked. Find the corresponding
            ///           \c ARDK_VPS_AnchorTrackingStateReason for more information as to why.
            ARDK_VPS_AnchorTrackingState_NotTracked,

            /// @brief    The anchor is being tracked, but VPS has limited confidence in the
            ///           accuracy of the anchor's transform.
            ARDK_VPS_AnchorTrackingState_Limited,

            /// @brief    The anchor is being tracked.
            ARDK_VPS_AnchorTrackingState_Tracked,
        } ARDK_VPS_AnchorTrackingState;
        */
        public TrackingState ConvertTrackingStateToUnity(int nativeTrackingState)
        {
            switch (nativeTrackingState)
            {
                case 0:
                    return TrackingState.None;
                case 1:
                    return TrackingState.Limited;
                case 2:
                    return TrackingState.Tracking;
            }

            throw new ArgumentOutOfRangeException(nameof(nativeTrackingState));
        }

        /**
         typedef enum ARDK_VPS_AnchorTrackingStateReason : uint8_t {
              /// @brief    No reason for the tracking state is available. Anchor tracking
              ///           should be active.
              ARDK_VPS_AnchorTrackingStateReason_None,

              /// @brief    VPS has not yet successfully localized, or the anchor is still
              ///           being initialized.
              ARDK_VPS_AnchorTrackingStateReason_Initializing,

              /// @brief    Tracking has been stopped for this anchor.
              ARDK_VPS_AnchorTrackingStateReason_Removed,

              /// @brief    An internal error has occurred in ARDK.
              ARDK_VPS_AnchorTrackingStateReason_InternalError,

              /// @brief    This anchor is part of a private VPS location that this app does
              ///           not have permission to localize to.
              ARDK_VPS_AnchorTrackingStateReason_PermissionDenied,

              /// @brief    Anchor tracking has failed due to an unrecoverable network error.
              /// @details  See \c ARDK_VPS_GetFeatureStatus for more information.
              ARDK_VPS_AnchorTrackingStateReason_FatalNetworkError
            } ARDK_VPS_AnchorTrackingStateReason;
         */
        public Vps2AnchorTrackingStateReason ConvertTrackingStateReasonToUnity(int nativeReason)
        {
            switch (nativeReason)
            {
                case 0:
                    return Vps2AnchorTrackingStateReason.None;
                case 1:
                    return Vps2AnchorTrackingStateReason.Initializing;
                case 2:
                    return Vps2AnchorTrackingStateReason.Removed;
                case 3:
                    return Vps2AnchorTrackingStateReason.InternalError;
                case 4:
                    return Vps2AnchorTrackingStateReason.PermissionDenied;
                case 5:
                    return Vps2AnchorTrackingStateReason.FatalNetworkError;
                case 6:
                    return Vps2AnchorTrackingStateReason.NoVisualLocalization;
            }

            throw new ArgumentOutOfRangeException(nameof(nativeReason));
        }

        private static class Native
        {
            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_AnchorProvider_Create")]
            public static extern IntPtr Construct(IntPtr unityContext);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_AnchorProvider_Destroy")]
            public static extern void Destroy(IntPtr providerHandle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_AnchorProvider_CreateAnchor")]
            public static extern NsdkStatus CreateAnchor(IntPtr providerHandle, NsdkTransform pose, [Out] byte[] anchorIdOut);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_AnchorProvider_TrackAnchor")]
            public static extern NsdkStatus TrackAnchor
            (
                IntPtr providerHandle,
                NsdkString anchorPayload,
                [Out] byte[] anchorIdOut
            );

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_AnchorProvider_RemoveAnchor")]
            public static extern NsdkStatus RemoveAnchor(IntPtr providerHandle, byte[] anchorId);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_AnchorProvider_GetAnchorUpdate")]
            public static extern NsdkStatus GetAnchorUpdate
            (
                IntPtr providerHandle,
                byte[] anchorId,
                out IApi.NsdkVpsAnchorUpdate updateOut
            );

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_AnchorProvider_GetAnchorPayload")]
            public static extern NsdkStatus GetAnchorPayload
            (
                IntPtr providerHandle,
                byte[] anchorId,
                out IntPtr anchorPayloadPtr,
                out int anchorPayloadSize,
                out IntPtr resourceHandle
            );
        }
    }
}
