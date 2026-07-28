using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace JasmineYamo.ComponentAutoBind.Editor
{
    [Serializable]
    public sealed class AutoBindComponentKeyMapEntry
    {
        public string Key;
        public string ComponentTypeName;
    }

    /// <summary>
    /// Maps hierarchy name prefixes to component types used by the scanner.
    /// </summary>
    public sealed class AutoBindKeyMapSetting : ScriptableObject
    {
        private static readonly IReadOnlyDictionary<string, string> s_DefaultComponentKeyMap =
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                { "Tf", "UnityEngine.Transform" },
                { "OAni", "UnityEngine.Animation" },
                { "NAni", "UnityEngine.Animator" },
                { "Rtf", "UnityEngine.RectTransform" },
                { "Cav", "UnityEngine.Canvas" },
                { "CGroup", "UnityEngine.CanvasGroup" },
                { "VLGroup", "UnityEngine.UI.VerticalLayoutGroup" },
                { "HLGroup", "UnityEngine.UI.HorizontalLayoutGroup" },
                { "GLGroup", "UnityEngine.UI.GridLayoutGroup" },
                { "TGroup", "UnityEngine.UI.ToggleGroup" },
                { "Btn", "UnityEngine.UI.Button" },
                { "Img", "UnityEngine.UI.Image" },
                { "RImg", "UnityEngine.UI.RawImage" },
                { "Txt", "UnityEngine.UI.Text" },
                { "Inf", "UnityEngine.UI.InputField" },
                { "Sld", "UnityEngine.UI.Slider" },
                { "Mask", "UnityEngine.UI.Mask" },
                { "Mask2D", "UnityEngine.UI.RectMask2D" },
                { "Tog", "UnityEngine.UI.Toggle" },
                { "Sbr", "UnityEngine.UI.Scrollbar" },
                { "SRect", "UnityEngine.UI.ScrollRect" },
                { "Drop", "UnityEngine.UI.Dropdown" }
            };

        [SerializeField]
        private List<AutoBindComponentKeyMapEntry> m_ExtraComponentKeyMap =
            new List<AutoBindComponentKeyMapEntry>();

        public IReadOnlyDictionary<string, string> DefaultComponentKeyMap => s_DefaultComponentKeyMap;
        public List<AutoBindComponentKeyMapEntry> ExtraComponentKeyMap => m_ExtraComponentKeyMap;

        public bool TryGetComponentTypeName(string key, out string componentTypeName)
        {
            if (s_DefaultComponentKeyMap.TryGetValue(key, out componentTypeName))
            {
                return true;
            }

            if (m_ExtraComponentKeyMap == null)
            {
                componentTypeName = null;
                return false;
            }

            for (int i = 0; i < m_ExtraComponentKeyMap.Count; i++)
            {
                AutoBindComponentKeyMapEntry entry = m_ExtraComponentKeyMap[i];
                if (entry != null && string.Equals(entry.Key, key, StringComparison.Ordinal))
                {
                    componentTypeName = entry.ComponentTypeName;
                    return true;
                }
            }

            componentTypeName = null;
            return false;
        }

        public IEnumerable<KeyValuePair<string, string>> GetAllComponentKeyMaps()
        {
            foreach (KeyValuePair<string, string> item in s_DefaultComponentKeyMap.OrderBy(pair => pair.Key))
            {
                yield return item;
            }

            if (m_ExtraComponentKeyMap == null)
            {
                yield break;
            }

            for (int i = 0; i < m_ExtraComponentKeyMap.Count; i++)
            {
                AutoBindComponentKeyMapEntry entry = m_ExtraComponentKeyMap[i];
                if (entry == null || string.IsNullOrWhiteSpace(entry.Key))
                {
                    continue;
                }

                yield return new KeyValuePair<string, string>(entry.Key, entry.ComponentTypeName);
            }
        }

        public bool TryAddExtraMapping(string key, string componentTypeName, out string error)
        {
            key = key?.Trim();
            componentTypeName = componentTypeName?.Trim();
            if (string.IsNullOrWhiteSpace(key))
            {
                error = "The component key cannot be empty.";
                return false;
            }

            if (key.IndexOf('_') >= 0 || key.IndexOfAny(new[] { ' ', '\t', '\r', '\n' }) >= 0)
            {
                error = "The component key cannot contain underscores or whitespace.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(componentTypeName))
            {
                error = "The component type name cannot be empty.";
                return false;
            }

            if ((m_ExtraComponentKeyMap != null
                && m_ExtraComponentKeyMap.Any(entry => entry != null && entry.Key == key))
                || s_DefaultComponentKeyMap.ContainsKey(key))
            {
                error = $"The component key '{key}' already exists.";
                return false;
            }

            if (!AutoBindComponentTypeResolver.TryResolve(componentTypeName, out Type componentType)
                || !typeof(Component).IsAssignableFrom(componentType))
            {
                error = $"'{componentTypeName}' is not a resolvable Unity Component type.";
                return false;
            }

            if (m_ExtraComponentKeyMap == null)
            {
                m_ExtraComponentKeyMap = new List<AutoBindComponentKeyMapEntry>();
            }

            m_ExtraComponentKeyMap.Add(new AutoBindComponentKeyMapEntry
            {
                Key = key,
                ComponentTypeName = componentType.FullName
            });
            error = null;
            return true;
        }

        [MenuItem("Tools/Component Auto Bind/Create Key Map Settings")]
        private static void CreateAutoBindKeyMapSetting()
        {
            AutoBindEditorAssetLocator.CreateSingletonAsset<AutoBindKeyMapSetting>(
                AutoBindSettingsPaths.KeyMapSettingAssetPath);
        }
    }
}
