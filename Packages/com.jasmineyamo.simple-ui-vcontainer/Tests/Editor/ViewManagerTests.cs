using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using VContainer;
using VContainer.Unity;

namespace JasmineYamo.SimpleUI.VContainer.Tests
{
    public sealed class ViewManagerTests
    {
        private readonly List<GameObject> m_GameObjects =
            new List<GameObject>();

        private LifetimeScope m_ParentScope;
        private Transform m_ViewRoot;
        private FakePrefabHelper m_PrefabHelper;

        [SetUp]
        public void SetUp()
        {
            GameObject scopeObject = Track(new GameObject("Parent Scope"));
            scopeObject.SetActive(false);
            m_ParentScope = scopeObject.AddComponent<LifetimeScope>();
            m_ParentScope.Build();

            m_ViewRoot = Track(new GameObject("View Root")).transform;
            m_PrefabHelper = new FakePrefabHelper();
        }

        [TearDown]
        public void TearDown()
        {
            for (int i = m_GameObjects.Count - 1; i >= 0; i--)
            {
                if (m_GameObjects[i] != null)
                {
                    UnityEngine.Object.DestroyImmediate(m_GameObjects[i]);
                }
            }

            m_GameObjects.Clear();
        }

        [Test]
        public void LayersUseIndependentStacksAndSortingOrders()
        {
            CreatePrefab("HudView", ViewLayer.HUD);
            CreatePrefab("NormalOneView", ViewLayer.Normal);
            CreatePrefab("NormalTwoView", ViewLayer.Normal);
            CreatePrefab("TopView", ViewLayer.Top);
            CreatePrefab("SystemView", ViewLayer.System);
            ViewManager manager = CreateManager();

            ViewLifetimeScope hud = manager.ShowView("HudView");
            ViewLifetimeScope normalOne = manager.ShowView("NormalOneView");
            ViewLifetimeScope normalTwo = manager.ShowView("NormalTwoView");
            ViewLifetimeScope top = manager.ShowView("TopView");
            ViewLifetimeScope system = manager.ShowView("SystemView");

            Assert.That(manager.GetPeekView(ViewLayer.HUD), Is.SameAs(hud));
            Assert.That(manager.GetPeekView(), Is.SameAs(normalTwo));
            Assert.That(manager.GetPeekView(ViewLayer.Top), Is.SameAs(top));
            Assert.That(manager.GetPeekView(ViewLayer.System), Is.SameAs(system));
            Assert.That(normalOne.gameObject.activeSelf, Is.False);
            Assert.That(normalTwo.gameObject.activeSelf, Is.True);
            Assert.That(top.gameObject.activeSelf, Is.True);
            Assert.That(system.gameObject.activeSelf, Is.True);
            Assert.That(
                normalOne.GetComponent<Canvas>().sortingOrder,
                Is.EqualTo((int)ViewLayer.Normal + 10));
            Assert.That(
                normalTwo.GetComponent<Canvas>().sortingOrder,
                Is.EqualTo((int)ViewLayer.Normal + 20));
            Assert.That(
                top.GetComponent<Canvas>().sortingOrder,
                Is.EqualTo((int)ViewLayer.Top + 10));

            manager.HideView(ViewLayer.Top);
            Assert.That(top.gameObject.activeSelf, Is.False);
            Assert.That(normalTwo.gameObject.activeSelf, Is.True);

            manager.HideView();
            Assert.That(normalTwo.gameObject.activeSelf, Is.False);
            Assert.That(normalOne.gameObject.activeSelf, Is.True);

            manager.HideAllViews();
            Assert.That(manager.GetPeekView(ViewLayer.HUD), Is.Null);
            Assert.That(manager.GetPeekView(ViewLayer.Normal), Is.Null);
            Assert.That(manager.GetPeekView(ViewLayer.Top), Is.Null);
            Assert.That(manager.GetPeekView(ViewLayer.System), Is.Null);
            Assert.That(manager.HasActivePopups, Is.False);
        }

        [Test]
        public void QueuesWaitForTheirOwnLayerToBecomeEmpty()
        {
            CreatePrefab("NormalOpenView", ViewLayer.Normal);
            CreatePrefab("NormalQueuedView", ViewLayer.Normal);
            CreatePrefab("TopQueuedView", ViewLayer.Top);
            ViewManager manager = CreateManager();

            manager.ShowView("NormalOpenView");
            manager.ShowView("NormalQueuedView", false, "normal payload");
            manager.ShowView("TopQueuedView", false, "top payload");

            manager.Tick();

            Assert.That(manager.GetPeekView().ViewName, Is.EqualTo("NormalOpenView"));
            Assert.That(
                manager.GetPeekView(ViewLayer.Top).ViewName,
                Is.EqualTo("TopQueuedView"));

            manager.HideView();
            manager.Tick();

            var queued = (TestView)manager.GetPeekView();
            Assert.That(queued.ViewName, Is.EqualTo("NormalQueuedView"));
            Assert.That(
                queued.InitializationArgs,
                Is.EqualTo(new object[] { "normal payload" }));
        }

