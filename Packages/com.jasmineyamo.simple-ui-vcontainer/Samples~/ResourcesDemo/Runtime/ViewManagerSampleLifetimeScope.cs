using JasmineYamo.SimpleUI.VContainer;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace JasmineYamo.SimpleUI.VContainer.Samples.ResourcesDemo
{
    public sealed class ViewManagerSampleLifetimeScope : LifetimeScope
    {
        [SerializeField]
        private Transform m_ViewRoot;

        protected override void Configure(IContainerBuilder builder)
        {
            if (m_ViewRoot == null)
            {
                m_ViewRoot = transform;
            }

            builder.Register<IViewPrefabHelper, ResourcesViewPrefabHelper>(
                Lifetime.Singleton);
            builder.RegisterSimpleUI(this, m_ViewRoot);
        }
    }
}
