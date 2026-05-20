// Copyright Niantic Spatial.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Niantic.Lightship.AR.Protobuf;
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
using NativeApi = NianticSpatial.NSDK.AR.Subsystems.Vps2.Api.NativeApi;

namespace NianticSpatial.NSDK.AR.Subsystems.Vps2
{
    [Preserve]
    public sealed class NsdkVps2Subsystem : XRVps2Subsystem, ISubsystemWithMutableApi<IApi>
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterDescriptor()
        {
            Log.Info(nameof(NsdkVps2Subsystem) + "." + nameof(RegisterDescriptor));
            var cinfo = new XRVps2SubsystemCinfo()
            {
                id = "NSDK-VPS2",
                providerType = typeof(NsdkProvider),
                subsystemTypeOverride = typeof(NsdkVps2Subsystem),
            };

            UnityEngine.SubsystemsImplementation.SubsystemDescriptorStore.RegisterDescriptor(
                XRVps2SubsystemDescriptor.Create(cinfo));
        }

        internal class NsdkProvider : Provider
        {
            private IApi _api;
            private IntPtr _nativeProviderHandle;

            // Passing in "0" values causes native code to use
            // native default values
            private bool _requestedUniversalLocalizationEnabled = false;
            private float _requestedUniversalLocalizationRps = 0;
            private bool _requestedVpsLocalizationEnabled = false;
            private float _requestedInitialVpsRps = 0;
            private float _requestedContinuousVpsRps = 0;
            private bool _geolocationSmoothingEnabled = false;
            private bool _deviceMapLocalizationEnabled = false;
            private int _deviceMapLocalizationFramerate = 0;
            private bool _vpsDebuggerEnabled = false;
            private int _universalLocalizationRequestTimeoutMs = 0;
            private uint _maxRequestsInTransitPerTarget = 0;
            private float _anchorDistanceGateMeters = 0;

            private bool _configDirty;

            // Trackable changes
            private Dictionary<TrackableId, string> _trackedAnchorIds = new Dictionary<TrackableId, string>();
            private HashSet<TrackableId> _seenAnchorIds = new HashSet<TrackableId>();

            public override bool UniversalLocalizationEnabled
            {
                set
                {
                    if (_requestedUniversalLocalizationEnabled != value)
                    {
                        _requestedUniversalLocalizationEnabled = value;
                        MarkConfigurationDirty();
                    }
                }
            }

            public override float UniversalLocalizationRequestsPerSecond
            {
                set
                {
                    if (value <= 0)
                    {
                        throw new ArgumentException("UniversalLocalizationRequestsPerSecond value must be greater than zero");
                    }

                    if (!Mathf.Approximately(_requestedUniversalLocalizationRps, value))
                    {
                        _requestedUniversalLocalizationRps = value;
                        MarkConfigurationDirty();
                    }
                }
            }

            public override bool VpsMapLocalizationEnabled
            {
                set
                {
                    if (_requestedVpsLocalizationEnabled != value)
                    {
                        _requestedVpsLocalizationEnabled = value;
                        MarkConfigurationDirty();
                    }
                }
            }

            public override float InitialVpsRequestsPerSecond
            {
                set
                {
                    if (value <= 0)
                    {
                        throw new ArgumentException("InitialVpsRequestsPerSecond value must be greater than zero");
                    }

                    if (!Mathf.Approximately(_requestedInitialVpsRps, value))
                    {
                        _requestedInitialVpsRps = value;
                        MarkConfigurationDirty();
                    }
                }
            }

            public override float ContinuousVpsRequestsPerSecond
            {
                set
                {
                    if (value <= 0)
                    {
                        throw new ArgumentException("ContinuousVpsRequestsPerSecond value must be greater than zero");
                    }

                    if (!Mathf.Approximately(_requestedContinuousVpsRps, value))
                    {
                        _requestedContinuousVpsRps = value;
                        MarkConfigurationDirty();
                    }
                }
            }

            public override bool GeolocationSmoothingEnabled
            {
                set
                {
                    if (_geolocationSmoothingEnabled != value)
                    {
                        _geolocationSmoothingEnabled = value;
                        MarkConfigurationDirty();
                    }
                }
            }

            public override bool DeviceMapLocalizationEnabled
            {
                set
                {
                    if (_deviceMapLocalizationEnabled != value)
                    {
                        _deviceMapLocalizationEnabled = value;
                        MarkConfigurationDirty();
                    }
                }
            }

            public override int DeviceMapLocalizationFramerate
            {
                set
                {
                    if (_deviceMapLocalizationFramerate != value)
                    {
                        _deviceMapLocalizationFramerate = value;
                        MarkConfigurationDirty();
                    }
                }
            }

