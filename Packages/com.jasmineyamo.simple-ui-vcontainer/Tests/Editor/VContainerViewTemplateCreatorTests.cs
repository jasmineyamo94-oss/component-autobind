using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using JasmineYamo.ComponentAutoBind;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using VContainer;
using VContainer.Unity;
using SimpleUIEditor = JasmineYamo.SimpleUI.VContainer.Editor;

namespace JasmineYamo.SimpleUI.VContainer.Tests
{
    public sealed class VContainerViewTemplateCreatorTests
    {
        private const string ViewTemplatePath =
            "Packages/com.jasmineyamo.simple-ui-vcontainer/"
            + "Editor/Templates/VContainerView.cs.txt";
        private const string PresenterTemplatePath =
            "Packages/com.jasmineyamo.simple-ui-vcontainer/"
            + "Editor/Templates/VContainerViewPresenter.cs.txt";

        [Test]
        public void MenuUsesTopLevelPathAndExpectedPriority()
        {
            MethodInfo createMethod = typeof(
                    SimpleUIEditor.VContainerViewTemplateCreator)
                .GetMethod(
                    "CreateVContainerView",
                    BindingFlags.Static | BindingFlags.NonPublic);
            Assert.That(createMethod, Is.Not.Null);

            var menuItem = createMethod.GetCustomAttribute<MenuItem>();
            Assert.That(menuItem, Is.Not.Null);
            Assert.That(
                menuItem.menuItem,
                Is.EqualTo("Assets/Create/C# Scripts VContainer View"));
            Assert.That(menuItem.priority, Is.EqualTo(80));
            Assert.That(
                menuItem.menuItem,
                Is.Not.EqualTo("Assets/Create/C# Scripts/VContainer View"));
        }

