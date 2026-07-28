using UnityEditor;
using UnityEngine;

namespace JasmineYamo.ComponentAutoBind.Editor
{
    internal sealed class DisplayOnlyAttribute : PropertyAttribute
    {
    }

    [CustomPropertyDrawer(typeof(DisplayOnlyAttribute))]
    internal sealed class ReadOnlyDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            bool previousEnabled = GUI.enabled;
            GUI.enabled = false;
            EditorGUI.PropertyField(position, property, label, true);
            GUI.enabled = previousEnabled;
        }
    }
}
