using System;
using System.Collections.Generic;
using UnityEngine;
using VContainer.Unity;
using Object = UnityEngine.Object;

namespace JasmineYamo.SimpleUI.VContainer
{
    public sealed class ViewManager : IViewManager, ITickable, IDisposable
    {
        private const int SortingOrderStep = 10;

        private readonly LifetimeScope m_ParentScope;
        private readonly IViewPrefabHelper m_ViewPrefabHelper;
        private readonly Transform m_ViewRoot;
        private readonly Dictionary<ViewLayer, List<ViewLifetimeScope>> m_LayerStacks =
            new Dictionary<ViewLayer, List<ViewLifetimeScope>>();
        private readonly Dictionary<ViewLayer, Queue<OpenViewInfo>> m_WaitingViews =
            new Dictionary<ViewLayer, Queue<OpenViewInfo>>();
        private readonly Dictionary<string, List<ViewLifetimeScope>> m_CachedViews =
            new Dictionary<string, List<ViewLifetimeScope>>(StringComparer.Ordinal);
        private readonly List<DelayedView> m_DelayedViews =
            new List<DelayedView>();

        private bool m_Disposed;

        public ViewManager(
            LifetimeScope parentScope,
            IViewPrefabHelper viewPrefabHelper,
            Transform viewRoot)
        {
            m_ParentScope = parentScope
                ?? throw new ArgumentNullException(nameof(parentScope));
            m_ViewPrefabHelper = viewPrefabHelper
                ?? throw new ArgumentNullException(nameof(viewPrefabHelper));
            m_ViewRoot = viewRoot
                ?? throw new ArgumentNullException(nameof(viewRoot));

            Array layers = Enum.GetValues(typeof(ViewLayer));
            for (int i = 0; i < layers.Length; i++)
            {
                var layer = (ViewLayer)layers.GetValue(i);
                m_LayerStacks.Add(layer, new List<ViewLifetimeScope>());
                m_WaitingViews.Add(layer, new Queue<OpenViewInfo>());
            }
        }

        public event Action<ViewLifetimeScope> OnViewOpened;
        public event Action<ViewLifetimeScope> OnViewClosed;

        public bool HasActivePopups =>
            m_LayerStacks[ViewLayer.Normal].Count > 0
            || m_LayerStacks[ViewLayer.Top].Count > 0
            || m_LayerStacks[ViewLayer.System].Count > 0;

        public ViewLifetimeScope GetPeekView()
        {
            return GetPeekView(ViewLayer.Normal);
        }

        public ViewLifetimeScope GetPeekView(ViewLayer layer)
        {
            List<ViewLifetimeScope> stack = GetLayerStack(layer);
            return stack.Count > 0 ? stack[stack.Count - 1] : null;
        }

        public ViewLifetimeScope ShowView(
            string viewName,
            bool immediately = true,
            params object[] args)
        {
            if (!TryResolvePrefab(
                    viewName,
                    out GameObject prefab,
                    out ViewLifetimeScope prefabView))
            {
                return null;
            }

            if (!immediately)
            {
                m_WaitingViews[prefabView.viewLayer].Enqueue(
                    new OpenViewInfo(viewName, args));
                return null;
            }

            ViewLayer layer = prefabView.viewLayer;
            ViewLifetimeScope oldPeekView = GetPeekView(layer);
            if (oldPeekView != null && oldPeekView.ViewName == viewName)
            {
                return oldPeekView;
            }

            oldPeekView?.Pause();
            ViewLifetimeScope view = AcquireView(
                viewName,
                prefab,
                prefabView.GetType(),
                args);
            if (view == null)
            {
                oldPeekView?.Resume();
                return null;
            }

            AddToLayer(view);
            return ShowOpenedView(view);
        }

        public ViewLifetimeScope ShowViewAtHidePeekView(
            string viewName,
            params object[] args)
        {
            if (!TryResolvePrefab(
                    viewName,
                    out GameObject prefab,
                    out ViewLifetimeScope prefabView))
            {
                return null;
            }

            ViewLayer layer = prefabView.viewLayer;
            ViewLifetimeScope oldPeekView = GetPeekView(layer);
            if (oldPeekView != null && oldPeekView.ViewName == viewName)
            {
                return oldPeekView;
            }

            ViewLifetimeScope view = AcquireView(
                viewName,
                prefab,
                prefabView.GetType(),
                args);
            if (view == null)
            {
                return null;
            }

            if (oldPeekView != null)
            {
                CloseView(oldPeekView, false);
            }

            AddToLayer(view);
            return ShowOpenedView(view);
        }

        public void HideView()
        {
            HideView(ViewLayer.Normal);
        }

        public void HideView(ViewLayer layer)
        {
            ViewLifetimeScope view = GetPeekView(layer);
            if (view == null)
            {
                Debug.LogError(
                    $"[Simple UI] No view is available to hide on layer {layer}.");
                return;
            }

            CloseView(view, true);
        }

        public void HideView(ViewLifetimeScope view)
        {
            if (view == null)
            {
                Debug.LogError("[Simple UI] The view to hide is null.");
                return;
            }

            if (!CloseView(view, true))
            {
                Debug.LogError(
                    $"[Simple UI] View '{view.name}' is not managed by this ViewManager.",
                    view);
            }
        }

