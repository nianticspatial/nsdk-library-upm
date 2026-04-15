// Copyright 2022-2026 Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.Core;
using NianticSpatial.NSDK.AR.Loader;
using NianticSpatial.NSDK.AR.Utilities;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using NianticSpatial.NSDK.AR.XRSubsystems;
using UnityEngine;
using UnityEngine.Scripting;
using IApi = NianticSpatial.NSDK.AR.Subsystems.DeviceMapping.Api.IApi;
using NativeApi = NianticSpatial.NSDK.AR.Subsystems.DeviceMapping.Api.NativeApi;

namespace NianticSpatial.NSDK.AR.Subsystems.DeviceMapping
{
    [Preserve]
    public sealed class NsdkDeviceMappingSubsystem : XRDeviceMappingSubsystem, ISubsystemWithMutableApi<IApi>
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void RegisterDescriptor()
        {
            Log.Info(nameof(NsdkDeviceMappingSubsystem) + "." + nameof(RegisterDescriptor));
            var cinfo = new XRDeviceMappingSubsystemDescriptor.Cinfo()
            {
                id = "NSDK-DeviceMapping",
                providerType = typeof(NsdkProvider),
                subsystemTypeOverride = typeof(NsdkDeviceMappingSubsystem),
            };

            XRDeviceMappingSubsystemDescriptor.Create(cinfo);
        }

        internal class NsdkProvider : Provider
        {
            private IApi _api;
            private IntPtr _nativeProviderHandle;

            private uint _requestedTargetFrameRate = 0;
            private float _requestedSplitterMaxDistanceMeters = 0;
            private float _requestedSplitterMaxDurationSeconds = 0;

            private bool _isMapping;
            private bool _configDirty;

            public override uint TargetFrameRate
            {
                set
                {
                    if (_requestedTargetFrameRate != value)
                    {
                        _requestedTargetFrameRate = value;
                        ValidateConfigurationChange();
                    }
                }
            }

            public override float SplitterMaxDistanceMeters
            {
                set
                {
                    if (!Mathf.Approximately(_requestedSplitterMaxDistanceMeters, value))
                    {
                        _requestedSplitterMaxDistanceMeters = value;
                        ValidateConfigurationChange();
                    }
                }
            }

            public override float SplitterMaxDurationSeconds
            {
                set
                {
                    if (!Mathf.Approximately(_requestedSplitterMaxDurationSeconds, value))
                    {
                        _requestedSplitterMaxDurationSeconds = value;
                        ValidateConfigurationChange();
                    }
                }
            }

            public override bool IsMapping => _isMapping;

            public NsdkProvider() : this(new NativeApi()) { }

            public NsdkProvider(IApi api)
            {
                Log.Info("NsdkDeviceMappingSubsystem.NsdkProvider constructor");
                _api = api;
#if NIANTICSPATIAL_NSDK_AR_LOADER_ENABLED
                _nativeProviderHandle = _api.Construct(NsdkUnityContext.UnityContextHandle);
#endif
                Log.Info("NsdkDeviceMappingSubsystem got _nativeProviderHandle: " + _nativeProviderHandle);
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
                _api.Start(_nativeProviderHandle);
                _isMapping = false;
            }

            public override void Stop()
            {
                if (!_nativeProviderHandle.IsValidHandle())
                {
                    return;
                }

                _api.Stop(_nativeProviderHandle);
                _isMapping = false;
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

            public override void StartMapping()
            {
                if (!_nativeProviderHandle.IsValidHandle())
                {
                    return;
                }

                _isMapping = true;
                _api.StartMapping(_nativeProviderHandle);
            }

            public override void StopMapping()
            {
                if (!_nativeProviderHandle.IsValidHandle())
                {
                    return;
                }

                _api.StopMapping(_nativeProviderHandle);
                _isMapping = false;
            }

            private void ValidateConfigurationChange()
            {
                if (running)
                {
                    Log.Warning("NsdkDeviceMappingSubsystem cannot be configured while it is running. Stop and restart to apply the configuration changes.");
                }
                else
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

                _configDirty = false;

                var config = new IApi.NsdkDeviceMappingConfig
                {
                    slickLearnedFeaturesEnabled = false,
                    forceCPULearnedFeatures = false,
                    slickMapperFps = _requestedTargetFrameRate,
                    splitterMaxDistanceMeters = _requestedSplitterMaxDistanceMeters,
                    splitterMaxDurationSeconds = _requestedSplitterMaxDurationSeconds,
                };

                _api.Configure(_nativeProviderHandle, config);
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
