using UnityEditor;
using UnityEngine;

namespace JasmineYamo.ComponentAutoBind.Editor
{
    [CustomEditor(typeof(AutoBindGlobalSetting))]
    internal sealed class AutoBindGlobalSettingInspector : UnityEditor.Editor
    {
        private SerializedProperty m_Namespace;
        private SerializedProperty m_CodePath;
        private SerializedProperty m_UseGlobalDefaultSavePath;

        private void OnEnable()
        {
            m_Namespace = serializedObject.FindProperty("m_Namespace");
            m_CodePath = serializedObject.FindProperty("m_CodePath");
            m_UseGlobalDefaultSavePath = serializedObject.FindProperty("m_UseGlobalDefaultSavePath");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            m_Namespace.stringValue = EditorGUILayout.TextField(
                "Default Namespace",
                m_Namespace.stringValue);

            m_UseGlobalDefaultSavePath.boolValue = EditorGUILayout.Toggle(
                "Use Target Script Folder",
                m_UseGlobalDefaultSavePath.boolValue);

            EditorGUILayout.LabelField("Default Code Path");
            EditorGUILayout.LabelField(
                m_UseGlobalDefaultSavePath.boolValue ? string.Empty : m_CodePath.stringValue,
                EditorStyles.helpBox);

            if (GUILayout.Button("Choose Folder"))
            {
                string folderPath = EditorUtility.OpenFolderPanel(
                    "Choose Generated Code Folder",
                    Application.dataPath,
                    string.Empty);
                if (!string.IsNullOrWhiteSpace(folderPath))
                {
                    string assetPath = AutoBindPathUtility.NormalizeAssetPath(folderPath);
                    if (!string.IsNullOrWhiteSpace(assetPath)
                        && assetPath.StartsWith("Assets", System.StringComparison.OrdinalIgnoreCase))
                    {
                        m_CodePath.stringValue = assetPath;
                    }
                    else
                    {
                        EditorUtility.DisplayDialog(
                            "Invalid Folder",
                            "Generated code must be stored under the project Assets folder.",
                            "OK");
                    }
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
