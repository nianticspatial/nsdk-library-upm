// Copyright 2026 Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.API;
using NianticSpatial.NSDK.AR.XRSubsystems;
using UnityEngine.XR.ARSubsystems;

namespace NianticSpatial.NSDK.AR.Subsystems.Vps2.Api
{
    // Native access for the VPS2 anchor subsystem. A separate provider from IApi: it
    // only borrows the shared native context and does not own the VPS2 feature's
    // lifecycle, so creating and destroying it never disturbs the main VPS2 subsystem.
    internal interface IAnchorApi
    {
        IntPtr Construct(IntPtr unityContext);

        void Destroy(IntPtr providerHandle);

        NsdkStatus CreateAnchor(IntPtr providerHandle, NsdkTransform pose, ref byte[] anchorId);

        NsdkStatus TrackAnchor(IntPtr providerHandle, NsdkString anchorPayload, ref byte[] anchorId);

        NsdkStatus RemoveAnchor(IntPtr providerHandle, byte[] anchorId);

        NsdkStatus GetAnchorUpdate
        (
            IntPtr providerHandle,
            byte[] anchorId,
            out IApi.NsdkVpsAnchorUpdate updateOut
        );

        NsdkStatus GetAnchorPayload
        (
            IntPtr providerHandle,
            byte[] anchorId,
            out IntPtr anchorPayloadPtr,
            out int anchorPayloadSize,
            out IntPtr resourceHandle
        );

        TrackingState ConvertTrackingStateToUnity(int nativeTrackingState);

        Vps2AnchorTrackingStateReason ConvertTrackingStateReasonToUnity(int nativeReason);
    }
}