        [Test]
        public void ReplaceTopKeepsOldViewWhenReplacementCannotLoad()
        {
            CreatePrefab("FirstView", ViewLayer.Normal);
            CreatePrefab("SecondView", ViewLayer.Normal);
            CreatePrefab("ReplacementView", ViewLayer.Normal);
            ViewManager manager = CreateManager();
            ViewLifetimeScope first = manager.ShowView("FirstView");
            ViewLifetimeScope second = manager.ShowView("SecondView");

            LogAssert.Expect(
                LogType.Error,
                "[Simple UI] View prefab 'MissingView' was not found.");
            ViewLifetimeScope missing =
                manager.ShowViewAtHidePeekView("MissingView");

            Assert.That(missing, Is.Null);
            Assert.That(manager.GetPeekView(), Is.SameAs(second));
            Assert.That(second.gameObject.activeSelf, Is.True);

            ViewLifetimeScope replacement =
                manager.ShowViewAtHidePeekView("ReplacementView");

            Assert.That(manager.GetPeekView(), Is.SameAs(replacement));
            Assert.That(second.gameObject.activeSelf, Is.False);
            manager.HideView();
            Assert.That(first.gameObject.activeSelf, Is.True);
        }

        [Test]
        public void DuplicateTopReturnsExistingInstance()
        {
            CreatePrefab("InventoryView", ViewLayer.Normal);
            ViewManager manager = CreateManager();

            ViewLifetimeScope opened =
                manager.ShowView("InventoryView", true, "first");
            ViewLifetimeScope duplicate =
                manager.ShowView("InventoryView", true, "second");

            Assert.That(duplicate, Is.SameAs(opened));
            Assert.That(
                ((TestView)duplicate).InitializationArgs,
                Is.EqualTo(new object[] { "first" }));
        }

        [Test]
        public void NonDestroyViewIsReusedAndReinitialized()
        {
            CreatePrefab(
                "InventoryView",
                ViewLayer.Normal,
                DestroyType.NonDestroy);
            ViewManager manager = CreateManager();

            ViewLifetimeScope first =
                manager.ShowView("InventoryView", true, "first");
            manager.HideView();
            ViewLifetimeScope reopened =
                manager.ShowView("InventoryView", true, "second");

            Assert.That(reopened, Is.SameAs(first));
            Assert.That(
                ((TestView)reopened).InitializationArgs,
                Is.EqualTo(new object[] { "second" }));
            Assert.That(((TestView)reopened).InitializationCount, Is.EqualTo(2));
            Assert.That(((TestView)reopened).AttachedManager, Is.SameAs(manager));
        }

        [Test]
        public void ZeroDelayViewIsDestroyedWhenClosed()
        {
            TestView prefab = CreatePrefab(
                "TransientView",
                ViewLayer.Normal,
                DestroyType.DelayDestroy);
            prefab.DestroyDelaySeconds = 0f;
            ViewManager manager = CreateManager();

            ViewLifetimeScope opened = manager.ShowView("TransientView");
            manager.HideView();

            Assert.That(opened == null, Is.True);
        }

        [Test]
        public void ReopeningDelayedViewCancelsPendingDestroy()
        {
            TestView prefab = CreatePrefab(
                "DelayedView",
                ViewLayer.Normal,
                DestroyType.DelayDestroy);
            prefab.DestroyDelaySeconds = 30f;
            ViewManager manager = CreateManager();

            ViewLifetimeScope first = manager.ShowView("DelayedView");
            manager.HideView();
            ViewLifetimeScope reopened = manager.ShowView("DelayedView");
            manager.Tick();

            Assert.That(reopened, Is.SameAs(first));
            Assert.That(reopened, Is.Not.Null);
            Assert.That(reopened.gameObject.activeSelf, Is.True);
        }

        [Test]
        public void DelayedViewIsDestroyedAfterDeadline()
        {
            TestView prefab = CreatePrefab(
                "DelayedView",
                ViewLayer.Normal,
                DestroyType.DelayDestroy);
            prefab.DestroyDelaySeconds = 0.01f;
            ViewManager manager = CreateManager();

            ViewLifetimeScope view = manager.ShowView("DelayedView");
            manager.HideView();
            Thread.Sleep(30);
            manager.Tick();

            Assert.That(view == null, Is.True);
        }

