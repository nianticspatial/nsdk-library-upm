// Copyright 2022-2026 Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.API;

namespace NianticSpatial.NSDK.AR.Subsystems
{
    internal interface IMeshDownloaderApi
    {
        public NsdkStatus NSDK_MeshDownloader_Create(IntPtr nsdkHandle);

        public NsdkStatus NSDK_MeshDownloader_Destroy(IntPtr nsdkHandle);

        public NsdkStatus NSDK_MeshDownloader_RequestLocationMesh(
            IntPtr nsdkHandle,
            NsdkString payload,
            bool getTexture,
            out ulong requestIdOut,
            UInt32 maxSizeKb);

        public NsdkStatus NSDK_MeshDownloader_GetLocationMeshResults(
            IntPtr nsdkHandle,
            ulong requestId,
            out MeshDownloadClient.NsdkMeshDownloaderResults resultsOut);

        public void NSDK_Release_Resource(IntPtr resource);
    }
}
