// Copyright 2026 Niantic Spatial.

using System;
using System.Collections.Generic;
using System.Text;
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
using IAnchorApi = NianticSpatial.NSDK.AR.Subsystems.Vps2.Api.IAnchorApi;
using AnchorNativeApi = NianticSpatial.NSDK.AR.Subsystems.Vps2.Api.AnchorNativeApi;

namespace NianticSpatial.NSDK.AR.Subsystems.Vps2
{
    [Preserve]
    public sealed class NsdkVps2AnchorSubsystem : XRVps2AnchorSubsystem, ISubsystemWithMutableApi<IAnchorApi>
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterDescriptor()
        {
            Log.Info(nameof(NsdkVps2AnchorSubsystem) + "." + nameof(RegisterDescriptor));
            var cinfo = new XRVps2AnchorSubsystemCinfo()
            {
                id = "NSDK-VPS2-Anchor",
                providerType = typeof(NsdkProvider),
                subsystemTypeOverride = typeof(NsdkVps2AnchorSubsystem),
            };

            UnityEngine.SubsystemsImplementation.SubsystemDescriptorStore.RegisterDescriptor(
                XRVps2AnchorSubsystemDescriptor.Create(cinfo));
        }

        internal class NsdkProvider : Provider
        {
            private IAnchorApi _api;
            private IntPtr _nativeProviderHandle;

            // Anchors known to this provider, keyed by the TrackableId derived from the
            // native anchor id. Entries leave when a Removed update surfaces or all
            // anchors are torn down at once.
            private readonly Dictionary<TrackableId, string> _trackedAnchorIds = new();

            // Anchors surfaced through GetChanges on the previous call: an anchor seen
            // before yields an update, an unseen one an addition.
            private HashSet<TrackableId> _seenAnchorIds = new();

            public NsdkProvider() : this(new AnchorNativeApi()) { }

            internal NsdkProvider(IAnchorApi api)
            {
                _api = api;
#if NIANTICSPATIAL_NSDK_AR_LOADER_ENABLED
                _nativeProviderHandle = _api.Construct(NsdkUnityContext.UnityContextHandle);
#endif
            }

            public void SwitchApiImplementation(IAnchorApi api)
            {
                if (_nativeProviderHandle != IntPtr.Zero)
                {
                    _api.Destroy(_nativeProviderHandle);
                }

                _api = api;
                _nativeProviderHandle = api.Construct(NsdkUnityContext.UnityContextHandle);
            }

            // The VPS2 feature's lifecycle is owned by the main VPS2 subsystem; this
            // provider only serves anchors against it, so starting is a no-op.
            public override void Start() { }

            public override void Stop()
            {
                _trackedAnchorIds.Clear();
                _seenAnchorIds.Clear();
            }

            public override void Destroy()
            {
                if (!_nativeProviderHandle.IsValidHandle())
                {
                    return;
                }

                _api.Destroy(_nativeProviderHandle);
                _nativeProviderHandle = IntPtr.Zero;
            }

            public override bool TryCreateAnchor(Pose pose, out XRVps2Anchor anchor)
            {
                if (!_nativeProviderHandle.IsValidHandle())
                {
                    anchor = new XRVps2Anchor();
                    return false;
                }

                var transform = pose.FromUnityToNsdk();
                var anchorIdOut = new byte[IApi.NSDK_VPS2_ANCHOR_ID_SIZE];
                var nsdkStatus = _api.CreateAnchor(_nativeProviderHandle, transform, ref anchorIdOut);
                if (!nsdkStatus.IsOk())
                {
                    Log.Warning($"Failed to create anchor due to error: {nsdkStatus}");
                    anchor = new XRVps2Anchor();
                    return false;
                }

                anchor = CreateXRVps2Anchor(anchorIdOut);
                return true;
            }

