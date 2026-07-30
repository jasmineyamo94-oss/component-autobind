using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text;
using JasmineYamo.ComponentAutoBind.Editor;
using UnityEditor;
using UnityEngine;

namespace JasmineYamo.SimpleUI.VContainer.Editor
{
    internal static class VContainerViewTemplateCreator
    {
        internal const string MenuPath = "Assets/Create/C# Scripts VContainer View";
        internal const int MenuPriority = 80;
        private const string NamespaceBodyToken = "#NAMESPACE_BODY#";
        private const string ViewNameToken = "#VIEW_NAME#";
        private const string PresenterNameToken = "#PRESENTER_NAME#";
        private const string ViewTemplatePath =
            "Packages/com.jasmineyamo.simple-ui-vcontainer/"
            + "Editor/Templates/VContainerView.cs.txt";
        private const string PresenterTemplatePath =
            "Packages/com.jasmineyamo.simple-ui-vcontainer/"
            + "Editor/Templates/VContainerViewPresenter.cs.txt";

        private static readonly HashSet<string> s_CSharpKeywords =
            new HashSet<string>(StringComparer.Ordinal)
            {
                "abstract",
                "as",
                "base",
                "bool",
                "break",
                "byte",
                "case",
                "catch",
                "char",
                "checked",
                "class",
                "const",
                "continue",
                "decimal",
                "default",
                "delegate",
                "do",
                "double",
                "else",
                "enum",
                "event",
                "explicit",
                "extern",
                "false",
                "finally",
                "fixed",
                "float",
                "for",
                "foreach",
                "goto",
                "if",
                "implicit",
                "in",
                "int",
                "interface",
                "internal",
                "is",
                "lock",
                "long",
                "namespace",
                "new",
                "null",
                "object",
                "operator",
                "out",
                "override",
                "params",
                "private",
                "protected",
                "public",
                "readonly",
                "ref",
                "return",
                "sbyte",
                "sealed",
                "short",
                "sizeof",
                "stackalloc",
                "static",
                "string",
                "struct",
                "switch",
                "this",
                "throw",
                "true",
                "try",
                "typeof",
                "uint",
                "ulong",
                "unchecked",
                "unsafe",
                "ushort",
                "using",
                "virtual",
                "void",
                "volatile",
                "while"
            };

        [MenuItem(MenuPath, false, MenuPriority)]
        private static void CreateVContainerView()
        {
#if JASMINEYAMO_SIMPLE_UI_VCONTAINER
            if (!TryGetActiveFolderPath(out string directoryAssetPath, out string error))
            {
                Debug.LogError(
                    "[Simple UI] VContainer View creation failed: "
                    + error);
                return;
            }

            VContainerViewCreateWindow.Open(directoryAssetPath);
#else
            Debug.LogError(
                "[Simple UI] VContainer View creation requires "
                + "jp.hadashikick.vcontainer.");
#endif
        }

        [MenuItem(MenuPath, true)]
        private static bool CanCreateVContainerView()
        {
#if JASMINEYAMO_SIMPLE_UI_VCONTAINER
            return true;
#else
            return false;
#endif
        }

        internal static bool TryCreate(
            string directoryAssetPath,
            string requestedName)
        {
            if (!TryBuildNames(
                    requestedName,
                    out VContainerViewTemplateNames names,
                    out string error))
            {
                Debug.LogError($"[Simple UI] VContainer View creation failed: {error}");
                return false;
            }

            directoryAssetPath = directoryAssetPath?.Replace('\\', '/');
            if (string.IsNullOrWhiteSpace(directoryAssetPath)
                || !AssetDatabase.IsValidFolder(directoryAssetPath))
            {
                Debug.LogError(
                    "[Simple UI] VContainer View creation failed: "
                    + $"'{directoryAssetPath}' is not a valid Project folder.");
                return false;
            }

            if (!TryGetConfiguredNamespace(out string namespaceName, out error))
            {
                Debug.LogError($"[Simple UI] VContainer View creation failed: {error}");
                return false;
            }

            if (!TryLoadTemplate(ViewTemplatePath, out string viewTemplate, out error)
                || !TryLoadTemplate(
                    PresenterTemplatePath,
                    out string presenterTemplate,
                    out error))
            {
                Debug.LogError($"[Simple UI] VContainer View creation failed: {error}");
                return false;
            }

            string viewCode;
            string presenterCode;
            try
            {
                viewCode = RenderTemplate(viewTemplate, names, namespaceName);
                presenterCode = RenderTemplate(presenterTemplate, names, namespaceName);
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    "[Simple UI] VContainer View creation failed while rendering "
                    + $"templates:\n{exception}");
                return false;
            }

