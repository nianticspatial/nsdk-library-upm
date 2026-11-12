// Copyright 2026 Niantic Spatial.

using System;
using System.Collections.Generic;
using NianticSpatial.NSDK.AR.API;
using NianticSpatial.NSDK.AR.Core;
using NianticSpatial.NSDK.AR.Utilities;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using NianticSpatial.NSDK.AR.XRSubsystems;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.XR.ARSubsystems;
using IApi = NianticSpatial.NSDK.AR.Subsystems.Vps2.Api.IApi;
using IAssetApi = NianticSpatial.NSDK.AR.Subsystems.Vps2.Api.IAssetApi;
using AssetNativeApi = NianticSpatial.NSDK.AR.Subsystems.Vps2.Api.AssetNativeApi;

namespace NianticSpatial.NSDK.AR.Subsystems.Vps2
{
    [Preserve]
    public sealed class NsdkVps2AssetSubsystem : XRVps2AssetSubsystem, ISubsystemWithMutableApi<IAssetApi>
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterDescriptor()
        {
            Log.Info(nameof(NsdkVps2AssetSubsystem) + "." + nameof(RegisterDescriptor));
            var cinfo = new XRVps2AssetSubsystemCinfo()
            {
                id = "NSDK-VPS2-Asset",
                providerType = typeof(NsdkProvider),
                subsystemTypeOverride = typeof(NsdkVps2AssetSubsystem),
            };

            UnityEngine.SubsystemsImplementation.SubsystemDescriptorStore.RegisterDescriptor(
                XRVps2AssetSubsystemDescriptor.Create(cinfo));
        }

        internal class NsdkProvider : Provider
        {
            private IAssetApi _api;
            private IntPtr _nativeProviderHandle;

            // Assets whose origin is being tracked: the localized asset lineage plus
            // explicit TryTrackAsset requests. Assets stay registered natively for the
            // session's lifetime, so entries are never removed.
            private readonly Dictionary<TrackableId, TrackedAsset> _trackedAssets = new();

            private class TrackedAsset
            {
                // The asset id string used for the native poll.
                public string AssetId;

                // Latest session-relative data surfaced through GetChanges.
                public XRVps2Asset Latest;

                // Set once the first tracking data has surfaced the asset as added.
                public bool HasData;
            }

            public NsdkProvider() : this(new AssetNativeApi()) { }

            internal NsdkProvider(IAssetApi api)
            {
                _api = api;
#if NIANTICSPATIAL_NSDK_AR_LOADER_ENABLED
                _nativeProviderHandle = _api.Construct(NsdkUnityContext.UnityContextHandle);
#endif
            }

            public void SwitchApiImplementation(IAssetApi api)
            {
                if (_nativeProviderHandle != IntPtr.Zero)
                {
                    _api.Destroy(_nativeProviderHandle);
                }

                _api = api;
                _nativeProviderHandle = api.Construct(NsdkUnityContext.UnityContextHandle);
            }

            // The VPS2 feature's lifecycle is owned by the main VPS2 subsystem; this
            // provider only polls, so starting and stopping is a no-op.
            public override void Start() { }

            public override void Stop() { }

            public override void Destroy()
            {
                if (!_nativeProviderHandle.IsValidHandle())
                {
                    return;
                }

                _api.Destroy(_nativeProviderHandle);
                _nativeProviderHandle = IntPtr.Zero;
            }

            public override bool TryTrackAsset(string assetId, out XRVps2Asset asset)
            {
                if (!Vps2AssetId.TryParse(assetId, out var trackableId))
                {
                    Log.Warning($"Cannot track asset: '{assetId}' is not a valid asset id");
                    asset = XRVps2Asset.defaultValue;
                    return false;
                }

                asset = GetOrAddTrackedAsset(trackableId, assetId).Latest;
                return true;
            }

            public override TrackableChanges<XRVps2Asset> GetChanges(Allocator allocator)
            {
                var added = new List<XRVps2Asset>();
                var updated = new List<XRVps2Asset>();

                if (_nativeProviderHandle.IsValidHandle())
                {
                    TrackLocalizedAsset();

                    foreach (var entry in _trackedAssets)
                    {
                        PollAsset(entry.Key, entry.Value, added, updated);
                    }
                }

                using var addedChanges = new NativeArray<XRVps2Asset>(added.ToArray(), Allocator.Temp);
                using var updatedChanges = new NativeArray<XRVps2Asset>(updated.ToArray(), Allocator.Temp);
                using var removedChanges = new NativeArray<TrackableId>(0, Allocator.Temp);

                return TrackableChanges<XRVps2Asset>.CopyFrom
                (
                    addedChanges,
                    updatedChanges,
                    removedChanges,
                    allocator
                );
            }

