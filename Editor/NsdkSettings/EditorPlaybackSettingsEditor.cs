// Copyright 2022-2026 Niantic Spatial.

using NianticSpatial.NSDK.AR.Loader;

namespace NianticSpatial.NSDK.AR.Editor
{
    public class EditorPlaybackSettingsEditor : BasePlaybackSettingsEditor
    {
        protected override INsdkPlaybackSettings PlaybackSettings => NsdkSettings.Instance.EditorPlaybackSettings;

        /// <summary>
        /// Override to prevent EditorPlaybackSettings from being marked dirty and persisted.
        /// Editor settings are meant to be transient and session-specific.
        /// </summary>
        protected override void MarkSettingsDirty()
        {
            // Intentionally do nothing - EditorPlaybackSettings should not be persisted
        }
    }
}