            string viewAssetPath = CombineAssetPath(
                directoryAssetPath,
                names.ViewFileName);
            string presenterAssetPath = CombineAssetPath(
                directoryAssetPath,
                names.PresenterFileName);
            string projectRoot = Directory.GetCurrentDirectory();
            string viewFilePath = Path.GetFullPath(Path.Combine(projectRoot, viewAssetPath));
            string presenterFilePath =
                Path.GetFullPath(Path.Combine(projectRoot, presenterAssetPath));

            if (!TryWriteScriptsDirectly(
                    viewFilePath,
                    viewCode,
                    presenterFilePath,
                    presenterCode,
                    out error))
            {
                Debug.LogError($"[Simple UI] VContainer View creation failed: {error}");
                return false;
            }

            try
            {
                AssetDatabase.Refresh();
                MonoScript viewScript = AssetDatabase.LoadAssetAtPath<MonoScript>(viewAssetPath);
                MonoScript presenterScript =
                    AssetDatabase.LoadAssetAtPath<MonoScript>(presenterAssetPath);
                if (viewScript == null || presenterScript == null)
                {
                    throw new InvalidOperationException(
                        "The created scripts could not both be imported:\n"
                        + $"{viewAssetPath}\n{presenterAssetPath}");
                }

                Selection.activeObject = viewScript;
                EditorGUIUtility.PingObject(viewScript);
                Debug.Log(
                    "[Simple UI] Created VContainer View scripts:\n"
                    + $"{viewAssetPath}\n{presenterAssetPath}");
                return true;
            }
            catch (Exception exception)
            {
                DeleteCreatedAsset(viewAssetPath, viewFilePath);
                DeleteCreatedAsset(presenterAssetPath, presenterFilePath);
                Debug.LogError(
                    "[Simple UI] VContainer View creation failed while importing "
                    + $"scripts:\n{exception}");
                return false;
            }
        }

        internal static bool TryGetActiveFolderPath(
            out string directoryAssetPath,
            out string error)
        {
            MethodInfo method = typeof(ProjectWindowUtil).GetMethod(
                "GetActiveFolderPath",
                BindingFlags.Static | BindingFlags.NonPublic);
            if (method == null)
            {
                directoryAssetPath = null;
                error =
                    "Unity did not expose the active Project folder for this Editor version.";
                return false;
            }

            try
            {
                directoryAssetPath = (method.Invoke(null, null) as string)
                    ?.Replace('\\', '/');
            }
            catch (Exception exception)
            {
                directoryAssetPath = null;
                Exception cause = exception is TargetInvocationException invocationException
                    && invocationException.InnerException != null
                    ? invocationException.InnerException
                    : exception;
                error = $"The active Project folder could not be read: {cause.Message}";
                return false;
            }

            if (string.IsNullOrWhiteSpace(directoryAssetPath)
                || !AssetDatabase.IsValidFolder(directoryAssetPath))
            {
                error = $"'{directoryAssetPath}' is not a valid Project folder.";
                return false;
            }

            error = null;
            return true;
        }

        internal static bool TryBuildNames(
            string requestedName,
            out VContainerViewTemplateNames names,
            out string error)
        {
            names = default;
            string trimmedName = requestedName?.Trim();
            if (string.IsNullOrEmpty(trimmedName))
            {
                error = "The View name is empty.";
                return false;
            }

            string baseName = trimmedName.EndsWith("View", StringComparison.Ordinal)
                ? trimmedName.Substring(0, trimmedName.Length - "View".Length)
                : trimmedName;
            if (string.IsNullOrEmpty(baseName))
            {
                error = "The View name must contain a name before the 'View' suffix.";
                return false;
            }

            if (!IsValidIdentifier(baseName))
            {
                error = $"'{baseName}' is not a valid C# identifier.";
                return false;
            }

            names = new VContainerViewTemplateNames(
                baseName + "View",
                baseName + "Presenter");
            error = null;
            return true;
        }

        internal static string RenderTemplate(
            string template,
            VContainerViewTemplateNames names,
            string namespaceName)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            string normalizedTemplate = template
                .Replace("\r\n", "\n")
                .Replace('\r', '\n');
            int markerIndex = normalizedTemplate.IndexOf(
                NamespaceBodyToken,
                StringComparison.Ordinal);
            if (markerIndex < 0
                || markerIndex != normalizedTemplate.LastIndexOf(
                    NamespaceBodyToken,
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"The template must contain one {NamespaceBodyToken} token.");
            }

