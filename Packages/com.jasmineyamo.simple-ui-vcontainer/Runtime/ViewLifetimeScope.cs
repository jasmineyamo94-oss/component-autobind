using System;
using JasmineYamo.ComponentAutoBind;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace JasmineYamo.SimpleUI.VContainer
{
    public enum ViewLayer
    {
        HUD = 1000,
        Normal = 1100,
        Top = 2000,
        System = 3000
    }

    public enum DestroyType
    {
        NonDestroy,
        DelayDestroy
    }

    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(GraphicRaycaster))]
    [RequireComponent(typeof(ComponentAutoBindTool))]
    public class ViewLifetimeScope : LifetimeScope, IAutoBindHost
    {
        [Serializable]
        public class UIView : IUiViewComponent
        {
        }

        protected UIView view;

        [SerializeField]
        public ViewLayer viewLayer = ViewLayer.Normal;

        [SerializeField]
        public DestroyType destroyType = DestroyType.NonDestroy;

        [SerializeField]
        [Min(0f)]
        private float m_DestroyDelaySeconds = 30f;

        private ViewBundle ViewBundle { get; } = new ViewBundle();

        public string ViewName { get; private set; }

        public float DestroyDelaySeconds
        {
            get => m_DestroyDelaySeconds;
            set => m_DestroyDelaySeconds = Mathf.Max(0f, value);
        }

        protected IViewManager ViewManager { get; private set; }

        protected override void Configure(IContainerBuilder builder)
        {
            EnsureAutoBind(gameObject);
        }

        public virtual void EnsureAutoBind(GameObject go)
        {
        }

        protected RegistrationBuilder RegisterCommon<T>(
            IContainerBuilder builder,
            IUiViewComponent iView = null)
        {
            if (iView != null)
            {
                builder.RegisterInstance(iView).AsSelf();
            }

            builder.RegisterEntryPointExceptionHandler(Debug.LogError);
            return builder.RegisterEntryPoint<T>().WithParameter(ViewBundle);
        }

        public virtual void Init(params object[] args)
        {
            ViewBundle.SetViewBundle(args);
        }

        public void SetCanvasOrder(int order)
        {
            Canvas canvas = GetComponent<Canvas>();
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera
                && canvas.worldCamera == null)
            {
                canvas.worldCamera = Camera.main;
            }

            canvas.overrideSorting = true;
            canvas.sortingOrder = order;
        }

        public void SetViewName(string viewName)
        {
            ViewName = viewName;
        }

        public virtual void Show()
        {
            gameObject.SetActive(true);
        }

        public virtual void Hide()
        {
            gameObject.SetActive(false);
        }

        public virtual void Pause()
        {
            gameObject.SetActive(false);
        }

        public virtual void Resume()
        {
            gameObject.SetActive(true);
        }

        internal void AttachViewManager(IViewManager viewManager)
        {
            ViewManager = viewManager;
        }
    }
}
