using System;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace JasmineYamo.SimpleUI.VContainer
{
    public static class SimpleUIVContainerBuilderExtensions
    {
        public static RegistrationBuilder RegisterSimpleUI(
            this IContainerBuilder builder,
            LifetimeScope parentScope,
            Transform viewRoot)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (parentScope == null)
            {
                throw new ArgumentNullException(nameof(parentScope));
            }

            if (viewRoot == null)
            {
                throw new ArgumentNullException(nameof(viewRoot));
            }

            return builder
                .RegisterEntryPoint<ViewManager>(Lifetime.Singleton)
                .WithParameter(parentScope)
                .WithParameter(viewRoot);
        }
    }
}
