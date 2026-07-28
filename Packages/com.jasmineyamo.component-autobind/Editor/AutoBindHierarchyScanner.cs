using System.Collections.Generic;
using UnityEngine;

namespace JasmineYamo.ComponentAutoBind.Editor
{
    internal static class AutoBindHierarchyScanner
    {
        public static AutoBindScanResult CollectBindings(
            ComponentAutoBindTool target,
            AutoBindKeyMapSetting keyMapSetting)
        {
            var result = new AutoBindScanResult();
            if (target == null)
            {
                result.AddError("The Component Auto Bind root is null.");
                return result;
            }

            if (keyMapSetting == null)
            {
                result.AddError("AutoBindKeyMapSetting is null.");
                return result;
            }

            var childRoots = new List<Transform>();
            CollectChildRoots(target.transform, childRoots);
            for (int i = 0; i < childRoots.Count; i++)
            {
                CollectTransformBindings(childRoots[i], keyMapSetting, result);
            }

            return result;
        }

        private static void CollectChildRoots(Transform transform, List<Transform> childRoots)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                Transform currentChild = transform.GetChild(i);
                if (currentChild.name.Contains("NonRoot ==>"))
                {
                    continue;
                }

                childRoots.Add(currentChild);
                if (currentChild.GetComponent<ComponentAutoBindTool>() != null)
                {
                    continue;
                }

                CollectChildRoots(currentChild, childRoots);
            }
        }

        private static void CollectTransformBindings(
            Transform target,
            AutoBindKeyMapSetting keyMapSetting,
            AutoBindScanResult result)
        {
            string[] nameParts = target.name.Split('_');
            if (nameParts.Length <= 1)
            {
                return;
            }

            string fieldSuffix = nameParts[nameParts.Length - 1];
            for (int i = 0; i < nameParts.Length - 1; i++)
            {
                string prefix = nameParts[i];
                if (!keyMapSetting.TryGetComponentTypeName(prefix, out string componentTypeName))
                {
                    result.AddError($"The name prefix '{prefix}' on '{target.name}' has no component mapping.");
                    return;
                }

                Component component = AutoBindComponentTypeResolver.GetComponent(
                    target.gameObject,
                    componentTypeName);
                if (component == null)
                {
                    result.AddError($"'{target.name}' does not contain a '{componentTypeName}' component.");
                    continue;
                }

                result.AddBindData($"{prefix}_{fieldSuffix}", component);
            }
        }
    }
}
