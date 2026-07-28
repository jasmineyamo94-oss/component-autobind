using System;
using JasmineYamo.ComponentAutoBind;
using UnityEditor;
using UnityEngine;

namespace JasmineYamo.ComponentAutoBind.Editor
{
    [CustomEditor(typeof(ComponentAutoBindTool))]
    public sealed class ComponentAutoBindToolInspector : UnityEditor.Editor
    {
        private ComponentAutoBindTool m_Target;
        private SerializedProperty m_BindDatas;
        private SerializedProperty m_BindComs;
        private SerializedProperty m_BindFieldNames;

        private AutoBindGlobalSetting m_Setting;
        private AutoBindKeyMapSetting m_KeyMapSetting;
        private SerializedProperty m_TargetScript;
        private SerializedProperty m_Namespace;
        private SerializedProperty m_ClassName;
        private SerializedProperty m_CodePath;

        private bool m_IsKeyMapExpanded;
        private bool m_IsBindingListExpanded = true;

        private void OnEnable()
        {
            m_Target = (ComponentAutoBindTool)target;
            m_BindDatas = serializedObject.FindProperty("BindDatas");
            m_BindComs = serializedObject.FindProperty("m_BindComs");
            m_BindFieldNames = serializedObject.FindProperty("m_BindFieldNames");
            m_TargetScript = serializedObject.FindProperty("m_targetScript");
            m_Namespace = serializedObject.FindProperty("m_Namespace");
            m_ClassName = serializedObject.FindProperty("m_ClassName");
            m_CodePath = serializedObject.FindProperty("m_CodePath");

            m_Setting = AutoBindEditorAssetLocator.LoadSingletonAsset<AutoBindGlobalSetting>()
                ?? AutoBindEditorAssetLocator.CreateSingletonAsset<AutoBindGlobalSetting>(
                    AutoBindSettingsPaths.GlobalSettingAssetPath);
            m_KeyMapSetting = AutoBindEditorAssetLocator.LoadSingletonAsset<AutoBindKeyMapSetting>()
                ?? AutoBindEditorAssetLocator.CreateSingletonAsset<AutoBindKeyMapSetting>(
                    AutoBindSettingsPaths.KeyMapSettingAssetPath);

            serializedObject.Update();
            if (m_Setting != null)
            {
                if (string.IsNullOrEmpty(m_Namespace.stringValue))
                {
                    m_Namespace.stringValue = m_Setting.Namespace;
                }

                if (string.IsNullOrEmpty(m_ClassName.stringValue))
                {
                    m_ClassName.stringValue = m_Target.gameObject.name;
                }

                if (string.IsNullOrEmpty(m_CodePath.stringValue))
                {
                    m_CodePath.stringValue = m_Setting.CodePath;
                }
            }

            if (AutoBindSerializedBindingUtility.NeedsRuntimeBindingSync(
                    m_BindDatas,
                    m_BindComs,
                    m_BindFieldNames))
            {
                AutoBindSerializedBindingUtility.SyncRuntimeBindingCaches(
                    m_BindDatas,
                    m_BindComs,
                    m_BindFieldNames);
            }

            serializedObject.ApplyModifiedProperties();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            if (m_Setting == null || m_KeyMapSetting == null)
            {
                EditorGUILayout.HelpBox(
                    "Component Auto Bind settings could not be created. Check the Unity Console for details.",
                    MessageType.Error);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            DrawHeaderSection();
            AutoBindInspectorDrawer.DrawBindComponentFoldout(
                m_BindDatas,
                ref m_IsBindingListExpanded,
                DrawBindingData);
            AutoBindInspectorDrawer.DrawKeyMapTip(
                m_KeyMapSetting,
                ref m_IsKeyMapExpanded);

            if (serializedObject.hasModifiedProperties)
            {
                AutoBindSerializedBindingUtility.SyncRuntimeBindingCaches(
                    m_BindDatas,
                    m_BindComs,
                    m_BindFieldNames);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeaderSection()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            AutoBindInspectorDrawer.DrawActionSection(
                HandleAutoBindAndValidate,
                HandleGenerateCode,
                HandleRemoveAll);
            GUILayout.Space(2f);
            DrawSettings();
            EditorGUILayout.EndVertical();
        }

        private void DrawSettings()
        {
            if (m_TargetScript != null)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUILayout.PropertyField(m_TargetScript, new GUIContent("Target Script"));
                bool targetScriptChanged = EditorGUI.EndChangeCheck();
                if (targetScriptChanged || m_TargetScript.objectReferenceValue != null)
                {
                    AutoBindTargetScriptSyncUtility.SyncFromTargetScript(
                        m_TargetScript,
                        m_Namespace,
                        m_ClassName,
                        m_CodePath,
                        m_Setting);
                }
            }

            AutoBindInspectorDrawer.DrawSettingsSection(
                m_Namespace.stringValue,
                m_ClassName.stringValue,
                m_CodePath.stringValue);
        }

        private void DrawBindingData()
        {
            int deleteIndex = AutoBindInspectorDrawer.DrawBindDataList(m_BindDatas);
            if (deleteIndex < 0)
            {
                return;
            }

            m_BindDatas.DeleteArrayElementAtIndex(deleteIndex);
            AutoBindSerializedBindingUtility.SyncRuntimeBindingCaches(
                m_BindDatas,
                m_BindComs,
                m_BindFieldNames);
        }

        private void HandleAutoBindAndValidate()
        {
            ApplySerializedChanges();
            AutoBindComponent();
            ValidateBindingsAndNotify();
        }

        private void HandleGenerateCode()
        {
            ApplySerializedChanges();
            GenerateCode();
        }

        private void HandleRemoveAll()
        {
            Undo.RecordObject(m_Target, "Clear Component Auto Bind Bindings");
            ApplySerializedChanges();
            m_BindDatas.ClearArray();
            ApplySerializedChanges(true);
        }

        private void AutoBindComponent()
        {
            AutoBindScanResult scanResult = AutoBindHierarchyScanner.CollectBindings(
                m_Target,
                m_KeyMapSetting);
            AutoBindSerializedBindingUtility.RebuildBindDatas(
                m_BindDatas,
                scanResult.BindDatas);
            ApplySerializedChanges(true);

            for (int i = 0; i < scanResult.Errors.Count; i++)
            {
                Debug.LogError(scanResult.Errors[i], m_Target);
            }
        }

        private void ApplySerializedChanges(bool forceSyncRuntimeBindings = false)
        {
            if (forceSyncRuntimeBindings || serializedObject.hasModifiedProperties)
            {
                AutoBindSerializedBindingUtility.SyncRuntimeBindingCaches(
                    m_BindDatas,
                    m_BindComs,
                    m_BindFieldNames);
            }

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
        }

        private void ValidateBindingsAndNotify()
        {
            AutoBindValidationResult validationResult = AutoBindValidator.Validate(m_Target, m_Setting);
            string report = validationResult.BuildReport();

            if (!validationResult.IsValid)
            {
                Debug.LogError($"[{m_Target.name}] Auto Bind validation failed.\n{report}", m_Target);
            }
            else if (validationResult.WarningCount > 0)
            {
                Debug.LogWarning(
                    $"[{m_Target.name}] Auto Bind validation passed with warnings.\n{report}",
                    m_Target);
            }
            else
            {
                Debug.Log($"[{m_Target.name}] Auto Bind validation passed.\n{report}", m_Target);
            }
        }

        private void GenerateCode()
        {
            AutoBindValidationResult validationResult = AutoBindValidator.Validate(m_Target, m_Setting);
            string report = validationResult.BuildReport();
            if (!validationResult.IsValid)
            {
                Debug.LogError(
                    $"[{m_Target.name}] Auto Bind generation was blocked by validation.\n{report}",
                    m_Target);
                return;
            }

            if (validationResult.WarningCount > 0)
            {
                Debug.LogWarning(
                    $"[{m_Target.name}] Auto Bind generation has warnings.\n{report}",
                    m_Target);
            }

            try
            {
                string filePath = AutoBindCodeGenerator.Generate(m_Target, m_Setting);
                AssetDatabase.Refresh();
                Debug.Log(
                    $"[{m_Target.name}] Auto Bind generated code:\n{filePath}",
                    m_Target);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception, m_Target);
            }
        }
    }
}
