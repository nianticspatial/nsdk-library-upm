// Copyright Niantic Spatial.

using System;
using System.Runtime.InteropServices;
using NianticSpatial.NSDK.AR.API;
using NianticSpatial.NSDK.AR.Core;

namespace NianticSpatial.NSDK.AR.Sites.Api
{
    /// <summary>
    /// Native API bindings for Sites Manager.
    /// </summary>
    internal static class NativeSitesApi
    {
        // ============================================================================
        // Marshaled structs matching C API types
        // ============================================================================

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeOrganizationInfo
        {
            public IntPtr id;
            public IntPtr name;
            public IntPtr status;
            public long created_timestamp;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeSiteInfo
        {
            public IntPtr id;
            public IntPtr name;
            public IntPtr status;
            public IntPtr organization_id;
            public double latitude;
            public double longitude;
            [MarshalAs(UnmanagedType.U1)]
            public bool has_location;
            public IntPtr parent_site_id;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeAssetArea
        {
            // Field order must match ARDK_SitesManager_AssetArea exactly.
            public IntPtr area_id;
            public int job_status;  // ARDK_SitesManager_AssetPipelineJobStatus
            public IntPtr space_id;
            public IntPtr root_node_id;
            public IntPtr node_ids;  // const char**
            public int node_ids_size;
            public IntPtr anchor_payload;
            public double mesh_coverage;
            public double min_lat;
            public double min_lng;
            public double max_lat;
            public double max_lng;
            [MarshalAs(UnmanagedType.U1)]
            public bool has_bounds;
        }

        // --- Typed Asset Data Structs: deprecated, replaced by NativeAssetInfo.areas ---

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeAssetMeshData
        {
            public IntPtr root_node_id;
            public IntPtr node_ids;  // const char**
            public int node_ids_size;
            public double mesh_coverage;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeAssetSplatData
        {
            public IntPtr root_node_id;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeAssetVpsData
        {
            public IntPtr anchor_payload;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeAssetInfo
        {
            // Field order must match ARDK_SitesManager_AssetInfo exactly.

            // From assetKey
            public IntPtr id;
            public long asset_timestamp;

            // From the response root
            public IntPtr site_id;

            // From assetData
            public IntPtr name;
            public IntPtr description;
            public int asset_type;        // ARDK_SitesManager_AssetType
            public int asset_status;      // ARDK_SitesManager_AssetStatusType
            public int deployment;        // ARDK_SitesManager_AssetDeploymentType

            // Deprecated typed asset data pointers, projecting assetData.areas[0]
            public IntPtr mesh_data;      // ARDK_SitesManager_AssetMeshData*
            public IntPtr splat_data;     // ARDK_SitesManager_AssetSplatData*
            public IntPtr vps_data;       // ARDK_SitesManager_AssetVpsData*

            // All three are deprecated: nothing fills them. pipeline_job_status parses
            // assetData.jobStatus, which the portal leaves unset — areas carry the status.
            public IntPtr pipeline_job_id;
            public int pipeline_job_status;  // ARDK_SitesManager_AssetPipelineJobStatus
            public IntPtr source_scan_ids;   // const char**
            public int source_scan_ids_size;

            public IntPtr areas;          // ARDK_SitesManager_AssetArea*
            public int areas_size;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeUserInfo
        {
            public IntPtr id;
            public IntPtr first_name;
            public IntPtr last_name;
            public IntPtr email;
            public IntPtr status;
            public long created_timestamp;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeOrganizationResult
        {
            public int status;  // ARDK_SitesManager_RequestStatus
            public int error;   // ARDK_SitesManager_Error
            public IntPtr organizations;  // ARDK_SitesManager_OrganizationInfo*
            public int organizations_size;
            public IntPtr handle;  // ARDK_ResourceHandle
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeSiteResult
        {
            public int status;
            public int error;
            public IntPtr sites;  // ARDK_SitesManager_SiteInfo*
            public int sites_size;
            public IntPtr handle;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeAssetResult
        {
            public int status;
            public int error;
            public IntPtr assets;  // ARDK_SitesManager_AssetInfo*
            public int assets_size;
            public IntPtr handle;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeUserResult
        {
            public int status;
            public int error;
            public IntPtr user;  // ARDK_SitesManager_UserInfo* (single item or null)
            public IntPtr handle;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeSiteAssetsInfo
        {
            public NativeSiteInfo site;
            public IntPtr assets;      // ARDK_SitesManager_AssetInfo*
            public int assets_size;
            public double distance;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeSiteAssetsResult
        {
            public int status;
            public int error;
            public IntPtr site_assets;  // ARDK_SitesManager_SiteAssetsInfo*
            public int site_assets_size;
            public IntPtr handle;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeScanInfo
        {
            // Field order must match ARDK_SitesManager_ScanInfo exactly.
            public IntPtr id;
            public IntPtr name;
            public int scan_type;    // ARDK_SitesManager_ScanType
            public int scan_status;  // ARDK_SitesManager_ScanStatus
            public double duration_seconds;
            public long capture_timestamp;
            public long created_timestamp;
            public double latitude;
            public double longitude;
            [MarshalAs(UnmanagedType.U1)]
            public bool has_location;
            public long total_size;
            [MarshalAs(UnmanagedType.U1)]
            public bool has_thumbnail;
            [MarshalAs(UnmanagedType.U1)]
            public bool has_preview;
            public IntPtr user_id;
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct NativeScanResult
        {
            public int status;
            public int error;
            public IntPtr scans;  // ARDK_SitesManager_ScanInfo*
            public int scans_size;
            public IntPtr handle;
        }

        // ============================================================================
        // Lifecycle
        // ============================================================================

        [DllImport(NsdkPlugin.Name)]
        internal static extern NsdkStatus ARDK_SitesManager_Create(IntPtr nsdk_handle);

        [DllImport(NsdkPlugin.Name)]
        internal static extern NsdkStatus ARDK_SitesManager_Destroy(IntPtr nsdk_handle);

        // ============================================================================
        // Request methods (raw P/Invoke)
        // ============================================================================

        [DllImport(NsdkPlugin.Name)]
        private static extern NsdkStatus ARDK_SitesManager_RequestOrganizationsForUser(
            IntPtr nsdk_handle,
            NsdkString user_id,
            out ulong request_id_out);

        [DllImport(NsdkPlugin.Name)]
        private static extern NsdkStatus ARDK_SitesManager_RequestSitesForOrganization(
            IntPtr nsdk_handle,
            NsdkString org_id,
            out ulong request_id_out);

        [DllImport(NsdkPlugin.Name)]
        private static extern NsdkStatus ARDK_SitesManager_RequestAssetsForSite(
            IntPtr nsdk_handle,
            NsdkString site_id,
            out ulong request_id_out);

        [DllImport(NsdkPlugin.Name)]
        private static extern NsdkStatus ARDK_SitesManager_RequestOrganizationInfo(
            IntPtr nsdk_handle,
            NsdkString org_id,
            out ulong request_id_out);

        [DllImport(NsdkPlugin.Name)]
        private static extern NsdkStatus ARDK_SitesManager_RequestSiteInfo(
            IntPtr nsdk_handle,
            NsdkString site_id,
            out ulong request_id_out);

        [DllImport(NsdkPlugin.Name)]
        private static extern NsdkStatus ARDK_SitesManager_RequestAssetInfo(
            IntPtr nsdk_handle,
            NsdkString asset_id,
            out ulong request_id_out);

        [DllImport(NsdkPlugin.Name)]
        private static extern NsdkStatus ARDK_SitesManager_RequestUserInfo(
            IntPtr nsdk_handle,
            NsdkString user_id,
            out ulong request_id_out);

        [DllImport(NsdkPlugin.Name)]
        internal static extern NsdkStatus ARDK_SitesManager_RequestSelfUserInfo(
            IntPtr nsdk_handle,
            out ulong request_id_out);

        [DllImport(NsdkPlugin.Name)]
        internal static extern NsdkStatus ARDK_SitesManager_RequestSelfOrganizationInfo(
            IntPtr nsdk_handle,
            out ulong request_id_out);

        [DllImport(NsdkPlugin.Name)]
        private static extern NsdkStatus ARDK_SitesManager_RequestSiteAssetsByLocation(
            IntPtr nsdk_handle,
            double lat,
            double lng,
            double radius_meters,
            int asset_type,  // ARDK_SitesManager_AssetType
            int scope,  // ARDK_SitesManager_AssetScope
            out ulong request_id_out);

        [DllImport(NsdkPlugin.Name)]
        private static extern NsdkStatus ARDK_SitesManager_RequestScansForSite(
            IntPtr nsdk_handle,
            NsdkString site_id,
            out ulong request_id_out);

        [DllImport(NsdkPlugin.Name)]
        private static extern NsdkStatus ARDK_SitesManager_RequestScanInfo(
            IntPtr nsdk_handle,
            NsdkString scan_id,
            out ulong request_id_out);

        // ============================================================================
        // Wrapper methods that handle string marshaling
        // ============================================================================

        internal static NsdkStatus RequestOrganizationsForUser(IntPtr nsdkHandle, string userId, out ulong requestId)
        {
            using (var str = new ManagedNsdkString(userId))
            {
                return ARDK_SitesManager_RequestOrganizationsForUser(nsdkHandle, str.ToNsdkString(), out requestId);
            }
        }

        internal static NsdkStatus RequestSitesForOrganization(IntPtr nsdkHandle, string orgId, out ulong requestId)
        {
            using (var str = new ManagedNsdkString(orgId))
            {
                return ARDK_SitesManager_RequestSitesForOrganization(nsdkHandle, str.ToNsdkString(), out requestId);
            }
        }

        internal static NsdkStatus RequestAssetsForSite(IntPtr nsdkHandle, string siteId, out ulong requestId)
        {
            using (var str = new ManagedNsdkString(siteId))
            {
                return ARDK_SitesManager_RequestAssetsForSite(nsdkHandle, str.ToNsdkString(), out requestId);
            }
        }

        internal static NsdkStatus RequestOrganizationInfo(IntPtr nsdkHandle, string orgId, out ulong requestId)
        {
            using (var str = new ManagedNsdkString(orgId))
            {
                return ARDK_SitesManager_RequestOrganizationInfo(nsdkHandle, str.ToNsdkString(), out requestId);
            }
        }

        internal static NsdkStatus RequestSiteInfo(IntPtr nsdkHandle, string siteId, out ulong requestId)
        {
            using (var str = new ManagedNsdkString(siteId))
            {
                return ARDK_SitesManager_RequestSiteInfo(nsdkHandle, str.ToNsdkString(), out requestId);
            }
        }

        internal static NsdkStatus RequestAssetInfo(IntPtr nsdkHandle, string assetId, out ulong requestId)
        {
            using (var str = new ManagedNsdkString(assetId))
            {
                return ARDK_SitesManager_RequestAssetInfo(nsdkHandle, str.ToNsdkString(), out requestId);
            }
        }

        internal static NsdkStatus RequestUserInfo(IntPtr nsdkHandle, string userId, out ulong requestId)
        {
            using (var str = new ManagedNsdkString(userId))
            {
                return ARDK_SitesManager_RequestUserInfo(nsdkHandle, str.ToNsdkString(), out requestId);
            }
        }

        internal static NsdkStatus RequestSelfUserInfo(IntPtr nsdkHandle, out ulong requestId)
        {
            return ARDK_SitesManager_RequestSelfUserInfo(nsdkHandle, out requestId);
        }

        internal static NsdkStatus RequestSelfOrganizationInfo(IntPtr nsdkHandle, out ulong requestId)
        {
            return ARDK_SitesManager_RequestSelfOrganizationInfo(nsdkHandle, out requestId);
        }

        internal static NsdkStatus RequestSiteAssetsByLocation(
            IntPtr nsdkHandle, double lat, double lng, double radiusMeters, AssetType assetType,
            AssetScope scope, out ulong requestId)
        {
            return ARDK_SitesManager_RequestSiteAssetsByLocation(
                nsdkHandle, lat, lng, radiusMeters, (int)assetType, (int)scope, out requestId);
        }

        internal static NsdkStatus RequestScansForSite(IntPtr nsdkHandle, string siteId, out ulong requestId)
        {
            using (var str = new ManagedNsdkString(siteId))
            {
                return ARDK_SitesManager_RequestScansForSite(nsdkHandle, str.ToNsdkString(), out requestId);
            }
        }

        internal static NsdkStatus RequestScanInfo(IntPtr nsdkHandle, string scanId, out ulong requestId)
        {
            using (var str = new ManagedNsdkString(scanId))
            {
                return ARDK_SitesManager_RequestScanInfo(nsdkHandle, str.ToNsdkString(), out requestId);
            }
        }

        // ============================================================================
        // Result polling methods
        // ============================================================================

        [DllImport(NsdkPlugin.Name)]
        internal static extern NsdkStatus ARDK_SitesManager_GetOrganizationResult(
            IntPtr nsdk_handle,
            ulong request_id,
            out NativeOrganizationResult result_out);

        [DllImport(NsdkPlugin.Name)]
        internal static extern NsdkStatus ARDK_SitesManager_GetSiteResult(
            IntPtr nsdk_handle,
            ulong request_id,
            out NativeSiteResult result_out);

        [DllImport(NsdkPlugin.Name)]
        internal static extern NsdkStatus ARDK_SitesManager_GetAssetResult(
            IntPtr nsdk_handle,
            ulong request_id,
            out NativeAssetResult result_out);

        [DllImport(NsdkPlugin.Name)]
        internal static extern NsdkStatus ARDK_SitesManager_GetUserResult(
            IntPtr nsdk_handle,
            ulong request_id,
            out NativeUserResult result_out);

        [DllImport(NsdkPlugin.Name)]
        internal static extern NsdkStatus ARDK_SitesManager_GetSiteAssetsResult(
            IntPtr nsdk_handle,
            ulong request_id,
            out NativeSiteAssetsResult result_out);

        [DllImport(NsdkPlugin.Name)]
        internal static extern NsdkStatus ARDK_SitesManager_GetScanResult(
            IntPtr nsdk_handle,
            ulong request_id,
            out NativeScanResult result_out);

        // ============================================================================
        // Conversion helpers
        // ============================================================================

        internal static string PtrToString(IntPtr ptr)
        {
            if (ptr == IntPtr.Zero) return null;
            return Marshal.PtrToStringAnsi(ptr);
        }

        internal static OrganizationInfo ConvertOrganization(NativeOrganizationInfo native)
        {
            return new OrganizationInfo(
                PtrToString(native.id) ?? "",
                PtrToString(native.name) ?? "",
                PtrToString(native.status) ?? "",
                native.created_timestamp
            );
        }

        internal static SiteInfo ConvertSite(NativeSiteInfo native)
        {
            return new SiteInfo(
                PtrToString(native.id) ?? "",
                PtrToString(native.name) ?? "",
                PtrToString(native.status) ?? "",
                PtrToString(native.organization_id) ?? "",
                native.latitude,
                native.longitude,
                native.has_location,
                PtrToString(native.parent_site_id)
            );
        }

        internal static AssetInfo ConvertAsset(NativeAssetInfo native)
        {
            // Convert source scan IDs array
            string[] sourceScanIds = Array.Empty<string>();
            if (native.source_scan_ids != IntPtr.Zero && native.source_scan_ids_size > 0)
            {
                sourceScanIds = new string[native.source_scan_ids_size];
                for (int i = 0; i < native.source_scan_ids_size; i++)
                {
                    IntPtr strPtr = Marshal.ReadIntPtr(native.source_scan_ids, i * IntPtr.Size);
                    sourceScanIds[i] = PtrToString(strPtr) ?? "";
                }
            }

            // Convert enums
            var assetType = (AssetType)native.asset_type;
            var assetStatus = (AssetStatusType)native.asset_status;
            var deployment = (AssetDeploymentType)native.deployment;
            var pipelineJobStatus = (AssetPipelineJobStatus)native.pipeline_job_status;

            // Convert typed asset data based on asset type
            AssetMeshData? meshData = null;
            AssetSplatData? splatData = null;
            AssetVpsData? vpsData = null;

            switch (assetType)
            {
                case AssetType.Mesh when native.mesh_data != IntPtr.Zero:
                    var nativeMesh = Marshal.PtrToStructure<NativeAssetMeshData>(native.mesh_data);
                    var nodeIds = ConvertStringArray(nativeMesh.node_ids, nativeMesh.node_ids_size);
                    meshData = new AssetMeshData(
                        PtrToString(nativeMesh.root_node_id) ?? "",
                        nodeIds,
                        nativeMesh.mesh_coverage
                    );
                    break;

                case AssetType.Splat when native.splat_data != IntPtr.Zero:
                    var nativeSplat = Marshal.PtrToStructure<NativeAssetSplatData>(native.splat_data);
                    splatData = new AssetSplatData(
                        PtrToString(nativeSplat.root_node_id) ?? ""
                    );
                    break;

                case AssetType.VpsInfo when native.vps_data != IntPtr.Zero:
                    var nativeVps = Marshal.PtrToStructure<NativeAssetVpsData>(native.vps_data);
                    vpsData = new AssetVpsData(
                        PtrToString(nativeVps.anchor_payload) ?? ""
                    );
                    break;
            }

            var areas = ConvertAreas(native.areas, native.areas_size);

            return new AssetInfo(
                id: PtrToString(native.id) ?? "",
                assetTimestamp: native.asset_timestamp,
                siteId: PtrToString(native.site_id) ?? "",
                name: PtrToString(native.name) ?? "",
                description: PtrToString(native.description),
                assetType: assetType,
                assetStatus: assetStatus,
                deployment: deployment,
                meshData: meshData,
                splatData: splatData,
                vpsData: vpsData,
                pipelineJobId: PtrToString(native.pipeline_job_id),
                pipelineJobStatus: pipelineJobStatus,
                sourceScanIds: sourceScanIds,
                areas: areas
            );
        }

        internal static AssetArea[] ConvertAreas(IntPtr ptr, int count)
        {
            if (ptr == IntPtr.Zero || count <= 0)
            {
                return Array.Empty<AssetArea>();
            }

            var areas = new AssetArea[count];
            int structSize = Marshal.SizeOf<NativeAssetArea>();
            for (int i = 0; i < count; i++)
            {
                var native = Marshal.PtrToStructure<NativeAssetArea>(IntPtr.Add(ptr, i * structSize));
                areas[i] = new AssetArea(
                    areaId: PtrToString(native.area_id) ?? "",
                    jobStatus: (AssetPipelineJobStatus)native.job_status,
                    spaceId: PtrToString(native.space_id) ?? "",
                    rootNodeId: PtrToString(native.root_node_id) ?? "",
                    nodeIds: ConvertStringArray(native.node_ids, native.node_ids_size),
                    anchorPayload: PtrToString(native.anchor_payload) ?? "",
                    meshCoverage: native.mesh_coverage,
                    minLat: native.min_lat,
                    minLng: native.min_lng,
                    maxLat: native.max_lat,
                    maxLng: native.max_lng,
                    hasBounds: native.has_bounds
                );
            }

            return areas;
        }

        internal static string[] ConvertStringArray(IntPtr ptr, int count)
        {
            if (ptr == IntPtr.Zero || count <= 0) return Array.Empty<string>();

            var result = new string[count];
            for (int i = 0; i < count; i++)
            {
                IntPtr strPtr = Marshal.ReadIntPtr(ptr, i * IntPtr.Size);
                result[i] = PtrToString(strPtr) ?? "";
            }
            return result;
        }

        internal static UserInfo ConvertUser(NativeUserInfo native)
        {
            return new UserInfo(
                PtrToString(native.id) ?? "",
                PtrToString(native.first_name) ?? "",
                PtrToString(native.last_name) ?? "",
                PtrToString(native.email) ?? "",
                PtrToString(native.status) ?? "",
                native.created_timestamp
            );
        }

        internal static OrganizationInfo[] ConvertOrganizations(IntPtr ptr, int count)
        {
            if (ptr == IntPtr.Zero || count <= 0) return Array.Empty<OrganizationInfo>();

            var result = new OrganizationInfo[count];
            int structSize = Marshal.SizeOf<NativeOrganizationInfo>();
            for (int i = 0; i < count; i++)
            {
                var native = Marshal.PtrToStructure<NativeOrganizationInfo>(IntPtr.Add(ptr, i * structSize));
                result[i] = ConvertOrganization(native);
            }
            return result;
        }

        internal static SiteInfo[] ConvertSites(IntPtr ptr, int count)
        {
            if (ptr == IntPtr.Zero || count <= 0) return Array.Empty<SiteInfo>();

            var result = new SiteInfo[count];
            int structSize = Marshal.SizeOf<NativeSiteInfo>();
            for (int i = 0; i < count; i++)
            {
                var native = Marshal.PtrToStructure<NativeSiteInfo>(IntPtr.Add(ptr, i * structSize));
                result[i] = ConvertSite(native);
            }
            return result;
        }

        internal static AssetInfo[] ConvertAssets(IntPtr ptr, int count)
        {
            if (ptr == IntPtr.Zero || count <= 0) return Array.Empty<AssetInfo>();

            var result = new AssetInfo[count];
            int structSize = Marshal.SizeOf<NativeAssetInfo>();
            for (int i = 0; i < count; i++)
            {
                var native = Marshal.PtrToStructure<NativeAssetInfo>(IntPtr.Add(ptr, i * structSize));
                result[i] = ConvertAsset(native);
            }
            return result;
        }

        internal static SiteAssetsInfo ConvertSiteAssetsInfo(NativeSiteAssetsInfo native)
        {
            var site = ConvertSite(native.site);
            var assets = ConvertAssets(native.assets, native.assets_size);
            return new SiteAssetsInfo(site, assets, native.distance);
        }

        internal static SiteAssetsInfo[] ConvertSiteAssets(IntPtr ptr, int count)
        {
            if (ptr == IntPtr.Zero || count <= 0) return Array.Empty<SiteAssetsInfo>();

            var result = new SiteAssetsInfo[count];
            int structSize = Marshal.SizeOf<NativeSiteAssetsInfo>();
            for (int i = 0; i < count; i++)
            {
                var native = Marshal.PtrToStructure<NativeSiteAssetsInfo>(IntPtr.Add(ptr, i * structSize));
                result[i] = ConvertSiteAssetsInfo(native);
            }
            return result;
        }

        internal static ScanInfo ConvertScan(NativeScanInfo native)
        {
            return new ScanInfo(
                id: PtrToString(native.id) ?? "",
                name: PtrToString(native.name) ?? "",
                scanType: Enum.IsDefined(typeof(ScanType), native.scan_type)
                    ? (ScanType)native.scan_type
                    : ScanType.Unspecified,
                scanStatus: Enum.IsDefined(typeof(ScanStatus), native.scan_status)
                    ? (ScanStatus)native.scan_status
                    : ScanStatus.Unspecified,
                durationSeconds: native.duration_seconds,
                captureTimestamp: native.capture_timestamp,
                createdTimestamp: native.created_timestamp,
                latitude: native.latitude,
                longitude: native.longitude,
                hasLocation: native.has_location,
                totalSize: native.total_size,
                hasThumbnail: native.has_thumbnail,
                hasPreview: native.has_preview,
                userId: PtrToString(native.user_id) ?? ""
            );
        }

        internal static ScanInfo[] ConvertScans(IntPtr ptr, int count)
        {
            if (ptr == IntPtr.Zero || count <= 0) return Array.Empty<ScanInfo>();

            var result = new ScanInfo[count];
            int structSize = Marshal.SizeOf<NativeScanInfo>();
            for (int i = 0; i < count; i++)
            {
                var native = Marshal.PtrToStructure<NativeScanInfo>(IntPtr.Add(ptr, i * structSize));
                result[i] = ConvertScan(native);
            }
            return result;
        }
    }
}
