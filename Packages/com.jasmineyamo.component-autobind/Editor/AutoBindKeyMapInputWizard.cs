using UnityEditor;
using UnityEngine;

namespace JasmineYamo.ComponentAutoBind.Editor
{
    internal sealed class AutoBindKeyMapInputWizard : ScriptableWizard
    {
        public string Key;
        public string ComponentTypeName;

        public static void Open(string key, string componentTypeName)
        {
            AutoBindKeyMapInputWizard wizard = DisplayWizard<AutoBindKeyMapInputWizard>(
                "Add Component Key Mapping",
                "Add Mapping");
            wizard.Key = key;
            wizard.ComponentTypeName = componentTypeName;
            wizard.minSize = new Vector2(360f, 160f);
            wizard.maxSize = new Vector2(560f, 240f);
        }

        private void OnGUI()
        {
            Key = EditorGUILayout.TextField("Key", Key);
            EditorGUILayout.LabelField("Component Type", ComponentTypeName ?? string.Empty);

            if (GUILayout.Button("Add Mapping"))
            {
                OnWizardCreate();
            }
        }

        private void OnWizardCreate()
        {
            AutoBindKeyMapSetting setting = AssetDatabase.LoadAssetAtPath<AutoBindKeyMapSetting>(
                AutoBindSettingsPaths.KeyMapSettingAssetPath);
            if (setting == null)
            {
                Debug.LogError(
                    $"Create {nameof(AutoBindKeyMapSetting)} at {AutoBindSettingsPaths.KeyMapSettingAssetPath} first.");
                return;
            }

            Undo.RecordObject(setting, "Add Component Auto Bind Mapping");
            if (!setting.TryAddExtraMapping(Key, ComponentTypeName, out string error))
            {
                EditorUtility.DisplayDialog("Mapping Not Added", error, "OK");
                return;
            }

            EditorUtility.SetDirty(setting);
            AssetDatabase.SaveAssets();
            Close();
        }
    }
}
