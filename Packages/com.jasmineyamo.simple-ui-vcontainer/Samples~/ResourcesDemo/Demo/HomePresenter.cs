using JasmineYamo.SimpleUI.VContainer;
using VContainer.Unity;

namespace JasmineYamo.SimpleUI.VContainer.Samples.ResourcesDemo.Demo
{
    public sealed class HomePresenter : IStartable
    {
        private readonly HomeView.UIView m_View;
        private readonly ViewBundle m_ViewBundle;
        private readonly IViewManager m_ViewManager;

        public HomePresenter(
            HomeView.UIView view,
            ViewBundle viewBundle,
            IViewManager viewManager)
        {
            m_View = view;
            m_ViewBundle = viewBundle;
            m_ViewManager = viewManager;
        }

        public void Start()
        {
            object payload = m_ViewBundle.DataBundle != null
                && m_ViewBundle.DataBundle.Length > 0
                    ? m_ViewBundle.DataBundle[0]
                    : "No payload";
            m_View.statusText.text = $"HomeView - {payload}";
            m_View.showDetailButton.onClick.AddListener(
                () => m_ViewManager.ShowView(
                    "DetailView",
                    true,
                    "Opened immediately"));
            m_View.queueDetailButton.onClick.AddListener(
                () => m_ViewManager.ShowView(
                    "DetailView",
                    false,
                    "Opened from queue"));
            m_View.replaceDetailButton.onClick.AddListener(
                () => m_ViewManager.ShowViewAtHidePeekView(
                    "DetailView",
                    "Replaced HomeView"));
            m_View.closeButton.onClick.AddListener(m_ViewManager.HideView);
        }
    }
}
