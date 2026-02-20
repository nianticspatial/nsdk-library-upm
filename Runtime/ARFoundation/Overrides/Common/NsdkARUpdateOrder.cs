// Copyright 2022-2026 Niantic Spatial.

namespace NianticSpatial.NSDK.AR.Common
{
    /// <summary>
    /// The update order for <c>MonoBehaviour</c>s in NSDK.
    /// </summary>
    public static class NsdkARUpdateOrder
    {
        /// <summary>
        /// The <see cref="ARSession"/>'s update order. Should come first.
        /// </summary>
        public const int Session = UnityEngine.XR.ARFoundation.ARUpdateOrder.k_Session;

        /// <summary>
        /// The <see cref="ARPersistentAnchorManager"/>'s update order.
        /// Should come after the <see cref="ARSession"/>.
        /// </summary>
        public const int PersistentAnchorManager = Session + 1;

        /// <summary>
        /// The <see cref="ARPersistentAnchor"/>'s update order.
        /// Should come after the <see cref="ARPersistentAnchorManager"/>.
        /// </summary>
        public const int PersistentAnchor = PersistentAnchorManager + 1;

        /// <summary>
        /// The <see cref="ARVps2Manager"/>'s update order.
        /// Should come after the <see cref="ARSession"/>.
        /// </summary>
        public const int Vps2Manager = Session + 1;

        /// <summary>
        /// The <see cref="ARScanningManager"/>'s update order.
        /// Should come after the <see cref="ARSession"/>.
        /// </summary>
        public const int ScanningManager = Session + 1;

        /// <summary>
        /// The <see cref="AROcclusionManager"/>'s update order.
        /// </summary>
        public const int OcclusionManager = UnityEngine.XR.ARFoundation.ARUpdateOrder.k_OcclusionManager;

        /// <summary>
        /// The <see cref="ARSemanticSegmentationManager"/>'s update order.
        /// Should come after the <see cref="AROcclusionManager"/> to ensure that the model choice is made by the
        /// occlusion manager before semantic segmentation starts.
        /// </summary>
        public const int SemanticSegmentationManager = OcclusionManager + 1;

        /// <summary>
        /// The <see cref="ARObjectDetectionManager"/>'s update order.
        /// Should come after the <see cref="ARSession"/>.
        /// </summary>
        public const int ObjectDetectionManager = Session + 1;
    }
}
