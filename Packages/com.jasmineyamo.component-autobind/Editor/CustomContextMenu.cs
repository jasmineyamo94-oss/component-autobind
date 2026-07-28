using UnityEditor;
using UnityEngine;

namespace JasmineYamo.ComponentAutoBind.Editor
{
    internal static class CustomContextMenu
    {
        [MenuItem("CONTEXT/MonoBehaviour/Add Component Auto Bind Tool", false, 0)]
        private static void AddAutoBindComponentTool(MenuCommand command)
        {
            MonoBehaviour target = command.context as MonoBehaviour;
            if (target == null || target is ComponentAutoBindTool)
            {
                return;
            }

            ComponentAutoBindTool tool = target.GetComponent<ComponentAutoBindTool>();
            if (tool == null)
            {
                Undo.AddComponent<ComponentAutoBindTool>(target.gameObject);
                tool = target.GetComponent<ComponentAutoBindTool>();
            }

            Undo.RecordObject(tool, "Set Auto Bind Target Script");
            tool.m_targetScript = target;
            EditorUtility.SetDirty(tool);
        }

        [MenuItem("CONTEXT/MonoBehaviour/Add Component Auto Bind Tool", true)]
        private static bool ValidateAddAutoBindComponentTool(MenuCommand command)
        {
            MonoBehaviour target = command.context as MonoBehaviour;
            return target != null && !(target is ComponentAutoBindTool);
        }

        [MenuItem("CONTEXT/MonoBehaviour/Add Component To Auto Bind Key Map", false, 1)]
        private static void AddComponentToAutoBindKeyMap(MenuCommand command)
        {
            MonoBehaviour target = command.context as MonoBehaviour;
            if (target == null || target is ComponentAutoBindTool)
            {
                return;
            }

            string key = AutoBindFieldNameUtility.BuildKeyPrefix(target.GetType().Name);
            AutoBindKeyMapInputWizard.Open(key, target.GetType().FullName);
        }

        [MenuItem("CONTEXT/MonoBehaviour/Add Component To Auto Bind Key Map", true)]
        private static bool ValidateAddComponentToAutoBindKeyMap(MenuCommand command)
        {
            MonoBehaviour target = command.context as MonoBehaviour;
            return target != null && !(target is ComponentAutoBindTool);
        }
    }
}