        [Test]
        public void ClosingMiddleViewCompactsSortingWithoutResumingBottom()
        {
            CreatePrefab("FirstView", ViewLayer.Normal);
            CreatePrefab("MiddleView", ViewLayer.Normal);
            CreatePrefab("TopView", ViewLayer.Normal);
            ViewManager manager = CreateManager();
            ViewLifetimeScope first = manager.ShowView("FirstView");
            ViewLifetimeScope middle = manager.ShowView("MiddleView");
            ViewLifetimeScope top = manager.ShowView("TopView");

            manager.HideView(middle);

            Assert.That(middle.gameObject.activeSelf, Is.False);
            Assert.That(first.gameObject.activeSelf, Is.False);
            Assert.That(top.gameObject.activeSelf, Is.True);
            Assert.That(
                top.GetComponent<Canvas>().sortingOrder,
                Is.EqualTo((int)ViewLayer.Normal + 20));

            manager.HideView(top);
            Assert.That(first.gameObject.activeSelf, Is.True);
        }

        [Test]
        public void OpenAndCloseEventsAreExposedThroughInterface()
        {
            CreatePrefab("EventView", ViewLayer.Normal);
            IViewManager manager = CreateManager();
            ViewLifetimeScope opened = null;
            ViewLifetimeScope closed = null;
            manager.OnViewOpened += value => opened = value;
            manager.OnViewClosed += value => closed = value;

            ViewLifetimeScope view = manager.ShowView("EventView");
            manager.HideView();

            Assert.That(opened, Is.SameAs(view));
            Assert.That(closed, Is.SameAs(view));
        }

        [Test]
        public void RegisterSimpleUIExposesSameManagerAndTickable()
        {
            var builder = new ContainerBuilder();
            builder.RegisterInstance(m_PrefabHelper).As<IViewPrefabHelper>();
            RegistrationBuilder registration =
                builder.RegisterSimpleUI(m_ParentScope, m_ViewRoot);
            IObjectResolver resolver = builder.Build();

            try
            {
                IViewManager manager = resolver.Resolve<IViewManager>();
                ITickable tickable = resolver.Resolve<ITickable>();

                Assert.That(registration, Is.Not.Null);
                Assert.That(manager, Is.TypeOf<ViewManager>());
                Assert.That(tickable, Is.SameAs(manager));
            }
            finally
            {
                (resolver as IDisposable)?.Dispose();
            }
        }

        [Test]
        public void MissingViewComponentLogsErrorAndReturnsNull()
        {
            GameObject invalid = Track(new GameObject("Invalid Prefab"));
            invalid.SetActive(false);
            m_PrefabHelper.Add("InvalidView", invalid);
            ViewManager manager = CreateManager();

            LogAssert.Expect(
                LogType.Error,
                "[Simple UI] Prefab 'InvalidView' does not contain ViewLifetimeScope.");
            ViewLifetimeScope result = manager.ShowView("InvalidView");

            Assert.That(result, Is.Null);
        }

        [Test]
        public void DisposeDestroysCachedViews()
        {
            CreatePrefab("CachedView", ViewLayer.Normal);
            ViewManager manager = CreateManager();
            ViewLifetimeScope view = manager.ShowView("CachedView");
            manager.HideView();

            manager.Dispose();
            manager.Dispose();

            Assert.That(view == null, Is.True);
        }

        private ViewManager CreateManager()
        {
            return new ViewManager(m_ParentScope, m_PrefabHelper, m_ViewRoot);
        }

        private TestView CreatePrefab(
            string viewName,
            ViewLayer layer,
            DestroyType destroyType = DestroyType.NonDestroy)
        {
            var prefab = new GameObject(
                viewName + " Prefab",
                typeof(RectTransform),
                typeof(Canvas),
                typeof(GraphicRaycaster));
            Track(prefab);
            prefab.SetActive(false);
            TestView view = prefab.AddComponent<TestView>();
            view.viewLayer = layer;
            view.destroyType = destroyType;
            m_PrefabHelper.Add(viewName, prefab);
            return view;
        }

        private GameObject Track(GameObject gameObject)
        {
            m_GameObjects.Add(gameObject);
            return gameObject;
        }

        private sealed class FakePrefabHelper : IViewPrefabHelper
        {
            private readonly Dictionary<string, GameObject> m_Prefabs =
                new Dictionary<string, GameObject>(StringComparer.Ordinal);

            public void Add(string viewName, GameObject prefab)
            {
                m_Prefabs.Add(viewName, prefab);
            }

            public GameObject GetViewPrefab(string viewName)
            {
                m_Prefabs.TryGetValue(viewName, out GameObject prefab);
                return prefab;
            }
        }
    }

    public sealed class TestView : ViewLifetimeScope
    {
        public object[] InitializationArgs { get; private set; }
        public int InitializationCount { get; private set; }
        public IViewManager AttachedManager => ViewManager;

        public override void Init(params object[] args)
        {
            base.Init(args);
            InitializationArgs = args;
            InitializationCount++;
        }
    }
}
