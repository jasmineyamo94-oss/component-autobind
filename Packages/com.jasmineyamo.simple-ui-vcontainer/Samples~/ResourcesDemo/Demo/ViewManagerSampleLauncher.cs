using JasmineYamo.SimpleUI.VContainer;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace JasmineYamo.SimpleUI.VContainer.Samples.ResourcesDemo.Demo
{
    public sealed class ViewManagerSampleLauncher : MonoBehaviour
    {
        [SerializeField]
        private ViewManagerSampleLifetimeScope m_LifetimeScope;

        [SerializeField]
        private Button m_ShowHomeButton;

        [SerializeField]
        private Button m_QueueDetailButton;

        [SerializeField]
        private Button m_HideButton;

        private IViewManager m_ViewManager;

        private void Start()
        {
            m_ViewManager =
                m_LifetimeScope.Container.Resolve<IViewManager>();
            m_ShowHomeButton.onClick.AddListener(
                () => m_ViewManager.ShowView(
                    "HomeView",
                    true,
                    "Opened from launcher"));
            m_QueueDetailButton.onClick.AddListener(
                () => m_ViewManager.ShowView(
                    "DetailView",
                    false,
                    "Queued from launcher"));
            m_HideButton.onClick.AddListener(m_ViewManager.HideView);
        }
    }
}
