// Copyright 2022-2026 Niantic Spatial.
using System;
using System.Runtime.InteropServices;
using NianticSpatial.NSDK.AR.Utilities.Logging;
using Random = UnityEngine.Random;

namespace NianticSpatial.NSDK.AR.PAM
{
    internal class MockApi : IApi
    {
        public IntPtr Handle
        {
            get => _handle;
        }

        private IntPtr _handle;

        private DataFormatFlags _readyDataFormats = DataFormatFlags.kNone;

        private bool _isLidarDepthEnabled = false;

        public IntPtr ARDK_SAH_Create(IntPtr unityContext, bool isLidarDepthEnabled)
        {
            _handle = new IntPtr((int)(Random.value * int.MaxValue));
            _isLidarDepthEnabled = isLidarDepthEnabled;
            return _handle;
        }

        public virtual void ARDK_SAH_OnFrame(IntPtr handle, IntPtr frameData)
        {
            if (handle == _handle)
            {
                Log.Info("Forwarded frame data");
            }
        }

        public void ARDK_SAH_Release(IntPtr handle)
        {
            if (handle == _handle)
            {
                _handle = IntPtr.Zero;
            }
        }

        public void ARDK_SAH_GetDataFormatsReadyForNewFrame
        (
            IntPtr handle,
            out uint dataFormatsReady
        )
        {
            dataFormatsReady = (uint)_readyDataFormats;
            ClearDataFormats();
        }

        public void ARDK_SAH_GetDispatchedFormatsToModules
        (
            IntPtr handle,
            out uint dispatchedFrameId,
            out ulong dispatchedToModules,
            out uint dispatchedDataFormats
        )
        {
            dispatchedFrameId = 0;
            dispatchedToModules = 0;
            dispatchedDataFormats = 0;
        }

        public void MarkDataFormatsReady(int size, DataFormatFlags formats)
        {
            _readyDataFormats = formats;
        }

        private void ClearDataFormats()
        {
            _readyDataFormats = DataFormatFlags.kNone;
        }

        /// <summary>
        /// Marshals the pointer to NsdkFrameData (ARDK_FrameData layout).
        /// </summary>
        public static NsdkFrameData IntPtrToFrameDataCStruct(IntPtr ptr)
        {
            return (NsdkFrameData)Marshal.PtrToStructure(ptr, typeof(NsdkFrameData));
        }

        /// <summary>
        /// Captures frame data including camera and depth frames. Must be called from within
        /// the ARDK_SAH_OnFrame callback while the frame data pointers are still valid,
        /// since CameraFrames and DepthFrames point to stack-allocated arrays that become
        /// invalid after the callback returns.
        /// </summary>
        public static (NsdkFrameData FrameData, NsdkCameraFrameCStruct? CameraFrame, NsdkDepthFrameCStruct? DepthFrame) CaptureFrameData(IntPtr ptr)
        {
            var frameData = (NsdkFrameData)Marshal.PtrToStructure(ptr, typeof(NsdkFrameData));
            NsdkCameraFrameCStruct? cameraFrame = null;
            NsdkDepthFrameCStruct? depthFrame = null;
            if (frameData.CameraFramesCount > 0 && frameData.CameraFrames != IntPtr.Zero)
                cameraFrame = (NsdkCameraFrameCStruct)Marshal.PtrToStructure(frameData.CameraFrames, typeof(NsdkCameraFrameCStruct));
            if (frameData.DepthFramesCount > 0 && frameData.DepthFrames != IntPtr.Zero)
                depthFrame = (NsdkDepthFrameCStruct)Marshal.PtrToStructure(frameData.DepthFrames, typeof(NsdkDepthFrameCStruct));
            return (frameData, cameraFrame, depthFrame);
        }

        /// <summary>
        /// Returns the first camera frame when present. For use by tests and tools.
        /// Note: Only use with frame data captured via CaptureFrameData; GetFirstCameraFrame
        /// dereferences pointers that may be invalid if the original frame was stack-allocated.
        /// </summary>
        public static NsdkCameraFrameCStruct? GetFirstCameraFrame(NsdkFrameData frameData)
        {
            if (frameData.CameraFramesCount == 0 || frameData.CameraFrames == IntPtr.Zero)
                return null;
            return (NsdkCameraFrameCStruct)Marshal.PtrToStructure(frameData.CameraFrames, typeof(NsdkCameraFrameCStruct));
        }

        /// <summary>
        /// Returns the first depth frame when present. For use by tests and tools.
        /// Note: Only use with frame data captured via CaptureFrameData; GetFirstDepthFrame
        /// dereferences pointers that may be invalid if the original frame was stack-allocated.
        /// </summary>
        public static NsdkDepthFrameCStruct? GetFirstDepthFrame(NsdkFrameData frameData)
        {
            if (frameData.DepthFramesCount == 0 || frameData.DepthFrames == IntPtr.Zero)
                return null;
            return (NsdkDepthFrameCStruct)Marshal.PtrToStructure(frameData.DepthFrames, typeof(NsdkDepthFrameCStruct));
        }
    }
}