            public override bool TryTrackAnchor(string anchorPayload, out XRVps2Anchor anchor)
            {
                if (!_nativeProviderHandle.IsValidHandle())
                {
                    anchor = new XRVps2Anchor();
                    return false;
                }

                var anchorIdOut = new byte[IApi.NSDK_VPS2_ANCHOR_ID_SIZE];
                using var managedStr = new ManagedNsdkString(anchorPayload);
                var nsdkStr = managedStr.ToNsdkString();

                var nsdkStatus =
                    _api.TrackAnchor(_nativeProviderHandle, nsdkStr, ref anchorIdOut);

                if (!nsdkStatus.IsOk())
                {
                    Log.Warning($"Failed to track anchor due to error: {nsdkStatus}");
                    anchor = new XRVps2Anchor();
                    return false;
                }

                anchor = CreateXRVps2Anchor(anchorIdOut);
                return true;
            }

            public override bool TryRemoveAnchor(TrackableId trackableId)
            {
                if (!_nativeProviderHandle.IsValidHandle())
                {
                    return false;
                }

                string anchorId = trackableId.ToNsdkHexString();
                var anchorIdBuffer = Encoding.UTF8.GetBytes(anchorId, 0, IApi.NSDK_VPS2_ANCHOR_ID_SIZE);
                var nsdkStatus = _api.RemoveAnchor(_nativeProviderHandle, anchorIdBuffer);
                if (nsdkStatus.IsOk())
                {
                    return true;
                }

                Log.Warning($"Failed to remove anchor with id {anchorId} due to error: {nsdkStatus}");
                return false;
            }

            public override void RemoveAllAnchors()
            {
                if (_nativeProviderHandle.IsValidHandle())
                {
                    foreach (var anchorId in _trackedAnchorIds.Values)
                    {
                        var anchorIdBuffer = Encoding.UTF8.GetBytes(anchorId, 0, IApi.NSDK_VPS2_ANCHOR_ID_SIZE);
                        // Failures are expected when the VPS2 feature has already
                        // stopped, which unregisters its anchors natively.
                        _api.RemoveAnchor(_nativeProviderHandle, anchorIdBuffer);
                    }
                }

                _trackedAnchorIds.Clear();
                _seenAnchorIds.Clear();
            }

            public override bool GetAnchorPayload(TrackableId trackableId, out string payload)
            {
                payload = null;

                if (!_nativeProviderHandle.IsValidHandle())
                {
                    return false;
                }

                string anchorId = trackableId.ToNsdkHexString();
                var anchorIdBuffer = Encoding.UTF8.GetBytes(anchorId, 0, IApi.NSDK_VPS2_ANCHOR_ID_SIZE);
                var nsdkStatus =
                    _api.GetAnchorPayload(_nativeProviderHandle, anchorIdBuffer, out var ptr, out var size, out var handle);

                if (nsdkStatus.IsOk())
                {
                    NativeArray<byte> bytes;
                    unsafe
                    {
                        bytes = NativeCopyUtility.PtrToNativeArrayWithDefault<byte>
                            (0, (void*)ptr, sizeof(byte), size, Allocator.Temp);
                    }

                    payload = Encoding.UTF8.GetString(bytes);
                }

                if (handle.IsValidHandle())
                {
                    NsdkExternUtils.ReleaseResource(handle);
                }

                return payload != null;
            }