        public void HideAllViews()
        {
            Array layers = Enum.GetValues(typeof(ViewLayer));
            for (int i = 0; i < layers.Length; i++)
            {
                HideAllViews((ViewLayer)layers.GetValue(i));
            }
        }

        public void HideAllViews(ViewLayer layer)
        {
            List<ViewLifetimeScope> stack = GetLayerStack(layer);
            while (stack.Count > 0)
            {
                CloseView(stack[stack.Count - 1], false);
            }
        }

        public void Tick()
        {
            if (m_Disposed)
            {
                return;
            }

            ProcessDelayedDestroy();

            Array layers = Enum.GetValues(typeof(ViewLayer));
            for (int i = 0; i < layers.Length; i++)
            {
                var layer = (ViewLayer)layers.GetValue(i);
                Queue<OpenViewInfo> queue = m_WaitingViews[layer];
                if (m_LayerStacks[layer].Count == 0 && queue.Count > 0)
                {
                    OpenViewInfo info = queue.Dequeue();
                    ShowView(info.ViewName, true, info.Args);
                }
            }
        }

        public void Dispose()
        {
            if (m_Disposed)
            {
                return;
            }

            m_Disposed = true;
            foreach (KeyValuePair<string, List<ViewLifetimeScope>> pair
                     in m_CachedViews)
            {
                List<ViewLifetimeScope> cached = pair.Value;
                for (int i = cached.Count - 1; i >= 0; i--)
                {
                    DestroyView(cached[i]);
                }
            }

            m_CachedViews.Clear();
            m_DelayedViews.Clear();
            foreach (Queue<OpenViewInfo> queue in m_WaitingViews.Values)
            {
                queue.Clear();
            }

            foreach (List<ViewLifetimeScope> stack in m_LayerStacks.Values)
            {
                stack.Clear();
            }
        }

        private List<ViewLifetimeScope> GetLayerStack(ViewLayer layer)
        {
            if (m_LayerStacks.TryGetValue(layer, out List<ViewLifetimeScope> stack))
            {
                return stack;
            }

            throw new ArgumentOutOfRangeException(nameof(layer), layer, null);
        }

        private bool TryResolvePrefab(
            string viewName,
            out GameObject prefab,
            out ViewLifetimeScope prefabView)
        {
            prefab = null;
            prefabView = null;
            if (string.IsNullOrWhiteSpace(viewName))
            {
                Debug.LogError("[Simple UI] View name is empty.");
                return false;
            }

            prefab = m_ViewPrefabHelper.GetViewPrefab(viewName);
            if (prefab == null)
            {
                Debug.LogError($"[Simple UI] View prefab '{viewName}' was not found.");
                return false;
            }

            prefabView = prefab.GetComponent<ViewLifetimeScope>();
            if (prefabView == null)
            {
                Debug.LogError(
                    $"[Simple UI] Prefab '{viewName}' does not contain "
                    + $"{nameof(ViewLifetimeScope)}.",
                    prefab);
                return false;
            }

            return true;
        }

        private ViewLifetimeScope AcquireView(
            string viewName,
            GameObject prefab,
            Type expectedType,
            object[] args)
        {
            ViewLifetimeScope view = TakeCachedView(viewName, expectedType);
            if (view == null)
            {
                GameObject instance = null;
                try
                {
                    using (LifetimeScope.EnqueueParent(m_ParentScope))
                    {
                        instance = Object.Instantiate(prefab, m_ViewRoot);
                    }

                    view = instance.GetComponent<ViewLifetimeScope>();
                    if (view == null)
                    {
                        Debug.LogError(
                            $"[Simple UI] Instantiated prefab '{viewName}' does not contain "
                            + $"{nameof(ViewLifetimeScope)}.",
                            instance);
                        DestroyGameObject(instance);
                        return null;
                    }

                    m_ParentScope.Container.InjectGameObject(instance);
                }
                catch (Exception exception)
                {
                    Debug.LogError(
                        $"[Simple UI] Failed to instantiate view '{viewName}':\n{exception}");
                    DestroyGameObject(instance);
                    return null;
                }
            }

            try
            {
                view.transform.SetParent(m_ViewRoot, false);
                view.AttachViewManager(this);
                view.SetViewName(viewName);
                view.Init(args);
                return view;
            }
            catch (Exception exception)
            {
                Debug.LogError(
                    $"[Simple UI] Failed to initialize view '{viewName}':\n{exception}",
                    view);
                DestroyView(view);
                return null;
            }
        }

        private ViewLifetimeScope TakeCachedView(
            string viewName,
            Type expectedType)
        {
            if (!m_CachedViews.TryGetValue(
                    viewName,
                    out List<ViewLifetimeScope> cached))
            {
                return null;
            }

            for (int i = cached.Count - 1; i >= 0; i--)
            {
                ViewLifetimeScope view = cached[i];
                cached.RemoveAt(i);
                if (view == null)
                {
                    continue;
                }

                RemoveDelayedDestroy(view);
                if (view.GetType() == expectedType)
                {
                    if (cached.Count == 0)
                    {
                        m_CachedViews.Remove(viewName);
                    }

                    return view;
                }

                DestroyView(view);
            }

            m_CachedViews.Remove(viewName);
            return null;
        }

