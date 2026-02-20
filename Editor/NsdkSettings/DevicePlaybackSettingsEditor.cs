// Copyright 2022-2026 Niantic Spatial.

using NianticSpatial.NSDK.AR.Loader;
using UnityEditor;
using UnityEngine;

namespace NianticSpatial.NSDK.AR.Editor
{
    public class DevicePlaybackSettingsEditor : BasePlaybackSettingsEditor
    {
        protected override INsdkPlaybackSettings PlaybackSettings => NsdkSettings.Instance.DevicePlaybackSettings;
    }
}
