// Copyright 2022-2026 Niantic Spatial.

using System;
using NianticSpatial.NSDK.AR.Settings;
using UnityEngine;

namespace NianticSpatial.NSDK.AR.VpsCoverage
{
    [Serializable]
    internal class LocalizationTargetsRequest
    {
        [SerializeField]
        private string[] query_id;

        [SerializeField]
        internal LegacyMetadataHelper.ARCommonMetadataStruct ar_common_metadata;

        public LocalizationTargetsRequest(string[] queryId, LegacyMetadataHelper.ARCommonMetadataStruct arCommonMetadata)
        {
            query_id = queryId;
            ar_common_metadata = arCommonMetadata;
        }
    }
}
