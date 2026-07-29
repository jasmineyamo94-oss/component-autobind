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
                StringAssert.Contains("IAutoBindHost", firstCode);
                StringAssert.Contains("IUiViewComponent", firstCode);
                StringAssert.Contains("public class UIView", firstCode);
                StringAssert.Contains("private UIView view;", firstCode);
                StringAssert.Contains(
                    "[global::System.CodeDom.Compiler.GeneratedCode(\"Component Auto Bind\", \"0.2.0\")]",
                    firstCode);
                StringAssert.Contains("global::UnityEngine.UI.Button", firstCode);
                StringAssert.DoesNotContain("IAutoBind" + "Target", firstCode);
                StringAssert.DoesNotContain("IAutoBind" + "ComponentSet", firstCode);
                StringAssert.DoesNotContain("AutoBind" + "ComponentSet", firstCode);
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

        [Test]
        public void GeneratedCodeOverridesVirtualEnsureAutoBindAndSupportsEmptyBindings()
        {
            GameObject root = new GameObject("InheritedEditorTestTarget");
            m_CreatedObjects.Add(root);
            InheritedEditorTestTarget targetScript = root.AddComponent<InheritedEditorTestTarget>();
            ComponentAutoBindTool tool = root.AddComponent<ComponentAutoBindTool>();
            tool.m_targetScript = targetScript;
            SetPrivateField(tool, "m_ClassName", nameof(InheritedEditorTestTarget));
            SetPrivateField(tool, "m_Namespace", typeof(InheritedEditorTestTarget).Namespace);
            tool.BindDatas = new List<ComponentAutoBindTool.BindData>();

            AutoBindGlobalSetting setting = CreateObject<AutoBindGlobalSetting>();
            string temporaryPath = Path.Combine(
                Path.GetTempPath(),
                "component-autobind-inherited-test-" + Guid.NewGuid().ToString("N"));
            SetPrivateField(setting, "m_CodePath", temporaryPath);

            try
            {
                string code = File.ReadAllText(InvokeCodeGenerator(tool, setting));

                StringAssert.Contains(
                    "InheritedEditorTestTarget : global::JasmineYamo.ComponentAutoBind.IAutoBindHost",
                    code);
                StringAssert.Contains("public override void EnsureAutoBind(GameObject go)", code);
                StringAssert.Contains("public class UIView", code);
                StringAssert.DoesNotContain("public new class UIView", code);
                StringAssert.Contains("private UIView view;", code);
                StringAssert.DoesNotContain("private new UIView view;", code);
                StringAssert.Contains("view = new UIView", code);
            }
            finally
            {
                if (Directory.Exists(temporaryPath))
                {
                    Directory.Delete(temporaryPath, true);
                }
            }
        }

        [Test]
        public void GeneratedCodeHidesInheritedUIViewAndViewField()
        {
            GameObject root = new GameObject("PlaceholderInheritedEditorTestTarget");
            m_CreatedObjects.Add(root);
            PlaceholderInheritedEditorTestTarget targetScript =
                root.AddComponent<PlaceholderInheritedEditorTestTarget>();
            ComponentAutoBindTool tool = root.AddComponent<ComponentAutoBindTool>();
            tool.m_targetScript = targetScript;
            SetPrivateField(tool, "m_ClassName", nameof(PlaceholderInheritedEditorTestTarget));
            SetPrivateField(
                tool,
                "m_Namespace",
                typeof(PlaceholderInheritedEditorTestTarget).Namespace);
            tool.BindDatas = new List<ComponentAutoBindTool.BindData>();

            AutoBindGlobalSetting setting = CreateObject<AutoBindGlobalSetting>();
            string temporaryPath = Path.Combine(
                Path.GetTempPath(),
                "component-autobind-placeholder-test-" + Guid.NewGuid().ToString("N"));
            SetPrivateField(setting, "m_CodePath", temporaryPath);

            try
            {
                string code = File.ReadAllText(InvokeCodeGenerator(tool, setting));

                StringAssert.Contains("public new class UIView", code);
                StringAssert.Contains("private new UIView view;", code);
                StringAssert.Contains("public override void EnsureAutoBind(GameObject go)", code);
            }
            finally
            {
                if (Directory.Exists(temporaryPath))
                {
                    Directory.Delete(temporaryPath, true);
                }
            }
        }

        [Test]
        public void ValidatorAllowsGeneratedUIViewEnsureAutoBindMethod()
        {
            GameObject root = new GameObject("GeneratedUIViewTarget");
            m_CreatedObjects.Add(root);
            GeneratedUIViewTarget targetScript = root.AddComponent<GeneratedUIViewTarget>();
            ComponentAutoBindTool tool = root.AddComponent<ComponentAutoBindTool>();
            tool.m_targetScript = targetScript;
            SetPrivateField(tool, "m_ClassName", nameof(GeneratedUIViewTarget));
            SetPrivateField(tool, "m_Namespace", typeof(GeneratedUIViewTarget).Namespace);

            AutoBindGlobalSetting setting = CreateObject<AutoBindGlobalSetting>();
            SetPrivateField(setting, "m_CodePath", "Assets/Generated/ComponentAutoBindTool");

            bool isValid = InvokeValidator(tool, setting, out string report);

            Assert.That(isValid, Is.True, report);
        }

        [Test]
        public void ValidatorAllowsLegacyGeneratedEnsureAutoBindShape()
        {
            GameObject root = new GameObject("LegacyGeneratedTarget");
            m_CreatedObjects.Add(root);
            LegacyGeneratedTarget targetScript = root.AddComponent<LegacyGeneratedTarget>();
            ComponentAutoBindTool tool = root.AddComponent<ComponentAutoBindTool>();
            tool.m_targetScript = targetScript;
            SetPrivateField(tool, "m_ClassName", nameof(LegacyGeneratedTarget));
            SetPrivateField(tool, "m_Namespace", typeof(LegacyGeneratedTarget).Namespace);

            AutoBindGlobalSetting setting = CreateObject<AutoBindGlobalSetting>();
            SetPrivateField(setting, "m_CodePath", "Assets/Generated/ComponentAutoBindTool");

            bool isValid = InvokeValidator(tool, setting, out string report);

            Assert.That(isValid, Is.True, report);
        }

        [Test]
        public void ValidatorRejectsUserDeclaredEnsureAutoBindMethod()
        {
            GameObject root = new GameObject("UserDeclaredEnsureTarget");
            m_CreatedObjects.Add(root);
            UserDeclaredEnsureTarget targetScript = root.AddComponent<UserDeclaredEnsureTarget>();
            ComponentAutoBindTool tool = root.AddComponent<ComponentAutoBindTool>();
            tool.m_targetScript = targetScript;
            SetPrivateField(tool, "m_ClassName", nameof(UserDeclaredEnsureTarget));
            SetPrivateField(tool, "m_Namespace", typeof(UserDeclaredEnsureTarget).Namespace);

            AutoBindGlobalSetting setting = CreateObject<AutoBindGlobalSetting>();
            SetPrivateField(setting, "m_CodePath", "Assets/Generated/ComponentAutoBindTool");

            bool isValid = InvokeValidator(tool, setting, out string report);

            Assert.That(isValid, Is.False);
            StringAssert.Contains(
                "The target type must not already declare EnsureAutoBind(GameObject)",
                report);
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

        private static bool InvokeValidator(
            ComponentAutoBindTool tool,
            AutoBindGlobalSetting setting,
            out string report)
        {
            Type validatorType = typeof(AutoBindGlobalSetting).Assembly.GetType(
                "JasmineYamo.ComponentAutoBind.Editor.AutoBindValidator");
            Assert.That(validatorType, Is.Not.Null);
            MethodInfo validateMethod = validatorType.GetMethod(
                "Validate",
                BindingFlags.Static | BindingFlags.Public);
            Assert.That(validateMethod, Is.Not.Null);

            object validationResult = validateMethod.Invoke(null, new object[] { tool, setting });
            Type resultType = validationResult.GetType();
            PropertyInfo isValidProperty = resultType.GetProperty(
                "IsValid",
                BindingFlags.Instance | BindingFlags.Public);
            MethodInfo buildReportMethod = resultType.GetMethod(
                "BuildReport",
                BindingFlags.Instance | BindingFlags.Public);
            Assert.That(isValidProperty, Is.Not.Null);
            Assert.That(buildReportMethod, Is.Not.Null);

            report = (string)buildReportMethod.Invoke(validationResult, null);
            return (bool)isValidProperty.GetValue(validationResult);
        }

        private sealed class EditorTestComponent : MonoBehaviour
        {
        }

        private sealed class EditorTestTarget : MonoBehaviour
        {
        }
    }

    public class AutoBindHostBase : MonoBehaviour, IAutoBindHost
    {
        public virtual void EnsureAutoBind(GameObject go)
        {
        }
    }

    public sealed class InheritedEditorTestTarget : AutoBindHostBase
    {
    }

    public class PlaceholderAutoBindHostBase : AutoBindHostBase
    {
        public class UIView : IUiViewComponent
        {
        }

        protected UIView view;
    }

    public sealed class PlaceholderInheritedEditorTestTarget : PlaceholderAutoBindHostBase
    {
    }

    public sealed class GeneratedUIViewTarget : MonoBehaviour, IAutoBindHost
    {
        [Serializable]
        public class UIView : IUiViewComponent
        {
        }

        private UIView view;

        public void EnsureAutoBind(GameObject go)
        {
            view = view ?? new UIView();
        }
    }

    public sealed class LegacyGeneratedTarget : MonoBehaviour, IAutoBindHost
    {
        [Serializable]
        public sealed class AutoBindComponentSet
        {
        }

        private AutoBindComponentSet m_AutoBindComponents;

        public void EnsureAutoBind(GameObject go)
        {
            m_AutoBindComponents = m_AutoBindComponents
                ?? new AutoBindComponentSet();
        }
    }

    public sealed class UserDeclaredEnsureTarget : MonoBehaviour, IAutoBindHost
    {
        public void EnsureAutoBind(GameObject go)
        {
        }
    }
}
