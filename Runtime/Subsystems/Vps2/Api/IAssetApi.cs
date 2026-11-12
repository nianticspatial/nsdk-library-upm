// Copyright 2026 Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.API;

namespace NianticSpatial.NSDK.AR.Subsystems.Vps2.Api
{
    // Native access for the VPS2 asset subsystem. A separate provider from IApi: it only
    // borrows the shared native context for polling and does not own the VPS2 feature's
    // lifecycle, so creating and destroying it never disturbs the main VPS2 subsystem.
    internal interface IAssetApi
    {
        IntPtr Construct(IntPtr unityContext);

        void Destroy(IntPtr providerHandle);

        NsdkStatus GetLatestLocalization(IntPtr providerHandle, out IApi.NsdkVps2Localization localization);

        // assetId is a single-element array naming an asset (marshalled as an optional
        // pointer); the asset subsystem always polls by explicit id.
        NsdkStatus GetAssetTrackingData
        (
            IntPtr providerHandle,
            NsdkString[] assetId,
            out IApi.NsdkVps2AssetTrackingData trackingData
        );
    }
}