        private void AddToLayer(ViewLifetimeScope view)
        {
            List<ViewLifetimeScope> stack = GetLayerStack(view.viewLayer);
            stack.Add(view);
            RefreshSortingOrders(view.viewLayer);
        }

        private ViewLifetimeScope ShowOpenedView(ViewLifetimeScope view)
        {
            view.Show();
            OnViewOpened?.Invoke(view);
            return view;
        }

        private bool CloseView(ViewLifetimeScope targetView, bool resumeNext)
        {
            if (!TryFindView(
                    targetView,
                    out ViewLayer layer,
                    out List<ViewLifetimeScope> stack,
                    out int index))
            {
                return false;
            }

            bool wasTop = index == stack.Count - 1;
            stack.RemoveAt(index);
            targetView.Hide();
            OnViewClosed?.Invoke(targetView);
            CacheOrDestroy(targetView);

            RefreshSortingOrders(layer);
            if (wasTop && resumeNext && stack.Count > 0)
            {
                stack[stack.Count - 1].Resume();
            }

            return true;
        }

        private bool TryFindView(
            ViewLifetimeScope targetView,
            out ViewLayer layer,
            out List<ViewLifetimeScope> stack,
            out int index)
        {
            foreach (KeyValuePair<ViewLayer, List<ViewLifetimeScope>> pair
                     in m_LayerStacks)
            {
                index = pair.Value.IndexOf(targetView);
                if (index >= 0)
                {
                    layer = pair.Key;
                    stack = pair.Value;
                    return true;
                }
            }

            layer = default;
            stack = null;
            index = -1;
            return false;
        }

        private void RefreshSortingOrders(ViewLayer layer)
        {
            List<ViewLifetimeScope> stack = GetLayerStack(layer);
            for (int i = 0; i < stack.Count; i++)
            {
                ViewLifetimeScope view = stack[i];
                if (view != null)
                {
                    view.SetCanvasOrder(
                        (int)layer + (i + 1) * SortingOrderStep);
                }
            }
        }

        private void CacheOrDestroy(ViewLifetimeScope view)
        {
            if (view == null)
            {
                return;
            }

            if (view.destroyType == DestroyType.DelayDestroy
                && view.DestroyDelaySeconds <= 0f)
            {
                DestroyView(view);
                return;
            }

            if (!m_CachedViews.TryGetValue(
                    view.ViewName,
                    out List<ViewLifetimeScope> cached))
            {
                cached = new List<ViewLifetimeScope>();
                m_CachedViews.Add(view.ViewName, cached);
            }

            cached.Add(view);
            if (view.destroyType == DestroyType.DelayDestroy)
            {
                m_DelayedViews.Add(
                    new DelayedView(
                        view,
                        Time.realtimeSinceStartup + view.DestroyDelaySeconds));
            }
        }

        private void ProcessDelayedDestroy()
        {
            float now = Time.realtimeSinceStartup;
            for (int i = m_DelayedViews.Count - 1; i >= 0; i--)
            {
                DelayedView delayed = m_DelayedViews[i];
                if (delayed.View == null || now >= delayed.DestroyAt)
                {
                    m_DelayedViews.RemoveAt(i);
                    RemoveCachedView(delayed.View);
                    DestroyView(delayed.View);
                }
            }
        }

        private void RemoveDelayedDestroy(ViewLifetimeScope view)
        {
            for (int i = m_DelayedViews.Count - 1; i >= 0; i--)
            {
                if (ReferenceEquals(m_DelayedViews[i].View, view))
                {
                    m_DelayedViews.RemoveAt(i);
                }
            }
        }

        private void RemoveCachedView(ViewLifetimeScope view)
        {
            if (view == null
                || string.IsNullOrEmpty(view.ViewName)
                || !m_CachedViews.TryGetValue(
                    view.ViewName,
                    out List<ViewLifetimeScope> cached))
            {
                return;
            }

            cached.Remove(view);
            if (cached.Count == 0)
            {
                m_CachedViews.Remove(view.ViewName);
            }
        }

        private static void DestroyView(ViewLifetimeScope view)
        {
            if (view != null)
            {
                DestroyGameObject(view.gameObject);
            }
        }

        private static void DestroyGameObject(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Object.Destroy(gameObject);
            }
            else
            {
                Object.DestroyImmediate(gameObject);
            }
        }

        private sealed class OpenViewInfo
        {
            public OpenViewInfo(string viewName, object[] args)
            {
                ViewName = viewName;
                Args = args;
            }

            public string ViewName { get; }
            public object[] Args { get; }
        }

        private readonly struct DelayedView
        {
            public DelayedView(ViewLifetimeScope view, float destroyAt)
            {
                View = view;
                DestroyAt = destroyAt;
            }

            public ViewLifetimeScope View { get; }
            public float DestroyAt { get; }
        }
    }
}
