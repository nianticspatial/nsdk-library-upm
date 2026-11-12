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
using UnityEngine;
using UnityEngine.Scripting;
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
            private bool _vpsDebuggerSavingFrameImageEnabled = false;
            private int _universalLocalizationRequestTimeoutMs = 0;
            private uint _maxRequestsInTransitPerTarget = 0;
            private float _anchorDistanceGateMeters = 0;
            private bool _blurDetectionEnabled = false;
            private float _minSharpnessScore = 0;
            private float _areaTargetingEnterMarginMeters = 0;
            private float _areaTargetingExitMarginMeters = 0;
            private bool _aerialLocalizationEnabled = false;

            // Not zero-defaulted like the fields above: temporal fusion is on natively, and
            // passing false disables it rather than falling back to the native default.
            private bool _temporalFusionEnabled = true;

            private bool _configDirty;

            // Set by Start and cleared by Stop. Not SubsystemProvider.running: only the
            // subsystem wrapper sets that, so a provider driven directly, as in tests,
            // would never be running.
            private bool _started;

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

            public override bool VpsDebuggerSavingFrameImageEnabled
            {
                set
                {
                    if (_vpsDebuggerSavingFrameImageEnabled != value)
                    {
                        _vpsDebuggerSavingFrameImageEnabled = value;
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

            public override bool BlurDetectionEnabled
            {
                set
                {
                    if (_blurDetectionEnabled != value)
                    {
                        _blurDetectionEnabled = value;
                        MarkConfigurationDirty();
                    }
                }
            }

            public override float MinSharpnessScore
            {
                set
                {
                    if (!Mathf.Approximately(_minSharpnessScore, value))
                    {
                        _minSharpnessScore = value;
                        MarkConfigurationDirty();
                    }
                }
            }

            public override float AreaTargetingEnterMarginMeters
            {
                set
                {
                    if (!Mathf.Approximately(_areaTargetingEnterMarginMeters, value))
                    {
                        _areaTargetingEnterMarginMeters = value;
                        MarkConfigurationDirty();
                    }
                }
            }

            public override float AreaTargetingExitMarginMeters
            {
                set
                {
                    if (!Mathf.Approximately(_areaTargetingExitMarginMeters, value))
                    {
                        _areaTargetingExitMarginMeters = value;
                        MarkConfigurationDirty();
                    }
                }
            }

            public override bool TemporalFusionEnabled
            {
                set
                {
                    if (_temporalFusionEnabled != value)
                    {
                        _temporalFusionEnabled = value;
                        MarkConfigurationDirty();
                    }
                }
            }

            public override bool AerialLocalizationEnabled
            {
                set
                {
                    if (_aerialLocalizationEnabled != value)
                    {
                        _aerialLocalizationEnabled = value;
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
                _started = true;
            }

            public override void Stop()
            {
                if (!_nativeProviderHandle.IsValidHandle())
                {
                    return;
                }

                _started = false;
                MonoBehaviourEventDispatcher.LateUpdating.RemoveListener(ConfigureIfDirty);

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

                if (_started)
                {
                    Log.Warning("VPS2 configuration changed while running. Request-rate changes " +
                        "apply live; other changes briefly interrupt tracking.");
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
                    anchorDistanceGateMeters = _anchorDistanceGateMeters,
                    blurDetectionEnabled = _blurDetectionEnabled,
                    minSharpnessScore = _minSharpnessScore,
                    vpsDebuggerSavingFrameImageEnabled = _vpsDebuggerSavingFrameImageEnabled,
                    areaTargetingEnterMarginMeters = _areaTargetingEnterMarginMeters,
                    areaTargetingExitMarginMeters = _areaTargetingExitMarginMeters,
                    vpsTemporalFusionEnabled = _temporalFusionEnabled,
                    aerialLocalizationEnabled = _aerialLocalizationEnabled
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
                    LocalizedAsset = DecodeLocalizedAsset(localizationOut.localizedAsset),
                    ReferenceLatitude = localizationOut.referenceLatitudeDegrees,
                    ReferenceLongitude = localizationOut.referenceLongitudeDegrees,
                    ReferenceAltitude = localizationOut.referenceAltitudeMeters,
                    TrackingToEdn = localizationOut.trackingToRelativeLonNegAltLat.FromColumnMajorArray()
                };
            }

            public override bool TryLocalize(string siteId)
            {
                return TryLocalize(siteId, string.Empty);
            }

            public override bool TryLocalize(string siteId, string assetId)
            {
                if (!_nativeProviderHandle.IsValidHandle())
                {
                    return false;
                }

                // The subsystem's lifecycle decides when VPS2 runs. Natively, a localize
                // request starts VPS2 on its own, so it is only forwarded while started.
                if (!_started)
                {
                    Log.Warning("Ignoring TryLocalize: the VPS2 subsystem is not running.");
                    return false;
                }

                using var managedSiteId = new ManagedNsdkString(siteId);
                using var managedAssetId = new ManagedNsdkString(assetId);
                var nsdkStatus = _api.LocalizeToSite
                (
                    _nativeProviderHandle,
                    managedSiteId.ToNsdkString(),
                    managedAssetId.ToNsdkString()
                );
                if (!nsdkStatus.IsOk())
                {
                    Log.Warning($"Failed to localize to site due to error: {nsdkStatus}");
                    return false;
                }

                return true;
            }

            public override bool TryLocalize(double latitude, double longitude, double radiusMeters = 0)
            {
                if (!_nativeProviderHandle.IsValidHandle())
                {
                    return false;
                }

                if (!_started)
                {
                    Log.Warning("Ignoring TryLocalize: the VPS2 subsystem is not running.");
                    return false;
                }

                var nsdkStatus = _api.LocalizeToNearbySites
                (
                    _nativeProviderHandle,
                    latitude,
                    longitude,
                    radiusMeters
                );

                if (!nsdkStatus.IsOk())
                {
                    Log.Warning($"Failed to localize to nearby sites due to error: {nsdkStatus}");
                    return false;
                }

                return true;
            }

            public override bool TryStopLocalize(string siteId = null)
            {
                if (!_nativeProviderHandle.IsValidHandle())
                {
                    return false;
                }

                // A null site id marshals to the empty "every requested site" form.
                using var managedSiteId = new ManagedNsdkString(siteId);
                var nsdkStatus = _api.StopLocalize(_nativeProviderHandle, managedSiteId.ToNsdkString());
                if (!nsdkStatus.IsOk())
                {
                    Log.Warning($"Failed to stop localizing due to error: {nsdkStatus}");
                    return false;
                }

                return true;
            }

            public override bool TryGetAssetTrackingData(out XRVps2AssetTrackingData trackingData, string assetId = null)
            {
                trackingData = default;
                if (!_nativeProviderHandle.IsValidHandle())
                {
                    return false;
                }

                // Only null selects the currently localized asset: an empty id never
                // names an asset, so it can never have tracking data.
                if (assetId == string.Empty)
                {
                    return false;
                }

                NsdkStatus nsdkStatus;
                IApi.NsdkVps2AssetTrackingData dataOut;
                if (assetId == null)
                {
                    nsdkStatus = _api.GetAssetTrackingData(_nativeProviderHandle, null, out dataOut);
                }
                else
                {
                    using var managedAssetId = new ManagedNsdkString(assetId);
                    nsdkStatus = _api.GetAssetTrackingData
                    (
                        _nativeProviderHandle,
                        new[] { managedAssetId.ToNsdkString() },
                        out dataOut
                    );
                }

                // Both return false. InvalidOperation is the keep-polling state: the asset
                // is not fully tracked (spin-up after the first poll, a lapse in tracking,
                // or a TryLocalize for its site still in flight). InvalidArgument means the
                // id is wrong or names an asset no request this session has fetched.
                // Polling does not change that.
                if (nsdkStatus is NsdkStatus.InvalidOperation or NsdkStatus.InvalidArgument)
                {
                    return false;
                }

                nsdkStatus.ThrowExceptionIfNeeded();

                var pose = dataOut.assetToLocalTrackingTransform.FromColumnMajorArray().FromNsdkToUnity();
                trackingData = new XRVps2AssetTrackingData
                {
                    SiteId = DecodeFixedSizeCString(dataOut.asset.siteId),
                    AssetId = DecodeFixedSizeCString(dataOut.asset.assetId),
                    AreaId = DecodeOptionalFixedSizeCString(dataOut.asset.areaId),
                    Pose = new Pose(pose.GetPosition(), pose.rotation),
                    Confidence = dataOut.trackingConfidence,
                    TimestampMs = dataOut.timestampMs,
                    Geolocation = dataOut.hasGeolocation != 0
                        ? ToXRGeolocation(dataOut.geolocationData)
                        : null
                };
                return true;
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

                return ConvertToXR(locationOut);
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
                    HeadingAccuracy = poseOut.rotationAccuracyDegrees,
                    TrackingState = (Vps2TrackingState)poseOut.trackingState
                };
            }

            public override bool TryGetPose
            (
                double latitude,
                double longitude,
                double altitude,
                Quaternion orientationEdn,
                out XRVps2Pose poseOut
            )
            {
                poseOut = new XRVps2Pose();
                if (!_nativeProviderHandle.IsValidHandle())
                {
                    return false;
                }

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

                var nsdkStatus = _api.GetPoseFromLatestLocalization
                (
                    _nativeProviderHandle,
                    nsdkGeolocation,
                    out IApi.NsdkVps2Pose nsdkPose
                );

                if (nsdkStatus == NsdkStatus.InvalidOperation)
                {
                    // Unlogged: this is what a poll sees every frame until VPS2 localizes.
                    return false;
                }

                if (!nsdkStatus.IsOk())
                {
                    Log.Warning($"Failed to get pose due to error: {nsdkStatus}");
                    return false;
                }

                poseOut = new XRVps2Pose
                {
                    Pose = nsdkPose.pose.FromNsdkToUnity(),
                    HorizontalAccuracy = nsdkPose.horizontalAccuracyMeters,
                    VerticalAccuracy = nsdkPose.verticalAccuracyMeters,
                    HeadingAccuracy = nsdkPose.rotationAccuracyDegrees,
                    TrackingState = (Vps2TrackingState)nsdkPose.trackingState
                };
                return true;
            }

            public override bool TryGetGeolocation(Pose pose, out XRVps2Geolocation geolocationOut)
            {
                geolocationOut = new XRVps2Geolocation();
                if (!_nativeProviderHandle.IsValidHandle())
                {
                    return false;
                }

                var nsdkStatus = _api.GetGeolocationFromLatestLocalization
                (
                    _nativeProviderHandle,
                    pose.FromUnityToNsdk(),
                    out IApi.NsdkVps2GeolocationData nsdkGeolocation
                );

                if (nsdkStatus == NsdkStatus.InvalidOperation)
                {
                    // Unlogged: this is what a poll sees every frame until VPS2 localizes.
                    return false;
                }

                if (!nsdkStatus.IsOk())
                {
                    Log.Warning($"Failed to get geolocation due to error: {nsdkStatus}");
                    return false;
                }

                geolocationOut = ConvertToXR(nsdkGeolocation);
                return true;
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

            private static Vps2LocalizedAsset? DecodeLocalizedAsset(IApi.NsdkVps2AssetIdentifier asset)
            {
                // The buffers hold empty strings until a visual localization succeeds
                // against a submitted asset.
                var siteId = DecodeFixedSizeCString(asset.siteId);
                var assetId = DecodeFixedSizeCString(asset.assetId);
                if (string.IsNullOrEmpty(siteId) || string.IsNullOrEmpty(assetId))
                {
                    return null;
                }

                return new Vps2LocalizedAsset
                {
                    SiteId = siteId,
                    AssetId = assetId,
                    AreaId = DecodeOptionalFixedSizeCString(asset.areaId)
                };
            }

            // An empty buffer is an id the native side left unset; the public structs carry that
            // as null.
            private static string DecodeOptionalFixedSizeCString(byte[] buffer)
            {
                var value = DecodeFixedSizeCString(buffer);
                return string.IsNullOrEmpty(value) ? null : value;
            }

            internal static string DecodeFixedSizeCString(byte[] buffer)
            {
                if (buffer == null)
                {
                    return string.Empty;
                }

                var nulIndex = Array.IndexOf(buffer, (byte)0);
                var length = nulIndex < 0 ? buffer.Length : nulIndex;
                return Encoding.UTF8.GetString(buffer, 0, length);
            }

            private static XRGeolocation ToXRGeolocation(NsdkGeolocationData geolocation)
            {
                return new XRGeolocation
                {
                    Latitude = geolocation.latitude,
                    Longitude = geolocation.longitude,
                    Altitude = geolocation.altitude,
                    Heading = geolocation.heading_edn,
                    OrientationEdn = new Quaternion
                    (
                        geolocation.orientation_edn_x,
                        geolocation.orientation_edn_y,
                        geolocation.orientation_edn_z,
                        geolocation.orientation_edn_w
                    )
                };
            }

            private static XRVps2Geolocation ConvertToXR(IApi.NsdkVps2GeolocationData data)
            {
                var geolocation = new XRGeolocation
                {
                    Latitude = data.geolocationData.latitude,
                    Longitude = data.geolocationData.longitude,
                    Altitude = data.geolocationData.altitude,
                    Heading = data.geolocationData.heading_edn,
                    OrientationEdn = new Quaternion
                    (
                        data.geolocationData.orientation_edn_x,
                        data.geolocationData.orientation_edn_y,
                        data.geolocationData.orientation_edn_z,
                        data.geolocationData.orientation_edn_w
                    )
                };

                return new XRVps2Geolocation
                {
                    TrackingState = (Vps2TrackingState)data.trackingState,
                    Geolocation = geolocation,
                    HorizontalAccuracy = data.horizontalAccuracyMeters,
                    VerticalAccuracy = data.verticalAccuracyMeters,
                    HeadingAccuracy = data.rotationAccuracyDegrees
                };
            }

            private static IApi.NsdkVps2Localization ConvertToNsdk(XRVps2Localization localization)
            {
                return new IApi.NsdkVps2Localization
                {
                    trackingState = (int)localization.TrackingState,
                    referenceLatitudeDegrees = localization.ReferenceLatitude,
                    referenceLongitudeDegrees = localization.ReferenceLongitude,
                    referenceAltitudeMeters = localization.ReferenceAltitude,
                    trackingToRelativeLonNegAltLat = localization.TrackingToEdn.ToColumnMajorArray(),
                    // The fixed-size buffers must be allocated for the struct to marshal.
                    // They stay zeroed: the only consumer of this conversion (GetPose)
                    // does not read the localized asset.
                    localizedAsset = new IApi.NsdkVps2AssetIdentifier
                    {
                        siteId = new byte[IApi.NSDK_VPS2_ASSET_ID_BUFFER_SIZE],
                        assetId = new byte[IApi.NSDK_VPS2_ASSET_ID_BUFFER_SIZE],
                        areaId = new byte[IApi.NSDK_VPS2_ASSET_ID_BUFFER_SIZE]
                    }
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
