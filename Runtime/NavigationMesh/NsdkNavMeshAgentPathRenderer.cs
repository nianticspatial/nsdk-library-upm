// Copyright 2022-2026 Niantic Spatial.

using System;
using System.Collections;
using System.Collections.Generic;
using NianticSpatial.NSDK.AR.Utilities;
using UnityEngine;

namespace NianticSpatial.NSDK.AR.NavigationMesh
{
    /// <summary>
    /// NsdkNavMeshAgentPathRenderer is a debug renderer to show you the path a  <see cref="NsdkNavMeshAgent"/> is moving along
    /// while navigating the environment. You add it to the <see cref="NsdkNavMeshAgent"/> <see cref="GameObject"/> in your scene
    /// and it will draw that agent's current path.
    /// </summary>
    [PublicAPI("apiref/Niantic/Lightship/AR/NavigationMesh/NsdkNavMeshAgentPathRenderer/")]
    public class NsdkNavMeshAgentPathRenderer : MonoBehaviour
    {
        /// <summary>
        /// The <see cref="NsdkNavMeshAgent"/> that you want to render the path for.
        /// </summary>
        [Tooltip("The NsdkNavMeshAgent that you want to render the path for.")]
        public NsdkNavMeshAgent _agent;
        /// <summary>
        /// The <see cref="Material"/> used to render the path. This <see cref="Material"/>  will be applied on a <see cref="LineRenderer"/>.
        /// </summary>
        [Tooltip("The Material used to render the path. This Material will be applied on a LineRenderer.")]
        public Material _material;
        private LineRenderer _lineRenderer;
        private List<Vector3> _points = new List<Vector3>();

        private void Start()
        {
            _lineRenderer = gameObject.AddComponent<LineRenderer>();
            _lineRenderer.material = _material;
            //.shader=Shader.Find("Unlit/Color");
            _lineRenderer.material.color = Color.blue;

            _lineRenderer.startWidth = 0.05f;
            _lineRenderer.endWidth = 0.05f;

        }

        private void AddLine(Vector3 start, Vector3 end)
        {
            _points.Add(start);
            _points.Add(end);
        }

        private void OnEnable()
        {
            if (_lineRenderer != null)
                _lineRenderer.enabled = true;
        }

        private void OnDisable()
        {
            _lineRenderer.enabled = false;
        }

        private void Update()
        {

            if (_agent.path.Waypoints == null)
                return;

            //get this agents path and make a flat array of points
            var path = _agent.path;
            _points.Clear();

            float offset = 0.01f;
            for (int i = 0; i < path.Waypoints.Count - 1; i++)
            {
                var points = path.Waypoints;
                AddLine(points[i].WorldPosition + Vector3.up * offset,
                    points[i + 1].WorldPosition + Vector3.up * offset);
            }

            _lineRenderer.positionCount = _points.Count;
            _lineRenderer.SetPositions(_points.ToArray());

        }
    }
}
