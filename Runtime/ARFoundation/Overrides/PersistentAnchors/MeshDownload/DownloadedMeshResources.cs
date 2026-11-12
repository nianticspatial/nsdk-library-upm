// Copyright 2022-2026 Niantic Spatial.

using System.Collections.Generic;
using UnityEngine;

namespace NianticSpatial.NSDK.AR.Subsystems
{
    /// <summary>
    /// Owns runtime Mesh / Texture / Material assets created for a downloaded mesh chunk.
    /// Destroying the GameObject frees those assets; Unity does not do this automatically.
    /// Attached for progressive (ByAsset) chunk downloads only; one-shot payload builds are a follow-up.
    /// The chunk GameObject must have been activated at least once so Unity will invoke OnDestroy
    /// (Unity skips OnDestroy for objects that were never active).
    /// </summary>
    internal class DownloadedMeshResources : MonoBehaviour
    {
        private readonly List<Mesh> _meshes = new List<Mesh>();
        private readonly List<Texture> _textures = new List<Texture>();
        private readonly List<Material> _materials = new List<Material>();
        private bool _released;

        internal void Register(
            IEnumerable<Mesh> meshes,
            IEnumerable<Texture> textures,
            IEnumerable<Material> materials)
        {
            if (meshes != null)
            {
                foreach (Mesh mesh in meshes)
                {
                    if (mesh != null)
                    {
                        _meshes.Add(mesh);
                    }
                }
            }

            if (textures != null)
            {
                foreach (Texture texture in textures)
                {
                    if (texture != null)
                    {
                        _textures.Add(texture);
                    }
                }
            }

            if (materials != null)
            {
                foreach (Material material in materials)
                {
                    if (material != null)
                    {
                        _materials.Add(material);
                    }
                }
            }
        }

        private void OnDestroy()
        {
            ReleaseOwnedAssets();
        }

        /// <summary>
        /// Frees owned runtime assets. Safe to call more than once.
        /// </summary>
        internal void ReleaseOwnedAssets()
        {
            if (_released)
            {
                return;
            }

            _released = true;

            // DestroyImmediate: these are owned runtime assets (not scene objects). Using
            // Destroy would defer free until end-of-frame and leave a window where Destroy(GO)
            // has run but Mesh/Texture/Material still occupy memory.
            foreach (Mesh mesh in _meshes)
            {
                if (mesh != null)
                {
                    DestroyImmediate(mesh);
                }
            }

            _meshes.Clear();

            foreach (Texture texture in _textures)
            {
                if (texture != null)
                {
                    DestroyImmediate(texture);
                }
            }

            _textures.Clear();

            foreach (Material material in _materials)
            {
                if (material != null)
                {
                    DestroyImmediate(material);
                }
            }

            _materials.Clear();
        }
    }
}
