using UnityEditor;
using UnityEngine;

namespace JasmineYamo.ComponentAutoBind.Editor
{
    internal static class AutoBindSettingsPaths
    {
        public const string SettingsFolder = "Assets/Settings/ComponentAutoBindTool";
        public const string GlobalSettingAssetPath = SettingsFolder + "/AutoBindGlobalSetting.asset";
        public const string KeyMapSettingAssetPath = SettingsFolder + "/AutoBindKeyMapSetting.asset";
        public const string DefaultCodePath = "Assets/Generated/ComponentAutoBindTool";
    }

    /// <summary>
    /// Project-level settings for generated component binding code.
    /// </summary>
    public sealed class AutoBindGlobalSetting : ScriptableObject
    {
        [SerializeField]
        private string m_CodePath = AutoBindSettingsPaths.DefaultCodePath;

        [SerializeField]
        private string m_Namespace = string.Empty;

        [SerializeField]
        private bool m_UseGlobalDefaultSavePath;

        public string CodePath => m_CodePath;
        public string Namespace => m_Namespace;
        public bool UseGlobalDefaultSavePath => m_UseGlobalDefaultSavePath;

        [MenuItem("Tools/Component Auto Bind/Create Global Settings")]
        private static void CreateAutoBindGlobalSetting()
        {
            AutoBindEditorAssetLocator.CreateSingletonAsset<AutoBindGlobalSetting>(
                AutoBindSettingsPaths.GlobalSettingAssetPath);
        }
    }
}
