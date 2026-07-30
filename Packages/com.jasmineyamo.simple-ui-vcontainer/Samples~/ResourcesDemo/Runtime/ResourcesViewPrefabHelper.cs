using JasmineYamo.SimpleUI.VContainer;
using UnityEngine;

namespace JasmineYamo.SimpleUI.VContainer.Samples.ResourcesDemo
{
    public sealed class ResourcesViewPrefabHelper : IViewPrefabHelper
    {
        private const string ViewsRoot = "Views/";

        public GameObject GetViewPrefab(string viewName)
        {
            if (string.IsNullOrWhiteSpace(viewName))
            {
                return null;
            }

            return Resources.Load<GameObject>(ViewsRoot + viewName);
        }
    }
}