            public override bool VpsDebuggerEnabled
            {
                set
                {
                    if (_vpsDebuggerEnabled != value)
                    {
                        _vpsDebuggerEnabled = value;
                        MarkConfigurationDirty();
                    }
                }
            }

            public override int UniversalLocalizationRequestTimeoutMs
            {
                set
                {
                    if (_universalLocalizationRequestTimeoutMs != value)
                    {
                        _universalLocalizationRequestTimeoutMs = value;
                        MarkConfigurationDirty();
                    }
                }
            }

            public override uint MaxRequestsInTransitPerTarget
            {
                set
                {
                    if (_maxRequestsInTransitPerTarget != value)
                    {
                        _maxRequestsInTransitPerTarget = value;
                        MarkConfigurationDirty();
                    }
                }
            }

            public override float AnchorDistanceGateMeters
            {
                set
                {
                    if (!Mathf.Approximately(_anchorDistanceGateMeters, value))
                    {
                        _anchorDistanceGateMeters = value;
                        MarkConfigurationDirty();
                    }
                }
            }

            public NsdkProvider() : this(new NativeApi()) { }
            public NsdkProvider(IApi api)
            {
                Log.Info("NsdkVps2Subsystem.NsdkProvider constructor");
                _api = api;
#if NIANTICSPATIAL_NSDK_AR_LOADER_ENABLED
                _nativeProviderHandle = _api.Construct(NsdkUnityContext.UnityContextHandle);
#endif
                Log.Info("NsdkVps2Subsystem got _nativeProviderHandle: " + _nativeProviderHandle);
            }

            public void SwitchApiImplementation(IApi api)
            {
                if (_nativeProviderHandle != IntPtr.Zero)
                {
                    _api.Stop(_nativeProviderHandle);
                    _api.Destroy(_nativeProviderHandle);
                }

                _api = api;
                _nativeProviderHandle = api.Construct(NsdkUnityContext.UnityContextHandle);
            }

            public override void Start()
            {
                if (!_nativeProviderHandle.IsValidHandle())
                {
                    return;
                }

                Configure();

                // Check ConfigureIfDirty once per frame to avoid calling Configure multiple times
                // in one frame if multiple fields are changed.
                MonoBehaviourEventDispatcher.LateUpdating.AddListener(ConfigureIfDirty);

                _api.Start(_nativeProviderHandle).ThrowExceptionIfNeeded();
            }

