using System;
using System.Collections.Generic;
using UnityEngine;
using Component = UnityEngine.Component;

namespace JasmineYamo.ComponentAutoBind
{
    /// <summary>
    /// Stores component references and provides generated binding lookups.
    /// </summary>
    public class ComponentAutoBindTool : MonoBehaviour, ISerializationCallbackReceiver
    {
#if UNITY_EDITOR
        [Serializable]
        public class BindData
        {
            public BindData()
            {
            }

            public BindData(string name, Component bindCom)
            {
                Name = name;
                BindCom = bindCom;
            }

            public string Name;
            public Component BindCom;
        }

        public List<BindData> BindDatas = new List<BindData>();

        [SerializeField] private string m_ClassName;

        [SerializeField] private string m_Namespace;

        public MonoBehaviour m_targetScript;

        [SerializeField] private string m_CodePath;

        public string ClassName
        {
            get { return m_ClassName; }
        }

        public string Namespace
        {
            get { return m_Namespace; }
        }

        public string CodePath
        {
            get { return m_CodePath; }
        }

#endif

        [SerializeField] public List<Component> m_BindComs = new List<Component>();
        [SerializeField] private List<string> m_BindFieldNames = new List<string>();
        [NonSerialized] private Dictionary<string, int> m_BindFieldIndexMap;
        [NonSerialized] private bool m_BindFieldIndexMapDirty = true;

        public T GetBindComponent<T>(int index) where T : Component
        {
            string fieldName = GetFieldName(index);
            return GetBindComponentInternal<T>(index, fieldName);
        }

        public T GetBindComponent<T>(int index, string fieldName) where T : Component
        {
            if (!string.IsNullOrWhiteSpace(fieldName))
            {
                int fieldIndex = FindBindIndex(fieldName);
                if (fieldIndex >= 0)
                {
                    return GetBindComponentInternal<T>(fieldIndex, fieldName);
                }

                if (m_BindComs != null && index >= 0 && index < m_BindComs.Count)
                {
                    Debug.LogWarning(
                        $"[{name}] Auto Bind field '{fieldName}' was not found; falling back to index {index}.",
                        this);
                }
            }

            return GetBindComponentInternal<T>(index, fieldName);
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
            InvalidateRuntimeCaches();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            InvalidateRuntimeCaches();
        }
#endif

        private T GetBindComponentInternal<T>(int index, string requestedFieldName) where T : Component
        {
            int bindCount = m_BindComs != null ? m_BindComs.Count : 0;
            if (index < 0 || index >= bindCount)
            {
                Debug.LogError(BuildBindError(index, requestedFieldName, typeof(T),
                    $"index is outside the binding list (count: {bindCount})"));
                return null;
            }

            Component rawComponent = m_BindComs[index];
            if (rawComponent == null)
            {
                Debug.LogError(BuildBindError(index, requestedFieldName, typeof(T), "the component reference is null"));
                return null;
            }

            T bindCom = rawComponent as T;

            if (bindCom == null)
            {
                Debug.LogError(BuildBindError(index, requestedFieldName, typeof(T),
                    $"the actual component type is {rawComponent.GetType().FullName}"));
                return null;
            }

            return bindCom;
        }

        private int FindBindIndex(string fieldName)
        {
            EnsureBindFieldIndexMap();

            if (m_BindFieldIndexMap != null
                && m_BindFieldIndexMap.TryGetValue(fieldName, out int fieldIndex))
            {
                return fieldIndex;
            }

            return -1;
        }

        private void EnsureBindFieldIndexMap()
        {
            if (!m_BindFieldIndexMapDirty && m_BindFieldIndexMap != null)
            {
                return;
            }

            if (m_BindFieldIndexMap == null)
            {
                int fieldCount = m_BindFieldNames != null ? m_BindFieldNames.Count : 0;
                m_BindFieldIndexMap = new Dictionary<string, int>(fieldCount, StringComparer.Ordinal);
            }
            else
            {
                m_BindFieldIndexMap.Clear();
            }

            if (m_BindFieldNames == null)
            {
                m_BindFieldIndexMapDirty = false;
                return;
            }

            for (int i = 0; i < m_BindFieldNames.Count; i++)
            {
                string fieldName = m_BindFieldNames[i];
                if (string.IsNullOrWhiteSpace(fieldName) || m_BindFieldIndexMap.ContainsKey(fieldName))
                {
                    continue;
                }

                m_BindFieldIndexMap.Add(fieldName, i);
            }

            m_BindFieldIndexMapDirty = false;
        }

        private void InvalidateRuntimeCaches()
        {
            m_BindFieldIndexMapDirty = true;
            m_BindFieldIndexMap = null;
        }

        private string GetFieldName(int index)
        {
            if (m_BindFieldNames != null
                && index >= 0
                && index < m_BindFieldNames.Count
                && !string.IsNullOrWhiteSpace(m_BindFieldNames[index]))
            {
                return m_BindFieldNames[index];
            }

            return "<unknown>";
        }

        private string BuildBindError(int index, string requestedFieldName, Type expectedType, string reason)
        {
            string fieldName = !string.IsNullOrWhiteSpace(requestedFieldName)
                ? requestedFieldName
                : GetFieldName(index);

            return
                $"[{name}] Auto Bind component lookup failed. Field={fieldName}, "
                + $"Index={index}, Expected={expectedType.FullName}, Reason={reason}";
        }
    }
}
