// Copyright Niantic Spatial.

using System;
using System.Runtime.InteropServices;
using NianticSpatial.NSDK.AR.API;
using NianticSpatial.NSDK.AR.Core;
using NianticSpatial.NSDK.AR.XRSubsystems;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace NianticSpatial.NSDK.AR.Subsystems.Vps2.Api
{
    internal class NativeApi : IApi
    {
        public IntPtr Construct(IntPtr unityContext) {
            return Native.Create(unityContext);
        }

        public void Destroy(IntPtr providerHandle)
        {
            Native.Destroy(providerHandle);
        }

        public NsdkStatus Start(IntPtr providerHandle)
        {
            return Native.Start(providerHandle);
        }

        public NsdkStatus Stop(IntPtr providerHandle)
        {
            return Native.Stop(providerHandle);
        }

        public NsdkFeatureStatus GetFeatureStatus(IntPtr providerHandle) {
            return Native.GetFeatureStatus(providerHandle);
        }

        public NsdkStatus Configure(IntPtr providerHandle, IApi.NsdkVps2Config config)
        {
            return Native.Configure(providerHandle, config);
        }

        public NsdkStatus GetLatestLocalization(IntPtr providerHandle, out IApi.NsdkVps2Localization localization)
        {
            return Native.GetLatestLocalization(providerHandle, out localization);
        }

        public NsdkStatus GetDeviceGeolocation(IntPtr providerHandle, HeadingMode headingMode, out IApi.NsdkVps2GeolocationData location)
        {
            return Native.GetDeviceGeolocation(providerHandle, (int)headingMode, out location);
        }

        public NsdkStatus GetPose(IApi.NsdkVps2Localization localization, NsdkGeolocationData location, out IApi.NsdkVps2Pose pose)
        {
            return Native.GetPose(ref localization, ref location, out pose);
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
            return Native.GetAnchorUpdate
            (
                providerHandle,
                anchorId,
                out updateOut
            );
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
            return Native.GetAnchorPayload(providerHandle, anchorId, out anchorPayloadPtr, out anchorPayloadSize, out resourceHandle);
        }

        public NsdkStatus GetLatestLocalizationRequestRecords(IntPtr providerHandle, out IntPtr networkRequestRecords, out int size, out IntPtr handle)
        {
            return Native.GetLatestLocalizationRequestRecords(providerHandle, out networkRequestRecords, out size, out handle);
        }

        public NsdkStatus GetLatestDebuggerLogs(IntPtr providerHandle, out IntPtr logs, out int count,
            out IntPtr handle)
        {
            return Native.GetLatestDebuggerLogs(providerHandle, out logs, out count, out handle);
        }

        public NsdkStatus GetSessionId(IntPtr providerHandle, ref byte[] sessionId)
        {
            return Native.GetSessionId(providerHandle, sessionId);
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
            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_Create")]
            public static extern IntPtr Create(IntPtr unityContext);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_Destroy")]
            public static extern void Destroy(IntPtr providerHandle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_GetFeatureStatus")]
            public static extern NsdkFeatureStatus GetFeatureStatus(IntPtr providerHandle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_Configure")]
            public static extern NsdkStatus Configure(IntPtr providerHandle, IApi.NsdkVps2Config config);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_Start")]
            public static extern NsdkStatus Start(IntPtr providerHandle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_Stop")]
            public static extern NsdkStatus Stop(IntPtr providerHandle);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_GetLatestLocalization")]
            public static extern NsdkStatus GetLatestLocalization(IntPtr providerHandle, out IApi.NsdkVps2Localization localization);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_GetDeviceGeolocation")]
            public static extern NsdkStatus GetDeviceGeolocation(IntPtr providerHandle, int headingMode, out IApi.NsdkVps2GeolocationData location);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_GetPose")]
            public static extern NsdkStatus GetPose(ref IApi.NsdkVps2Localization localization, ref NsdkGeolocationData location, out IApi.NsdkVps2Pose pose);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_CreateAnchor")]
            public static extern NsdkStatus CreateAnchor(IntPtr providerHandle, NsdkTransform pose, [Out] byte[] anchorIdOut);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_TrackAnchor")]
            public static extern NsdkStatus TrackAnchor
            (
                IntPtr providerHandle,
                NsdkString anchorPayload,
                [Out] byte[] anchorIdOut
            );

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_RemoveAnchor")]
            public static extern NsdkStatus RemoveAnchor(IntPtr providerHandle, byte[] anchorId);

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_GetAnchorUpdate")]
            public static extern NsdkStatus GetAnchorUpdate
            (
                IntPtr providerHandle,
                byte[] anchorId,
                out IApi.NsdkVpsAnchorUpdate updateOut
            );

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_GetAnchorPayload")]
            public static extern NsdkStatus GetAnchorPayload
            (
                IntPtr providerHandle,
                byte[] anchorId,
                out IntPtr anchorPayloadPtr,
                out int anchorPayloadSize,
                out IntPtr resourceHandle
            );

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_GetLatestLocalizationRequestRecords")]
            public static extern NsdkStatus GetLatestLocalizationRequestRecords(
                IntPtr providerHandle,
                out IntPtr networkRequestRecords,
                out int count,
                out IntPtr handle
            );

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_GetLatestDebuggerLogs")]
            public static extern NsdkStatus GetLatestDebuggerLogs(
                IntPtr providerHandle,
                out IntPtr logs,
                out int count,
                out IntPtr handle
            );

            [DllImport(NsdkPlugin.Name, EntryPoint = "Lightship_ARDK_Unity_VPS2_Provider_GetSessionId")]
            public static extern NsdkStatus GetSessionId(IntPtr providerHandle, [Out] byte[] sessionId);
        }
    }
}
