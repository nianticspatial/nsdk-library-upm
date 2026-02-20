using NianticSpatial.NSDK.AR.Loader;
using NianticSpatial.NSDK.AR.Utilities;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace NianticSpatial.NSDK.AR.Editor.Auth
{
    /// <summary>
    /// Class that sets up and saves the AuthBuildSettings asset before each build.
    /// </summary>
    public class AuthBuildPreprocessor : IPreprocessBuildWithReport
    {
        // This needs to execute before NsdkProjectValidationBuildPreprocess:
        public int callbackOrder => -1;

        public void OnPreprocessBuild(BuildReport report)
        {
            // Deploy all settings needed to AuthBuildSettings
            AuthEditorDeploySettingsCommand.Instance.Execute();

            // Save the build settings to disk before we start building (so that they will be included in the build)
            SettingsUtils.SaveImmediatelyInEditor(NsdkSettings.Instance.AuthBuildSettings);
        }
    }
}