        [Test]
        public void MenuAppearsAboveCSharpScript()
        {
            string[] menuPaths = GetCreateMenuPaths();
            int folderIndex = Array.IndexOf(menuPaths, "Assets/Create/Folder");
            int csharpIndex = Array.IndexOf(menuPaths, "Assets/Create/C# Script");
            int viewIndex = Array.IndexOf(
                menuPaths,
                SimpleUIEditor.VContainerViewTemplateCreator.MenuPath);

            Assert.That(folderIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(csharpIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(viewIndex, Is.GreaterThan(folderIndex));
            Assert.That(viewIndex, Is.LessThan(csharpIndex));
        }

        [TestCase("AutoBindTest", "AutoBindTestView", "AutoBindTestPresenter")]
        [TestCase("AutoBindTestView", "AutoBindTestView", "AutoBindTestPresenter")]
        [TestCase(
            "AutoBindViewTestView",
            "AutoBindViewTestView",
            "AutoBindViewTestPresenter")]
        public void BuildNamesNormalizesOneTrailingViewSuffix(
            string requestedName,
            string expectedViewName,
            string expectedPresenterName)
        {
            bool success = SimpleUIEditor.VContainerViewTemplateCreator.TryBuildNames(
                requestedName,
                out SimpleUIEditor.VContainerViewTemplateNames names,
                out string error);

            Assert.That(success, Is.True, error);
            Assert.That(names.ViewClassName, Is.EqualTo(expectedViewName));
            Assert.That(names.PresenterClassName, Is.EqualTo(expectedPresenterName));
            Assert.That(names.ViewFileName, Is.EqualTo(expectedViewName + ".cs"));
            Assert.That(
                names.PresenterFileName,
                Is.EqualTo(expectedPresenterName + ".cs"));
        }

        [Test]
        public void DefaultDialogNamePreviewsTwoScripts()
        {
            bool success = SimpleUIEditor.VContainerViewTemplateCreator.TryBuildNames(
                SimpleUIEditor.VContainerViewCreateWindow.DefaultName,
                out SimpleUIEditor.VContainerViewTemplateNames names,
                out string error);

            Assert.That(success, Is.True, error);
            Assert.That(names.ViewFileName, Is.EqualTo("NewView.cs"));
            Assert.That(names.PresenterFileName, Is.EqualTo("NewPresenter.cs"));
        }

        [Test]
        public void ActiveFolderResolverReturnsValidProjectFolder()
        {
            bool success =
                SimpleUIEditor.VContainerViewTemplateCreator.TryGetActiveFolderPath(
                    out string directoryAssetPath,
                    out string error);

            Assert.That(success, Is.True, error);
            Assert.That(AssetDatabase.IsValidFolder(directoryAssetPath), Is.True);
        }

        [TestCase("")]
        [TestCase("View")]
        [TestCase("classView")]
        [TestCase("123View")]
        [TestCase("Invalid-Name")]
        public void BuildNamesRejectsInvalidIdentifiers(string requestedName)
        {
            bool success = SimpleUIEditor.VContainerViewTemplateCreator.TryBuildNames(
                requestedName,
                out _,
                out string error);

            Assert.That(success, Is.False);
            Assert.That(error, Is.Not.Empty);
        }

        [Test]
        public void TemplatesRenderWithoutNamespace()
        {
            var names = new SimpleUIEditor.VContainerViewTemplateNames(
                "AutoBindTestView",
                "AutoBindTestPresenter");

            string viewCode = SimpleUIEditor.VContainerViewTemplateCreator.RenderTemplate(
                LoadTemplate(ViewTemplatePath),
                names,
                string.Empty);
            string presenterCode = SimpleUIEditor.VContainerViewTemplateCreator.RenderTemplate(
                LoadTemplate(PresenterTemplatePath),
                names,
                string.Empty);

            StringAssert.Contains(
                "public partial class AutoBindTestView : ViewLifetimeScope",
                viewCode);
            StringAssert.Contains(
                "RegisterCommon<AutoBindTestPresenter>(builder, view);",
                viewCode);
            StringAssert.Contains(
                "public sealed class AutoBindTestPresenter : IStartable",
                presenterCode);
            StringAssert.Contains(
                "AutoBindTestView.UIView view,\n        ViewBundle viewBundle)",
                presenterCode);
            StringAssert.Contains(
                "using JasmineYamo.SimpleUI.VContainer;",
                viewCode);
            StringAssert.Contains(
                "using JasmineYamo.SimpleUI.VContainer;",
                presenterCode);
            string legacyNamespace =
                "JasmineYamo.ComponentAutoBind." + "ViewCore";
            StringAssert.DoesNotContain(
                legacyNamespace,
                viewCode);
            StringAssert.DoesNotContain(
                legacyNamespace,
                presenterCode);
            StringAssert.DoesNotContain("namespace ", viewCode);
            StringAssert.DoesNotContain("#VIEW_NAME#", viewCode);
            StringAssert.DoesNotContain("#PRESENTER_NAME#", presenterCode);
        }

        [Test]
        public void TemplatesRenderInsideConfiguredNamespace()
        {
            var names = new SimpleUIEditor.VContainerViewTemplateNames(
                "AutoBindTestView",
                "AutoBindTestPresenter");

            string viewCode = SimpleUIEditor.VContainerViewTemplateCreator.RenderTemplate(
                LoadTemplate(ViewTemplatePath),
                names,
                "Example.Views");

            StringAssert.Contains("namespace Example.Views\n{", viewCode);
            StringAssert.Contains(
                "    public partial class AutoBindTestView : ViewLifetimeScope",
                viewCode);
            Assert.That(
                SimpleUIEditor.VContainerViewTemplateCreator.IsValidNamespace("Example.Views"),
                Is.True);
            Assert.That(
                SimpleUIEditor.VContainerViewTemplateCreator.IsValidNamespace("Example..Views"),
                Is.False);
        }

        [Test]
        public void DirectWriteCreatesExactlyTwoUtf8Scripts()
        {
            string temporaryPath = CreateTemporaryDirectory();
            try
            {
                string viewPath = Path.Combine(temporaryPath, "AutoBindTestView.cs");
                string presenterPath =
                    Path.Combine(temporaryPath, "AutoBindTestPresenter.cs");

                bool success =
                    SimpleUIEditor.VContainerViewTemplateCreator.TryWriteScriptsDirectly(
                        viewPath,
                        "public class AutoBindTestView {}\n",
                        presenterPath,
                        "public class AutoBindTestPresenter {}\n",
                        out string error);

                Assert.That(success, Is.True, error);
                Assert.That(File.Exists(viewPath), Is.True);
                Assert.That(File.Exists(presenterPath), Is.True);
                Assert.That(Directory.GetFiles(temporaryPath), Has.Length.EqualTo(2));
                Assert.That(
                    File.Exists(Path.Combine(temporaryPath, "NewView.cs")),
                    Is.False);
                Assert.That(File.ReadAllText(viewPath), Does.Contain("AutoBindTestView"));
                byte[] bytes = File.ReadAllBytes(viewPath);
                Assert.That(
                    bytes.Length < 3
                    || bytes[0] != 0xEF
                    || bytes[1] != 0xBB
                    || bytes[2] != 0xBF,
                    Is.True,
                    "Generated scripts must use UTF-8 without BOM.");
            }
            finally
            {
                Directory.Delete(temporaryPath, true);
            }
        }

        [Test]
        public void DirectWriteDoesNotCreateOrOverwriteOnConflict()
        {
            string temporaryPath = CreateTemporaryDirectory();
            try
            {
                string viewPath = Path.Combine(temporaryPath, "AutoBindTestView.cs");
                string presenterPath =
                    Path.Combine(temporaryPath, "AutoBindTestPresenter.cs");
                File.WriteAllText(presenterPath, "existing");

                bool success =
                    SimpleUIEditor.VContainerViewTemplateCreator.TryWriteScriptsDirectly(
                        viewPath,
                        "view",
                        presenterPath,
                        "presenter",
                        out string error);

                Assert.That(success, Is.False);
                Assert.That(File.Exists(viewPath), Is.False);
                Assert.That(File.ReadAllText(presenterPath), Is.EqualTo("existing"));
                StringAssert.Contains("already exists", error);
            }
            finally
            {
                Directory.Delete(temporaryPath, true);
            }
        }

        [Test]
        public void DirectWriteRollsBackFirstScriptWhenSecondWriteFails()
        {
            string temporaryPath = CreateTemporaryDirectory();
            try
            {
                string viewPath = Path.Combine(temporaryPath, "AutoBindTestView.cs");
                string presenterPath = Path.Combine(
                    temporaryPath,
                    "Missing",
                    "AutoBindTestPresenter.cs");

                bool success =
                    SimpleUIEditor.VContainerViewTemplateCreator.TryWriteScriptsDirectly(
                        viewPath,
                        "view",
                        presenterPath,
                        "presenter",
                        out string error);

                Assert.That(success, Is.False);
                Assert.That(error, Is.Not.Empty);
                Assert.That(File.Exists(viewPath), Is.False);
                Assert.That(File.Exists(presenterPath), Is.False);
                Assert.That(Directory.GetFiles(temporaryPath), Is.Empty);
            }
            finally
            {
                Directory.Delete(temporaryPath, true);
            }
        }

        private static string LoadTemplate(string assetPath)
        {
            TextAsset template = AssetDatabase.LoadAssetAtPath<TextAsset>(assetPath);
            Assert.That(template, Is.Not.Null, assetPath);
            return template.text;
        }

        private static string[] GetCreateMenuPaths()
        {
            Type menuType =
                typeof(UnityEditor.Editor).Assembly.GetType("UnityEditor.Menu");
            Type menuItemType =
                typeof(UnityEditor.Editor).Assembly.GetType(
                    "UnityEditor.ScriptingMenuItem");
            Assert.That(menuType, Is.Not.Null);
            Assert.That(menuItemType, Is.Not.Null);

            MethodInfo getMenuItems = menuType.GetMethod(
                "GetMenuItems",
                BindingFlags.Static | BindingFlags.NonPublic);
            PropertyInfo pathProperty = menuItemType.GetProperty(
                "path",
                BindingFlags.Instance
                | BindingFlags.Public
                | BindingFlags.NonPublic);
            Assert.That(getMenuItems, Is.Not.Null);
            Assert.That(pathProperty, Is.Not.Null);

            var items = (Array)getMenuItems.Invoke(
                null,
                new object[] { "Assets/Create", true, false });
            var paths = new List<string>(items.Length);
            foreach (object item in items)
            {
                paths.Add((string)pathProperty.GetValue(item));
            }

            return paths.ToArray();
        }

        private static string CreateTemporaryDirectory()
        {
            string path = Path.Combine(
                Path.GetTempPath(),
                "component-autobind-view-template-test-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(path);
            return path;
        }
    }

    public partial class UngeneratedTemplateView : ViewLifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            RegisterCommon<UngeneratedTemplatePresenter>(builder, view);
        }
    }

