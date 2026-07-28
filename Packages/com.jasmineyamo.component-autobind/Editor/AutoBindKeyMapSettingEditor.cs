using UnityEditor;

namespace JasmineYamo.ComponentAutoBind.Editor
{
    [CustomEditor(typeof(AutoBindKeyMapSetting))]
    internal sealed class AutoBindKeyMapSettingEditor : UnityEditor.Editor
    {
        private SerializedProperty m_ExtraComponentKeyMap;
        private bool m_ShowDefaultKeyMap = true;

        private void OnEnable()
        {
            m_ExtraComponentKeyMap = serializedObject.FindProperty("m_ExtraComponentKeyMap");
        }

        public override void OnInspectorGUI()
        {
            AutoBindKeyMapSetting setting = (AutoBindKeyMapSetting)target;
            serializedObject.Update();

            m_ShowDefaultKeyMap = EditorGUILayout.Foldout(
                m_ShowDefaultKeyMap,
                "Default Component Key Map",
                true);
            if (m_ShowDefaultKeyMap)
            {
                EditorGUI.indentLevel++;
                foreach (var item in setting.DefaultComponentKeyMap)
                {
                    EditorGUILayout.LabelField(item.Key, item.Value);
                }
                EditorGUI.indentLevel--;
            }

            EditorGUILayout.Space(4f);
            EditorGUILayout.PropertyField(m_ExtraComponentKeyMap, true);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