            private TrackedAsset GetOrAddTrackedAsset(TrackableId trackableId, string assetId)
            {
                if (!_trackedAssets.TryGetValue(trackableId, out var tracked))
                {
                    tracked = new TrackedAsset
                    {
                        AssetId = assetId,
                        Latest = new XRVps2Asset(trackableId)
                    };
                    _trackedAssets.Add(trackableId, tracked);
                }

                return tracked;
            }

            /// <summary>
            /// Registers the asset behind the latest visual localization, so the localized
            /// asset lineage is tracked without an explicit request.
            /// </summary>
            private void TrackLocalizedAsset()
            {
                if (!_api.GetLatestLocalization(_nativeProviderHandle, out var localization).IsOk())
                {
                    return;
                }

                var assetId = NsdkVps2Subsystem.NsdkProvider.DecodeFixedSizeCString(
                    localization.localizedAsset.assetId);
                if (Vps2AssetId.TryParse(assetId, out var trackableId))
                {
                    GetOrAddTrackedAsset(trackableId, assetId);
                }
            }

            private void PollAsset(
                TrackableId trackableId,
                TrackedAsset tracked,
                List<XRVps2Asset> added,
                List<XRVps2Asset> updated)
            {
                using var managedAssetId = new ManagedNsdkString(tracked.AssetId);
                var nsdkStatus = _api.GetAssetTrackingData
                (
                    _nativeProviderHandle,
                    new[] { managedAssetId.ToNsdkString() },
                    out var dataOut
                );

                switch (nsdkStatus)
                {
                    case NsdkStatus.Ok:
                        SurfaceTrackingData(trackableId, tracked, dataOut, added, updated);
                        break;

                    // No tracking data to serve: tracking is spinning up or lapsed
                    // (InvalidOperation), the asset is not registered with the session
                    // (InvalidArgument), or the VPS2 feature is not up
                    // (FeatureDoesNotExist). Keep polling either way.
                    case NsdkStatus.InvalidOperation:
                    case NsdkStatus.InvalidArgument:
                    case NsdkStatus.FeatureDoesNotExist:
                        DegradeToLimited(trackableId, tracked, updated);
                        break;

                    default:
                        nsdkStatus.ThrowExceptionIfNeeded();
                        break;
                }
            }

            /// <summary>
            /// Surfaces served tracking data: the first data adds the asset as
            /// <c>Tracking</c>, later data updates it.
            /// </summary>
            private static void SurfaceTrackingData(
                TrackableId trackableId,
                TrackedAsset tracked,
                IApi.NsdkVps2AssetTrackingData data,
                List<XRVps2Asset> added,
                List<XRVps2Asset> updated)
            {
                var pose = data.assetToLocalTrackingTransform.FromColumnMajorArray().FromNsdkToUnity();
                Guid.TryParse(
                    NsdkVps2Subsystem.NsdkProvider.DecodeFixedSizeCString(data.asset.siteId),
                    out var siteGuid);

                var asset = new XRVps2Asset
                (
                    trackableId,
                    siteGuid,
                    new Pose(pose.GetPosition(), pose.rotation),
                    TrackingState.Tracking,
                    data.trackingConfidence,
                    data.timestampMs
                );

                if (tracked.HasData)
                {
                    updated.Add(asset);
                }
                else
                {
                    added.Add(asset);
                    tracked.HasData = true;
                }

                tracked.Latest = asset;
            }

            /// <summary>
            /// Degrades an asset that has served data to <c>Limited</c> with its last
            /// known pose; before any data there is nothing to surface.
            /// </summary>
            private static void DegradeToLimited(
                TrackableId trackableId,
                TrackedAsset tracked,
                List<XRVps2Asset> updated)
            {
                if (!tracked.HasData || tracked.Latest.trackingState == TrackingState.Limited)
                {
                    return;
                }

                var asset = new XRVps2Asset
                (
                    trackableId,
                    tracked.Latest.siteGuid,
                    tracked.Latest.pose,
                    TrackingState.Limited,
                    0f,
                    tracked.Latest.timestampMs
                );

                updated.Add(asset);
                tracked.Latest = asset;
            }
        }

        void ISubsystemWithMutableApi<IAssetApi>.SwitchApiImplementation(IAssetApi api)
        {
            ((NsdkProvider)provider).SwitchApiImplementation(api);
        }

        public void SwitchToInternalMockImplementation()
        {
            throw new NotImplementedException();
        }
    }
}