            string prefix = normalizedTemplate.Substring(0, markerIndex).TrimEnd('\n');
            string body = normalizedTemplate
                .Substring(markerIndex + NamespaceBodyToken.Length)
                .Trim('\n')
                .Replace(ViewNameToken, names.ViewClassName)
                .Replace(PresenterNameToken, names.PresenterClassName);

            var builder = new StringBuilder(template.Length + 128);
            builder.Append(prefix).AppendLine().AppendLine();
            if (string.IsNullOrWhiteSpace(namespaceName))
            {
                builder.AppendLine(body);
                return builder.ToString().Replace("\r\n", "\n");
            }

            builder.Append("namespace ").Append(namespaceName).AppendLine();
            builder.AppendLine("{");
            AppendIndented(builder, body);
            builder.AppendLine("}");
            return builder.ToString().Replace("\r\n", "\n");
        }

        internal static bool TryWriteScriptsDirectly(
            string viewFilePath,
            string viewCode,
            string presenterFilePath,
            string presenterCode,
            out string error)
        {
            if (File.Exists(viewFilePath) || File.Exists(presenterFilePath))
            {
                error =
                    "Neither file was created because a target file already exists:\n"
                    + $"{viewFilePath}\n{presenterFilePath}";
                return false;
            }

            bool viewCreated = false;
            bool presenterCreated = false;
            try
            {
                WriteNewFile(viewFilePath, viewCode);
                viewCreated = true;
                WriteNewFile(presenterFilePath, presenterCode);
                presenterCreated = true;
                error = null;
                return true;
            }
            catch (Exception exception)
            {
                if (presenterCreated)
                {
                    DeleteCreatedFile(presenterFilePath);
                }

                if (viewCreated)
                {
                    DeleteCreatedFile(viewFilePath);
                }

                error = exception.Message;
                return false;
            }
        }

        internal static bool IsValidNamespace(string namespaceName)
        {
            if (string.IsNullOrWhiteSpace(namespaceName))
            {
                return true;
            }

            string[] segments = namespaceName.Split('.');
            for (int i = 0; i < segments.Length; i++)
            {
                if (!IsValidIdentifier(segments[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool TryGetConfiguredNamespace(
            out string namespaceName,
            out string error)
        {
            string[] settingGuids = AssetDatabase.FindAssets(
                $"t:{nameof(AutoBindGlobalSetting)}");
            if (settingGuids.Length == 0)
            {
                namespaceName = string.Empty;
                error = null;
                return true;
            }

            if (settingGuids.Length > 1)
            {
                namespaceName = null;
                error =
                    "More than one AutoBindGlobalSetting asset was found. "
                    + "Keep one project setting asset.";
                return false;
            }

            string settingPath = AssetDatabase.GUIDToAssetPath(settingGuids[0]);
            AutoBindGlobalSetting setting =
                AssetDatabase.LoadAssetAtPath<AutoBindGlobalSetting>(settingPath);
            if (setting == null)
            {
                namespaceName = null;
                error = $"AutoBindGlobalSetting could not be loaded from '{settingPath}'.";
                return false;
            }

            namespaceName = setting.Namespace?.Trim() ?? string.Empty;
            if (!IsValidNamespace(namespaceName))
            {
                error = $"'{namespaceName}' is not a valid C# namespace.";
                return false;
            }

            error = null;
            return true;
        }

        private static bool TryLoadTemplate(
            string templatePath,
            out string template,
            out string error)
        {
            TextAsset templateAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(templatePath);
            if (templateAsset == null)
            {
                template = null;
                error = $"Template asset was not found at '{templatePath}'.";
                return false;
            }

            template = templateAsset.text;
            error = null;
            return true;
        }

        private static bool IsValidIdentifier(string value)
        {
            if (string.IsNullOrEmpty(value) || s_CSharpKeywords.Contains(value))
            {
                return false;
            }

            if (!IsIdentifierStartCharacter(value[0]))
            {
                return false;
            }

            for (int i = 1; i < value.Length; i++)
            {
                if (!IsIdentifierPartCharacter(value[i]))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsIdentifierStartCharacter(char character)
        {
            return character == '_' || char.IsLetter(character);
        }

        private static bool IsIdentifierPartCharacter(char character)
        {
            if (IsIdentifierStartCharacter(character) || char.IsDigit(character))
            {
                return true;
            }

            UnicodeCategory category = char.GetUnicodeCategory(character);
            return category == UnicodeCategory.NonSpacingMark
                || category == UnicodeCategory.SpacingCombiningMark
                || category == UnicodeCategory.ConnectorPunctuation
                || category == UnicodeCategory.Format;
        }

        private static void AppendIndented(StringBuilder builder, string body)
        {
            string[] lines = body.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Length > 0)
                {
                    builder.Append("    ").AppendLine(lines[i]);
                }
                else
                {
                    builder.AppendLine();
                }
            }
        }

        private static string CombineAssetPath(string directory, string fileName)
        {
            return directory.TrimEnd('/') + "/" + fileName;
        }

        private static void WriteNewFile(string filePath, string content)
        {
            using (var stream = new FileStream(
                       filePath,
                       FileMode.CreateNew,
                       FileAccess.Write,
                       FileShare.None))
            using (var writer = new StreamWriter(stream, new UTF8Encoding(false)))
            {
                writer.Write(content);
            }
        }

        private static void DeleteCreatedFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        private static void DeleteCreatedAsset(string assetPath, string filePath)
        {
            if (AssetDatabase.DeleteAsset(assetPath))
            {
                return;
            }

            DeleteCreatedFile(filePath);
            DeleteCreatedFile(filePath + ".meta");
        }
    }

    internal readonly struct VContainerViewTemplateNames
    {
        public VContainerViewTemplateNames(
            string viewClassName,
            string presenterClassName)
        {
            ViewClassName = viewClassName;
            PresenterClassName = presenterClassName;
        }

        public string ViewClassName { get; }
        public string PresenterClassName { get; }
        public string ViewFileName => ViewClassName + ".cs";
        public string PresenterFileName => PresenterClassName + ".cs";
    }

    internal sealed class VContainerViewCreateWindow : EditorWindow
    {
        private const float WindowWidth = 440f;
        private const float WindowHeight = 230f;
        private const string NameControl = "VContainerViewName";
        internal const string DefaultName = "NewView";

        [SerializeField]
        private string m_DirectoryAssetPath;

        [SerializeField]
        private string m_RequestedName = DefaultName;

        private bool m_ShouldFocusName = true;

        internal static void Open(string directoryAssetPath)
        {
            var window = CreateInstance<VContainerViewCreateWindow>();
            window.titleContent = new GUIContent("Create VContainer View");
            window.m_DirectoryAssetPath = directoryAssetPath;
            window.minSize = new Vector2(WindowWidth, WindowHeight);
            window.maxSize = window.minSize;
            window.ShowModalUtility();
        }

        private void OnGUI()
        {
            Event currentEvent = Event.current;
            if (currentEvent.type == EventType.KeyDown
                && currentEvent.keyCode == KeyCode.Escape)
            {
                currentEvent.Use();
                Close();
                return;
            }

            GUILayout.Space(12f);
            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.Space(12f);
                using (new EditorGUILayout.VerticalScope())
                {
                    EditorGUILayout.LabelField(
                        "Create VContainer View",
                        EditorStyles.boldLabel);
                    EditorGUILayout.Space(4f);

                    GUI.SetNextControlName(NameControl);
                    m_RequestedName = EditorGUILayout.TextField(
                        "View Name",
                        m_RequestedName);
                    if (m_ShouldFocusName)
                    {
                        EditorGUI.FocusTextInControl(NameControl);
                        m_ShouldFocusName = false;
                    }

                    EditorGUILayout.Space(4f);
                    EditorGUILayout.LabelField("Files", EditorStyles.miniBoldLabel);

                    bool hasValidName = VContainerViewTemplateCreator.TryBuildNames(
                        m_RequestedName,
                        out VContainerViewTemplateNames names,
                        out string error);
                    string preview = hasValidName
                        ? names.ViewFileName + "\n" + names.PresenterFileName
                        : "-\n-";
                    EditorGUILayout.HelpBox(preview, MessageType.None);

                    if (!hasValidName)
                    {
                        EditorGUILayout.HelpBox(error, MessageType.Error);
                    }

                    GUILayout.FlexibleSpace();
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        GUILayout.FlexibleSpace();
                        if (GUILayout.Button("Cancel", GUILayout.Width(90f)))
                        {
                            Close();
                            return;
                        }

                        bool enterPressed = currentEvent.type == EventType.KeyDown
                            && (currentEvent.keyCode == KeyCode.Return
                                || currentEvent.keyCode == KeyCode.KeypadEnter);
                        bool createClicked;
                        using (new EditorGUI.DisabledScope(!hasValidName))
                        {
                            createClicked = GUILayout.Button(
                                "Create",
                                GUILayout.Width(90f));
                        }

                        if (createClicked || (hasValidName && enterPressed))
                        {
                            currentEvent.Use();
                            if (VContainerViewTemplateCreator.TryCreate(
                                    m_DirectoryAssetPath,
                                    m_RequestedName))
                            {
                                Close();
                            }
                        }
                    }
                }

                GUILayout.Space(12f);
            }
        }
    }
}
