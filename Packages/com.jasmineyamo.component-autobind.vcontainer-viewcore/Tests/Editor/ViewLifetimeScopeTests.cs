using System;
using NUnit.Framework;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace JasmineYamo.ComponentAutoBind.ViewCore.Tests
{
    public sealed class ViewLifetimeScopeTests
    {
        private GameObject m_Root;

        [TearDown]
        public void TearDown()
        {
            if (m_Root != null)
            {
                UnityEngine.Object.DestroyImmediate(m_Root);
            }
        }

        [Test]
        public void ConfigureEnsuresBindingsBeforeRegistration()
        {
            TestViewLifetimeScope scope = CreateScope();
            var builder = new ContainerBuilder();

            scope.ConfigureForTest(builder);

            Assert.That(scope.EnsureAutoBindCallCount, Is.EqualTo(1));
        }

        [Test]
        public void RegisterCommonInjectsConcreteUIViewAndViewBundle()
        {
            TestViewLifetimeScope scope = CreateScope();
            var builder = new ContainerBuilder();
            var uiView = new TestUIView();
            scope.Init("payload", 42);

            RegistrationBuilder registration =
                scope.RegisterForTest<TestPresenter>(builder, uiView);
            IObjectResolver resolver = builder.Build();

            try
            {
                TestPresenter presenter = resolver.Resolve<IStartable>() as TestPresenter;

                Assert.That(registration, Is.Not.Null);
                Assert.That(presenter, Is.Not.Null);
                Assert.That(presenter.UIView, Is.SameAs(uiView));
                Assert.That(presenter.ViewBundle.DataBundle, Is.EqualTo(new object[] { "payload", 42 }));
            }
            finally
            {
                (resolver as IDisposable)?.Dispose();
            }
        }

        private TestViewLifetimeScope CreateScope()
        {
            m_Root = new GameObject("TestViewLifetimeScope");
            return m_Root.AddComponent<TestViewLifetimeScope>();
        }
    }

    public sealed class TestUIView : IUiViewComponent
    {
    }

    public sealed class TestPresenter : IStartable
    {
        public TestPresenter(TestUIView uiView, ViewBundle viewBundle)
        {
            UIView = uiView;
            ViewBundle = viewBundle;
        }

        public TestUIView UIView { get; }
        public ViewBundle ViewBundle { get; }

        public void Start()
        {
        }
    }

    public sealed class TestViewLifetimeScope : ViewLifetimeScope
    {
        public int EnsureAutoBindCallCount { get; private set; }

        public override void EnsureAutoBind(GameObject go)
        {
            EnsureAutoBindCallCount++;
        }

        public void ConfigureForTest(IContainerBuilder builder)
        {
            base.Configure(builder);
        }

        public RegistrationBuilder RegisterForTest<T>(
            IContainerBuilder builder,
            IUiViewComponent uiView)
        {
            return RegisterCommon<T>(builder, uiView);
        }
    }
}