            public override void Stop()
            {
                if (!_nativeProviderHandle.IsValidHandle())
                {
                    return;
                }

                _trackedAnchorIds.Clear();
                _seenAnchorIds.Clear();

                _api.Stop(_nativeProviderHandle).ThrowExceptionIfNeeded();
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

            private void MarkConfigurationDirty()
            {
                _configDirty = true;
            }

            private void ConfigureIfDirty()
            {
                if (_configDirty)
                {
                    Configure();
                }
            }

            private void Configure()
            {
                if (!_nativeProviderHandle.IsValidHandle())
                {
                    return;
                }

                if (running)
                {
                    Log.Warning("Configuration changed while running, stop and restart the " +
                        "subsystem to use the new configuration");
                }

                _configDirty = false;

                var config = new IApi.NsdkVps2Config
                {
                    bevLocalizationEnabled = _requestedUniversalLocalizationEnabled,
                    bevRequestsPerSecond = _requestedUniversalLocalizationRps,
                    vpsLocalizationEnabled = _requestedVpsLocalizationEnabled,
                    initialVpsRequestsPerSecond = _requestedInitialVpsRps,
                    continuousVpsRequestsPerSecond = _requestedContinuousVpsRps,
                    geolocationSmoothingEnabled = _geolocationSmoothingEnabled,
                    deviceMapLocalizationEnabled = _deviceMapLocalizationEnabled,
                    deviceMapLocalizationFramerate = _deviceMapLocalizationFramerate,
                    vpsDebuggerEnabled = _vpsDebuggerEnabled,
                    universalLocalizationRequestTimeoutMs = _universalLocalizationRequestTimeoutMs,
                    maxRequestsInTransitPerTarget = _maxRequestsInTransitPerTarget,
                    anchorDistanceGateMeters = _anchorDistanceGateMeters
                };

                _api.Configure(_nativeProviderHandle, config).ThrowExceptionIfNeeded();
            }

            public override XRVps2Localization GetLatestLocalization()
            {
                if (!_nativeProviderHandle.IsValidHandle())
                {
                    return new XRVps2Localization { TrackingState = Vps2TrackingState.Unavailable };
                }

                _api.GetLatestLocalization
                (
                    _nativeProviderHandle,
                    out IApi.NsdkVps2Localization localizationOut
                ).ThrowExceptionIfNeeded();

                return new XRVps2Localization
                {
                    TrackingState = (Vps2TrackingState)localizationOut.trackingState,
                    ReferenceLatitude = localizationOut.referenceLatitudeDegrees,
                    ReferenceLongitude = localizationOut.referenceLongitudeDegrees,
                    ReferenceAltitude = localizationOut.referenceAltitudeMeters,
                    TrackingToEdn = localizationOut.trackingToRelativeLonNegAltLat.FromColumnMajorArray()
                };
            }

            public override XRVps2Geolocation GetDeviceGeolocation(
                HeadingMode headingMode = HeadingMode.CameraDirection)
            {
                _api.GetDeviceGeolocation
                (
                    _nativeProviderHandle,
                    headingMode,
                    out IApi.NsdkVps2GeolocationData locationOut
                ).ThrowExceptionIfNeeded();

                var orientation = new Quaternion
                (
                    locationOut.geolocationData.orientation_edn_x,
                    locationOut.geolocationData.orientation_edn_y,
                    locationOut.geolocationData.orientation_edn_z,
                    locationOut.geolocationData.orientation_edn_w
                );

                var geolocation = new XRGeolocation
                {
                    Latitude = locationOut.geolocationData.latitude,
                    Longitude = locationOut.geolocationData.longitude,
                    Altitude = locationOut.geolocationData.altitude,
                    Heading = locationOut.geolocationData.heading_edn,
                    OrientationEdn = orientation
                };

                return new XRVps2Geolocation
                {
                    TrackingState = (Vps2TrackingState)locationOut.trackingState,
                    Geolocation = geolocation,
                    HorizontalAccuracy = locationOut.horizontalAccuracyMeters,
                    VerticalAccuracy = locationOut.verticalAccuracyMeters,
                    HeadingAccuracy = locationOut.rotationAccuracyDegrees
                };
            }

            public override XRVps2Pose GetPose
            (
                XRVps2Localization localization,
                double latitude,
                double longitude,
                double altitude,
                Quaternion orientationEdn
            )
            {
                var nsdkGeolocation = new NsdkGeolocationData
                {
                    latitude = latitude,
                    longitude = longitude,
                    altitude = altitude,
                    orientation_edn_x = orientationEdn.x,
                    orientation_edn_y = orientationEdn.y,
                    orientation_edn_z = orientationEdn.z,
                    orientation_edn_w = orientationEdn.w
                };

                _api.GetPose
                (
                    ConvertToNsdk(localization),
                    nsdkGeolocation,
                    out IApi.NsdkVps2Pose poseOut
                ).ThrowExceptionIfNeeded();

                return new XRVps2Pose
                {
                    Pose = poseOut.pose.FromNsdkToUnity(),
                    HorizontalAccuracy = poseOut.horizontalAccuracyMeters,
                    VerticalAccuracy = poseOut.verticalAccuracyMeters,
                    HeadingAccuracy = poseOut.rotationAccuracyDegrees
                };
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

            private XRVps2Anchor CreateXRVps2Anchor(byte[] anchorIdBuffer)
            {
                string anchorId = Encoding.UTF8.GetString(anchorIdBuffer, 0, IApi.NSDK_VPS2_ANCHOR_ID_SIZE);
                TrackableId trackableId = TrackableIdExtension.FromNativeUuid(anchorId);
                _trackedAnchorIds.Add(trackableId, anchorId);

                return new XRVps2Anchor(trackableId);
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

            public override TrackableChanges<XRVps2Anchor> GetChanges(Allocator allocator)
            {
                var added = new List<XRVps2Anchor>();
                var updated = new List<XRVps2Anchor>();
                var removed = new List<TrackableId>();

                var currentAnchorIds = new HashSet<TrackableId>();

                foreach (var anchorId in _trackedAnchorIds)
                {
                    var anchorIdBuffer = Encoding.UTF8.GetBytes(anchorId.Value, 0, IApi.NSDK_VPS2_ANCHOR_ID_SIZE);
                    var status = _api.GetAnchorUpdate(_nativeProviderHandle, anchorIdBuffer, out var updateOut);
                    if (status == NsdkStatus.InvalidArgument)
                    {
                        // anchor has not yet been added by native module
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

                var addedChanges = new NativeArray<XRVps2Anchor>(added.Count, Allocator.Temp);
                var updatedChanges = new NativeArray<XRVps2Anchor>(updated.Count, Allocator.Temp);
                var removedChanges = new NativeArray<TrackableId>(removed.Count, Allocator.Temp);

                for (int i = 0; i < added.Count; i++)
                {
                    addedChanges[i] = added[i];
                }

                for (int i = 0; i < updated.Count; i++)
                {
                    updatedChanges[i] = updated[i];
                }

                for (int i = 0; i < removed.Count; i++)
                {
                    removedChanges[i] = removed[i];
                }

                return TrackableChanges<XRVps2Anchor>.CopyFrom
                (
                    addedChanges,
                    updatedChanges,
                    removedChanges,
                    allocator
                );
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

            public override List<XRVps2LocalizationRequestRecord> GetLatestLocalizationRequestRecords()
            {
                var records = new List<XRVps2LocalizationRequestRecord>();
                if (!_nativeProviderHandle.IsValidHandle())
                {
                    return records;
                }

                _api.GetLatestLocalizationRequestRecords
                (
                    _nativeProviderHandle,
                    out var recordsPtr,
                    out var recordsCount,
                    out var handle
                ).ThrowExceptionIfNeeded();

                if (!(recordsPtr.IsValidHandle() && recordsCount > 0 && handle.IsValidHandle()))
                {
                    return records;
                }

                var recordSize = Marshal.SizeOf<IApi.NsdkVps2NetworkResponseRecord>();
                for (int i = 0; i < recordsCount; i++)
                {
                    var recordPtr = recordsPtr + i * recordSize;
                    var nativeRecord = Marshal.PtrToStructure<IApi.NsdkVps2NetworkResponseRecord>(recordPtr);

                    // identifier is a 32-char hex-encoded UUID (same format as anchor IDs)
                    string hexId = Encoding.UTF8.GetString(
                        nativeRecord.requestIdentifier, 0, IApi.NSDK_VPS2_ANCHOR_ID_SIZE);
                    Guid requestId = Guid.TryParse(hexId, out var parsed) ? parsed : Guid.Empty;

                    var xrRecord = new XRVps2LocalizationRequestRecord
                    {
                        RequestId = requestId,
                        Status = (Vps2LocalizationRequestStatus)nativeRecord.status,
                        RequestType = (Vps2LocalizationRequestType)nativeRecord.type,
                        ErrorCode = (Vps2LocalizationError)nativeRecord.error,
                        StartTimeMs = nativeRecord.startTimeMs,
                        EndTimeMs = nativeRecord.endTimeMs,
                        FrameId = nativeRecord.frameId,
                    };
                    records.Add(xrRecord);
                }

                NsdkExternUtils.ReleaseResource(handle);
                return records;
            }

            public override List<VpsDebuggerDataEvent> GetLatestDebuggerData()
            {
                var events = new List<VpsDebuggerDataEvent>();

                if (!_nativeProviderHandle.IsValidHandle())
                {
                    return events;
                }

                _api.GetLatestDebuggerLogs(
                    _nativeProviderHandle,
                    out var logsPtr,
                    out var count,
                    out var handle
                ).ThrowExceptionIfNeeded();

                if (!(logsPtr.IsValidHandle() && count > 0 && handle.IsValidHandle()))
                {
                    // Empty, not null
                    return events;
                }

                var logs = Marshal.PtrToStringAnsi(logsPtr, count);
                NsdkExternUtils.ReleaseResource(handle);

                var lines = logs.Split('\n', StringSplitOptions.RemoveEmptyEntries);
                foreach (var line in lines)
                {
                    var debuggerEvent = VpsDebuggerDataEvent.Parser.ParseJson(line);
                    events.Add(debuggerEvent);
                }

                return events;
            }

            public override bool GetSessionId(out string sessionId)
            {
                if (!_nativeProviderHandle.IsValidHandle())
                {
                    sessionId = null;
                    return false;
                }

                var sessionIdOut = new byte[IApi.NSDK_VPS2_SESSION_ID_SIZE];
                var nsdkStatus = _api.GetSessionId(_nativeProviderHandle, ref sessionIdOut);

                if (!nsdkStatus.IsOk())
                {
                    Log.Warning($"Failed to get session id due to error: {nsdkStatus}");
                    sessionId = null;
                    return false;
                }

                sessionId = Encoding.UTF8.GetString(sessionIdOut, 0, IApi.NSDK_VPS2_SESSION_ID_SIZE);
                return true;
            }

            private static IApi.NsdkVps2Localization ConvertToNsdk(XRVps2Localization localization)
            {
                return new IApi.NsdkVps2Localization
                {
                    trackingState = (int)localization.TrackingState,
                    referenceLatitudeDegrees = localization.ReferenceLatitude,
                    referenceLongitudeDegrees = localization.ReferenceLongitude,
                    referenceAltitudeMeters = localization.ReferenceAltitude,
                    trackingToRelativeLonNegAltLat = localization.TrackingToEdn.ToColumnMajorArray()
                };
            }
        }
        void ISubsystemWithMutableApi<IApi>.SwitchApiImplementation(IApi api)
        {
            ((NsdkProvider)provider).SwitchApiImplementation(api);
        }

        public void SwitchToInternalMockImplementation()
        {
            throw new NotImplementedException();
        }
    }
}
