// Copyright 2026 Niantic Spatial.

using System;
using System.Runtime.InteropServices;
using NianticSpatial.NSDK.AR.PAM;

namespace NianticSpatial.NSDK.AR.Subsystems.Playback
{
    /// <summary>
    /// One frame's recorded data as native playback reports it, copied out of native memory.
    /// <see cref="CameraPose"/> is in NSDK's convention.
    /// </summary>
    internal readonly struct PlaybackFrameRecord
    {
        public readonly uint Sequence;
        public readonly ulong TimestampMs;
        public readonly uint TrackingState;
        public readonly TransformCStruct CameraPose;
        public readonly CameraIntrinsicsCStruct Intrinsics;
        public readonly uint ImageWidth;
        public readonly uint ImageHeight;
        public readonly GpsLocationCStruct Gps;
        public readonly CompassDataCStruct Compass;
        public readonly uint DepthWidth;
        public readonly uint DepthHeight;

        public bool HasDepth => DepthWidth > 0 && DepthHeight > 0;

        private PlaybackFrameRecord(in NsdkFrameData frame, in NsdkCameraFrameCStruct camera,
            uint depthWidth, uint depthHeight)
        {
            Sequence = frame.FrameId;
            TimestampMs = camera.CameraTimestampMs;
            TrackingState = frame.TrackingState;
            CameraPose = camera.CameraPose;
            Intrinsics = camera.CameraIntrinsics;
            ImageWidth = camera.CameraImageWidth;
            ImageHeight = camera.CameraImageHeight;
            Gps = frame.GpsLocation;
            Compass = frame.CompassData;
            DepthWidth = depthWidth;
            DepthHeight = depthHeight;
        }

        /// <summary>Copies <paramref name="frame"/>'s first camera and depth frame headers.</summary>
        public static PlaybackFrameRecord From(in NsdkFrameData frame)
        {
            if (frame.CameraFrames == IntPtr.Zero || frame.CameraFramesCount == 0)
            {
                throw new InvalidOperationException($"Playback frame {frame.FrameId} has no camera frame");
            }

            var camera = Marshal.PtrToStructure<NsdkCameraFrameCStruct>(frame.CameraFrames);
            uint depthWidth = 0, depthHeight = 0;
            if (frame.DepthFrames != IntPtr.Zero && frame.DepthFramesCount > 0)
            {
                var depth = Marshal.PtrToStructure<NsdkDepthFrameCStruct>(frame.DepthFrames);
                depthWidth = depth.DepthImageDataWidth;
                depthHeight = depth.DepthImageDataHeight;
            }

            return new PlaybackFrameRecord(frame, camera, depthWidth, depthHeight);
        }
    }

    /// <summary>One frame's image file bytes and raw depth and confidence buffers.</summary>
    internal readonly struct PlaybackFrameBuffers
    {
        /// <summary>Complete JPEG file.</summary>
        public readonly byte[] Image;
        /// <summary>Float32 meters, row-major.</summary>
        public readonly byte[] Depth;
        /// <summary>One byte per depth pixel.</summary>
        public readonly byte[] Confidence;

        private PlaybackFrameBuffers(byte[] image, byte[] depth, byte[] confidence)
        {
            Image = image;
            Depth = depth;
            Confidence = confidence;
        }

        /// <summary>Copies <paramref name="frame"/>'s image and depth bytes, which native frees on the next fetch.</summary>
        public static PlaybackFrameBuffers From(in NsdkFrameData frame)
        {
            byte[] image = null, depth = null, confidence = null;
            if (frame.CameraFrames != IntPtr.Zero && frame.CameraFramesCount > 0)
            {
                var camera = Marshal.PtrToStructure<NsdkCameraFrameCStruct>(frame.CameraFrames);
                image = Copy(camera.CameraImagePlane0.DataPtr, (int)camera.CameraImagePlane0.DataSize);
            }

            if (frame.DepthFrames != IntPtr.Zero && frame.DepthFramesCount > 0)
            {
                var depthFrame = Marshal.PtrToStructure<NsdkDepthFrameCStruct>(frame.DepthFrames);
                var pixels = (int)depthFrame.DepthAndConfidenceDataLength;
                depth = Copy(depthFrame.DepthImageData, pixels * sizeof(float));
                confidence = Copy(depthFrame.DepthConfidenceData, pixels);
            }

            return new PlaybackFrameBuffers(image, depth, confidence);
        }

        private static byte[] Copy(IntPtr pointer, int count)
        {
            if (pointer == IntPtr.Zero || count <= 0)
            {
                return null;
            }

            var bytes = new byte[count];
            Marshal.Copy(pointer, bytes, 0, count);
            return bytes;
        }
    }
}