    public sealed class UngeneratedTemplatePresenter : IStartable
    {
        private readonly UngeneratedTemplateView.UIView m_View;
        private readonly ViewBundle m_ViewBundle;

        public UngeneratedTemplatePresenter(
            UngeneratedTemplateView.UIView view,
            ViewBundle viewBundle)
        {
            m_View = view;
            m_ViewBundle = viewBundle;
        }

        public void Start()
        {
        }
    }

    public partial class GeneratedTemplateView : ViewLifetimeScope
    {
        protected override void Configure(IContainerBuilder builder)
        {
            base.Configure(builder);
            RegisterCommon<GeneratedTemplatePresenter>(builder, view);
        }
    }

    public partial class GeneratedTemplateView : IAutoBindHost
    {
        [Serializable]
        public new class UIView : IUiViewComponent
        {
        }

        private new UIView view;

        public override void EnsureAutoBind(GameObject go)
        {
            view = view ?? new UIView();
        }
    }

    public sealed class GeneratedTemplatePresenter : IStartable
    {
        private readonly GeneratedTemplateView.UIView m_View;
        private readonly ViewBundle m_ViewBundle;

        public GeneratedTemplatePresenter(
            GeneratedTemplateView.UIView view,
            ViewBundle viewBundle)
        {
            m_View = view;
            m_ViewBundle = viewBundle;
        }

        public void Start()
        {
        }
    }
}
