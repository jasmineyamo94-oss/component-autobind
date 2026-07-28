using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using JasmineYamo.ComponentAutoBind;
using JasmineYamo.ComponentAutoBind.Editor;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

namespace JasmineYamo.ComponentAutoBind.Tests.Editor
{
    public sealed class ComponentAutoBindEditorTests
    {
        private readonly List<UnityEngine.Object> m_CreatedObjects = new List<UnityEngine.Object>();

        [TearDown]
        public void TearDown()
        {
            for (int i = m_CreatedObjects.Count - 1; i >= 0; i--)
            {
                if (m_CreatedObjects[i] != null)
                {
                    UnityEngine.Object.DestroyImmediate(m_CreatedObjects[i]);
                }
            }

            m_CreatedObjects.Clear();
        }

        [Test]
        public void DefaultKeyMapContainsCoreUguiMappings()
        {
            AutoBindKeyMapSetting setting = CreateObject<AutoBindKeyMapSetting>();

            Assert.That(setting.TryGetComponentTypeName("Btn", out string buttonType), Is.True);
            Assert.That(buttonType, Is.EqualTo("UnityEngine.UI.Button"));
            Assert.That(setting.TryGetComponentTypeName("Txt", out string textType), Is.True);
            Assert.That(textType, Is.EqualTo("UnityEngine.UI.Text"));
        }

        [Test]
        public void CustomMappingStoresResolvedComponentFullName()
        {
            AutoBindKeyMapSetting setting = CreateObject<AutoBindKeyMapSetting>();

            Assert.That(
                setting.TryAddExtraMapping("Marker", typeof(EditorTestComponent).FullName, out string error),
                Is.True,
                error);
            Assert.That(setting.TryGetComponentTypeName("Marker", out string typeName), Is.True);
            Assert.That(typeName, Is.EqualTo(typeof(EditorTestComponent).FullName));
        }

        [Test]
        public void CustomMappingRejectsDuplicatesAndInvalidTypes()
        {
            AutoBindKeyMapSetting setting = CreateObject<AutoBindKeyMapSetting>();
            Assert.That(setting.TryAddExtraMapping("Btn", typeof(Button).FullName, out _), Is.False);
            Assert.That(setting.TryAddExtraMapping("Marker", "Not.A.Component", out _), Is.False);
            Assert.That(setting.TryAddExtraMapping("Bad_Key", typeof(Button).FullName, out _), Is.False);
        }

        [Test]
        public void PathUtilityKeepsGeneratedCodeInsideAssets()
        {
            Type utilityType = typeof(AutoBindGlobalSetting).Assembly.GetType(
                "JasmineYamo.ComponentAutoBind.Editor.AutoBindPathUtility");
            Assert.That(utilityType, Is.Not.Null);

            MethodInfo resolveMethod = utilityType.GetMethod(
                "ResolveProjectPath",
                BindingFlags.Static | BindingFlags.Public);
            MethodInfo isUnderAssetsMethod = utilityType.GetMethod(
                "IsPathUnderAssets",
                BindingFlags.Static | BindingFlags.Public);
            Assert.That(resolveMethod, Is.Not.Null);
            Assert.That(isUnderAssetsMethod, Is.Not.Null);

            string insidePath = (string)resolveMethod.Invoke(null, new object[] { "Assets/Generated/AutoBind" });
            string outsidePath = (string)resolveMethod.Invoke(
                null,
                new object[] { Path.Combine(Path.GetTempPath(), "Generated") });

            Assert.That((bool)isUnderAssetsMethod.Invoke(null, new object[] { insidePath }), Is.True);
            Assert.That((bool)isUnderAssetsMethod.Invoke(null, new object[] { outsidePath }), Is.False);
        }

        [Test]
        public void GeneratedCodeContainsPublicContractsAndIsDeterministic()
        {
            GameObject root = new GameObject("GeneratedTarget");
            m_CreatedObjects.Add(root);
            EditorTestTarget targetScript = root.AddComponent<EditorTestTarget>();
            ComponentAutoBindTool tool = root.AddComponent<ComponentAutoBindTool>();
            tool.m_targetScript = targetScript;
            SetPrivateField(tool, "m_ClassName", nameof(EditorTestTarget));
            SetPrivateField(tool, "m_Namespace", typeof(EditorTestTarget).Namespace);

            GameObject buttonObject = new GameObject("Button");
            m_CreatedObjects.Add(buttonObject);
            buttonObject.transform.SetParent(root.transform);
            Button button = buttonObject.AddComponent<Button>();
            tool.BindDatas = new List<ComponentAutoBindTool.BindData>
            {
                new ComponentAutoBindTool.BindData("Btn_Submit", button)
            };

            AutoBindGlobalSetting setting = CreateObject<AutoBindGlobalSetting>();
            string temporaryPath = Path.Combine(
                Path.GetTempPath(),
                "component-autobind-editor-test-" + Guid.NewGuid().ToString("N"));
            SetPrivateField(setting, "m_CodePath", temporaryPath);

            try
            {
                string firstPath = InvokeCodeGenerator(tool, setting);
                string firstCode = File.ReadAllText(firstPath);
                string secondPath = InvokeCodeGenerator(tool, setting);
                string secondCode = File.ReadAllText(secondPath);

                Assert.That(firstPath, Is.EqualTo(secondPath));
                Assert.That(secondCode, Is.EqualTo(firstCode));
                StringAssert.Contains("IAutoBindTarget", firstCode);
                StringAssert.Contains("IAutoBindComponentSet", firstCode);
                StringAssert.Contains("AutoBindComponentSet", firstCode);
                StringAssert.Contains("global::UnityEngine.UI.Button", firstCode);
                StringAssert.DoesNotContain("UI" + "View", firstCode);
                StringAssert.DoesNotContain("DateTime" + "." + "Now", firstCode);
            }
            finally
            {
                if (Directory.Exists(temporaryPath))
                {
                    Directory.Delete(temporaryPath, true);
                }
            }
        }

        private T CreateObject<T>() where T : ScriptableObject
        {
            T value = ScriptableObject.CreateInstance<T>();
            m_CreatedObjects.Add(value);
            return value;
        }

        private static void SetPrivateField(object target, string fieldName, object value)
        {
            FieldInfo field = target.GetType().GetField(
                fieldName,
                BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, fieldName);
            field.SetValue(target, value);
        }

        private static string InvokeCodeGenerator(ComponentAutoBindTool tool, AutoBindGlobalSetting setting)
        {
            Type generatorType = typeof(AutoBindGlobalSetting).Assembly.GetType(
                "JasmineYamo.ComponentAutoBind.Editor.AutoBindCodeGenerator");
            Assert.That(generatorType, Is.Not.Null);
            MethodInfo generateMethod = generatorType.GetMethod(
                "Generate",
                BindingFlags.Static | BindingFlags.Public);
            Assert.That(generateMethod, Is.Not.Null);
            return (string)generateMethod.Invoke(null, new object[] { tool, setting });
        }

        private sealed class EditorTestComponent : MonoBehaviour
        {
        }

        private sealed class EditorTestTarget : MonoBehaviour
        {
        }
    }
}
