using System;
using UnityEngine;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace JasmineYamo.ComponentAutoBind.ViewCore
{
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

        private ViewBundle ViewBundle { get; } = new ViewBundle();

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
    }
}
