using System;
using System.IO;
using UnityEngine;

namespace JasmineYamo.ComponentAutoBind.Editor
{
    internal static class AutoBindPathUtility
    {
        public static string GetResolvedCodePath(AutoBindGlobalSetting setting, ComponentAutoBindTool target)
        {
            if (setting == null || target == null)
            {
                return string.Empty;
            }

            string storedPath = setting.UseGlobalDefaultSavePath && !string.IsNullOrWhiteSpace(target.CodePath)
                ? target.CodePath
                : setting.CodePath;
            return ResolveProjectPath(storedPath);
        }

        public static string GetInspectorCodePath(AutoBindGlobalSetting setting, string scriptAssetPath)
        {
            if (setting == null)
            {
                return string.Empty;
            }

            if (setting.UseGlobalDefaultSavePath && !string.IsNullOrWhiteSpace(scriptAssetPath))
            {
                string scriptDirectory = Path.GetDirectoryName(scriptAssetPath)?.Replace('\\', '/');
                if (!string.IsNullOrWhiteSpace(scriptDirectory))
                {
                    return NormalizeAssetPath(scriptDirectory);
                }
            }

            return NormalizeAssetPath(setting.CodePath);
        }

        public static bool IsPathUnderAssets(string fullPath)
        {
            if (string.IsNullOrWhiteSpace(fullPath))
            {
                return false;
            }

            try
            {
                string assetsPath = NormalizeDirectoryPath(Application.dataPath);
                string targetPath = NormalizeDirectoryPath(fullPath);
                return string.Equals(targetPath, assetsPath, StringComparison.OrdinalIgnoreCase)
                    || targetPath.StartsWith(
                        assetsPath + Path.DirectorySeparatorChar,
                        StringComparison.OrdinalIgnoreCase)
                    || targetPath.StartsWith(
                        assetsPath + Path.AltDirectorySeparatorChar,
                        StringComparison.OrdinalIgnoreCase);
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        public static string ResolveProjectPath(string storedPath)
        {
            if (string.IsNullOrWhiteSpace(storedPath))
            {
                return string.Empty;
            }

            if (Path.IsPathRooted(storedPath))
            {
                return Path.GetFullPath(storedPath);
            }

            string normalizedPath = storedPath.Replace('\\', '/');
            string projectRoot = Directory.GetParent(Application.dataPath).FullName;
            if (string.Equals(normalizedPath, "Assets", StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFullPath(Application.dataPath);
            }

            if (normalizedPath.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                return Path.GetFullPath(Path.Combine(projectRoot, normalizedPath));
            }

            return Path.GetFullPath(Path.Combine(Application.dataPath, normalizedPath.TrimStart('/')));
        }

        public static string NormalizeAssetPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
            {
                return string.Empty;
            }

            string normalized = path.Replace('\\', '/');
            if (string.Equals(normalized, "Assets", StringComparison.OrdinalIgnoreCase))
            {
                return "Assets";
            }

            if (normalized.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
            {
                return normalized;
            }

            if (Path.IsPathRooted(normalized))
            {
                string projectRoot = Directory.GetParent(Application.dataPath).FullName.Replace('\\', '/');
                if (normalized.StartsWith(projectRoot + "/", StringComparison.OrdinalIgnoreCase))
                {
                    return normalized.Substring(projectRoot.Length + 1);
                }

                return string.Empty;
            }

            return "Assets/" + normalized.TrimStart('/');
        }

        private static string NormalizeDirectoryPath(string path)
        {
            return Path.GetFullPath(path)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}
