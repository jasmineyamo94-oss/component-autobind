using System.Collections;
using JasmineYamo.SimpleUI.VContainer.Samples.ResourcesDemo.Demo;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.UI;
using VContainer;

namespace JasmineYamo.SimpleUI.VContainer.Samples.ResourcesDemo.Tests
{
    public sealed class ViewManagerSamplePlayModeTests
    {
        [UnityTest]
        public IEnumerator DemoPrefabsOpenInjectAndResume()
        {
            var root = new GameObject("ViewManager Test Root");
            root.SetActive(false);
            ViewManagerSampleLifetimeScope scope =
                root.AddComponent<ViewManagerSampleLifetimeScope>();
            root.SetActive(true);
            yield return null;

            IViewManager manager =
                scope.Container.Resolve<IViewManager>();
            ViewLifetimeScope home =
                manager.ShowView(
                    "HomeView",
                    true,
                    "PlayMode payload");
            yield return null;

            Assert.That(home, Is.TypeOf<HomeView>());
            Assert.That(home.ViewName, Is.EqualTo("HomeView"));
            Assert.That(
                FindText(home, "StatusText").text,
                Does.Contain("PlayMode payload"));

            ViewLifetimeScope detail =
                manager.ShowView(
                    "DetailView",
                    true,
                    "Detail payload");
            yield return null;

            Assert.That(detail, Is.TypeOf<DetailView>());
            Assert.That(home.gameObject.activeSelf, Is.False);
            Assert.That(
                FindText(detail, "StatusText").text,
                Does.Contain("Detail payload"));

            manager.HideView();
            yield return null;

            Assert.That(detail.gameObject.activeSelf, Is.False);
            Assert.That(home.gameObject.activeSelf, Is.True);

            Object.Destroy(root);
            yield return null;
        }

        private static Text FindText(
            ViewLifetimeScope view,
            string objectName)
        {
            Text[] texts = view.GetComponentsInChildren<Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i].name == objectName)
                {
                    return texts[i];
                }
            }

            Assert.Fail(
                $"Text '{objectName}' was not found below '{view.name}'.");
            return null;
        }
    }
}