            public override TrackableChanges<XRVps2Anchor> GetChanges(Allocator allocator)
            {
                var added = new List<XRVps2Anchor>();
                var updated = new List<XRVps2Anchor>();
                var removed = new List<TrackableId>();

                if (_nativeProviderHandle.IsValidHandle())
                {
                    var currentAnchorIds = new HashSet<TrackableId>();

                    foreach (var anchorId in _trackedAnchorIds)
                    {
                        var anchorIdBuffer = Encoding.UTF8.GetBytes(anchorId.Value, 0, IApi.NSDK_VPS2_ANCHOR_ID_SIZE);
                        var status = _api.GetAnchorUpdate(_nativeProviderHandle, anchorIdBuffer, out var updateOut);
                        if (!status.IsOk())
                        {
                            // No update to serve: the anchor is not yet registered
                            // natively (InvalidArgument) or the VPS2 feature is not up
                            // (FeatureDoesNotExist). Keep polling, and keep the anchor's
                            // seen state so updates resume as updated changes rather
                            // than fresh additions.
                            if (_seenAnchorIds.Contains(anchorId.Key))
                            {
                                currentAnchorIds.Add(anchorId.Key);
                            }

                            continue;
                        }

                        var pose = updateOut.pose.FromColumnMajorArray().FromNsdkToUnity();

                        XRGeolocation? geolocation = null;
                        if (updateOut.hasGeolocation != 0)
                        {
                            var orientation = new Quaternion
                            (
                                updateOut.geolocationData.orientation_edn_x,
                                updateOut.geolocationData.orientation_edn_y,
                                updateOut.geolocationData.orientation_edn_z,
                                updateOut.geolocationData.orientation_edn_w
                            );

                            geolocation = new XRGeolocation
                            {
                                Latitude = updateOut.geolocationData.latitude,
                                Longitude = updateOut.geolocationData.longitude,
                                Altitude = updateOut.geolocationData.altitude,
                                Heading = updateOut.geolocationData.heading_edn,
                                OrientationEdn = orientation
                            };
                        }

                        var anchor = new XRVps2Anchor(
                            anchorId.Key,
                            new Pose(pose.GetPosition(), pose.rotation),
                            _api.ConvertTrackingStateToUnity(updateOut.trackingState),
                            _api.ConvertTrackingStateReasonToUnity(updateOut.trackingStateReason),
                            new XRVps2AnchorPayload(),
                            updateOut.timestamp,
                            updateOut.confidence,
                            geolocation
                        );

                        if (anchor.trackingStateReason == Vps2AnchorTrackingStateReason.Removed)
                        {
                            removed.Add(anchorId.Key);
                        }
                        else if (_seenAnchorIds.Contains(anchorId.Key))
                        {
                            // Anchor was seen before, so it's an update
                            updated.Add(anchor);
                            currentAnchorIds.Add(anchorId.Key);
                        }
                        else
                        {
                            // Anchor is new, so it's an addition
                            added.Add(anchor);
                            currentAnchorIds.Add(anchorId.Key);
                        }
                    }

                    // Now that removed change has been surfaced, forget from tracked anchors
                    // so the update isn't fetched again
                    foreach (var removedAnchor in removed)
                    {
                        _trackedAnchorIds.Remove(removedAnchor);
                    }

                    // Update seen anchor IDs for next call
                    _seenAnchorIds = currentAnchorIds;
                }

                using var addedChanges = new NativeArray<XRVps2Anchor>(added.ToArray(), Allocator.Temp);
                using var updatedChanges = new NativeArray<XRVps2Anchor>(updated.ToArray(), Allocator.Temp);
                using var removedChanges = new NativeArray<TrackableId>(removed.ToArray(), Allocator.Temp);

                return TrackableChanges<XRVps2Anchor>.CopyFrom
                (
                    addedChanges,
                    updatedChanges,
                    removedChanges,
                    allocator
                );
            }

            private XRVps2Anchor CreateXRVps2Anchor(byte[] anchorIdBuffer)
            {
                string anchorId = Encoding.UTF8.GetString(anchorIdBuffer, 0, IApi.NSDK_VPS2_ANCHOR_ID_SIZE);
                TrackableId trackableId = TrackableIdExtension.FromNativeUuid(anchorId);
                _trackedAnchorIds.Add(trackableId, anchorId);

                return new XRVps2Anchor(trackableId);
            }
        }

        void ISubsystemWithMutableApi<IAnchorApi>.SwitchApiImplementation(IAnchorApi api)
        {
            ((NsdkProvider)provider).SwitchApiImplementation(api);
        }

        public void SwitchToInternalMockImplementation()
        {
            throw new NotImplementedException();
        }
    }
}
