// Copyright 2022-2026 Niantic Spatial.

using System;
using UnityEngine;

// Used for simulation settings in editor only, does not exist on device.
#if UNITY_EDITOR
using UnityEngine.XR.Simulation;
#endif

namespace NianticSpatial.NSDK.AR.Loader
{
    [Serializable]
    public class NsdkSimulationParams
    {
        [SerializeField]
        [Tooltip("When enabled, uses the geometry depth from camera z-buffer, instead of NSDK depth prediction")]
        private bool _useZBufferDepth = true;

        /// <summary>
        /// Whether to use Z-buffer depth instead of NSDK depth prediction
        /// </summary>
        public bool UseZBufferDepth
        {
            get => _useZBufferDepth;
            set => _useZBufferDepth = value;
        }

        internal NsdkSimulationParams()
        {
        }

        internal NsdkSimulationParams(NsdkSimulationParams source)
        {
            CopyFrom(source);
        }

        internal void CopyFrom(NsdkSimulationParams source)
        {
            UseZBufferDepth = source._useZBufferDepth;
        }
    }
}
