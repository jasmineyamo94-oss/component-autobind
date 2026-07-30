using System;
using UnityEngine;

namespace JasmineYamo.SimpleUI.VContainer
{
    public interface IViewManager
    {
        event Action<ViewLifetimeScope> OnViewOpened;
        event Action<ViewLifetimeScope> OnViewClosed;

        bool HasActivePopups { get; }

        ViewLifetimeScope GetPeekView();

        ViewLifetimeScope GetPeekView(ViewLayer layer);

        ViewLifetimeScope ShowView(
            string viewName,
            bool immediately = true,
            params object[] args);

        ViewLifetimeScope ShowViewAtHidePeekView(
            string viewName,
            params object[] args);

        void HideView();

        void HideView(ViewLayer layer);

        void HideView(ViewLifetimeScope view);

        void HideAllViews();

        void HideAllViews(ViewLayer layer);
    }

    public interface IViewPrefabHelper
    {
        GameObject GetViewPrefab(string viewName);
    }
}
