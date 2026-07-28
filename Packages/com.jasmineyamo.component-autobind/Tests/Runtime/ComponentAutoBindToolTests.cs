using System.Collections.Generic;
using System.Reflection;
using JasmineYamo.ComponentAutoBind;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;

namespace JasmineYamo.ComponentAutoBind.Tests.Runtime
{
    public sealed class ComponentAutoBindToolTests
    {
        private readonly List<GameObject> m_CreatedObjects = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            for (int i = m_CreatedObjects.Count - 1; i >= 0; i--)
            {
                if (m_CreatedObjects[i] != null)
                {
                    Object.DestroyImmediate(m_CreatedObjects[i]);
                }
            }

            m_CreatedObjects.Clear();
        }

        [Test]
        public void GetBindComponentByIndexReturnsStoredComponent()
        {
            ComponentAutoBindTool tool = CreateTool();
            Button button = CreateChild(tool, "Button").AddComponent<Button>();
            tool.m_BindComs.Add(button);

            Assert.That(tool.GetBindComponent<Button>(0), Is.SameAs(button));
        }

        [Test]
        public void GetBindComponentByFieldNameUsesFieldIndex()
        {
            ComponentAutoBindTool tool = CreateTool();
            Button firstButton = CreateChild(tool, "First").AddComponent<Button>();
            Button secondButton = CreateChild(tool, "Second").AddComponent<Button>();
            tool.m_BindComs.Add(firstButton);
            tool.m_BindComs.Add(secondButton);
            SetFieldNames(tool, "firstButton", "secondButton");

            Assert.That(
                tool.GetBindComponent<Button>(0, "secondButton"),
                Is.SameAs(secondButton));
        }

        [Test]
        public void GetBindComponentReturnsNullForNullComponent()
        {
            ComponentAutoBindTool tool = CreateTool();
            tool.m_BindComs.Add(null);
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("component reference is null"));

            Assert.That(tool.GetBindComponent<Button>(0), Is.Null);
        }

        [Test]
        public void GetBindComponentReturnsNullForInvalidIndex()
        {
            ComponentAutoBindTool tool = CreateTool();
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("outside the binding list"));

            Assert.That(tool.GetBindComponent<Button>(3), Is.Null);
        }

        [Test]
        public void GetBindComponentReturnsNullForWrongComponentType()
        {
            ComponentAutoBindTool tool = CreateTool();
            Transform transform = CreateChild(tool, "Transform").transform;
            tool.m_BindComs.Add(transform);
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("actual component type"));

            Assert.That(tool.GetBindComponent<Button>(0), Is.Null);
        }

        [Test]
        public void DeserializationRebuildsFieldNameCache()
        {
            ComponentAutoBindTool tool = CreateTool();
            Button firstButton = CreateChild(tool, "First").AddComponent<Button>();
            Button secondButton = CreateChild(tool, "Second").AddComponent<Button>();
            tool.m_BindComs.Add(firstButton);
            tool.m_BindComs.Add(secondButton);
            SetFieldNames(tool, "firstButton", "secondButton");

            Assert.That(tool.GetBindComponent<Button>(0, "secondButton"), Is.SameAs(secondButton));

            SetFieldNames(tool, "secondButton", "firstButton");
            ((ISerializationCallbackReceiver)tool).OnAfterDeserialize();

            Assert.That(tool.GetBindComponent<Button>(0, "secondButton"), Is.SameAs(firstButton));
        }

        private ComponentAutoBindTool CreateTool()
        {
            GameObject root = new GameObject("AutoBindRoot");
            m_CreatedObjects.Add(root);
            return root.AddComponent<ComponentAutoBindTool>();
        }

        private static GameObject CreateChild(ComponentAutoBindTool tool, string name)
        {
            GameObject child = new GameObject(name);
            child.transform.SetParent(tool.transform);
            return child;
        }

        private static void SetFieldNames(ComponentAutoBindTool tool, params string[] fieldNames)
        {
            FieldInfo field = typeof(ComponentAutoBindTool).GetField(
                "m_BindFieldNames",
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null);
            field.SetValue(tool, new List<string>(fieldNames));
        }
    }
}
