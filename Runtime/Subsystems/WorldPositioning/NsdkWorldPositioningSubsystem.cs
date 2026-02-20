// Copyright 2022-2026 Niantic Spatial.

using System;

using NianticSpatial.NSDK.AR.Core;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using NianticSpatial.NSDK.AR.XRSubsystems;

using UnityEngine;
using UnityEngine.Scripting;
using UnityEngine.SubsystemsImplementation;


namespace NianticSpatial.NSDK.AR.Subsystems.WorldPositioning
{
    /// <summary>
    /// The Lightship implementation of the <c>XRWorldPositioningSubsystem</c>. Do not create this directly.
    /// Use the <c>SubsystemManager</c> instead.
    /// </summary>
    [Preserve]
    public sealed class NsdkWorldPositioningSubsystem : XRWorldPositioningSubsystem
    {
        internal class NsdkProvider : Provider
        {
            private IApi _api;

            /// <summary>
            /// The handle to the native version of the provider
            /// </summary>
            private IntPtr _nativeProviderHandle;

            private int _framerate = 120;
            private bool _smoothingEnabled = true;
            private bool _bevEnabled = false;
            private int _bevFramerate = 0;

            /// <summary>
            /// Construct the implementation provider.
            /// </summary>
            public NsdkProvider()
                : this(new NativeApi())
            {
            }

            /// <summary>
            /// Property to get or set the target frame rate for world positioning updates.
            /// </summary>
            /// <value>
            /// The requested target frame rate in frames per second.
            /// </value>
            public override int Framerate
            {
                get => _framerate;
                set
                {
                    if (value <= 0)
                    {
                        Log.Error("Target frame rate value must be greater than zero.");
                        return;
                    }

                    if (_framerate != value)
                    {
                        _framerate = value;
                        ConfigureProvider();
                    }
                }
            }

            /// <summary>
            /// Property to get or set whether smoothing is enabled for world positioning.
            /// </summary>
            public override bool Smoothing
            {
                get => _smoothingEnabled;
                set
                {
                    if (_smoothingEnabled != value)
                    {
                        _smoothingEnabled = value;
                        ConfigureProvider();
                    }
                }
            }

            public NsdkProvider(IApi api)
            {
                _api = api;
#if NIANTICSPATIAL_NSDK_AR_LOADER_ENABLED
                _nativeProviderHandle = _api.Construct(NsdkUnityContext.UnityContextHandle);
#endif
            }

            // Destruct the native provider and replace it with the provided (or default mock) provider
            // Used for testing and mocking
            public void SwitchApiImplementation(IApi api)
            {
                if (_nativeProviderHandle != IntPtr.Zero)
                {
                    _api.Stop(_nativeProviderHandle);
                    _api.Destruct(_nativeProviderHandle);
                }

                _api = api;
                _nativeProviderHandle = _api.Construct(NsdkUnityContext.UnityContextHandle);
            }

            public override void Start()
            {
                if (!_nativeProviderHandle.IsValidHandle())
                {
                    return;
                }

                _api.Start(_nativeProviderHandle);
            }

            public override void Stop()
            {
                if (!_nativeProviderHandle.IsValidHandle())
                {
                    return;
                }

                _api.Stop(_nativeProviderHandle);
            }

            public override void Destroy()
            {
                if (!_nativeProviderHandle.IsValidHandle())
                {
                    return;
                }

                _api.Destruct(_nativeProviderHandle);
                _nativeProviderHandle = IntPtr.Zero;
                ;
            }

            public override WorldPositioningStatus TryGetXRToWorld
            (
                ref Matrix4x4 arToWorld,
                ref double originLatitude,
                ref double originLongitude,
                ref double originAltitude
            )
            {
                if (!running)
                {
                    return WorldPositioningStatus.SubsystemNotRunning;
                }

                if (!_nativeProviderHandle.IsValidHandle())
                {
                    arToWorld = Matrix4x4.zero;
                    originLatitude = 0.0;
                    originLongitude = 0.0;
                    originAltitude = 0.0;
                    return WorldPositioningStatus.Initializing;
                }

                return (WorldPositioningStatus)_api.TryGetXRToWorld
                (
                    _nativeProviderHandle,
                    out arToWorld,
                    out originLatitude,
                    out originLongitude,
                    out originAltitude
                );
            }

            private void ConfigureProvider()
            {
                if (!_nativeProviderHandle.IsValidHandle())
                {
                    return;
                }
                _api.Configure(_nativeProviderHandle, _framerate, _smoothingEnabled, _bevEnabled, _bevFramerate);
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void RegisterDescriptor()
        {
            var cinfo = new XRWorldPositioningSubsystemCinfo()
            {
                id = "Nsdk-WorldPositioning",
                providerType = typeof(NsdkProvider),
                subsystemTypeOverride = typeof(NsdkWorldPositioningSubsystem),
            };

            SubsystemDescriptorStore.RegisterDescriptor
                (XRWorldPositioningSubsystemDescriptor.Create(cinfo));
        }

        void SwitchApiImplementation(IApi api)
        {
            ((NsdkProvider)provider).SwitchApiImplementation(api);
        }

        void SwitchToInternalMockImplementation()
        {
            ((NsdkProvider)provider).SwitchApiImplementation(new MockApi());
        }
    }
}
