using System.IO;
using UnityEditor;
using UnityEngine;

namespace JasmineYamo.ComponentAutoBind.Editor
{
    internal static class AutoBindEditorAssetLocator
    {
        public static T LoadSingletonAsset<T>() where T : ScriptableObject
        {
            string[] assetGuids = AssetDatabase.FindAssets($"t:{typeof(T).Name}");
            if (assetGuids.Length == 0)
            {
                return null;
            }

            if (assetGuids.Length > 1)
            {
                Debug.LogError($"Found more than one {typeof(T).Name} asset. Keep one project setting asset.");
                return null;
            }

            string assetPath = AssetDatabase.GUIDToAssetPath(assetGuids[0]);
            return AssetDatabase.LoadAssetAtPath<T>(assetPath);
        }

        public static T CreateSingletonAsset<T>(string assetPath) where T : ScriptableObject
        {
            T existing = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (existing != null)
            {
                Selection.activeObject = existing;
                EditorGUIUtility.PingObject(existing);
                return existing;
            }

            string directory = Path.GetDirectoryName(assetPath)?.Replace('\\', '/');
            if (!string.IsNullOrWhiteSpace(directory) && !AssetDatabase.IsValidFolder(directory))
            {
                Directory.CreateDirectory(AutoBindPathUtility.ResolveProjectPath(directory));
                AssetDatabase.Refresh();
            }

            T setting = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(setting, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Selection.activeObject = setting;
            EditorGUIUtility.PingObject(setting);
            return setting;
        }
    }
}
