using UnityEditor;
using UnityEngine;

namespace JasmineYamo.ComponentAutoBind.Editor
{
    internal static class AutoBindTargetScriptSyncUtility
    {
        public static bool SyncFromTargetScript(
            SerializedProperty targetScriptProperty,
            SerializedProperty namespaceProperty,
            SerializedProperty classNameProperty,
            SerializedProperty codePathProperty,
            AutoBindGlobalSetting setting)
        {
            if (targetScriptProperty == null || targetScriptProperty.objectReferenceValue == null)
            {
                return false;
            }

            MonoBehaviour targetScript = targetScriptProperty.objectReferenceValue as MonoBehaviour;
            if (targetScript == null)
            {
                Debug.LogError("The target script must be a MonoBehaviour.");
                return false;
            }

            System.Type scriptType = targetScript.GetType();
            MonoScript monoScript = MonoScript.FromMonoBehaviour(targetScript);
            string scriptPath = monoScript == null ? string.Empty : AssetDatabase.GetAssetPath(monoScript);

            namespaceProperty.stringValue = scriptType.Namespace ?? string.Empty;
            classNameProperty.stringValue = scriptType.Name;

            string codePath = AutoBindPathUtility.GetInspectorCodePath(setting, scriptPath);
            if (string.IsNullOrEmpty(codePath))
            {
                Debug.LogError("Unable to resolve the Component Auto Bind code path.");
                return false;
            }

            codePathProperty.stringValue = codePath;
            return true;
        }
    }
}
