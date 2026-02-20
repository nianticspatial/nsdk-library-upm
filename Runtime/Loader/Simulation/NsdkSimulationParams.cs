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

        [SerializeField]
        [Tooltip("When enabled, use NSDK Persistent Anchors instead of simulation Persistent Anchors")]
        private bool _useSimulationPersistentAnchor = true;

        [SerializeField]
        [Tooltip("Parameters for simulating the persistent anchor subsystem")]
        NsdkSimulationPersistentAnchorParams _simulationPersistentAnchorParams = new();

        /// <summary>
        /// Layer used for the depth
        /// </summary>
        public bool UseZBufferDepth
        {
            get => _useZBufferDepth;
            set => _useZBufferDepth = value;
        }

        /// <summary>
        /// Layer used for the persistent anchor
        /// </summary>
        public bool UseSimulationPersistentAnchor
        {
            get => _useSimulationPersistentAnchor;
            set => _useSimulationPersistentAnchor = value;
        }

        /// <summary>
        /// Parameters for simulating the persistent anchor subsystem
        /// </summary>
        public NsdkSimulationPersistentAnchorParams SimulationPersistentAnchorParams
        {
            get => _simulationPersistentAnchorParams;
        }

        internal NsdkSimulationParams()
        {
            _simulationPersistentAnchorParams = new NsdkSimulationPersistentAnchorParams();
        }

        internal NsdkSimulationParams(NsdkSimulationParams source)
        {
            _simulationPersistentAnchorParams = new NsdkSimulationPersistentAnchorParams();
            CopyFrom(source);
        }

        internal void CopyFrom(NsdkSimulationParams source)
        {
            UseZBufferDepth = source._useZBufferDepth;
            UseSimulationPersistentAnchor = source._useSimulationPersistentAnchor;
            SimulationPersistentAnchorParams.CopyFrom(source._simulationPersistentAnchorParams);
        }
    }
}
