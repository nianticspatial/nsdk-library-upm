// Copyright 2022-2026 Niantic Spatial.

using System;
using System.ComponentModel;

namespace NianticSpatial.NSDK.AR.Utilities.Preloading
{
    /// <summary>
    /// The NSDK depth model to use.
    /// </summary>
    /// <remarks>
    /// This is analogous to Unity's <see cref="UnityEngine.XR.ARSubsystems.EnvironmentDepthMode"/>.
    /// </remarks>
    [PublicAPI]
    public enum DepthMode : byte
    {
        /// <summary>
        /// The default model will be used, if applicable.
        /// </summary>
        [Description("No model specified")]
        Unspecified = 0,

        [Obsolete("For custom model files, please register to one of the existing modes (Fast, Medium or Smooth).")]
        Custom = 1,

        /// <summary>
        /// Depth will be generated at the fastest resolution.
        /// </summary>
        [Description("Fast")]
        Fast = 2,

        /// <summary>
        /// Depth will be generated at a medium resolution.
        /// </summary>
        [Description("Medium")]
        Medium = 3,

        /// <summary>
        /// Depth will be generated at the best resolution.
        /// </summary>
        [Description("Smooth")]
        Smooth = 4,

    }

    /// <summary>
    /// The NSDK semantic segmentation model to use.
    /// </summary>
    [PublicAPI]
    public enum SemanticsMode : byte
    {
        /// <summary>
        /// The default model will be used, if applicable.
        /// </summary>
        [Description("No model specified")]
        Unspecified = 0,

        [Obsolete("For custom model files, please register to one of the existing modes (Fast, Medium or Smooth).")]
        Custom = 1,

        /// <summary>
        /// Semantic segmentation will be generated at the fastest resolution.
        /// </summary>
        [Description("Fast")]
        Fast = 2,

        /// <summary>
        /// Semantic segmentation will be generated at a medium resolution.
        /// </summary>
        [Description("Medium")]
        Medium = 3,

        /// <summary>
        /// Semantic segmentation will be generated at the best resolution.
        /// </summary>
        [Description("Smooth")]
        Smooth = 4,
    }

    /// <summary>
    /// The NSDK Scanning SQC model to use.
    /// </summary>
    [PublicAPI]
    public enum ScanningSQCMode : byte
    {
        /// <summary>
        /// The default model will be used.
        /// </summary>
        [Description("Default model")]
        Default = 0,
    }

    /// <summary>
    /// The NSDK object detection model to use.
    /// </summary>
    [PublicAPI]
    public enum ObjectDetectionMode : byte
    {
        /// <summary>
        /// The default model will be used.
        /// </summary>
        [Description("Default model")]
        Default = 0,
    }
}
